//*******************************************************************************************************
//  AboutDialog.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  05/26/2006 - Pinal C. Patel
//       Generated original version of source code.
//  10/01/2008 - James R Carroll
//       Converted to C#.
//
//*******************************************************************************************************

using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using TVA.Reflection;

namespace TVA.Windows.Forms
{
    public partial class AboutDialog
    {
        #region [ Members ]

        // Fields
        private string m_url;
        private List<AssemblyInfo> m_assemblies;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a default instance of the standard TVA About Dialog.
        /// </summary>
        /// <remarks></remarks>
        public AboutDialog()
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();

            // Add any initialization after the InitializeComponent() call.
            // Set the defaults.
            AssemblyInfo executingAssembly = AssemblyInfo.ExecutingAssembly;
            SetCompanyUrl("http://www.tva.gov");
            SetCompanyLogo(executingAssembly.GetEmbeddedResource("TVA.Windows.Forms.TVALogo.bmp"));
            SetCompanyDisclaimer(executingAssembly.GetEmbeddedResource("TVA.Windows.Forms.TVADisclaimer.txt"));
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
                PictureBoxLogo.Image = new System.Drawing.Bitmap(logoStream);
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
                StreamReader disclaimerReader = new StreamReader(disclaimerFile);
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
                RichTextBoxDisclaimer.Text = new StreamReader(disclaimerStream).ReadToEnd();
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

        private void AboutDialog_Load(System.Object sender, System.EventArgs e)
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
            AddListViewItem(ListViewApplicationInfo, "Trademark", AssemblyInfo.EntryAssembly.Trademark);

            // Query all the assemblies used by the calling application.
            if (m_assemblies == null)
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

        private void PictureBoxLogo_Click(System.Object sender, System.EventArgs e)
        {
            if (!string.IsNullOrEmpty(m_url))
                Process.Start(m_url);
        }

        private void RichTextBoxDisclaimer_LinkClicked(object sender, System.Windows.Forms.LinkClickedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.LinkText))
                Process.Start(e.LinkText);
        }

        private void ComboBoxAssemblies_SelectedIndexChanged(System.Object sender, System.EventArgs e)
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

        private void ButtonOK_Click(System.Object sender, System.EventArgs e)
        {
            Close();
            Dispose();
        }

        #endregion
   }
}