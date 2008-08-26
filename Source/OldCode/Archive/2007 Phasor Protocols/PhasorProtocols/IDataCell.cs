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
using TVA.Measurements;

//*******************************************************************************************************
//  IDataCell.vb - Data cell interface
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
//  04/16/2005 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************

namespace PhasorProtocols
{
    /// <summary>This interface represents the protocol independent representation of a set of phasor related data values.</summary>
    [CLSCompliant(false)]
    public interface IDataCell : IChannelCell, IMeasurement
    {


        new IDataFrame Parent
        {
            get;
        }

        IConfigurationCell ConfigurationCell
        {
            get;
            set;
        }

        string StationName
        {
            get;
        }

        string IDLabel
        {
            get;
        }

        short StatusFlags
        {
            get;
            set;
        }

        int CommonStatusFlags
        {
            get;
            set;
        }

        bool AllValuesAssigned
        {
            get;
        }

        PhasorValueCollection PhasorValues
        {
            get;
        }

        IFrequencyValue FrequencyValue
        {
            get;
            set;
        }

        AnalogValueCollection AnalogValues
        {
            get;
        }

        DigitalValueCollection DigitalValues
        {
            get;
        }

        // These properties correspond to the CommonStatusFlags enumeration
        // allowing all protocols to implement a common set of status flags

        bool DataIsValid
        {
            get;
            set;
        }

        bool SynchronizationIsValid
        {
            get;
            set;
        }

        DataSortingType DataSortingType
        {
            get;
            set;
        }

        bool PmuError
        {
            get;
            set;
        }

    }
}
