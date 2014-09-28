//******************************************************************************************************
//  CommandException.cs - Gbtc
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
//  08/27/2014 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace GSF.Console
{
    /// <summary>
    /// Represents an exception that is thrown when <see cref="Command.Execute(string, string, int)"/> reports standard error output.
    /// </summary>
    [Serializable]
    public class CommandException : Exception
    {
        #region [ Members ]

        // Fields
        private readonly bool m_processCompleted;
        private readonly int m_exitCode;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="CommandException"/>.
        /// </summary>
        public CommandException()
        {
        }

        /// <summary>
        /// Creates a new <see cref="CommandException"/> with the specified parameters.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="processCompleted">Flag that determines if the source of command exception completed processing.</param>
        /// <param name="exitCode">Exit code of command process, assuming process successfully completed.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public CommandException(string message, bool processCompleted, int exitCode, Exception innerException = null)
            : base(message, innerException)
        {
            m_processCompleted = processCompleted;
            m_exitCode = exitCode;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets flag that determines if the source of this command exception completed processing.
        /// </summary>
        public bool ProcessCompleted
        {
            get
            {
                return m_processCompleted;
            }
        }

        /// <summary>
        /// Gets exit code from command process, assuming successful process completion.
        /// </summary>
        public int ExitCode
        {
            get
            {
                return m_exitCode;
            }
        }

        #endregion
    }
}
