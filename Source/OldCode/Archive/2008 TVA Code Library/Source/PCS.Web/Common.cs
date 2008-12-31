using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Text;
//using System.Environment;
using System.Windows.Forms;

//*******************************************************************************************************
//  PCS.Web.Commmon.vb - Common Functions for Web Pages
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [PCS]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  03/04/2005 - Pinal C. Patel
//       Original version of source code generated
//  04/28/2006 - Pinal C. Patel
//       2.0 version of source code migrated from 1.1 source (PCS.Asp.Common)
//
//*******************************************************************************************************



namespace PCS.Web
{
	public sealed class Common
	{
		
		
		private Common()
		{
			
			// This class contains only global functions and is not meant to be instantiated
			
		}
		
		/// <summary>
		/// Performs JavaScript encoding on given string.
		/// </summary>
		/// <param name="text">The string to be encoded.</param>
		/// <returns>The encoded string.</returns>
		/// <remarks></remarks>
		public static string JavaScriptEncode(string text)
		{
			
			text = text.Replace("\\", "\\\\");
			text = text.Replace("\'", "\\\'");
			text = text.Replace("\"", "\\\"");
			text = text.Replace(Convert.ToChar(8), "\\b");
			text = text.Replace(Convert.ToChar(9), "\\t");
			text = text.Replace(Convert.ToChar(10), "\\r");
			text = text.Replace(Convert.ToChar(12), "\\f");
			text = text.Replace(Convert.ToChar(13), "\\n");
			
			return text;
			
		}
		
		/// <summary>
		/// Decodes JavaScript characters from given string.
		/// </summary>
		/// <param name="text">The string to be decoded.</param>
		/// <returns>The decoded string.</returns>
		/// <remarks></remarks>
		public static string JavaScriptDecode(string text)
		{
			
			text = text.Replace("\\\\", "\\");
			text = text.Replace("\\\'", "\'");
			text = text.Replace("\\\"", "\"");
			text = text.Replace("\\b", Convert.ToChar(8));
			text = text.Replace("\\t", Convert.ToChar(9));
			text = text.Replace("\\r", Convert.ToChar(10));
			text = text.Replace("\\f", Convert.ToChar(12));
			text = text.Replace("\\n", Convert.ToChar(13));
			
			return text;
			
		}
		
		/// <summary>
		/// Ensures a string is compliant with cookie name requirements.
		/// </summary>
		/// <param name="text">The string to be validated.</param>
		/// <returns>The validated string.</returns>
		/// <remarks></remarks>
		public static string ValidCookieName(string text)
		{
			
			text = text.Replace("=", "");
			text = text.Replace(";", "");
			text = text.Replace(",", "");
			text = text.Replace(Convert.ToChar(9), "");
			text = text.Replace(Convert.ToChar(10), "");
			text = text.Replace(Convert.ToChar(13), "");
			
			return text;
			
		}
		
		/// <summary>
		/// Ensures a string is compliant with cookie value requirements.
		/// </summary>
		/// <param name="text">The string to be validated.</param>
		/// <returns>The validated string.</returns>
		/// <remarks></remarks>
		public static string ValidCookieValue(string text)
		{
			
			text = text.Replace(";", "");
			text = text.Replace(",", "");
			
			return text;
			
		}
		
		/// <summary>
		/// Sets the focus to the specified web control.
		/// </summary>
		/// <param name="control">The web control to which the focus is to be set.</param>
		/// <remarks></remarks>
		public static void Focus(System.Web.UI.Control control)
		{
			
			if (! control.Page.ClientScript.IsClientScriptBlockRegistered("Focus"))
			{
				control.Page.ClientScript.RegisterClientScriptBlock(control.Page.GetType(), "Focus", CreateClientSideScript(ClientSideScript.Focus));
			}
			
			System.Text.StringBuilder with_1 = new StringBuilder;
			with_1.Append("<script language=\"javascript\">" + System.Environment.NewLine);
			with_1.Append("   Focus(\'" + control.ClientID+ "\');" + System.Environment.NewLine);
			with_1.Append("</script>" + System.Environment.NewLine);
			
			if (! control.Page.ClientScript.IsStartupScriptRegistered("Focus." + control.ClientID))
			{
				control.Page.ClientScript.RegisterStartupScript(control.Page.GetType(), "Focus." + control.ClientID, with_1.ToString());
			}
			
		}
		
		/// <summary>
		/// Assigns a default action button to be activated when the ENTER key is pressed inside the specified textbox.
		/// </summary>
		/// <param name="textbox">The textbox for which the default action button is to be registered.</param>
		/// <param name="control">The button that will be assigned as the default action handler.</param>
		/// <remarks></remarks>
		public static void DefaultButton(System.Web.UI.WebControls.TextBox textbox, System.Web.UI.Control control)
		{
			
			if (! control.Page.ClientScript.IsClientScriptBlockRegistered("DefaultButton"))
			{
				control.Page.ClientScript.RegisterClientScriptBlock(control.Page.GetType(), "DefaultButton", CreateClientSideScript(ClientSideScript.DefaultButton));
			}
			
			textbox.Attributes.Add("OnKeyDown", "javascript:DefaultButton(\'" + control.ClientID+ "\')");
			
		}
		
		/// <summary>
		/// Shows the specified text inside the textbox that can be used to provide a hint.
		/// </summary>
		/// <param name="textbox">The textbox in which the text is to be displayed.</param>
		/// <param name="text">The text to be displayed inside the textbox.</param>
		/// <remarks></remarks>
		public static void SmartText(System.Web.UI.WebControls.TextBox textbox, string text)
		{
			
			System.Web.UI.WebControls.TextBox with_1 = textbox;
			with_1.Attributes.Add("Value", text);
			with_1.Attributes.Add("OnFocus", "this.select();");
			with_1.Attributes.Add("OnBlur", "if(this.value == \'\'){this.value = \'" + text + "\';}");
			
		}
		
		/// <summary>
		/// Brings the browser window in which the specified web page has loaded to the foreground.
		/// </summary>
		/// <param name="page">The web page whose browser window is to be brought to the foreground.</param>
		/// <remarks></remarks>
		public static void BringToFront(System.Web.UI.Page page)
		{
			
			if (! page.ClientScript.IsStartupScriptRegistered("BringToFront"))
			{
				System.Text.StringBuilder with_1 = new StringBuilder;
				with_1.Append("<script language=\"javascript\">" + System.Environment.NewLine);
				with_1.Append("   window.focus();" + System.Environment.NewLine);
				with_1.Append("</script>" + System.Environment.NewLine);
				
				page.ClientScript.RegisterStartupScript(page.GetType(), "BringToFront", with_1.ToString());
			}
			
		}
		
		/// <summary>
		/// Pushes the browser window in which the specified web page has loaded to the background.
		/// </summary>
		/// <param name="page">The web page whose browser window is to be pushed to the background.</param>
		/// <remarks></remarks>
		public static void PushToBack(System.Web.UI.Page page)
		{
			
			if (! page.ClientScript.IsStartupScriptRegistered("PushToBack"))
			{
				System.Text.StringBuilder with_1 = new StringBuilder;
				with_1.Append("<script language=\"javascript\">" + System.Environment.NewLine);
				with_1.Append("   window.blur();" + System.Environment.NewLine);
				with_1.Append("</script>" + System.Environment.NewLine);
				
				page.ClientScript.RegisterStartupScript(page.GetType(), "PushToBack", with_1.ToString());
			}
			
		}
		
		/// <summary>
		/// Plays audio in the background when the specified web page has loaded.
		/// </summary>
		/// <param name="page">The web page in which audio is to be played.</param>
		/// <param name="soundFilename">Name of the audio file.</param>
		/// <param name="repeatCount">Number of times the audio is to be replayed.</param>
		/// <remarks></remarks>
		public static void PlayBackgroundSound(System.Web.UI.Page page, string soundFilename, int repeatCount)
		{
			
			if (! page.ClientScript.IsStartupScriptRegistered("PlayBackgroundSound"))
			{
				System.Text.StringBuilder with_1 = new StringBuilder;
				with_1.Append("<BGSOUND SRC=\"" + soundFilename + "\" LOOP=\"" + repeatCount + "\">" + System.Environment.NewLine);
				
				page.ClientScript.RegisterStartupScript(page.GetType(), "PlayBackgroundSound", with_1.ToString());
			}
			
		}
		
		#region " Show Overloads "
		
		/// <summary>
		/// Shows a modeless popup window for the specified web page url when the specified web page has loaded.
		/// </summary>
		/// <param name="page">The web page that will show the popup window when loaded.</param>
		/// <param name="url">The web page url for which the popup windows is to be shown.</param>
		/// <remarks></remarks>
		public static void Show(System.Web.UI.Page page, string url)
		{
			
			Show(page, url, 400, 600);
			
		}
		
		/// <summary>
		/// Shows a modeless popup window for the specified web page url when the specified web page has loaded.
		/// </summary>
		/// <param name="page">The web page that will show the popup window when loaded.</param>
		/// <param name="url">The web page url for which the popup windows is to be shown.</param>
		/// <param name="height">The height of the popup window.</param>
		/// <param name="width">The width of the popup window.</param>
		/// <remarks></remarks>
		public static void Show(System.Web.UI.Page page, string url, int height, int width)
		{
			
			Show(page, url, height, width, 0, 0, true, false, false, false);
			
		}
		
