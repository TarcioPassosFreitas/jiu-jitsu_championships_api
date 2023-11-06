namespace JiuJitsuMaster.AuthAPI.Models;

public class LoginResponseModel
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}