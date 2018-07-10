using Lykke.Backoffice.Common.Extensions.BrowserExtension;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private readonly IEnumerable<string> _skipUrls;
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="next"></param>
        /// <param name="browsers"></param>
        public CheckBrowserMiddleware(RequestDelegate next, IEnumerable<Browser> browsers)
        {
            _next = next;
            _browsers = browsers;
            var urls = new List<string>();
            urls.Add("/api/isalive");
            _skipUrls = urls;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        /// <param name="browsers"></param>
        /// <param name="skipUrls"></param>
        public CheckBrowserMiddleware(RequestDelegate next, IEnumerable<Browser> browsers, IEnumerable<string> skipUrls)
        {
            _next = next;
            _browsers = browsers;
            _skipUrls = skipUrls;
        }
        /// <summary>
        /// Invoke method
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task Invoke(HttpContext context)
        {
            if (context != null && context.Request != null &&
                context.Request.Path != null &&
                context.Request.Path.HasValue 
                && _skipUrls.Contains(context.Request.Path.Value.ToLower()))
            {
                context.Response.StatusCode = 200;
                return _next(context);
            }

            var useragentHeader = context.Request.Headers["User-Agent"];
            var useragent = new UserAgent(useragentHeader);
            var supportedBrowser = CheckBrowserMajorVersion(useragent.Browser.Name, useragent.Browser.Major);
            context.Response.StatusCode = 200;
            if (!supportedBrowser)
            {
                context.Response.StatusCode = 403;
                var sb = new StringBuilder();
                foreach (var browser in _browsers)
                    sb.AppendLine(string.Format("{0} min version: '{1}', max version: '{2}'", browser.Name, browser.MinMajorVersion, browser.MaxMajorVersion));
                context.Response.WriteAsync(string.Format("<html><div>Forbidden, because your browser does not meet safety requirements. Following browsers are allowed:</div><div>{0}</div></html>", sb.ToString()));
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
            var minVersion = TryParseNullable(browser.MinMajorVersion);
            var maxVersion = TryParseNullable(browser.MaxMajorVersion);

            if (minVersion != null && maxVersion != null)
                return (Convert.ToInt32(useragentVersion) > minVersion && Convert.ToInt32(useragentVersion) < maxVersion);
            if (minVersion == null && maxVersion == null)
                return false;
            if (maxVersion == null && minVersion != null)
                return Convert.ToInt32(useragentVersion) > minVersion;
            if (maxVersion != null && minVersion == null)
                return Convert.ToInt32(useragentVersion) < maxVersion;
            return false;
        }
        private int? TryParseNullable(string val)
        {
            int outValue;
            return int.TryParse(val, out outValue) ? (int?)outValue : null;
        }
    }
}
