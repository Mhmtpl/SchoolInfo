using MediatR;

namespace SchoolInfo.Application.Features.Users.Commands.UpdateCurrentUser;

public record UpdateCurrentUserCommand(string FirstName, string LastName, string Email, string FcmToken) : IRequest;
