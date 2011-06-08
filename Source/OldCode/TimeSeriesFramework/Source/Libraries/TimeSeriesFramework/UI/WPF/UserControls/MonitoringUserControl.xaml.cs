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
using TimeSeriesFramework.UI.ViewModels;

namespace TimeSeriesFramework.UI.UserControls
{
    /// <summary>
    /// Interaction logic for MonitoringUserControl.xaml
    /// </summary>
    public partial class MonitoringUserControl : UserControl
    {
        Monitor m_dataContext;
        public MonitoringUserControl()
        {
            InitializeComponent();
            m_dataContext = new Monitor();
            this.Unloaded += new RoutedEventHandler(MonitoringUserControl_Unloaded);
        }

        void MonitoringUserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            m_dataContext.DetachServiceEvents();
        }
    }
}
