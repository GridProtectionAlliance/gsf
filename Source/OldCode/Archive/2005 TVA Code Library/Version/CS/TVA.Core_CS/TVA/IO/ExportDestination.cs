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

using System.IO;

namespace TVA.IO
{
    /// <summary>
    /// Represents a destination location when exporting data using <see cref="MultipleDestinationExporter"/>.
    /// </summary>
    /// <seealso cref="MultipleDestinationExporter"/>
    public struct ExportDestination
    {
        /// <summary>
        /// Path and file name of export destination.
        /// </summary>
        public string DestinationFile;

        /// <summary>
        /// Determines whether or not to attempt network connection to share specified in DestinationFile.
        /// </summary>
        public bool ConnectToShare;

        /// <summary>
        /// Domain used to authenticate network connection if ConnectToShare is True.
        /// </summary>
        public string Domain;

        /// <summary>
        /// User name used to authenticate network connection if ConnectToShare is True.
        /// </summary>
        public string UserName;

        /// <summary>
        /// Password used to authenticate network connection if ConnectToShare is True.
        /// </summary>
        public string Password;

        public ExportDestination(string destinationFile, bool connectToShare, string domain, string userName, string password)
        {
            this.DestinationFile = destinationFile;
            this.ConnectToShare = connectToShare;
            this.Domain = domain;
            this.UserName = userName;
            this.Password = password;
        }

        /// <summary>
        /// Path root of DestinationFile (e.g., E:\ or \\server\share)
        /// </summary>
        public string Share
        {
            get
            {
                return Path.GetPathRoot(DestinationFile);
            }
        }

        /// <summary>
        /// Path and filename of DestinationFile
        /// </summary>
        public string FileName
        {
            get
            {
                return DestinationFile.Substring(Share.Length);
            }
        }

        public override string ToString()
        {
            return DestinationFile;
        }
    }
}