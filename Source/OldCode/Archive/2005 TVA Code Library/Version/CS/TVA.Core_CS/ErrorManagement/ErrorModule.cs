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
    /// 
    /// </summary>
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
        #region [ Methods ]

        public void Init(HttpApplication context)
        {
            context.Error += OnError;   // Register to be notified on unhandled exceptions.
        }

        public void Dispose()
        {
            Logger.Dispose();
        }

        private void OnError(object sender, System.EventArgs e)
        {
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

        public static ErrorLogger Logger
        {
            get 
            {
                return m_logger;
            }
        }

        #endregion
        



        //public void Init(System.Web.HttpApplication context)
        //{
        //    try
        //    {
        //        if (ConfigurationFile.Current.Settings[typeof(ErrorLogger).Name].Count == 0)
        //        {
        //            ErrorLogger logger = new ErrorLogger();
        //            logger.PersistSettings = true;
        //            logger.SaveSettings();
        //        }
        //    }
        //    catch
        //    {

        //    }
        //    context.Error += new System.EventHandler(OnError);
        //}

        //public void Dispose()
        //{
        //    // We do not have to dispose of anything.
        //}

        //private void OnError(object sender, System.EventArgs e)
        //{
        //    ErrorLogger logger = new ErrorLogger();
        //    // Logs the encountered exception.
        //    logger.BeginInit();
        //    logger.EndInit();
        //    logger.Log(HttpContext.Current.Server.GetLastError());
        //}
    }
}
