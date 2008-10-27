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
//  09/24/2008 - Pinal C Patel
//       Converted code to C#.
//  10/17/2008 - Pinal C Patel
//       Edited code comments.
//
//*******************************************************************************************************

using System.Diagnostics;
using System.Web;

namespace PCS.ErrorManagement
{
    /// <summary>
    /// Represents an HTTP module that can be used to handle exceptions globally in Web Sites and Web Services.
    /// </summary>
    /// <seealso cref="ErrorLogger"/>
    /// <example>
    /// Below is the config file entry required for enabling error handling using <see cref="ErrorModule"/>:
    /// <code>
    /// <![CDATA[
    /// <configuration>
    ///   <system.web>
    ///     <httpModules>
    ///       <add name="ErrorModule" type="PCS.ErrorManagement.ErrorModule, PCS.Core" />
    ///     </httpModules>
    ///   </system.web>
    /// </configuration>
    /// ]]>
    /// </code>
    /// Below is the config file entry required for changing the settings of <see cref="ErrorModule.Logger"/>:
    /// <code>
    /// <![CDATA[
    /// <configuration>
    ///   <configSections>
    ///     <section name="categorizedSettings" type="PCS.Configuration.CategorizedSettingsSection, PCS.Core" />
    ///   </configSections>
    ///   <categorizedSettings>
    ///     <errorLogger>
    ///       <add name="LogToUI" value="False" description="True if an encountered exception is to be logged to the User Interface; otherwise False."
    ///         encrypted="false" />
    ///       <add name="LogToFile" value="True" description="True if an encountered exception is to be logged to a file; otherwise False."
    ///         encrypted="false" />
    ///       <add name="LogToEmail" value="False" description="True if an email is to be sent to ContactEmail with the details of an encountered exception; otherwise False."
    ///         encrypted="false" />
    ///       <add name="LogToEventLog" value="True" description="True if an encountered exception is to be logged to the Event Log; otherwise False."
    ///         encrypted="false" />
    ///       <add name="LogToScreenshot" value="False" description="True if a screenshot is to be taken when an exception is encountered; otherwise False."
    ///         encrypted="false" />
    ///       <add name="SmtpServer" value="smtp.email.com" description="Name of the SMTP server to be used for sending the email messages."
    ///         encrypted="false" />
    ///       <add name="ContactEmail" value="" description="Comma-seperated list of recipient email addresses for the email message."
    ///         encrypted="false" />
    ///       <add name="ContactName" value="" description="Name of the person that the end-user can contact when an exception is encountered."
    ///         encrypted="false" />
    ///       <add name="ContactPhone" value="" description="Phone number of the person that the end-user can contact when an exception is encountered."
    ///         encrypted="false" />
    ///       <add name="HandleUnhandledException" value="True" description="True if unhandled exceptions are to be handled automatically; otherwise False."
    ///         encrypted="false" />
    ///       <add name="ExitOnUnhandledException" value="False" description="True if the application must exit when an unhandled exception is encountered; otherwise False."
    ///         encrypted="false" />
    ///     </errorLogger>
    ///   </categorizedSettings>
    /// </configuration>
    /// ]]>
    /// </code>
    /// </example>
    public class ErrorModule : IHttpModule
    {
        #region [ Members ]

        // Fields
        private HttpApplication m_context;

        #endregion

        #region [ Methods ]
        
        /// <summary>
        /// Initializes the <see cref="ErrorModule"/> and prepares it to handle requests.
        /// </summary>
        /// <param name="context"></param>
        public void Init(HttpApplication context)
        {
            if (Logger.HandleUnhandledException && !Debugger.IsAttached)
            {
                m_context = context;        // Save reference to the application.   
                m_context.Error += OnError; // Register to be notified for unhandled exceptions.                
            }
        }

        /// <summary>
        /// Disposes the resources (other than memory) used by the <see cref="ErrorModule"/>.
        /// </summary>
        public void Dispose()
        {
            if (Logger.HandleUnhandledException && !Debugger.IsAttached)
            {
                m_context.Error -= OnError; // Unregister from being notified for unhandled exceptions.
            }
        }

        private void OnError(object sender, System.EventArgs e)
        {
            // Log the last encountered exception.
            Logger.Log(HttpContext.Current.Server.GetLastError());
        }

        #endregion

        #region [ Static ]

        private static ErrorLogger m_logger;

        static ErrorModule()
        {
            m_logger = new ErrorLogger();
            m_logger.PersistSettings = true;
            m_logger.Initialize();          // Initialize error logger for use.
        }

        /// <summary>
        /// Gets the <see cref="ErrorLogger"/> object used by the <see cref="ErrorModule"/> object for logging exceptions.
        /// </summary>
        /// <remarks>
        /// <see cref="Logger"/> property can be used for logging handled exception throughout the web application.
        /// </remarks>
        /// <example>
        /// This example shows the use of <see cref="ErrorModule.Logger"/> for logging handled exception:
        /// <code>
        /// using System;
        /// using PCS.ErrorManagement;
        ///
        /// namespace WebApp
        /// {
        ///     public partial class _Default : System.Web.UI.Page
        ///     {
        ///         protected void Page_Load(object sender, EventArgs e)
        ///         {
        ///             try
        ///             {
        ///                 string s = null;
        ///                 s.ToCharArray();            // This will result in NullReferenceException.
        ///             }
        ///             catch (Exception ex)
        ///             {
        ///                 ErrorModule.Logger.Log(ex); // Log the encountered exception.
        ///             }
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public static ErrorLogger Logger
        {
            get 
            {
                return m_logger;
            }
        }

        #endregion
    }
}
