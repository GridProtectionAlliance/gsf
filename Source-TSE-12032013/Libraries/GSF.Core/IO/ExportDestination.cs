//******************************************************************************************************
//  ExportDestination.cs - Gbtc
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
//  02/13/2008 - J. Ritchie Carroll
//       Initial version of source generated.
//  09/19/2008 - J. Ritchie Carroll
//       Converted to C#.
//  10/23/2008 - Pinal C. Patel
//       Edited code comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  09/22/2011 - J. Ritchie Carroll
//       Added Mono implementation exception regions.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Windows.Forms.Design;

namespace GSF.IO
{
    /// <summary>
    /// Represents a destination location when exporting data using <see cref="MultipleDestinationExporter"/>.
    /// </summary>
    /// <seealso cref="MultipleDestinationExporter"/>
    public class ExportDestination
    {
        #region [ Members ]

        // Fields
        private string m_destinationFile;
        private bool m_connectToShare;
        private string m_domain;
        private string m_userName;
        private string m_password;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new <see cref="ExportDestination"/>.
        /// </summary>
        public ExportDestination()
        {
        }

        /// <summary>
        /// Constructs a new <see cref="ExportDestination"/> given the specified parameters.
        /// </summary>
        /// <param name="destinationFile">Path and file name of export destination.</param>
        /// <param name="connectToShare">Determines whether or not to attempt network connection to share specified in <paramref name="destinationFile"/>.</param>
        /// <param name="domain">Domain used to authenticate network connection if <paramref name="connectToShare"/> is true.</param>
        /// <param name="userName">User name used to authenticate network connection if <paramref name="connectToShare"/> is true.</param>
        /// <param name="password">Password used to authenticate network connection if <paramref name="connectToShare"/> is true.</param>
        public ExportDestination(string destinationFile, bool connectToShare, string domain = "", string userName = "", string password = "")
        {
            m_destinationFile = destinationFile;
            m_connectToShare = connectToShare;
            m_domain = domain;
            m_userName = userName;
            m_password = password;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Path and file name of export destination.
        /// </summary>
        [Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
        public string DestinationFile
        {
            get
            {
                return m_destinationFile;
            }
            set
            {
                m_destinationFile = value;
            }
        }

        /// <summary>
        /// Determines whether or not to attempt network connection to share specified in <see cref="ExportDestination.DestinationFile"/>.
        /// </summary>
        /// <remarks>
        /// This option is ignored under Mono deployments.
        /// </remarks>
        public bool ConnectToShare
        {
            get
            {
                return m_connectToShare;
            }
            set
            {
                m_connectToShare = value;
            }
        }

        /// <summary>
        /// Domain used to authenticate network connection if <see cref="ExportDestination.ConnectToShare"/> is true.
        /// </summary>
        /// <remarks>
        /// This option is ignored under Mono deployments.
        /// </remarks>
        public string Domain
        {
            get
            {
                return m_domain;
            }
            set
            {
                m_domain = value;
            }
        }

        /// <summary>
        /// User name used to authenticate network connection if <see cref="ExportDestination.ConnectToShare"/> is true.
        /// </summary>
        /// <remarks>
        /// This option is ignored under Mono deployments.
        /// </remarks>
        public string UserName
        {
            get
            {
                return m_userName;
            }
            set
            {
                m_userName = value;
            }
        }

        /// <summary>
        /// Password used to authenticate network connection if <see cref="ExportDestination.ConnectToShare"/> is true.
        /// </summary>
        /// <remarks>
        /// This option is ignored under Mono deployments.
        /// </remarks>
        public string Password
        {
            get
            {
                return m_password;
            }
            set
            {
                m_password = value;
            }
        }

        /// <summary>
        /// Path root of <see cref="ExportDestination.DestinationFile"/> (e.g., E:\ or \\server\share).
        /// </summary>
        [Browsable(false)]
        public string Share
        {
            get
            {
                return Path.GetPathRoot(m_destinationFile);
            }
        }

        /// <summary>
        /// Path and filename of <see cref="ExportDestination.DestinationFile"/> without drive or server share prefix.
        /// </summary>
        [Browsable(false)]
        public string FileName
        {
            get
            {
                return m_destinationFile.Substring(Share.Length);
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="ExportDestination"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="ExportDestination"/>.</returns>
        public override string ToString()
        {
            return m_destinationFile;
        }

        #endregion
    }
}