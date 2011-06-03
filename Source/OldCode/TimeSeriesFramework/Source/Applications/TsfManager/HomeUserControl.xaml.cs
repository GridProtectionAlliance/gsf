using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TimeSeriesFramework.UI.UserControls;
using TimeSeriesFramework.UI;
using System.Collections.ObjectModel;

namespace TsfManager
{
    /// <summary>
    /// Interaction logic for HomeUserControl.xaml
    /// </summary>
    public partial class HomeUserControl : UserControl
    {
        private ObservableCollection<MenuDataItem> m_menuDataItem;

        public HomeUserControl()
        {
            InitializeComponent();
            m_menuDataItem = ((MainWindow)Application.Current.MainWindow).MenuDataItems;
        }

        private void ButtonBrowseDevices_Click(object sender, RoutedEventArgs e)
        {

            (m_menuDataItem.FirstOrDefault(m => m.MenuText == "Browse")).Command.Execute(null);
            
            //m_menuDataItem.Select(m => m.UserControlPath.Contains("DeviceList"));
        }
    }
}
