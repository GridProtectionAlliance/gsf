//*******************************************************************************************************
//  ExportDestination.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  02/13/2008 - J. Ritchie Carroll
//       Initial version of source generated
//  09/19/2008 - James R Carroll
//       Converted to C#.
//  10/23/2008 - Pinal C. Patel
//       Edited code comments.
//
//*******************************************************************************************************

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Windows.Forms.Design;

namespace TVA.IO
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
        public ExportDestination(string destinationFile, bool connectToShare, string domain, string userName, string password)
        {
            this.m_destinationFile = destinationFile;
            this.m_connectToShare = connectToShare;
            this.m_domain = domain;
            this.m_userName = userName;
            this.m_password = password;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Path and file name of export destination.
        /// </summary>
        [Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
        public string DestinationFile
        {
            get { return m_destinationFile; }
            set { m_destinationFile = value; }
        }

        /// <summary>
        /// Determines whether or not to attempt network connection to share specified in <see cref="ExportDestination.DestinationFile"/>.
        /// </summary>
        public bool ConnectToShare
        {
            get { return m_connectToShare; }
            set { m_connectToShare = value; }
        }

        /// <summary>
        /// Domain used to authenticate network connection if <see cref="ExportDestination.ConnectToShare"/> is true.
        /// </summary>
        public string Domain
        {
            get { return m_domain; }
            set { m_domain = value; }
        }

        /// <summary>
        /// User name used to authenticate network connection if <see cref="ExportDestination.ConnectToShare"/> is true.
        /// </summary>
        public string UserName
        {
            get { return m_userName; }
            set { m_userName = value; }
        }

        /// <summary>
        /// Password used to authenticate network connection if <see cref="ExportDestination.ConnectToShare"/> is true.
        /// </summary>
        public string Password
        {
            get { return m_password; }
            set { m_password = value; }
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
        /// Path and filename of <see cref="ExportDestination.DestinationFile"/>.
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
        /// Returns a <see cref="String"/> that represents the current <see cref="ExportDestination"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="ExportDestination"/>.</returns>
        public override string ToString()
        {
            return FilePath.GetFileName(m_destinationFile);
        }

        #endregion
    }
}