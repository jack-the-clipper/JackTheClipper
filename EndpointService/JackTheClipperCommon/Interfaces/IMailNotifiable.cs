namespace JackTheClipperCommon.Interfaces
{
    /// <summary>
    /// Interface for an object that can be notified by email
    /// </summary>
    public interface IMailNotifiable
    {
        /// <summary>
        /// The email address of the notifiable object
        /// </summary>
        string UserMailAddress { get; }

        /// <summary>
        /// The name of the notifiable object
        /// </summary>
        string UserName { get; }
    }
}