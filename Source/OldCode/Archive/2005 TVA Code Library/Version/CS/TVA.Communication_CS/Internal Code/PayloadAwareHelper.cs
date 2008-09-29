//*******************************************************************************************************
//  PayloadAwareHelper.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
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

namespace TVA.Communication
{
	internal static class PayloadAwareHelper
	{
		/// <summary>
		/// Size of the header that is prepended to the payload. This header has information about the payload.
		/// </summary>
		public const int PayloadHeaderSize = 8;
		
		/// <summary>
		/// A sequence of bytes that will mark the beginning of a payload.
		/// </summary>
		public static byte[] PayloadBeginMarker = {0xAA, 0xBB, 0xCC, 0xDD};
		
		public static byte[] AddPayloadHeader(byte[] payload)
		{
			// The resulting buffer will be 8 bytes bigger than the payload.

			// Resulting buffer = 4 bytes for payload marker + 4 bytes for the payload size + The payload
			byte[] result = TVA.Common.CreateArray<byte>(payload.Length + PayloadHeaderSize);
			
			// First, copy the the payload marker to the buffer.
			Buffer.BlockCopy(PayloadBeginMarker, 0, result, 0, 4);

			// Then, copy the payload's size to the buffer after the payload marker.
			Buffer.BlockCopy(BitConverter.GetBytes(payload.Length), 0, result, 4, 4);

			// At last, copy the payload after the payload marker and payload size.
			Buffer.BlockCopy(payload, 0, result, 8, payload.Length);
			
			return result;
		}
		
		public static bool HasPayloadBeginMarker(byte[] data)
		{
			for (int i = 0; i <= PayloadBeginMarker.Length - 1; i++)
			{
				if (data[i] != PayloadBeginMarker[i])
					return false;
			}

			return true;
		}
		
		public static int GetPayloadSize(byte[] data)
		{
			if (data.Length >= PayloadHeaderSize && HasPayloadBeginMarker(data))
			{
				// We have a buffer that's at least as big as the payload header and has the payload marker.
				return BitConverter.ToInt32(data, PayloadBeginMarker.Length);
			}
			else
			{
				return - 1;
			}
		}
		
		public static byte[] GetPayload(byte[] data)
		{
			if (data.Length > PayloadHeaderSize && HasPayloadBeginMarker(data))
			{
				int payloadSize = GetPayloadSize(data);

				if (payloadSize > (data.Length - PayloadHeaderSize))
					payloadSize = data.Length - PayloadHeaderSize;
				
				return data.CopyBuffer(PayloadHeaderSize, payloadSize);
			}
			else
			{
				return new byte[] {};
			}
		}
	}
}
