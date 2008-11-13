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
//  IChannelDefinition.vb - Channel data definition interface
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
//  02/18/2005 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************


namespace PCS.PhasorProtocols
{
    /// <summary>This interface represents a protocol independent definition of any kind of data.</summary>
    [CLSCompliant(false)]
    public interface IChannelDefinition : IChannel, ISerializable, IEquatable<IChannelDefinition>, IComparable<IChannelDefinition>, IComparable
    {


        IConfigurationCell Parent
        {
            get;
        }

        DataFormat DataFormat
        {
            get;
        }

        int Index
        {
            get;
            set;
        }

        float Offset
        {
            get;
            set;
        }

        int ScalingFactor
        {
            get;
            set;
        }

        int MaximumScalingFactor
        {
            get;
        }

        float ConversionFactor
        {
            get;
            set;
        }

        float ScalePerBit
        {
            get;
        }

        string Label
        {
            get;
            set;
        }

        byte[] LabelImage
        {
            get;
        }

        int MaximumLabelLength
        {
            get;
        }

    }
}
