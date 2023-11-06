using JiuJitsuMaster.Core.Interfaces.Repositories;
using JiuJitsuMaster.Core.Interfaces.Services;
using MimeKit;
using System.Threading.Tasks;

namespace JiuJitsuMaster.Core.Services;

public class MailKitService : IMailKitService
{
    private readonly IMailKitRepository _mailKitRepository;

    public MailKitService(IMailKitRepository mailKitRepository)
    {
        _mailKitRepository = mailKitRepository;
    }

    public async Task<bool> SendMessageAsync(string email, string name, string subject, string messageBody)
    {
        var message = new MimeMessage();
        message.To.Add(new MailboxAddress(name, email));
        message.Subject = subject;

        message.Body = new TextPart("plain")
        {
            Text = messageBody
        };

        return await _mailKitRepository.SendEmailAsync(message);
    }
}
