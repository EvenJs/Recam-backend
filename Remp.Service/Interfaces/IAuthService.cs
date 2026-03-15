using System.Security.Claims;
using Remp.Service.DTOs.Auth;

namespace Remp.Service.Interfaces;

public interface IAuthService
{
  Task<LoginResponse> LoginAsync(LoginRequest request);
  Task<object> GetCurrentUserAsync(ClaimsPrincipal user);
  Task UpdatePasswordAsync(ClaimsPrincipal user, UpdatePasswordRequest request);
}