		/// <summary>
		/// Shows a modeless popup window for the specified web page url when the specified web page has loaded.
		/// </summary>
		/// <param name="page">The web page that will show the popup window when loaded.</param>
		/// <param name="url">The web page url for which the popup windows is to be shown.</param>
		/// <param name="height">The height of the popup window.</param>
		/// <param name="width">The width of the popup window.</param>
		/// <param name="left">Location of the popup window on X-axis.</param>
		/// <param name="top">Location of the popup window on Y-axis.</param>
		/// <remarks></remarks>
		public static void Show(System.Web.UI.Page page, string url, int height, int width, int left, int top)
		{
			
			Show(page, url, height, width, left, top, false, false, false, false);
			
		}
		
		/// <summary>
		/// Shows a modeless popup window for the specified web page url when the specified web page has loaded.
		/// </summary>
		/// <param name="page">The web page that will show the popup window when loaded.</param>
		/// <param name="url">The web page url for which the popup windows is to be shown.</param>
		/// <param name="height">The height of the popup window.</param>
		/// <param name="width">The width of the popup window.</param>
		/// <param name="left">Location of the popup window on X-axis.</param>
		/// <param name="top">Location of the popup window on Y-axis.</param>
		/// <param name="center">True if the popup window is to be centered; otherwise False.</param>
		/// <param name="help">True if help button is to be displayed; otherwise False.</param>
		/// <param name="resizable">True if the popup window can be resized; otherwise False.</param>
		/// <param name="status">True if the status bar is to be displayed; otherwise False.</param>
		public static void Show(System.Web.UI.Page page, string url, int height, int width, int left, int top, bool center, bool help, bool resizable, bool status)
		{
			
			if (! page.ClientScript.IsClientScriptBlockRegistered("Show"))
			{
				page.ClientScript.RegisterClientScriptBlock(page.GetType(), "Show", CreateClientSideScript(ClientSideScript.Show));
			}
			
			if (! page.ClientScript.IsStartupScriptRegistered("Show." + url))
			{
				System.Text.StringBuilder with_1 = new StringBuilder;
				with_1.Append("<script language=\"javascript\">" + System.Environment.NewLine);
				with_1.Append("   Show(\'" + url + "\', " + height + ", " + width + ", " + left + ", " + top + ", " + Math.Abs(System.Convert.ToInt32(center)) + ", " + Math.Abs(System.Convert.ToInt32(help)) + ", " + Math.Abs(System.Convert.ToInt32(resizable)) + ", " + Math.Abs(System.Convert.ToInt32(status)) + ");" + System.Environment.NewLine);
				with_1.Append("</script>" + System.Environment.NewLine);
				
				if (! page.ClientScript.IsStartupScriptRegistered("Show." + url))
				{
					page.ClientScript.RegisterStartupScript(page.GetType(), "Show." + url, with_1.ToString());
				}
			}
			
		}
		
		/// <summary>
		/// Shows a modeless popup window for the specified web page url when the specified web control is clicked.
		/// </summary>
		/// <param name="control">The web control that will show the popup window when clicked.</param>
		/// <param name="url">The web page url for which the popup windows is to be shown.</param>
		/// <remarks></remarks>
		public static void Show(System.Web.UI.Control control, string url)
		{
			
			Show(control, url, 400, 600, 0, 0, true, false, false, false);
			
		}
		
		/// <summary>
		/// Shows a modeless popup window for the specified web page url when the specified web control is clicked.
		/// </summary>
		/// <param name="control">The web control that will show the popup window when clicked.</param>
		/// <param name="url">The web page url for which the popup windows is to be shown.</param>
		/// <param name="height">The height of the popup window.</param>
		/// <param name="width">The width of the popup window.</param>
		/// <remarks></remarks>
		public static void Show(System.Web.UI.Control control, string url, int height, int width)
		{
			
			Show(control, url, height, width, 0, 0, true, false, false, false);
			
		}
		
		/// <summary>
		/// Shows a modeless popup window for the specified web page url when the specified web control is clicked.
		/// </summary>
		/// <param name="control">The web control that will show the popup window when clicked.</param>
		/// <param name="url">The web page url for which the popup windows is to be shown.</param>
		/// <param name="height">The height of the popup window.</param>
		/// <param name="width">The width of the popup window.</param>
		/// <param name="left">Location of the popup window on X-axis.</param>
		/// <param name="top">Location of the popup window on Y-axis.</param>
		/// <remarks></remarks>
		public static void Show(System.Web.UI.Control control, string url, int height, int width, int left, int top)
		{
			
			Show(control, url, height, width, left, top, false, false, false, false);
			
		}
		
		/// <summary>
		/// Shows a modeless popup window for the specified web page url when the specified web control is clicked.
		/// </summary>
		/// <param name="control">The web control that will show the popup window when clicked.</param>
		/// <param name="url">The web page url for which the popup windows is to be shown.</param>
		/// <param name="height">The height of the popup window.</param>
		/// <param name="width">The width of the popup window.</param>
		/// <param name="left">Location of the popup window on X-axis.</param>
		/// <param name="top">Location of the popup window on Y-axis.</param>
		/// <param name="center">True if the popup window is to be centered; otherwise False.</param>
		/// <param name="help">True if help button is to be displayed; otherwise False.</param>
		/// <param name="resizable">True if the popup window can be resized; otherwise False.</param>
		/// <param name="status">True if the status bar is to be displayed; otherwise False.</param>
		/// <remarks></remarks>
		public static void Show(System.Web.UI.Control control, string url, int height, int width, int left, int top, bool center, bool help, bool resizable, bool status)
		{
			
			if (! control.Page.ClientScript.IsClientScriptBlockRegistered("Show"))
			{
				control.Page.ClientScript.RegisterClientScriptBlock(control.Page.GetType(), "Show", CreateClientSideScript(ClientSideScript.Show));
			}
			
			HookupScriptToControl(control, "javascript:return(Show(\'" + url + "\', " + height + ", " + width + ", " + left + ", " + top + ", " + Math.Abs(System.Convert.ToInt32(center)) + ", " + Math.Abs(System.Convert.ToInt32(help)) + ", " + Math.Abs(System.Convert.ToInt32(resizable)) + ", " + Math.Abs(System.Convert.ToInt32(status)) + "))", "OnClick");
			
		}
		
		#endregion
		
		#region " ShowDialog Overloads "
		
		/// <summary>
		/// Shows a modal popup window for the specified web page url when the specified web page has loaded.
		/// </summary>
		/// <param name="page">The web page that will show the popup window when loaded.</param>
		/// <param name="url">The web page url for which the popup windows is to be shown.</param>
		/// <remarks></remarks>
		public static void ShowDialog(System.Web.UI.Page page, string url)
		{
			
			ShowDialog(page, url, 400, 600);
			
		}
		
		/// <summary>
		/// Shows a modal popup window for the specified web page url when the specified web page has loaded.
		/// </summary>
		/// <param name="page">The web page that will show the popup window when loaded.</param>
		/// <param name="url">The web page url for which the popup windows is to be shown.</param>
		/// <param name="height">The height of the popup window.</param>
		/// <param name="width">The width of the popup window.</param>
		/// <remarks></remarks>
		public static void ShowDialog(System.Web.UI.Page page, string url, int height, int width)
		{
			
			ShowDialog(page, url, null, height, width, 0, 0, true, false, false, false);
			
		}
		
		/// <summary>
		/// Shows a modal popup window for the specified web page url when the specified web page has loaded.
		/// </summary>
		/// <param name="page">The web page that will show the popup window when loaded.</param>
		/// <param name="url">The web page url for which the popup windows is to be shown.</param>
		/// <param name="height">The height of the popup window.</param>
		/// <param name="width">The width of the popup window.</param>
		/// <param name="left">Location of the popup window on X-axis.</param>
		/// <param name="top">Location of the popup window on Y-axis.</param>
		/// <remarks></remarks>
		public static void ShowDialog(System.Web.UI.Page page, string url, int height, int width, int left, int top)
		{
			
			ShowDialog(page, url, null, height, width, left, top, false, false, false, false);
			
		}
		
		/// <summary>
		/// Shows a modal popup window for the specified web page url when the specified web page has loaded.
		/// </summary>
		/// <param name="page">The web page that will show the popup window when loaded.</param>
		/// <param name="url">The web page url for which the popup windows is to be shown.</param>
		/// <param name="dialogResultHolder">
		/// The web control whose text is to updated with the text returned by the web page displayed in the popup window.
		/// </param>
		/// <remarks></remarks>
		public static void ShowDialog(System.Web.UI.Page page, string url, System.Web.UI.Control dialogResultHolder)
		{
			
			ShowDialog(page, url, dialogResultHolder, 400, 600);
			
		}
		
