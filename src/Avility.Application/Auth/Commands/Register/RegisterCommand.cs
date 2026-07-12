using MediatR;

namespace Avility.Application.Auth.Commands.Register;

public sealed record RegisterCommand(string Email, string Password, string Role) : IRequest<AuthResponse>;
