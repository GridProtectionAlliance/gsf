//*******************************************************************************************************
//  ErrorModule.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C Patel
//      Office: INFO SVCS APP DEV, CHATTANOOGA - MR 2W-C
//       Phone: 423-751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  04/17/2007 - Pinal C. Patel
//       Generated original version of source code.
//  09/13/2007 - Darrell Zuercher
//       Edited code comments.
//
//*******************************************************************************************************

using System.Web;
using TVA.Configuration;

namespace TVA.ErrorManagement
{
    /// <summary>
    /// 
    /// </summary>
    /// <example>
    /// Usage sample in web.config:
    /// <code>
    ///<configuration>
    ///  <configSections>
    ///    <section name="categorizedSettings" type="TVA.Configuration.CategorizedSettingsSection, TVA.Core" />
    ///  </configSections>
    ///  <categorizedSettings>
    ///    <errorLogger>
    ///      <clear />
    ///      <add name="AutoStart" value="False" description="True if the logger is to be started automatically after initialization is complete; otherwise False." encrypted="false" />
    ///      <add name="LogToUI" value="True" description="True if an encountered exception is to be logged to the User Interface; otherwise False." encrypted="false" />
    ///      <add name="LogToFile" value="False" description="True if an encountered exception is to be logged to a file; otherwise False." encrypted="false" />
    ///      <add name="LogToEmail" value="True" description="True if an email is to be sent with the details of an encountered exception; otherwise False." encrypted="false" />
    ///      <add name="LogToEventLog" value="False" description="True if an encountered exception is to be logged to the Event Log; otherwise False." encrypted="false" />
    ///      <add name="LogToScreenshot" value="False" description="True if a screenshot is to be taken when an exception is encountered; otherwise False." encrypted="false" />
    ///      <add name="EmailServer" value="mailhost.cha.tva.gov" description="Name of the email server to be used for sending the email message." encrypted="false" />
    ///      <add name="EmailRecipients" value="pcpatel@tva.gov" description="Comma-seperated list of recipients email addresses for the email message." encrypted="false" />
    ///      <add name="ContactPersonName" value="" description="Name of the person that the end-user can contact when an exception is encountered." encrypted="false" />
    ///      <add name="ContactPersonPhone" value="" description="Phone number of the person that the end-user can contact when an exception is encountered." encrypted="false" />
    ///    </errorLogger>
    ///  </categorizedSettings>
    ///  <system.web>
    ///    <httpModules>
    ///      <add name="ErrorModule" type="TVA.ErrorManagement.ErrorModule, TVA.Core" />
    ///    </httpModules>
    ///  </system.web>
    ///</configuration>
    /// </code>
    /// </example>
    public class ErrorModule : IHttpModule
    {
        public void Init(System.Web.HttpApplication context)
        {
            try
            {
                if (ConfigurationFile.Current.Settings[typeof(ErrorLogger).Name].Count == 0)
                {
                    ErrorLogger logger = new ErrorLogger();
                    logger.PersistSettings = true;
                    logger.SaveSettings();
                }
            }
            catch
            {

            }
            context.Error += new System.EventHandler(OnError);
        }

        public void Dispose()
        {
            // We do not have to dispose of anything.
        }

        private void OnError(object sender, System.EventArgs e)
        {
            ErrorLogger logger = new ErrorLogger();
            // Logs the encountered exception.
            logger.BeginInit();
            logger.EndInit();
            logger.Log(HttpContext.Current.Server.GetLastError());
        }
    }
}
