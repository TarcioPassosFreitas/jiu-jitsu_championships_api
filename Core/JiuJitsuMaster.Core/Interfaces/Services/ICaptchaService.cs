using Ardalis.Result;

namespace JiuJitsuMaster.Core.Interfaces.Services;

public interface ICaptchaService
{
    Task<bool> VerifyCaptcha(string captchaToken);
}