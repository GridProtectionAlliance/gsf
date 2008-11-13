using System.Diagnostics;
using System;
//using PCS.Common;
using System.Collections;
using PCS.Interop;
using Microsoft.VisualBasic;
using PCS;
using System.Collections.Generic;
//using PCS.Interop.Bit;
using System.Linq;
using System.Runtime.Serialization;
//using PCS.IO.Compression.Common;

//*******************************************************************************************************
//  DataFrame.vb - FNet Data Frame
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2008
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  02/08/2007 - J. Ritchie Carroll & Jian (Ryan) Zuo
//       Initial version of source generated
//
//*******************************************************************************************************


namespace PCS.PhasorProtocols
{
    namespace FNet
    {

        // This is essentially a "row" of PMU data at a given timestamp
        [CLSCompliant(false), Serializable()]
        public class DataFrame : DataFrameBase
        {



            private short m_sampleIndex;

            protected DataFrame()
            {
            }

            protected DataFrame(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {


                // Deserialize data frame
                m_sampleIndex = info.GetInt16("sampleIndex");

            }

            public DataFrame(long ticks, ConfigurationFrame configurationFrame)
                : base(new DataCellCollection(), ticks, configurationFrame)
            {


            }

            public DataFrame(ConfigurationFrame configurationFrame, byte[] binaryImage, int startIndex)
                : base(new DataFrameParsingState(new DataCellCollection(), 0, configurationFrame, FNet.DataCell.CreateNewDataCell), binaryImage, startIndex)
            {


            }

            public DataFrame(IDataFrame dataFrame)
                : base(dataFrame)
            {


            }

            /// <summary>
            /// Return the type
            /// </summary>

            public override System.Type DerivedType
            {
                get
                {
                    return this.GetType();
                }
            }

            public new DataCellCollection Cells
            {
                get
                {
                    return (DataCellCollection)base.Cells;
                }
            }

            public new ConfigurationFrame ConfigurationFrame
            {
                get
                {
                    return (ConfigurationFrame)base.ConfigurationFrame;
                }
                set
                {
                    base.ConfigurationFrame = value;
                }
            }

            /// <summary>
            /// Set and Return the SampleIndex
            /// </summary>
            public short SampleIndex
            {
                get
                {
                    return m_sampleIndex;
                }
                set
                {
                    m_sampleIndex = value;
                }
            }

            /// <summary>
            /// Return the checksum verification result. Since FNET data has no checksum, it is always valid.
            /// </summary>
            protected override bool ChecksumIsValid(byte[] buffer, int startIndex)
            {

                // FNet uses no checksum, we assume data packet is valid
                return true;

            }

            public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            {

                base.GetObjectData(info, context);

                // Serialize data frame
                info.AddValue("sampleIndex", m_sampleIndex);

            }

            /// <summary>
            /// Set and return the attributes of the FNET protocol
            /// </summary>

            public override Dictionary<string, string> Attributes
            {
                get
                {
                    Dictionary<string, string> baseAttributes = base.Attributes;

                    baseAttributes.Add("Sample Index", SampleIndex.ToString());

                    return baseAttributes;
                }
            }

        }

    }
}
