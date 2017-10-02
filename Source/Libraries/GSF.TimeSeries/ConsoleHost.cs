//******************************************************************************************************
//  ConsoleHostBase.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  08/14/2014 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Security.Principal;
using System.Text;
using System.Threading;
using GSF.Identity;
using GSF.Reflection;

namespace GSF.TimeSeries
{
    /// <summary>
    /// Console application operations used to host the time-series framework service.
    /// </summary>
    public class ConsoleHost
    {
        #region [ Members ]

        // Fields
        private readonly ServiceHostBase m_serviceHost;
        private readonly ConsoleColor m_originalForegroundColor;
        private readonly IPrincipal m_consoleHostPrincipal;
        private readonly Guid m_clientID;
        private readonly object m_displayLock;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates new <see cref="ConsoleHost"/>.
        /// </summary>
        /// <param name="serviceHost">Service host instance.</param>
        public ConsoleHost(ServiceHostBase serviceHost)
        {
            m_serviceHost = serviceHost;
            m_originalForegroundColor = System.Console.ForegroundColor;
            m_consoleHostPrincipal = new GenericPrincipal(new GenericIdentity("ConsoleHost"), new[] { "Administrator" });
            m_clientID = Guid.NewGuid();
            m_displayLock = new object();
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Runs the console host.
        /// </summary>
        public void Run()
        {
            if ((object)m_serviceHost == null)
                throw new NullReferenceException("Service host reference is null");

            string userInput = "";

            try
            {
                m_serviceHost.UpdatedStatus += m_serviceHost_UpdatedStatus;
                m_serviceHost.StartHostedService();
                m_serviceHost.SendRequest(m_consoleHostPrincipal, m_clientID, "Filter -Remove 0");

                while (!string.Equals(userInput, "Exit", StringComparison.OrdinalIgnoreCase))
                {
                    // Wait for a command from the user. 
                    userInput = System.Console.ReadLine();

                    // Write a blank line to the console.
                    WriteLine();

                    if (!string.IsNullOrWhiteSpace(userInput))
                    {
                        // The user typed in a command and didn't just hit <ENTER>. 
                        switch (userInput.ToUpper())
                        {
                            case "CLS":
                                // User wants to clear the console window. 
                                System.Console.Clear();
                                break;
                            case "EXIT":
                                // User wants to exit hosted session.
                                break;
                            default:
                                // User wants to send a request to the service. 
                                m_serviceHost.SendRequest(m_consoleHostPrincipal, m_clientID, userInput);

                                if (string.Compare(userInput, "Help", StringComparison.OrdinalIgnoreCase) == 0)
                                    DisplayHelp();

                                break;
                        }
                    }
                }
            }
            finally
            {
                m_serviceHost.StopHostedService();
                m_serviceHost.UpdatedStatus -= m_serviceHost_UpdatedStatus;
            }
        }

        private void m_serviceHost_UpdatedStatus(object sender, EventArgs<Guid, string, UpdateType> e)
        {
            if (e.Argument1 != m_clientID)
                return;

            lock (m_displayLock)
            {
                // Output status updates from the service to the console window.
                switch (e.Argument3)
                {
                    case UpdateType.Alarm:
                        System.Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case UpdateType.Warning:
                        System.Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                }

                Write(e.Argument2);
                System.Console.ForegroundColor = m_originalForegroundColor;
            }
        }

        private void DisplayHelp()
        {
            StringBuilder help = new StringBuilder();

            help.AppendFormat("Commands supported by {0}:", AssemblyInfo.EntryAssembly.Name);
            help.AppendLine();
            help.AppendLine();
            help.Append("Command".PadRight(20));
            help.Append(" ");
            help.Append("Description".PadRight(55));
            help.AppendLine();
            help.Append(new string('-', 20));
            help.Append(" ");
            help.Append(new string('-', 55));
            help.AppendLine();
            help.Append("Cls".PadRight(20));
            help.Append(" ");
            help.Append("Clears this console screen".PadRight(55));
            help.AppendLine();
            help.Append("Exit".PadRight(20));
            help.Append(" ");
            help.Append("Exits this console screen".PadRight(55));
            help.AppendLine();
            help.AppendLine();
            help.AppendLine();

            Write(help.ToString());
        }

        private void Write(string format, params object[] args)
        {
            lock (m_displayLock)
            {
                if (args.Length == 0)
                    System.Console.Write(format);
                else
                    System.Console.Write(format, args);
            }
        }

        private void WriteLine()
        {
            lock (m_displayLock)
            {
                System.Console.WriteLine();
            }
        }

        private void WriteLine(string format, params object[] args)
        {
            lock (m_displayLock)
            {
                if (args.Length == 0)
                    System.Console.WriteLine(format);
                else
                    System.Console.WriteLine(format, args);
            }
        }

        #endregion
    }
}
