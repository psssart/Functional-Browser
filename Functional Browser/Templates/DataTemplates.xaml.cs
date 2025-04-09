using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Functional_Browser
{
    public partial class TabHeaderResources : ResourceDictionary
    {
        public TabHeaderResources()
        {
            InitializeComponent();
        }

        // Обработчик клика для закрытия вкладки
        private void Tab_Close_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                // Находим родительский элемент типа TabItem
                TabItem tabItem = FindParent<TabItem>(button);
                if (tabItem != null)
                {
                    // Находим TabControl, содержащий этот TabItem
                    TabControl tabControl = FindParent<TabControl>(tabItem);
                    if (tabControl != null)
                    {
                        // При необходимости можно проверять, чтобы не удалять особую вкладку, например, с заголовком "+"
                        tabControl.Items.Remove(tabItem);
                    }
                }
            }
        }

        // Вспомогательный метод для поиска родительского элемента указанного типа
        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null)
                return null;

            if (parentObject is T parent)
                return parent;
            else
                return FindParent<T>(parentObject);
        }
    }
}
