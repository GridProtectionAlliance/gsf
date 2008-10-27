//*******************************************************************************************************
//  Payload.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [PCS]
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  07/06/2006 - Pinal C. Patel
//       Original version of source code generated
//  09/29/2008 - James R Carroll
//       Converted to C#.
//
//*******************************************************************************************************

using System;

namespace PCS.Communication
{
	internal static class Payload
	{
		/// <summary>
		/// Size of the header that is prepended to the payload. This header has information about the payload.
		/// </summary>
		public const int HeaderSize = 8;
		
		/// <summary>
		/// A sequence of bytes that will mark the beginning of a payload.
		/// </summary>
		public static byte[] BeginMarker = {0xAA, 0xBB, 0xCC, 0xDD};
		
		public static byte[] AddHeader(byte[] payload)
		{
			// The resulting buffer will be 8 bytes bigger than the payload.

			// Resulting buffer = 4 bytes for payload marker + 4 bytes for the payload size + The payload
			byte[] result = new byte[payload.Length + HeaderSize];
			
			// First, copy the the payload marker to the buffer.
			Buffer.BlockCopy(BeginMarker, 0, result, 0, 4);

			// Then, copy the payload's size to the buffer after the payload marker.
			Buffer.BlockCopy(BitConverter.GetBytes(payload.Length), 0, result, 4, 4);

			// At last, copy the payload after the payload marker and payload size.
			Buffer.BlockCopy(payload, 0, result, 8, payload.Length);
			
			return result;
		}
		
		public static bool HasBeginMarker(byte[] data)
		{
			for (int i = 0; i <= BeginMarker.Length - 1; i++)
			{
				if (data[i] != BeginMarker[i])
					return false;
			}

			return true;
		}
		
		public static int GetSize(byte[] data)
		{
			if (data.Length >= HeaderSize && HasBeginMarker(data))
			{
				// We have a buffer that's at least as big as the payload header and has the payload marker.
				return BitConverter.ToInt32(data, BeginMarker.Length);
			}
			else
			{
				return - 1;
			}
		}
		
		public static byte[] Retrieve(byte[] data)
		{
			if (data.Length > HeaderSize && HasBeginMarker(data))
			{
				int payloadSize = GetSize(data);

				if (payloadSize > (data.Length - HeaderSize))
					payloadSize = data.Length - HeaderSize;
				
				return data.CopyBuffer(HeaderSize, payloadSize);
			}
			else
			{
				return new byte[] {};
			}
		}
	}
}
