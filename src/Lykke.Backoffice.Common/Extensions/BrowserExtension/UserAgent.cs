namespace Lykke.Backoffice.Common.Extensions.BrowserExtension
{
    /// <summary>
    /// User agent
    /// </summary>
    public class UserAgent
    {
        private string _userAgent;
        private ClientBrowser _browser;
        /// <summary>
        /// Client browser
        /// </summary>
        public ClientBrowser Browser
        {
            get
            {
                if (_browser == null)
                {
                    _browser = new ClientBrowser(_userAgent);
                }
                return _browser;
            }
        }
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="userAgent"></param>
        public UserAgent(string userAgent)
        {
            _userAgent = userAgent;
        }
    }
}
