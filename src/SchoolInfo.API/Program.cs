using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using SchoolInfo.API.Extensions;
using SchoolInfo.API.Middleware;
using SchoolInfo.Infrastructure;
using System.Reflection;
using FluentValidation;
using MediatR;
using SchoolInfo.Application.Common.Behaviors;
using System;

var builder = WebApplication.CreateBuilder(args);

// 1. Domain (Herhangi bir servis kaydı yok)

// 2. Application
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SchoolInfo.Application.Common.Interfaces.IAppDbContext).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(SchoolInfo.Application.Common.Interfaces.IAppDbContext).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

// Real Current User Service (Gerçek uygulamada HttpContextAccessor üzerinden User alınır)
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
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "SecretKey"))
        };
    });
builder.Services.AddAuthorization();

// 6. Swagger Config (JWT desteği)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SchoolInfo API", Version = "v1" });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Put ONLY your JWT token in the textbox below.",
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

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SchoolInfo.Infrastructure.Persistence.AppDbContext>();
    await SchoolInfo.Infrastructure.Persistence.DatabaseInitializer.SeedAsync(dbContext);
}

app.UseAuthentication();
app.UseAuthorization();

// Endpoint Mapping
app.MapEndpoints();

app.Run();
// Program.cs ends here.