		/// <summary>
		/// Shows a modal popup window for the specified web page url when the specified web page has loaded.
		/// </summary>
		/// <param name="page">The web page that will show the popup window when loaded.</param>
		/// <param name="url">The web page url for which the popup windows is to be shown.</param>
		/// <param name="dialogResultHolder">
		/// The web control whose text is to updated with the text returned by the web page displayed in the popup window.
		/// </param>
		/// <param name="height">The height of the popup window.</param>
		/// <param name="width">The width of the popup window.</param>
		/// <remarks></remarks>
		public static void ShowDialog(System.Web.UI.Page page, string url, System.Web.UI.Control dialogResultHolder, int height, int width)
		{
			
			ShowDialog(page, url, dialogResultHolder, height, width, 0, 0, true, false, false, false);
			
		}
		
		/// <summary>
		/// Shows a modal popup window for the specified web page url when the specified web page has loaded.
		/// </summary>
		/// <param name="page">The web page that will show the popup window when loaded.</param>
		/// <param name="url">The web page url for which the popup windows is to be shown.</param>
		/// <param name="dialogResultHolder">
		/// The web control whose text is to updated with the text returned by the web page displayed in the popup window.
		/// </param>
		/// <param name="height">The height of the popup window.</param>
		/// <param name="width">The width of the popup window.</param>
		/// <param name="left">Location of the popup window on X-axis.</param>
		/// <param name="top">Location of the popup window on Y-axis.</param>
		/// <remarks></remarks>
		public static void ShowDialog(System.Web.UI.Page page, string url, System.Web.UI.Control dialogResultHolder, int height, int width, int left, int top)
		{
			
			ShowDialog(page, url, dialogResultHolder, height, width, left, top, false, false, false, false);
			
		}
		
		/// <summary>
		/// Shows a modal popup window for the specified web page url when the specified web page has loaded.
		/// </summary>
		/// <param name="page">The web page that will show the popup window when loaded.</param>
		/// <param name="url">The web page url for which the popup windows is to be shown.</param>
		/// <param name="dialogResultHolder">
		/// The web control whose text is to updated with the text returned by the web page displayed in the popup window.
		/// </param>
		/// <param name="height">The height of the popup window.</param>
		/// <param name="width">The width of the popup window.</param>
		/// <param name="left">Location of the popup window on X-axis.</param>
		/// <param name="top">Location of the popup window on Y-axis.</param>
		/// <param name="center">True if the popup window is to be centered; otherwise False.</param>
		/// <param name="help">True if help button is to be displayed; otherwise False.</param>
		/// <param name="resizable">True if the popup window can be resized; otherwise False.</param>
		/// <param name="status">True if the status bar is to be displayed; otherwise False.</param>
		/// <remarks></remarks>
		public static void ShowDialog(System.Web.UI.Page page, string url, System.Web.UI.Control dialogResultHolder, int height, int width, int left, int top, bool center, bool help, bool resizable, bool status)
		{
			
			if (! page.ClientScript.IsClientScriptBlockRegistered("ShowDialog"))
			{
				page.ClientScript.RegisterClientScriptBlock(page.GetType(), "ShowDialog", CreateClientSideScript(ClientSideScript.ShowDialog));
			}
			
			if (! page.ClientScript.IsStartupScriptRegistered("ShowDialog." + url))
			{
				System.Text.StringBuilder with_1 = new StringBuilder;
				with_1.Append("<input type=\"hidden\" name=\"PCS_EVENT_TARGET\" value=\"\" />" + System.Environment.NewLine);
				with_1.Append("<input type=\"hidden\" name=\"PCS_EVENT_ARGUMENT\" value=\"\" />" + System.Environment.NewLine);
				with_1.Append("<script language=\"javascript\">" + System.Environment.NewLine);
				if (dialogResultHolder != null)
				{
					with_1.Append("   ShowDialog(\'" + url + "\', \'" + dialogResultHolder.ClientID+ "\', " + height + ", " + width + ", " + left + ", " + top + ", " + Math.Abs(System.Convert.ToInt32(center)) + ", " + Math.Abs(System.Convert.ToInt32(help)) + ", " + Math.Abs(System.Convert.ToInt32(resizable)) + ", " + Math.Abs(System.Convert.ToInt32(status)) + ");" + System.Environment.NewLine);
				}
				else
				{
					with_1.Append("   if (ShowDialog(\'" + url + "\', null, " + height + ", " + width + ", " + left + ", " + top + ", " + Math.Abs(System.Convert.ToInt32(center)) + ", " + Math.Abs(System.Convert.ToInt32(help)) + ", " + Math.Abs(System.Convert.ToInt32(resizable)) + ", " + Math.Abs(System.Convert.ToInt32(status)) + "))" + System.Environment.NewLine);
					with_1.Append("   {" + System.Environment.NewLine);
					
					foreach (System.Web.UI.Control Control in page.Controls)
					{
						if (Control is System.Web.UI.HtmlControls.HtmlForm)
						{
							with_1.Append("       " + Control.ClientID() + ".PCS_EVENT_TARGET.value = \'ShowDialog\';" + System.Environment.NewLine);
							with_1.Append("       " + Control.ClientID+ ".PCS_EVENT_ARGUMENT.value = \'" + url + "\';" + System.Environment.NewLine);
							with_1.Append("       document." + Control.ClientID() + ".submit();" + System.Environment.NewLine);
							break;
						}
					}
					with_1.Append("   }" + System.Environment.NewLine);
				}
				with_1.Append("</script>" + System.Environment.NewLine);
				
				page.ClientScript.RegisterStartupScript(page.GetType(), "ShowDialog." + url, with_1.ToString());
			}
			
		}
		
		/// <summary>
		/// Shows a modal popup window for the specified web page url when the specified web control is clicked.
		/// </summary>
		/// <param name="control">The web control that will show the popup window when clicked.</param>
		/// <param name="url">The web page url for which the popup windows is to be shown.</param>
		/// <remarks></remarks>
		public static void ShowDialog(System.Web.UI.Control control, string url)
		{
			
			ShowDialog(control, url, 400, 600);
			
		}
		
		/// <summary>
		/// Shows a modal popup window for the specified web page url when the specified web control is clicked.
		/// </summary>
		/// <param name="control">The web control that will show the popup window when clicked.</param>
		/// <param name="url">The web page url for which the popup windows is to be shown.</param>
		/// <param name="height">The height of the popup window.</param>
		/// <param name="width">The width of the popup window.</param>
		/// <remarks></remarks>
		public static void ShowDialog(System.Web.UI.Control control, string url, int height, int width)
		{
			
			ShowDialog(control, url, null, height, width, 0, 0, true, false, false, false);
			
		}
		
		/// <summary>
		/// Shows a modal popup window for the specified web page url when the specified web control is clicked.
		/// </summary>
		/// <param name="control">The web control that will show the popup window when clicked.</param>
		/// <param name="url">The web page url for which the popup windows is to be shown.</param>
		/// <param name="height">The height of the popup window.</param>
		/// <param name="width">The width of the popup window.</param>
		/// <param name="left">Location of the popup window on X-axis.</param>
		/// <param name="top">Location of the popup window on Y-axis.</param>
		/// <remarks></remarks>
		public static void ShowDialog(System.Web.UI.Control control, string url, int height, int width, int left, int top)
		{
			
			ShowDialog(control, url, null, height, width, left, top, false, false, false, false);
			
		}
		
		/// <summary>
		/// Shows a modal popup window for the specified web page url when the specified web control is clicked.
		/// </summary>
		/// <param name="control">The web control that will show the popup window when clicked.</param>
		/// <param name="url">The web page url for which the popup windows is to be shown.</param>
		/// <param name="dialogResultHolder">
		/// The web control whose text is to updated with the text returned by the web page displayed in the popup window.
		/// </param>
		/// <remarks></remarks>
		public static void ShowDialog(System.Web.UI.Control control, string url, System.Web.UI.Control dialogResultHolder)
		{
			
			ShowDialog(control, url, dialogResultHolder, 400, 600);
			
		}
		
		/// <summary>
		/// Shows a modal popup window for the specified web page url when the specified web control is clicked.
		/// </summary>
		/// <param name="control">The web control that will show the popup window when clicked.</param>
		/// <param name="url">The web page url for which the popup windows is to be shown.</param>
		/// <param name="dialogResultHolder">
		/// The web control whose text is to updated with the text returned by the web page displayed in the popup window.
		/// </param>
		/// <param name="height">The height of the popup window.</param>
		/// <param name="width">The width of the popup window.</param>
		/// <remarks></remarks>
		public static void ShowDialog(System.Web.UI.Control control, string url, System.Web.UI.Control dialogResultHolder, int height, int width)
		{
			
			ShowDialog(control, url, dialogResultHolder, height, width, 0, 0, true, false, false, false);
			
		}
		
