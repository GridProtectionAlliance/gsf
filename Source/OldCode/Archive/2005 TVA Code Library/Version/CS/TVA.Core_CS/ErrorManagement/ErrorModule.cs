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
//
//*******************************************************************************************************

using System.Web;
using TVA.Configuration;

namespace TVA.ErrorManagement
{
    /// <summary>
    /// Represents an HTTP module that can be used to handle exceptions globally in Web Sites and Web Services.
    /// </summary>
    /// <seealso cref="ErrorLogger"/>
    /// <example>
    /// Usage in web.config:
    /// <code>
    /// <configuration>
    ///   <system.web>
    ///     <httpModules>
    ///       <add name="ErrorModule" type="TVA.ErrorManagement.ErrorModule, TVA.Core" />
    ///     </httpModules>
    ///   </system.web>
    /// </configuration>
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
            m_context = context;        // Save reference to the application.   
            m_context.Error += OnError; // Register to be notified for unhandled exceptions.
        }

        /// <summary>
        /// Disposes the resources (other than memory) used by the <see cref="ErrorModule"/>.
        /// </summary>
        public void Dispose()
        {
            m_context.Error -= OnError; // Unregister from being notified for unhandled exceptions.
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
            m_logger.Initialize();  // This will cause settings, if persisted previously, to be loaded.
        }

        /// <summary>
        /// Gets the <see cref="ErrorLogger"/> object used by the <see cref="ErrorModule"/> object for logging exceptions.
        /// </summary>
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
