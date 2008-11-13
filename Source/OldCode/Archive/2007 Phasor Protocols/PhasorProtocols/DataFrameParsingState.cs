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

//*******************************************************************************************************
//  DataFrameParsingState.vb - Data frame parsing state class
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
//  01/14/2005 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************

namespace PCS.PhasorProtocols
{
    /// <summary>This class represents the protocol independent common implementation the parsing state of a data frame that can be sent or received from a PMU.</summary>
    [CLSCompliant(false)]
    public class DataFrameParsingState : ChannelFrameParsingStateBase<IDataCell>, IDataFrameParsingState
    {



        private IConfigurationFrame m_configurationFrame;

        public DataFrameParsingState(DataCellCollection cells, ushort frameLength, IConfigurationFrame configurationFrame, CreateNewCellFunctionSignature<IDataCell> createNewCellFunction)
            : base(cells, frameLength, createNewCellFunction)
        {


            CellCount = configurationFrame.Cells.Count;
            m_configurationFrame = configurationFrame;

        }

        public override System.Type DerivedType
        {
            get
            {
                return this.GetType();
            }
        }

        public virtual IConfigurationFrame ConfigurationFrame
        {
            get
            {
                return m_configurationFrame;
            }
        }

        public virtual new DataCellCollection Cells
        {
            get
            {
                return (DataCellCollection)base.Cells;
            }
        }

    }
}
