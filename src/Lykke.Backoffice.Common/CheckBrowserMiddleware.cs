using Lykke.Backoffice.Common.Extensions.BrowserExtension;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lykke.Backoffice.Common
{
    /// <summary>
    /// Check minimal supported browser version for backoffice
    /// </summary>
    public class CheckBrowserMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IEnumerable<Browser> _browsers;
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="next"></param>
        /// <param name="browsers"></param>
        public CheckBrowserMiddleware(RequestDelegate next, IEnumerable<Browser> browsers)
        {
            _next = next;
            _browsers = browsers;
        }
        /// <summary>
        /// Invoke method
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task Invoke(HttpContext context)
        {
            var useragentHeader = context.Request.Headers["User-Agent"];
            var useragent = new UserAgent(useragentHeader);
            var supportedBrowser = CheckBrowserMajorVersion(useragent.Browser.Name, useragent.Browser.Major);
            context.Response.StatusCode = 200;
            if (!supportedBrowser)
            {
                context.Response.StatusCode = 403;
                context.Response.WriteAsync("<html><div>Forbidden</div></html>");
            }
            return _next(context);
        }
        private bool CheckBrowserMajorVersion(string name, string useragentVersion)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;
            var browser = _browsers.FirstOrDefault(x => x.Name == name);
            if (browser == null)
                return false;
            if (browser.MaxMajorVersion != null && browser.MinMajorVersion != null)
                return (Convert.ToInt32(useragentVersion) > browser.MinMajorVersion && Convert.ToInt32(useragentVersion) < browser.MaxMajorVersion);
            if (browser.MinMajorVersion == null && browser.MaxMajorVersion == null)
                return false;
            if (browser.MaxMajorVersion == null && browser.MinMajorVersion != null)
                return Convert.ToInt32(useragentVersion) > browser.MinMajorVersion;
            if (browser.MaxMajorVersion != null && browser.MinMajorVersion == null)
                return Convert.ToInt32(useragentVersion) < browser.MaxMajorVersion;
            return false;
        }
    }
}
