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
    /// Interaction logic for CalculatedMeasurementUserControl.xaml
    /// </summary>
    public partial class CalculatedMeasurementUserControl : UserControl
    {
        public CalculatedMeasurementUserControl()
        {
            InitializeComponent();
            this.Unloaded += new RoutedEventHandler(CalculatedMeasurementUserControl_Unloaded);
            this.DataContext = new CalculatedMeasurements(10);
        }

        void CalculatedMeasurementUserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            (this.DataContext as CalculatedMeasurements).ProcessPropertyChange();
        }

        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                DataGrid dataGrid = sender as DataGrid;
                if (dataGrid.SelectedItems.Count > 0)
                {
                    if (MessageBox.Show("Are you sure you want to delete " + dataGrid.SelectedItems.Count + " selected item(s)?", "Delete Selected Items", MessageBoxButton.YesNo) == MessageBoxResult.No)
                        e.Handled = true;
                }
            }
        }
    }
}
