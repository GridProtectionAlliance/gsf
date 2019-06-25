//******************************************************************************************************
//  Arguments.cs - Gbtc
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
//  09/12/2004 - J. Ritchie Carroll
//       Generated original version of source code.
//  03/28/2006 - Pinal C. Patel
//       Migrated 2.0 version of source code from 1.1 source (GSF.Console).
//  10/09/2007 - J. Ritchie Carroll / Pinal C. Patel
//       Fixed stand-alone argument issue at end of line and changed to use generic Dictionary class.
//  09/15/2008 - J. Ritchie Carroll
//      Converted to C#.
//  09/26/2008 - Pinal C. Patel
//      Entered code comments.
//  08/05/2009 - Josh L. Patterson
//      Edited Comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  04/2/2012 - J. Ritchie Carroll
//      Added new TryGetValue() method.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using GSF.Collections;

namespace GSF.Console
{
    /// <summary>
    /// Represents an ordered set of arguments along with optional arguments parsed from a command-line.
    /// </summary>
    /// <example>
    /// This example shows how to parse a command-line command that does not contain an executable name:
    /// <code>
    /// using System;
    /// using GSF;
    /// using GSF.Console;
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
    /// using GSF.Console;
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
    [Serializable]
    public class Arguments : IEnumerable<KeyValuePair<string, string>>, ISerializable
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Regular expression pattern for tokenizing a list of arguments.
        /// </summary>
        public const string TokenRegex = @"(?:(?<Open>"")(?:[^\\""]*(?:\\.)*)*(?<Close-Open>"")(?(Open)(?!))|\S)+";

        /// <summary>
        /// Default value for <see cref="OrderedArgID"/>.
        /// </summary>
        public const string DefaultOrderedArgID = "OrderedArg";

        // Fields
        private readonly string m_commandLine;
        private readonly string m_orderedArgID;
        private readonly int m_orderedArgCount;
        private readonly Dictionary<string, string> m_arguments;

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

            if (skipFirstArgument)
            {
                if (args.Length > 1)
                    args = args.Copy(1, args.Length - 1);
                else
                    args = new string[0];
            }

            foreach (string arg in args)
            {
                // Found just a parameter in last pass...
                // The last parameter is still waiting, with no value, set it to nothing.
                if ((object)parameter != null)
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
            if ((object)parameter != null && !m_arguments.ContainsKey(parameter))
                m_arguments.Add(parameter, null);
        }

        /// <summary>
        /// Creates a new <see cref="Arguments"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected Arguments(SerializationInfo info, StreamingContext context)
        {
            // Deserialize argument fields
            m_commandLine = info.GetOrDefault("commandLine", "");
            m_orderedArgID = info.GetOrDefault("orderedArgID", DefaultOrderedArgID);
            m_orderedArgCount = info.GetOrDefault("orderedArgCount", 0);
            m_arguments = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);

            int argumentCount = info.GetOrDefault("argumentCount", 0);

