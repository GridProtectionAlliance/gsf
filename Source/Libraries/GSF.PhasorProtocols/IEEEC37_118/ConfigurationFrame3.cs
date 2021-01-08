//******************************************************************************************************
//  ConfigurationFrame3.cs - Gbtc
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
//  12/06/2011 - Andrew Krohne
//       Generated original version of source code.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using GSF.Parsing;

namespace GSF.PhasorProtocols.IEEEC37_118
{
    /// <summary>
    /// Represents the IEEE C37.118 implementation of a <see cref="IConfigurationFrame"/>, type 3, that can be sent or received.
    /// </summary>
    [Serializable]
    public class ConfigurationFrame3 : ConfigurationFrame1
    {
        #region [ Members ]

        // Constants
        private new const int FixedHeaderLength = CommonFrameHeader.FixedLength + 6 + 2;

        // Fields
        private int m_contIDX = 0;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConfigurationFrame3"/>.
        /// </summary>
        /// <remarks>
        /// This constructor is used by <see cref="FrameImageParserBase{TTypeIdentifier,TOutputType}"/> to parse an IEEE C37.118 configuration frame, type 3.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ConfigurationFrame3()
        {
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationFrame3"/> from specified parameters.
        /// </summary>
        /// <param name="timebase">Timebase to use for fraction second resolution.</param>
        /// <param name="idCode">The ID code of this <see cref="ConfigurationFrame3"/>.</param>
        /// <param name="timestamp">The exact timestamp, in <see cref="Ticks"/>, of the data represented by this <see cref="ConfigurationFrame3"/>.</param>
        /// <param name="frameRate">The defined frame rate of this <see cref="ConfigurationFrame3"/>.</param>
        /// <remarks>
        /// This constructor is used by a consumer to generate an IEEE C37.118 configuration frame, type 3.
        /// </remarks>
        public ConfigurationFrame3(uint timebase, ushort idCode, Ticks timestamp, ushort frameRate)
            : base(timebase, idCode, timestamp, frameRate)
        {
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationFrame3"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ConfigurationFrame3(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region [ Methods ]

        /*
        protected override int ParseHeaderImage(byte[] buffer, int startIndex, int length)
        {
            // Skip past header that was already parsed...
            startIndex += CommonFrameHeader.FixedLength;
            // State.CONT_IDX = BigEndian.ToInt16(buffer, startIndex); FIXME: For now, this is completely ignored
            m_timebase = BigEndian.ToUInt32(buffer, startIndex + 2) & ~Common.TimeQualityFlagsMask;
            State.CellCount = BigEndian.ToUInt16(buffer, startIndex + 6);

            return FixedHeaderLength;
        }
        */

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="FrameType"/> of this <see cref="ConfigurationFrame3"/>.
        /// </summary>
        public override FrameType TypeID => IEEEC37_118.FrameType.ConfigurationFrame3;

        /// <summary>
        /// Gets the binary header image of the <see cref="ConfigurationFrame3"/> object.
        /// </summary>
        protected override byte[] HeaderImage
        {
            get
            {
                // Make sure to provide proper frame length for use in the common header image
                unchecked
                {
                    CommonHeader.FrameLength = (ushort)BinaryLength;
                }

                byte[] buffer = new byte[FixedHeaderLength];
                int index = 0;

                CommonHeader.BinaryImage.CopyImage(buffer, ref index, CommonFrameHeader.FixedLength);
                BigEndian.CopyBytes(m_contIDX, buffer, index);
                BigEndian.CopyBytes(m_timebase, buffer, index + 2);
                BigEndian.CopyBytes((ushort)Cells.Count, buffer, index + 6);

                return buffer;
            }
        }

        #endregion

        #region [ Methods ]

        // ReSharper disable once RedundantOverriddenMember

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            // TODO: Serialize configuration frame
            //info.AddValue("frameHeader", m_frameHeader, typeof(CommonFrameHeader));
            //info.AddValue("timebase", m_timebase);
        }

        #endregion
    }
}