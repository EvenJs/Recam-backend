using Remp.Service.DTOs.Auth;

namespace Remp.Service.Interfaces;

public interface IAuthService
{
  Task<LoginResponse> LoginAsync(LoginRequest request);
  Task<string> GetCurrentUserAsync(string userId);
  Task UpdatePasswordAsync(string userId, string oldPassword, string newPassword);
}
