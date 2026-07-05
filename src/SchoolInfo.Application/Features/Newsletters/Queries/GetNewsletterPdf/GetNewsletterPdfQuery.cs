using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SchoolInfo.Application.Features.Newsletters.Queries.GetNewsletterPdf;

public record GetNewsletterPdfQuery(Guid Id) : IRequest<byte[]>;

public class GetNewsletterPdfQueryHandler : IRequestHandler<GetNewsletterPdfQuery, byte[]>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    static GetNewsletterPdfQueryHandler()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public GetNewsletterPdfQueryHandler(IAppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<byte[]> Handle(GetNewsletterPdfQuery request, CancellationToken cancellationToken)
    {
        var newsletter = await _dbContext.Newsletters
            .Include(n => n.Sections)
            .FirstOrDefaultAsync(n => n.Id == request.Id && n.SchoolId == _currentUserService.SchoolId, cancellationToken);

        if (newsletter == null)
            throw new KeyNotFoundException("Bülten bulunamadı.");

        // Role-based security checks (BOLA prevention)
        if (_currentUserService.Role == "Parent")
        {
            if (newsletter.Status != SchoolInfo.Domain.Enums.NewsletterStatus.Published)
                throw new UnauthorizedAccessException("Yayınlanmamış bir bültene erişemezsiniz.");

            var hasChildInClass = await _dbContext.Students
                .AnyAsync(s => s.ClassroomId == newsletter.ClassroomId && !s.IsDeleted && s.Parents.Any(p => p.Id == _currentUserService.UserId), cancellationToken);

            if (!hasChildInClass)
                throw new UnauthorizedAccessException("Çocuğunuzun bulunmadığı bir sınıfın bültenine erişemezsiniz.");
        }
        else if (_currentUserService.Role == "Teacher")
        {
            var isAssigned = await _dbContext.Classrooms
                .AnyAsync(c => c.Id == newsletter.ClassroomId && !c.IsDeleted && c.Teachers.Any(t => t.Id == _currentUserService.UserId), cancellationToken);

            if (!isAssigned)
                throw new UnauthorizedAccessException("Atanmadığınız bir sınıfın bültenine erişemezsiniz.");
        }
        else if (_currentUserService.Role != "Admin")
        {
            throw new UnauthorizedAccessException("Bültene erişim yetkiniz bulunmamaktadır.");
        }

        var school = await _dbContext.Schools.FirstOrDefaultAsync(s => s.Id == newsletter.SchoolId, cancellationToken);
        var schoolName = school?.Name ?? "Veliport Anaokulu";

        // Generate PDF using QuestPDF
        var document = new NewsletterDocument(newsletter, schoolName);
        return document.GeneratePdf();
    }
}

public class NewsletterDocument : IDocument
{
    private readonly Newsletter _newsletter;
    private readonly string _schoolName;

