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

//*******************************************************************************************************
//  ConfigurationCellCollection.vb - Configuration cell collection class
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
    /// <summary>This class represents the protocol independent collection of the common implementation of a set of configuration related data settings that can be sent or received from a PMU.</summary>
    [CLSCompliant(false), Serializable()]
    public class ConfigurationCellCollection : ChannelCellCollectionBase<IConfigurationCell>
    {



        protected ConfigurationCellCollection()
        {
        }

        protected ConfigurationCellCollection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {


        }

        public ConfigurationCellCollection(int maximumCount, bool constantCellLength)
            : base(maximumCount, constantCellLength)
        {


        }

        public override Type DerivedType
        {
            get
            {
                return this.GetType();
            }
        }

        public virtual bool TryGetByIDLabel(string label, ref IConfigurationCell configurationCell)
        {

            foreach (IConfigurationCell cell in this)
            {
                if (string.Compare(cell.IDLabel, label, true) == 0)
                {
                    configurationCell = cell;
                    return true;
                }
            }

            configurationCell = null;
            return false;

        }

        public virtual bool TryGetByIDCode(ushort idCode, ref IConfigurationCell configurationCell)
        {

            foreach (IConfigurationCell cell in this)
            {
                if (cell.IDCode == idCode)
                {
                    configurationCell = cell;
                    return true;
                }
            }

            configurationCell = null;
            return false;

        }

        public virtual int IndexOfIDLabel(string label)
        {

            for (int index = 0; index <= Count - 1; index++)
            {
                if (string.Compare(this[index].IDLabel, label, true) == 0)
                {
                    return index;
                }
            }

            return -1;

        }

        public virtual int IndexOfIDCode(ushort idCode)
        {

            for (int index = 0; index <= Count - 1; index++)
            {
                if (this[index].IDCode == idCode)
                {
                    return index;
                }
            }

            return -1;

        }

    }
}