		/// <summary>
		/// Shows a modal popup window for the specified web page url when the specified web control is clicked.
		/// </summary>
		/// <param name="control">The web control that will show the popup window when clicked.</param>
		/// <param name="url">The web page url for which the popup windows is to be shown.</param>
		/// <param name="dialogResultHolder">
		/// The web control whose text is to updated with the text returned by the web page displayed in the popup window.
		/// </param>
		/// <param name="height">The height of the popup window.</param>
		/// <param name="width">The width of the popup window.</param>
		/// <param name="left">Location of the popup window on X-axis.</param>
		/// <param name="top">Location of the popup window on Y-axis.</param>
		/// <remarks></remarks>
		public static void ShowDialog(System.Web.UI.Control control, string url, System.Web.UI.Control dialogResultHolder, int height, int width, int left, int top)
		{
			
			ShowDialog(control, url, dialogResultHolder, height, width, left, top, false, false, false, false);
			
		}
		
		/// <summary>
		/// Shows a modal popup window for the specified web page url when the specified web control is clicked.
		/// </summary>
		/// <param name="control">The web control that will show the popup window when clicked.</param>
		/// <param name="url">The web page url for which the popup windows is to be shown.</param>
		/// <param name="dialogResultHolder">
		/// The web control whose text is to updated with the text returned by the web page displayed in the popup window.
		/// </param>
		/// <param name="height">The height of the popup window.</param>
		/// <param name="width">The width of the popup window.</param>
		/// <param name="left">Location of the popup window on X-axis.</param>
		/// <param name="top">Location of the popup window on Y-axis.</param>
		/// <param name="center">True if the popup window is to be centered; otherwise False.</param>
		/// <param name="help">True if help button is to be displayed; otherwise False.</param>
		/// <param name="resizable">True if the popup window can be resized; otherwise False.</param>
		/// <param name="status">True if the status bar is to be displayed; otherwise False.</param>
		/// <remarks></remarks>
		public static void ShowDialog(System.Web.UI.Control control, string url, System.Web.UI.Control dialogResultHolder, int height, int width, int left, int top, bool center, bool help, bool resizable, bool status)
		{
			
			if (! control.Page.ClientScript.IsClientScriptBlockRegistered("ShowDialog"))
			{
				control.Page.ClientScript.RegisterClientScriptBlock(control.Page.GetType(), "ShowDialog", CreateClientSideScript(ClientSideScript.ShowDialog));
			}
			
			if (dialogResultHolder != null)
			{
				HookupScriptToControl(control, "javascript:return(ShowDialog(\'" + url + "\', \'" + dialogResultHolder.ClientID+ "\', " + height + ", " + width + ", " + left + ", " + top + ", " + Math.Abs(System.Convert.ToInt32(center)) + ", " + Math.Abs(System.Convert.ToInt32(help)) + ", " + Math.Abs(System.Convert.ToInt32(resizable)) + ", " + Math.Abs(System.Convert.ToInt32(status)) + "))", "OnClick");
			}
			else
			{
				HookupScriptToControl(control, "javascript:return(ShowDialog(\'" + url + "\', null, " + height + ", " + width + ", " + left + ", " + top + ", " + Math.Abs(System.Convert.ToInt32(center)) + ", " + Math.Abs(System.Convert.ToInt32(help)) + ", " + Math.Abs(System.Convert.ToInt32(resizable)) + ", " + Math.Abs(System.Convert.ToInt32(status)) + "))", "OnClick");
			}
			
		}
		
		#endregion
		
		#region " ShowPopup Overloads "
		
		/// <summary>
		/// Shows a popup for the specified web page url when the specified web page has loaded.
		/// </summary>
		/// <param name="page">The web page that will show the popup when loaded.</param>
		/// <param name="url">The web page url for which the popup is to be shown.</param>
		/// <remarks></remarks>
		public static void ShowPopup(System.Web.UI.Page page, string url)
		{
			
			ShowPopup(page, url, 400, 600);
			
		}
		
		/// <summary>
		/// Shows a popup for the specified web page url when the specified web page has loaded.
		/// </summary>
		/// <param name="page">The web page that will show the popup when loaded.</param>
		/// <param name="url">The web page url for which the popup is to be shown.</param>
		/// <param name="height">The height of the popup.</param>
		/// <param name="width">The width of the popup.</param>
		/// <remarks></remarks>
		public static void ShowPopup(System.Web.UI.Page page, string url, int height, int width)
		{
			
			ShowPopup(page, url, height, width, 0, 0, true, false, false, false, false, false, false, false);
			
		}
		
		/// <summary>
		/// Shows a popup for the specified web page url when the specified web page has loaded.
		/// </summary>
		/// <param name="page">The web page that will show the popup when loaded.</param>
		/// <param name="url">The web page url for which the popup is to be shown.</param>
		/// <param name="height">The height of the popup.</param>
		/// <param name="width">The width of the popup.</param>
		/// <param name="left">Location of the popup on X-axis.</param>
		/// <param name="top">Location of the popup on Y-axis.</param>
		/// <remarks></remarks>
		public static void ShowPopup(System.Web.UI.Page page, string url, int height, int width, int left, int top)
		{
			
			ShowPopup(page, url, height, width, left, top, false, false, false, false, false, false, false, false);
			
		}
		
		/// <summary>
		/// Shows a popup for the specified web page url when the specified web page has loaded.
		/// </summary>
		/// <param name="page">The web page that will show the popup when loaded.</param>
		/// <param name="url">The web page url for which the popup is to be shown.</param>
		/// <param name="height">The height of the popup.</param>
		/// <param name="width">The width of the popup.</param>
		/// <param name="left">Location of the popup on X-axis.</param>
		/// <param name="top">Location of the popup on Y-axis.</param>
		/// <param name="center">True if the popup is to be centered; otherwise False.</param>
		/// <param name="resizable">True if the popup can be resized; otherwise False.</param>
		/// <param name="scrollbars">True is the scrollbars are to be displayed; otherwise False.</param>
		/// <param name="toolbar">True if the toolbar is to be displayed; otherwise False.</param>
		/// <param name="menubar">True if the menu bar is to be displayed; otherwise False.</param>
		/// <param name="location">True if the address bar is to be displayed; otherwise False.</param>
		/// <param name="status">True if the status bar is to be displayed; otherwise False.</param>
		/// <param name="directories">True if the directories buttons (Netscape only) are to be displayed; otherwise False.</param>
		/// <remarks></remarks>
		public static void ShowPopup(System.Web.UI.Page page, string url, int height, int width, int left, int top, bool center, bool resizable, bool scrollbars, bool toolbar, bool menubar, bool location, bool status, bool directories)
		{
			
			if (! page.ClientScript.IsClientScriptBlockRegistered("ShowPopup"))
			{
				page.ClientScript.RegisterClientScriptBlock(page.GetType(), "ShowPopup", CreateClientSideScript(ClientSideScript.ShowPopup));
			}
			
			if (! page.ClientScript.IsStartupScriptRegistered("ShowPopup." + url))
			{
				System.Text.StringBuilder with_1 = new StringBuilder;
				with_1.Append("<script language=\"javascript\">" + System.Environment.NewLine);
				with_1.Append("   ShowPopup(\'" + url + "\', " + height + ", " + width + ", " + left + ", " + top + ", " + Math.Abs(System.Convert.ToInt32(center)) + ", " + Math.Abs(System.Convert.ToInt32(resizable)) + ", " + Math.Abs(System.Convert.ToInt32(scrollbars)) + ", " + Math.Abs(System.Convert.ToInt32(toolbar)) + ", " + Math.Abs(System.Convert.ToInt32(menubar)) + ", " + Math.Abs(System.Convert.ToInt32(location)) + ", " + Math.Abs(System.Convert.ToInt32(status)) + ", " + Math.Abs(System.Convert.ToInt32(directories)) + ");" + System.Environment.NewLine);
				with_1.Append("</script>" + System.Environment.NewLine);
				
				page.ClientScript.RegisterStartupScript(page.GetType(), "ShowPopup." + url, with_1.ToString());
			}
			
		}
		
		/// <summary>
		/// Shows a popup for the specified web page url when the specified web control is clicked.
		/// </summary>
		/// <param name="control">The web control that will show the popup when clicked.</param>
		/// <param name="url">The web page url for which the popup is to be shown.</param>
		/// <remarks></remarks>
		public static void ShowPopup(System.Web.UI.Control control, string url)
		{
			
			ShowPopup(control, url, 400, 600);
			
		}
		
		/// <summary>
		/// Shows a popup for the specified web page url when the specified web control is clicked.
		/// </summary>
		/// <param name="control">The web control that will show the popup when clicked.</param>
		/// <param name="url">The web page url for which the popup is to be shown.</param>
		/// <param name="height">The height of the popup.</param>
		/// <param name="width">The width of the popup.</param>
		/// <remarks></remarks>
		public static void ShowPopup(System.Web.UI.Control control, string url, int height, int width)
		{
			
			ShowPopup(control, url, height, width, 0, 0, true, false, false, false, false, false, false, false);
			
		}
		
		/// <summary>
		/// Shows a popup for the specified web page url when the specified web control is clicked.
		/// </summary>
		/// <param name="control">The web control that will show the popup when clicked.</param>
		/// <param name="url">The web page url for which the popup is to be shown.</param>
		/// <param name="height">The height of the popup.</param>
		/// <param name="width">The width of the popup.</param>
		/// <param name="left">Location of the popup on X-axis.</param>
		/// <param name="top">Location of the popup on Y-axis.</param>
		/// <remarks></remarks>
		public static void ShowPopup(System.Web.UI.Control control, string url, int height, int width, int left, int top)
		{
			
			ShowPopup(control, url, height, width, left, top, false, false, false, false, false, false, false, false);
			
		}
		
