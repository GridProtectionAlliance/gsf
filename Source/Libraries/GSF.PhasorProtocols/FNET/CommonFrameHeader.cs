//******************************************************************************************************
//  CommonFrameHeader.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  03/20/2009 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Text;
using GSF.Parsing;

namespace GSF.PhasorProtocols.FNET
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

    #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="CommonFrameHeader"/> from given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">Buffer that contains data to parse.</param>
        /// <param name="startIndex">Start index into buffer where valid data begins.</param>
        /// <param name="length">Length of valid data in <paramref name="buffer"/>.</param>
        public CommonFrameHeader(byte[] buffer, int startIndex, int length)
        {
            // Validate F-NET data image
            if (buffer[startIndex] != Common.StartByte)
                throw new InvalidOperationException("Bad data stream, expected start byte 0x01 as first byte in F-NET frame, got 0x" + buffer[startIndex].ToString("X").PadLeft(2, '0'));

            int endIndex = -1, stopIndex = Array.IndexOf(buffer, Common.EndByte, startIndex, length);

            if (stopIndex < 0)
                throw new InvalidOperationException("Bad data stream, did not find stop byte 0x00 in F-NET frame");

            for (int x = stopIndex; x < length; x++)
            {
                // We continue to scan through duplicate end bytes (nulls)
                if (buffer[x] == Common.EndByte)
                    endIndex = x;
                else if (endIndex >= 0)
                    break;
            }

            if (endIndex > -1)
            {
                // Parse F-NET data frame into individual fields separated by spaces
                DataElements = Encoding.ASCII.GetString(buffer, startIndex + 1, stopIndex - startIndex - 1).RemoveDuplicateWhiteSpace().Trim().Split(' ');

                // Make sure all the needed data elements exist (could be a bad frame)
                if (DataElements.Length != 8)
                    throw new InvalidOperationException("Bad data stream, invalid number of data elements encountered in F-NET data stream line: \"" + Encoding.ASCII.GetString(buffer, startIndex + 1, stopIndex - startIndex - 1).RemoveControlCharacters().Trim() + "\".  Got " + DataElements.Length + " elements, expected 8.");

                // Remove any extraneous spaces or control characters that may have been injected by the source device
                for (int i = 0; i < DataElements.Length; i++)
                {
                    DataElements[i] = DataElements[i].RemoveWhiteSpace().RemoveControlCharacters();
                }

                // Calculate total bytes parsed including start and stop bytes
                ParsedLength = endIndex - startIndex + 1;
            }
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets F-NET data elements parsed during construction.
        /// </summary>
        public string[] DataElements { get; }

        /// <summary>
        /// Gets length of data parsed during construction.
        /// </summary>
        public int ParsedLength { get; }

    #endregion
    }
}