namespace JackTheClipperBusiness.CrawlerManagement
{
    /// <summary>
    /// Interface for any crawler controller.
    /// </summary>
    public interface ICrawlerController
    {
        /// <summary>
        /// Restarts the controller instance.
        /// </summary>
        void Restart();

        /// <summary>
        /// Clears all indexes (use with care, this is a permanent operation).
        /// </summary>
        void ClearAllIndexes();
    }
}