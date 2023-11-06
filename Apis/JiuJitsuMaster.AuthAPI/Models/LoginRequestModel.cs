using System.ComponentModel.DataAnnotations;

namespace JiuJitsuMaster.AuthAPI.Models;

public class LoginRequestModel
{
    [Required] public string Email { get; set; } = null!;

    [Required] public string Password { get; set; } = null!;
}