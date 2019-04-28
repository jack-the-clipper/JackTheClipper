namespace JackTheClipperBusiness.Notification
{
    /// <summary>
    /// Class which implements <see cref="INotificationController"/> and wraps around the real <see cref="NotificationControllerBAD"/>.
    /// Mainly created to avoid merge conflicts as <see cref="NotificationControllerBAD"/> is actively refactored at the moment.
    /// </summary>
    public class NotificationControllerWrapper : INotificationController
    {
        /// <summary>
        /// Starts the notification processing pipeline of the <see cref="NotificationControllerBAD"/>.
        /// </summary>
        public void StartNotificationProcessing()
        {
            NotificationControllerBAD.Start();
        }
    }
}