using MediatR;

namespace SchoolInfo.Application.Features.Users.Commands.ChangePassword;

public record ChangePasswordCommand(string OldPassword, string NewPassword) : IRequest;
