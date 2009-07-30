//*******************************************************************************************************
//  Arguments.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  09/12/2004 - James R. Carroll
//       Generated original version of source code.
//  03/28/2006 - Pinal C. Patel
//       Migrated 2.0 version of source code from 1.1 source (TVA.Console)
//  10/09/2007 - James R. Carroll / Pinal C. Patel
//       Fixed stand-alone argument bug at end of line and changed class to use generic Dictionary class
//  09/15/2008 - James R. Carroll
//      Converted to C#.
//  09/26/2008 - Pinal C. Patel
//      Entered code comments.
//
//*******************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TVA.Collections;

namespace TVA.Console
{
    /// <summary>
    /// A class to parse command-line command containing ordered arguments along with optional arguments.
    /// </summary>
    /// <example>
    /// This example shows how to parse a command-line command that does not contain an executable name:
    /// <code>
    /// using System;
    /// using TVA;
    /// using TVA.Console;
    ///
    /// class Program
    /// {
    ///     static void Main()
    ///     {
    ///         Arguments args = new Arguments("Sample.txt -wrap=true");
    ///         string file = args["OrderedArg1"];
    ///         bool wrapText = args["wrap"].ParseBoolean();
    ///        
    ///         Console.WriteLine(string.Format("File: {0}", file));
    ///         Console.WriteLine(string.Format("Wrap text: {0}", wrapText));
    ///         Console.ReadLine();
    ///     }
    /// }
    /// </code>
    /// This example shows how to parse a command-line command that contains an executable name in it:
    /// <code>
    /// using System;
    /// using TVA.Console;
    ///
    /// class Program
    /// {
    ///     static void Main()
    ///     {
    ///         // Environment.CommandLine = @"""c:\program files\tva\theme application\app.exe"" Document1.dcx -theme=default"
    ///         Arguments args = new Arguments(Environment.CommandLine, true);
    ///         string doc = args["OrderedArg1"];
    ///         string theme = args["theme"];
    ///        
    ///         Console.WriteLine(string.Format("Document: {0}", doc));
    ///         Console.WriteLine(string.Format("Application theme: {0}", theme));
    ///         Console.ReadLine();
    ///     }
    /// }
    /// </code>
    /// </example>
    [Serializable()]
    public class Arguments : IEnumerable, IEnumerable<KeyValuePair<string, string>>
    {
        #region [ Members ]

