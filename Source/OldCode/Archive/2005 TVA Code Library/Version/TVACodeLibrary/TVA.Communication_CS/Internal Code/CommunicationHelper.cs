using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
//using TVA.IO.Compression.Common;
using TVA.Security.Cryptography;
//using TVA.Security.Cryptography.Common;

// 06-01-06


namespace TVA.Communication
{
	internal sealed class CommunicationHelper
	{
		
		
		private CommunicationHelper()
		{
			
			// This class contains only global functions and is not meant to be instantiated
			
		}
		
		/// <summary>
		/// Gets an IP endpoint for the specified host name and port number.
		/// </summary>
		/// <param name="hostNameOrAddress">The host name or IP address to resolve.</param>
		/// <param name="port">The port number to be associated with the address.</param>
		/// <returns>IP endpoint for the specified host name and port number.</returns>
		public static IPEndPoint GetIpEndPoint(string hostNameOrAddress, int port)
		{
			
			try
			{
				return new IPEndPoint(Dns.GetHostEntry(hostNameOrAddress).AddressList(0), port);
			}
			catch (SocketException)
			{
				// SocketException will be thrown if the host is not found, so we'll try manual IP
				return new IPEndPoint(IPAddress.Parse(hostNameOrAddress), port);
			}
			catch
			{
				throw;
			}
			
		}
		
		/// <summary>
		/// Determines whether the specified port is valid.
		/// </summary>
		/// <param name="port">The port number to be validated.</param>
		/// <returns>True if the port number is valid.</returns>
		public static bool ValidPortNumber(string port)
		{
			
			int portNumber;
			if (int.TryParse(port, ref portNumber))
			{
				// The specified port is a valid integer value.
				if (portNumber >= 0 && portNumber <= 65535)
				{
					// The port number is within the valid range.
					return true;
				}
				else
				{
					throw (new ArgumentOutOfRangeException("Port", "Port number must be between 0 and 65535."));
				}
			}
			else
			{
				throw (new ArgumentException("Port number is not a valid number."));
			}
			
		}
		
		public static byte[] CompressData(byte[] data, TVA.IO.Compression.CompressLevel compressionLevel)
		{
			
			try
			{
				if (compressionLevel != System.IO.Compression.CompressLevel.NoCompression)
				{
					return ((MemoryStream) (TVA.IO.Compression.Common.Compress(Serialization.GetStream(data), compressionLevel))).ToArray();
				}
				else
				{
					// No compression is required.
					return data;
				}
			}
			catch (Exception)
			{
				// We'll return what we received if encounter an exception during compression.
				return data;
			}
			
		}
		
		public static byte[] UncompressData(byte[] data, TVA.IO.Compression.CompressLevel compressionLevel)
		{
			
			try
			{
				if (compressionLevel != System.IO.Compression.CompressLevel.NoCompression)
				{
					return ((MemoryStream) (TVA.IO.Compression.Common.Uncompress(Serialization.GetStream(data)))).ToArray();
				}
				else
				{
					// No uncompression is required.
					return data;
				}
			}
			catch (Exception)
			{
				// We'll return what we received if encounter an exception during uncompression.
				return data;
			}
			
		}
		
		public static byte[] EncryptData(byte[] data, string encryptionKey, TVA.Security.Cryptography.EncryptLevel encryptionLevel)
		{
			
			if (! string.IsNullOrEmpty(encryptionKey) && encryptionLevel != EncryptLevel.None)
			{
				byte[] key = System.Text.Encoding.ASCII.GetBytes(encryptionKey);
				byte[] iv = System.Text.Encoding.ASCII.GetBytes(encryptionKey);
				return TVA.Security.Cryptography.Common.Encrypt(data, key, iv, encryptionLevel);
			}
			else
			{
				// No encryption is required.
				return data;
			}
			
		}
		
		public static byte[] DecryptData(byte[] data, string encryptionKey, TVA.Security.Cryptography.EncryptLevel encryptionLevel)
		{
			
			if (! string.IsNullOrEmpty(encryptionKey) && encryptionLevel != EncryptLevel.None)
			{
				byte[] key = System.Text.Encoding.ASCII.GetBytes(encryptionKey);
				byte[] iv = System.Text.Encoding.ASCII.GetBytes(encryptionKey);
				return TVA.Security.Cryptography.Common.Decrypt(data, key, iv, encryptionLevel);
			}
			else
			{
				return data;
			}
			
		}
		
		public static bool IsDestinationReachable(IPEndPoint targetIPEndPoint)
		{
			
			try
			{
				// We'll check if the target endpoint exist by sending empty data to it and then wait for data from it.
				// If the endpoint doesn't exist then we'll receive a ConnectionReset socket exception.
				EndPoint targetEndPoint = (EndPoint) targetIPEndPoint;
				using (Socket targetChecker = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
				{
					targetChecker.ReceiveTimeout = 1;
					targetChecker.SendTo(new byte[] {}, targetEndPoint);
					targetChecker.ReceiveFrom(new byte[] {}, ref targetEndPoint);
				}
				
			}
			catch (SocketException ex)
			{
				switch (ex.SocketErrorCode)
				{
					case SocketError.ConnectionReset:
						// This means that the target endpoint is unreachable.
						return false;
				}
			}
			catch (Exception)
			{
				// We'll ignore any other exceptions we might encounter.
			}
			
			return true;
			
		}
		
	}
}
