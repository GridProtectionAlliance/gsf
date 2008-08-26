using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using TVA.Communication.Common;

//*******************************************************************************************************
//  TVA.Communication.ICommunicationClient.vb - Abstract communications client interface
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
//  07/26/2006 - Pinal C. Patel
//       Original version of source code generated
//  09/06/2006 - J. Ritchie Carroll
//       Added ReceiveRawDataFunction delegate to allow bypass optimizations for high-speed data access
//  05/01/2007 - Pinal C. Patel
//       Made WaitForConnection() functions that returns a boolean value indicating success or failure
//  09/27/2007 - J. Ritchie Carroll
//       Added disconnect timeout overload
//
//*******************************************************************************************************


namespace TVA.Communication
{
	public interface ICommunicationClient : TVA.Services.IServiceComponent
	{
		
		
		/// <summary>
		/// Occurs when the client is trying to connect to the server.
		/// </summary>
		// Event Connecting As EventHandler VBConversions Warning: events in interfaces not supported in C#.
		
		/// <summary>
		/// Occurs when connecting of the client to the server has been cancelled.
		/// </summary>
		// Event ConnectingCancelled As EventHandler VBConversions Warning: events in interfaces not supported in C#.
		
		/// <summary>
		/// Occurs when an exception is encountered while connecting to the server.
		/// </summary>
		// Event ConnectingException As EventHandler(Of GenericEventArgs(Of Exception)) VBConversions Warning: events in interfaces not supported in C#.
		
		/// <summary>
		/// Occurs when the client has successfully connected to the server.
		/// </summary>
		// Event Connected As EventHandler VBConversions Warning: events in interfaces not supported in C#.
		
		/// <summary>
		/// Occurs when the client has disconnected from the server.
		/// </summary>
		// Event Disconnected As EventHandler VBConversions Warning: events in interfaces not supported in C#.
		
		/// <summary>
		/// Occurs when the client begins sending data to the server.
		/// </summary>
		// Event SendDataBegin As EventHandler(Of GenericEventArgs(Of IdentifiableItem(Of Guid, Byte()))) VBConversions Warning: events in interfaces not supported in C#.
		
		/// <summary>
		/// Occurs when the client has successfully send data to the server.
		/// </summary>
		// Event SendDataComplete As EventHandler(Of GenericEventArgs(Of IdentifiableItem(Of Guid, Byte()))) VBConversions Warning: events in interfaces not supported in C#.
		
		/// <summary>
		/// Occurs when the client receives data from the server.
		/// </summary>
		// Event ReceivedData As EventHandler(Of GenericEventArgs(Of IdentifiableItem(Of Guid, Byte()))) VBConversions Warning: events in interfaces not supported in C#.
		
		/// <summary>
		/// Occurs when no data is received from the server after waiting for the specified time.
		/// </summary>
		// Event ReceiveTimedOut As EventHandler VBConversions Warning: events in interfaces not supported in C#.
		
		/// <summary>
		/// Gets or sets the data required by the client to connect to the server.
		/// </summary>
		/// <value></value>
		/// <returns>The data required by the client to connect to the server.</returns>
		string ConnectionString{
			get;
			set;
		}
		
		/// <summary>
		/// Gets or sets the maximum number of times the client will attempt to connect to the server.
		/// </summary>
		/// <value></value>
		/// <returns>The maximum number of times the client will attempt to connect to the server.</returns>
		int MaximumConnectionAttempts{
			get;
			set;
		}
		
		/// <summary>
		/// Gets or sets a boolean value indicating whether the data exchanged between the server and clients
		/// will be encrypted using a private session passphrase.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// True if the data exchanged between the server and clients will be encrypted using a private session
		/// passphrase; otherwise False.
		/// </returns>
		bool SecureSession{
			get;
			set;
		}
		
		/// <summary>
		/// Gets or sets a boolean value indicating whether the server will do a handshake with the client after
		/// accepting its connection.
		/// </summary>
		/// <value></value>
		/// <returns>True if the server will do a handshake with the client; otherwise False.</returns>
		bool Handshake{
			get;
			set;
		}
		
		/// <summary>
		/// Gets or sets the passpharse that will be provided to the server for authentication during the handshake
		/// process.
		/// </summary>
		/// <value></value>
		/// <returns>The passpharse that will provided to the server for authentication during the handshake process.</returns>
		string HandshakePassphrase{
			get;
			set;
		}
		
		/// <summary>
		/// Gets or sets the maximum number of bytes that can be received at a time by the client from the server.
		/// </summary>
		/// <value></value>
		/// <returns>The maximum number of bytes that can be received at a time by the client from the server.</returns>
		int ReceiveBufferSize{
			get;
			set;
		}
		
		/// <summary>
		/// Gets or sets the time to wait in seconds for data to be received from the server before timing out.
		/// </summary>
		/// <value></value>
		/// <returns>The time to wait in seconds for data to be received from the server before timing out.</returns>
		int ReceiveTimeout{
			get;
			set;
		}
		
		/// <summary>
		/// Gets or sets the encryption level to be used for encrypting the data exchanged between the client and
		/// server.
		/// </summary>
		/// <value></value>
		/// <returns>The encryption level to be used for encrypting the data exchanged between the client and server.</returns>
		TVA.Security.Cryptography.EncryptLevel Encryption{
			get;
			set;
		}
		
