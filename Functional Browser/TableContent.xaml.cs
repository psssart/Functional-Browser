using System;
using System.Collections.Generic;
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

namespace Functional_Browser
{
    /// <summary>
    /// Логика взаимодействия для TableContent.xaml
    /// </summary>
    public partial class TableContent : UserControl
    {
        public TableContent()
        {
            InitializeComponent();
        }



        public string Url
        {
            get { return (string)GetValue(UrlProperty); }
            set { SetValue(UrlProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UrlProperty =
            DependencyProperty.Register("Url", typeof(string), typeof(TableContent), new PropertyMetadata("https://www.bing.com"));

        private void Browser_Initialized(object sender, EventArgs e)
        {
            Browser.Address = Url;
        }
    }
}
