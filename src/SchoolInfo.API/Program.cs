using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using SchoolInfo.API.Extensions;
using SchoolInfo.API.Middleware;
using SchoolInfo.Infrastructure;
using System.Reflection;
using FluentValidation;
using MediatR;
using SchoolInfo.Application.Common.Behaviors;
using System;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// 1. Domain (Herhangi bir servis kaydı yok)

// 2. Application
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SchoolInfo.Application.Common.Interfaces.IAppDbContext).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(SchoolInfo.Application.Common.Interfaces.IAppDbContext).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

// Real Current User Service
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<SchoolInfo.Application.Common.Interfaces.ICurrentUserService, SchoolInfo.API.Services.CurrentUserService>();

// 3. Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

// 4. Endpoints Registration (Reflection ile tüm IEndpoint'ler)
builder.Services.AddEndpoints();

// 5. JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]
                    ?? throw new InvalidOperationException("Jwt:Key yapılandırması eksik!")))
        };
    });
builder.Services.AddAuthorization();

// 6. Rate Limiting — Login'e brute-force koruması
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(5);
        opt.PermitLimit = 10;          // 5 dakikada max 10 deneme
        opt.QueueLimit = 0;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    // Genel API rate limiting
    options.AddFixedWindowLimiter("api", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 120;         // Dakikada 120 istek
        opt.QueueLimit = 0;
    });

    options.RejectionStatusCode = 429;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync(
            "{\"error\":\"Çok fazla istek gönderildi. Lütfen birkaç dakika bekleyip tekrar deneyiniz.\"}", token);
    };
});

// 7. Swagger Config (JWT desteği) — sadece Development'ta
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SchoolInfo API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Sadece token değerini girin.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Middleware: Exception Handling
app.UseMiddleware<ExceptionHandlingMiddleware>();

// HTTPS Yönlendirme (Production'da zorunlu)
app.UseHttpsRedirection();

// Swagger yalnızca Development ortamında açık olsun
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.MapGet("/", context =>
    {
        context.Response.Redirect("/swagger");
        return Task.CompletedTask;
    });
}

// Rate Limiter middleware
app.UseRateLimiter();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SchoolInfo.Infrastructure.Persistence.AppDbContext>();
    // Veri tabanını ve tablolarını otomatik oluştur/güncelle
    await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.MigrateAsync(dbContext.Database);
    await SchoolInfo.Infrastructure.Persistence.DatabaseInitializer.SeedAsync(dbContext);
}

app.UseAuthentication();
app.UseAuthorization();

// Endpoint Mapping
app.MapEndpoints();

app.Run();
