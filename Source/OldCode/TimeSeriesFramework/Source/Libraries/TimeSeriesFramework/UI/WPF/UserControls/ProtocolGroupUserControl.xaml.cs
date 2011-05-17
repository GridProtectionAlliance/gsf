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
    /// Interaction logic for ProtocolGroupUserControl.xaml
    /// </summary>
    public partial class ProtocolGroupUserControl : UserControl
    {
        #region [ Constructor ]

        /// <summary>
        /// Creates an instance of <see cref="ProtocolGroupUserControl"/> class.
        /// </summary> 
        public ProtocolGroupUserControl()
        {
            InitializeComponent();
            this.Unloaded += new RoutedEventHandler(ProtocolGroupUserControl_Unloaded);
            this.DataContext = new Protocols(10);
        } 

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Handles unloaded event.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Arguments of the event.</param>
        void ProtocolGroupUserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            (this.DataContext as Protocols).ProcessPropertyChange();
        }

        /// <summary>
        /// Handles key down event on the datagrid object.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Arguments of the event.</param>
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
        #endregion
    }
}
