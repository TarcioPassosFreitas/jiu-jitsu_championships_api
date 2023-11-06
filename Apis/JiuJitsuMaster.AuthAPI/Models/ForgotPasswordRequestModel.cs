using System.ComponentModel.DataAnnotations;

namespace JiuJitsuMaster.AuthAPI.Models;

public class ForgotPasswordRequestModel
{
    [Required] public string Email { get; set; } = string.Empty;
}