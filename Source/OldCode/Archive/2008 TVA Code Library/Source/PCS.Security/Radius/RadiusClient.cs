using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Threading;
//using PCS.IO.Common;
//using PCS.Text.Common;
using PCS.Communication;

//*******************************************************************************************************
//  PCS.Security.Radius.RadiusClient.vb - RADIUS authentication client
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [PCS]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  04/11/2008 - Pinal C. Patel
//       Original version of source code generated.
//
//*******************************************************************************************************


namespace PCS.Security
{
	namespace Radius
	{
		
		public class RadiusClient : IDisposable
		{
			
			
			#region " Member Declaration "
			
			private short m_requestAttempts;
			private int m_reponseTimeout;
			private string m_sharedSecret;
			private string m_newPinModeMessage1;
			private string m_newPinModeMessage2;
			private string m_newPinModeMessage3;
			private string m_nextTokenModeMessage;
			private bool m_disposed;
			private byte[] m_responseBytes;
			
			private UdpClient m_udpClient;
			
			#endregion
			
			#region " Code Scope: Public "
			
			/// <summary>
			/// Default port of the RADIUS server.
			/// </summary>
			public const int DefaultServerPort = 1812;
			
			/// <summary>
			/// Default text for comparing with the text of ReplyMessage attribute in an AccessChallenge
			/// server response to determine whether or not Step 1 (ensuring that the user account is in
			/// the "New Pin" mode) of creating a new pin was successful.
			/// </summary>
			public const string DefaultNewPinModeMessage1 = "Enter a new PIN";
			
			/// <summary>
			/// Default text for comparing with the text of ReplyMessage attribute in an AccessChallenge
			/// server response to determine whether or not Step 2 (new pin is accepted in attempt #1)
			///  of creating a new pin was successful.
			/// </summary>
			public const string DefaultNewPinModeMessage2 = "Please re-enter new PIN";
			
			/// <summary>
			/// Default text for comparing with the text of ReplyMessage attribute in an AccessChallenge
			/// server response to determine whether or not Step 3 (new pin is accepted in attempts #2)
			/// of creating a new pin was successful.
			/// </summary>
			public const string DefaultNewPinModeMessage3 = "PIN Accepted";
			
			/// <summary>
			/// Default text for comparing with the text of ReplyMessage attribute in an AccessChallenge
			/// server response to determine whether or not a user account is in the "Next Token" mode.
			/// </summary>
			public const string DefaultNextTokenModeMessage = "Wait for token to change";
			
			/// <summary>
			/// Creates an instance of RADIUS client for sending request to a RADIUS server.
			/// </summary>
			/// <param name="serverName">Name or address of the RADIUS server.</param>
			/// <param name="sharedSecret">Shared secret used for encryption and authentication.</param>
			public RadiusClient(string serverName, string sharedSecret) : this(serverName, DefaultServerPort, sharedSecret)
			{


			}
			
