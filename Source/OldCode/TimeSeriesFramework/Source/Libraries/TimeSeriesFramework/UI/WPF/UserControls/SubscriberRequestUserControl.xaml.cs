//******************************************************************************************************
//  SubscriberRequestUserControl.xaml.cs - Gbtc
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
//  05/18/2011 - Magdiel Lorenzo
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Serialization;
using TVA.Data;
using TVA.Security.Cryptography;

namespace TimeSeriesFramework.UI.UserControls
{
    /// <summary>
    /// Interaction logic for SubscriberRequestUserControl.xaml
    /// </summary>
    public partial class SubscriberRequestUserControl : UserControl
    {

        #region [ Constructor ]

        /// <summary>
        /// Creates an instance of <see cref="SubscriberRequestUserControl"/> class.
        /// </summary>     
        public SubscriberRequestUserControl()
        {
            InitializeComponent();
            this.Loaded += SubscriberRequestUserControl_Loaded;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Populates screen fields on load.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Arguments of the event.</param>
        void SubscriberRequestUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            AdoDataConnection database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory);
            DataRow row = database.Connection.RetrieveRow(database.AdapterType, "SELECT Company.Acronym, Company.Name FROM Company, Node WHERE Company.ID = Node.CompanyID AND Node.ID = @id", database.CurrentNodeID());
            m_acronymField.Text = row.Field<string>("Acronym");
            m_nameField.Text = row.Field<string>("Name");
            byte[] buffer = new byte[4];
            TVA.Security.Cryptography.Random.GetBytes(buffer);
            m_sharedSecretField.Text = Convert.ToBase64String(buffer);
            AesManaged sa = new AesManaged();
            sa.GenerateKey();
            m_authKeyField.Text = Convert.ToBase64String(sa.Key);
            IEnumerable<IPAddress> addresses = Dns.GetHostAddresses(Dns.GetHostName());
            m_validIpAddressesField.Text = TVA.Collections.CollectionExtensions.ToDelimitedString(addresses, ';');
        }

        /// <summary>
        /// Click Event for Export button.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Arguments of the event</param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if ((m_keyField.Text == "") && (m_ivField.Text == ""))
            {
                string key_iv = Cipher.ExportKeyIV(m_sharedSecretField.Text, 256);
                string[] keyAndIv = key_iv.Split('|');
                m_keyField.Text = keyAndIv[0];
                m_ivField.Text = keyAndIv[1];
            }

            string filename;
            System.Windows.Forms.SaveFileDialog dg = new System.Windows.Forms.SaveFileDialog();
            dg.DefaultExt = ".xml";
            dg.ShowDialog();
            filename = dg.FileName;
            AuthenticationRequest request = new AuthenticationRequest();
            request.Acronym = m_acronymField.Text;
            request.Name = m_nameField.Text;
            request.SharedSecret = m_sharedSecretField.Text;
            request.AuthKey = m_authKeyField.Text;
            request.Key = m_keyField.Text;
            request.IV = m_ivField.Text;
            request.ValidIPAddresses = m_validIpAddressesField.Text;
            

            XmlSerializer serializer = new XmlSerializer(typeof(AuthenticationRequest));
            using (XmlWriter writer = XmlWriter.Create(filename))
                serializer.Serialize(writer, request);
        }

        /// <summary>
        /// Click Event for Generate button.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Arguments of the event.</param>
        private void m_generateButton_Click(object sender, RoutedEventArgs e)
        {
            string key_iv = Cipher.ExportKeyIV(m_sharedSecretField.Text, 256);
            string[] keyAndIv = key_iv.Split('|');
            m_keyField.Text = keyAndIv[0];
            m_ivField.Text = keyAndIv[1];
        }

        #endregion
    }
}