        // Fields
        private string m_commandLine;
        private string m_orderedArgID;
        private int m_orderedArgCount;
        private Dictionary<string, string> m_arguments;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="Arguments"/> class.
        /// </summary>
        /// <param name="commandLine">The command-line command to be parsed.</param>
        public Arguments(string commandLine)
            : this(commandLine, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Arguments"/> class.
        /// </summary>
        /// <param name="commandLine">The command-line command to be parsed.</param>
        /// <param name="skipFirstArgument">true if the first argument in the command-line command is to be skipped from being processed; otherwise false.</param>
        public Arguments(string commandLine, bool skipFirstArgument)
            : this(commandLine, "OrderedArg", skipFirstArgument)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Arguments"/> class.
        /// </summary>
        /// <param name="commandLine">The command-line command to be parsed.</param>
        /// <param name="orderedArgID">The prefix to be used in the identifier of ordered arguments.</param>
        public Arguments(string commandLine, string orderedArgID)
            : this(commandLine, orderedArgID, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Arguments"/> class.
        /// </summary>
        /// <param name="commandLine">The command-line command to be parsed.</param>
        /// <param name="orderedArgID">The prefix to be used in the identifier of ordered arguments.</param>
        /// <param name="skipFirstArgument">true if the first argument in the command-line command is to be skipped from being processed; otherwise false.</param>
        public Arguments(string commandLine, string orderedArgID, bool skipFirstArgument)
        {
            Regex spliter = new Regex("^-{1,2}|^/|=|:", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            Regex remover = new Regex("^[\'\"]?(.*?)[\'\"]?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            string parameter = null;
            string argument;
            string[] parts;

            // We create a case-insensitive dictionary for parameters
            m_arguments = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            m_orderedArgCount = 0;
            m_commandLine = commandLine;
            m_orderedArgID = orderedArgID;

            // Valid parameters forms:
            //   {-,/,--}param{=,:}((",')value(",'))
            // Examples:
            //   -param1=value1 --param2 /param3:"Test-:-work"
            //   /param4=happy -param5 '--=nice=--'
            string[] args = ParseCommand(m_commandLine);

            if (skipFirstArgument && args.Length > 0)
                args = args.Copy(1, args.Length - 1);

            foreach (string arg in args)
            {
                // Found just a parameter in last pass...
                // The last parameter is still waiting, with no value, set it to nothing.
                if (parameter != null)
                {
                    if (!m_arguments.ContainsKey(parameter))
                        m_arguments.Add(parameter, null);
                }

                parameter = null;

                if (!string.IsNullOrEmpty(arg))
                {
                    // If this argument begins with a quote, we treat it as a stand-alone argument
                    if (arg[0] == '\"' || arg[0] == '\'')
                    {
                        // Handle stand alone ordered arguments
                        m_orderedArgCount++;
                        parameter = orderedArgID + m_orderedArgCount;

                        // Remove possible enclosing characters (",')
                        if (!m_arguments.ContainsKey(parameter))
                        {
                            argument = remover.Replace(arg, "$1");
                            m_arguments.Add(parameter, argument);
                        }

                        parameter = null;
                    }
                    else
                    {
                        // Look for new parameters (-,/ or --) and a
                        // possible enclosed value (=,:)
                        parts = spliter.Split(arg, 3);

                        switch (parts.Length)
                        {
                            case 1:
                                // Handle stand alone ordered arguments
                                m_orderedArgCount++;
                                parameter = orderedArgID + m_orderedArgCount;

                                // Remove possible enclosing characters (",')
                                if (!m_arguments.ContainsKey(parameter))
                                {
                                    argument = remover.Replace(arg, "$1");
                                    m_arguments.Add(parameter, argument);
                                }

                                parameter = null;
                                break;
                            case 2:
                                // Found just a parameter
                                parameter = parts[1];
                                break;
                            case 3:
                                // Parameter with enclosed value
                                parameter = parts[1];

                                // Remove possible enclosing characters (",')
                                if (!m_arguments.ContainsKey(parameter))
                                {
                                    parts[2] = remover.Replace(parts[2], "$1");
                                    m_arguments.Add(parameter, parts[2]);
                                }

                                parameter = null;
                                break;
                        }
                    }
                }
            }

            // In case a parameter is still waiting
            if (parameter != null && !m_arguments.ContainsKey(parameter))
                m_arguments.Add(parameter, null);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the value for the specified argument from the command-line command.
        /// </summary>
        /// <param name="argument">The argument whose value is to retrieved.</param>
        /// <returns>Value for the specified argument if found; otherwise null.</returns>
        public virtual string this[string argument]
        {
            get
            {
                string value;
                m_arguments.TryGetValue(argument, out value);
                return value;
            }
        }

        /// <summary>
        /// Gets the total number of arguments (ordered and optional) present in the command-line command.
        /// </summary>
        public virtual int Count
        {
            get
            {
                return m_arguments.Count;
            }
        }

        /// <summary>
        /// Gets the prefix text in the identifier of ordered arguments present in the command-line command.
        /// </summary>
        public virtual string OrderedArgID
        {
            get
            {
                return m_orderedArgID;
            }
        }

        /// <summary>
        /// Gets the total number of ordered arguments in the command-line command.
        /// </summary>
        public virtual int OrderedArgCount
        {
            get
            {
                return m_orderedArgCount;
            }
        }

        /// <summary>
        /// Gets a boolean value that indicates whether the command-line command contains request for displaying help.
        /// </summary>
        public virtual bool ContainsHelpRequest
        {
            get
            {
                return (m_arguments.ContainsKey("?") || m_arguments.ContainsKey("Help"));
            }
        }

        /// <summary>
        /// Gets the dictionary containing all of the arguments present in the command-line command.
        /// </summary>
        protected Dictionary<string, string> InternalDictionary
        {
            get
            {
                return m_arguments;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets a boolean value that indicates whether the specified argument is present in the command-line command.
        /// </summary>
        /// <param name="argument">The argument to be checked.</param>
        /// <returns>true if the argument exists in the command-line command; otherwise false.</returns>
        public virtual bool Exists(string argument)
        {
            return m_arguments.ContainsKey(argument);
        }

        /// <summary>
        /// Gets a string representation of the <see cref="Arguments"/> object.
        /// </summary>
        public override string ToString()
        {
            return m_commandLine;
        }

        /// <summary>
        /// Returns an <see cref="IEnumerator"/> for iterating through all the comman-line command arguments.
        /// </summary>
        /// <returns>An <see cref="IEnumerator"/> for the command-line command arguments.</returns>
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return m_arguments.GetEnumerator();
        }

        /// <summary>
        /// Returns an <see cref="IEnumerator"/> for iterating through all the comman-line command arguments.
        /// </summary>
        /// <returns>An <see cref="IEnumerator"/> for the command-line command arguments.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)m_arguments).GetEnumerator();
        }

        #endregion

        #region [ Static ]

        /// <summary>
        /// This function can be used to parse a single parameterized string and turn it into an array of parameters.
        /// </summary>
        /// <param name="command">String of parameters.</param>
        /// <returns>Array of parameters.</returns>
        public static string[] ParseCommand(string command)
        {
            List<string> parsedCommand = new List<string>();

            if (!string.IsNullOrEmpty(command))
            {
                string encodedQuote = Guid.NewGuid().ToString();
                string encodedSpace = Guid.NewGuid().ToString();
                StringBuilder encodedCommand = new StringBuilder();
                bool argumentInQuotes = false;
                char currentCharacter;
                string argument;

                // Encodes embedded quotes. It allows embedded/nested quotes encoded as \".
                command = command.Replace("\\\"", encodedQuote);

                // Combines any quoted strings into a single arg by encoding embedded spaces.
                for (int x = 0; x < command.Length; x++)
                {
                    currentCharacter = command[x];

                    if (currentCharacter == '\"')
                    {
                        if (argumentInQuotes)
                            argumentInQuotes = false;
                        else
                            argumentInQuotes = true;
                    }

                    if (argumentInQuotes)
                    {
                        if (currentCharacter == ' ')
                            encodedCommand.Append(encodedSpace);
                        else
                            encodedCommand.Append(currentCharacter);
                    }
                    else
                    {
                        encodedCommand.Append(currentCharacter);
                    }
                }

                command = encodedCommand.ToString();

                // Parses every argument out by space and combine any quoted strings into a single arg.
                foreach (string arg in command.Split(' '))
                {
                    // Adds tokenized argument, making sure to unencode any embedded quotes or spaces.
                    argument = arg.Replace(encodedQuote, "\"").Replace(encodedSpace, " ").Trim();
                    if (argument.Length > 0)
                        parsedCommand.Add(argument);
                }
            }

            return parsedCommand.ToArray();
        }

        #endregion
    }
}