		/// <summary>
		/// Shows a popup for the specified web page url when the specified web control is clicked.
		/// </summary>
		/// <param name="control">The web control that will show the popup when clicked.</param>
		/// <param name="url">The web page url for which the popup is to be shown.</param>
		/// <param name="height">The height of the popup.</param>
		/// <param name="width">The width of the popup.</param>
		/// <param name="left">Location of the popup on X-axis.</param>
		/// <param name="top">Location of the popup on Y-axis.</param>
		/// <param name="center">True if the popup is to be centered; otherwise False.</param>
		/// <param name="resizable">True if the popup can be resized; otherwise False.</param>
		/// <param name="scrollbars">True is the scrollbars are to be displayed; otherwise False.</param>
		/// <param name="toolbar">True if the toolbar is to be displayed; otherwise False.</param>
		/// <param name="menubar">True if the menu bar is to be displayed; otherwise False.</param>
		/// <param name="location">True if the address bar is to be displayed; otherwise False.</param>
		/// <param name="status">True if the status bar is to be displayed; otherwise False.</param>
		/// <param name="directories">True if the directories buttons (Netscape only) are to be displayed; otherwise False.</param>
		/// <remarks></remarks>
		public static void ShowPopup(System.Web.UI.Control control, string url, int height, int width, int left, int top, bool center, bool resizable, bool scrollbars, bool toolbar, bool menubar, bool location, bool status, bool directories)
		{
			
			if (! control.Page.ClientScript.IsClientScriptBlockRegistered("ShowPopup"))
			{
				control.Page.ClientScript.RegisterClientScriptBlock(control.Page.GetType(), "ShowPopup", CreateClientSideScript(ClientSideScript.ShowPopup));
			}
			
			HookupScriptToControl(control, "javascript:return(ShowPopup(\'" + url + "\', " + height + ", " + width + ", " + left + ", " + top + ", " + Math.Abs(System.Convert.ToInt32(center)) + ", " + Math.Abs(System.Convert.ToInt32(resizable)) + ", " + Math.Abs(System.Convert.ToInt32(scrollbars)) + ", " + Math.Abs(System.Convert.ToInt32(toolbar)) + ", " + Math.Abs(System.Convert.ToInt32(menubar)) + ", " + Math.Abs(System.Convert.ToInt32(location)) + ", " + Math.Abs(System.Convert.ToInt32(status)) + ", " + Math.Abs(System.Convert.ToInt32(directories)) + "))", "OnClick");
			
		}
		
		#endregion
		
		#region " Close Overloads "
		
		/// <summary>
		/// Closes the current web page when it has finished loading in the browser and returns the specified value to
		/// the web page that opened it.
		/// </summary>
		/// <param name="page">The current web page.</param>
		/// <remarks></remarks>
		public static void Close(System.Web.UI.Page page)
		{
			
			Close(page, null);
			
		}
		
		/// <summary>
		/// Closes the current web page when it has finished loading in the browser and returns the specified value to
		/// the web page that opened it.
		/// </summary>
		/// <param name="page">The current web page.</param>
		/// <param name="returnValue">The value to be returned to the parent web page that open this web page.</param>
		/// <remarks></remarks>
		public static void Close(System.Web.UI.Page page, string returnValue)
		{
			
			if (! page.ClientScript.IsClientScriptBlockRegistered("Close"))
			{
				page.ClientScript.RegisterClientScriptBlock(page.GetType(), "Close", CreateClientSideScript(ClientSideScript.Close));
			}
			
			if (! page.ClientScript.IsStartupScriptRegistered("Close.Page"))
			{
				System.Text.StringBuilder with_1 = new StringBuilder;
				with_1.Append("<script language=\"javascript\">" + System.Environment.NewLine);
				with_1.Append("   Close(\'" + returnValue + "\');" + System.Environment.NewLine);
				with_1.Append("</script>" + System.Environment.NewLine);
				
				page.ClientScript.RegisterStartupScript(page.GetType(), "Close.Page", with_1.ToString());
			}
			
		}
		
		/// <summary>
		/// Closes the current web page when the specified web control is clicked and returns the specified value to the
		/// web page that opened it.
		/// </summary>
		/// <param name="control">The web control that, when clicked, will close the current web page.</param>
		/// <remarks></remarks>
		public static void Close(System.Web.UI.Control control)
		{
			
			Close(control, null);
			
		}
		
		/// <summary>
		/// Closes the current web page when the specified web control is clicked and returns the specified value to the
		/// web page that opened it.
		/// </summary>
		/// <param name="control">The web control that, when clicked, will close the current web page.</param>
		/// <param name="returnValue">The value to be returned to the parent web page that open this web page.</param>
		/// <remarks></remarks>
		public static void Close(System.Web.UI.Control control, string returnValue)
		{
			
			if (! control.Page.ClientScript.IsClientScriptBlockRegistered("Close"))
			{
				control.Page.ClientScript.RegisterClientScriptBlock(control.Page.GetType(), "Close", CreateClientSideScript(ClientSideScript.Close));
			}
			
			HookupScriptToControl(control, "javascript:return(Close(\'" + returnValue + "\'))", "OnClick");
			
		}
		
		#endregion
		
		#region " MsgBox Overloads "
		
		/// <summary>
		/// The buttons to be displayed in the message box.
		/// </summary>
		/// <remarks></remarks>
		[Flags()]public enum MsgBoxStyle
		{
			/// <summary>
			/// OK Button.
			/// </summary>
			/// <remarks></remarks>
			OKOnly = 0,
			/// <summary>
			/// OK and Cancel buttons.
			/// </summary>
			/// <remarks></remarks>
			OKCancel = 1,
			/// <summary>
			/// Abort,Retry, and Ignore buttons.
			/// </summary>
			/// <remarks></remarks>
			AbortRetryIgnore = 2,
			/// <summary>
			/// Yes, No, and Cancel buttons.
			/// </summary>
			/// <remarks></remarks>
			YesNoCancel = 3,
			/// <summary>
			/// Yes and No buttons.
			/// </summary>
			/// <remarks></remarks>
			YesNo = 4,
			/// <summary>
			/// Retry and Cancel buttons.
			/// </summary>
			/// <remarks></remarks>
			RetryCancel = 5,
			/// <summary>
			/// Critical message.
			/// </summary>
			/// <remarks></remarks>
			Critical = 16,
			/// <summary>
			/// Warning query.
			/// </summary>
			/// <remarks></remarks>
			Question = 32,
			/// <summary>
			/// Warning message.
			/// </summary>
			/// <remarks></remarks>
			Exclamation = 48,
			/// <summary>
			/// Information message.
			/// </summary>
			/// <remarks></remarks>
			Information = 64,
			/// <summary>
			/// First button is default.
			/// </summary>
			/// <remarks></remarks>
			DefaultButton1 = 0,
			/// <summary>
			/// Second button is default.
			/// </summary>
			/// <remarks></remarks>
			DefaultButton2 = 256,
			/// <summary>
			/// Third button is default.
			/// </summary>
			/// <remarks></remarks>
			DefaultButton3 = 512,
			/// <summary>
			/// Fourth button is default.
			/// </summary>
			/// <remarks></remarks>
			DefaultButton4 = 768,
			/// <summary>
			/// Application modal message box.
			/// </summary>
			/// <remarks></remarks>
			ApplicationModal = 0,
			/// <summary>
			/// System modal message box.
			/// </summary>
			/// <remarks></remarks>
			SystemModal = 4096
		}
		
		/// <summary>
		/// Shows a windows application style message box when the specified web page has loaded.
		/// </summary>
		/// <param name="page">The web page that will show the message box when loaded.</param>
		/// <param name="prompt">The text that is to be displayed in the message box.</param>
		/// <param name="title">The title of the message box.</param>
		/// <param name="buttons">The buttons to be displayed in the message box.</param>
		/// <remarks></remarks>
		public static void MsgBox(System.Web.UI.Page page, string prompt, string title, MsgBoxStyle buttons)
		{
			
			MsgBox(page, prompt, title, buttons, true);
			
		}
		
