using JiuJitsuMaster.Core.Interfaces.Repositories;
using JiuJitsuMaster.Infrastructure.Options;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace JiuJitsuMaster.Infrastructure.Repositories;

public class MailKitRepository : IMailKitRepository
{
    private readonly MailKitOptions _options;
    private readonly ILogger<MailKitRepository> _logger;

    public MailKitRepository(
        IOptions<MailKitOptions> options,
        ILogger<MailKitRepository> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(MimeMessage message)
    {
        try
        {
            message.From.Add(new MailboxAddress(_options.SenderName, _options.SenderEmail));

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_options.HostAddress, _options.HostPort, _options.UseSsl);
                await client.AuthenticateAsync(_options.HostUsername, _options.HostPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }

            _logger.LogInformation("E-mail enviado com sucesso.");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao enviar e-mail: {ex.Message}");

            return false;
        }
    }
}