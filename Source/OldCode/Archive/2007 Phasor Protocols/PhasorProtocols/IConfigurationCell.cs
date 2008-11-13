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

//*******************************************************************************************************
//  IConfigurationCell.vb - Configuration cell interface
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

namespace PCS.PhasorProtocols
{
    /// <summary>This interface represents the protocol independent representation of a set of configuration related data settings (typically related to a PMU).</summary>
    [CLSCompliant(false)]
    public interface IConfigurationCell : IChannelCell, IComparable<IConfigurationCell>, IComparable
    {


        new IConfigurationFrame Parent
        {
            get;
        }

        string StationName
        {
            get;
            set;
        }

        byte[] StationNameImage
        {
            get;
        }

        int MaximumStationNameLength
        {
            get;
        }

        string IDLabel
        {
            get;
            set;
        }

        byte[] IDLabelImage
        {
            get;
        }

        int IDLabelLength
        {
            get;
        }

        PhasorDefinitionCollection PhasorDefinitions
        {
            get;
        }

        DataFormat PhasorDataFormat
        {
            get;
            set;
        }

        CoordinateFormat PhasorCoordinateFormat
        {
            get;
            set;
        }

        IFrequencyDefinition FrequencyDefinition
        {
            get;
            set;
        }

        DataFormat FrequencyDataFormat
        {
            get;
            set;
        }

        LineFrequency NominalFrequency
        {
            get;
            set;
        }

        AnalogDefinitionCollection AnalogDefinitions
        {
            get;
        }

        DataFormat AnalogDataFormat
        {
            get;
            set;
        }

        DigitalDefinitionCollection DigitalDefinitions
        {
            get;
        }

        short FrameRate
        {
            get;
        }

        ushort RevisionCount
        {
            get;
            set;
        }

    }
}