    public NewsletterDocument(Newsletter newsletter, string schoolName)
    {
        _newsletter = newsletter;
        _schoolName = schoolName;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    private string GetPrimaryColor()
    {
        return _newsletter.Theme switch
        {
            "Playful" => "#8B5CF6", // Violet
            "Nature" => "#047857",  // Emerald
            "Sunset" => "#D97706",  // Amber
            _ => "#0F172A"          // Slate (Default)
        };
    }

    private string GetAccentColor()
    {
        return _newsletter.Theme switch
        {
            "Playful" => "#EC4899", // Pink
            "Nature" => "#10B981",  // Mint
            "Sunset" => "#F97316",  // Orange
            _ => "#E94560"          // Coral/Red (Default)
        };
    }

    private string GetCardBackgroundColor()
    {
        return _newsletter.Theme switch
        {
            "Playful" => "#FDF4FF", // Light purple
            "Nature" => "#F0FDF4",  // Light green
            "Sunset" => "#FFFBEB",  // Light amber
            _ => "#F8FAFC"          // Light slate (Default)
        };
    }

    private string GetBorderColor()
    {
        return _newsletter.Theme switch
        {
            "Playful" => "#F5D0FE", // Purple border
            "Nature" => "#BBF7D0",  // Green border
            "Sunset" => "#FEF3C7",  // Amber border
            _ => "#E2E8F0"          // Slate border (Default)
        };
    }

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(1.5f, QuestPDF.Infrastructure.Unit.Centimetre);
            page.Size(PageSizes.A4);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().Element(ComposeFooter);
        });
    }

    private void ComposeHeader(IDocumentContainer container)
    {
        // Not used, using page.Header().Element(ComposeHeader) which passes IContainer
    }

    private void ComposeHeader(IContainer container)
    {
        var primaryColor = GetPrimaryColor();
        var accentColor = GetAccentColor();

        container.BorderBottom(2f).BorderColor(accentColor).PaddingBottom(10).Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text(_schoolName.ToUpper())
                    .FontSize(12)
                    .Bold()
                    .FontColor("#64748B");

                column.Item().Text(_newsletter.Title)
                    .FontSize(22)
                    .Bold()
                    .FontColor(primaryColor);

                if (!string.IsNullOrEmpty(_newsletter.WeekName))
                {
                    column.Item().Text(_newsletter.WeekName)
                        .FontSize(12)
                        .SemiBold()
                        .FontColor(accentColor);
                }
            });

            row.ConstantItem(60).AlignRight().AlignMiddle().Column(col =>
            {
                col.Item().Text("🏫").FontSize(32);
            });
        });
    }

    private void ComposeContent(IContainer container)
    {
        var primaryColor = GetPrimaryColor();
        var accentColor = GetAccentColor();
        var cardBg = GetCardBackgroundColor();
        var borderCol = GetBorderColor();

        container.PaddingTop(15).Column(column =>
        {
            // Main Body Content
            column.Item().Background(cardBg).Border(1).BorderColor(borderCol).Padding(14).Column(card =>
            {
                card.Item().Text("Öğretmenimizin Notu")
                    .FontSize(13)
                    .Bold()
                    .FontColor(primaryColor);

                card.Item().PaddingTop(6).Text(_newsletter.Content)
                    .FontSize(11)
                    .LineHeight(1.5f)
                    .FontColor("#334155");
            });

            // Sections list (Branş dersleri)
            if (_newsletter.Sections != null && _newsletter.Sections.Any())
            {
                column.Item().PaddingTop(20).Text("HAFTALIK BRANŞ & ETKİNLİK DETAYLARI")
                    .FontSize(12)
                    .Bold()
                    .FontColor("#64748B");

                foreach (var section in _newsletter.Sections)
                {
                    column.Item().PaddingTop(10).Background("#FFFFFF").Border(1).BorderColor(borderCol).Padding(12).Column(sectionCard =>
                    {
                        // Header row of the section
                        sectionCard.Item().Row(row =>
                        {
                            row.RelativeItem().Text(section.Subject)
                                .FontSize(13)
                                .Bold()
                                .FontColor(accentColor);

                            if (!string.IsNullOrEmpty(section.InstructorName))
                            {
                                row.ConstantItem(150).AlignRight().Text($"Eğitmen: {section.InstructorName}")
                                    .FontSize(9)
                                    .Italic()
                                    .FontColor("#64748B");
                            }
                        });

                        sectionCard.Item().PaddingTop(6);

                        if (!string.IsNullOrEmpty(section.ThisWeekSummary))
                        {
                            sectionCard.Item().Text(x =>
                            {
                                x.Span("Bu Hafta Neler Yaptık: ").Bold().FontSize(10).FontColor(primaryColor);
                                x.Span(section.ThisWeekSummary).FontSize(10).FontColor("#334155");
                            });
                        }

                        if (!string.IsNullOrEmpty(section.NextWeekTopic))
                        {
                            sectionCard.Item().PaddingTop(4).Text(x =>
                            {
                                x.Span("Gelecek Hafta Planımız: ").Bold().FontSize(10).FontColor(primaryColor);
                                x.Span(section.NextWeekTopic).FontSize(10).FontColor("#334155");
                            });
                        }
                    });
                }
            }
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.BorderTop(1).BorderColor("#E2E8F0").PaddingTop(8).Row(row =>
        {
            row.RelativeItem().Text("Veliport Okul Öncesi Bilgi Platformu")
                .FontSize(8)
                .FontColor("#94A3B8");

            row.RelativeItem().AlignRight().Text(x =>
            {
                x.Span("Sayfa ").FontSize(8).FontColor("#94A3B8");
                x.CurrentPageNumber().FontSize(8).FontColor("#94A3B8");
                x.Span(" / ").FontSize(8).FontColor("#94A3B8");
                x.TotalPages().FontSize(8).FontColor("#94A3B8");
            });
        });
    }
}
