using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Functional_Browser
{
    public class CustomRequestHandler : IRequestHandler
    {
        private readonly List<string> adKeywords = new List<string>
        {
            "doubleclick.net",
            "googlesyndication.com",
            "ads.",
            "/ads/",
            "adservice.",
            "adnxs.com",
            "track.",
            "banner"
        };

        public CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser,
            IFrame frame, IRequest request, IRequestCallback callback)
        {
            var url = request.Url.ToLowerInvariant();

            if (adKeywords.Any(keyword => url.Contains(keyword)))
            {
                Console.WriteLine($"[AdBlock] Blocked: {url}");
                return CefReturnValue.Cancel;
            }

            return CefReturnValue.Continue;
        }

        public IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser,
            IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator,
            ref bool disableDefaultHandling)
        {
            return null;
        }

        public bool OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame,
            IRequest request, bool userGesture, bool isRedirect) => false;

        public bool OnCertificateError(IWebBrowser chromiumWebBrowser, IBrowser browser, CefErrorCode errorCode,
            string requestUrl, ISslInfo sslInfo, IRequestCallback callback) => false;

        public bool OnOpenUrlFromTab(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame,
            string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture) => false;

        public bool OnProtocolExecution(IWebBrowser chromiumWebBrowser, IBrowser browser, string url) => false;

        public bool OnQuotaRequest(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl,
            long newSize, IRequestCallback callback) => false;

        public void OnRenderProcessTerminated(IWebBrowser chromiumWebBrowser, IBrowser browser,
            CefTerminationStatus status, int errorCode, string errorMessage)
        {
            Console.WriteLine($"[CEF] Render process terminated. Code: {errorCode}, Message: {errorMessage}");
        }

        public void OnRenderViewReady(IWebBrowser chromiumWebBrowser, IBrowser browser) { }

        public bool OnSelectClientCertificate(IWebBrowser chromiumWebBrowser, IBrowser browser, bool isProxy,
            string host, int port, X509Certificate2Collection certificates, ISelectClientCertificateCallback callback) => false;

        public void OnDocumentAvailableInMainFrame(IWebBrowser chromiumWebBrowser, IBrowser browser) { }

        public void OnFaviconUrlChange(IWebBrowser chromiumWebBrowser, IBrowser browser, IList<string> urls) { }

        public void OnPluginCrashed(IWebBrowser chromiumWebBrowser, IBrowser browser, string pluginPath) { }

        public bool GetAuthCredentials(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame,
            bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback) => false;

        public bool GetAuthCredentials(IWebBrowser chromiumWebBrowser, IBrowser browser,
            string originUrl, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback) => false;
    }
}
