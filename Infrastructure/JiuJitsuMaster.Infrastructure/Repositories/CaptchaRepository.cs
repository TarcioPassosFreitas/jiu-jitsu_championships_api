using JiuJitsuMaster.Core.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace JiuJitsuMaster.Infrastructure.Repositories;

public class CaptchaRepository : ICaptchaRepository
{
    private readonly IConfiguration _configuration;

    public CaptchaRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<bool> VerifyCaptcha(string captchaToken)
    {
        var secret = _configuration["RECAPTCHA_SECRET_KEY"] ?? Environment.GetEnvironmentVariable("RECAPTCHA_SECRET_KEY");

        var postData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("secret", secret),
            new KeyValuePair<string, string>("response", captchaToken)
        };

        using (var httpClient = new HttpClient())
        {
            var response = await httpClient.PostAsync(
                "https://www.google.com/recaptcha/api/siteverify",
                new FormUrlEncodedContent(postData));

            if (response.IsSuccessStatusCode)
            {
                var jsonResult = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(jsonResult);

                return result.success == true && (result.score == null || result.score >= 0.5);
            }
        }

        return false;
    }
}
