using System;
using MediatR;

namespace SchoolInfo.Application.Features.Newsletters.Commands.PublishNewsletter;

public record PublishNewsletterCommand(Guid Id) : IRequest;
