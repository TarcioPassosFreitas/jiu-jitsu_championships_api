using JiuJitsuMaster.Core.Enums;

namespace JiuJitsuMaster.AdminAPI.Models.UserModel;

public class RegisterRequestModel
{
    public string Name { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Email { get; set; } = null!;
    public UserRole? Role { get; set; }
}