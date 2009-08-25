using System;
using System.ComponentModel;
using System.Text;
using TVA;
using TVA.Console;
using TVA.Reflection;
using TVA.Services;

namespace UDPRebroadcasterConsole
{
    public partial class ServiceClient : Component
    {
        #region [ Members ]

        // Fields
        private bool m_telnetActive;
        private ConsoleColor m_originalBgColor;
        private ConsoleColor m_originalFgColor;

        #endregion

        #region [ Constructors ]

        public ServiceClient()
            : base()
        {
            InitializeComponent();

            // Register event handlers.
            m_clientHelper.AuthenticationFailure += ClientHelper_AuthenticationFailure;
            m_clientHelper.ReceivedServiceUpdate += ClientHelper_ReceivedServiceUpdate;
            m_clientHelper.ReceivedServiceResponse += ClientHelper_ReceivedServiceResponse;
            m_clientHelper.TelnetSessionEstablished += ClientHelper_TelnetSessionEstablished;
            m_clientHelper.TelnetSessionTerminated += ClientHelper_TelnetSessionTerminated;
        }

        #endregion

        #region [ Methods ]

        public void Start(string[] args)
        {
            string userInput = null;
            Arguments arguments = new Arguments(string.Join(" ", args));

            if (arguments.Exists("server"))
            {
                // Override default settings with user provided input. 
                m_clientHelper.PersistSettings = false;
                m_remotingClient.PersistSettings = false;
                if (arguments.Exists("server"))
                    m_remotingClient.ConnectionString = string.Format("Server={0}", arguments["server"]);
            }

            // Connect to service and send commands. 
            m_clientHelper.Connect();
            while (m_clientHelper.Enabled &&
                   string.Compare(userInput, "Exit", true) != 0)
            {
                // Wait for a command from the user. 
                userInput = Console.ReadLine();
                // Write a blank line to the console.
                Console.WriteLine();

                if (!string.IsNullOrEmpty(userInput))
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
            // Prompt for authentication method.
            StringBuilder prompt = new StringBuilder();
            prompt.Append("Remote connection was has rejected due to authentication failure. Please ");
            prompt.Append("select from one of the options below to re-authenticate the remote connection:");
            prompt.AppendLine();
            prompt.AppendLine();
            prompt.Append("[0] Abort (no retry)");
            prompt.AppendLine();
            prompt.Append("[1] NTLM Authentication");
            prompt.AppendLine();
            prompt.Append("[2] Kerberos Authentication");
            prompt.AppendLine();
            prompt.AppendLine();
            prompt.Append("Selection: ");
            Console.Write(prompt.ToString());

            // Capture authentication method selection.
            int selection;
            int.TryParse(Console.ReadLine(), out selection);

            Console.WriteLine();
            if (selection == 1)         // NTLM Authentication
            {
                // Capture the username.
                string username = "";
                Console.Write("Enter username: ");
                username = Console.ReadLine();

                // Capture the password.
                string password = "";
                ConsoleKeyInfo key;
                Console.Write("Enter password: ");
                while ((key = Console.ReadKey(true)).KeyChar != '\r')
                {
                    password += key.KeyChar;
                }

                // Update authentication parameters.
                e.Cancel = false;
                m_clientHelper.AuthenticationMethod = IdentityToken.Ntlm;
                m_clientHelper.AuthenticationInput = username + ":" + password;
            }
            else if (selection == 2)    // Kerberos Authentication
            {
                // Update authentication parameters.
                e.Cancel = false;
                Console.Write("Enter service principal: ");
                m_clientHelper.AuthenticationMethod = IdentityToken.Kerberos;
                m_clientHelper.AuthenticationInput = Console.ReadLine();
            }
            Console.WriteLine();
            Console.WriteLine();
        }

        private void ClientHelper_ReceivedServiceUpdate(object sender, EventArgs<string> e)
        {
            // Output status updates from the service to the console window.
            Console.Write(e.Argument);
        }

        private void ClientHelper_ReceivedServiceResponse(object sender, EventArgs<ServiceResponse> e)
        {
            // TODO: Handle custom service responses here.
            Console.Write(string.Format("Received custom response \"{0}\" from remote service.\r\n\r\n", e.Argument.Type));
        }

        private void ClientHelper_TelnetSessionEstablished(object sender, EventArgs e)
        {
            // Save the current state.
            m_telnetActive = true;
            m_originalBgColor = Console.BackgroundColor;
            m_originalFgColor = Console.ForegroundColor;

            // Change the console color scheme to indicate active telnet session.
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Clear();
        }

        private void ClientHelper_TelnetSessionTerminated(object sender, EventArgs e)
        {
            // Revert to saved state.
            m_telnetActive = false;
            Console.BackgroundColor = m_originalBgColor;
            Console.ForegroundColor = m_originalFgColor;
            Console.Clear();
        }

        #endregion
    }
}
