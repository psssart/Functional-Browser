using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CefSharp.Wpf;

namespace Functional_Browser
{
    public partial class TableContent : UserControl
    {
        public TableContent()
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
            DependencyProperty.Register("Url", typeof(string), typeof(TableContent), new PropertyMetadata("https://www.bing.com", OnUrlChanged));

        private static void OnUrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as TableContent;
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
            DependencyProperty.Register("Title", typeof(string), typeof(TableContent), new PropertyMetadata("New Document"));
        #endregion

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

        private void Browser_AddressChanged(object sender, EventArgs e)
        {
            string newAddress = Browser.Address;
            string displayAddress = newAddress;

            if (!string.IsNullOrEmpty(newAddress))
            {
                if (newAddress.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    displayAddress = newAddress.Substring("https://".Length);
                }
                else if (newAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                {
                    displayAddress = newAddress.Substring("http://".Length);
                }
            }

            Dispatcher.Invoke(() =>
            {
                AddressBox.Text = displayAddress;
                Url = newAddress;

                if (!string.IsNullOrEmpty(newAddress) &&
                    newAddress.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    SecureStatusTextBlock.Text = "Secure";
                    SecureStatusIcon.Data = (Geometry)Application.Current.Resources["PadlockIcon"];
                }
                else
                {
                    SecureStatusTextBlock.Text = "Not Secure";
                    SecureStatusIcon.Data = (Geometry)Application.Current.Resources["UnlockedPadlockIcon"];
                }
            });
        }

        private void AddressBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string input = (sender as TextBox)?.Text.Trim();
                if (string.IsNullOrEmpty(input))
                    return;

                if (!input.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !input.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    if (input.Contains(" ") || !input.Contains("."))
                    {
                        input = $"https://www.google.com/search?q={Uri.EscapeDataString(input)}";
                    }
                    else
                    {
                        input = "http://" + input;
                    }
                }

                Browser.Load(input);
                Url = input;
                e.Handled = true;
            }
        }
    }
}
