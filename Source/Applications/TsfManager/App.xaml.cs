//******************************************************************************************************
//  App.xaml.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  04/01/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Security.Principal;
using System.Windows;
using GSF.Windows.ErrorManagement;
using GSF.Reflection;
using GSF.TimeSeries.UI;

namespace TsfManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region [ Members ]

        // Fields
        private Guid m_nodeID;
        private readonly ErrorLogger m_errorLogger;
        private readonly Func<string> m_defaultErrorText;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates an instance of <see cref="App"/> class.
        /// </summary>
        public App()
        {
            AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);

            m_errorLogger = new ErrorLogger
            {
                ErrorTextMethod = ErrorText,
                ExitOnUnhandledException = false,
                HandleUnhandledException = true,
                LogToEmail = false,
                LogToEventLog = true,
                LogToFile = true,
                LogToScreenshot = true,
                LogToUI = true
            };

            m_errorLogger.Initialize();
            
            m_defaultErrorText = m_errorLogger.ErrorTextMethod;

            Version appVersion = AssemblyInfo.EntryAssembly.Version;
            Title = AssemblyInfo.EntryAssembly.Title + " (v" + appVersion.Major + "." + appVersion.Minor + "." + appVersion.Build + ") ";
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the ID of the node user is currently connected to.
        /// </summary>
        public Guid NodeID
        {
            get => m_nodeID;
            set
            {
                m_nodeID = value;
                m_nodeID.SetAsCurrentNodeID();
            }
        }

        public string Title { get; }

        #endregion

        #region [ Methods ]

        private string ErrorText()
        {
            string errorMessage = m_defaultErrorText();
            Exception ex = m_errorLogger.LastException;

            if (ex is null)
                return errorMessage;

            if (string.Compare(ex.Message, "UnhandledException", StringComparison.OrdinalIgnoreCase) == 0 && ex.InnerException != null)
                ex = ex.InnerException;

            errorMessage = $"{errorMessage}\r\n\r\nError details: {ex.Message}";

            return errorMessage;
        }

        #endregion
    }
}
