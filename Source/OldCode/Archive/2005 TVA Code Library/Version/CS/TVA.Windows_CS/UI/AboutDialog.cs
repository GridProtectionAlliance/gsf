using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
//using System.Reflection.Assembly;
//using TVA.Assembly;

//*******************************************************************************************************
//  TVA.Windows.UI.AboutDialog.vb - Standard TVA About Dialog
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  05/26/2006 - Pinal C. Patel
//       Original version of source code generated
//
//*******************************************************************************************************


namespace TVA.Windows
{
	namespace UI
	{
		
		public partial class AboutDialog
		{
			
			#region " Private Declarations "
			
			private string m_url;
			private List<Assembly> m_assemblies;
			
			#endregion
			
			#region " Public Methods "
			
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
				Assembly thisAssembly = new Assembly(System.Reflection.Assembly.GetExecutingAssembly());
				SetCompanyUrl("http://www.tva.gov");
				SetCompanyLogo(thisAssembly.GetEmbeddedResource("TVA.Windows.UI.TVALogo.bmp"));
				SetCompanyDisclaimer(thisAssembly.GetEmbeddedResource("TVA.Windows.UI.TVADisclaimer.txt"));
				
			}
			
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
					PictureBoxLogo.Image = new System.Drawing.Bitmap(logoStream);
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
				{
					RichTextBoxDisclaimer.Text = new StreamReader(disclaimerStream).ReadToEnd();
				}
				
			}
			
			#endregion
			
			#region " Private Methods "
			
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
			
			#endregion
			
			#region " Form Events "
			
			private void AboutDialog_Load(System.Object sender, System.EventArgs e)
			{
				
				this.Text = string.Format(this.Text, Assembly.EntryAssembly.Title());
				if ((this.Owneris ))null;);
				{
					Size originalSize = this.Size;
					
					this.Font = this.Owner.Font;
					this.Size = originalSize;
				}
				
				// Show information about the application that opened this dialog box.
				ListViewApplicationInfo.Items.Clear();
				AddListViewItem(ListViewApplicationInfo, "Friendly Name", AppDomain.CurrentDomain.FriendlyName);
				AddListViewItem(ListViewApplicationInfo, "Name", TVA.Assembly.EntryAssembly.Name);
				AddListViewItem(ListViewApplicationInfo, "Version", TVA.Assembly.EntryAssembly.Version.ToString());
				AddListViewItem(ListViewApplicationInfo, "Build Date", TVA.Assembly.EntryAssembly.BuildDate.ToString());
				AddListViewItem(ListViewApplicationInfo, "Location", TVA.Assembly.EntryAssembly.Location);
				AddListViewItem(ListViewApplicationInfo, "Title", TVA.Assembly.EntryAssembly.Title);
				AddListViewItem(ListViewApplicationInfo, "Description", TVA.Assembly.EntryAssembly.Description);
				AddListViewItem(ListViewApplicationInfo, "Company", TVA.Assembly.EntryAssembly.Company);
				AddListViewItem(ListViewApplicationInfo, "Product", TVA.Assembly.EntryAssembly.Product);
				AddListViewItem(ListViewApplicationInfo, "Copyright", TVA.Assembly.EntryAssembly.Copyright);
				AddListViewItem(ListViewApplicationInfo, "Trademark", TVA.Assembly.EntryAssembly.Trademark);
				
				// Query all the assemblies used by the calling application.
				if (m_assemblies == null)
				{
					m_assemblies = new List<Assembly>();
					foreach (System.Reflection.Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
					{
						try
						{
							if (File.Exists(asm.Location))
							{
								// Discard assemblies that are embedded into the application.
								m_assemblies.Add(new Assembly(asm));
							}
						}
						catch (Exception)
						{
							// Accessing Location property on assemblies that are built dynamically will result in an
							// exception since such assemblies only exist in-memory, so we'll ignore such assemblies.
						}
					}
				}
				
				// Show a list of all the queried assemblies.
				ComboBoxAssemblies.DisplayMember = "Name";
				ComboBoxAssemblies.DataSource = m_assemblies;
				if (ComboBoxAssemblies.Items.Count> 0)
				{
					ComboBoxAssemblies.SelectedIndex = 0;
				}
				
			}
			
			private void PictureBoxLogo_Click(System.Object sender, System.EventArgs e)
			{
				
				if (! string.IsNullOrEmpty(m_url))
				{
					Process.Start(m_url);
				}
				
			}
			
			private void RichTextBoxDisclaimer_LinkClicked(object sender, System.Windows.Forms.LinkClickedEventArgs e)
			{
				
				if (! string.IsNullOrEmpty(e.LinkText))
				{
					Process.Start(e.LinkText);
				}
				
			}
			
			private void ComboBoxAssemblies_SelectedIndexChanged(System.Object sender, System.EventArgs e)
			{
				
				if ((ComboBoxAssemblies.SelectedItemis )null);
				{
					ListViewAssemblyInfo.Items.Clear();
					System.Collections.Specialized.NameValueCollection attributes = ((Assembly) ComboBoxAssemblies.SelectedItem).GetAttributes();
					foreach (string key in attributes)
					{
						AddListViewItem(ListViewAssemblyInfo, key, attributes[key]);
					}
				}
				
			}
			
			private void ButtonOK_Click(System.Object sender, System.EventArgs e)
			{
				
				this.Close();
				this.Dispose();
				
			}
			
			#endregion
			
		}
		
	}
}
