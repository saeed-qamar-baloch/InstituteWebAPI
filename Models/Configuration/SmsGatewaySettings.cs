namespace InstituteWebAPI.Models.Configuration
{
    /// <summary>
    /// Config for a generic HTTP/GET-style local SMS gateway (the pattern used by
    /// most Pakistani bulk-SMS providers, e.g. SendPK: BaseUrl + username/password/
    /// sender/mobile/message query params). Swapping providers later only requires
    /// changing these settings, not code, as long as the new provider follows the
    /// same simple query-param convention.
    /// </summary>
    public class SmsGatewaySettings
    {
        /// <summary>Master switch. Keep false until a provider account + approved
        /// sender ID (mask) are in place — sends are skipped (logged, not thrown)
        /// while disabled so the rest of the feature can be tested safely.</summary>
        public bool Enabled { get; set; } = false;

        /// <summary>e.g. https://sendpk.com/api/sms.php</summary>
        public string BaseUrl { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        /// <summary>Your PTA-approved alphanumeric sender ID (mask), e.g. "ROZHN".
        /// Must be registered with the gateway/PTA before SMS will actually deliver.</summary>
        public string SenderId { get; set; } = string.Empty;
    }
}
