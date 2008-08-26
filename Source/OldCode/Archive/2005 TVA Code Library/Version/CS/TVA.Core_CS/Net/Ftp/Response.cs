using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.IO;
using System.Net.Sockets;

// James Ritchie Carroll - 2003


namespace TVA
{
	namespace Net
	{
		namespace Ftp
		{
			
			
			public class Response
			{
				
				
				private Queue m_responses;
				private int m_code;
				
				public const int InvalidCode = - 1;
				public const int DataChannelOpenedTransferStart = 125;
				public const int FileOkBeginOpenDataChannel = 150;
				public const int ServiceReady = 220;
				public const int ClosingDataChannel = 226;
				public const int EnterPassiveMode = 227;
				public const int RequestFileActionComplete = 250;
				public const int UserLoggedIn = 230;
				public const int UserAcceptedWaitingPass = 331;
				public const int RequestFileActionPending = 350;
				public const int ServiceUnavailable = 421;
				public const int TransferAborted = 426;
				
				internal Response(NetworkStream stream)
				{
					
					string response;
					
					m_responses = new Queue();
					
					do
					{
						response = GetLine(stream);
						
						try
						{
							m_code = InvalidCode;
							m_code = int.Parse(response.Substring(0, 3));
						}
						catch
						{
							throw (new InvalidResponseException("Invalid response", this));
						}
						
						m_responses.Enqueue(response);
					} while (response.Length >= 4 && response[3] == "-");
					
					if (m_code == ServiceUnavailable)
					{
						throw (new ServerDownException(this));
					}
					
				}
				
				public string Message
				{
					get
					{
						return m_responses.Peek().ToString();
					}
				}
				
				public Queue Respones
				{
					get
					{
						return m_responses;
					}
				}
				
				public int Code
				{
					get
					{
						return m_code;
					}
				}
				
				private char ReadAppendChar(NetworkStream stream, System.Text.StringBuilder toAppend)
				{
					
					int i = stream.ReadByte();
					
					if (i > - 1)
					{
						char c = Strings.Chr(i);
						toAppend.Append(c);
						return c;
					}
					else
					{
						throw (new EndOfStreamException("Attempt to read past end of stream"));
					}
					
				}
				
				private string GetLine(NetworkStream stream)
				{
					
					System.Text.StringBuilder buff = new System.Text.StringBuilder(256);
					
					while (true)
					{
						while (ReadAppendChar(stream, buff) != ControlChars.Cr)
						{
						}
						
						while (ReadAppendChar(stream, buff) == ControlChars.Cr)
						{
						}
						
						if (buff[buff.Length - 1] == ControlChars.Lf)
						{
							break;
						}
					}
					
					return buff.ToString();
					
				}
				
			}
			
		}
	}
}
