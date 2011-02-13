//******************************************************************************************************
//  ServiceClient.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//
//******************************************************************************************************

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using TVA;
using TVA.Console;
using TVA.Reflection;
using TVA.Services.ServiceProcess;

namespace TimeSeriesFramework
{
    /// <summary>
    /// Represents a client that can remotely access the time-series framework service host.
    /// </summary>
    public partial class ServiceClientBase : Component
    {
        #region [ Members ]

        // Fields
        private bool m_telnetActive;
        private ConsoleColor m_originalBgColor;
        private ConsoleColor m_originalFgColor;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="ServiceClientBase"/> class.
        /// </summary>
        public ServiceClientBase()
            : base()
        {
            InitializeComponent();

            // Save the color scheme.
            m_originalBgColor = Console.BackgroundColor;
            m_originalFgColor = Console.ForegroundColor;

            // Register event handlers.
            m_clientHelper.AuthenticationFailure += ClientHelper_AuthenticationFailure;
            m_clientHelper.ReceivedServiceUpdate += ClientHelper_ReceivedServiceUpdate;
            m_clientHelper.ReceivedServiceResponse += ClientHelper_ReceivedServiceResponse;
            m_clientHelper.TelnetSessionEstablished += ClientHelper_TelnetSessionEstablished;
            m_clientHelper.TelnetSessionTerminated += ClientHelper_TelnetSessionTerminated;
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
                            Console.WriteLine("Attempting to stop the {0} Windows service...", serviceName);

                            serviceController.Stop();

                            // Can't wait forever for service to stop, so we time-out after 20 seconds
                            serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(20.0D));

                            if (serviceController.Status == ServiceControllerStatus.Stopped)
                                Console.WriteLine("Successfully stopped the {0} Windows service.", serviceName);
                            else
                                Console.WriteLine("Failed to stop the {0} Windows service after trying for 20 seconds...", serviceName);

                            // Add an extra line for visual separation of service termination status
                            Console.WriteLine("");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to stop the {0} Windows service: {1}\r\n", serviceName, ex.Message);
                    }
                }

                // If the service failed to stop or it is installed as stand-alone debug application, we try to forcibly stop any remaining running instances
                try
                {
                    Process[] instances = Process.GetProcessesByName(serviceName);

                    if (instances.Length > 0)
                    {
                        int total = 0;
                        Console.WriteLine("Attempting to stop running instances of the {0}...", serviceName);

                        // Terminate all instances of service running on the local computer
                        foreach (Process process in instances)
                        {
                            process.Kill();
                            total++;
                        }

                        if (total > 0)
                            Console.WriteLine(string.Format("Stopped {0} {1} instance{2}.", total, serviceName, total > 1 ? "s" : ""));

                        // Add an extra line for visual separation of process termination status
                        Console.WriteLine("");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to terminate running instances of the {0}: {1}\r\n", serviceName, ex.Message);
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
                        Console.WriteLine("Failed to restart the {0} Windows service: {1}\r\n", serviceName, ex.Message);
                    }
                }
            }
            else
            {
                if (arguments.Exists("server") || arguments.Exists("secret"))
                {
                    // Override default settings with user provided input. 
                    m_clientHelper.PersistSettings = false;
                    m_remotingClient.PersistSettings = false;

                    if (arguments.Exists("secret"))
                        m_remotingClient.SharedSecret = arguments["secret"];

                    if (arguments.Exists("server"))
                        m_remotingClient.ConnectionString = string.Format("Server={0}", arguments["server"]);
                }

                // Connect to service and send commands. 
                m_clientHelper.Connect();

                while (m_clientHelper.Enabled && string.Compare(userInput, "Exit", true) != 0)
                {
                    // Wait for a command from the user. 
                    userInput = Console.ReadLine();
                    // Write a blank line to the console.
                    Console.WriteLine();

                    if (!string.IsNullOrWhiteSpace(userInput))
                    {
                        // The user typed in a command and didn't just hit <ENTER>. 
                        switch (userInput.ToUpper())
                        {
                            case "CLS":
                                // User wants to clear the console window. 
                                Console.Clear();
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

            Console.Write(help.ToString());
        }

        private void ClientHelper_AuthenticationFailure(object sender, CancelEventArgs e)
        {
            // Prompt for credentials.
            StringBuilder prompt = new StringBuilder();
            prompt.AppendLine();
            prompt.AppendLine();
            prompt.Append("Connection to the service was rejected due to authentication failure. \r\n");
            prompt.Append("Enter the credentials to be used for authentication with the service.");
            prompt.AppendLine();
            prompt.AppendLine();
            Console.Write(prompt.ToString());

            // Capture the username.
            Console.Write("Enter username: ");
            m_clientHelper.Username = Console.ReadLine();

            // Capture the password.
            ConsoleKeyInfo key;
            Console.Write("Enter password: ");
            while ((key = Console.ReadKey(true)).KeyChar != '\r')
            {
                m_clientHelper.Password += key.KeyChar;
            }

            // Re-attempt connection with new credentials.
            e.Cancel = false;
            Console.WriteLine();
            Console.WriteLine();
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
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case UpdateType.Information:
                    Console.ForegroundColor = m_originalFgColor;
                    break;
                case UpdateType.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
            }

            Console.Write(e.Argument2);
            Console.ForegroundColor = m_originalFgColor;
        }

        /// <summary>
        /// Client helper service response reception handler.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="e">Event argument containing the service response.</param>
        protected virtual void ClientHelper_ReceivedServiceResponse(object sender, EventArgs<ServiceResponse> e)
        {
            string response = e.Argument.Type;
            string message = e.Argument.Message;

            // Handle response message, if any
            if (!string.IsNullOrWhiteSpace(response))
            {
                // Reponse types are formatted as "Command:Success" or "Command:Failure"
                string[] parts = response.Split(':');
                string action;
                bool success;

                if (parts.Length > 1)
                {
                    action = parts[0].Trim().ToTitleCase();
                    success = (string.Compare(parts[1].Trim(), "Success", true) == 0);
                }
                else
                {
                    action = response;
                    success = true;
                }

                if (success)
                {
                    if (string.IsNullOrWhiteSpace(message))
                        Console.Write(string.Format("{0} command processed successfully.\r\n\r\n", action));
                    else
                        Console.Write(string.Format("{0}\r\n\r\n", message));
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;

                    if (string.IsNullOrWhiteSpace(message))
                        Console.Write(string.Format("{0} failure.\r\n\r\n", action));
                    else
                        Console.Write(string.Format("{0} failure: {1}\r\n\r\n", action, message));

                    Console.ForegroundColor = m_originalFgColor;
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
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Clear();
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
            Console.BackgroundColor = m_originalBgColor;
            Console.ForegroundColor = m_originalFgColor;
            Console.Clear();
        }

        #endregion
    }
}