            for (int i = 0; i < argumentCount; i++)
            {
                string key = info.GetOrDefault("argumentKey" + i, null as string);

                if (!string.IsNullOrEmpty(key))
                    m_arguments.Add(key, info.GetOrDefault("argumentValue" + i, null as string));
            }
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
        /// Gets the ordered arguments as an array of strings.
        /// </summary>
        public virtual string[] OrderedArgs
        {
            get
            {
                return Enumerable.Range(1, OrderedArgCount)
                    .Select(arg => this[OrderedArgID + arg])
                    .ToArray();
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
        /// Gets the value associated with the specified argument in the command-line command.
        /// </summary>
        /// <param name="argument">The argument whose value is to be retrieved.</param>
        /// <param name="value">Value associated with the specified argument if it exists in the command-line command, otherwise null.</param>
        /// <returns><c>true</c> if argument existed and was returned; otherwise <c>false</c>.</returns>
        public virtual bool TryGetValue(string argument, out string value)
        {
            return m_arguments.TryGetValue(argument, out value);
        }

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
        /// <returns>A string representation of the <see cref="Arguments"/> object.</returns>
        public override string ToString()
        {
            return m_commandLine;
        }

        /// <summary>
        /// Returns an <see cref="IEnumerator"/> for iterating through all the command-line command arguments.
        /// </summary>
        /// <returns>An <see cref="IEnumerator"/> for the command-line command arguments.</returns>
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return m_arguments.GetEnumerator();
        }

        /// <summary>
        /// Returns an <see cref="IEnumerator"/> for iterating through all the command-line command arguments.
        /// </summary>
        /// <returns>An <see cref="IEnumerator"/> for the command-line command arguments.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)m_arguments).GetEnumerator();
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination (see <see cref="StreamingContext"/>) for this serialization.</param>
        /// <exception cref="SecurityException">The caller does not have the required permission.</exception>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Serialize argument fields
            info.AddValue("commandLine", m_commandLine);
            info.AddValue("orderedArgID", m_orderedArgID);
            info.AddValue("orderedArgCount", m_orderedArgCount);
            info.AddValue("argumentCount", m_arguments.Count);

            int i = 0;

            foreach (KeyValuePair<string, string> argument in m_arguments)
            {
                info.AddValue("argumentKey" + i, argument.Key);
                info.AddValue("argumentValue" + i, argument.Value);
                i++;
            }
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly Regex CompiledTokenRegex = new Regex(TokenRegex, RegexOptions.Compiled);

        // Static Methods

        /// <summary>
        /// This function can be used to parse a single parameterized string and turn it into an array of parameters.
        /// </summary>
        /// <param name="command">String of parameters.</param>
        /// <returns>Array of parameters.</returns>
        public static string[] ParseCommand(string command)
        {
            lock (CompiledTokenRegex)
                return CompiledTokenRegex.Matches(command)
                    .Cast<Match>()
                    .Select(match => match.Value)
                    .ToArray();
        }

        /// <summary>
        /// Provides an array of arguments parsed from the given command
        /// using parsing rules similar to those used in POSIX environments.
        /// </summary>
        /// <param name="command">The command to be parsed.</param>
        /// <returns>An array of arguments.</returns>
        /// <remarks>
        /// <para>
        /// This function processes a set of command line arguments using a simple set of rules based on
        /// those used in popular POSIX command shells. It is important to note that these rules do not apply
        /// to instances of the <see cref="Arguments"/> class or the <see cref="ParseCommand"/> function.
        /// </para>
        /// 
        /// <para>
        /// The motivation for this function was to provide a more user-friendly set of rules by which to
        /// specify command-line arguments. This is of course helpful for the user who has to type the
        /// command-line arguments by hand, but it also ends up being useful for systems that have to
        /// programmatically escape arguments to be consumed by other command-line applications (see
        /// the <see cref="Escape"/> function).
        /// </para>
        /// 
        /// <para>
        /// Also note that Windows has historically not provided a standard method for handling
        /// command-line arguments and that some of the command-line tools in Windows have rolled their
        /// own (potentially faulty) methods for doing so. More recently, Microsoft has provided the
        /// <see cref="Environment.GetCommandLineArgs"/> function for .NET applications to retrieve their
        /// own command line arguments using a standard format, but this function lacks the appropriate
        /// supporting functions for programmatically handling arbitrary command strings within an
        /// application. Furthermore, the rules for escaping spaces, quotes, and backslashes are very
        /// inconsistent, making it difficult to write a robust method for escaping individual arguments,
        /// let alone fully reproducing the same command parsing rules. For this reason, some programmers
        /// may be interested in using this method in conjunction with <see cref="Environment.CommandLine"/>
        /// for parsing their own command line arguments.
        /// </para>
        /// </remarks>
        public static string[] ToArgs(string command)
        {
            string[] tokenCharacterPatterns =
            {
                // Backslash followed by another character
                @"\\.",

                // Substring wrapped in double quotes
                @"""(?:\\.|[^\\""])*""",

                // Substring wrapped in single quotes
                @"'[^']*'",

                // Mismatched double quote
                @"""(?:\\.|[^\\""])*\\?$",

                // Mismatched single quote
                @"'[^']*$",

                // A single character that is not a
                // single quote, double quote, or whitespace
                @"[^'""\s]"
            };

            // Define the regular expression pattern to match tokens
            string pattern = $"(?<=^|\\s)({string.Join("|", tokenCharacterPatterns)})+(?=\\s|$)";

            // This function converts an escape
            // sequence into the corresponding character
            Func<string, string> unescape = escapeSequence =>
            {
                if (escapeSequence == @"\n")
                    return "\n";

                if (escapeSequence == @"\r")
                    return "\r";

                if (escapeSequence == @"\t")
                    return "\t";

                return escapeSequence.Substring(1);
            };

            // This function unquotes the given string by
            // simply removing the first and last characters
            Func<string, string> unquote = str =>
                str.Substring(1, str.Length - 2);

            // This function converts a token character into
            // its corresponding output in the args array
            Func<Capture, string> interpretCharacter = tokenChar =>
            {
                string value = tokenChar.Value;

                switch (value[0])
                {
                    // Backslash indicates an escape sequence
                    case '\\':
                        if (value.Length == 1)
                            throw new FormatException("Malformed expression - dangling escape sequence.");

                        return unescape(value);

                    // Expressions wrapped in double quotes must be stripped of the surrounding
                    // double quotes, and processed internally for escape sequences
                    case '"':
                        if (!value.EndsWith("\"", StringComparison.Ordinal))
                            throw new FormatException($"Malformed expression - mismatched quote. arg: {value}");

                        // A simple regex can be used to find the escape sequences since
                        // it's already known that this is a cohesive double-quoted token
                        List<string> innerTokens = Regex.Matches(unquote(value), @"\\?.")
                            .Cast<Match>()
                            .Select(match => match.Value)
                            .ToList();

                        // Mismatched quotes could also end in an escaped quote,
                        // indicated here by an inner dangling escape sequence
                        if (innerTokens.Last() == @"\")
                            throw new FormatException($"Malformed expression - mismatched quote. arg: {value}");

                        return string.Concat(innerTokens.Select(token => token.Length > 1 ? unescape(token) : token));

                    // Expressions wrapped in single quotes must
                    // be stripped of the surrounding single quotes
                    case '\'':
                        if (!value.EndsWith("'", StringComparison.Ordinal))
                            throw new FormatException($"Malformed expression - mismatched quote. arg: {value}");

                        return unquote(value);

                    // Any other character produces itself
                    default:
                        return value;
                }
            };

            // This function converts a token to its corresponding output in the args array
            Func<Match, string> interpretToken = token => string.Concat(token.Groups[1].Captures.Cast<Capture>().Select(interpretCharacter));

            // Apply the regular expression pattern and
            // convert the result into the array of arguments
            return Regex.Matches(command, pattern)
                .Cast<Match>()
                .Select(interpretToken)
                .ToArray();
        }

        /// <summary>
        /// Escapes a string so that it will be parsed as a
        /// single argument by the <see cref="ToArgs"/> method.
        /// </summary>
        /// <param name="arg">The argument to be escaped.</param>
        /// <returns>The escaped argument.</returns>
        public static string Escape(string arg)
        {
            StringBuilder escapedArgBuilder = new StringBuilder();

            // Empty string is encoded as
            // two empty double quotes
            if (string.IsNullOrEmpty(arg))
                return new string('"', 2);

            foreach (char c in arg)
            {
                switch (c)
                {
                    case '\\':
                    case '"':
                    case '\'':
                        // Quotes and backslashes need
                        // to be escaped by a backslash
                        escapedArgBuilder.Append('\\').Append(c);
                        break;

                    case '\n':
                        // Newline (\n)
                        escapedArgBuilder.Append(@"\n");
                        break;

                    case '\r':
                        // Carriage return (\r)
                        escapedArgBuilder.Append(@"\r");
                        break;

                    case '\t':
                        // Tab (\t)
                        escapedArgBuilder.Append(@"\t");
                        break;
                        
                    default:
                        // White space needs to be
                        // escaped by a backslash
                        if (char.IsWhiteSpace(c))
                            escapedArgBuilder.Append('\\');

                        // All other characters do
                        // not need to be escaped
                        escapedArgBuilder.Append(c);
                        break;
                }
            }

            return escapedArgBuilder.ToString();
        }

        #endregion
    }
}