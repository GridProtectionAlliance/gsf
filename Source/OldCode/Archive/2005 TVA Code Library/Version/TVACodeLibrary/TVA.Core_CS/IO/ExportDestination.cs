using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.IO;

//*******************************************************************************************************
//  ExportDestination.vb - Defines an Export Destination - used by MultipleDestinationExporter
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2008
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  02/13/2008 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************


namespace TVA
{
	namespace IO
	{
		
		/// <summary>
		/// Export destination.
		/// </summary>
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
	
}
