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

        private void CommonClickHandler(object sender, RoutedEventArgs e)
        {
            MenuDataItem item = new MenuDataItem();
            FrameworkElement source = e.Source as FrameworkElement;

            switch(source.Name)
            {
                case "ButtonBrowseDevices": GetMenuDataItem(m_menuDataItem, "Browse", ref item);
                                            break;
                case "ButtonSubscriberRequest": GetMenuDataItem(m_menuDataItem, "Request", ref item);
                                                break;
                case "ButtonSubscribers": GetMenuDataItem(m_menuDataItem, "Subscribers", ref item);
                                          break;
                case "ButtonMeasurementGroup": GetMenuDataItem(m_menuDataItem, "Measurement Groups", ref item);
                                               break;
                case "ButtonSubscriberMeasurement": GetMenuDataItem(m_menuDataItem, "Subscriber Measurement", ref item);
                                                    break;
                case "ButtonMeasurementSubscription": GetMenuDataItem(m_menuDataItem, "Measurement Subscription", ref item);
                                                      break;
                case "ButtonSecurity": GetMenuDataItem(m_menuDataItem, "Security", ref item);
                                       break;
                default: break;
            }

            if (item.MenuText != null)
            {
                item.Command.Execute(null);
            }

        }

        private void GetMenuDataItem(ObservableCollection<MenuDataItem> items, string stringToMatch, ref MenuDataItem item)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].MenuText.Contains(stringToMatch))
                {
                    item = items[i];
                    break;
                }
                else
                {
                    if (items[i].SubMenuItems.Count > 0)
                    {
                        GetMenuDataItem(items[i].SubMenuItems, stringToMatch, ref item);
                    }
                }
            }
        }
    }
}
