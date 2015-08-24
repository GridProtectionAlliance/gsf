using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;

//*******************************************************************************************************
//  TVA.Security.Application.Controls.ControlContainer.vb - Container control for user input controls
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
//  04/21/2008 - Pinal C. Patel
//       Original version of source code generated.
//  05/12/2008 - Pinal C. Patel
//       Modified Redirect() method to strip out query string variable used to force page lock.
//
//*******************************************************************************************************


namespace TVA.Security
{
	namespace Application
	{
		namespace Controls
		{
			
			
			/// <summary>
			/// Providers user interface (UI) that hosts user input controls.
			/// </summary>
			public class ControlContainer : CompositeControl
			{
				
				
				#region " Member Declaration "
				
				private Table m_table;
				private Dictionary<string, Control> m_controls;
				
				#endregion
				
				#region " Code Scope: Public "
				
				/// <summary>
				/// Creates an instance of the container control.
				/// </summary>
				/// <param name="securityProvider">Current security control.</param>
				/// <param name="activeControl">Link text of the default active control.</param>
				public ControlContainer(WebSecurityProvider securityProvider, string activeControl)
				{
					
					m_controls = new Dictionary<string, Control>(StringComparer.CurrentCultureIgnoreCase);
					
					// Add the default controls.
					AddControl("Login", new Login(this, securityProvider));
					AddControl("Change Password", new ChangePassword(this, securityProvider));
					
					// Set the default property values.
					this.Width = Unit.Parse("75px");
					this.HelpText = "For immediate assistance, please contact the Operations Duty Specialist at 423-751-1700.";
					this.CompanyText = "TENNESSEE VALLEY AUTHORITY";
					this.ActiveControl = activeControl;
					
				}
				
				/// <summary>
				/// Gets or sets the text to be displayed for help.
				/// </summary>
				/// <value></value>
				/// <returns>Text to be displayed for help.</returns>
				public string HelpText
				{
					get
					{
						object value = ViewState["HelpText"];
						if (value != null)
						{
							return value.ToString();
						}
						else
						{
							return string.Empty;
						}
					}
					set
					{
						ViewState["HelpText"] = value;
					}
				}
				
				/// <summary>
				/// Gets or sets the company specific information to be displayed.
				/// </summary>
				/// <value></value>
				/// <returns>Company specific information to be displayed.</returns>
				public string CompanyText
				{
					get
					{
						object value = ViewState["CompanyText"];
						if (value != null)
						{
							return value.ToString();
						}
						else
						{
							return string.Empty;
						}
					}
					set
					{
						ViewState["CompanyText"] = value;
					}
				}
				
				/// <summary>
				/// Gets or sets the any annoucement message to be displayed.
				/// </summary>
				/// <value></value>
				/// <returns>Annoucement message to be displayed.</returns>
				public string MessageText
				{
					get
					{
						object value = ViewState["MessageText"];
						if (value != null)
						{
							return value.ToString();
						}
						else
						{
							return string.Empty;
						}
					}
					set
					{
						ViewState["MessageText"] = value;
					}
				}
				
				/// <summary>
				/// Gets or sets the link text of the active control.
				/// </summary>
				/// <value></value>
				/// <returns>Link text of the active control.</returns>
				public string ActiveControl
				{
					get
					{
						object value = ViewState["ActiveControl"];
						if (value != null)
						{
							return value.ToString();
						}
						else
						{
							return string.Empty;
						}
					}
					set
					{
						if (string.IsNullOrEmpty(value) || m_controls.ContainsKey(value))
						{
							// Either null value is specified for the active control link text or valid link text value
							// is specified. Null value is a valid value because then no control is an active control.
							ViewState["ActiveControl"] = value;
						}
						else
						{
							throw (new ArgumentException("No such control."));
						}
					}
				}
				
				/// <summary>
				/// Adds the specified control.
				/// </summary>
				/// <param name="linkText">Link text for the control.</param>
				/// <param name="control">The control to be added.</param>
				public void AddControl(string linkText, Control control)
				{
					
					if (! m_controls.ContainsKey(linkText))
					{
						// A control doesn't already exist with the specified link text.
						m_controls.Add(linkText, control);
					}
					
				}
				
