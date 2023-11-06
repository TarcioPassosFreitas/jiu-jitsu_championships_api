namespace JiuJitsuMaster.Infrastructure.Options;

public class MailKitOptions
{
    public string HostAddress { get; set; } = null!;
    public int HostPort { get; set; }
    public bool UseSsl { get; set; }
    public string HostUsername { get; set; } = null!;
    public string HostPassword { get; set; } = null!;
    public string SenderName { get; set; } = null!;
    public string SenderEmail { get; set; } = null!;
}