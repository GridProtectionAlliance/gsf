using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Text;
using System.Web.UI.WebControls;
//using TVA.Security.Cryptography.Common;

//*******************************************************************************************************
//  TVA.Security.Application.Controls.Login.vb - Control for logging in to a web site
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
//  04/22/2008 - Pinal C. Patel
//       Original version of source code generated.
//
//*******************************************************************************************************


namespace TVA.Security
{
	namespace Application
	{
		namespace Controls
		{
			
			
			/// <summary>
			/// Provides user interface (UI) for logging in to a secure Web Site.
			/// </summary>
			public class Login : CompositeControl
			{
				
				
				#region " Member Declaration "
				
				private TextBox m_usernameTextBox;
				private TextBox m_passwordTextBox;
				private ControlContainer m_container;
				private WebSecurityProvider m_securityProvider;
				
				#endregion
				
				#region " Code Scope: Public "
				
				/// <summary>
				/// Session key used to cross-check whether or not a user account is in the "new pin" mode before a user
				/// is allowed to create a new pin under RSA authentication mode.
				/// </summary>
				public const string NewPinVerify = "NewPinMode";
				
				/// <summary>
				/// Creates an instance of the login control.
				/// </summary>
				/// <param name="container">Control containing this control.</param>
				/// <param name="securityProvider">Current security control.</param>
				public Login(ControlContainer container, WebSecurityProvider securityProvider)
				{
					
					m_container = container;
					m_securityProvider = securityProvider;
					
				}
				
				/// <summary>
				/// Gets or sets the container control for this control.
				/// </summary>
				/// <value></value>
				/// <returns>Container control for this control.</returns>
				public ControlContainer Container
				{
					get
					{
						return m_container;
					}
					set
					{
						if (value != null)
						{
							m_container = value;
						}
						else
						{
							throw (new ArgumentNullException("Container"));
						}
					}
				}
				
				#endregion
				
				#region " Code Scope: Protected "
				
				/// <summary>
				/// Performs layout of the control.
				/// </summary>
				protected override void CreateChildControls()
				{
					
					// -----------------------------------------
					// |              | ---------------------- |
					// | Username:*   | |                    | |
					// |              | ---------------------- |
					// -----------------------------------------
					// |              | ---------------------- |
					// | Password:*   | |                    | |
					// |              | ---------------------- |
					// -----------------------------------------
					// |              |         -------------- |
					// |              |         |   Submit   | |
					// |              |         -------------- |
					// -----------------------------------------
					
					// Layout the control.
					Table container = ControlContainer.NewTable(3, 2);
					
					// Row #1
					m_usernameTextBox = new TextBox();
					m_usernameTextBox.ID = "UsernameTextBox";
					m_usernameTextBox.Width = Unit.Parse("150px");
					RequiredFieldValidator usernameValidator = new RequiredFieldValidator();
					usernameValidator.Display = ValidatorDisplay.None;
					usernameValidator.ErrorMessage = "Username is required.";
					usernameValidator.ControlToValidate = m_usernameTextBox.ID;
					container.Rows[0].Cells[0].Text = "Username:*&nbsp;";
					container.Rows[0].Cells[0].HorizontalAlign = HorizontalAlign.Right;
					container.Rows[0].Cells[1].Controls.Add(m_usernameTextBox);
					container.Rows[0].Cells[1].Controls.Add(usernameValidator);
					
					// Row #2
					m_passwordTextBox = new TextBox();
					m_passwordTextBox.ID = "PasswordTextBox";
					m_passwordTextBox.Width = Unit.Parse("150px");
					m_passwordTextBox.TextMode = TextBoxMode.Password;
					RequiredFieldValidator passwordValidator = new RequiredFieldValidator();
					passwordValidator.Display = ValidatorDisplay.None;
					passwordValidator.ErrorMessage = "Password is required.";
					passwordValidator.ControlToValidate = m_passwordTextBox.ID;
					container.Rows[1].Cells[0].Text = "Password:*&nbsp;";
					container.Rows[1].Cells[0].HorizontalAlign = HorizontalAlign.Right;
					container.Rows[1].Cells[1].Controls.Add(m_passwordTextBox);
					container.Rows[1].Cells[1].Controls.Add(passwordValidator);
					
					// Row #3
					Button submitButton = new Button();
					submitButton.Text = "Submit";
					submitButton.Click += new System.EventHandler(SubmitButton_Click);
					ValidationSummary validationSummary = new ValidationSummary();
					validationSummary.ShowSummary = false;
					validationSummary.ShowMessageBox = true;
					container.Rows[2].Cells[0].Controls.Add(validationSummary);
					container.Rows[2].Cells[1].HorizontalAlign = HorizontalAlign.Right;
					container.Rows[2].Cells[1].Controls.Add(submitButton);
					
					this.Controls.Clear();
					this.Controls.Add(container);
					
					// Setup client-side scripts.
					Page.SetFocus(m_usernameTextBox);
					System.Text.StringBuilder with_1 = new StringBuilder();
					with_1.Append("if (typeof(Page_ClientValidate) == \'function\') {");
					with_1.Append("if (Page_ClientValidate() == false) { return false; }}");
					with_1.Append("this.disabled = true;");
					with_1.AppendFormat("document.all.{0}.disabled = true;", submitButton.ClientID);
					with_1.AppendFormat("{0};", Page.ClientScript.GetPostBackEventReference(submitButton, null));
					
					submitButton.OnClientClick = with_1.ToString();
					
					if (m_securityProvider.AuthenticationMode == AuthenticationMode.RSA)
					{
						// If RSA authentication is used, we'll provided a hint about the username and password, as there
						// may be some confussion as to what makes-up the password when a web page is secured using RSA.
						System.Text.StringBuilder with_2 = new StringBuilder();
						with_2.Append("Note: This web page is secured using RSA Security. The Username is the ID that was ");
						with_2.Append("provided to you when you received your RSA SecurID key, and the Password consists ");
						with_2.Append("of your pin followed by the token currently being displayed on your RSA SecurID key.");
						
						m_container.UpdateMessageText(with_2.ToString(), MessageType.Information);
					}
					
				}
				
