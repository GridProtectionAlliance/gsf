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
using System.Xml.Serialization;
using System.Xml;

namespace TimeSeriesFramework.UI.UserControls
{
    /// <summary>
    /// Interaction logic for SubscriberUserControl.xaml
    /// </summary>
    public partial class SubscriberUserControl : UserControl
    {
        public AuthenticationRequest m_request;

        public SubscriberUserControl()
        {
            InitializeComponent();
            this.Unloaded += new RoutedEventHandler(SubscriberUserControl_Unloaded);
            this.DataContext = new Subscribers(10);
        }

        void SubscriberUserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            (this.DataContext as Subscribers).ProcessPropertyChange();
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dg = new System.Windows.Forms.OpenFileDialog();
            dg.DefaultExt = ".xml";

            XmlRootAttribute xmlRootAttribute = new XmlRootAttribute("AuthenticationRequest");
            XmlSerializer serializer = new XmlSerializer(typeof(AuthenticationRequest), xmlRootAttribute);
            using (XmlReader reader = XmlReader.Create(dg.FileName))
                m_request = (AuthenticationRequest)serializer.Deserialize(reader);

            m_idField.Text = m_request.ID.ToString();
            m_acronymField.Text = m_request.Acronym;
            m_nameField.Text = m_request.Name;
            m_sharedSecretField.Text = m_request.SharedSecret;
            m_authKeyField.Text = m_request.AuthKey;
            m_keyField.Text = m_request.Key;
            m_ivField.Text = m_request.IV;
            m_validIpAddressesField.Text = m_request.ValidIPAddresses;
        }
    }
}
