//*******************************************************************************************************
//  ClientSideExtensions.cs - Gbtc
//
//  Tennessee Valley Authority, 2010
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  12/22/2010 - Pinal C. Patel
//       Generated original version of source code.
//  06/15/2011 - Pinal C. Patel
//       Added Refresh(), Show(), ShowDialog(), ShowPopup(), Close() and MsgBox() client-side extensions.
//
//*******************************************************************************************************

#region [ TVA Open Source Agreement ]
/*

 THIS OPEN SOURCE AGREEMENT ("AGREEMENT") DEFINES THE RIGHTS OF USE,REPRODUCTION, DISTRIBUTION,
 MODIFICATION AND REDISTRIBUTION OF CERTAIN COMPUTER SOFTWARE ORIGINALLY RELEASED BY THE
 TENNESSEE VALLEY AUTHORITY, A CORPORATE AGENCY AND INSTRUMENTALITY OF THE UNITED STATES GOVERNMENT
 ("GOVERNMENT AGENCY"). GOVERNMENT AGENCY IS AN INTENDED THIRD-PARTY BENEFICIARY OF ALL SUBSEQUENT
 DISTRIBUTIONS OR REDISTRIBUTIONS OF THE SUBJECT SOFTWARE. ANYONE WHO USES, REPRODUCES, DISTRIBUTES,
 MODIFIES OR REDISTRIBUTES THE SUBJECT SOFTWARE, AS DEFINED HEREIN, OR ANY PART THEREOF, IS, BY THAT
 ACTION, ACCEPTING IN FULL THE RESPONSIBILITIES AND OBLIGATIONS CONTAINED IN THIS AGREEMENT.

 Original Software Designation: openPDC
 Original Software Title: The GSF Open Source Phasor Data Concentrator
 User Registration Requested. Please Visit https://naspi.tva.com/Registration/
 Point of Contact for Original Software: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>

 1. DEFINITIONS

 A. "Contributor" means Government Agency, as the developer of the Original Software, and any entity
 that makes a Modification.

 B. "Covered Patents" mean patent claims licensable by a Contributor that are necessarily infringed by
 the use or sale of its Modification alone or when combined with the Subject Software.

 C. "Display" means the showing of a copy of the Subject Software, either directly or by means of an
 image, or any other device.

 D. "Distribution" means conveyance or transfer of the Subject Software, regardless of means, to
 another.

 E. "Larger Work" means computer software that combines Subject Software, or portions thereof, with
 software separate from the Subject Software that is not governed by the terms of this Agreement.

 F. "Modification" means any alteration of, including addition to or deletion from, the substance or
 structure of either the Original Software or Subject Software, and includes derivative works, as that
 term is defined in the Copyright Statute, 17 USC § 101. However, the act of including Subject Software
 as part of a Larger Work does not in and of itself constitute a Modification.

 G. "Original Software" means the computer software first released under this Agreement by Government
 Agency entitled openPDC, including source code, object code and accompanying documentation, if any.

 H. "Recipient" means anyone who acquires the Subject Software under this Agreement, including all
 Contributors.

 I. "Redistribution" means Distribution of the Subject Software after a Modification has been made.

 J. "Reproduction" means the making of a counterpart, image or copy of the Subject Software.

 K. "Sale" means the exchange of the Subject Software for money or equivalent value.

 L. "Subject Software" means the Original Software, Modifications, or any respective parts thereof.

 M. "Use" means the application or employment of the Subject Software for any purpose.

 2. GRANT OF RIGHTS

 A. Under Non-Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor,
 with respect to its own contribution to the Subject Software, hereby grants to each Recipient a
 non-exclusive, world-wide, royalty-free license to engage in the following activities pertaining to
 the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Modification

 5. Redistribution

 6. Display

 B. Under Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor, with
 respect to its own contribution to the Subject Software, hereby grants to each Recipient under Covered
 Patents a non-exclusive, world-wide, royalty-free license to engage in the following activities
 pertaining to the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Sale

 5. Offer for Sale

 C. The rights granted under Paragraph B. also apply to the combination of a Contributor's Modification
 and the Subject Software if, at the time the Modification is added by the Contributor, the addition of
 such Modification causes the combination to be covered by the Covered Patents. It does not apply to
 any other combinations that include a Modification. 

 D. The rights granted in Paragraphs A. and B. allow the Recipient to sublicense those same rights.
 Such sublicense must be under the same terms and conditions of this Agreement.

 3. OBLIGATIONS OF RECIPIENT

 A. Distribution or Redistribution of the Subject Software must be made under this Agreement except for
 additions covered under paragraph 3H. 

 1. Whenever a Recipient distributes or redistributes the Subject Software, a copy of this Agreement
 must be included with each copy of the Subject Software; and

 2. If Recipient distributes or redistributes the Subject Software in any form other than source code,
 Recipient must also make the source code freely available, and must provide with each copy of the
 Subject Software information on how to obtain the source code in a reasonable manner on or through a
 medium customarily used for software exchange.

 B. Each Recipient must ensure that the following copyright notice appears prominently in the Subject
 Software:

          No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.

 C. Each Contributor must characterize its alteration of the Subject Software as a Modification and
 must identify itself as the originator of its Modification in a manner that reasonably allows
 subsequent Recipients to identify the originator of the Modification. In fulfillment of these
 requirements, Contributor must include a file (e.g., a change log file) that describes the alterations
 made and the date of the alterations, identifies Contributor as originator of the alterations, and
 consents to characterization of the alterations as a Modification, for example, by including a
 statement that the Modification is derived, directly or indirectly, from Original Software provided by
 Government Agency. Once consent is granted, it may not thereafter be revoked.

 D. A Contributor may add its own copyright notice to the Subject Software. Once a copyright notice has
 been added to the Subject Software, a Recipient may not remove it without the express permission of
 the Contributor who added the notice.

 E. A Recipient may not make any representation in the Subject Software or in any promotional,
 advertising or other material that may be construed as an endorsement by Government Agency or by any
 prior Recipient of any product or service provided by Recipient, or that may seek to obtain commercial
 advantage by the fact of Government Agency's or a prior Recipient's participation in this Agreement.

 F. In an effort to track usage and maintain accurate records of the Subject Software, each Recipient,
 upon receipt of the Subject Software, is requested to register with Government Agency by visiting the
 following website: https://naspi.tva.com/Registration/. Recipient's name and personal information
 shall be used for statistical purposes only. Once a Recipient makes a Modification available, it is
 requested that the Recipient inform Government Agency at the web site provided above how to access the
 Modification.

 G. Each Contributor represents that that its Modification does not violate any existing agreements,
 regulations, statutes or rules, and further that Contributor has sufficient rights to grant the rights
 conveyed by this Agreement.

 H. A Recipient may choose to offer, and to charge a fee for, warranty, support, indemnity and/or
 liability obligations to one or more other Recipients of the Subject Software. A Recipient may do so,
 however, only on its own behalf and not on behalf of Government Agency or any other Recipient. Such a
 Recipient must make it absolutely clear that any such warranty, support, indemnity and/or liability
 obligation is offered by that Recipient alone. Further, such Recipient agrees to indemnify Government
 Agency and every other Recipient for any liability incurred by them as a result of warranty, support,
 indemnity and/or liability offered by such Recipient.

 I. A Recipient may create a Larger Work by combining Subject Software with separate software not
 governed by the terms of this agreement and distribute the Larger Work as a single product. In such
 case, the Recipient must make sure Subject Software, or portions thereof, included in the Larger Work
 is subject to this Agreement.

 J. Notwithstanding any provisions contained herein, Recipient is hereby put on notice that export of
 any goods or technical data from the United States may require some form of export license from the
 U.S. Government. Failure to obtain necessary export licenses may result in criminal liability under
 U.S. laws. Government Agency neither represents that a license shall not be required nor that, if
 required, it shall be issued. Nothing granted herein provides any such export license.

 4. DISCLAIMER OF WARRANTIES AND LIABILITIES; WAIVER AND INDEMNIFICATION

 A. No Warranty: THE SUBJECT SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTY OF ANY KIND, EITHER
 EXPRESSED, IMPLIED, OR STATUTORY, INCLUDING, BUT NOT LIMITED TO, ANY WARRANTY THAT THE SUBJECT
 SOFTWARE WILL CONFORM TO SPECIFICATIONS, ANY IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 PARTICULAR PURPOSE, OR FREEDOM FROM INFRINGEMENT, ANY WARRANTY THAT THE SUBJECT SOFTWARE WILL BE ERROR
 FREE, OR ANY WARRANTY THAT DOCUMENTATION, IF PROVIDED, WILL CONFORM TO THE SUBJECT SOFTWARE. THIS
 AGREEMENT DOES NOT, IN ANY MANNER, CONSTITUTE AN ENDORSEMENT BY GOVERNMENT AGENCY OR ANY PRIOR
 RECIPIENT OF ANY RESULTS, RESULTING DESIGNS, HARDWARE, SOFTWARE PRODUCTS OR ANY OTHER APPLICATIONS
 RESULTING FROM USE OF THE SUBJECT SOFTWARE. FURTHER, GOVERNMENT AGENCY DISCLAIMS ALL WARRANTIES AND
 LIABILITIES REGARDING THIRD-PARTY SOFTWARE, IF PRESENT IN THE ORIGINAL SOFTWARE, AND DISTRIBUTES IT
 "AS IS."

 B. Waiver and Indemnity: RECIPIENT AGREES TO WAIVE ANY AND ALL CLAIMS AGAINST GOVERNMENT AGENCY, ITS
 AGENTS, EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT. IF RECIPIENT'S USE
 OF THE SUBJECT SOFTWARE RESULTS IN ANY LIABILITIES, DEMANDS, DAMAGES, EXPENSES OR LOSSES ARISING FROM
 SUCH USE, INCLUDING ANY DAMAGES FROM PRODUCTS BASED ON, OR RESULTING FROM, RECIPIENT'S USE OF THE
 SUBJECT SOFTWARE, RECIPIENT SHALL INDEMNIFY AND HOLD HARMLESS  GOVERNMENT AGENCY, ITS AGENTS,
 EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT, TO THE EXTENT PERMITTED BY
 LAW.  THE FOREGOING RELEASE AND INDEMNIFICATION SHALL APPLY EVEN IF THE LIABILITIES, DEMANDS, DAMAGES,
 EXPENSES OR LOSSES ARE CAUSED, OCCASIONED, OR CONTRIBUTED TO BY THE NEGLIGENCE, SOLE OR CONCURRENT, OF
 GOVERNMENT AGENCY OR ANY PRIOR RECIPIENT.  RECIPIENT'S SOLE REMEDY FOR ANY SUCH MATTER SHALL BE THE
 IMMEDIATE, UNILATERAL TERMINATION OF THIS AGREEMENT.

 5. GENERAL TERMS

 A. Termination: This Agreement and the rights granted hereunder will terminate automatically if a
 Recipient fails to comply with these terms and conditions, and fails to cure such noncompliance within
 thirty (30) days of becoming aware of such noncompliance. Upon termination, a Recipient agrees to
 immediately cease use and distribution of the Subject Software. All sublicenses to the Subject
 Software properly granted by the breaching Recipient shall survive any such termination of this
 Agreement.

 B. Severability: If any provision of this Agreement is invalid or unenforceable under applicable law,
 it shall not affect the validity or enforceability of the remainder of the terms of this Agreement.

 C. Applicable Law: This Agreement shall be subject to United States federal law only for all purposes,
 including, but not limited to, determining the validity of this Agreement, the meaning of its
 provisions and the rights, obligations and remedies of the parties.

 D. Entire Understanding: This Agreement constitutes the entire understanding and agreement of the
 parties relating to release of the Subject Software and may not be superseded, modified or amended
 except by further written agreement duly executed by the parties.

 E. Binding Authority: By accepting and using the Subject Software under this Agreement, a Recipient
 affirms its authority to bind the Recipient to all terms and conditions of this Agreement and that
 Recipient hereby agrees to all terms and conditions herein.

 F. Point of Contact: Any Recipient contact with Government Agency is to be directed to the designated
 representative as follows: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>.

*/
#endregion

