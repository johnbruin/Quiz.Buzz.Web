using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace View
{
    /// <summary>
    /// Interaction logic for StatusView.xaml
    /// </summary>
    public partial class StatusView : Window
    {
        public StatusView()
        {
            InitializeComponent();
            ((INotifyCollectionChanged)mainDataGrid.Items).CollectionChanged += StatusView_CollectionChanged;
        }

        private void StatusView_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (mainDataGrid.Items.Count > 0)
            {
                if (VisualTreeHelper.GetChild(mainDataGrid, 0) is Decorator border)
                {
                    if (border.Child is ScrollViewer scroll) scroll.ScrollToEnd();
                }
            }
        }
    }
}