using System;

namespace Remp.Service.DTOs.Auth;

public class UpdatePasswordRequest
{
  public string OldPassword { get; set; } = string.Empty;
  public string NewPassword { get; set; } = string.Empty;
}
