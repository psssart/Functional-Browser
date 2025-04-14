using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CefSharp;
using CefSharp.Wpf;

namespace Functional_Browser
{
    public partial class TabContent : UserControl
    {
        public string SearchBar { get; set; } = "Top";

        public TabContent()
        {
            InitializeComponent();
            Browser.LifeSpanHandler = new CustomLifeSpanHandler();
            DependencyPropertyDescriptor.FromProperty(ChromiumWebBrowser.AddressProperty, typeof(ChromiumWebBrowser))
            .AddValueChanged(Browser, Browser_AddressChanged);
        }

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

        public void UpdateContentMargin(double topMargin)
        {
            ContentGrid.Margin = new Thickness(0, topMargin, 0, 0);
        }
    }
}
