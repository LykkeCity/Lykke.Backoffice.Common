using Lykke.Backoffice.Common.Extensions.BrowserExtension;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Backoffice.Common
{
    /// <summary>
    /// Check minimal supported browser version for backoffice
    /// </summary>
    public class CheckBrowserMiddleware
    {
        private readonly RequestDelegate _next;
        private IDictionary<string, int> BrowserMajorVarsionMap = new Dictionary<string, int>
        {
            {"Chrome", 25},
            {"Firefox", 23},
            {"Safari", 7},
            {"Edge", 13},
        };
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="next"></param>
        public CheckBrowserMiddleware(RequestDelegate next)
        {
            _next = next;
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
            if (string.IsNullOrWhiteSpace(name) || !BrowserMajorVarsionMap.TryGetValue(name, out var version))
                return false;
            return Convert.ToInt32(useragentVersion) > version;
        }
    }
}
