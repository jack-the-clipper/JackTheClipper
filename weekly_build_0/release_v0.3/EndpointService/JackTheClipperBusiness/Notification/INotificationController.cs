namespace JackTheClipperBusiness.Notification
{
    /// <summary>
    /// Interface for any notification controller.
    /// </summary>
    public interface INotificationController
    {
        /// <summary>
        /// Starts the notification processing pipeline.
        /// </summary>
        void StartNotificationProcessing();
    }
}