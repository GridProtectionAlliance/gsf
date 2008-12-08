using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Net;
//using PCS.Common;
using PCS.Parsing;

//*******************************************************************************************************
//  PCS.Security.Radius.RadiusPacketAttribute.vb - RADIUS authentication packet attribute
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
		
		public class RadiusPacketAttribute : IBinaryImageProvider, IBinaryDataConsumer
		{
			
			
			// 0                   1                   2
			// 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0
			//+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
			//|     Type      |    Length     |  Value ...
			//+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
			
			#region " Member Declaration "
			
			private AttributeType m_type;
			private byte[] m_value;
			
			#endregion
			
			#region " Code Scope: Public "
			
			/// <summary>
			/// Creates a default instance of RADIUS packet attribute.
			/// </summary>
			public RadiusPacketAttribute()
			{
				
				// No initialization required.
				
			}
			
			/// <summary>
			/// Creates an instance of RADIUS packet attribute.
			/// </summary>
			/// <param name="type">Type of the attribute.</param>
			/// <param name="value">Text value of the attribute.</param>
			public RadiusPacketAttribute(AttributeType type, string value) : this(type, RadiusPacket.GetBytes(value))
			{
				
				
			}
			
			/// <summary>
			/// Creates an instance of RADIUS packet attribute.
			/// </summary>
			/// <param name="type">Type of the attribute.</param>
			/// <param name="value">32-bit unsigned integer value of the attribute.</param>
			[CLSCompliant(false)]public RadiusPacketAttribute(AttributeType type, UInt32 value) : this(type, RadiusPacket.GetBytes(value))
			{
				
				
			}
			
			/// <summary>
			/// Creates an instance of RADIUS packet attribute.
			/// </summary>
			/// <param name="type">Type of the attribute.</param>
			/// <param name="value">IP address value of the attribute.</param>
			public RadiusPacketAttribute(AttributeType type, IPAddress value) : this(type, value.GetAddressBytes())
			{
				
				
			}
			
			/// <summary>
			/// Creates an instance of RADIUS packet attribute.
			/// </summary>
			/// <param name="type">Type of the attribute.</param>
			/// <param name="value">Byte array value of the attribute.</param>
			public RadiusPacketAttribute(AttributeType type, byte[] value)
			{
				
				this.Type = type;
				this.Value = value;
				
			}
			
			/// <summary>
			/// Creates an instance of RADIUS packet attribute.
			/// </summary>
			/// <param name="binaryImage">A byte array.</param>
			/// <param name="startIndex">Starting point in the byte array.</param>
			public RadiusPacketAttribute(byte[] binaryImage, int startIndex)
			{
				
				Initialize(binaryImage, startIndex);
				
			}
			
			/// <summary>
			/// Gets or sets the type of the attribute.
			/// </summary>
			/// <value></value>
			/// <returns>Type of the attribute.</returns>
			public AttributeType Type
			{
				get
				{
					return m_type;
				}
				set
				{
					m_type = value;
				}
			}
			
			/// <summary>
			/// Gets or sets the value of the attribute.
			/// </summary>
			/// <value></value>
			/// <returns>Value of the attribute.</returns>
			public byte[] Value
			{
				get
				{
					return m_value;
				}
				set
				{
					if ((value != null)&& Value.Length > 0)
					{
						// By definition, attribute value cannot be null or zero-length.
						m_value = value;
					}
					else
					{
						throw (new ArgumentNullException("Value"));
					}
				}
			}
			
			#region " Interface Implementations "
			
			#region " IBinaryDataProvider "
			
			/// <summary>
			/// Gets the binary lenght of the attribute.
			/// </summary>
			/// <value></value>
			/// <returns>32-bit signed integer value.</returns>
			public int BinaryLength
			{
				get
				{
					// 2 bytes are fixed + length of the value
					if (m_value == null)
					{
						return 2;
					}
					else
					{
						return 2 + m_value.Length;
					}
				}
			}
			
			/// <summary>
			/// Gets the binary image of the attribute.
			/// </summary>
			/// <value></value>
			/// <returns>A byte array.</returns>
			public byte[] BinaryImage
			{
				get
				{
					byte[] image = PCS.Common.CreateArray<byte>(BinaryLength);
					image[0] = System.Convert.ToByte(m_type);
					image[1] = (byte) BinaryLength;
					if ((m_value != null)&& m_value.Length > 0)
					{
						Array.Copy(m_value, 0, image, 2, m_value.Length);
					}
					else
					{
						throw (new ArgumentNullException("Value", "Attribute value cannot be null or zero-length."));
					}
					
					return image;
				}
			}
			
			#endregion
			
			#region " IBinaryDataConsumer "
			
			/// <summary>
			/// Initializes the attribute from the specified binary image.
			/// </summary>
			/// <param name="binaryImage">A byte array.</param>
			/// <param name="startIndex">Starting point in the byte array.</param>
			/// <returns>Number of bytes used to initialize the attribute.</returns>
			public int Initialize(byte[] binaryImage, int startIndex)
			{
				
				if ((binaryImage != null)&& binaryImage.Length >= 2)
				{
					// We have a valid buffer to work with.
					m_type = (AttributeType) (binaryImage[startIndex]);
					m_value = PCS.Common.CreateArray<byte>(System.Convert.ToInt16(binaryImage[startIndex + 1] - 2));
					Array.Copy(binaryImage, startIndex + 2, m_value, 0, m_value.Length);
					
					return BinaryLength;
				}
				else
				{
					throw (new ArgumentException("Buffer is not valid."));
				}
				
			}
			
			#endregion
			
			#endregion
			
			#endregion
			
		}
		
	}
}
