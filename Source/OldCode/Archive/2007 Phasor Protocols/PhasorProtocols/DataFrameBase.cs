using System.Diagnostics;
using System;
////using PCS.Common;
using System.Collections;
using PCS.Interop;
using Microsoft.VisualBasic;
using PCS;
using System.Collections.Generic;
////using PCS.Interop.Bit;
using System.Linq;
using System.Runtime.Serialization;

//*******************************************************************************************************
//  DataFrameBase.vb - Data frame base class
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2008
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/14/2005 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************


namespace PCS.PhasorProtocols
{
    /// <summary>This class represents the protocol independent common implementation of a data frame that can be sent or received from a PMU.</summary>
    [CLSCompliant(false), Serializable()]
    public abstract class DataFrameBase : ChannelFrameBase<IDataCell>, IDataFrame
    {



        private IConfigurationFrame m_configurationFrame;

        protected DataFrameBase()
        {
        }

        protected DataFrameBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {


            // Deserialize data frame
            m_configurationFrame = (IConfigurationFrame)info.GetValue("configurationFrame", typeof(IConfigurationFrame));

        }

        protected DataFrameBase(DataCellCollection cells)
            : base(cells)
        {


        }

        protected DataFrameBase(DataCellCollection cells, long ticks, IConfigurationFrame configurationFrame)
            : base(0, cells, ticks)
        {


            m_configurationFrame = configurationFrame;

        }

        //// Derived classes are expected to expose a Public Sub New(ByVal configurationFrame As IConfigurationFrame, ByVal binaryImage As Byte(), ByVal startIndex As int)
        //// and automatically pass in parsing state
        //protected DataFrameBase(IDataFrameParsingState state, byte[] binaryImage, int startIndex)
        //    : base(state, binaryImage, startIndex)
        //{


        //    m_configurationFrame = state.ConfigurationFrame;

        //}

        // Derived classes are expected to expose a Public Sub New(ByVal dataFrame As IDataFrame)
        protected DataFrameBase(IDataFrame dataFrame)
            : this(dataFrame.Cells, dataFrame.Ticks, dataFrame.ConfigurationFrame)
        {


        }

        public override FundamentalFrameType FrameType
        {
            get
            {
                return PhasorProtocols.FundamentalFrameType.DataFrame;
            }
        }

        public virtual IConfigurationFrame ConfigurationFrame
        {
            get
            {
                return m_configurationFrame;
            }
            set
            {
                m_configurationFrame = value;
            }
        }

        public override ushort IDCode
        {
            get
            {
                return m_configurationFrame.IDCode;
            }
            set
            {
                throw (new NotSupportedException("IDCode of a data frame is read-only, change IDCode of associated configuration frame instead"));
            }
        }

        public virtual new DataCellCollection Cells
        {
            get
            {
                return (DataCellCollection)base.Cells;
            }
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {

            base.GetObjectData(info, context);

            // Serialize data frame
            info.AddValue("configurationFrame", m_configurationFrame, typeof(IConfigurationFrame));

        }

    }
}
