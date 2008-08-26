using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
//using TVA.Common;


namespace TVA.Communication
{
	internal sealed class PayloadAwareHelper
	{
		
		
		private PayloadAwareHelper()
		{
			
			// This class contains only global functions and is not meant to be instantiated
			
		}
		
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
				{
					return false;
				}
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
				{
					payloadSize = data.Length - PayloadHeaderSize;
				}
				
				return TVA.IO.Common.CopyBuffer(data, PayloadHeaderSize, payloadSize);
			}
			else
			{
				return new byte[] {};
			}
			
		}
		
	}
	
}
