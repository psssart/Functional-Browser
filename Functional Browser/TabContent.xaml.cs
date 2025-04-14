using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CefSharp.Wpf;

namespace Functional_Browser
{
    public partial class TabContent : UserControl
    {
        public TabContent()
        {
            InitializeComponent();
            Browser.LifeSpanHandler = new CustomLifeSpanHandler();
            Browser.DisplayHandler = new CustomDisplayHandler();
            Browser.Tag = this;
            DependencyPropertyDescriptor.FromProperty(ChromiumWebBrowser.AddressProperty, typeof(ChromiumWebBrowser))
            .AddValueChanged(Browser, Browser_AddressChanged);
        }

        /// <summary>
        /// Web-page url.
        /// </summary>
        #region Dependency Property: Url
        public string Url
        {
            get { return (string)GetValue(UrlProperty); }
            set { SetValue(UrlProperty, value); }
        }
        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UrlProperty =
            DependencyProperty.Register("Url", typeof(string), typeof(TabContent), new PropertyMetadata("https://www.bing.com", OnUrlChanged));

        private static void OnUrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as TabContent;
            if (control != null && control.Browser != null)
            {
                control.Browser.Address = e.NewValue as string;
            }
        }
        #endregion

        /// <summary>
        /// Web-page title.
        /// </summary>
        #region Dependency Property: Title
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(TabContent), new PropertyMetadata("New Document"));
        #endregion

        /// <summary>
        /// Icon (favicon) of web-page.
        /// </summary>
        #region Dependency Property: Icon
        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(ImageSource), typeof(TabContent), new PropertyMetadata(null));
        #endregion

        /// <summary>
        /// Update web-page icon.
        /// </summary>
        public void UpdateIcon(ImageSource icon)
        {
            Icon = icon;
        }

        /// <summary>
        /// Handle browser initialization.
        /// </summary>
        private void Browser_Initialized(object sender, EventArgs e)
        {
            Browser.Address = Url;
            Browser.TitleChanged += (s, args) =>
            {
                Dispatcher.Invoke(() =>
                {
                    Title = args.NewValue as string;
                });
            };
        }

        /// <summary>
        /// Handle browser address changing.
        /// </summary>
        private void Browser_AddressChanged(object sender, EventArgs e)
        {
            Url = Browser.Address;
        }

        /// <summary>
        /// Set margin for web-page space.
        /// </summary>
        public void UpdateContentMargin(double topMargin)
        {
            ContentGrid.Margin = new Thickness(0, topMargin, 0, 0);
        }
    }
}
