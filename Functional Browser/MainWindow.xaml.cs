using CefSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Functional_Browser
{
    public partial class MainWindow : Window
    {
        public static readonly RoutedCommand CloseTabCommand = new RoutedCommand();
        private bool isToolbarAtBottom = false;

        private static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
            }
            return (IntPtr)0;
        }

        private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));
            int MONITOR_DEFAULTTONEAREST = 0x00000002;
            IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
            if (monitor != IntPtr.Zero)
            {
                MONITORINFO monitorInfo = new MONITORINFO();
                GetMonitorInfo(monitor, monitorInfo);
                RECT rcWorkArea = monitorInfo.rcWork;
                RECT rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
            }
            Marshal.StructureToPtr(mmi, lParam, true);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            /// <summary>x coordinate of point.</summary>
            public int x;
            /// <summary>y coordinate of point.</summary>
            public int y;
            /// <summary>Construct a point of coordinates (x,y).</summary>
            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MONITORINFO
        {
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
            public RECT rcMonitor = new RECT();
            public RECT rcWork = new RECT();
            public int dwFlags = 0;
        }

        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
            public static readonly RECT Empty = new RECT();
            public int Width { get { return Math.Abs(right - left); } }
            public int Height { get { return bottom - top; } }
            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }
            public RECT(RECT rcSrc)
            {
                left = rcSrc.left;
                top = rcSrc.top;
                right = rcSrc.right;
                bottom = rcSrc.bottom;
            }
            public bool IsEmpty { get { return left >= right || top >= bottom; } }
            public override string ToString()
            {
                if (this == Empty) { return "RECT {Empty}"; }
                return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom + " }";
            }
            public override bool Equals(object obj)
            {
                if (!(obj is Rect)) { return false; }
                return (this == (RECT)obj);
            }
            /// <summary>Return the HashCode for this struct (not garanteed to be unique)</summary>
            public override int GetHashCode() => left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();
            /// <summary> Determine if 2 RECT are equal (deep compare)</summary>
            public static bool operator ==(RECT rect1, RECT rect2) { return (rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom); }
            /// <summary> Determine if 2 RECT are different(deep compare)</summary>
            public static bool operator !=(RECT rect1, RECT rect2) { return !(rect1 == rect2); }
        }

        [DllImport("user32")]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        [DllImport("User32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

        public MainWindow()
        {
            InitializeComponent();

            // Binding a command to handlers
            CommandBinding closeTabBinding = new CommandBinding(CloseTabCommand, CloseTabCommand_Executed, CloseTabCommand_CanExecute);
            this.CommandBindings.Add(closeTabBinding);

            SourceInitialized += (s, e) =>
            {
                IntPtr handle = (new WindowInteropHelper(this)).Handle;
                HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WindowProc));
            };

            MinimizeButton.Click += (s, e) => WindowState = WindowState.Minimized;
            MaximizeButton.Click += (s, e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            CloseBtn.Click += (s, e) => this.Close();

            // Windows 11 rounded corners
            IntPtr hWnd = new WindowInteropHelper(GetWindow(this)).EnsureHandle();
            var attribute = DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE;
            var preference = DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;

            DwmSetWindowAttribute(hWnd, attribute, ref preference, sizeof(uint));
            UpdateGlobalAddressBar();
            // Resize TabContent window with delay
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(200);
            timer.Tick += (s, args) =>
            {
                timer.Stop();
                updateCurrentTabMargin();
            };
            timer.Start();
        }

        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern long DwmSetWindowAttribute(IntPtr hwnd,
                                                     DWMWINDOWATTRIBUTE attribute,
                                                     ref DWM_WINDOW_CORNER_PREFERENCE pvAttribute,
                                                     uint cbAttribute);



        public enum DWMWINDOWATTRIBUTE
        {
            DWMWA_WINDOW_CORNER_PREFERENCE = 33
        }

        public enum DWM_WINDOW_CORNER_PREFERENCE
        {
            DWMWCP_DEFAULT = 0,
            DWMWCP_DONOTROUND = 1,
            DWMWCP_ROUND = 2,
            DWMWCP_ROUNDSMALL = 3
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            IntPtr hWnd = new WindowInteropHelper(GetWindow(this)).EnsureHandle();
            MainWindow Current = this;

            if (Current.WindowState != WindowState.Maximized)
            {

                var attribute = DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE;
                var preference = DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
                DwmSetWindowAttribute(hWnd, attribute, ref preference, sizeof(uint));
            }
            else
            {
                var attribute = DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE;
                var preference = DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_DONOTROUND;
                DwmSetWindowAttribute(hWnd, attribute, ref preference, sizeof(uint));
            }

        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabControl.SelectedItem is TabItem selectedItem)
            {
                if (selectedItem.Header is string header && header == "+")
                {
                    CreateNewTab();
                }
                updateCurrentTabMargin(selectedItem);
            }
            UpdateGlobalAddressBar();
        }

        public void CreateNewTab(string url = "https://www.bing.com")
        {
            TabItem newTab = new TabItem();
            var tabContent = new TabContent
            {
                Url = url
            };
            newTab.Content = tabContent;
            newTab.Header = tabContent;
            newTab.HeaderTemplate = (DataTemplate)FindResource("TabHeaderTemplate");

            // It is assumed that there is a special tab (AddTabItem)
            int addTabIndex = TabControl.Items.IndexOf(AddTabItem);
            TabControl.Items.Insert(addTabIndex, newTab);
            TabControl.SelectedItem = newTab;
        }

        private void CloseTabCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = e.Parameter is TabItem tabItem &&
                           !(tabItem.Header is string header && header == "+");
        }

        private void CloseTabCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is TabItem tabItem)
            {
                int removedIndex = TabControl.Items.IndexOf(tabItem);
                bool wasSelected = TabControl.SelectedItem == tabItem;

                if (tabItem.Header is string header && header == "+")
                    return;

                if (wasSelected)
                {
                    if (removedIndex - 1 >= 0)
                    {
                        TabControl.SelectedIndex = removedIndex - 1;
                    }
                    else
                    {
                        MessageBoxResult result = MessageBox.Show("Close Browser?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            this.Close();
                            return;
                        }
                        else if (TabControl.Items.Count > 1)
                        {
                            TabControl.SelectedIndex = 1;
                        }
                        else
                        {
                            return;
                        }
                    }
                }
                TabControl.Items.Remove(tabItem);
            }
        }

        private TabContent GetCurrentTabContent()
        {
            if (TabControl.SelectedItem is TabItem selectedTab)
            {
                return selectedTab.Content as TabContent;
            }
            return null;
        }

        private void UpdateGlobalAddressBar()
        {
            var currentTab = GetCurrentTabContent();
            if (currentTab != null)
            {
                AddressBox.Text = currentTab.Url;
            }
            else
            {
                AddressBox.Text = "";
            }
        }

        private void AddressBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var input = AddressBox.Text.Trim();
                if (string.IsNullOrEmpty(input))
                    return;

                // Простейшая нормализация ввода
                if (!input.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !input.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    // Если строка содержит пробелы или нет точки – считаем, что это поисковый запрос
                    if (input.Contains(" ") || !input.Contains("."))
                    {
                        input = $"https://www.google.com/search?q={Uri.EscapeDataString(input)}";
                    }
                    else
                    {
                        input = "http://" + input;
                    }
                }

                var currentTab = GetCurrentTabContent();
                if (currentTab != null)
                {
                    currentTab.Browser.Load(input);
                    currentTab.Url = input;
                }

                e.Handled = true;
            }
        }

        // Кнопки навигации — делегируют действие активной вкладке.
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            var currentTab = GetCurrentTabContent();
            if (currentTab?.Browser?.CanGoBack == true)
                currentTab.Browser.Back();
        }
        private void btnForward_Click(object sender, RoutedEventArgs e)
        {
            var currentTab = GetCurrentTabContent();
            if (currentTab?.Browser?.CanGoForward == true)
                currentTab.Browser.Forward();
        }
        private void btnReload_Click(object sender, RoutedEventArgs e)
        {
            var currentTab = GetCurrentTabContent();
            currentTab?.Browser.Reload();
        }
        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            var currentTab = GetCurrentTabContent();
            if (currentTab != null)
            {
                currentTab.Browser.Load("https://www.bing.com");
                currentTab.Url = "https://www.bing.com";
            }
        }
        // Переключение расположения панели инструментов.
        private void btnTogglePosition_Click(object sender, RoutedEventArgs e)
        {
            isToolbarAtBottom = !isToolbarAtBottom;
            if (isToolbarAtBottom)
            {
                // Устанавливаем панель в нижнюю строку.
                Grid.SetRow(GlobalToolbar, 2);
                Grid.SetRowSpan(TabControl, 2);
                RootGrid.RowDefinitions[2].Height = new GridLength(60);
                RootGrid.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);
                // При необходимости можно менять высоту строк: например, фиксированная высота для панели и оставшаяся часть — для контента.
            }
            else
            {
                // Устанавливаем панель в верхнюю строку.
                Grid.SetRow(GlobalToolbar, 1);
                Grid.SetRowSpan(TabControl, 3);
                RootGrid.RowDefinitions[1].Height = new GridLength(60);
                RootGrid.RowDefinitions[2].Height = new GridLength(2, GridUnitType.Star);
            }

            updateCurrentTabMargin();
        }


        /// <summary>
        /// Custom method for Popup positioning.
        /// </summary>
        private CustomPopupPlacement[] PlacePopup(Size popupSize, Size targetSize, Point offset)
        {
            double x = -popupSize.Width;
            double y = 0;

            if (!isToolbarAtBottom)
            {
                y = targetSize.Height;
            }
            else
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

        private void btnShowMenu_Click(object sender, RoutedEventArgs e)
        {
            // Set custom position for Popup
            popupMenu.Placement = PlacementMode.Custom;
            popupMenu.PlacementTarget = btnShowMenu;
            popupMenu.CustomPopupPlacementCallback = new CustomPopupPlacementCallback(PlacePopup);
            popupMenu.IsOpen = true;
        }

        private void updateCurrentTabMargin(TabItem tabItem = null)
        {
            TabContent activeTabContent;
            if (tabItem == null)
            {
                activeTabContent = GetCurrentTabContent();
            }
            else
            {
                activeTabContent = tabItem.Content as TabContent;
            }

            if (activeTabContent == null)
            {
                Debug.WriteLine("Active TabContent is null. Cannot update margin.");
                return;
            }

            double topMargin = isToolbarAtBottom ? 0 : GlobalToolbar.ActualHeight;
            activeTabContent.UpdateContentMargin(topMargin);
        }
    }
}