			/// <summary>
			/// Creates an instance of RADIUS client for sending request to a RADIUS server.
			/// </summary>
			/// <param name="serverName">Name or address of the RADIUS server.</param>
			/// <param name="serverPort">Port number of the RADIUS server.</param>
			/// <param name="sharedSecret">Shared secret used for encryption and authentication.</param>
			/// <remarks></remarks>
			public RadiusClient(string serverName, int serverPort, string sharedSecret)
			{
				
				this.SharedSecret = sharedSecret;
				this.RequestAttempts = 1;
				this.ReponseTimeout = 15000;
				this.NewPinModeMessage1 = DefaultNewPinModeMessage1;
				this.NewPinModeMessage2 = DefaultNewPinModeMessage2;
				this.NewPinModeMessage3 = DefaultNewPinModeMessage3;
				this.NextTokenModeMessage = DefaultNextTokenModeMessage;
				m_udpClient = new UdpClient(string.Format("Server={0}; RemotePort={1}; LocalPort=0", serverName, serverPort));
				m_udpClient.ReceivedData += new System.EventHandler`1[[PCS.GenericEventArgs`1[[PCS.IdentifiableItem`2[[Guid, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Byte[], mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], PCS.Core, Version=3.0.116.286, Culture=neutral, PublicKeyToken=null]], PCS.Core, Version=3.0.116.286, Culture=neutral, PublicKeyToken=null]](m_udpClient_ReceivedData);
				m_udpClient.Handshake = false;
				m_udpClient.PayloadAware = false;
				m_udpClient.Connect(); // Start the connection cycle.
				
			}
			
			/// <summary>
			/// Gets or sets the name or address of the RADIUS server.
			/// </summary>
			/// <value></value>
			/// <returns>Name or address of RADIUS server.</returns>
			public string ServerName
			{
				get
				{
					return PCS.Text.Common.ParseKeyValuePairs(m_udpClient.ConnectionString)["server"];
				}
				set
				{
					CheckDisposed();
					if (! string.IsNullOrEmpty(value))
					{
						Dictionary<string, string> parts = PCS.Text.Common.ParseKeyValuePairs(m_udpClient.ConnectionString);
						parts("server") = value;
						m_udpClient.ConnectionString = PCS.Text.Common.JoinKeyValuePairs(parts);
					}
					else
					{
						throw (new ArgumentNullException("ServerName"));
					}
				}
			}
			
			/// <summary>
			/// Gets or sets the port number of the RADIUS server.
			/// </summary>
			/// <value></value>
			/// <returns>Port number of the RADIUS server.</returns>
			public int ServerPort
			{
				get
				{
					return System.Convert.ToInt32(PCS.Text.Common.ParseKeyValuePairs(m_udpClient.ConnectionString)["remoteport"]);
				}
				set
				{
					CheckDisposed();
					if (value >= 0 && value <= 65535)
					{
						Dictionary<string, string> parts = PCS.Text.Common.ParseKeyValuePairs(m_udpClient.ConnectionString);
						parts("remoteport") = value.ToString();
						m_udpClient.ConnectionString = PCS.Text.Common.JoinKeyValuePairs(parts);
					}
					else
					{
						throw (new ArgumentOutOfRangeException("ServerPort", "Value must be between 0 and 65535."));
					}
				}
			}
			
			/// <summary>
			/// Gets or sets the number of time a request is to sent to the server until a valid response is received.
			/// </summary>
			/// <value></value>
			/// <returns>Number of time a request is to sent to the server until a valid response is received.</returns>
			public short RequestAttempts
			{
				get
				{
					return m_requestAttempts;
				}
				set
				{
					CheckDisposed();
					if (value >= 1 && value <= 10)
					{
						m_requestAttempts = value;
					}
					else
					{
						throw (new ArgumentOutOfRangeException("RequestAttempts", "Value must be between 1 and 10."));
					}
				}
			}
			
			/// <summary>
			/// Gets or sets the time (in milliseconds) to wait for a response from server after sending a request.
			/// </summary>
			/// <value></value>
			/// <returns>Time (in milliseconds) to wait for a response from server after sending a request.</returns>
			public int ReponseTimeout
			{
				get
				{
					return m_reponseTimeout;
				}
				set
				{
					CheckDisposed();
					if (value >= 1000 && value <= 60000)
					{
						m_reponseTimeout = value;
					}
					else
					{
						throw (new ArgumentOutOfRangeException("ResponseTimeout", "Value must be between 1000 and 60000."));
					}
				}
			}
			
			/// <summary>
			/// Gets or sets the shared secret used between the client and server for encryption and authentication.
			/// </summary>
			/// <value></value>
			/// <returns>Shared secret used between the client and server for encryption and authentication.</returns>
			public string SharedSecret
			{
				get
				{
					return m_sharedSecret;
				}
				set
				{
					CheckDisposed();
					if (! string.IsNullOrEmpty(value))
					{
						m_sharedSecret = value;
					}
					else
					{
						throw (new ArgumentNullException("SharedSecret"));
					}
				}
			}
			
			/// <summary>
			/// Gets or sets the text for comparing with the text of ReplyMessage attribute in an AccessChallenge
			/// server response to determine whether or not Step 1 (ensuring that the user account is in  the
			/// "New Pin" mode) of creating a new pin was successful.
			/// </summary>
			/// <value></value>
			/// <returns>Text for "New Pin" mode's first message.</returns>
			public string NewPinModeMessage1
			{
				get
				{
					return m_newPinModeMessage1;
				}
				set
				{
					CheckDisposed();
					if (! string.IsNullOrEmpty(value))
					{
						m_newPinModeMessage1 = value;
					}
					else
					{
						throw (new ArgumentNullException("NewPinModeMessage1"));
					}
				}
			}
			
			/// <summary>
			/// Gets or sets the text for comparing with the text of ReplyMessage attribute in an AccessChallenge
			/// server response to determine whether or not Step 2 (new pin is accepted in attempt #1) of creating
			/// a new pin was successful.
			/// </summary>
			/// <value></value>
			/// <returns>Text for "New Pin" mode's second message.</returns>
			public string NewPinModeMessage2
			{
				get
				{
					return m_newPinModeMessage2;
				}
				set
				{
					CheckDisposed();
					if (! string.IsNullOrEmpty(value))
					{
						m_newPinModeMessage2 = value;
					}
					else
					{
						throw (new ArgumentNullException("NewPinModeMessage2"));
					}
				}
			}
			
			/// <summary>
			/// Gets or sets the text for comparing with the text of ReplyMessage attribute in an AccessChallenge
			/// server response to determine whether or not Step 3 (new pin is accepted in attempts #2) of creating
			/// a new pin was successful.
			/// </summary>
			/// <value></value>
			/// <returns>Text for "New Pin" mode's third message.</returns>
			public string NewPinModeMessage3
			{
				get
				{
					return m_newPinModeMessage3;
				}
				set
				{
					CheckDisposed();
					if (! string.IsNullOrEmpty(value))
					{
						m_newPinModeMessage3 = value;
					}
					else
					{
						throw (new ArgumentNullException("NewPinModeMessage3"));
					}
				}
			}
			
			/// <summary>
			/// Gets or sets the text for comparing with the text of ReplyMessage attribute in an AccessChallenge
			/// server response to determine whether or not a user account is in the "Next Token" mode.
			/// </summary>
			/// <value></value>
			/// <returns>Text for "Next Token" mode.</returns>
			public string NextTokenModeMessage
			{
				get
				{
					return m_nextTokenModeMessage;
				}
				set
				{
					CheckDisposed();
					if (! string.IsNullOrEmpty(value))
					{
						m_nextTokenModeMessage = value;
					}
					else
					{
						throw (new ArgumentNullException("NextTokenModeMessage"));
					}
				}
			}
			
			/// <summary>
			/// Send a request to the server and waits for a response back.
			/// </summary>
			/// <param name="request">Request to be sent to the server.</param>
			/// <returns>Response packet if a valid reponse is received from the server; otherwise Nothing.</returns>
			public RadiusPacket ProcessRequest(RadiusPacket request)
			{
				
				CheckDisposed();
				RadiusPacket response = null;
				// We wait indefinately for the connection to establish. But since this is UDP, the connection
				// will always be successful (locally we're binding to any available UDP port).
				if (m_udpClient.WaitForConnection(- 1) && m_udpClient.IsConnected)
				{
					// We have a UDP socket we can use for exchanging packets.
					DateTime stopTime;
					for (int i = 1; i <= m_requestAttempts; i++)
					{
						m_responseBytes = null;
						m_udpClient.Send(request.BinaryImage);
						
						stopTime = DateTime.Now.AddMilliseconds(m_reponseTimeout);
						while (true)
						{
							Thread.Sleep(1);
							// Stay in the loop until:
							// 1) We receive a response OR
							// 2) We exceed the response timeout duration
							if ((m_responseBytes != null)|| DateTime.Now > stopTime)
							{
								break;
							}
						}
						
						if (m_responseBytes != null)
						{
							// The server sent a response.
							response = new RadiusPacket(m_responseBytes, 0);
							if (response.Identifier == request.Identifier && PCS.IO.Common.CompareBuffers(response.Authenticator, RadiusPacket.CreateResponseAuthenticator(m_sharedSecret, request, response)) == 0)
							{
								// The response has passed the verification.
								break;
							}
							else
							{
								// The response failed the verification, so we'll silently discard it.
								response = null;
							}
						}
					}
				}
				
				return response;
				
			}
			
			/// <summary>
			/// Create a new pin for the user.
			/// </summary>
			/// <param name="username">Name of the user.</param>
			/// <param name="token">Current token of the user.</param>
			/// <param name="pin">New pin of the user.</param>
			/// <returns>True if a new pin is created for the user successfully; otherwise False.</returns>
			/// <remarks>NOTE: This method is specific to RSA RADIUS implementation.</remarks>
			public bool CreateNewPin(string username, string token, string pin)
			{
				
				CheckDisposed();
				if (! string.IsNullOrEmpty(pin))
				{
					byte[] reply;
					RadiusPacket response;
					
					// Step 1: Send username and token for password, receive a challenge response with reply
					//         message worded "Enter a new PIN". [Verification]
					// Step 2: Send username and new ping for password, receive a challenge response with reply
					//         message worded "Please re-enter.  [Attempt #1]
					// Step 3: Send username and new ping for password, receive a challenge response with reply
					//         message worded "PIN Accepted".    [Attempt #2]
					
					response = Authenticate(username, token);
					if (IsUserInNewPinMode(response))
					{
						// User account is really in "New Pin" mode.
						response = Authenticate(username, pin, response.GetAttributeValue(AttributeType.State));
						reply = response.GetAttributeValue(AttributeType.ReplyMessage);
						if (! RadiusPacket.ToText(reply, 0, reply.Length).ToLower().Contains(m_newPinModeMessage2.ToLower()))
						{
							return false; // New pin not accepted in attempt #1.
						}
						
						response = Authenticate(username, pin, response.GetAttributeValue(AttributeType.State));
						reply = response.GetAttributeValue(AttributeType.ReplyMessage);
						if (! RadiusPacket.ToText(reply, 0, reply.Length).ToLower().Contains(m_newPinModeMessage3.ToLower()))
						{
							return false; // New pin not accepted in attempt #2.
						}
						
						return true; // All is good - new pin is created for the user.
					}
					else
					{
						return false;
					}
				}
				else
				{
					throw (new ArgumentNullException("pin"));
				}
				
			}
			
			/// <summary>
			/// Authenticates the username and password against the RADIUS server.
			/// </summary>
			/// <param name="username">Username to be authenticated.</param>
			/// <param name="password">Password to be authenticated.</param>
			/// <returns>Response packet received from the server for the authentication request.</returns>
			/// <remarks>
			/// <para>
			/// The type of response packet (if any) will be one of the following:
			/// <list>
			/// <item>AccessAccept: If the authentication is successful.</item>
			/// <item>AccessReject: If the authentication is not successful.</item>
			/// <item>AccessChallenge: If the server need more information from the user.</item>
			/// </list>
			/// </para>
			/// <para>
			/// When an AccessChallenge response packet is received from the server, it contains a State attribute
			/// that must be included in the AccessRequest packet that is being sent in response to the AccessChallenge
			/// response. So if this method returns an AccessChallenge packet, then this method is to be called again
			/// with the requested information (from ReplyMessage attribute) in the password field and the value State
			/// attribute.
			/// </para>
			/// </remarks>
			public RadiusPacket Authenticate(string username, string password)
			{
				
				return Authenticate(username, password, null);
				
			}
			
			/// <summary>
			/// Authenticates the username and password against the RADIUS server.
			/// </summary>
			/// <param name="username">Username to be authenticated.</param>
			/// <param name="password">Password to be authenticated.</param>
			/// <param name="state">State value from a previous challenge response.</param>
			/// <returns>Response packet received from the server for the authentication request.</returns>
			/// <remarks>
			/// <para>
			/// The type of response packet (if any) will be one of the following:
			/// <list>
			/// <item>AccessAccept: If the authentication is successful.</item>
			/// <item>AccessReject: If the authentication is not successful.</item>
			/// <item>AccessChallenge: If the server need more information from the user.</item>
			/// </list>
			/// </para>
			/// <para>
			/// When an AccessChallenge response packet is received from the server, it contains a State attribute
			/// that must be included in the AccessRequest packet that is being sent in response to the AccessChallenge
			/// response. So if this method returns an AccessChallenge packet, then this method is to be called again
			/// with the requested information (from ReplyMessage attribute) in the password field and the value State
			/// attribute.
			/// </para>
			/// </remarks>
			public RadiusPacket Authenticate(string username, string password, byte[] state)
			{
				
				CheckDisposed();
				if (! string.IsNullOrEmpty(username) && ! string.IsNullOrEmpty(password))
				{
					RadiusPacket request = new RadiusPacket(PacketType.AccessRequest);
					byte[] authenticator = RadiusPacket.CreateRequestAuthenticator(m_sharedSecret);
					
					request.Authenticator = authenticator;
					request.Attributes.Add(new RadiusPacketAttribute(AttributeType.UserName, username));
					request.Attributes.Add(new RadiusPacketAttribute(AttributeType.UserPassword, RadiusPacket.EncryptPassword(password, m_sharedSecret, authenticator)));
					if (state != null)
					{
						// State attribute is used when responding to a AccessChallenge reponse.
						request.Attributes.Add(new RadiusPacketAttribute(AttributeType.State, state));
					}
					
					return ProcessRequest(request);
				}
				else
				{
					throw (new ArgumentException("Username and Password cannot be null."));
				}
				
			}
			
			/// <summary>
			/// Determines whether or not the response indicates that the user account is in "New Pin" mode.
			/// </summary>
			/// <param name="response">Response packet sent by the server.</param>
			/// <returns>True if the user account is in "New Pin" mode; otherwise False.</returns>
			/// <remarks>
			/// <para>A user's account can be in the "New Pin" mode when set on the server.</para>
			/// <para>NOTE: This method is specific to RSA RADIUS implementation.</para>
			/// </remarks>
			public bool IsUserInNewPinMode(RadiusPacket response)
			{
				
				CheckDisposed();
				if (response != null)
				{
					byte[] messageBytes = response.GetAttributeValue(AttributeType.ReplyMessage);
					if (messageBytes != null)
					{
						// Unfortunately, the only way of determining whether or not a user account is in the
						// "New Pin" mode is from the text present in the ReplyMessage attribute of the
						// AccessChallenge response from server.
						string messageString = RadiusPacket.ToText(messageBytes, 0, messageBytes.Length);
						if (messageString.ToLower().Contains(m_newPinModeMessage1.ToLower()))
						{
							return true; // User account is in "New Pin" mode.
						}
					}
				}
				else
				{
					throw (new ArgumentNullException("response"));
				}
				
			}
			
			/// <summary>
			/// Determines whether or not the response indicates that the user account is in "Next Token" mode.
			/// </summary>
			/// <param name="response">Response packet sent by the server.</param>
			/// <returns>True if the user account is in "Next Token" mode; otherwise False.</returns>
			/// <remarks>
			/// <para>
			/// A user's account can enter the "Next Token" mode after the user enters incorrect passwords for a few
			/// times (3 times by default) and then enters the correct password. Note that repeatedly entering
			/// incorrect passwords will disable the user account.
			/// </para>
			/// <para>NOTE: This method is specific to RSA RADIUS implementation.</para>
			/// </remarks>
			public bool IsUserInNextTokenMode(RadiusPacket response)
			{
				
				CheckDisposed();
				if (response != null)
				{
					byte[] messageBytes = response.GetAttributeValue(AttributeType.ReplyMessage);
					if (messageBytes != null)
					{
						// Unfortunately, the only way of determining whether or not a user account is in the
						// "Next Token" mode is from the text present in the ReplyMessage attribute of the
						// AccessChallenge response from server.
						string messageString = RadiusPacket.ToText(messageBytes, 0, messageBytes.Length);
						if (messageString.ToLower().Contains(m_nextTokenModeMessage.ToLower()))
						{
							return true; // User account is in "Next Token" mode.
						}
					}
				}
				else
				{
					throw (new ArgumentNullException("response"));
				}
				
			}
			
			#region " Interface Implementation "
			
			#region " IDisposable "
			
			/// <summary>
			/// Releases the used resources.
			/// </summary>
			public void Dispose()
			{
				
				Dispose(true);
				GC.SuppressFinalize(this);
				
			}
			
			#endregion
			
			#endregion
			
			#endregion
			
			#region " Code Scope: Protected "
			
			/// <summary>
			/// Helper method to check whether or not the object instance has been disposed.
			/// </summary>
			/// <remarks>This method is to be called before performing any operation.</remarks>
			protected void CheckDisposed()
			{
				
				if (m_disposed)
				{
					throw (new ObjectDisposedException(this.GetType().Name));
				}
				
			}
			
			/// <summary>
			/// Releases the used resources.
			/// </summary>
			protected virtual void Dispose(bool disposing)
			{
				
				if (! m_disposed)
				{
					if (disposing)
					{
						m_udpClient.Dispose();
						m_udpClient = null;
						m_udpClient.ReceivedData += new System.EventHandler`1[[PCS.GenericEventArgs`1[[PCS.IdentifiableItem`2[[Guid, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Byte[], mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], PCS.Core, Version=3.0.116.286, Culture=neutral, PublicKeyToken=null]], PCS.Core, Version=3.0.116.286, Culture=neutral, PublicKeyToken=null]](m_udpClient_ReceivedData);
					}
				}
				m_disposed = true;
				
			}
			
			#endregion
			
			#region " Code Scope: Private "
			
			private void m_udpClient_ReceivedData(object sender, GenericEventArgs<IdentifiableItem<Guid, byte[]>> e)
			{
				
				m_responseBytes = e.Argument.Item;
				
			}
			
			#endregion
			
		}
		
	}
}
