using CefSharp;
using CefSharp.Wpf;
using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Functional_Browser
{
    internal class CustomLifeSpanHandler : ILifeSpanHandler
    {
        // Intercept the popup window creation event
        public bool OnBeforePopup(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl,
                                  string targetFrameName, WindowOpenDisposition targetDisposition,
                                  bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo,
                                  IBrowserSettings settings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
        {
            // Cancel the standard behavior
            newBrowser = null;

            // Call the creation of a new tab in the UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.CreateNewTab(targetUrl);
                }
            });

            return true;
        }

        public void OnAfterCreated(IWebBrowser browserControl, IBrowser browser) { }
        public bool DoClose(IWebBrowser browserControl, IBrowser browser) => false;
        public void OnBeforeClose(IWebBrowser browserControl, IBrowser browser) { }
    }
}
