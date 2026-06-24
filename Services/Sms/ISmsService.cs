namespace InstituteWebAPI.Services.Sms
{
    public interface ISmsService
    {
        /// <summary>
        /// Sends a single SMS. Throws on gateway-reported failure so callers
        /// (e.g. NotificationsController) can mark the notification Failed
        /// with the real error message.
        /// </summary>
        Task SendAsync(string phoneNumber, string message);
    }
}
