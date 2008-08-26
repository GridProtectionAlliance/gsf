using System.Diagnostics;
using System;
//using TVA.Common;
using System.Collections;
using TVA.Interop;
using Microsoft.VisualBasic;
using TVA;
using System.Collections.Generic;
//using TVA.Interop.Bit;
using System.Linq;

//*******************************************************************************************************
//  ChannelCellParsingStateBase.vb - Channel frame cell parsing state base class
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
//  3/7/2005 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************

namespace PhasorProtocols
{
    /// <summary>This class represents the common implementation of the protocol independent parsing state of any kind of data cell.</summary>
    public abstract class ChannelCellParsingStateBase : ChannelParsingStateBase, IChannelCellParsingState
    {



        private int m_phasorCount;
        private int m_analogCount;
        private int m_digitalCount;

        public virtual int PhasorCount
        {
            get
            {
                return m_phasorCount;
            }
            set
            {
                m_phasorCount = value;
            }
        }

        public virtual int AnalogCount
        {
            get
            {
                return m_analogCount;
            }
            set
            {
                m_analogCount = value;
            }
        }

        public virtual int DigitalCount
        {
            get
            {
                return m_digitalCount;
            }
            set
            {
                m_digitalCount = value;
            }
        }

    }
}