		/// <summary>
		/// Gets or sets the compression level to be used for compressing the data exchanged between the client and
		/// server.
		/// </summary>
		/// <value></value>
		/// <returns>The compression level to be used for compressing the data exchanged between the client and server.</returns>
		TVA.IO.Compression.CompressLevel Compression{
			get;
			set;
		}
		
		/// <summary>
		/// Gets or sets a boolean value indicating whether the client is enabled.
		/// </summary>
		/// <value></value>
		/// <returns>True if the client is enabled; otherwise False.</returns>
		bool Enabled{
			get;
			set;
		}
		
		/// <summary>
		/// Gets or sets the encoding to be used for the text sent to the server.
		/// </summary>
		/// <value></value>
		/// <returns>The encoding to be used for the text sent to the server.</returns>
		System.Text.Encoding TextEncoding{
			get;
			set;
		}
		
		/// <summary>
		/// Gets the protocol used by the client for transferring data to and from the server.
		/// </summary>
		/// <value></value>
		/// <returns>The protocol used by the client for transferring data to and from the server.</returns>
		TransportProtocol Protocol{
			get;
			set;
		}
		
		/// <summary>
		/// Gets the ID of the server to which the client is connected.
		/// </summary>
		/// <value></value>
		/// <returns>ID of the server to which the client is connected.</returns>
		Guid ServerID{
			get;
			set;
		}
		
		/// <summary>
		/// Gets the ID of the client.
		/// </summary>
		/// <value></value>
		/// <returns>ID of the client.</returns>
		Guid ClientID{
			get;
			set;
		}
		
		/// <summary>
		/// Setting this property allows consumer to "intercept" data before it goes through normal processing
		/// </summary>
		ReceiveRawDataFunctionSignature ReceiveRawDataFunction{
			get;
			set;
		}
		
		/// <summary>
		/// Gets the current instance of communication client.
		/// </summary>
		/// <value></value>
		/// <returns>The current instance communication client.</returns>
		ICommunicationClient This{
			get;
		}
		
		/// <summary>
		/// Gets a boolean value indicating whether the client is currently connected to the server.
		/// </summary>
		/// <value></value>
		/// <returns>True if the client is connected; otherwise False.</returns>
		bool IsConnected{
			get;
		}
		
		/// <summary>
		/// Gets the time in seconds for which the client has been connected to the server.
		/// </summary>
		/// <value></value>
		/// <returns>The time in seconds for which the client has been connected to the server.</returns>
		double ConnectionTime{
			get;
		}
		
		/// <summary>
		/// Gets the total number of bytes sent by the client to the server since the connection is established.
		/// </summary>
		/// <value></value>
		/// <returns>The total number of bytes sent by the client to the server since the connection is established.</returns>
		long TotalBytesSent{
			get;
		}
		
		/// <summary>
		/// Gets the total number of bytes received by the client from the server since the connection is established.
		/// </summary>
		/// <value></value>
		/// <returns>The total number of bytes received by the client from the server since the connection is established.</returns>
		long TotalBytesReceived{
			get;
		}
		
		/// <summary>
		/// Connects the client to the server.
		/// </summary>
		void Connect();
		
		/// <summary>
		/// Cancels connecting to the server.
		/// </summary>
		void CancelConnect();
		
		/// <summary>
		/// Disconnects the client from the connected server.
		/// </summary>
		void Disconnect();
		
		/// <summary>
		/// Disconnects the client from the connected server, timing out within specified milliseconds if needed.
		/// </summary>
		void Disconnect(int timeout);
		
		/// <summary>
		/// Sends data to the server.
		/// </summary>
		/// <param name="data">The plain-text data that is to be sent to the server.</param>
		void Send(string data);
		
		/// <summary>
		/// Sends data to the server.
		/// </summary>
		/// <param name="serializableObject">The serializable object that is to be sent to the server.</param>
		void Send(object serializableObject);
		
		/// <summary>
		/// Sends data to the server.
		/// </summary>
		/// <param name="data">The binary data that is to be sent to the server.</param>
		void Send(byte[] data);
		
		/// <summary>
		/// Sends the specified subset of data from the data buffer to the server.
		/// </summary>
		/// <param name="data">The buffer that contains the binary data to be sent.</param>
		/// <param name="offset">The zero-based position in the buffer parameter at which to begin sending data.</param>
		/// <param name="size">The number of bytes to be sent.</param>
		void Send(byte[] data, int offset, int size);
		
		/// <summary>
		/// Waits for the client to connect to the server for the specified time and optionally stop the client from
		/// retrying connection attempts if the client is unable to connect to the server within the specified time.
		/// </summary>
		/// <param name="waitTime">
		/// The time in milliseconds to wait for the client to connect to the server. Specifying a value of -1 or 0
		/// will cause this method to wait indefinately until the client establishes connection with the server.
		/// </param>
		bool WaitForConnection(int waitTime);
		
		/// <summary>
		/// Waits for the client to connect to the server for the specified time and optionally stop the client from
		/// retrying connection attempts if the client is unable to connect to the server within the specified time.
		/// </summary>
		/// <param name="waitTime">
		/// The time in milliseconds to wait for the client to connect to the server. Specifying a value of -1 or 0
		/// will cause this method to wait indefinately until the client establishes connection with the server.
		/// </param>
		/// <param name="stopRetrying">
		/// Boolean value indicating whether the client should stop trying to connect to the server if it is unable to
		/// connect to the server after waiting for the specified duration.
		/// </param>
		bool WaitForConnection(int waitTime, bool stopRetrying);
		
	}
	
}