				/// <summary>
				/// Performs a redirect to the specified URL or to the current URL if none specified.
				/// </summary>
				/// <param name="url">URL to redirect to.</param>
				public void Redirect(string url)
				{
					
					if (! string.IsNullOrEmpty(url))
					{
						// Redirect to the specified URL.
						Page.Response.Redirect(url, false);
					}
					else
					{
						// No URL is specified, so redirect to the current URL. We will however remove the key that can
						// be used to force a web page lock-down from the query string if it is present before redirecting.
						// Doing so will allow the end-user's request for login or changing password to be processed, or
						// else the page would always remain in lock-down mode.
						if (Page.Request[WebSecurityProvider.LockModeKey] == null)
						{
							// No "LockDown" key is present in query string.
							Page.Response.Redirect(Page.Request.Url.PathAndQuery, false);
						}
						else
						{
							// "LockDown" key is present in the query string, so strip it out before redirect.
							string pattern = string.Format("[\\?&]({0}=[^&=]+)", WebSecurityProvider.LockModeKey);
							Page.Response.Redirect(Regex.Replace(Page.Request.Url.PathAndQuery, pattern, ""), false);
						}
					}
					
				}
				
				/// <summary>
				/// Causes the control with the specified link text to be the active control and re-renders this control.
				/// </summary>
				/// <param name="linkText">Link text of the control to be made the active control.</param>
				public void UpdateActiveControl(string linkText)
				{
					
					this.ActiveControl = linkText;
					if (m_table != null)
					{
						// Instead of just updating the cells for active control, we will re-render this control again
						// so that the controls (this control and the active control) are created in the right order.
						// This is required for events inside of the active control to be handled correctly on postbacks
						// as events are handled based on the UniqueID of the control causing the postback and this
						// UniqueID is dependent on the order in which the controls are created/rendered.
						// Refer: http://msdn2.microsoft.com/en-us/library/aa720472(VS.71).aspx
						CreateChildControls();
					}
					
					// When switching the active control, we reset the message text that might be present.
					UpdateMessageText(string.Empty, MessageType.Error);
					
				}
				
				/// <summary>
				/// Causes the message text to be set to the specified text and updates the control to reflect the change.
				/// </summary>
				/// <param name="message">Text to set as the message text.</param>
				/// <param name="type">Indicates the type of message.</param>
				public void UpdateMessageText(string message, MessageType type)
				{
					
					this.MessageText = message;
					if (m_table != null)
					{
						// Control has been rendered, so update the message text in the cell designated for message text.
						m_table.Rows[4].Cells[0].Text = message;
						m_table.Rows[4].Cells[0].CssClass = Type.ToString() + "Message";
					}
					
				}
				
				#region " Shared "
				
				/// <summary>
				/// Creates a server table.
				/// </summary>
				/// <param name="rowCount">Number of rows for the table.</param>
				/// <param name="columnCount">Number of columns for the table.</param>
				/// <returns>A server table.</returns>
				public static Table NewTable(int rowCount, int columnCount)
				{
					
					Table table = new Table();
					for (int i = 1; i <= rowCount; i++)
					{
						TableRow row = new TableRow();
						for (int j = 1; j <= columnCount; j++)
						{
							row.Cells.Add(new TableCell());
						}
						table.Rows.Add(row);
					}
					
					return table;
					
				}
				
				#endregion
				
				#endregion
				
				#region " Code Scope: Protected "
				
