using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using TVA.Parsing;

// 03/07/2007


namespace TVA
{
	namespace Interop
	{
		
		public class VBArrayDescriptor : IBinaryDataProvider
		{
			
			
			#region " Member Declaration "
			
			private List<DimensionDescriptor> m_arrayDimensionDescriptors;
			
			#endregion
			
			#region " Public Code "
			
			public VBArrayDescriptor(int[] arrayLengths, int[] arrayLowerBounds)
			{
				
				if (arrayLengths.Length == arrayLowerBounds.Length)
				{
					m_arrayDimensionDescriptors = new List<DimensionDescriptor>();
					for (int i = 0; i <= arrayLengths.Length - 1; i++)
					{
						m_arrayDimensionDescriptors.Add(new DimensionDescriptor(arrayLengths[i], arrayLowerBounds[i]));
					}
				}
				else
				{
					throw (new ArgumentException("Number of lengths and lower bounds must be the same."));
				}
				
			}
			
			#region " Shared Code "
			
			public static VBArrayDescriptor ZeroBasedOneDimensionalArray(int arrayLength)
			{
				return new VBArrayDescriptor(new int[] {arrayLength}, new int[] {0});
			}
			
			public static VBArrayDescriptor OneBasedOneDimensionalArray(int arrayLength)
			{
				return new VBArrayDescriptor(new int[] {arrayLength}, new int[] {1});
			}
			
			public static VBArrayDescriptor ZeroBasedTwoDimensionalArray(int dimensionOneLength, int dimensionTwoLength)
			{
				return new VBArrayDescriptor(new int[] {dimensionOneLength, dimensionTwoLength}, new int[] {0, 0});
			}
			
			public static VBArrayDescriptor OneBasedTwoDimensionalArray(int dimensionOneLength, int dimensionTwoLength)
			{
				return new VBArrayDescriptor(new int[] {dimensionOneLength, dimensionTwoLength}, new int[] {1, 1});
			}
			
			#endregion
			
			#region " IBinaryDataProvider Implementation "
			
			public byte[] BinaryImage
			{
				get
				{
					byte[] image = TVA.Common.CreateArray<byte>(this.BinaryLength);
					
					Array.Copy(BitConverter.GetBytes(m_arrayDimensionDescriptors.Count), 0, image, 0, 2);
					for (int i = 0; i <= m_arrayDimensionDescriptors.Count - 1; i++)
					{
						Array.Copy(BitConverter.GetBytes(m_arrayDimensionDescriptors(i).Length), 0, image, (i * DimensionDescriptor.BinaryLength) + 2, 4);
						Array.Copy(BitConverter.GetBytes(m_arrayDimensionDescriptors(i).LowerBound), 0, image, (i * DimensionDescriptor.BinaryLength) + 6, 4);
					}
					
					return image;
				}
			}
			
			public int BinaryLength
			{
				get
				{
					return 2 + 8 * m_arrayDimensionDescriptors.Count;
				}
			}
			
			#endregion
			
			#endregion
			
			#region " Private Code "
			
			private class DimensionDescriptor
			{
				
				
				public DimensionDescriptor(int dimensionLength, int dimensionLowerBound)
				{
					
					Length = dimensionLength;
					LowerBound = dimensionLowerBound;
					
				}
				
				public int Length;
				
				public int LowerBound;
				
				public const int BinaryLength = 8;
				
			}
			
			#endregion
			
		}
		
	}
}