		/// <summary>
		/// Shows a windows application style message box when the specified web page has loaded.
		/// </summary>
		/// <param name="page">The web page that will show the message box when loaded.</param>
		/// <param name="prompt">The text that is to be displayed in the message box.</param>
		/// <param name="title">The title of the message box.</param>
		/// <param name="buttons">The buttons to be displayed in the message box.</param>
		/// <param name="doPostBack">
		/// True if a post-back is to be performed when either OK, Retry, or Yes buttons are clicked in the message box;
		/// otherwise False.
		/// </param>
		/// <remarks></remarks>
		public static void MsgBox(System.Web.UI.Page page, string prompt, string title, MsgBoxStyle buttons, bool doPostBack)
		{
			
			if (! page.ClientScript.IsClientScriptBlockRegistered("ShowMsgBox"))
			{
				page.ClientScript.RegisterClientScriptBlock(page.GetType(), "ShowMsgBox", CreateClientSideScript(ClientSideScript.MsgBox));
			}
			
			if (! page.ClientScript.IsStartupScriptRegistered("ShowMsgBox." + prompt))
			{
				System.Text.StringBuilder with_1 = new StringBuilder;
				with_1.Append("<input type=\"hidden\" name=\"PCS_EVENT_TARGET\" value=\"\" />" + System.Environment.NewLine);
				with_1.Append("<input type=\"hidden\" name=\"PCS_EVENT_ARGUMENT\" value=\"\" />" + System.Environment.NewLine);
				with_1.Append("<script language=\"javascript\">" + System.Environment.NewLine);
				with_1.Append("   if (ShowMsgBox(\'" + JavaScriptEncode(prompt) + " \', \'" + title + "\', " + buttons + ", " + Strings.LCase(doPostBack) + ")) " + System.Environment.NewLine);
				with_1.Append("   {" + System.Environment.NewLine);
				
				foreach (System.Web.UI.Control Control in page.Controls)
				{
					if (Control is System.Web.UI.HtmlControls.HtmlForm)
					{
						with_1.Append("       " + Control.ClientID() + ".PCS_EVENT_TARGET.value = \'MsgBox\';" + System.Environment.NewLine);
						with_1.Append("       " + Control.ClientID() + ".PCS_EVENT_ARGUMENT.value = \'" + title + "\';" + System.Environment.NewLine);
						with_1.Append("       document." + Control.ClientID() + ".submit();" + System.Environment.NewLine);
						break;
					}
				}
				with_1.Append("   }" + System.Environment.NewLine);
				with_1.Append("</script>" + System.Environment.NewLine);
				
				page.ClientScript.RegisterStartupScript(page.GetType(), "ShowMsgBox." + prompt, with_1.ToString());
			}
			
		}
		
		/// <summary>
		/// Shows a windows application style message box when the specified web control is clicked.
		/// </summary>
		/// <param name="control">The web control that will show the message box when clicked.</param>
		/// <param name="prompt">The text that is to be displayed in the message box.</param>
		/// <param name="title">The title of the message box.</param>
		/// <param name="buttons">The buttons to be displayed in the message box.</param>
		/// <remarks></remarks>
		public static void MsgBox(System.Web.UI.Control control, string prompt, string title, MsgBoxStyle buttons)
		{
			
			MsgBox(control, prompt, title, buttons, true);
			
		}
		
		/// <summary>
		/// Shows a windows application style message box when the specified web control is clicked.
		/// </summary>
		/// <param name="control">The web control that will show the message box when clicked.</param>
		/// <param name="prompt">The text that is to be displayed in the message box.</param>
		/// <param name="title">The title of the message box.</param>
		/// <param name="buttons">The buttons to be displayed in the message box.</param>
		/// <param name="doPostBack">
		/// True if a post-back is to be performed when either OK, Retry, or Yes buttons are clicked in the message box;
		/// otherwise False.
		/// </param>
		/// <remarks></remarks>
		public static void MsgBox(System.Web.UI.Control control, string prompt, string title, MsgBoxStyle buttons, bool doPostBack)
		{
			
			if (! control.Page.ClientScript.IsClientScriptBlockRegistered("ShowMsgBox"))
			{
				control.Page.ClientScript.RegisterClientScriptBlock(control.Page.GetType(), "ShowMsgBox", CreateClientSideScript(ClientSideScript.MsgBox));
			}
			
			HookupScriptToControl(control, "javascript:return(ShowMsgBox(\'" + JavaScriptEncode(prompt) + " \', \'" + title + "\', " + buttons + ", " + Strings.LCase(doPostBack) + "))", "OnClick");
			
		}
		
		#endregion
		
		#region " Refresh Overloads "
		
		/// <summary>
		/// Refreshes the web page as soon as it has loaded.
		/// </summary>
		/// <param name="page">The web page that is to be refreshed.</param>
		/// <remarks></remarks>
		public static void Refresh(System.Web.UI.Page page)
		{
			
			Refresh(page, false);
			
		}
		
		/// <summary>
		/// Refreshes the web page as soon as it has loaded.
		/// </summary>
		/// <param name="page">The web page that is to be refreshed.</param>
		/// <param name="postRefresh">
		/// True if the web page is to be refreshed after the entire web page is rendered and loaded; otherwise False.
		/// </param>
		/// <remarks></remarks>
		public static void Refresh(System.Web.UI.Page page, bool postRefresh)
		{
			
			if (! page.ClientScript.IsClientScriptBlockRegistered("Refresh"))
			{
				page.ClientScript.RegisterClientScriptBlock(page.GetType(), "Refresh", CreateClientSideScript(ClientSideScript.Refresh));
			}
			
			if (postRefresh)
			{
				if (! page.ClientScript.IsStartupScriptRegistered("PostRefresh"))
				{
					System.Text.StringBuilder with_1 = new StringBuilder;
					with_1.Append("<script language=\"javascript\">" + System.Environment.NewLine);
					with_1.Append("   Refresh();" + System.Environment.NewLine);
					with_1.Append("</script>" + System.Environment.NewLine);
					
					page.ClientScript.RegisterStartupScript(page.GetType(), "PostRefresh", with_1.ToString());
				}
			}
			else
			{
				if (! page.ClientScript.IsClientScriptBlockRegistered("PreRefresh"))
				{
					System.Text.StringBuilder with_2 = new StringBuilder;
					with_2.Append("<script language=\"javascript\">" + System.Environment.NewLine);
					with_2.Append("   Refresh();" + System.Environment.NewLine);
					with_2.Append("</script>" + System.Environment.NewLine);
					
					page.ClientScript.RegisterClientScriptBlock(page.GetType(), "PreRefresh", with_2.ToString());
				}
			}
			
		}
		
		/// <summary>
		/// Refreshes the web page when the specified web control is clicked.
		/// </summary>
		/// <param name="control">The web control that will refersh the web page when clicked.</param>
		/// <remarks></remarks>
		public static void Refresh(System.Web.UI.Control control)
		{
			
			if (! control.Page.ClientScript.IsClientScriptBlockRegistered("Refresh"))
			{
				control.Page.ClientScript.RegisterClientScriptBlock(control.Page.GetType(), "Refresh", CreateClientSideScript(ClientSideScript.Refresh));
			}
			
			HookupScriptToControl(control, "javascript:return(Refresh())", "OnClick");
			
		}
		
		#endregion
		
		#region " Maximize Overloads "
		
		/// <summary>
		/// Maximizes the browser window when the specified web page has loaded.
		/// </summary>
		/// <param name="page">The web page whose browser window is to be maximized when loaded.</param>
		/// <remarks></remarks>
		public static void Maximize(System.Web.UI.Page page)
		{
			
			if (! page.ClientScript.IsClientScriptBlockRegistered("Maximize"))
			{
				page.ClientScript.RegisterClientScriptBlock(page.GetType(), "Maximize", CreateClientSideScript(ClientSideScript.Maximize));
			}
			
			System.Text.StringBuilder with_1 = new StringBuilder;
			with_1.Append("<script language=\"javascript\">" + System.Environment.NewLine);
			with_1.Append("   Maximize();" + System.Environment.NewLine);
			with_1.Append("</script>" + System.Environment.NewLine);
			
			page.ClientScript.RegisterStartupScript(page.GetType(), "Maximize:" + VBMath.Rnd(), with_1.ToString());
			
		}
		
		/// <summary>
		/// Maximizes the browser window when the specified web control is clicked.
		/// </summary>
		/// <param name="control">The web control that will maximize the browser window when clicked.</param>
		/// <remarks></remarks>
		public static void Maximize(System.Web.UI.Control control)
		{
			
			if (! control.Page.ClientScript.IsClientScriptBlockRegistered("Maximize"))
			{
				control.Page.ClientScript.RegisterClientScriptBlock(control.Page.GetType(), "Maximize", CreateClientSideScript(ClientSideScript.Maximize));
			}
			
			HookupScriptToControl(control, "javascript:return(Maximize())", "OnClick");
			
		}
		
		#endregion
		
		#region " Minimize Overloads "
		
		/// <summary>
		/// Minimizes the browser windows when the specified web page has loaded.
		/// </summary>
		/// <param name="page">The web page whose browser window is to be minimized when loaded.</param>
		/// <remarks></remarks>
		public static void Minimize(System.Web.UI.Page page)
		{
			
			if (! page.ClientScript.IsClientScriptBlockRegistered("Minimize"))
			{
				page.ClientScript.RegisterClientScriptBlock(page.GetType(), "Minimize", CreateClientSideScript(ClientSideScript.Minimize));
			}
			
			if (! page.ClientScript.IsStartupScriptRegistered("Minimize.Page"))
			{
				System.Text.StringBuilder with_1 = new StringBuilder;
				with_1.Append("<script language=\"javascript\">" + System.Environment.NewLine);
				with_1.Append("   Minimize();" + System.Environment.NewLine);
				with_1.Append("</script>" + System.Environment.NewLine);
				
				page.ClientScript.RegisterStartupScript(page.GetType(), "Minimize.Page", with_1.ToString());
			}
			
		}
		
