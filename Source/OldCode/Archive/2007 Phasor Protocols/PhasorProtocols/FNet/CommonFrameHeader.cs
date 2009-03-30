//*******************************************************************************************************
//  CommonFrameHeader.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  03/20/2009 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Text;
using PCS.Parsing;

namespace PCS.PhasorProtocols.FNet
{
    /// <summary>
    /// Represents the common header for a F-NET frame of data.
    /// </summary>
    /// <remarks>
    /// Because of its simplicity, all F-NET parsing is handled in the constructor of
    /// this class. Subsequently all parsed data is available as an array of strings
    /// where needed.
    /// </remarks>
    public class CommonFrameHeader : CommonHeaderBase<int>
    {
        #region [ Members ]

        // Fields
        private string[] m_data;
        private int m_parsedLength;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="CommonFrameHeader"/> from given <paramref name="binaryImage"/>.
        /// </summary>
        /// <param name="binaryImage">Buffer that contains data to parse.</param>
        /// <param name="startIndex">Start index into buffer where valid data begins.</param>
        /// <param name="length">Length of valid data in <paramref name="binaryImage"/>.</param>
        public CommonFrameHeader(byte[] binaryImage, int startIndex, int length)
        {
            // Validate F-NET data image
            if (binaryImage[startIndex] != Common.StartByte)
                throw new InvalidOperationException("Bad data stream, expected start byte 0x01 as first byte in F-NET frame, got " + binaryImage[startIndex].ToString("X").PadLeft(2, '0'));

            int endIndex = 0, stopIndex = 0;

            for (int x = startIndex; x < length; x++)
            {
                if (binaryImage[x] == Common.EndByte)
                {
                    // We continue to scan through duplicate end bytes
                    endIndex = x;
                    
                    if (stopIndex == 0)
                        stopIndex = x;
                }
                else if (endIndex != 0)
                    break;
            }

            if (stopIndex == 0)
                throw new InvalidOperationException("Bad data stream, did not find stop byte 0x00 in F-NET frame");

            // Parse F-NET data frame into individual fields separated by spaces
            m_data = Encoding.ASCII.GetString(binaryImage, startIndex + 1, stopIndex - startIndex - 1).RemoveDuplicateWhiteSpace().Trim().Split(' ');

            // Make sure all the needed data elements exist (could be a bad frame)
            if (m_data.Length < 8)
                throw new InvalidOperationException("Bad data stream, invalid number of data elements encountered in F-NET data stream line: \"" + Encoding.ASCII.GetString(binaryImage, startIndex + 1, stopIndex - startIndex - 1).RemoveControlCharacters().Trim() + "\".  Got " + m_data.Length + " elements, expected 8.");

            // Calculate total bytes parsed including start and stop bytes
            m_parsedLength = endIndex - startIndex + 1;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets F-NET data elements parsed during construction.
        /// </summary>
        public string[] DataElements
        {
            get
            {
                return m_data;
            }
        }

        /// <summary>
        /// Gets length of data parsed during construction.
        /// </summary>
        public int ParsedLength
        {
            get
            {
                return m_parsedLength;
            }
        }

        #endregion       
    }
}