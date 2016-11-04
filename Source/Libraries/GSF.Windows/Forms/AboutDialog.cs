//******************************************************************************************************
//  AboutDialog.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/26/2006 - Pinal C. Patel
//       Original version of source code generated.
//  10/01/2008 - J. Ritchie Carroll
//       Converted to C#.
//  03/20/2009 - Pinal C. Patel
//       Edited code comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using GSF.Reflection;

namespace GSF.Windows.Forms
{
    /// <summary>
    /// Represents a common about dialog box.
    /// </summary>
    public partial class AboutDialog : Form
    {
        #region [ Members ]

        // Fields
        private string m_url;
        private List<AssemblyInfo> m_assemblies;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="AboutDialog"/> class.
        /// </summary>
        public AboutDialog()
        {
            InitializeComponent();

            AssemblyInfo executingAssembly = AssemblyInfo.ExecutingAssembly;
            SetCompanyUrl("http://www.gridprotectionalliance.org/");
            SetCompanyLogo(executingAssembly.GetEmbeddedResource("GSF.Windows.Forms.Logo.bmp"));
            SetCompanyDisclaimer(executingAssembly.GetEmbeddedResource("GSF.Windows.Forms.Disclaimer.txt"));
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Conceals the tab where disclaimer text is displayed.
        /// </summary>
        /// <remarks></remarks>
        public void HideDisclaimerTab()
        {
            TabControlInformation.TabPages.Remove(TabPageDisclaimer);
        }

        /// <summary>
        /// Conceals the tab where application information is displayed.
        /// </summary>
        /// <remarks></remarks>
        public void HideApplicationTab()
        {
            TabControlInformation.TabPages.Remove(TabPageApplication);
        }

        /// <summary>
        /// Conceals the tab where assemblies and their information is displayed.
        /// </summary>
        /// <remarks></remarks>
        public void HideAssembliesTab()
        {
            TabControlInformation.TabPages.Remove(TabPageAssemblies);
        }

        /// <summary>
        /// Sets the URL that will be opened when the logo is clicked.
        /// </summary>
        /// <param name="url">URL of the company's home page.</param>
        /// <remarks></remarks>
        public void SetCompanyUrl(string url)
        {
            m_url = url;
        }

        /// <summary>
        /// Sets the logo that is to be displayed in the About Dialog.
        /// </summary>
        /// <param name="logoFile">Location of the logo file.</param>
        /// <remarks></remarks>
        public void SetCompanyLogo(string logoFile)
        {
            if (File.Exists(logoFile))
            {
                // Logo file exists so load it in memory.
                StreamReader logoReader = new StreamReader(logoFile);
                SetCompanyLogo(logoReader.BaseStream);
                logoReader.Close(); // Release all locks on the file.
            }
            else
            {
                MessageBox.Show("The logo file \'" + logoFile + "\' does not exist.", "Missing File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Sets the logo that is to be displayed in the About Dialog.
        /// </summary>
        /// <param name="logoStream">System.IO.Stream of the logo.</param>
        /// <remarks></remarks>
        public void SetCompanyLogo(Stream logoStream)
        {
            if (logoStream != null)
            {
                using (Image img = PictureBoxLogo.Image)
                {
                    PictureBoxLogo.Image = new Bitmap(logoStream);
                }
            }
        }

        /// <summary>
        /// Sets the disclaimer text that is to be displayed in the About Dialog.
        /// </summary>
        /// <param name="disclaimerFile">Location of the file that contains the disclaimer text.</param>
        /// <remarks></remarks>
        public void SetCompanyDisclaimer(string disclaimerFile)
        {
            if (File.Exists(disclaimerFile))
            {
                // Disclaimer file exists so load it in memory.
                StreamReader disclaimerReader = new StreamReader(disclaimerFile, Encoding.Default);
                SetCompanyDisclaimer(disclaimerReader.BaseStream);
                disclaimerReader.Close(); // Release all locks on the file.
            }
            else
            {
                MessageBox.Show("The disclaimer file \'" + disclaimerFile + "\' does not exist.", "Missing File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Sets the disclaimer text that is to be displayed in the About Dialog.
        /// </summary>
        /// <param name="disclaimerStream">System.IO.Stream of the disclaimer text.</param>
        /// <remarks></remarks>
        public void SetCompanyDisclaimer(Stream disclaimerStream)
        {
            if (disclaimerStream != null)
                RichTextBoxDisclaimer.Text = new StreamReader(disclaimerStream, Encoding.Default).ReadToEnd();
        }

        private void AddListViewItem(ListView listView, string text, params string[] subitems)
        {
            //Add a new ListViewItem with the specified data to the specified ListView.
            ListViewItem item = new ListViewItem();
            item.Text = text;

            foreach (string subitem in subitems)
            {
                item.SubItems.Add(subitem);
            }

            listView.Items.Add(item);
        }

        private void _AboutDialog_Load(object sender, EventArgs e)
        {
            Text = string.Format(this.Text, AssemblyInfo.EntryAssembly.Title);

            if (Owner != null)
            {
                Size originalSize = this.Size;
                Font = Owner.Font;
                Size = originalSize;
            }

            // Show information about the application that opened this dialog box.
            ListViewApplicationInfo.Items.Clear();

            AddListViewItem(ListViewApplicationInfo, "Friendly Name", AppDomain.CurrentDomain.FriendlyName);
            AddListViewItem(ListViewApplicationInfo, "Name", AssemblyInfo.EntryAssembly.Name);
            AddListViewItem(ListViewApplicationInfo, "Version", AssemblyInfo.EntryAssembly.Version.ToString());
            AddListViewItem(ListViewApplicationInfo, "Build Date", AssemblyInfo.EntryAssembly.BuildDate.ToString());
            AddListViewItem(ListViewApplicationInfo, "Location", AssemblyInfo.EntryAssembly.Location);
            AddListViewItem(ListViewApplicationInfo, "Title", AssemblyInfo.EntryAssembly.Title);
            AddListViewItem(ListViewApplicationInfo, "Description", AssemblyInfo.EntryAssembly.Description);
            AddListViewItem(ListViewApplicationInfo, "Company", AssemblyInfo.EntryAssembly.Company);
            AddListViewItem(ListViewApplicationInfo, "Product", AssemblyInfo.EntryAssembly.Product);
            AddListViewItem(ListViewApplicationInfo, "Copyright", AssemblyInfo.EntryAssembly.Copyright);

            // None of the application assemblies contain trademark information...
            //AddListViewItem(ListViewApplicationInfo, "Trademark", AssemblyInfo.EntryAssembly.Trademark);

            // Query all the assemblies used by the calling application.
            if ((object)m_assemblies == null)
            {
                m_assemblies = new List<AssemblyInfo>();
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        // Ignore assemblies that are not physically accessible from the file system.
                        if (File.Exists(asm.Location))
                            m_assemblies.Add(new AssemblyInfo(asm));
                    }
                    catch
                    {
                        // Accessing Location property on assemblies that are built dynamically will result in an
                        // exception since such assemblies only exist in-memory, so we'll ignore such assemblies.
                    }
                }
            }

            // Show a list of all the queried assemblies.
            ComboBoxAssemblies.DisplayMember = "Name";
            ComboBoxAssemblies.DataSource = m_assemblies;

            if (ComboBoxAssemblies.Items.Count > 0)
                ComboBoxAssemblies.SelectedIndex = 0;
        }

        private void PictureBoxLogo_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(m_url))
                Process.Start(m_url);
        }

        private void RichTextBoxDisclaimer_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.LinkText))
                Process.Start(e.LinkText);
        }

        private void ComboBoxAssemblies_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ComboBoxAssemblies.SelectedItem != null)
            {
                ListViewAssemblyInfo.Items.Clear();

                NameValueCollection attributes = ((AssemblyInfo)ComboBoxAssemblies.SelectedItem).GetAttributes();

                foreach (string key in attributes)
                {
                    AddListViewItem(ListViewAssemblyInfo, key, attributes[key]);
                }
            }
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            Close();
        }

        #endregion
    }
}
