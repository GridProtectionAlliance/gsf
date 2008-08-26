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
//  TVA.Security.Application.Controls.ChangePassword.vb - Control for changing password
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
			/// Provides user interface (UI) that enable users to change their password.
			/// </summary>
			public class ChangePassword : CompositeControl
			{
				
				
				#region " Member Declaration "
				
				private TextBox m_usernameTextBox;
				private TextBox m_oldPasswordTextBox;
				private TextBox m_newPasswordTextBox;
				private TextBox m_confirmPasswordTextBox;
				private ControlContainer m_container;
				private WebSecurityProvider m_securityProvider;
				
				#endregion
				
				#region " Code Scope: Public "
				
				/// <summary>
				/// Creates an instance of the change password control.
				/// </summary>
				/// <param name="container">Control containing this control.</param>
				/// <param name="securityProvider">Current security control.</param>
				public ChangePassword(ControlContainer container, WebSecurityProvider securityProvider)
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
					
					// -----------------------------------------------
					// |                    | ---------------------- |
					// | Username:*         | |                    | |
					// |                    | ---------------------- |
					// -----------------------------------------------
					// |                    | ---------------------- |
					// | Old Password:*     | |                    | |
					// |                    | ---------------------- |
					// -----------------------------------------------
					// |                    | ---------------------- |
					// | New Password:*     | |                    | |
					// |                    | ---------------------- |
					// -----------------------------------------------
					// |                    | ---------------------- |
					// | Confirm Password:* | |                    | |
					// |                    | ---------------------- |
					// -----------------------------------------------
					// |                    |         -------------- |
					// |                    |         |   Submit   | |
					// |                    |         -------------- |
					// -----------------------------------------------
					
					// Layout the control.
					Table container = ControlContainer.NewTable(5, 2);
					
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
					m_oldPasswordTextBox = new TextBox();
					m_oldPasswordTextBox.ID = "OldPasswordTextBox";
					m_oldPasswordTextBox.Width = Unit.Parse("150px");
					m_oldPasswordTextBox.TextMode = TextBoxMode.Password;
					RequiredFieldValidator oldPasswordValidator = new RequiredFieldValidator();
					oldPasswordValidator.Display = ValidatorDisplay.None;
					oldPasswordValidator.ErrorMessage = "Old Password is required.";
					oldPasswordValidator.ControlToValidate = m_oldPasswordTextBox.ID;
					container.Rows[1].Cells[0].Text = "Old Password:*&nbsp;";
					container.Rows[1].Cells[0].HorizontalAlign = HorizontalAlign.Right;
					container.Rows[1].Cells[1].Controls.Add(m_oldPasswordTextBox);
					container.Rows[1].Cells[1].Controls.Add(oldPasswordValidator);
					
					// Row #3
					m_newPasswordTextBox = new TextBox();
					m_newPasswordTextBox.ID = "NewPasswordTextBox";
					m_newPasswordTextBox.Width = Unit.Parse("150px");
					m_newPasswordTextBox.TextMode = TextBoxMode.Password;
					RequiredFieldValidator newPasswordValidator = new RequiredFieldValidator();
					newPasswordValidator.Display = ValidatorDisplay.None;
					newPasswordValidator.ErrorMessage = "New Password is required.";
					newPasswordValidator.ControlToValidate = m_newPasswordTextBox.ID;
					container.Rows[2].Cells[0].Text = "New Password:*&nbsp;";
					container.Rows[2].Cells[0].HorizontalAlign = HorizontalAlign.Right;
					container.Rows[2].Cells[1].Controls.Add(m_newPasswordTextBox);
					container.Rows[2].Cells[1].Controls.Add(newPasswordValidator);
					
					// Row #4
					m_confirmPasswordTextBox = new TextBox();
					m_confirmPasswordTextBox.ID = "ConfirmPasswordTextBox";
					m_confirmPasswordTextBox.Width = Unit.Parse("150px");
					m_confirmPasswordTextBox.TextMode = TextBoxMode.Password;
					RequiredFieldValidator confirmPasswordValidator = new RequiredFieldValidator();
					confirmPasswordValidator.Display = ValidatorDisplay.None;
					confirmPasswordValidator.ErrorMessage = "Confirm Password is required.";
					confirmPasswordValidator.ControlToValidate = m_confirmPasswordTextBox.ID;
					container.Rows[3].Cells[0].Text = "Confirm Password:*&nbsp;";
					container.Rows[3].Cells[0].HorizontalAlign = HorizontalAlign.Right;
					container.Rows[3].Cells[1].Controls.Add(m_confirmPasswordTextBox);
					container.Rows[3].Cells[1].Controls.Add(confirmPasswordValidator);
					
					// Row #5
					Button submitButton = new Button();
					submitButton.Text = "Submit";
					submitButton.Click += new System.EventHandler(SubmitButton_Click);
					CompareValidator passwordCompareValidator = new CompareValidator();
					passwordCompareValidator.Display = ValidatorDisplay.None;
					passwordCompareValidator.ErrorMessage = "New Password and Confirm Password must match.";
					passwordCompareValidator.ControlToValidate = m_newPasswordTextBox.ID;
					passwordCompareValidator.ControlToCompare = m_confirmPasswordTextBox.ID;
					ValidationSummary validationSummary = new ValidationSummary();
					validationSummary.ShowSummary = false;
					validationSummary.ShowMessageBox = true;
					container.Rows[4].Cells[0].Controls.Add(passwordCompareValidator);
					container.Rows[4].Cells[0].Controls.Add(validationSummary);
					container.Rows[4].Cells[1].HorizontalAlign = HorizontalAlign.Right;
					container.Rows[4].Cells[1].Controls.Add(submitButton);
					
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
						// In RSA authentication mode the following substitution must take place:
						// Old Password      -> Token
						// New Password      -> New Pin
						// Confirm Password  -> Confirm Pin
						oldPasswordValidator.ErrorMessage = "Token is required.";
						newPasswordValidator.ErrorMessage = "New Pin is required.";
						confirmPasswordValidator.ErrorMessage = "Confirm Pin is required.";
						passwordCompareValidator.ErrorMessage = "New Pin and Confirm Pin must match.";
						container.Rows[1].Cells[0].Text = "Token:*";
						container.Rows[2].Cells[0].Text = "New Pin:*";
						container.Rows[3].Cells[0].Text = "Confirm Pin:*";
						
						if (Page.Session[Login.NewPinVerify] != null)
						{
							// It is verified that the user account is in "new pin" mode and user must create a new pin.
							System.Text.StringBuilder with_2 = new StringBuilder();
							with_2.Append("Note: You are required to create a new pin for your RSA SecurID key. The pin must ");
							with_2.Append("be a 4 to 8 character alpha-numeric string. Please wait for the token on your RSA ");
							with_2.Append("SecurID key to change before proceeding.");
							
							m_container.UpdateMessageText(with_2.ToString(), MessageType.Information);
						}
						else
						{
							// User clicked on the Change Password link, so cannot allow a new pin to be created.
							this.Enabled = false;
							System.Text.StringBuilder with_3 = new StringBuilder();
							with_3.Append("This screen is only active as part of an automated process. To create a new pin, ");
							with_3.Append("you must call the Operations Duty Specialist at 423-751-1700.");
							
							m_container.UpdateMessageText(with_3.ToString(), MessageType.Error);
						}
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
							// We instantiate a User object, but skip the authentication. Here's why:
							// - Under AD authentication mode, current password will be verified by ChangePassword().
							// - Under RSA authentication mode, we can use a token (i.e. old password substitution) only
							//   once and if we perform authentication, we will not be able to create a new pin using the
							//   same token again because the RSA server will reject our request to create a pin.
							user = new User(m_usernameTextBox.Text, m_oldPasswordTextBox.Text, m_securityProvider.ApplicationName, m_securityProvider.Server, m_securityProvider.AuthenticationMode, false);
							
							if (! user.IsDefined)
							{
								// Don't proceed, account doesn't exist for user.
								m_container.UpdateMessageText("Operation aborted. No such account.", MessageType.Error);
								return;
							}
							else if (user.IsLockedOut)
							{
								// Don't proceed, user's account has been locked.
								m_container.UpdateMessageText("Operation aborted. Account is locked.", MessageType.Error);
								return;
							}
							else if (m_securityProvider.AuthenticationMode == AuthenticationMode.AD)
							{
								// Under AD authentication, the following restriction apply:
								if (! user.IsExternal)
								{
									// Don't proceed, only external user can change their password from here.
									m_container.UpdateMessageText("Operation aborted. Internal users cannot perform this task.", MessageType.Error);
									return;
								}
								else
								{
									// User's old and new password must be verified.
									if (user.Password != user.EncryptPassword(m_oldPasswordTextBox.Text))
									{
										// Don't proceed, user's failed to provide the correct current password.
										m_container.UpdateMessageText("Operation aborted. Old password verification failed.", MessageType.Error);
										return;
									}
									
									try
									{
										user.EncryptPassword(m_newPasswordTextBox.Text);
									}
									catch (Exception ex)
									{
										// Don't proceed, user's new password doesn't meeet strong password requirements.
										m_container.UpdateMessageText(ex.Message.Replace(Environment.NewLine, "<br />"), MessageType.Error);
										return;
									}
								}
							}
							
							// Go ahead and attempt to change the user password or create a new pin.
							if (user.ChangePassword(m_oldPasswordTextBox.Text, m_newPasswordTextBox.Text))
							{
								// Inform user about the success.
								if (m_securityProvider.AuthenticationMode == AuthenticationMode.AD)
								{
									m_container.UpdateMessageText("Your password has been changed!<br />You can now use your new password to login.", MessageType.Information);
								}
								else if (m_securityProvider.AuthenticationMode == AuthenticationMode.RSA)
								{
									this.Enabled = false;
									Page.Session.Remove(Login.NewPinVerify);
									m_container.UpdateMessageText("Your new pin has been created!<br />Please wait for the token to change before next login.", MessageType.Information);
								}
							}
							else
							{
								// This is highly unlikely because we've performed all checks before we actually attempted
								// to change the password or create a new pin.
								m_container.UpdateMessageText("Operation failed. Reason unknown.", MessageType.Error);
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
							user.LogError("API -> Change Password Control -> Submit", ex.ToString());
						}
					}
					
				}
				
				#endregion
				
			}
			
		}
	}
}