using System;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace GSF.Web.UI
{
    #region [ Enumerations ]

    /// <summary>
    /// Specifies the buttons to be displayed in a message box.
    /// </summary>
    [Flags()]
    public enum MsgBoxStyle
    {
        /// <summary>
        /// OK Button.
        /// </summary>
        OKOnly = 0,
        /// <summary>
        /// OK and Cancel buttons.
        /// </summary>
        OKCancel = 1,
        /// <summary>
        /// Abort,Retry, and Ignore buttons.
        /// </summary>
        AbortRetryIgnore = 2,
        /// <summary>
        /// Yes, No, and Cancel buttons.
        /// </summary>
        YesNoCancel = 3,
        /// <summary>
        /// Yes and No buttons.
        /// </summary>
        YesNo = 4,
        /// <summary>
        /// Retry and Cancel buttons.
        /// </summary>
        RetryCancel = 5,
        /// <summary>
        /// Critical message.
        /// </summary>
        Critical = 16,
        /// <summary>
        /// Warning query.
        /// </summary>
        Question = 32,
        /// <summary>
        /// Warning message.
        /// </summary>
        Exclamation = 48,
        /// <summary>
        /// Information message.
        /// </summary>
        Information = 64,
        /// <summary>
        /// First button is default.
        /// </summary>
        DefaultButton1 = 0,
        /// <summary>
        /// Second button is default.
        /// </summary>
        DefaultButton2 = 256,
        /// <summary>
        /// Third button is default.
        /// </summary>
        DefaultButton3 = 512,
        /// <summary>
        /// Fourth button is default.
        /// </summary>
        DefaultButton4 = 768,
        /// <summary>
        /// Application modal message box.
        /// </summary>
        ApplicationModal = 0,
        /// <summary>
        /// System modal message box.
        /// </summary>
        SystemModal = 4096,
    }

    #endregion

    /// <summary>
    /// Defines javascript client-side extensions for web controls.
    /// </summary>
    public static class ClientSideExtensions
    {
        /// <summary>
        /// Performs JavaScript encoding on given string.
        /// </summary>
        /// <param name="text">The string to be encoded.</param>
        public static string JavaScriptEncode(ref string text)
        {
            text = text.Replace("\\", "\\\\");
            text = text.Replace("'", "\\'");
            text = text.Replace("\"", "\\\"");
            text = text.Replace(Convert.ToChar(8), '\b');
            text = text.Replace(Convert.ToChar(9), '\t');
            text = text.Replace(Convert.ToChar(10), '\r');
            text = text.Replace(Convert.ToChar(12), '\f');
            text = text.Replace(Convert.ToChar(13), '\n');

            return text;
        }

        /// <summary>
        /// Decodes JavaScript characters from given string.
        /// </summary>
        /// <param name="text">The string to be decoded.</param>
        public static string JavaScriptDecode(ref string text)
        {
            text = text.Replace("\\\\", "\\");
            text = text.Replace("\\'", "'");
            text = text.Replace("\\\"", "\"");
            text = text.Replace('\b', Convert.ToChar(8));
            text = text.Replace('\t', Convert.ToChar(9));
            text = text.Replace('\r', Convert.ToChar(10));
            text = text.Replace('\f', Convert.ToChar(12));
            text = text.Replace('\n', Convert.ToChar(13));

            return text;
        }

        /// <summary>
        /// Registers a default button to be activated when the ENTER key is pressed inside this <paramref name="textbox"/>.
        /// </summary>
        /// <param name="textbox"><see cref="TextBox"/> for which the default button is to be registered.</param>
        /// <param name="control">Button <see cref="Control"/> to be assigned as the default action handler.</param>
        public static void SetDefaultButton(this TextBox textbox, Control control)
        {
            if (!textbox.Page.ClientScript.IsClientScriptBlockRegistered("DefaultButton"))
            {
                StringBuilder script = new StringBuilder();
                script.Append("<script language=\"javascript\">\r\n");
                script.Append("   function DefaultButton(e, controlId)\r\n");
                script.Append("   {\r\n");
                script.Append("       if (e.keyCode == 13 || e.charCode == 13)\r\n");
                script.Append("       {\r\n");
                script.Append("           e.returnValue = false;\r\n");
                script.Append("           e.cancel = true;\r\n");
                script.Append("           if (document.getElementById(controlId) != null)\r\n");
                script.Append("           {\r\n");
                script.Append("               document.getElementById(controlId).click();\r\n");
                script.Append("           }\r\n");
                script.Append("       }\r\n");
                script.Append("   }\r\n");
                script.Append("</script>\r\n");

                textbox.Page.ClientScript.RegisterClientScriptBlock(textbox.Page.GetType(), "DefaultButton", script.ToString());
            }

            textbox.Attributes.Add("OnKeyDown", "javascript:DefaultButton(event, \'" + control.ClientID + "\')");
        }

        /// <summary>
        /// Registers the <paramref name="button"/> to be disabled after one click to prevent multiple postbacks.
        /// </summary>
        /// <param name="button"><see cref="Button"/> for which submit once script is to be registered.</param>
        public static void SetSubmitOnce(this Button button)
        {
            StringBuilder script = new StringBuilder();
            if (button.CausesValidation)
            {
                script.Append("if (typeof(Page_ClientValidate) == \'function\') {");
                script.Append("if (Page_ClientValidate() == false) { return false; }}");
            }
            script.Append("this.disabled = true;");
            script.AppendFormat("{0};", button.Page.ClientScript.GetPostBackEventReference(button, null));

            button.OnClientClick = script.ToString();
        }

        /// <summary>
        /// Brings the <paramref name="page"/>'s browser window to the foreground.
        /// </summary>
        /// <param name="page"><see cref="Page"/> to be brought to the foreground.</param>
        public static void BringToFront(this Page page)
        {
            if (!page.ClientScript.IsStartupScriptRegistered("BringToFront"))
            {
                StringBuilder script = new StringBuilder();
                script.Append("<script language=\"javascript\">\r\n");
                script.Append("   window.focus();\r\n");
                script.Append("</script>\r\n");

                page.ClientScript.RegisterStartupScript(page.GetType(), "BringToFront", script.ToString());
            }
        }

        /// <summary>
        /// Pushes the <paramref name="page"/>'s browser window to the background.
        /// </summary>
        /// <param name="page"><see cref="Page"/> to be pushed to the background.</param>
        public static void PushToBack(this Page page)
        {
            if (!page.ClientScript.IsStartupScriptRegistered("PushToBack"))
            {
                StringBuilder script = new StringBuilder();
                script.Append("<script language=\"javascript\">\r\n");
                script.Append("   window.blur();\r\n");
                script.Append("</script>\r\n");

                page.ClientScript.RegisterStartupScript(page.GetType(), "PushToBack", script.ToString());
            }
        }

        /// <summary>
        /// Registers this <paramref name="page"/> to perform a page refresh.
        /// </summary>
        /// <param name="page"><see cref="Page"/> to refresh.</param>
        /// <param name="refreshOnLoad">Refreshes the <paramref name="page"/> after the page content has loaded.</param>
        public static void Refresh(this Page page, bool refreshOnLoad = false)
        {
            if (!page.ClientScript.IsClientScriptBlockRegistered("Refresh"))
                page.ClientScript.RegisterClientScriptBlock(page.GetType(), "Refresh", GetRefreshScript());

            if (refreshOnLoad)
            {
                // Refresh page after page load.
                if (!page.ClientScript.IsStartupScriptRegistered("RefreshPostLoad"))
                {
                    StringBuilder script = new StringBuilder();
                    script.Append("<script language=\"javascript\">\r\n");
                    script.Append("   Refresh();\r\n");
                    script.Append("</script>\r\n");

                    page.ClientScript.RegisterStartupScript(page.GetType(), "RefreshPostLoad", script.ToString());
                }
            }
            else
            {
                // Refresh page before page load.
                if (!page.ClientScript.IsClientScriptBlockRegistered("RefreshPreLoad"))
                {
                    StringBuilder script = new StringBuilder();
                    script.Append("<script language=\"javascript\">\r\n");
                    script.Append("   Refresh();\r\n");
                    script.Append("</script>\r\n");

                    page.ClientScript.RegisterClientScriptBlock(page.GetType(), "RefreshPreLoad", script.ToString());
                }
            }
        }

        /// <summary>
        /// Registers the <paramref name="control"/> to perform a page refresh.
        /// </summary>
        /// <param name="control"><see cref="Control"/> that will perform a page refresh when clicked.</param>
        public static void Refresh(this Control control)
        {
            if (!control.Page.ClientScript.IsClientScriptBlockRegistered("Refresh"))
                control.Page.ClientScript.RegisterClientScriptBlock(control.Page.GetType(), "Refresh", GetRefreshScript());

            HookupScriptToControl(control, "javascript:return(Refresh())", "OnClick");
        }

        /// <summary>
        /// Shows a modeless popup window for the specified web page <paramref name="url"/> when the <paramref name="page"/> has loaded.
        /// </summary>
        /// <param name="page"><see name="Page"/> that will show the popup window when loaded.</param>
        /// <param name="url">Web page url for which the popup windows is to be shown.</param>
        /// <param name="height">Height of the popup window.</param>
        /// <param name="width">Width of the popup window.</param>
        /// <param name="left">Location of the popup window on X-axis.</param>
        /// <param name="top">Location of the popup window on Y-axis.</param>
        /// <param name="center">True if the popup window is to be centered, otherwise False.</param>
        /// <param name="help">True if help button is to be displayed, otherwise False.</param>
        /// <param name="resizable">True if the popup window can be resized, otherwise False.</param>
        /// <param name="status">True if the status bar is to be displayed, otherwise False.</param>
        public static void Show(this Page page, string url, int height = 400, int width = 600, int left = 0, int top = 0, bool center = true, bool help = false, bool resizable = false, bool status = false)
        {
            if (!page.ClientScript.IsClientScriptBlockRegistered("Show"))
                page.ClientScript.RegisterClientScriptBlock(page.GetType(), "Show", GetShowScript());

            if (!page.ClientScript.IsStartupScriptRegistered("Show." + url))
            {
                StringBuilder script = new StringBuilder();
                script.Append("<script language=\"javascript\">\r\n");
                script.Append("   Show('" + url + "', " + height + ", " + width + ", " + left + ", " + top + ", " +
                                            Math.Abs(Convert.ToInt32(center)) + ", " + Math.Abs(Convert.ToInt32(help)) + ", " +
                                            Math.Abs(Convert.ToInt32(resizable)) + ", " + Math.Abs(Convert.ToInt32(status)) + ");\r\n");
                script.Append("</script>\r\n");

                page.ClientScript.RegisterStartupScript(page.GetType(), "Show." + url, script.ToString());
            }
        }

        /// <summary>
        /// Shows a modeless popup window for the specified web page <paramref name="url"/> when the <paramref name="control"/> is clicked.
        /// </summary>
        /// <param name="control"><see name="Control"/> that will show the popup window when clicked.</param>
        /// <param name="url">Web page url for which the popup windows is to be shown.</param>
        /// <param name="height">Height of the popup window.</param>
        /// <param name="width">Width of the popup window.</param>
        /// <param name="left">Location of the popup window on X-axis.</param>
        /// <param name="top">Location of the popup window on Y-axis.</param>
        /// <param name="center">True if the popup window is to be centered, otherwise False.</param>
        /// <param name="help">True if help button is to be displayed, otherwise False.</param>
        /// <param name="resizable">True if the popup window can be resized, otherwise False.</param>
        /// <param name="status">True if the status bar is to be displayed, otherwise False.</param>
        public static void Show(this Control control, string url, int height = 400, int width = 600, int left = 0, int top = 0, bool center = true, bool help = false, bool resizable = false, bool status = false)
        {
            if (!control.Page.ClientScript.IsClientScriptBlockRegistered("Show"))
                control.Page.ClientScript.RegisterClientScriptBlock(control.Page.GetType(), "Show", GetShowScript());

            HookupScriptToControl(control, "javascript:return(Show('" + url + "', " + height + ", " + width + ", " + left + ", " +
                                            top + ", " + Math.Abs(Convert.ToInt32(center)) + ", " + Math.Abs(Convert.ToInt32(help)) + ", " +
                                            Math.Abs(Convert.ToInt32(resizable)) + ", " + Math.Abs(Convert.ToInt32(status)) + "))", "OnClick");
        }

        /// <summary>
        /// Shows a modal popup window for the specified web page <paramref name="url"/> when the <paramref name="page"/> has loaded.
        /// </summary>
        /// <param name="page">Web page that will show the popup window when loaded.</param>
        /// <param name="url">Web page url for which the popup windows is to be shown.</param>
        /// <param name="dialogResultHolder"><see cref="Control"/> whose text is to updated with the text returned by the web page displayed in the popup window.</param>
        /// <param name="height">Height of the popup window.</param>
        /// <param name="width">Width of the popup window.</param>
        /// <param name="left">Location of the popup window on X-axis.</param>
        /// <param name="top">Location of the popup window on Y-axis.</param>
        /// <param name="center">True if the popup window is to be centered, otherwise False.</param>
        /// <param name="help">True if help button is to be displayed, otherwise False.</param>
        /// <param name="resizable">True if the popup window can be resized, otherwise False.</param>
        /// <param name="status">True if the status bar is to be displayed, otherwise False.</param>
        public static void ShowDialog(this Page page, string url, Control dialogResultHolder = null, int height = 400, int width = 600, int left = 0, int top = 0, bool center = true, bool help = false, bool resizable = false, bool status = false)
        {
            if (!page.ClientScript.IsClientScriptBlockRegistered("ShowDialog"))
                page.ClientScript.RegisterClientScriptBlock(page.GetType(), "ShowDialog", GetShowDialogScript());

            if (!page.ClientScript.IsStartupScriptRegistered("ShowDialog." + url))
            {
                StringBuilder script = new StringBuilder();
                script.Append("<input type=\"hidden\" name=\"TVA_EVENT_TARGET\" value=\"\" />\r\n");
                script.Append("<input type=\"hidden\" name=\"TVA_EVENT_ARGUMENT\" value=\"\" />\r\n");
                script.Append("<script language=\"javascript\">\r\n");
                if (dialogResultHolder != null)
                {
                    script.Append("   ShowDialog('" + url + "', '" + dialogResultHolder.ClientID + "', " + height + ", " + width + ", " +
                                                    left + ", " + top + ", " + Math.Abs(Convert.ToInt32(center)) + ", " + Math.Abs(Convert.ToInt32(help)) + ", " +
                                                    Math.Abs(Convert.ToInt32(resizable)) + ", " + Math.Abs(Convert.ToInt32(status)) + ");\r\n");
                }
                else
                {
                    script.Append("   if (ShowDialog('" + url + "', null, " + height + ", " + width + ", " + left + ", " + top + ", " +
                                                        Math.Abs(Convert.ToInt32(center)) + ", " + Math.Abs(Convert.ToInt32(help)) + ", " +
                                                        Math.Abs(Convert.ToInt32(resizable)) + ", " + Math.Abs(Convert.ToInt32(status)) + "))\r\n");
                    script.Append("   {\r\n");
                    foreach (Control control in page.Controls)
                    {
                        if (control is HtmlForm)
                        {
                            script.Append("       " + control.ClientID + ".TVA_EVENT_TARGET.value = 'ShowDialog';\r\n");
                            script.Append("       " + control.ClientID + ".TVA_EVENT_ARGUMENT.value = '" + url + "';\r\n");
                            script.Append("       document." + control.ClientID + ".submit();\r\n");
                            break;
                        }
                    }
                    script.Append("   }\r\n");
                }
                script.Append("</script>\r\n");

                page.ClientScript.RegisterStartupScript(page.GetType(), "ShowDialog." + url, script.ToString());
            }
        }

        /// <summary>
        /// Shows a modal popup window for the specified web page <paramref name="url"/> when the <paramref name="control"/> is clicked.
        /// </summary>
        /// <param name="control"><see cref="Control"/> that will show the popup window when clicked.</param>
        /// <param name="url">Web page url for which the popup windows is to be shown.</param>
        /// <param name="dialogResultHolder"><see cref="Control"/> whose text is to updated with the text returned by the web page displayed in the popup window.</param>
        /// <param name="height">Height of the popup window.</param>
        /// <param name="width">Width of the popup window.</param>
        /// <param name="left">Location of the popup window on X-axis.</param>
        /// <param name="top">Location of the popup window on Y-axis.</param>
        /// <param name="center">True if the popup window is to be centered, otherwise False.</param>
        /// <param name="help">True if help button is to be displayed, otherwise False.</param>
        /// <param name="resizable">True if the popup window can be resized, otherwise False.</param>
        /// <param name="status">True if the status bar is to be displayed, otherwise False.</param>
        public static void ShowDialog(this Control control, string url, Control dialogResultHolder = null, int height = 400, int width = 600, int left = 0, int top = 0, bool center = true, bool help = false, bool resizable = false, bool status = false)
        {
            if (!control.Page.ClientScript.IsClientScriptBlockRegistered("ShowDialog"))
                control.Page.ClientScript.RegisterClientScriptBlock(control.Page.GetType(), "ShowDialog", GetShowDialogScript());

            if (dialogResultHolder != null)
            {
                HookupScriptToControl(control, "javascript:return(ShowDialog('" + url + "', '" + dialogResultHolder.ClientID + "', " +
                                                                    height + ", " + width + ", " + left + ", " + top + ", " +
                                                                    Math.Abs(Convert.ToInt32(center)) + ", " + Math.Abs(Convert.ToInt32(help)) + ", " +
                                                                    Math.Abs(Convert.ToInt32(resizable)) + ", " + Math.Abs(Convert.ToInt32(status)) + "))", "OnClick");
            }
            else
            {
                HookupScriptToControl(control, "javascript:return(ShowDialog('" + url + "', null, " + height + ", " + width + ", " + left + ", " + top + ", " +
                                                                                Math.Abs(Convert.ToInt32(center)) + ", " + Math.Abs(Convert.ToInt32(help)) + ", " +
                                                                                Math.Abs(Convert.ToInt32(resizable)) + ", " + Math.Abs(Convert.ToInt32(status)) + "))", "OnClick");
            }
        }

        /// <summary>
        /// Shows a popup for the specified web page <paramref name="url"/> when the <paramref name="page"/> has loaded.
        /// </summary>
        /// <param name="page"><see cref="Page"/> that will show the popup when loaded.</param>
        /// <param name="url">Web page url for which the popup is to be shown.</param>
        /// <param name="height">Height of the popup.</param>
        /// <param name="width">Width of the popup.</param>
        /// <param name="left">Location of the popup on X-axis.</param>
        /// <param name="top">Location of the popup on Y-axis.</param>
        /// <param name="center">True if the popup is to be centered, otherwise False.</param>
        /// <param name="resizable">True if the popup can be resized, otherwise False.</param>
        /// <param name="scrollbars">True is the scrollbars are to be displayed, otherwise False.</param>
        /// <param name="toolbar">True if the toolbar is to be displayed, otherwise False.</param>
        /// <param name="menubar">True if the menu bar is to be displayed, otherwise False.</param>
        /// <param name="location">True if the address bar is to be displayed, otherwise False.</param>
        /// <param name="status">True if the status bar is to be displayed, otherwise False.</param>
        /// <param name="directories">True if the directories buttons (Netscape only) are to be displayed, otherwise False.</param>
        public static void ShowPopup(this Page page, string url, int height = 400, int width = 600, int left = 0, int top = 0, bool center = true, bool resizable = false, bool scrollbars = false, bool toolbar = false, bool menubar = false, bool location = false, bool status = false, bool directories = false)
        {
            if (!page.ClientScript.IsClientScriptBlockRegistered("ShowPopup"))
                page.ClientScript.RegisterClientScriptBlock(page.GetType(), "ShowPopup", GetShowPopupScript());

            if (!page.ClientScript.IsStartupScriptRegistered("ShowPopup." + url))
            {
                StringBuilder script = new StringBuilder();
                script.Append("<script language=\"javascript\">\r\n");
                script.Append("   ShowPopup('" + url + "', " + height + ", " + width + ", " + left + ", " + top + ", " +
                                                Math.Abs(Convert.ToInt32(center)) + ", " + Math.Abs(Convert.ToInt32(resizable)) + ", " +
                                                Math.Abs(Convert.ToInt32(scrollbars)) + ", " + Math.Abs(Convert.ToInt32(toolbar)) + ", " +
                                                Math.Abs(Convert.ToInt32(menubar)) + ", " + Math.Abs(Convert.ToInt32(location)) + ", " +
                                                Math.Abs(Convert.ToInt32(status)) + ", " + Math.Abs(Convert.ToInt32(directories)) + ");\r\n");
                script.Append("</script>\r\n");

                page.ClientScript.RegisterStartupScript(page.GetType(), "ShowPopup." + url, script.ToString());
            }
        }

        /// <summary>
        /// Shows a popup for the specified web page <paramref name="url"/> when the <paramref name="control"/> is clicked.
        /// </summary>
        /// <param name="control"><see cref="Control"/> that will show the popup when clicked.</param>
        /// <param name="url">Web page url for which the popup is to be shown.</param>
        /// <param name="height">Height of the popup.</param>
        /// <param name="width">Width of the popup.</param>
        /// <param name="left">Location of the popup on X-axis.</param>
        /// <param name="top">Location of the popup on Y-axis.</param>
        /// <param name="center">True if the popup is to be centered, otherwise False.</param>
        /// <param name="resizable">True if the popup can be resized, otherwise False.</param>
        /// <param name="scrollbars">True is the scrollbars are to be displayed, otherwise False.</param>
        /// <param name="toolbar">True if the toolbar is to be displayed, otherwise False.</param>
        /// <param name="menubar">True if the menu bar is to be displayed, otherwise False.</param>
        /// <param name="location">True if the address bar is to be displayed, otherwise False.</param>
        /// <param name="status">True if the status bar is to be displayed, otherwise False.</param>
        /// <param name="directories">True if the directories buttons (Netscape only) are to be displayed, otherwise False.</param>
        public static void ShowPopup(this Control control, string url, int height = 400, int width = 600, int left = 0, int top = 0, bool center = true, bool resizable = false, bool scrollbars = false, bool toolbar = false, bool menubar = false, bool location = false, bool status = false, bool directories = false)
        {
            if (!control.Page.ClientScript.IsClientScriptBlockRegistered("ShowPopup"))
                control.Page.ClientScript.RegisterClientScriptBlock(control.Page.GetType(), "ShowPopup", GetShowPopupScript());

            HookupScriptToControl(control, "javascript:return(ShowPopup('" + url + "', " + height + ", " + width + ", " + left + ", " + top + ", " +
                                                                Math.Abs(Convert.ToInt32(center)) + ", " + Math.Abs(Convert.ToInt32(resizable)) + ", " +
                                                                Math.Abs(Convert.ToInt32(scrollbars)) + ", " + Math.Abs(Convert.ToInt32(toolbar)) + ", " +
                                                                Math.Abs(Convert.ToInt32(menubar)) + ", " + Math.Abs(Convert.ToInt32(location)) + ", " +
                                                                Math.Abs(Convert.ToInt32(status)) + ", " + Math.Abs(Convert.ToInt32(directories)) + "))", "OnClick");
        }

        /// <summary>
        /// Closes the current <paramref name="page"/> when it has finished loading in the browser and returns the specified <paramref name="returnValue"/> to the web page that opened it.
        /// </summary>
        /// <param name="page"><see cref="Page"/> to be closed.</param>
        /// <param name="returnValue">Value to be returned to the parent web page.</param>
        public static void Close(this Page page, string returnValue = null)
        {
            if (!page.ClientScript.IsClientScriptBlockRegistered("Close"))
                page.ClientScript.RegisterClientScriptBlock(page.GetType(), "Close", GetCloseScript());

            if (!page.ClientScript.IsStartupScriptRegistered("Close.Page"))
            {
                StringBuilder script = new StringBuilder();
                script.Append("<script language=\"javascript\">\r\n");
                script.Append("   Close('" + returnValue + "');\r\n");
                script.Append("</script>\r\n");

                page.ClientScript.RegisterStartupScript(page.GetType(), "Close.Page", script.ToString());
            }
        }

        /// <summary>
        /// Closes the current page when the <paramref name="control"/> is clicked and returns the specified <paramref name="returnValue"/> to the web page that opened it.
        /// </summary>
        /// <param name="control"><see cref="Control"/> that will close the current web page when clicked.</param>
        /// <param name="returnValue">Value to be returned to the parent web page.</param>
        public static void Close(this Control control, string returnValue = null)
        {
            if (!control.Page.ClientScript.IsClientScriptBlockRegistered("Close"))
                control.Page.ClientScript.RegisterClientScriptBlock(control.Page.GetType(), "Close", GetCloseScript());

            HookupScriptToControl(control, "javascript:return(Close('" + returnValue + "'))", "OnClick");
        }

        /// <summary>
        /// Shows a windows application style message box when the <paramref name="page"/> has loaded.
        /// </summary>
        /// <param name="page"><see cref="Page"/> that will show the message box when loaded.</param>
        /// <param name="prompt">Text that is to be displayed in the message box.</param>
        /// <param name="title">Title of the message box.</param>
        /// <param name="buttons">Buttons to be displayed in the message box.</param>
        /// <param name="doPostBack">True if a post-back is to be performed when either OK, Retry, or Yes buttons are clicked in the message box, otherwise False.</param>
        public static void MsgBox(this Page page, string prompt, string title, MsgBoxStyle buttons, bool doPostBack = false)
        {
            if (!page.ClientScript.IsClientScriptBlockRegistered("ShowMsgBox"))
                page.ClientScript.RegisterClientScriptBlock(page.GetType(), "ShowMsgBox", GetMsgBoxScript());

            if (!page.ClientScript.IsStartupScriptRegistered("ShowMsgBox." + prompt))
            {
                StringBuilder script = new StringBuilder();
                script.Append("<input type=\"hidden\" name=\"TVA_EVENT_TARGET\" value=\"\" />\r\n");
                script.Append("<input type=\"hidden\" name=\"TVA_EVENT_ARGUMENT\" value=\"\" />\r\n");
                script.Append("<script language=\"javascript\">\r\n");
                script.Append("   if (ShowMsgBox('" + JavaScriptEncode(ref prompt) + " ', '" + title + "', " + (int)buttons + ", " + doPostBack.ToString().ToLower() + "))\r\n");
                script.Append("   {\r\n");
                foreach (Control control in page.Controls)
                {
                    if (control is HtmlForm)
                    {
                        script.Append("       " + control.ClientID + ".TVA_EVENT_TARGET.value = 'MsgBox';\r\n");
                        script.Append("       " + control.ClientID + ".TVA_EVENT_ARGUMENT.value = '" + title + "';\r\n");
                        script.Append("       document." + control.ClientID + ".submit();\r\n");
                        break;
                    }
                }
                script.Append("   }\r\n");
                script.Append("</script>\r\n");

                page.ClientScript.RegisterStartupScript(page.GetType(), "ShowMsgBox." + prompt, script.ToString());
            }
        }

        /// <summary>
        /// Shows a windows application style message box when the <paramref name="control"/> is clicked.
        /// </summary>
        /// <param name="control"><see cref="Control"/> that will show the message box when clicked.</param>
        /// <param name="prompt">Text that is to be displayed in the message box.</param>
        /// <param name="title">Title of the message box.</param>
        /// <param name="buttons">Buttons to be displayed in the message box.</param>
        /// <param name="doPostBack">True if a post-back is to be performed when either OK, Retry, or Yes buttons are clicked in the message box, otherwise False.</param>
        public static void MsgBox(this Control control, string prompt, string title, MsgBoxStyle buttons, bool doPostBack = false)
        {
            if (!control.Page.ClientScript.IsClientScriptBlockRegistered("ShowMsgBox"))
                control.Page.ClientScript.RegisterClientScriptBlock(control.Page.GetType(), "ShowMsgBox", GetMsgBoxScript());

            HookupScriptToControl(control, "javascript:return(ShowMsgBox('" + JavaScriptEncode(ref prompt) + " ', '" + title + "', " + (int)buttons + ", " + doPostBack.ToString().ToLower() + "))", "OnClick");
        }

        private static void HookupScriptToControl(Control control, string script, string attribute)
        {
            if (control is Button)
                ((Button)control).Attributes.Add(attribute, script);
            else if (control is LinkButton)
                ((LinkButton)control).Attributes.Add(attribute, script);
            else if (control is ImageButton)
                ((ImageButton)control).Attributes.Add(attribute, script);
            else if (control is Image)
                ((Image)control).Attributes.Add(attribute, script);
            else if (control is Label)
                ((Label)control).Attributes.Add(attribute, script);
            else if (control is TextBox)
                ((TextBox)control).Attributes.Add(attribute, script);
        }

        private static string GetRefreshScript()
        {
            StringBuilder script = new StringBuilder();
            script.Append("<script language=\"javascript\">\r\n");
            script.Append("   function Refresh()\r\n");
            script.Append("   {\r\n");
            script.Append("       window.location.href = unescape(window.location.pathname);\r\n");
            script.Append("       return false;\r\n");
            script.Append("   }\r\n");
            script.Append("</script>\r\n");

            return script.ToString();
        }

        private static string GetShowScript()
        {
            StringBuilder script = new StringBuilder();
            script.Append("<script language=\"javascript\">\r\n");
            script.Append("   function Show(url, height, width, left, top, center, help, resizable, status)\r\n");
            script.Append("   {\r\n");
            script.Append("       window.showModelessDialog(url, window.self, \'dialogWidth:\' + width + \'px;dialogHeight:\' + height + \'px;left:\' + left + \';top:\' + top + \';center:\' + center + \';help:\' + help + \';resizable:\' + resizable + \';status:\' + status);\r\n");
            script.Append("       return false;\r\n");
            script.Append("   }\r\n");
            script.Append("</script>\r\n");

            return script.ToString();
        }

        private static string GetShowDialogScript()
        {
            StringBuilder script = new StringBuilder();
            script.Append("<script language=\"javascript\">\r\n");
            script.Append("   function ShowDialog(url, resultHolderId, height, width, left, top, center, help, resizable, status)\r\n");
            script.Append("   {\r\n");
            script.Append("       returnValue = window.showModalDialog(url, window.self,\'dialogWidth:\' + width + \'px;dialogHeight:\' + height + \'px;left:\' + left + \';top:\' + top + \';center:\' + center + \';help:\' + help + \';resizable:\' + resizable + \';status:\' + status);\r\n");
            script.Append("       if (returnValue != null)\r\n");
            script.Append("       {\r\n");
            script.Append("           if ((resultHolderId != null) && (document.getElementById(resultHolderId) != null))\r\n");
            script.Append("           {\r\n");
            script.Append("               document.getElementById(resultHolderId).value = returnValue;\r\n");
            script.Append("               return false;\r\n");
            script.Append("           }\r\n");
            script.Append("           else\r\n");
            script.Append("           {\r\n");
            script.Append("               return true;\r\n");
            script.Append("           }\r\n");
            script.Append("       }\r\n");
            script.Append("       else\r\n");
            script.Append("       {\r\n");
            script.Append("           return false;\r\n");
            script.Append("       }\r\n");
            script.Append("   }\r\n");
            script.Append("</script>\r\n");

            return script.ToString();
        }

        private static string GetShowPopupScript()
        {
            StringBuilder script = new StringBuilder();
            script.Append("<script language=\"javascript\">\r\n");
            script.Append("   function ShowPopup(url, height, width, left, top, center, resizable, scrollbars, toolbar, menubar, location, status, directories)\r\n");
            script.Append("   {\r\n");
            script.Append("       specialCharacters = /([^a-zA-Z0-9\\s])/gi;\r\n");
            script.Append("       popupName = url.replace(specialCharacters, \'\');\r\n");
            script.Append("       if (center)\r\n");
            script.Append("       {\r\n");
            script.Append("           popup = window.open(url, popupName, \'height=\' + height + \',width=\' + width + \',top=\' + ((screen.availHeight / 2) - (height / 2)) + \',left=\' + ((screen.availWidth / 2) - (width / 2)) + \',resizable=\' + resizable + \',scrollbars=\' + scrollbars + \',toolbar=\' + toolbar + \',menubar=\' + menubar + \',location=\' + location + \',status=\' + status + \',directories=\' + directories);\r\n");
            script.Append("       }\r\n");
            script.Append("       else\r\n");
            script.Append("       {\r\n");
            script.Append("            popup = window.open(url, popupName, \'height=\' + height + \',width=\' + width + \',top=\' + top + \',left=\' + left + \',resizable=\' + resizable + \',scrollbars=\' + scrollbars + \',toolbar=\' + toolbar + \',menubar=\' + menubar + \',location=\' + location + \',status=\' + status + \',directories=\' + directories);\r\n");
            script.Append("       }\r\n");
            script.Append("       popup.focus();\t\n");
            script.Append("       return false;\r\n");
            script.Append("   }\r\n");
            script.Append("</script>\r\n");

            return script.ToString();
        }

        private static string GetCloseScript()
        {
            StringBuilder script = new StringBuilder();
            script.Append("<script language=\"javascript\">\r\n");
            script.Append("   function Close(returnValue)\r\n");
            script.Append("   {\r\n");
            script.Append("       if (returnValue == \'\')\r\n");
            script.Append("       {\r\n");
            script.Append("           returnValue = null;\r\n");
            script.Append("       }\r\n");
            script.Append("       window.returnValue = returnValue;\r\n");
            script.Append("       window.close();\r\n");
            script.Append("       return false;\r\n");
            script.Append("   }\r\n");
            script.Append("</script>\r\n");

            return script.ToString();
        }

        private static string GetMsgBoxScript()
        {
            StringBuilder script = new StringBuilder();
            script.Append("<script language=\"vbscript\">\r\n");
            script.Append("   Function ShowMsgBox(prompt, title, buttons, doPostBack)\r\n");
            script.Append("       result = MsgBox(prompt, buttons ,title)\r\n");
            script.Append("       If doPostBack Then\r\n");
            script.Append("           If (result = vbOK) Or (result = vbRetry) Or (result = vbYes) Then\r\n");
            script.Append("               ShowMsgBox = True\r\n");
            script.Append("           Else\r\n");
            script.Append("               ShowMsgBox = False\r\n");
            script.Append("           End If\r\n");
            script.Append("       Else\r\n");
            script.Append("           ShowMsgBox = False\r\n");
            script.Append("       End If\r\n");
            script.Append("   End Function\r\n");
            script.Append("</script>\r\n");

            return script.ToString();
        }

        #region [ Old Code ]

        ///// <summary>
        ///// The different types of client-side scripts.
        ///// </summary>
        ///// <remarks></remarks>
        //private enum ClientSideScript
        //{
        //    Focus,
        //    DefaultButton,
        //    Show,
        //    ShowDialog,
        //    ShowPopup,
        //    Close,
        //    MsgBox,
        //    Refresh,
        //    Maximize,
        //    Minimize,
        //    RunClientExe
        //}

        ///// <summary>
        ///// Creates the appropriate client-side script based on the specified PCS.Web.ClientSideScript value.
        ///// </summary>
        ///// <param name="script">One of the PCS.Web.ClientSideScript values.</param>
        ///// <returns>The client-side script for the specified PCS.Web.ClientSideScript value</returns>
        ///// <remarks></remarks>
        //private static string CreateClientSideScript(ClientSideScript script)
        //{

        //    string clientScript = "";
        //    switch (script)
        //    {
        //        case ClientSideScript.Focus: // Client-side script for Focus.
        //            System.Text.StringBuilder with_1 = new StringBuilder();
        //            with_1.Append("<script language=\"javascript\">" + System.Environment.NewLine);
        //            with_1.Append("   function Focus(controlId)" + System.Environment.NewLine);
        //            with_1.Append("   {" + System.Environment.NewLine);
        //            with_1.Append("       if (document.getElementById(controlId) != null)" + System.Environment.NewLine);
        //            with_1.Append("       {" + System.Environment.NewLine);
        //            with_1.Append("           document.getElementById(controlId).focus();" + System.Environment.NewLine);
        //            with_1.Append("       }" + System.Environment.NewLine);
        //            with_1.Append("   }" + System.Environment.NewLine);
        //            with_1.Append("</script>" + System.Environment.NewLine);

        //            clientScript = with_1.ToString();
        //            break;
        //        case ClientSideScript.DefaultButton: // Client-side script for DefaultButton.
        //            System.Text.StringBuilder with_2 = new StringBuilder();
        //            with_2.Append("<script language=\"javascript\">" + System.Environment.NewLine);
        //            with_2.Append("   function DefaultButton(controlId)" + System.Environment.NewLine);
        //            with_2.Append("   {" + System.Environment.NewLine);
        //            with_2.Append("       if (event.keyCode == 13)" + System.Environment.NewLine);
        //            with_2.Append("       {" + System.Environment.NewLine);
        //            with_2.Append("           event.returnValue = false;" + System.Environment.NewLine);
        //            with_2.Append("           event.cancel = true;" + System.Environment.NewLine);
        //            with_2.Append("           if (document.getElementById(controlId) != null)" + System.Environment.NewLine);
        //            with_2.Append("           {" + System.Environment.NewLine);
        //            with_2.Append("               document.getElementById(controlId).click();" + System.Environment.NewLine);
        //            with_2.Append("           }" + System.Environment.NewLine);
        //            with_2.Append("       }" + System.Environment.NewLine);
        //            with_2.Append("   }" + System.Environment.NewLine);
        //            with_2.Append("</script>" + System.Environment.NewLine);

        //            clientScript = with_2.ToString();
        //            break;
        //        case ClientSideScript.Show: // Client-side script for Show.
        //            System.Text.StringBuilder with_3 = new StringBuilder();
        //            with_3.Append("<script language=\"javascript\">" + System.Environment.NewLine);
        //            with_3.Append("   function Show(url, height, width, left, top, center, help, resizable, status)" + System.Environment.NewLine);
        //            with_3.Append("   {" + System.Environment.NewLine);
        //            with_3.Append("       window.showModelessDialog(url, window.self,\'dialogWidth:\' + width + \'px;dialogHeight:\' + height + \'px;left:\' + left + \';top:\' + top + \';center:\' + center + \';help:\' + help + \';resizable:\' + resizable + \';status:\' + status);" + System.Environment.NewLine);
        //            with_3.Append("       return false;" + System.Environment.NewLine);
        //            with_3.Append("   }" + System.Environment.NewLine);
        //            with_3.Append("</script>" + System.Environment.NewLine);

        //            clientScript = with_3.ToString();
        //            break;
        //        case ClientSideScript.ShowDialog: //Client-side script for ShowDialog.
        //            System.Text.StringBuilder with_4 = new StringBuilder();
        //            with_4.Append("<script language=\"javascript\">" + System.Environment.NewLine);
        //            with_4.Append("   function ShowDialog(url, resultHolderId, height, width, left, top, center, help, resizable, status)" + System.Environment.NewLine);
        //            with_4.Append("   {" + System.Environment.NewLine);
        //            with_4.Append("       returnValue = window.showModalDialog(url, window.self,\'dialogWidth:\' + width + \'px;dialogHeight:\' + height + \'px;left:\' + left + \';top:\' + top + \';center:\' + center + \';help:\' + help + \';resizable:\' + resizable + \';status:\' + status);" + System.Environment.NewLine);
        //            with_4.Append("       if (returnValue != null)" + System.Environment.NewLine);
        //            with_4.Append("       {" + System.Environment.NewLine);
        //            with_4.Append("           if ((resultHolderId != null) && (document.getElementById(resultHolderId) != null))" + System.Environment.NewLine);
        //            with_4.Append("           {" + System.Environment.NewLine);
        //            with_4.Append("               document.getElementById(resultHolderId).value = returnValue;" + System.Environment.NewLine);
        //            with_4.Append("               return false;" + System.Environment.NewLine);
        //            with_4.Append("           }" + System.Environment.NewLine);
        //            with_4.Append("           else" + System.Environment.NewLine);
        //            with_4.Append("           {" + System.Environment.NewLine);
        //            with_4.Append("               return true;" + System.Environment.NewLine);
        //            with_4.Append("           }" + System.Environment.NewLine);
        //            with_4.Append("       }" + System.Environment.NewLine);
        //            with_4.Append("       else" + System.Environment.NewLine);
        //            with_4.Append("       {" + System.Environment.NewLine);
        //            with_4.Append("           return false;" + System.Environment.NewLine);
        //            with_4.Append("       }" + System.Environment.NewLine);
        //            with_4.Append("   }" + System.Environment.NewLine);
        //            with_4.Append("</script>" + System.Environment.NewLine);

        //            clientScript = with_4.ToString();
        //            break;
        //        case ClientSideScript.ShowPopup: // Client-side script for ShowPopup.
        //            System.Text.StringBuilder with_5 = new StringBuilder();
        //            with_5.Append("<script language=\"javascript\">" + System.Environment.NewLine);
        //            with_5.Append("   function ShowPopup(url, height, width, left, top, center, resizable, scrollbars, toolbar, menubar, location, status, directories)" + System.Environment.NewLine);
        //            with_5.Append("   {" + System.Environment.NewLine);
        //            with_5.Append("       specialCharacters = /([^a-zA-Z0-9\\s])/gi;" + System.Environment.NewLine);
        //            with_5.Append("       popupName = url.replace(specialCharacters, \'\');" + System.Environment.NewLine);
        //            with_5.Append("       if (center)" + System.Environment.NewLine);
        //            with_5.Append("       {" + System.Environment.NewLine);
        //            with_5.Append("           popup = window.open(url, popupName, \'height=\' + height + \',width=\' + width + \',top=\' + ((screen.availHeight / 2) - (height / 2)) + \',left=\' + ((screen.availWidth / 2) - (width / 2)) + \',resizable=\' + resizable + \',scrollbars=\' + scrollbars + \',toolbar=\' + toolbar + \',menubar=\' + menubar + \',location=\' + location + \',status=\' + status + \',directories=\' + directories);" + System.Environment.NewLine);
        //            with_5.Append("       }" + System.Environment.NewLine);
        //            with_5.Append("       else" + System.Environment.NewLine);
        //            with_5.Append("       {" + System.Environment.NewLine);
        //            with_5.Append("            popup = window.open(url, popupName, \'height=\' + height + \',width=\' + width + \',top=\' + top + \',left=\' + left + \',resizable=\' + resizable + \',scrollbars=\' + scrollbars + \',toolbar=\' + toolbar + \',menubar=\' + menubar + \',location=\' + location + \',status=\' + status + \',directories=\' + directories);" + System.Environment.NewLine);
        //            with_5.Append("       }" + System.Environment.NewLine);
        //            with_5.Append("       popup.focus();" + System.Environment.NewLine);
        //            with_5.Append("       return false;" + System.Environment.NewLine);
        //            with_5.Append("   }" + System.Environment.NewLine);
        //            with_5.Append("</script>" + System.Environment.NewLine);

        //            clientScript = with_5.ToString();
        //            break;
        //        case ClientSideScript.Close: // Client-side script for Close.
        //            System.Text.StringBuilder with_6 = new StringBuilder();
        //            with_6.Append("<script language=\"javascript\">" + System.Environment.NewLine);
        //            with_6.Append("   function Close(returnValue)" + System.Environment.NewLine);
        //            with_6.Append("   {" + System.Environment.NewLine);
        //            with_6.Append("       if (returnValue == \'\')" + System.Environment.NewLine);
        //            with_6.Append("       {" + System.Environment.NewLine);
        //            with_6.Append("           returnValue = null;" + System.Environment.NewLine);
        //            with_6.Append("       }" + System.Environment.NewLine);
        //            with_6.Append("       window.returnValue = returnValue;" + System.Environment.NewLine);
        //            with_6.Append("       window.close();" + System.Environment.NewLine);
        //            with_6.Append("       return false;" + System.Environment.NewLine);
        //            with_6.Append("   }" + System.Environment.NewLine);
        //            with_6.Append("</script>" + System.Environment.NewLine);

        //            clientScript = with_6.ToString();
        //            break;
        //        case ClientSideScript.MsgBox: // Client-side script for MsgBox.
        //            System.Text.StringBuilder with_7 = new StringBuilder();
        //            with_7.Append("<script language=\"vbscript\">" + System.Environment.NewLine);
        //            with_7.Append("   Function ShowMsgBox(prompt, title, buttons, doPostBack)" + System.Environment.NewLine);
        //            with_7.Append("       result = MsgBox(prompt, buttons ,title)" + System.Environment.NewLine);
        //            with_7.Append("       If doPostBack Then" + System.Environment.NewLine);
        //            with_7.Append("           If (result = vbOK) Or (result = vbRetry) Or (result = vbYes) Then" + System.Environment.NewLine);
        //            with_7.Append("               ShowMsgBox = True" + System.Environment.NewLine);
        //            with_7.Append("           Else" + System.Environment.NewLine);
        //            with_7.Append("               ShowMsgBox = False" + System.Environment.NewLine);
        //            with_7.Append("           End If" + System.Environment.NewLine);
        //            with_7.Append("       Else" + System.Environment.NewLine);
        //            with_7.Append("           ShowMsgBox = False" + System.Environment.NewLine);
        //            with_7.Append("       End If" + System.Environment.NewLine);
        //            with_7.Append("   End Function" + System.Environment.NewLine);
        //            with_7.Append("</script>" + System.Environment.NewLine);

        //            clientScript = with_7.ToString();
        //            break;
        //        case ClientSideScript.Refresh: // Client-side script for Refresh.
        //            System.Text.StringBuilder with_8 = new StringBuilder();
        //            with_8.Append("<script language=\"javascript\">" + System.Environment.NewLine);
        //            with_8.Append("   function Refresh()" + System.Environment.NewLine);
        //            with_8.Append("   {" + System.Environment.NewLine);
        //            with_8.Append("       window.location.href = unescape(window.location.pathname);" + System.Environment.NewLine);
        //            with_8.Append("       return false;" + System.Environment.NewLine);
        //            with_8.Append("   }" + System.Environment.NewLine);
        //            with_8.Append("</script>" + System.Environment.NewLine);

        //            clientScript = with_8.ToString();
        //            break;
        //        case ClientSideScript.Maximize: // Client-side script for Maximize.
        //            System.Text.StringBuilder with_9 = new StringBuilder();
        //            with_9.Append("<script language=\"javascript\">" + System.Environment.NewLine);
        //            with_9.Append("   function Maximize()" + System.Environment.NewLine);
        //            with_9.Append("   {" + System.Environment.NewLine);
        //            with_9.Append("       window.moveTo(0, 0);" + System.Environment.NewLine);
        //            with_9.Append("       window.resizeTo(window.screen.availWidth, window.screen.availHeight);" + System.Environment.NewLine);
        //            with_9.Append("       return false;" + System.Environment.NewLine);
        //            with_9.Append("   }" + System.Environment.NewLine);
        //            with_9.Append("</script>" + System.Environment.NewLine);

        //            clientScript = with_9.ToString();
        //            break;
        //        case ClientSideScript.Minimize: // Client-side script for Minimize.
        //            System.Text.StringBuilder with_10 = new StringBuilder();
        //            with_10.Append("<script language=\"javascript\">" + System.Environment.NewLine);
        //            with_10.Append("   function Minimize()" + System.Environment.NewLine);
        //            with_10.Append("   {" + System.Environment.NewLine);
        //            with_10.Append("       window.blur();" + System.Environment.NewLine);
        //            with_10.Append("       return false;" + System.Environment.NewLine);
        //            with_10.Append("   }" + System.Environment.NewLine);
        //            with_10.Append("</script>" + System.Environment.NewLine);

        //            clientScript = with_10.ToString();
        //            break;
        //        case ClientSideScript.RunClientExe: // Client-side script for RunClientExe.
        //            System.Text.StringBuilder with_11 = new StringBuilder();
        //            with_11.Append("<script language=\"vbscript\">" + System.Environment.NewLine);
        //            with_11.Append("   Function RunClientExe(exeToRun)" + System.Environment.NewLine);
        //            with_11.Append("       On Error Resume Next" + System.Environment.NewLine);
        //            with_11.Append("       Set shell = CreateObject(\"WScript.Shell\")" + System.Environment.NewLine);
        //            with_11.Append("       returnCode = shell.Run(exeToRun)" + System.Environment.NewLine);
        //            with_11.Append("       Set shell = Nothing" + System.Environment.NewLine);
        //            with_11.Append("       If Err.number <> 0 Then" + System.Environment.NewLine);
        //            with_11.Append("           result = MsgBox(\"Failed to execute \" & exeToRun & \".\", 16, \"RunClientExe\")" + System.Environment.NewLine);
        //            with_11.Append("           RunClientExe = True" + System.Environment.NewLine);
        //            with_11.Append("       Else" + System.Environment.NewLine);
        //            with_11.Append("           RunClientExe = False" + System.Environment.NewLine);
        //            with_11.Append("       End If" + System.Environment.NewLine);
        //            with_11.Append("   End Function" + System.Environment.NewLine);
        //            with_11.Append("</script>" + System.Environment.NewLine);

        //            clientScript = with_11.ToString();
        //            break;
        //    }

        //    return clientScript;
        //}

        #endregion
    }
}
