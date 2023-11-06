using CommonUtility.Interfaces;
using JiuJitsuMaster.Core.Aggregates;

namespace JiuJitsuMaster.Core.Interfaces.Services;

public interface IMailKitService : IBaseService<User>
{
    Task<bool> SendMessageAsync(string email, string name, string subject, string messageBody);
}