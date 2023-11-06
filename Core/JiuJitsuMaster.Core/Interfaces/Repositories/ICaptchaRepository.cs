namespace JiuJitsuMaster.Core.Interfaces.Repositories;

public interface ICaptchaRepository
{
    Task<bool> VerifyCaptcha(string captchaToken);
}
