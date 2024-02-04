//******************************************************************************************************
//  ServiceClientBase.cs - Gbtc
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
//  08/20/2010 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using GSF.Communication;
using GSF.Configuration;
using GSF.Console;
using GSF.Diagnostics;
using GSF.ErrorManagement;
using GSF.Identity;
using GSF.IO;
using GSF.Net.Security;
using GSF.Reflection;
using GSF.Security;
using GSF.ServiceProcess;
using GSF.Threading;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Broker;
using Logger = GSF.Diagnostics.Logger;

namespace GSF.TimeSeries
{
    /// <summary>
    /// Represents a client that can remotely access the time-series framework service host.
    /// </summary>
    public class ServiceClientBase : IDisposable
    {
        #region [ Members ]

        // Fields
        private bool m_telnetActive;
        private volatile bool m_authenticated;
        private volatile bool m_authenticationFailure;
        private volatile bool m_authenticationReset;
        private volatile bool m_isAzureAD;
        private readonly object m_displayLock;

        private readonly ConsoleColor m_originalBgColor;
        private readonly ConsoleColor m_originalFgColor;

        private ClientBase m_remotingClient;
        private ClientHelper m_clientHelper;
        private ErrorLogger m_errorLogger;

        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="ServiceClientBase"/> class.
        /// </summary>
        public ServiceClientBase()
        {
            Initialize();

            // Save the color scheme.
            m_originalBgColor = System.Console.BackgroundColor;
            m_originalFgColor = System.Console.ForegroundColor;
            m_displayLock = new object();

            // Register event handlers.
            m_clientHelper.AuthenticationSuccess += ClientHelper_AuthenticationSuccess;
            m_clientHelper.AuthenticationFailure += ClientHelper_AuthenticationFailure;
            m_clientHelper.ReceivedServiceUpdate += ClientHelper_ReceivedServiceUpdate;
            m_clientHelper.ReceivedServiceResponse += ClientHelper_ReceivedServiceResponse;
            m_clientHelper.TelnetSessionEstablished += ClientHelper_TelnetSessionEstablished;
            m_clientHelper.TelnetSessionTerminated += ClientHelper_TelnetSessionTerminated;
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="ServiceClientBase"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~ServiceClientBase() => 
            Dispose(false);

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes the remoting client, client helper, and error logger.
        /// </summary>
        public void Initialize()
        {
            CategorizedSettingsElementCollection remotingClientSettings;

            try
            {
                remotingClientSettings = ConfigurationFile.Current.Settings["remotingClient"];

                // Setup default logging parameters for remote console applications
                string logPath = FilePath.GetAbsolutePath("Logs");

                if (!Directory.Exists(logPath))
                    Directory.CreateDirectory(logPath);

                Logger.FileWriter.SetPath(logPath);
                Logger.FileWriter.SetLoggingFileCount(10);
            }
            catch
            {
                remotingClientSettings = null;
            }

            if (remotingClientSettings is not null)
            {
                if (remotingClientSettings.Cast<CategorizedSettingsElement>().Any(element => element.Name.Equals("EnabledSslProtocols", StringComparison.OrdinalIgnoreCase) && !element.Value.Equals("None", StringComparison.OrdinalIgnoreCase)))
                    m_remotingClient = InitializeTlsClient();
                else
                    m_remotingClient = InitializeTcpClient();
            }
            else
            {
                m_remotingClient = InitializeTlsClient();
            }

            string[] args = Arguments.ToArgs(Environment.CommandLine);

            string filter = Enumerable.Range(0, args.Length)
                                .Where(index => args[index].StartsWith("--filter=", StringComparison.OrdinalIgnoreCase))
                                .Select(index => Regex.Replace(args[index], "^--filter=", "", RegexOptions.IgnoreCase))
                                .FirstOrDefault() ?? ClientHelper.DefaultStatusMessageFilter;

            m_clientHelper = new ClientHelper
            {
                PersistSettings = true,
                RemotingClient = m_remotingClient,
                StatusMessageFilter = filter
            };

            m_clientHelper.Initialize();

            m_errorLogger = new ErrorLogger
            {
                LogToEventLog = false,
                LogToUI = true,
                PersistSettings = true
            };

            m_errorLogger.ErrorLog.FileName = "ServiceClient.ErrorLog.txt";
            m_errorLogger.ErrorLog.Initialize();

            m_errorLogger.Initialize();
        }

        /// <summary>
        /// Handles service start event.
        /// </summary>
        /// <param name="args">Service start arguments.</param>
        public virtual void Start(string[] args)
        {
            string userInput = string.Empty;
            Arguments arguments = new(string.Join(" ", Arguments.ToArgs(Environment.CommandLine).Where(arg => !arg.StartsWith("--filter=", StringComparison.OrdinalIgnoreCase)).Skip(1)));

            // Handle external service restart requests
            if (arguments.Exists("OrderedArg1") && (arguments.Exists("restart") || arguments.Exists("restartWithDelay")))
            {
                string serviceName = arguments["OrderedArg1"];
                
                if (arguments.Exists("restartWithDelay"))
                {
                    int delay = int.Parguments["restartWithDelay"];
                    Thread.Sleep(delay*1000);
                }

                if (GSF.Common.IsPosixEnvironment)
                {
                    string serviceCommand = FilePath.GetAbsolutePath(serviceName);

                    try
                    {
                        Command.Execute(serviceCommand, "stop");
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = $"Failed to stop the {serviceName} daemon: {ex.Message}";
                        WriteError(errorMessage);
                        Logger.SwallowException(ex, errorMessage);
                    }

                    try
                    {
                        Command.Execute(serviceCommand, "start");
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = $"Failed to restart the {serviceName} daemon: {ex.Message}";
                        WriteError(errorMessage);
                        Logger.SwallowException(ex, errorMessage);
                    }
                }
                else
                {
                    // Attempt to access service controller for the specified Windows service
                    ServiceController serviceController = ServiceController.GetServices().SingleOrDefault(svc => string.Compare(svc.ServiceName, serviceName, StringComparison.OrdinalIgnoreCase) == 0);

                    if (serviceController is not null)
                    {
                        try
                        {
                            if (serviceController.Status == ServiceControllerStatus.Running)
                            {
                                WriteLine($"Attempting to stop the {serviceName} Windows service...");

                                serviceController.Stop();

                                // Can't wait forever for service to stop, so we time-out after 20 seconds
                                serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(20.0D));

                                WriteLine(serviceController.Status == ServiceControllerStatus.Stopped ? 
                                    $"Successfully stopped the {serviceName} Windows service." : 
                                    $"Failed to stop the {serviceName} Windows service after trying for 20 seconds...");

                                // Add an extra line for visual separation of service termination status
                                WriteLine();
                            }
                        }
                        catch (Exception ex)
                        {
                            string errorMessage = $"Failed to stop the {serviceName} Windows service: {ex.Message}";
                            WriteError(errorMessage);
                            Logger.SwallowException(ex, errorMessage);
                        }
                    }

                    // If the service failed to stop or it is installed as stand-alone debug application, we try to forcibly stop any remaining running instances
                    try
                    {
                        Process[] instances = Process.GetProcessesByName(serviceName);

                        if (instances.Length > 0)
                        {
                            int total = 0;
                            WriteLine($"Attempting to stop running instances of the {serviceName}...");

                            // Terminate all instances of service running on the local computer
                            foreach (Process process in instances)
                            {
                                process.Kill();
                                total++;
                            }

                            if (total > 0)
                                WriteLine($"Stopped {total:N0} {serviceName} instance{(total > 1 ? "s" : "")}.");

                            // Add an extra line for visual separation of process termination status
                            WriteLine();
                        }
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = $"Failed to terminate running instances of the {serviceName}: {ex.Message}";
                        WriteError(errorMessage);
                        Logger.SwallowException(ex, errorMessage);
                    }

                    // Attempt to restart Windows service...
                    if (serviceController is not null)
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
                            string errorMessage = $"Failed to restart the {serviceName} Windows service: {ex.Message}";
                            WriteError(errorMessage);
                            Logger.SwallowException(ex, errorMessage);
                        }
                    }
                }

