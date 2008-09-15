//*******************************************************************************************************
//  TVA.Console.Arguments.vb - Command Line Parameter Parsing Class
//  Copyright © 2007 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  03/28/2006 - Pinal C. Patel
//       Migrated 2.0 version of source code from 1.1 source (TVA.Console)
//  10/09/2007 - J. Ritchie Carroll / Pinal C. Patel
//       Fixed stand-alone argument bug at end of line and changed class to use generic Dictionary class
//  09/15/2008 - J. Ritchie Carroll
//      Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TVA.Console
{
    [Serializable()]
    public class Arguments : IEnumerable, IEnumerable<KeyValuePair<string, string>>
    {
        private string m_commandLine;
        private string m_orderedArgID;
        private int m_orderedArgCount;
        private Dictionary<string, string> m_parameters;

        public Arguments(string commandLine)
            : this(commandLine, "OrderedArg")
        {
        }

        public Arguments(string commandLine, string orderedArgID)
        {
            Regex spliter = new Regex("^-{1,2}|^/|=|:", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            Regex remover = new Regex("^[\'\"]?(.*?)[\'\"]?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            string parameter = null;
            string argument;
            string[] parts;

            // We create a case-insensitive dictionary for parameters
            m_parameters = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            m_orderedArgCount = 0;
            m_commandLine = commandLine;
            m_orderedArgID = orderedArgID;

            // Valid parameters forms:
            //   {-,/,--}param{=,:}((",')value(",'))
            // Examples:
            //   -param1=value1 --param2 /param3:"Test-:-work"
            //   /param4=happy -param5 '--=nice=--'
            foreach (string arg in ParseCommand(m_commandLine))
            {
                // Found just a parameter in last pass...
                // The last parameter is still waiting, with no value, set it to nothing.
                if (parameter != null)
                {
                    if (!m_parameters.ContainsKey(parameter))
                        m_parameters.Add(parameter, null);
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
                        if (!m_parameters.ContainsKey(parameter))
                        {
                            argument = remover.Replace(arg, "$1");
                            m_parameters.Add(parameter, argument);
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
                                if (!m_parameters.ContainsKey(parameter))
                                {
                                    argument = remover.Replace(arg, "$1");
                                    m_parameters.Add(parameter, argument);
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
                                if (!m_parameters.ContainsKey(parameter))
                                {
                                    parts[2] = remover.Replace(parts[2], "$1");
                                    m_parameters.Add(parameter, parts[2]);
                                }

                                parameter = null;
                                break;
                        }
                    }
                }
            }

            // In case a parameter is still waiting
            if (parameter != null)
            {
                if (!m_parameters.ContainsKey(parameter))
                    m_parameters.Add(parameter, null);
            }

        }

        // Retrieve a parameter value if it exists
        public virtual string this[string param]
        {
            get
            {
                return m_parameters[param];
            }
        }

        public virtual bool Exists(string param)
        {
            return m_parameters.ContainsKey(param);
        }

        public virtual int Count
        {
            get
            {
                return m_parameters.Count;
            }
        }

        public virtual string OrderedArgID
        {
            get
            {
                return m_orderedArgID;
            }
        }

        public virtual int OrderedArgCount
        {
            get
            {
                return m_orderedArgCount;
            }
        }

        public virtual bool ContainsHelpRequest
        {
            get
            {
                return (m_parameters.ContainsKey("?") || m_parameters.ContainsKey("Help"));
            }
        }

        public override string ToString()
        {
            return m_commandLine;
        }

        protected Dictionary<string, string> InternalDictionary
        {
            get
            {
                return m_parameters;
            }
        }

        /// <summary>This function can be used to parse a single parameterized string and turn it into an array of parameters.</summary>
        /// <remarks>This function will always return at least one argument, even if it is an empty string.</remarks>
        /// <param name="command">String of parameters.</param>
        /// <returns>Array of parameters.</returns>
        public static string[] ParseCommand(string command)
        {
            List<string> parsedCommand = new List<string>();

            if (command.Length > 0)
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
                for (int x = 0; x <= command.Length - 1; x++)
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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)m_parameters).GetEnumerator();
        }

        IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator()
        {
            return m_parameters.GetEnumerator();
        }
    }
}