				/// <summary>
				/// Performs layout of the control.
				/// </summary>
				protected override void CreateChildControls()
				{
					
					// Apply style-sheet.
					string includeTemplate = "<link rel=\'stylesheet\' text=\'text/css\' href=\'{0}\' />";
					string includeLocation = Page.ClientScript.GetWebResourceUrl(typeof(ControlContainer), "TVA.Security.Application.Controls.StyleSheet.css");
					Page.Header.Controls.Add(new LiteralControl(string.Format(includeTemplate, includeLocation)));
					
					// ----------------------------------------------------------------------------------
					// |                                                                                |
					// | Header Image                                                                   |
					// |                                                                                |
					// ----------------------------------------------------------------------------------
					// | :: Control Link  :: Control Link                                               |
					// ----------------------------------------------------------------------------------
					// |                             Active Control Link Text                           |
					// ----------------------------------------------------------------------------------
					// |                                                                                |
					// |                                                                                |
					// |                                                                                |
					// |                              Active Control Content                            |
					// |                                                                                |
					// |                                                                                |
					// |                                                                                |
					// ----------------------------------------------------------------------------------
					// | Message Text                                                                   |
					// ----------------------------------------------------------------------------------
					// | Help Text                                                                      |
					// ----------------------------------------------------------------------------------
					// | Company Text                                                               | ? |
					// ----------------------------------------------------------------------------------
					
					// Layout the control.
					m_table = NewTable(7, 1);
					m_table.CssClass = "Container";
					
					// Row #1
					Image headerImage = new Image();
					headerImage.ImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(ControlContainer), "TVA.Security.Application.Controls.LogoExternal.jpg");
					m_table.Rows[0].Cells[0].CssClass = "HeaderSection";
					m_table.Rows[0].Cells[0].Controls.Add(headerImage);
					
					// Row #2, #3, #4
					Table linksTable = NewTable(1, m_controls.Count);
					for (int i = 0; i <= m_controls.Count - 1; i++)
					{
						Label text = new Label();
						text.Text = "&nbsp;&nbsp;::&nbsp;";
						text.Font.Bold = true;
						LinkButton link = new LinkButton();
						link.Text = m_controls.Keys(i);
						link.CausesValidation = false;
						link.Click += new System.EventHandler(Link_Click);
						linksTable.Rows[0].Cells[i].Controls.Add(text);
						linksTable.Rows[0].Cells[i].Controls.Add(link);
						
						if (string.Compare(ActiveControl, m_controls.Keys(i), true) == 0)
						{
							// Active control caption
							m_table.Rows[2].Cells[0].Text = m_controls.Keys(i);
							// Active control content
							m_table.Rows[3].Cells[0].Controls.Add(m_controls.Values(i));
						}
					}
					m_table.Rows[1].Cells[0].CssClass = "ControlLinks";
					m_table.Rows[2].Cells[0].CssClass = "ActiveControlCaption";
					m_table.Rows[3].Cells[0].CssClass = "ActiveControlContent";
					m_table.Rows[1].Cells[0].Controls.Add(linksTable);
					
					// Row #5
					m_table.Rows[4].Cells[0].Text = MessageText;
					m_table.Rows[4].Cells[0].CssClass = "ErrorMessage";
					
					// Row #6
					m_table.Rows[5].Cells[0].Text = HelpText;
					m_table.Rows[5].Cells[0].CssClass = "HelpText";
					
					// Row #7
					Table footerTable = NewTable(1, 2);
					string imageLinkText = "<a href=\"{0}\" target=\"_blank\"><img src=\"{1}\" style=\"height:16px;width:16px;border-width:0px;\" /></a>";
					imageLinkText = string.Format(imageLinkText, Page.ClientScript.GetWebResourceUrl(typeof(ControlContainer), "TVA.Security.Application.Controls.Help.pdf"), Page.ClientScript.GetWebResourceUrl(typeof(ControlContainer), "TVA.Security.Application.Controls.Help.gif"));
					footerTable.Width = Unit.Parse("100%");
					footerTable.Rows[0].Cells[0].Text = "&nbsp;&nbsp;" + CompanyText;
					footerTable.Rows[0].Cells[0].Width = Unit.Parse("95%");
					footerTable.Rows[0].Cells[1].Text = imageLinkText;
					m_table.Rows[6].Cells[0].CssClass = "FooterSection";
					m_table.Rows[6].Cells[0].Controls.Add(footerTable);
					
					this.Controls.Clear();
					this.Controls.Add(m_table);
					
				}
				
				#endregion
				
				#region " Code Scope: Private "
				
				private void Link_Click(object sender, System.EventArgs e)
				{
					
					// Set the active control
					UpdateActiveControl(((LinkButton) sender).Text);
					
				}
				
				#endregion
				
			}
			
			
		}
	}
	
}
