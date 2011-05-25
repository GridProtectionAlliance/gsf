//******************************************************************************************************
//  SubscriberUserControl.xaml.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/19/2011 - Magdiel Lorenzo
//       Generated original version of source code.
//  05/19/2011 - Mehulbhai P Thakkar
//       Added code regions, comments etc.
//  05/25/2011 - J. Ritchie Carroll
//       Attached to before save event so class could import new keyIV's into local crypto cache.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TimeSeriesFramework.UI.DataModels;
using TimeSeriesFramework.UI.ViewModels;
using TVA;
using TVA.Security.Cryptography;

namespace TimeSeriesFramework.UI.UserControls
{
    /// <summary>
    /// Interaction logic for SubscriberUserControl.xaml
    /// </summary>
    public partial class SubscriberUserControl : UserControl
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates an instance of <see cref="SubscriberUserControl"/>.
        /// </summary>
        public SubscriberUserControl()
        {
            InitializeComponent();
            this.Loaded += SubscriberUserControl_Loaded;
            this.Unloaded += SubscriberUserControl_Unloaded;
            this.DataContext = new Subscribers(10);
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Handles loaded event for <see cref="SubscriberUserControl"/>.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void SubscriberUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Attach to load/before save event so class can load/add crypto keys from/to local keyIV cache
            (this.DataContext as Subscribers).PropertyChanged += SubscriberUserControl_PropertyChanged;
            (this.DataContext as Subscribers).BeforeSave += SubscriberUserControl_BeforeSave;
            LoadCurrentKeyIV();
        }

        /// <summary>
        /// Handles unloaded event for <see cref="SubscriberUserControl"/>.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void SubscriberUserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            (this.DataContext as Subscribers).ProcessPropertyChange();
            (this.DataContext as Subscribers).BeforeSave -= SubscriberUserControl_BeforeSave;
            (this.DataContext as Subscribers).PropertyChanged -= SubscriberUserControl_PropertyChanged;
        }

        /// <summary>
        /// Handles property changed event for <see cref="SubscriberUserControl"/>.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Property changed event arguments.</param>
        private void SubscriberUserControl_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Update information when current item changes...
            if (string.Compare(e.PropertyName, "CurrentItem", true) == 0)
                LoadCurrentKeyIV();
        }

        /// <summary>
        /// Handles before save event for <see cref="SubscriberUserControl"/>.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Cancel event arguments.</param>
        private void SubscriberUserControl_BeforeSave(object sender, CancelEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(m_sharedSecretField.Text) || string.IsNullOrWhiteSpace(m_keyField.Text) || string.IsNullOrWhiteSpace(m_ivField.Text))
                {
                    MessageBox.Show("Failed to import key and initialization vectors for associated shared secret - these fields cannot be blank.", "Crypto Key Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    e.Cancel = true;
                }
                else
                {
                    // Import key and initialization vector for subscriber into local crypto cache
                    Cipher.ImportKeyIV(m_sharedSecretField.Text.Trim(), 256, m_keyField.Text.Trim() + "|" + m_ivField.Text.Trim());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to import key and initialization vectors for associated shared secret due to exception: " + ex.Message, "Crypto Key Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Handles preview key down event on datagrid.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
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

        /// <summary>
        /// Handles click event on button.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AuthenticationRequest m_request = new AuthenticationRequest();
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();

            openFileDialog.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
            openFileDialog.DefaultExt = ".xml";
            openFileDialog.ShowDialog();

            m_request = Serialization.Deserialize<AuthenticationRequest>(File.ReadAllBytes(openFileDialog.FileName), SerializationFormat.Xml);

            Subscriber subscriber = new Subscriber()
            {
                NodeID = ((KeyValuePair<Guid, string>)ComboboxNode.SelectedItem).Key,
                Acronym = m_request.Acronym.ToUpper(),
                Name = m_request.Name,
                SharedSecret = m_request.SharedSecret,
                AuthKey = m_request.AuthenticationID,
                ValidIPAddresses = m_request.ValidIPAddresses
            };

            (this.DataContext as Subscribers).CurrentItem = subscriber;
            m_keyField.Text = m_request.Key;
            m_ivField.Text = m_request.IV;
        }

        /// <summary>
        /// Load current key and initialization vector for given shared secret.
        /// </summary>
        private void LoadCurrentKeyIV()
        {
            // After record has been loaded, load existing key and IV from crypto cache
            if (string.IsNullOrWhiteSpace(m_sharedSecretField.Text))
            {
                m_keyField.Text = "";
                m_ivField.Text = "";
            }
            else
            {
                string keyIV = Cipher.ExportKeyIV(m_sharedSecretField.Text, 256);
                string[] parts = keyIV.Split('|');

                m_keyField.Text = parts[0];
                m_ivField.Text = parts[1];
            }
        }

        #endregion
    }
}
