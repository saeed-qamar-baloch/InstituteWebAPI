using InstituteWebAPI.Models.Configuration;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace InstituteWebAPI.Services.Sms
{
    /// <summary>
    /// Generic HTTP/GET SMS gateway client. Targets the simple query-string
    /// convention shared by most local Pakistani bulk-SMS providers (SendPK,
    /// and similar — username/password/sender/mobile/message as GET params,
    /// "OK" or a numeric error code in the response body).
    ///
    /// Disabled by default via SmsGatewaySettings.Enabled — until a provider
    /// account + PTA-approved sender ID (mask) are configured, sends are
    /// logged and skipped rather than thrown, so the rest of the fee-reminder
    /// flow (notification rows, UI) can be exercised safely.
    /// </summary>
    public class HttpSmsService : ISmsService
    {
        private readonly HttpClient httpClient;
        private readonly SmsGatewaySettings settings;
        private readonly ILogger<HttpSmsService> logger;

        public HttpSmsService(HttpClient httpClient, IOptions<SmsGatewaySettings> options, ILogger<HttpSmsService> logger)
        {
            this.httpClient = httpClient;
            this.settings = options.Value;
            this.logger = logger;
        }

        public async Task SendAsync(string phoneNumber, string message)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new InvalidOperationException("No phone number on file for this recipient.");

            if (!settings.Enabled)
            {
                logger.LogInformation("SMS gateway disabled — skipping send to {Phone}: {Message}", phoneNumber, message);
                return;
            }

            if (string.IsNullOrWhiteSpace(settings.BaseUrl) || string.IsNullOrWhiteSpace(settings.Username)
                || string.IsNullOrWhiteSpace(settings.Password) || string.IsNullOrWhiteSpace(settings.SenderId))
            {
                throw new InvalidOperationException("SMS gateway is enabled but BaseUrl/Username/Password/SenderId are not fully configured.");
            }

            var normalizedPhone = NormalizePakistaniNumber(phoneNumber);

            var url = QueryHelpers.AddQueryString(settings.BaseUrl, new Dictionary<string, string?>
            {
                ["username"] = settings.Username,
                ["password"] = settings.Password,
                ["sender"] = settings.SenderId,
                ["mobile"] = normalizedPhone,
                ["message"] = message
            });

            HttpResponseMessage response;
            try
            {
                response = await httpClient.GetAsync(url);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Could not reach SMS gateway: {ex.Message}", ex);
            }

            var body = (await response.Content.ReadAsStringAsync()).Trim();

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"SMS gateway returned HTTP {(int)response.StatusCode}: {body}");

            // Most local gateways (incl. SendPK) reply "OK ID:<n>" on success and a
            // bare numeric error code (1-9) on failure. Treat anything that doesn't
            // start with "OK" as a failure so it surfaces in Notification.ErrorMessage.
            if (!body.StartsWith("OK", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"SMS gateway rejected message: {body}");
        }

        private static string NormalizePakistaniNumber(string raw)
        {
            var digits = new string(raw.Where(char.IsDigit).ToArray());

            if (digits.StartsWith("0"))
                digits = "92" + digits[1..];
            else if (!digits.StartsWith("92"))
                digits = "92" + digits;

            return digits;
        }
    }
}
