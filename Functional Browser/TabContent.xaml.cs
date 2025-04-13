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

        /// <summary>
        /// Handle keys clicks in address box.
        /// </summary>
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

        /// <summary>
        /// Handle back button click.
        /// </summary>
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (Browser.CanGoBack)
                Browser.Back();
        }

        /// <summary>
        /// Handle forward button click.
        /// </summary>
        private void btnForward_Click(object sender, RoutedEventArgs e)
        {
            if (Browser.CanGoForward)
                Browser.Forward();
        }

        /// <summary>
        /// Handle reload button click.
        /// </summary>
        private void btnReload_Click(object sender, RoutedEventArgs e)
        {
            Browser.Reload();
        }

        /// <summary>
        /// Handle home button click.
        /// </summary>
        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            Browser.Address = "https://www.bing.com";
        }

        /// <summary>
        /// Handle menu button click.
        /// </summary>
        private void btnShowMenu_Click(object sender, RoutedEventArgs e)
        {
            // Set custom position for Popup
            popupMenu.Placement = PlacementMode.Custom;
            popupMenu.PlacementTarget = btnShowMenu;
            popupMenu.CustomPopupPlacementCallback = new CustomPopupPlacementCallback(PlacePopup);
            popupMenu.IsOpen = true;
        }

        /// <summary>
        /// Custom method for Popup positioning.
        /// </summary>
        private CustomPopupPlacement[] PlacePopup(Size popupSize, Size targetSize, Point offset)
        {
            double x = -popupSize.Width;
            double y = 0;

            if (SearchBar.Equals("Top", StringComparison.OrdinalIgnoreCase))
            {
                y = targetSize.Height;
            }
            else if (SearchBar.Equals("Bottom", StringComparison.OrdinalIgnoreCase))
            {
                y = -popupSize.Height;
            }

            CustomPopupPlacement placement = new CustomPopupPlacement(new Point(x, y), PopupPrimaryAxis.None);
            return new CustomPopupPlacement[] { placement };
        }

        /// <summary>
        /// Switch app color theme.
        /// </summary>
        private void btnSwitchTheme_Click(object sender, RoutedEventArgs e)
        {
            var mergedDictionaries = Application.Current.Resources.MergedDictionaries;

            var currentThemeDictionary = mergedDictionaries.FirstOrDefault(dict =>
                 dict.Contains("IsThemeDictionary") && (bool)dict["IsThemeDictionary"] == true);

            if (currentThemeDictionary != null && currentThemeDictionary.Source != null)
            {
                string source = currentThemeDictionary.Source.OriginalString.ToLowerInvariant();

                if (source.EndsWith("darktheme.xaml"))
                {
                    mergedDictionaries.Remove(currentThemeDictionary);
                    mergedDictionaries.Add(new ResourceDictionary
                    {
                        Source = new Uri("/Assets/Themes/LightTheme.xaml", UriKind.Relative)
                    });
                }
                else if (source.EndsWith("lighttheme.xaml"))
                {
                    mergedDictionaries.Remove(currentThemeDictionary);
                    mergedDictionaries.Add(new ResourceDictionary
                    {
                        Source = new Uri("/Assets/Themes/DarkTheme.xaml", UriKind.Relative)
                    });
                }
                else
                {
                    // Default behavior - Dark Theme
                    mergedDictionaries.Remove(currentThemeDictionary);
                    mergedDictionaries.Add(new ResourceDictionary
                    {
                        Source = new Uri("/Assets/Themes/DarkTheme.xaml", UriKind.Relative)
                    });
                }
            }
            else
            {
                // Default behavior - Dark Theme
                mergedDictionaries.Add(new ResourceDictionary
                {
                    Source = new Uri("/Assets/Themes/DarkTheme.xaml", UriKind.Relative)
                });
            }
        }
    }
}
