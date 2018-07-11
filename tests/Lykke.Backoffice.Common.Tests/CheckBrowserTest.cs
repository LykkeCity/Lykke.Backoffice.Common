using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Backoffice.Common.Tests
{
    [TestFixture]
    public class CheckBrowserTest
    {
        private readonly DefaultHttpContext _context;
        private CheckBrowserMiddleware _middleware { get; set; }
        public Mock<IDelegateMock> RequestDelegateMock { get; set; }
        public CheckBrowserTest()
        {
            _context = new DefaultHttpContext();
            RequestDelegateMock = new Mock<IDelegateMock>();
            RequestDelegateMock
                .Setup(x => x.RequestDelegate(It.IsAny<HttpContext>()))
                .Returns(Task.FromResult(0));

            var list = new List<Browser>();
            var browser = new Browser()
            {
                Name = "Chrome",
                MinMajorVersion = "25",
                MaxMajorVersion = "70"
            };
            list.Add(browser);
            browser = new Browser()
            {
                Name = "Firefox",
                MinMajorVersion = "23",
                MaxMajorVersion = "70"
            };
            list.Add(browser);
            browser = new Browser()
            {
                Name = "Safari",
                MinMajorVersion = "7",
                MaxMajorVersion = "13"
            };
            list.Add(browser);
            browser = new Browser()
            {
                Name = "Edge",
                MinMajorVersion = "13",
                MaxMajorVersion = "20"
            };
            list.Add(browser);

            var skipUrls = new [] { "/api/isalive", "/api/test" };
            _middleware = new CheckBrowserMiddleware(RequestDelegateMock.Object.RequestDelegate, list, skipUrls);
        }

        [TestCase("1dfdf", ExpectedResult = 403)]
        [TestCase("", ExpectedResult = 403)]
        public async Task<int> CheckBrowserVersion_WhenOthersBrowser_VersionNotSupport(object userAgent)
        {
            _context.Request.Headers["User-Agent"] = userAgent.ToString();
            await _middleware.Invoke(_context);
            return _context.Response.StatusCode;
        }

        [TestCase("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3396.99 Safari/537.36", ExpectedResult = 403)]
        [TestCase("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36", ExpectedResult = 200)]
        [TestCase("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/23.0.3396.99 Safari/537.36", ExpectedResult = 403)]
        public async Task<int> CheckBrowserVersion_WhenChrome_VersionShouldBeGreater25(object userAgent)
        {
            _context.Request.Headers["User-Agent"] = userAgent.ToString();
            await _middleware.Invoke(_context);
            return _context.Response.StatusCode;
        }

        [TestCase("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:61.0) Gecko/20100101 Firefox/70.0", ExpectedResult = 403)]
        [TestCase("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:61.0) Gecko/20100101 Firefox/61.0", ExpectedResult = 200)]
        [TestCase("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:61.0) Gecko/20100101 Firefox/22.0.1", ExpectedResult = 403)]
        public async Task<int> CheckBrowserVersion_WhenFirefox_VersionShouldBeGreater23(object userAgent)
        {
            _context.Request.Headers["User-Agent"] = userAgent.ToString();
            await _middleware.Invoke(_context);
            return _context.Response.StatusCode;
        }

        [TestCase("Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko", ExpectedResult = 403)]
        public async Task<int> CheckBrowserVersion_WhenIE_Version(object userAgent)
        {
            _context.Request.Headers["User-Agent"] = userAgent.ToString();
            await _middleware.Invoke(_context);
            return _context.Response.StatusCode;
        }
        [TestCase("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_5) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/13.1.1 Safari/605.1.15", ExpectedResult = 403)]
        [TestCase("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_5) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/11.1.1 Safari/605.1.15", ExpectedResult = 200)]
        [TestCase("Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/534.54.16 (KHTML, like Gecko) Version/5.1.4 Safari/534.54.16", ExpectedResult = 403)]
        public async Task<int> CheckBrowserVersion_WhenSafari_VersionShouldBeGreater7(object userAgent)
        {
            _context.Request.Headers["User-Agent"] = userAgent.ToString();
            await _middleware.Invoke(_context);
            return _context.Response.StatusCode;
        }
        [TestCase("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.140 Safari/537.36 Edge/20.17134", ExpectedResult = 403)]
        [TestCase("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.140 Safari/537.36 Edge/17.17134", ExpectedResult = 200)]
        [TestCase("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.140 Safari/537.36 Edge/12.17134", ExpectedResult = 403)]
        public async Task<int> CheckBrowserVersion_WhenEdge_VersionShouldBeGreater12(object userAgent)
        {
            _context.Request.Headers["User-Agent"] = userAgent.ToString();
            await _middleware.Invoke(_context);
            return _context.Response.StatusCode;
        }
    }
    public interface IDelegateMock
    {
        Task RequestDelegate(HttpContext context);
    }
}
