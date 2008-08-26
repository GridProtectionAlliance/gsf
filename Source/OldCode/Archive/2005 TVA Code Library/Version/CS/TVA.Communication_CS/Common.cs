using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
//using TVA.Text.Common;

//*******************************************************************************************************
//  TVA.Communication.Common.vb - Common global communications functions
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  08/04/2006 - Pinal C. Patel
//       Original version of source code generated
//  01/05/2007 - J. Ritchie Carroll
//       Moved raw data function signature into common (more logical location)
//
//*******************************************************************************************************


namespace TVA.Communication
{
	public sealed class Common
	{
		
		
		private Common()
		{
			
			// This class contains only global functions and is not meant to be instantiated
			
		}
		
		/// <summary>
		/// This function signature gets implemented as needed to allow consumers to "intercept" data before it goes through normal processing
		/// </summary>
		public delegate void ReceiveRawDataFunctionSignature(byte[] data, int offset, int length);
		
		/// <summary>
		/// Create a communications server
		/// </summary>
		/// <remarks>
		/// Note that typical configuration string should be prefixed with a "protocol=tcp" or a "protocol=udp"
		/// </remarks>
		public static ICommunicationServer CreateCommunicationServer(string configurationString)
		{
			
			ICommunicationServer server = null;
			string protocol = "";
			Dictionary<string, string> configurationData = TVA.Text.Common.ParseKeyValuePairs(configurationString);
			
			if (configurationData.TryGetValue("protocol", protocol))
			{
				configurationData.Remove("protocol");
				System.Text.StringBuilder with_1 = new System.Text.StringBuilder();
				foreach (string key in configurationData.Keys)
				{
					with_1.Append(key);
					with_1.Append("=");
					with_1.Append(configurationData(key));
					with_1.Append(";");
				}
				switch (protocol.ToLower())
				{
					case "tcp":
						server = new TcpServer(with_1.ToString());
						break;
					case "udp":
						server = new UdpServer(with_1.ToString());
						break;
					default:
						throw (new ArgumentException("Transport protocol \'" + protocol + "\' is not valid."));
						break;
				}
			}
			else
			{
				throw (new ArgumentException("Transport protocol must be specified."));
			}
			
			return server;
			
		}
		
		/// <summary>
		/// Create a communications client
		/// </summary>
		/// <remarks>
		/// Note that typical connection string should be prefixed with a "protocol=tcp", "protocol=udp", "protocol=serial" or "protocol=file"
		/// </remarks>
		public static ICommunicationClient CreateCommunicationClient(string connectionString)
		{
			
			ICommunicationClient client = null;
			string protocol = "";
			Dictionary<string, string> connectionData = TVA.Text.Common.ParseKeyValuePairs(connectionString);
			
			if (connectionData.TryGetValue("protocol", protocol))
			{
				connectionData.Remove("protocol");
				System.Text.StringBuilder with_1 = new System.Text.StringBuilder();
				foreach (string key in connectionData.Keys)
				{
					with_1.Append(key);
					with_1.Append("=");
					with_1.Append(connectionData(key));
					with_1.Append(";");
				}
				switch (protocol.ToLower())
				{
					case "tcp":
						client = new TcpClient(with_1.ToString());
						break;
					case "udp":
						client = new UdpClient(with_1.ToString());
						break;
					case "serial":
						client = new SerialClient(with_1.ToString());
						break;
					case "file":
						client = new FileClient(with_1.ToString());
						break;
					default:
						throw (new ArgumentException("Transport protocol \'" + protocol + "\' is not valid."));
						break;
				}
			}
			else
			{
				throw (new ArgumentException("Transport protocol must be specified."));
			}
			
			return client;
			
		}
		
	}
	
}
