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
//using PhasorProtocols.IeeeC37_118.Common;

//*******************************************************************************************************
//  DataCell.vb - IEEE C37.118 PMU Data Cell
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


namespace PCS.PhasorProtocols
{
    namespace IeeeC37_118
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
                : base(parent, false, configurationCell, Common.MaximumPhasorValues, Common.MaximumAnalogValues, Common.MaximumDigitalValues)
            {


                int x;

                // Initialize phasor values and frequency value with an empty value
                for (x = 0; x <= ConfigurationCell.PhasorDefinitions.Count - 1; x++)
                {
                    PhasorValues.Add(new PhasorValue(this, ConfigurationCell.PhasorDefinitions[x], float.NaN, float.NaN));
                }

                // Initialize frequency and df/dt
                FrequencyValue = new FrequencyValue(this, configurationCell.FrequencyDefinition, float.NaN, float.NaN);

                // Initialize analog values
                for (x = 0; x <= ConfigurationCell.AnalogDefinitions.Count - 1; x++)
                {
                    AnalogValues.Add(new AnalogValue(this, ConfigurationCell.AnalogDefinitions[x], float.NaN));
                }

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
                : base(parent, false, Common.MaximumPhasorValues, Common.MaximumAnalogValues, Common.MaximumDigitalValues, new DataCellParsingState(state.ConfigurationFrame.Cells[index], PhasorValue.CreateNewPhasorValue, IeeeC37_118.FrequencyValue.CreateNewFrequencyValue, AnalogValue.CreateNewAnalogValue, DigitalValue.CreateNewDigitalValue), binaryImage, startIndex)
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

            public new StatusFlags StatusFlags
            {
                get
                {
                    return (StatusFlags)base.StatusFlags & ~(StatusFlags.UnlockedTimeMask | StatusFlags.TriggerReasonMask);
                }
                set
                {
                    base.StatusFlags = (short)((base.StatusFlags & (short)(StatusFlags.UnlockedTimeMask | StatusFlags.TriggerReasonMask)) | (ushort)value);
                }
            }

            public UnlockedTime UnlockedTime
            {
                get
                {
                    return (UnlockedTime)(base.StatusFlags & (short)StatusFlags.UnlockedTimeMask);
                }
                set
                {
                    base.StatusFlags = (short)((base.StatusFlags & ~(short)StatusFlags.UnlockedTimeMask) | (ushort)value);
                    SynchronizationIsValid = (value == IeeeC37_118.UnlockedTime.SyncLocked);
                }
            }

            public TriggerReason TriggerReason
            {
                get
                {
                    return (TriggerReason)(base.StatusFlags & (short)StatusFlags.TriggerReasonMask);
                }
                set
                {
                    base.StatusFlags = (short)((base.StatusFlags & ~(short)StatusFlags.TriggerReasonMask) | (ushort)value);
                    PmuTriggerDetected = (value != IeeeC37_118.TriggerReason.Manual);
                }
            }

            public override bool DataIsValid
            {
                get
                {
                    return (StatusFlags & IeeeC37_118.StatusFlags.DataIsValid) == 0;
                }
                set
                {
                    if (value)
                    {
                        StatusFlags = StatusFlags & ~IeeeC37_118.StatusFlags.DataIsValid;
                    }
                    else
                    {
                        StatusFlags = StatusFlags | IeeeC37_118.StatusFlags.DataIsValid;
                    }
                }
            }

            public override bool SynchronizationIsValid
            {
                get
                {
                    return (StatusFlags & IeeeC37_118.StatusFlags.PmuSynchronizationError) == 0;
                }
                set
                {
                    if (value)
                    {
                        StatusFlags = StatusFlags & ~IeeeC37_118.StatusFlags.PmuSynchronizationError;
                    }
                    else
                    {
                        StatusFlags = StatusFlags | IeeeC37_118.StatusFlags.PmuSynchronizationError;
                    }
                }
            }

            public override bool PmuError
            {
                get
                {
                    return (StatusFlags & IeeeC37_118.StatusFlags.PmuError) > 0;
                }
                set
                {
                    if (value)
                    {
                        StatusFlags = StatusFlags | IeeeC37_118.StatusFlags.PmuError;
                    }
                    else
                    {
                        StatusFlags = StatusFlags & ~IeeeC37_118.StatusFlags.PmuError;
                    }
                }
            }

            public override DataSortingType DataSortingType
            {
                get
                {
                    return (((StatusFlags & IeeeC37_118.StatusFlags.DataSortingType) == 0) ? PhasorProtocols.DataSortingType.ByTimestamp : PhasorProtocols.DataSortingType.ByArrival);
                }
                set
                {
                    if (value == PhasorProtocols.DataSortingType.ByTimestamp)
                    {
                        StatusFlags = StatusFlags & ~IeeeC37_118.StatusFlags.DataSortingType;
                    }
                    else
                    {
                        StatusFlags = StatusFlags | IeeeC37_118.StatusFlags.DataSortingType;
                    }
                }
            }

            public bool PmuTriggerDetected
            {
                get
                {
                    return (StatusFlags & IeeeC37_118.StatusFlags.PmuTriggerDetected) > 0;
                }
                set
                {
                    if (value)
                    {
                        StatusFlags = StatusFlags | IeeeC37_118.StatusFlags.PmuTriggerDetected;
                    }
                    else
                    {
                        StatusFlags = StatusFlags & ~IeeeC37_118.StatusFlags.PmuTriggerDetected;
                    }
                }
            }

            public bool ConfigurationChangeDetected
            {
                get
                {
                    return (StatusFlags & IeeeC37_118.StatusFlags.ConfigurationChanged) > 0;
                }
                set
                {
                    if (value)
                    {
                        StatusFlags = StatusFlags | IeeeC37_118.StatusFlags.ConfigurationChanged;
                    }
                    else
                    {
                        StatusFlags = StatusFlags & ~IeeeC37_118.StatusFlags.ConfigurationChanged;
                    }
                }
            }

            public override Dictionary<string, string> Attributes
            {
                get
                {
                    Dictionary<string, string> baseAttributes = base.Attributes;

                    baseAttributes.Add("Unlocked Time", (int)UnlockedTime + ": " + UnlockedTime);
                    baseAttributes.Add("PMU Trigger Detected", PmuTriggerDetected.ToString());
                    baseAttributes.Add("Trigger Reason", (int)TriggerReason + ": " + TriggerReason);
                    baseAttributes.Add("Configuration Change Detected", ConfigurationChangeDetected.ToString());

                    return baseAttributes;
                }
            }

        }

    }
}