		/// <summary>
		/// Minimizes the browser window when the specified web control is clicked.
		/// </summary>
		/// <param name="control">The web control that will minimize the browser window when clicked.</param>
		/// <remarks></remarks>
		public static void Minimize(System.Web.UI.Control control)
		{
			
			if (! control.Page.ClientScript.IsClientScriptBlockRegistered("Minimize"))
			{
				control.Page.ClientScript.RegisterClientScriptBlock(control.Page.GetType(), "Minimize", CreateClientSideScript(ClientSideScript.Minimize));
			}
			
			HookupScriptToControl(control, "javascript:return(Minimize())", "OnClick");
			
		}
		
		#endregion
		
		#region " RunClientExe Overloads "
		
		/// <summary>
		/// Runs the specified executable on the client's machine when the specified web page finishes loading in the browser.
		/// </summary>
		/// <param name="page">The web page that will run the executable.</param>
		/// <param name="executable">Name of the executable to run.</param>
		/// <remarks></remarks>
		public static void RunClientExe(System.Web.UI.Page page, string executable)
		{
			
			if (! page.ClientScript.IsClientScriptBlockRegistered("RunClientExe"))
			{
				page.ClientScript.RegisterClientScriptBlock(page.GetType(), "RunClientExe", CreateClientSideScript(ClientSideScript.RunClientExe));
			}
			
			if (! page.ClientScript.IsStartupScriptRegistered("RunClientExe." + executable))
			{
				System.Text.StringBuilder with_1 = new StringBuilder;
				with_1.Append("<script language=\"javascript\">" + System.Environment.NewLine);
				with_1.Append("   RunClientExe(\'" + JavaScriptEncode(executable) + "\');" + System.Environment.NewLine);
				with_1.Append("</script>" + System.Environment.NewLine);
				
				page.ClientScript.RegisterStartupScript(page.GetType(), "RunClientExe." + executable, with_1.ToString());
			}
			
		}
		
		/// <summary>
		/// Runs the specified executable on the client's machine when the specified web control is clicked.
		/// </summary>
		/// <param name="control">The web control that will run the executable.</param>
		/// <param name="executable">Name of the executable to run.</param>
		/// <remarks></remarks>
		public static void RunClientExe(System.Web.UI.Control control, string executable)
		{
			
			if (! control.Page.ClientScript.IsClientScriptBlockRegistered("RunClientExe"))
			{
				control.Page.ClientScript.RegisterClientScriptBlock(control.Page.GetType(), "RunClientExe", CreateClientSideScript(ClientSideScript.RunClientExe));
			}
			
			HookupScriptToControl(control, "javascript:return(RunClientExe(\'" + JavaScriptEncode(executable) + "\'))", "OnClick");
			
		}
		
		#endregion
		
		#region " Helpers "
		
		/// <summary>
		/// The different types of client-side scripts.
		/// </summary>
		/// <remarks></remarks>
		private enum ClientSideScript
		{
			
			Focus,
			DefaultButton,
			Show,
			ShowDialog,
			ShowPopup,
			Close,
			MsgBox,
			Refresh,
			Maximize,
			Minimize,
			RunClientExe
			
		}
		
