//******************************************************************************************************
//  Command.cs - Gbtc
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
//  08/27/2014 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace GSF.Console
{
    /// <summary>
    /// Defines methods related to command line operations
    /// </summary>
    public static class Command
    {
        // Defines a command process.
        private sealed class CommandProcess : IDisposable
        {
            #region [ Members ]

            // Fields
            private readonly Process m_process;
            private readonly StringBuilder m_standardOutput;
            private readonly StringBuilder m_standardError;
            private int m_exitCode;
            private bool m_disposed;

            #endregion

            #region [ Constructors ]

            /// <summary>
            /// Creates a new <see cref="CommandProcess"/> instance.
            /// </summary>
            /// <param name="fileName">File name to execute.</param>
            /// <param name="arguments">Command line parameters, if any. Set to empty string for none.</param>
            public CommandProcess(string fileName, string arguments)
            {
                m_standardOutput = new StringBuilder();
                m_standardError = new StringBuilder();
                m_exitCode = -1;

                m_process = new Process
                {
                    StartInfo =
                    {
                        FileName = fileName,
                        Arguments = arguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };

                m_process.OutputDataReceived += m_process_OutputDataReceived;
                m_process.ErrorDataReceived += m_process_ErrorDataReceived;
            }

            /// <summary>
            /// Releases the unmanaged resources before the <see cref="CommandProcess"/> object is reclaimed by <see cref="GC"/>.
            /// </summary>
            ~CommandProcess()
            {
                Dispose(false);
            }

            #endregion

            #region [ Properties ]

            /// <summary>
            /// Gets any standard output from process after execution.
            /// </summary>
            public string StandardOutput
            {
                get
                {
                    return m_standardOutput.ToString();
                }
            }

            /// <summary>
            /// Gets any standard error from process after execution.
            /// </summary>
            public string StandardError
            {
                get
                {
                    return m_standardError.ToString();
                }
            }

            /// <summary>
            /// Gets exit code assuming process
            /// </summary>
            public int ExitCode
            {
                get
                {
                    return m_exitCode;
                }
            }

            #endregion

            #region [ Methods ]

            /// <summary>
            /// Releases all the resources used by the <see cref="CommandProcess"/> object.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            // Releases the unmanaged resources and optionally releases the managed resources.
            private void Dispose(bool disposing)
            {
                if (!m_disposed)
                {
                    try
                    {
                        if (disposing)
                        {
                            if ((object)m_process != null)
                            {
                                m_process.OutputDataReceived -= m_process_OutputDataReceived;
                                m_process.ErrorDataReceived -= m_process_ErrorDataReceived;
                                m_process.Dispose();
                            }
                        }
                    }
                    finally
                    {
                        m_disposed = true;  // Prevent duplicate dispose.
                    }
                }
            }

            /// <summary>
            /// Attempts to execute process.
            /// </summary>
            /// <param name="timeout">Timeout, in milliseconds, to wait for command line operation to complete.</param>
            /// <returns><c>true</c> if process completed; otherwise, <c>false</c> if process timed-out.</returns>
            public bool Execute(int timeout)
            {
                m_process.Start();

                m_process.BeginOutputReadLine();
                m_process.BeginErrorReadLine();

                if (!m_process.WaitForExit(timeout))
                {
                    m_process.CancelOutputRead();
                    m_process.CancelErrorRead();
                    return false;
                }

                m_exitCode = m_process.ExitCode;
                return true;
            }

            private void m_process_OutputDataReceived(object sender, DataReceivedEventArgs e)
            {
                m_standardOutput.AppendLine(e.Data);
            }

            private void m_process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
            {
                m_standardError.AppendLine(e.Data);
            }

            #endregion
        }

        /// <summary>
        /// Executes a command line operation and returns its standard output and exit code or throws an exception with the standard error.
        /// </summary>
        /// <param name="fileName">Command line file name to execute.</param>
        /// <param name="arguments">Command line arguments to use, if any.</param>
        /// <param name="timeout">Timeout, in milliseconds, to wait for command line operation to complete.</param>
        /// <returns>A <see cref="CommandResponse"/> containing the standard output received from command and the exit code.</returns>
        /// <exception cref="CommandException">
        /// Exception occurs when executed command process reports standard error output or process times-out.
        /// </exception>
        public static CommandResponse Execute(string fileName, string arguments = null, int timeout = Timeout.Infinite)
        {
            string standardOutput, standardError;
            bool processCompleted;
            int exitCode;

            if (!Execute(fileName, arguments, out standardOutput, out standardError, out processCompleted, out exitCode, timeout))
                throw new CommandException(standardError, processCompleted, exitCode);

            if (!processCompleted)
                throw new CommandException("Process timed-out.", false, -1);

            return new CommandResponse(standardOutput, exitCode);
        }

        /// <summary>
        /// Executes a command line operation and returns <c>true</c> if there was no standard error reported.
        /// </summary>
        /// <param name="fileName">Command line file name to execute.</param>
        /// <param name="arguments">Command line arguments to use, if any.</param>
        /// <param name="standardOutput">Any standard output reported by the command line operation.</param>
        /// <param name="standardError">Any standard error reported by the command line operation.</param>
        /// <param name="exitCode">Exit code of the process, assuming process successfully completed.</param>
        /// <returns><c>true</c> if there was no standard error reported; otherwise, <c>false</c>.</returns>
        /// <remarks>This function waits indefinitely for the command line operation to complete.</remarks>
        public static bool Execute(string fileName, string arguments, out string standardOutput, out string standardError, out int exitCode)
        {
            bool processCompleted;
            return Execute(fileName, arguments, out standardOutput, out standardError, out processCompleted, out exitCode, Timeout.Infinite);
        }

        /// <summary>
        /// Executes a command line operation and returns <c>true</c> if there was no standard error reported.
        /// </summary>
        /// <param name="fileName">Command line file name to execute.</param>
        /// <param name="arguments">Command line arguments to use, if any.</param>
        /// <param name="standardOutput">Any standard output reported by the command line operation.</param>
        /// <param name="standardError">Any standard error reported by the command line operation.</param>
        /// <param name="processCompleted">Flag that determines if process completed or timed-out. This is only relevant if <paramref name="timeout"/> is not -1.</param>
        /// <param name="exitCode">Exit code of the process, assuming process successfully completed.</param>
        /// <param name="timeout">Timeout, in milliseconds, to wait for command line operation to complete. Set to <see cref="Timeout.Infinite"/>, i.e., -1, for infinite wait.</param>
        /// <returns><c>true</c> if there was no standard error reported; otherwise, <c>false</c>.</returns>
        public static bool Execute(string fileName, string arguments, out string standardOutput, out string standardError, out bool processCompleted, out int exitCode, int timeout)
        {
            using (CommandProcess process = new CommandProcess(fileName, arguments ?? ""))
            {
                processCompleted = process.Execute(timeout);
                standardOutput = process.StandardOutput;
                standardError = process.StandardError;
                exitCode = process.ExitCode;
            }

            return string.IsNullOrWhiteSpace(standardError);
        }

        /// <summary>
        /// Shell encodes a command line parameter by converting "\" to "\\".
        /// </summary>
        /// <param name="parameter">Parameter to shell encode.</param>
        /// <returns>Shell encoded <paramref name="parameter"/>.</returns>
        public static string ShellEncode(this string parameter)
        {
            if ((object)parameter == null)
                throw new ArgumentNullException(nameof(parameter));

            return parameter.Replace("\\", "\\\\");
        }

        /// <summary>
        /// Decodes a command line parameter previously encoded by <see cref="ShellEncode"/>.
        /// </summary>
        /// <param name="parameter">Parameter to decode.</param>
        /// <returns>Decoded <paramref name="parameter"/>.</returns>
        public static string ShellDecode(this string parameter)
        {
            if ((object)parameter == null)
                throw new ArgumentNullException(nameof(parameter));

            return parameter.Replace("\\\\", "\\");
        }
    }
}