				#endregion
				
				#region " Code Scope: Private "
				
				private void SubmitButton_Click(object sender, System.EventArgs e)
				{
					
					User user = null;
					try
					{
						if (m_securityProvider != null)
						{
							user = new User(m_usernameTextBox.Text, m_passwordTextBox.Text, m_securityProvider.ApplicationName, m_securityProvider.Server, m_securityProvider.AuthenticationMode, true);
							
							if (user.PasswordChangeDateTime > DateTime.MinValue && user.PasswordChangeDateTime <= DateTime.Now)
							{
								// It's time for the user to change password/create pin.
								if (m_securityProvider.AuthenticationMode == AuthenticationMode.RSA)
								{
									// Under RSA authentication, we don't allow a new pin to be created on-demand as it is
									// not supported. In other words, user cannot navigate to the Change Password control by
									// clicking on the link directly, instead the user must redirected from here after ensuring
									// that the user account is actually in the "new pin" mode (done by Authenticate()).
									Page.Session[NewPinVerify] = true;
								}
								
								m_container.UpdateActiveControl("Change Password");
								return;
							}
							
							if (user.IsAuthenticated)
							{
								// User's credentials have been verified, so we'll save them for use by the security control.
								// We'll save both the username and password in 2 places:
								// 1) Session for use by security control.
								// 2) Cookie for single-signon, but not when RSA security is employed.
								string username = TVA.Security.Cryptography.Common.Encrypt(m_usernameTextBox.Text);
								string password = TVA.Security.Cryptography.Common.Encrypt(m_passwordTextBox.Text);
								Page.Session.Add(WebSecurityProvider.UsernameKey, username);
								Page.Session.Add(WebSecurityProvider.PasswordKey, password);
								if (m_securityProvider.AuthenticationMode != AuthenticationMode.RSA)
								{
									Page.Response.Cookies[WebSecurityProvider.CredentialCookie][WebSecurityProvider.UsernameKey] = username;
									Page.Response.Cookies[WebSecurityProvider.CredentialCookie][WebSecurityProvider.PasswordKey] = password;
								}
								
								m_container.Redirect(string.Empty); // Refresh.
							}
							else
							{
								if (! user.IsDefined)
								{
									// Account doesn't exist for user.
									Page.SetFocus(m_usernameTextBox);
									m_container.UpdateMessageText("Login failed. No such account.", MessageType.Error);
								}
								else if (user.IsLockedOut)
								{
									// User's account has been locked-out.
									Page.SetFocus(m_usernameTextBox);
									m_container.UpdateMessageText("Login failed. Account is locked.", MessageType.Error);
								}
								else
								{
									// Failed to verify the credentials.
									Page.SetFocus(m_passwordTextBox);
									m_container.UpdateMessageText("Login failed. Credential verification was unsuccessful.", MessageType.Error);
								}
							}
						}
					}
					catch (Exception ex)
					{
						// Show the encountered exception to the user.
						m_container.UpdateMessageText(string.Format("ERROR: {0}", ex.Message), MessageType.Error);
						// Log encountered exception to security database if possible.
						if (user != null)
						{
							user.LogError("API -> Login Control -> Submit", ex.ToString());
						}
					}
					
				}
				
				#endregion
				
			}
			
		}
	}
}
