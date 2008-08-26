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
using System.Runtime.Serialization;
//using PhasorProtocols.Ieee1344.Common;

//*******************************************************************************************************
//  DataCell.vb - IEEE 1344 PMU Data Cell
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
//  11/12/2004 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************


namespace PhasorProtocols
{
    namespace Ieee1344
    {

        // This data cell represents what most might call a "field" in table of rows - it is a single unit of data for a specific PMU
        [CLSCompliant(false), Serializable()]
        public class DataCell : DataCellBase
        {



            protected DataCell()
            {
            }

            protected DataCell(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {


            }

            public DataCell(IDataFrame parent, IConfigurationCell configurationCell)
                : base(parent, false, configurationCell, PhasorProtocols.Ieee1344.Common.MaximumPhasorValues, PhasorProtocols.Ieee1344.Common.MaximumAnalogValues, PhasorProtocols.Ieee1344.Common.MaximumDigitalValues)
            {


                int x;

                // Initialize phasor values and frequency value with an empty value
                for (x = 0; x <= ConfigurationCell.PhasorDefinitions.Count - 1; x++)
                {
                    PhasorValues.Add(new PhasorValue(this, ConfigurationCell.PhasorDefinitions[x], float.NaN, float.NaN));
                }

                // Initialize frequency and df/dt
                FrequencyValue = new FrequencyValue(this, configurationCell.FrequencyDefinition, float.NaN, float.NaN);

                // Initialize any digital values
                for (x = 0; x <= ConfigurationCell.DigitalDefinitions.Count - 1; x++)
                {
                    DigitalValues.Add(new DigitalValue(this, ConfigurationCell.DigitalDefinitions[x], -1));
                }

            }

            public DataCell(IDataCell dataCell)
                : base(dataCell)
            {


            }

            public DataCell(IDataFrame parent, DataFrameParsingState state, int index, byte[] binaryImage, int startIndex)
                : base(parent, false, PhasorProtocols.Ieee1344.Common.MaximumPhasorValues, PhasorProtocols.Ieee1344.Common.MaximumAnalogValues, PhasorProtocols.Ieee1344.Common.MaximumDigitalValues, new DataCellParsingState(state.ConfigurationFrame.Cells[index], Ieee1344.PhasorValue.CreateNewPhasorValue, Ieee1344.FrequencyValue.CreateNewFrequencyValue, null, Ieee1344.DigitalValue.CreateNewDigitalValue), binaryImage, startIndex)
            {


            }

            internal static IDataCell CreateNewDataCell(IChannelFrame parent, IChannelFrameParsingState<IDataCell> state, int index, byte[] binaryImage, int startIndex)
            {

                return new DataCell((IDataFrame)parent, (DataFrameParsingState)state, index, binaryImage, startIndex);

            }

            public override System.Type DerivedType
            {
                get
                {
                    return this.GetType();
                }
            }

            public new DataFrame Parent
            {
                get
                {
                    return (DataFrame)base.Parent;
                }
            }

            public new ConfigurationCell ConfigurationCell
            {
                get
                {
                    return (ConfigurationCell)base.ConfigurationCell;
                }
                set
                {
                    base.ConfigurationCell = value;
                }
            }

            public override bool DataIsValid
            {
                get
                {
                    return (StatusFlags & Bit.Bit14) == 0;
                }
                set
                {
                    if (value)
                    {
                        StatusFlags = (short)(StatusFlags & ~Bit.Bit14);
                    }
                    else
                    {
                        StatusFlags = (short)(StatusFlags | Bit.Bit14);
                    }
                }
            }

            public override bool SynchronizationIsValid
            {
                get
                {
                    return (StatusFlags & Bit.Bit15) == 0;
                }
                set
                {
                    if (value)
                    {
                        StatusFlags = (short)(StatusFlags & ~Bit.Bit15);
                    }
                    else
                    {
                        StatusFlags = (short)(StatusFlags | Bit.Bit15);
                    }
                }
            }

            public override DataSortingType DataSortingType
            {
                get
                {
                    return (SynchronizationIsValid ? PhasorProtocols.DataSortingType.ByTimestamp : PhasorProtocols.DataSortingType.ByArrival);
                }
                set
                {
                    // We just ignore this value as we have defined data sorting type as a derived value based on synchronization validity
                }
            }

            public override bool PmuError
            {
                get
                {
                    return false;
                }
                set
                {
                    // We just ignore this value as IEEE 1344 defines no flags for data errors
                }
            }

            public TriggerStatus TriggerStatus
            {
                get
                {
                    return (TriggerStatus)(StatusFlags & PhasorProtocols.Ieee1344.Common.TriggerMask);
                }
                set
                {
                    StatusFlags = (short)((StatusFlags & ~PhasorProtocols.Ieee1344.Common.TriggerMask) | (ushort)value);
                }
            }

            public override Dictionary<string, string> Attributes
            {
                get
                {
                    Dictionary<string, string> baseAttributes = base.Attributes;

                    baseAttributes.Add("Trigger Status", (int)TriggerStatus + ": " + Enum.GetName(typeof(TriggerStatus), TriggerStatus));

                    return baseAttributes;
                }
            }

        }

    }
}
