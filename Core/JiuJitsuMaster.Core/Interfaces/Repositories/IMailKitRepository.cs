using MimeKit;

namespace JiuJitsuMaster.Core.Interfaces.Repositories;

public interface IMailKitRepository
{
    Task<bool> SendEmailAsync(MimeMessage message);
}