                return;
            }

            // Handle clearing of dynamic RazorEngine assemblies
            if (arguments.Exists("clearCache"))
            {
                string assemblyDirectory = FilePath.GetAbsolutePath(Common.DynamicAssembliesFolderName);

                if (!Directory.Exists(assemblyDirectory))
                    return;

                string[] razorFolders = Directory.EnumerateDirectories(assemblyDirectory, "RazorEngine_*", SearchOption.TopDirectoryOnly).ToArray();

                foreach (string razorFolder in razorFolders)
                {
                    try
                    {
                        Directory.Delete(razorFolder, true);
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = $"Failed to remove temporary dynamic assembly folder: {razorFolder}";
                        WriteLine(errorMessage);
                        Logger.SwallowException(ex, errorMessage);
                    }
                }

                return;
            }

            // Handle normal remote console operations
            if (arguments.Exists("server"))
            {
                // Override default settings with user provided input. 
                m_clientHelper.PersistSettings = false;
                m_remotingClient.PersistSettings = false;
                m_remotingClient.ConnectionString = $"Server={arguments["server"]}";
            }

            long lastConnectAttempt = 0;

            // Connect to service and send commands.
            while (userInput is not null && !string.Equals(userInput, "Exit", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    // Do not reattempt connection too quickly
                    while (DateTime.UtcNow.Ticks - lastConnectAttempt < Ticks.PerSecond)
                        Thread.Sleep(200);

                    lastConnectAttempt = DateTime.UtcNow.Ticks;

                    ICancellationToken timeoutCancellationToken = new Threading.CancellationToken();

                    if (System.Console.IsInputRedirected)
                    {
                        // If the client is invoked as part of a command line script,
                        // implement a 5-second timeout in case of connectivity issues
                        Action timeoutAction = () =>
                        {
                            if (!timeoutCancellationToken.IsCancelled)
                                Environment.Exit(1);
                        };

                        timeoutAction.DelayAndExecute(5000);
                    }

                    if (!m_authenticationFailure)
                    {
                        // If there has been no authentication
                        // failure, connect normally
                        Connect();
                    }
                    else
                    {
                        AzureADSettings settings = null;
                        m_isAzureAD = false;

                        try
                        {
                            settings = AzureADSettings.Load();
                        }
                        catch (Exception ex)
                        {
                            Log.Publish(MessageLevel.Info, $"{nameof(ServiceClientBase)}.{nameof(Start)}", "AzureADSettings.Load()", exception: ex);
                        }

                        // If last username was an Azure AD account, try login with cached MSAL user
                        if (!m_authenticationReset && (settings?.Enabled ?? false) && (m_clientHelper.Username?.Contains('@') ?? false))
                        {
                            (string username, string password) result = LoginToAzureAD(settings);
                
                            if (!string.IsNullOrEmpty(result.username))
                            {
                                m_isAzureAD = true;
                                Connect(result.username, result.password);
                            }
                        }

                        if (m_authenticationFailure)
                        {
                            StringBuilder username = new();
                            StringBuilder password = new();

                            // If there has been an authentication failure,
                            // prompt the user for new credentials
                            PromptForCredentials(settings, username, password);

                            // Skip setting network credentials if using Azure AD user
                            if (!m_isAzureAD)
                            {
                                try
                                {
                                    // Attempt to set network credentials used when attempting AD authentication
                                    using UserInfo userInfo = new(username.ToString());
                                    userInfo.Initialize();
                                    SetNetworkCredential(new NetworkCredential(userInfo.LoginID, password.ToString()));
                                }
                                catch (Exception ex)
                                {
                                    // Even if this fails, we can still pass along default credentials
                                    SetNetworkCredential(null);
                                    Logger.SwallowException(ex);
                                }
                            }

                            Connect(username.ToString(), password.ToString());
                        }
                    }

                    timeoutCancellationToken.Cancel();

                    while (m_authenticated && m_clientHelper.Enabled && userInput is not null && !string.Equals(userInput, "Exit", StringComparison.OrdinalIgnoreCase))
                    {
                        // Wait for a command from the user. 
                        userInput = System.Console.ReadLine()?.Trim();

                        // Write a blank line to the console.
                        WriteLine();

                        if (string.IsNullOrWhiteSpace(userInput))
                            continue;

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

                            case "LOGIN":
                                m_authenticated = false;
                                m_authenticationFailure = true;
                                m_authenticationReset = true;
                                break;

                            default:
                                // User wants to send a request to the service. 
                                m_clientHelper.SendRequest(userInput);

                                if (string.Compare(userInput, "Help", StringComparison.OrdinalIgnoreCase) == 0)
                                    DisplayHelp();

                                break;
                        }
                    }

                    m_clientHelper.Disconnect();
                }
                catch (Exception ex)
                {
                    // Errors during the outer connection loop
                    // should simply force an attempt to reconnect
                    m_clientHelper.Disconnect();
                    Logger.SwallowException(ex);
                }
            }
        }

        /// <summary>
        /// Releases all the resources used by the <see cref="ServiceClientBase"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ServiceClientBase"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (m_disposed)
                return;

            try
            {
                if (!disposing)
                    return;

                m_clientHelper?.Dispose();
                m_remotingClient?.Dispose();
                m_errorLogger?.Dispose();

                m_clientHelper = null;
                m_remotingClient = null;
                m_errorLogger = null;
            }
            finally
            {
                m_disposed = true;  // Prevent duplicate dispose.
            }
        }

        private TcpClient InitializeTcpClient()
        {
            TcpClient remotingClient = new()
            {
                ConnectionString = "Server=localhost:8500",
                IgnoreInvalidCredentials = true,
                PayloadAware = true,
                PersistSettings = true,
                SettingsCategory = "RemotingClient"
            };

            remotingClient.Initialize();

            return remotingClient;
        }

        private TlsClient InitializeTlsClient()
        {
            TlsClient remotingClient = new()
            {
                ConnectionString = "Server=localhost:8500",
                IgnoreInvalidCredentials = true,
                PayloadAware = true,
                PersistSettings = true,
                SettingsCategory = "RemotingClient",
                TrustedCertificatesPath = $"Certs{Path.DirectorySeparatorChar}Remotes",
                ValidChainFlags = X509ChainStatusFlags.UntrustedRoot,
                ValidPolicyErrors = SslPolicyErrors.RemoteCertificateChainErrors
            };

            remotingClient.Initialize();

            // Override remote certificate validation so that we always
            // accept localhost, but fall back on SimplePolicyChecker.
            remotingClient.RemoteCertificateValidationCallback = RemoteCertificateValidationCallback;

            return remotingClient;
        }

        private void Connect()
        {
            m_authenticated = false;
            m_authenticationFailure = false;
            
            m_clientHelper.Connect();

            if (m_authenticationFailure)
                m_clientHelper.Disconnect();
        }

        private void Connect(string username, string password)
        {
            m_clientHelper.Username = username.ToNonNullString();

            // If the communications channel is secured or the user
            // has entered a blank password, send the password as-is
            if (string.IsNullOrEmpty(password) || m_clientHelper.RemotingClient is TlsClient)
            {
                m_clientHelper.Password = password.ToNonNullString();
                Connect();
            }

            // If the client fails to connect with the as-is password,
            // attempt to connect again with an encrypted password
            // because the server may only be authenticating against
            // the encrypted password
            if (m_clientHelper.RemotingClient.Enabled || string.IsNullOrEmpty(password))
                return;

            m_clientHelper.Password = SecurityProviderUtility.EncryptPassword(password);
            Connect();
        }

        private void PromptForCredentials(AzureADSettings settings, StringBuilder username, StringBuilder password)
        {
            StringBuilder prompt = new();
            m_isAzureAD = false;

            lock (m_displayLock)
            {
                prompt.AppendLine();
                prompt.AppendLine();
                prompt.AppendLine("Connection to the service was rejected due to authentication failure.");
                prompt.AppendLine("Enter the credentials to be used for authentication with the service.");

                if (settings?.Enabled ?? false)
                {
                    prompt.AppendLine();
                    prompt.AppendLine("To authenticate with Azure AD, enter 'azure' as the user name, without quotes,");
                    prompt.AppendLine("as needed this will pop-up a dialog to authenticate with Azure AD.");
                }

                prompt.AppendLine();

                Write(prompt.ToString());

                // Capture the user name.
                Write("Enter user name: ");

                username.Append(System.Console.ReadLine());

                // See if user requests Azure AD authentication
                if ((settings?.Enabled ?? false) && username.ToString().Equals("azure", StringComparison.OrdinalIgnoreCase))
                {
                    (string username, string password) result = LoginToAzureAD(settings);

                    if (!string.IsNullOrEmpty(result.username))
                    {
                        username.Clear().Append(result.username);
                        password.Clear().Append(result.password);
                        m_isAzureAD = true;
                    }

                    return;
                }

                // Capture the password.
                Write("Enter password: ");

                char endOfLine = GSF.Common.IsPosixEnvironment ? '\n' : '\r';

                ConsoleKeyInfo key;
                while ((key = System.Console.ReadKey(true)).KeyChar != endOfLine)
                {
                    switch (key.Key)
                    {
                        case ConsoleKey.Backspace:
                            if (password.Length > 0)
                                password.Remove(password.Length - 1, 1);

                            break;

                        case ConsoleKey.Escape:
                            password.Clear();
                            break;

                        default:
                            password.Append(key.KeyChar);
                            break;
                    }
                }

                WriteLine();
            }
        }

        private (string, string) LoginToAzureAD(AzureADSettings settings)
        {
            WriteLine($"{Environment.NewLine}Logging into Azure AD...");

            IPublicClientApplication app = null;
            IAccount account;

            (AuthenticationResult, Exception) acquireTokenInteractive(IEnumerable<string> scopes)
            {
                try
                {
                    [DllImport("kernel32.dll")]
                    static extern IntPtr GetConsoleWindow();

                    AcquireTokenInteractiveParameterBuilder authParams = app!.AcquireTokenInteractive(scopes)
                        .WithAccount(account)
                        .WithParentActivityOrWindow(GetConsoleWindow())
                        .WithPrompt(Prompt.NoPrompt);

                    return (authParams.ExecuteAsync().Result, null);
                }
                catch (AggregateException ex)
                {
                    throw new InvalidOperationException(string.Join("; ", ex.Flatten().InnerExceptions.Select(inex => inex.Message)), ex);
                }
                catch (Exception ex)
                {
                    Log.Publish(MessageLevel.Error, nameof(PromptForCredentials), "Azure AD Error Acquiring Token", exception: ex);
                    WriteError($"Azure AD Error Acquiring Token:{Environment.NewLine}{ex.Message}");
                    return (null, ex);
                }
            }

            try
            {
                PublicClientApplicationBuilder appParams = PublicClientApplicationBuilder.Create(settings.ClientID)
                    .WithAuthority(settings.Authority)
                    .WithDefaultRedirectUri()
                    .WithBrokerPreview();

                app = appParams.Build();
                TokenCacheHelper.EnableSerialization(app.UserTokenCache);

                account = app.GetAccountsAsync().Result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Log.Publish(MessageLevel.Error, nameof(PromptForCredentials), nameof(MsalUiRequiredException), exception: ex);
                account = PublicClientApplication.OperatingSystemAccount;
            }

            AuthenticationResult authResult;
            Exception exception = null;
            string[] scopes = { "user.read" };

            try
            {
                authResult = app?.AcquireTokenSilent(scopes, account).ExecuteAsync().Result;
            }
            catch (MsalUiRequiredException ex)
            {
                Log.Publish(MessageLevel.Info, nameof(PromptForCredentials), nameof(MsalUiRequiredException), exception: ex);
                (authResult, exception) = acquireTokenInteractive(scopes);
            }
            catch (Exception ex)
            {
                Log.Publish(MessageLevel.Error, nameof(PromptForCredentials), "Azure AD Error Acquiring Token Silently", exception: ex);
                (authResult, exception) = acquireTokenInteractive(scopes);
            }

            if (authResult is null)
            {
                WriteError($"Azure AD Failed to Acquire Authorization Result:{Environment.NewLine}No result retrieved for silent nor interactive authorization: {exception?.Message}");
                return (null, null);
            }

            WriteLine($"Azure AD login successful, validating user \"{authResult.Account.Username}\" with security service...{Environment.NewLine}");

            return (authResult.Account.Username, authResult.AccessToken);
        }

        private void SetNetworkCredential(NetworkCredential credential)
        {
            switch (m_remotingClient)
            {
                case TlsClient tlsClient:
                    tlsClient.NetworkCredential = credential;
                    break;
                case TcpClient tcpClient:
                    tcpClient.NetworkCredential = credential;
                    break;
            }
        }

        private bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (m_remotingClient is not TlsClient remotingClient)
                return false;

            if (remotingClient.Client.RemoteEndPoint is IPEndPoint remoteEndPoint)
            {
                // Create an exception and do not check policy for localhost
                IPHostEntry localhost = Dns.GetHostEntry("localhost");

                if (localhost.AddressList.Any(address => address.Equals(remoteEndPoint.Address)))
                    return true;
            }

            // Not connected to localhost, so use the policy checker
            SimplePolicyChecker policyChecker = new()
            {
                ValidPolicyErrors = remotingClient.ValidPolicyErrors,
                ValidChainFlags = remotingClient.ValidChainFlags
            };

            return policyChecker.ValidateRemoteCertificate(sender, certificate, chain, sslPolicyErrors);

        }

        private void DisplayHelp()
        {
            StringBuilder help = new();

            help.AppendFormat("Commands supported by {0}:", AssemblyInfo.EntryAssembly.Name);
            help.AppendLine();
            help.AppendLine();
            help.Append(nameof(Command).PadRight(20));
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
            help.Append("Login".PadRight(20));
            help.Append(" ");
            help.Append("Disconnects from server and prompts for credentials".PadRight(55));
            help.AppendLine();
            help.Append("Exit".PadRight(20));
            help.Append(" ");
            help.Append("Exits this console screen".PadRight(55));
            help.AppendLine();
            help.AppendLine();
            help.AppendLine();

            Write(help.ToString());
        }

        /// <summary>
        /// Client helper authentication success reception handler.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="e">Event argument.</param>
        private void ClientHelper_AuthenticationSuccess(object sender, EventArgs e)
        {
            // Post authentication operation for Azure AD clients, clear the password from
            // the client helper so it will not be serialized to user configuration files.
            // This client authentication token is temporal and will not be valid after it
            // is expired by Azure AD. It is also not needed for subsequent operations.
            if (m_isAzureAD)
                m_clientHelper.Password = "";

            m_authenticated = true;
            m_authenticationReset = false;

            try
            {
                // As soon as client is successfully authenticated, flush user cache
                // so that even a hard exit will allow auto-login user at next run
                m_clientHelper.SaveSettings();
            }
            catch (Exception ex)
            {
                // Any failure here is not catastrophic
                Logger.SwallowException(ex);
            }
        }

        /// <summary>
        /// Client helper authentication failure reception handler.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="e">Event argument.</param>
        private void ClientHelper_AuthenticationFailure(object sender, CancelEventArgs e)
        {
            // Post authentication operation for Azure AD clients, clear the password from
            // the client helper so it will not be serialized to user configuration files.
            // This client authentication token is temporal and will not be valid after it
            // is expired by Azure AD. It is also not needed for subsequent operations.
            if (m_isAzureAD)
                m_clientHelper.Password = "";

            m_authenticationFailure = true;
        }

        /// <summary>
        /// Client helper service update reception handler.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="e">Event argument containing update type and associated message data.</param>
        protected virtual void ClientHelper_ReceivedServiceUpdate(object sender, EventArgs<UpdateType, string> e)
        {
            lock (m_displayLock)
            {
                // Output status updates from the service to the console window.
                System.Console.ForegroundColor = e.Argument1 switch
                {
                    UpdateType.Alarm => ConsoleColor.Red,
                    UpdateType.Information => m_originalFgColor,
                    UpdateType.Warning => ConsoleColor.Yellow,
                    _ => System.Console.ForegroundColor
                };

                Write(e.Argument2);
                System.Console.ForegroundColor = m_originalFgColor;
            }
        }

        /// <summary>
        /// Client helper service response reception handler.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="e">Event argument containing the service response.</param>
        protected virtual void ClientHelper_ReceivedServiceResponse(object sender, EventArgs<ServiceResponse> e)
        {
            if (!ClientHelper.TryParseActionableResponse(e.Argument, out string sourceCommand, out bool responseSuccess))
                return;

            string message = e.Argument.Message;

            lock (m_displayLock)
            {
                if (responseSuccess)
                {
                    if (string.IsNullOrWhiteSpace(message))
                        WriteLine($"{sourceCommand} command processed successfully.{Environment.NewLine}");
                    else
                        WriteLine($"{message}{Environment.NewLine}");
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(message))
                        WriteError($"{sourceCommand} failure.");
                    else
                        WriteError($"{sourceCommand} failure: {message}");
                }
            }

            try
            {
                // Handle reports coming from service
                if (!responseSuccess || !sourceCommand.Equals("GetReport", StringComparison.OrdinalIgnoreCase))
                    return;

                if (e.Argument.Attachments[0] is not byte[] reportData)
                    return;

                string tempPath = Path.Combine(Path.GetTempPath(), $"{Process.GetCurrentProcess().Id}.pdf");
                File.WriteAllBytes(tempPath, reportData);
                Process.Start(tempPath);
            }
            catch (Exception ex)
            {
                lock (m_displayLock)
                    WriteError($"Unable to display report due to exception: {ex.Message}");

                Logger.SwallowException(ex);
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

            lock (m_displayLock)
            {
                System.Console.BackgroundColor = ConsoleColor.Blue;
                System.Console.ForegroundColor = ConsoleColor.Gray;
                System.Console.Clear();
            }
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

            lock (m_displayLock)
            {
                System.Console.BackgroundColor = m_originalBgColor;
                System.Console.ForegroundColor = m_originalFgColor;
                System.Console.Clear();
            }
        }

        private void Write(string message)
        {
            lock (m_displayLock)
                System.Console.Write(message);
        }

        private void WriteLine(string message = "")
        {
            lock (m_displayLock)
                System.Console.WriteLine(message);
        }

        private void WriteError(string message)
        {
            lock (m_displayLock)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine($"{message}{Environment.NewLine}");
                System.Console.ForegroundColor = m_originalFgColor;
            }
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly LogPublisher Log = Logger.CreatePublisher(typeof(ServiceClientBase), MessageClass.Component);

        #endregion
    }
}