		/// <summary>
		/// Creates the appropriate client-side script based on the specified PCS.Web.ClientSideScript value.
		/// </summary>
		/// <param name="script">One of the PCS.Web.ClientSideScript values.</param>
		/// <returns>The client-side script for the specified PCS.Web.ClientSideScript value</returns>
		/// <remarks></remarks>
		private static string CreateClientSideScript(ClientSideScript script)
		{
			
			string clientScript = "";
			switch (script)
			{
				case ClientSideScript.Focus: // Client-side script for Focus.
					System.Text.StringBuilder with_1 = new StringBuilder();
					with_1.Append("<script language=\"javascript\">" + System.Environment.NewLine);
					with_1.Append("   function Focus(controlId)" + System.Environment.NewLine);
					with_1.Append("   {" + System.Environment.NewLine);
					with_1.Append("       if (document.getElementById(controlId) != null)" + System.Environment.NewLine);
					with_1.Append("       {" + System.Environment.NewLine);
					with_1.Append("           document.getElementById(controlId).focus();" + System.Environment.NewLine);
					with_1.Append("       }" + System.Environment.NewLine);
					with_1.Append("   }" + System.Environment.NewLine);
					with_1.Append("</script>" + System.Environment.NewLine);
					
					clientScript = with_1.ToString();
					break;
				case ClientSideScript.DefaultButton: // Client-side script for DefaultButton.
					System.Text.StringBuilder with_2 = new StringBuilder();
					with_2.Append("<script language=\"javascript\">" + System.Environment.NewLine);
					with_2.Append("   function DefaultButton(controlId)" + System.Environment.NewLine);
					with_2.Append("   {" + System.Environment.NewLine);
					with_2.Append("       if (event.keyCode == 13)" + System.Environment.NewLine);
					with_2.Append("       {" + System.Environment.NewLine);
					with_2.Append("           event.returnValue = false;" + System.Environment.NewLine);
					with_2.Append("           event.cancel = true;" + System.Environment.NewLine);
					with_2.Append("           if (document.getElementById(controlId) != null)" + System.Environment.NewLine);
					with_2.Append("           {" + System.Environment.NewLine);
					with_2.Append("               document.getElementById(controlId).click();" + System.Environment.NewLine);
					with_2.Append("           }" + System.Environment.NewLine);
					with_2.Append("       }" + System.Environment.NewLine);
					with_2.Append("   }" + System.Environment.NewLine);
					with_2.Append("</script>" + System.Environment.NewLine);
					
					clientScript = with_2.ToString();
					break;
				case ClientSideScript.Show: // Client-side script for Show.
					System.Text.StringBuilder with_3 = new StringBuilder();
					with_3.Append("<script language=\"javascript\">" + System.Environment.NewLine);
					with_3.Append("   function Show(url, height, width, left, top, center, help, resizable, status)" + System.Environment.NewLine);
					with_3.Append("   {" + System.Environment.NewLine);
					with_3.Append("       window.showModelessDialog(url, window.self,\'dialogWidth:\' + width + \'px;dialogHeight:\' + height + \'px;left:\' + left + \';top:\' + top + \';center:\' + center + \';help:\' + help + \';resizable:\' + resizable + \';status:\' + status);" + System.Environment.NewLine);
					with_3.Append("       return false;" + System.Environment.NewLine);
					with_3.Append("   }" + System.Environment.NewLine);
					with_3.Append("</script>" + System.Environment.NewLine);
					
					clientScript = with_3.ToString();
					break;
				case ClientSideScript.ShowDialog: //Client-side script for ShowDialog.
					System.Text.StringBuilder with_4 = new StringBuilder();
					with_4.Append("<script language=\"javascript\">" + System.Environment.NewLine);
					with_4.Append("   function ShowDialog(url, resultHolderId, height, width, left, top, center, help, resizable, status)" + System.Environment.NewLine);
					with_4.Append("   {" + System.Environment.NewLine);
					with_4.Append("       returnValue = window.showModalDialog(url, window.self,\'dialogWidth:\' + width + \'px;dialogHeight:\' + height + \'px;left:\' + left + \';top:\' + top + \';center:\' + center + \';help:\' + help + \';resizable:\' + resizable + \';status:\' + status);" + System.Environment.NewLine);
					with_4.Append("       if (returnValue != null)" + System.Environment.NewLine);
					with_4.Append("       {" + System.Environment.NewLine);
					with_4.Append("           if ((resultHolderId != null) && (document.getElementById(resultHolderId) != null))" + System.Environment.NewLine);
					with_4.Append("           {" + System.Environment.NewLine);
					with_4.Append("               document.getElementById(resultHolderId).value = returnValue;" + System.Environment.NewLine);
					with_4.Append("               return false;" + System.Environment.NewLine);
					with_4.Append("           }" + System.Environment.NewLine);
					with_4.Append("           else" + System.Environment.NewLine);
					with_4.Append("           {" + System.Environment.NewLine);
					with_4.Append("               return true;" + System.Environment.NewLine);
					with_4.Append("           }" + System.Environment.NewLine);
					with_4.Append("       }" + System.Environment.NewLine);
					with_4.Append("       else" + System.Environment.NewLine);
					with_4.Append("       {" + System.Environment.NewLine);
					with_4.Append("           return false;" + System.Environment.NewLine);
					with_4.Append("       }" + System.Environment.NewLine);
					with_4.Append("   }" + System.Environment.NewLine);
					with_4.Append("</script>" + System.Environment.NewLine);
					
					clientScript = with_4.ToString();
					break;
				case ClientSideScript.ShowPopup: // Client-side script for ShowPopup.
					System.Text.StringBuilder with_5 = new StringBuilder();
					with_5.Append("<script language=\"javascript\">" + System.Environment.NewLine);
					with_5.Append("   function ShowPopup(url, height, width, left, top, center, resizable, scrollbars, toolbar, menubar, location, status, directories)" + System.Environment.NewLine);
					with_5.Append("   {" + System.Environment.NewLine);
					with_5.Append("       specialCharacters = /([^a-zA-Z0-9\\s])/gi;" + System.Environment.NewLine);
					with_5.Append("       popupName = url.replace(specialCharacters, \'\');" + System.Environment.NewLine);
					with_5.Append("       if (center)" + System.Environment.NewLine);
					with_5.Append("       {" + System.Environment.NewLine);
					with_5.Append("           popup = window.open(url, popupName, \'height=\' + height + \',width=\' + width + \',top=\' + ((screen.availHeight / 2) - (height / 2)) + \',left=\' + ((screen.availWidth / 2) - (width / 2)) + \',resizable=\' + resizable + \',scrollbars=\' + scrollbars + \',toolbar=\' + toolbar + \',menubar=\' + menubar + \',location=\' + location + \',status=\' + status + \',directories=\' + directories);" + System.Environment.NewLine);
					with_5.Append("       }" + System.Environment.NewLine);
					with_5.Append("       else" + System.Environment.NewLine);
					with_5.Append("       {" + System.Environment.NewLine);
					with_5.Append("            popup = window.open(url, popupName, \'height=\' + height + \',width=\' + width + \',top=\' + top + \',left=\' + left + \',resizable=\' + resizable + \',scrollbars=\' + scrollbars + \',toolbar=\' + toolbar + \',menubar=\' + menubar + \',location=\' + location + \',status=\' + status + \',directories=\' + directories);" + System.Environment.NewLine);
					with_5.Append("       }" + System.Environment.NewLine);
					with_5.Append("       popup.focus();" + System.Environment.NewLine);
					with_5.Append("       return false;" + System.Environment.NewLine);
					with_5.Append("   }" + System.Environment.NewLine);
					with_5.Append("</script>" + System.Environment.NewLine);
					
					clientScript = with_5.ToString();
					break;
				case ClientSideScript.Close: // Client-side script for Close.
					System.Text.StringBuilder with_6 = new StringBuilder();
					with_6.Append("<script language=\"javascript\">" + System.Environment.NewLine);
					with_6.Append("   function Close(returnValue)" + System.Environment.NewLine);
					with_6.Append("   {" + System.Environment.NewLine);
					with_6.Append("       if (returnValue == \'\')" + System.Environment.NewLine);
					with_6.Append("       {" + System.Environment.NewLine);
					with_6.Append("           returnValue = null;" + System.Environment.NewLine);
					with_6.Append("       }" + System.Environment.NewLine);
					with_6.Append("       window.returnValue = returnValue;" + System.Environment.NewLine);
					with_6.Append("       window.close();" + System.Environment.NewLine);
					with_6.Append("       return false;" + System.Environment.NewLine);
					with_6.Append("   }" + System.Environment.NewLine);
					with_6.Append("</script>" + System.Environment.NewLine);
					
					clientScript = with_6.ToString();
					break;
				case ClientSideScript.MsgBox: // Client-side script for MsgBox.
					System.Text.StringBuilder with_7 = new StringBuilder();
					with_7.Append("<script language=\"vbscript\">" + System.Environment.NewLine);
					with_7.Append("   Function ShowMsgBox(prompt, title, buttons, doPostBack)" + System.Environment.NewLine);
					with_7.Append("       result = MsgBox(prompt, buttons ,title)" + System.Environment.NewLine);
					with_7.Append("       If doPostBack Then" + System.Environment.NewLine);
					with_7.Append("           If (result = vbOK) Or (result = vbRetry) Or (result = vbYes) Then" + System.Environment.NewLine);
					with_7.Append("               ShowMsgBox = True" + System.Environment.NewLine);
					with_7.Append("           Else" + System.Environment.NewLine);
					with_7.Append("               ShowMsgBox = False" + System.Environment.NewLine);
					with_7.Append("           End If" + System.Environment.NewLine);
					with_7.Append("       Else" + System.Environment.NewLine);
					with_7.Append("           ShowMsgBox = False" + System.Environment.NewLine);
					with_7.Append("       End If" + System.Environment.NewLine);
					with_7.Append("   End Function" + System.Environment.NewLine);
					with_7.Append("</script>" + System.Environment.NewLine);
					
					clientScript = with_7.ToString();
					break;
				case ClientSideScript.Refresh: // Client-side script for Refresh.
					System.Text.StringBuilder with_8 = new StringBuilder();
					with_8.Append("<script language=\"javascript\">" + System.Environment.NewLine);
					with_8.Append("   function Refresh()" + System.Environment.NewLine);
					with_8.Append("   {" + System.Environment.NewLine);
					with_8.Append("       window.location.href = unescape(window.location.pathname);" + System.Environment.NewLine);
					with_8.Append("       return false;" + System.Environment.NewLine);
					with_8.Append("   }" + System.Environment.NewLine);
					with_8.Append("</script>" + System.Environment.NewLine);
					
					clientScript = with_8.ToString();
					break;
				case ClientSideScript.Maximize: // Client-side script for Maximize.
					System.Text.StringBuilder with_9 = new StringBuilder();
					with_9.Append("<script language=\"javascript\">" + System.Environment.NewLine);
					with_9.Append("   function Maximize()" + System.Environment.NewLine);
					with_9.Append("   {" + System.Environment.NewLine);
					with_9.Append("       window.moveTo(0, 0);" + System.Environment.NewLine);
					with_9.Append("       window.resizeTo(window.screen.availWidth, window.screen.availHeight);" + System.Environment.NewLine);
					with_9.Append("       return false;" + System.Environment.NewLine);
					with_9.Append("   }" + System.Environment.NewLine);
					with_9.Append("</script>" + System.Environment.NewLine);
					
					clientScript = with_9.ToString();
					break;
				case ClientSideScript.Minimize: // Client-side script for Minimize.
					System.Text.StringBuilder with_10 = new StringBuilder();
					with_10.Append("<script language=\"javascript\">" + System.Environment.NewLine);
					with_10.Append("   function Minimize()" + System.Environment.NewLine);
					with_10.Append("   {" + System.Environment.NewLine);
					with_10.Append("       window.blur();" + System.Environment.NewLine);
					with_10.Append("       return false;" + System.Environment.NewLine);
					with_10.Append("   }" + System.Environment.NewLine);
					with_10.Append("</script>" + System.Environment.NewLine);
					
					clientScript = with_10.ToString();
					break;
				case ClientSideScript.RunClientExe: // Client-side script for RunClientExe.
					System.Text.StringBuilder with_11 = new StringBuilder();
					with_11.Append("<script language=\"vbscript\">" + System.Environment.NewLine);
					with_11.Append("   Function RunClientExe(exeToRun)" + System.Environment.NewLine);
					with_11.Append("       On Error Resume Next" + System.Environment.NewLine);
					with_11.Append("       Set shell = CreateObject(\"WScript.Shell\")" + System.Environment.NewLine);
					with_11.Append("       returnCode = shell.Run(exeToRun)" + System.Environment.NewLine);
					with_11.Append("       Set shell = Nothing" + System.Environment.NewLine);
					with_11.Append("       If Err.number <> 0 Then" + System.Environment.NewLine);
					with_11.Append("           result = MsgBox(\"Failed to execute \" & exeToRun & \".\", 16, \"RunClientExe\")" + System.Environment.NewLine);
					with_11.Append("           RunClientExe = True" + System.Environment.NewLine);
					with_11.Append("       Else" + System.Environment.NewLine);
					with_11.Append("           RunClientExe = False" + System.Environment.NewLine);
					with_11.Append("       End If" + System.Environment.NewLine);
					with_11.Append("   End Function" + System.Environment.NewLine);
					with_11.Append("</script>" + System.Environment.NewLine);
					
					clientScript = with_11.ToString();
					break;
			}
			return clientScript;
			
		}
		
		/// <summary>
		/// Associates client-side script to a web control.
		/// </summary>
		/// <param name="control">The control to which the client-side script is to be associated.</param>
		/// <param name="script">The script that is to be associated with the control.</param>
		/// <param name="attribute">Attribute of the control through which the client-side script is to be associated with the control.</param>
		/// <remarks></remarks>
		private static void HookupScriptToControl(System.Web.UI.Control control, string script, string attribute)
		{
			
			if (control is System.Web.UI.WebControls.Button)
			{
				((System.Web.UI.WebControls.Button) control).Attributes.Add(attribute, script);
			}
			else if (control is System.Web.UI.WebControls.LinkButton)
			{
				((System.Web.UI.WebControls.LinkButton) control).Attributes.Add(attribute, script);
			}
			else if (control is System.Web.UI.WebControls.ImageButton)
			{
				((System.Web.UI.WebControls.ImageButton) control).Attributes.Add(attribute, script);
			}
			else if (control is System.Web.UI.WebControls.Image)
			{
				((System.Web.UI.WebControls.Image) control).Attributes.Add(attribute, script);
			}
			else if (control is System.Web.UI.WebControls.Label)
			{
				((System.Web.UI.WebControls.Label) control).Attributes.Add(attribute, script);
			}
			else if (control is System.Web.UI.WebControls.TextBox)
			{
				((System.Web.UI.WebControls.TextBox) control).Attributes.Add(attribute, script);
			}
			
		}
		
		#endregion
		
	}
	
}
