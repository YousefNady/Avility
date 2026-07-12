using MediatR;

namespace Avility.Application.Auth.Commands.Logout;

public sealed record LogoutCommand(string RefreshToken) : IRequest;
