using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using CefSharp;
using CefSharp.Enums;
using CefSharp.Structs;
using CefSharp.Wpf;

namespace Functional_Browser
{
    public class CustomDisplayHandler : IDisplayHandler
    {
        /// <summary>
        /// Called when the web page's favicon changes.
        /// </summary>
        public void OnFaviconUrlChange(IWebBrowser browserControl, IBrowser browser, IList<string> urls)
        {
            if (urls != null && urls.Any())
            {
                string faviconUrl = urls.First();

                // Download favicon in a separate thread
                Task.Run(() =>
                {
                    try
                    {
                        WebRequest request = WebRequest.Create(faviconUrl);
                        using (WebResponse response = request.GetResponse())
                        using (Stream stream = response.GetResponseStream())
                        using (MemoryStream memStream = new MemoryStream())
                        {
                            // Copy data from the source stream to MemoryStream
                            stream.CopyTo(memStream);
                            byte[] imageData = memStream.ToArray();

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                BitmapImage bitmap = new BitmapImage();
                                bitmap.BeginInit();
                                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                // Create a new MemoryStream on the UI thread
                                bitmap.StreamSource = new MemoryStream(imageData);
                                bitmap.EndInit();
                                bitmap.Freeze(); // Safely freeze the object

                                // Using the Tag property to get the browser owner (TabContent)
                                var chromiumBrowser = browserControl as ChromiumWebBrowser;
                                if (chromiumBrowser != null && chromiumBrowser.Tag is TabContent tabContent)
                                {
                                    tabContent.UpdateIcon(bitmap);

                                    // Update the main window icon
                                    if (Application.Current.MainWindow is MainWindow mainWindow)
                                    {
                                        mainWindow.Icon = bitmap;
                                    }
                                }
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                });
            }
        }

        public void OnAddressChanged(IWebBrowser chromiumWebBrowser, AddressChangedEventArgs addressChangedArgs) { }
        public bool OnAutoResize(IWebBrowser chromiumWebBrowser, IBrowser browser, CefSharp.Structs.Size newSize) => false;
        public bool OnCursorChange(IWebBrowser chromiumWebBrowser, IBrowser browser, IntPtr cursor, CursorType type, CursorInfo customCursorInfo) => false;
        public void OnTitleChanged(IWebBrowser chromiumWebBrowser, TitleChangedEventArgs titleChangedArgs){ }
        public void OnFullscreenModeChange(IWebBrowser chromiumWebBrowser, IBrowser browser, bool fullscreen){ }

        public void OnLoadingProgressChange(IWebBrowser chromiumWebBrowser, IBrowser browser, double progress){ }
        public bool OnTooltipChanged(IWebBrowser chromiumWebBrowser, ref string text) => false;
        public void OnStatusMessage(IWebBrowser chromiumWebBrowser, StatusMessageEventArgs statusMessageArgs){ } // Status messages can be processed if necessary.
        public bool OnConsoleMessage(IWebBrowser chromiumWebBrowser, ConsoleMessageEventArgs consoleMessageArgs) => false;
    }
}
