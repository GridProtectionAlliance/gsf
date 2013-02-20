//******************************************************************************************************
//  ServiceClientBase.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  08/20/2010 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using GSF.Console;
using GSF.Identity;
using GSF.Reflection;
using GSF.Security;
using GSF.ServiceProcess;

namespace GSF.TimeSeries
{
    /// <summary>
    /// Represents a client that can remotely access the time-series framework service host.
    /// </summary>
    public partial class ServiceClientBase : Component
    {
        #region [ Members ]

        // Constants
        private const int VK_RETURN = 0x0D;
        private const int WM_KEYDOWN = 0x100;

        // Fields
        private bool m_innerLoopActive;
        private bool m_telnetActive;
        private ConsoleColor m_originalBgColor;
        private ConsoleColor m_originalFgColor;
        private readonly ManualResetEvent m_authenticationWaitHandle;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="ServiceClientBase"/> class.
        /// </summary>
        public ServiceClientBase()
        {
            InitializeComponent();

            // Save the color scheme.
            m_originalBgColor = System.Console.BackgroundColor;
            m_originalFgColor = System.Console.ForegroundColor;

            // Register event handlers.
            m_clientHelper.AuthenticationFailure += ClientHelper_AuthenticationFailure;
            m_clientHelper.ReceivedServiceUpdate += ClientHelper_ReceivedServiceUpdate;
            m_clientHelper.ReceivedServiceResponse += ClientHelper_ReceivedServiceResponse;
            m_clientHelper.TelnetSessionEstablished += ClientHelper_TelnetSessionEstablished;
            m_clientHelper.TelnetSessionTerminated += ClientHelper_TelnetSessionTerminated;

            // Authentication wait handle.
            m_authenticationWaitHandle = new ManualResetEvent(true);
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Handles service start event.
        /// </summary>
        /// <param name="args">Service start arguments.</param>
        public virtual void Start(string[] args)
        {
            string userInput = null;
            Arguments arguments = new Arguments(string.Join(" ", args));

            if (arguments.Exists("OrderedArg1") && arguments.Exists("restart"))
            {
                string serviceName = arguments["OrderedArg1"];

                // Attempt to access service controller for the specified Windows service
                ServiceController serviceController = ServiceController.GetServices().SingleOrDefault(svc => string.Compare(svc.ServiceName, serviceName, true) == 0);

                if (serviceController != null)
                {
                    try
                    {
                        if (serviceController.Status == ServiceControllerStatus.Running)
                        {
                            System.Console.WriteLine("Attempting to stop the {0} Windows service...", serviceName);

                            serviceController.Stop();

                            // Can't wait forever for service to stop, so we time-out after 20 seconds
                            serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(20.0D));

                            if (serviceController.Status == ServiceControllerStatus.Stopped)
                                System.Console.WriteLine("Successfully stopped the {0} Windows service.", serviceName);
                            else
                                System.Console.WriteLine("Failed to stop the {0} Windows service after trying for 20 seconds...", serviceName);

                            // Add an extra line for visual separation of service termination status
                            System.Console.WriteLine("");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine("Failed to stop the {0} Windows service: {1}\r\n", serviceName, ex.Message);
                    }
                }

                // If the service failed to stop or it is installed as stand-alone debug application, we try to forcibly stop any remaining running instances
                try
                {
                    Process[] instances = Process.GetProcessesByName(serviceName);

                    if (instances.Length > 0)
                    {
                        int total = 0;
                        System.Console.WriteLine("Attempting to stop running instances of the {0}...", serviceName);

                        // Terminate all instances of service running on the local computer
                        foreach (Process process in instances)
                        {
                            process.Kill();
                            total++;
                        }

                        if (total > 0)
                            System.Console.WriteLine("Stopped {0} {1} instance{2}.", total, serviceName, total > 1 ? "s" : "");

                        // Add an extra line for visual separation of process termination status
                        System.Console.WriteLine("");
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("Failed to terminate running instances of the {0}: {1}\r\n", serviceName, ex.Message);
                }

                // Attempt to restart Windows service...
                if (serviceController != null)
                {
                    try
                    {
                        // Refresh state in case service process was forcibly stopped
                        serviceController.Refresh();

                        if (serviceController.Status != ServiceControllerStatus.Running)
                            serviceController.Start();
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine("Failed to restart the {0} Windows service: {1}\r\n", serviceName, ex.Message);
                    }
                }
            }
            else
            {
                if (arguments.Exists("server"))
                {
                    // Override default settings with user provided input. 
                    m_clientHelper.PersistSettings = false;
                    m_remotingClient.PersistSettings = false;
                    m_remotingClient.ConnectionString = string.Format("Server={0}", arguments["server"]);
                }

                // Connect to service and send commands.
                while (!m_clientHelper.Enabled)
                {
                    m_authenticationWaitHandle.WaitOne();
                    m_clientHelper.Connect();

                    while (m_clientHelper.Enabled && string.Compare(userInput, "Exit", true) != 0)
                    {
                        m_innerLoopActive = true;

                        // Wait for a command from the user. 
                        userInput = System.Console.ReadLine();

                        // Write a blank line to the console.
                        System.Console.WriteLine();

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
                                    // User wants to exit the telnet session with the service. 
                                    if (m_telnetActive)
                                    {
                                        userInput = string.Empty;
                                        m_clientHelper.SendRequest("Telnet -disconnect");
                                    }
                                    break;
                                default:
                                    // User wants to send a request to the service. 
                                    m_clientHelper.SendRequest(userInput);
                                    if (string.Compare(userInput, "Help", true) == 0)
                                        DisplayHelp();

                                    break;
                            }
                        }
                    }

                    m_innerLoopActive = false;
                }
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

            System.Console.Write(help.ToString());
        }

        private void ClientHelper_AuthenticationFailure(object sender, CancelEventArgs e)
        {
            // Prompt for credentials.
            StringBuilder prompt = new StringBuilder();

            UserInfo userInfo;
            string username;
            StringBuilder passwordBuilder = new StringBuilder();

            prompt.AppendLine();
            prompt.AppendLine();
            prompt.Append("Connection to the service was rejected due to authentication failure. \r\n");
            prompt.Append("Enter the credentials to be used for authentication with the service.");
            prompt.AppendLine();
            prompt.AppendLine();
            System.Console.Write(prompt.ToString());

            // Tell outer connect loop to wait for authentication.
            m_authenticationWaitHandle.Reset();

            // If the inner loop is active, post enter to the console
            // to escape the Console.ReadLine() in the inner loop.
            if (m_innerLoopActive)
            {
                m_remotingClient.Enabled = false;
                IntPtr hWnd = Process.GetCurrentProcess().MainWindowHandle;
                NativeMethods.PostMessage(hWnd, WM_KEYDOWN, VK_RETURN, 0);
                Thread.Sleep(500);
            }

            // Capture the username.
            System.Console.Write("Enter username: ");
            username = System.Console.ReadLine();

            // Capture the password.
            ConsoleKeyInfo key;
            System.Console.Write("Enter password: ");
            while ((key = System.Console.ReadKey(true)).KeyChar != '\r')
            {
                passwordBuilder.Append(key.KeyChar);
            }

            // Set network credentials used when attempting AD authentication
            userInfo = new UserInfo(username);
            userInfo.Initialize();
            m_remotingClient.NetworkCredential = new NetworkCredential(userInfo.LoginID, passwordBuilder.ToString());

            // Set the username on the client helper.
            m_clientHelper.Username = username;
            m_clientHelper.Password = SecurityProviderUtility.EncryptPassword(passwordBuilder.ToString());

            // Done with the console; signal reconnect.
            m_authenticationWaitHandle.Set();

            // Re-attempt connection with new credentials.
            e.Cancel = true;
            System.Console.WriteLine();
            System.Console.WriteLine();
        }

        /// <summary>
        /// Client helper service update reception handler.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="e">Event argument containing update type and associated message data.</param>
        protected virtual void ClientHelper_ReceivedServiceUpdate(object sender, EventArgs<UpdateType, string> e)
        {
            // Output status updates from the service to the console window.
            switch (e.Argument1)
            {
                case UpdateType.Alarm:
                    System.Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case UpdateType.Information:
                    System.Console.ForegroundColor = m_originalFgColor;
                    break;
                case UpdateType.Warning:
                    System.Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
            }

            System.Console.Write(e.Argument2);
            System.Console.ForegroundColor = m_originalFgColor;
        }

        /// <summary>
        /// Client helper service response reception handler.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="e">Event argument containing the service response.</param>
        protected virtual void ClientHelper_ReceivedServiceResponse(object sender, EventArgs<ServiceResponse> e)
        {
            string sourceCommand;
            bool responseSuccess;

            if (ClientHelper.TryParseActionableResponse(e.Argument, out sourceCommand, out responseSuccess))
            {
                string message = e.Argument.Message;

                if (responseSuccess)
                {
                    if (string.IsNullOrWhiteSpace(message))
                        System.Console.Write("{0} command processed successfully.\r\n\r\n", sourceCommand);
                    else
                        System.Console.Write("{0}\r\n\r\n", message);
                }
                else
                {
                    System.Console.ForegroundColor = ConsoleColor.Red;

                    if (string.IsNullOrWhiteSpace(message))
                        System.Console.Write("{0} failure.\r\n\r\n", sourceCommand);
                    else
                        System.Console.Write("{0} failure: {1}\r\n\r\n", sourceCommand, message);

                    System.Console.ForegroundColor = m_originalFgColor;
                }
            }
        }

        /// <summary>
        /// Client helper telnet session established handler.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="e">Event arguments, if any.</param>
        protected virtual void ClientHelper_TelnetSessionEstablished(object sender, EventArgs e)
        {
            // Change the console color scheme to indicate active telnet session.
            m_telnetActive = true;
            System.Console.BackgroundColor = ConsoleColor.Blue;
            System.Console.ForegroundColor = ConsoleColor.Gray;
            System.Console.Clear();
        }

        /// <summary>
        /// Client helper telnet session terminated handler.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="e">Event arguments, if any.</param>
        protected virtual void ClientHelper_TelnetSessionTerminated(object sender, EventArgs e)
        {
            // Revert to original color scheme to indicate end of telnet session.
            m_telnetActive = false;
            System.Console.BackgroundColor = m_originalBgColor;
            System.Console.ForegroundColor = m_originalFgColor;
            System.Console.Clear();
        }

        #endregion
    }
}