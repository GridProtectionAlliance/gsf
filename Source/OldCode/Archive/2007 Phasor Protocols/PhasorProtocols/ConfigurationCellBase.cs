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
//using System.Buffer;
using System.Text;
//using PhasorProtocols.Common;
//using PCS.Text.Common;

//*******************************************************************************************************
//  ConfigurationCellBase.vb - Configuration cell base class
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
    /// <summary>This class represents the protocol independent common implementation of a set of configuration related data settings that can be sent or received from a PMU.</summary>
    [CLSCompliant(false), Serializable()]
    public abstract class ConfigurationCellBase : ChannelCellBase, IConfigurationCell
    {



        private string m_stationName;
        private string m_idLabel;
        private PhasorDefinitionCollection m_phasorDefinitions;
        private IFrequencyDefinition m_frequencyDefinition;
        private AnalogDefinitionCollection m_analogDefinitions;
        private DigitalDefinitionCollection m_digitalDefinitions;
        private LineFrequency m_nominalFrequency;
        private ushort m_revisionCount;

        protected ConfigurationCellBase()
        {
        }

        protected ConfigurationCellBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {


            // Deserialize configuration cell values
            this.StationName = info.GetString("stationName");
            this.IDLabel = info.GetString("idLabel");
            m_phasorDefinitions = (PhasorDefinitionCollection)info.GetValue("phasorDefinitions", typeof(PhasorDefinitionCollection));
            m_frequencyDefinition = (IFrequencyDefinition)info.GetValue("frequencyDefinition", typeof(IFrequencyDefinition));
            m_analogDefinitions = (AnalogDefinitionCollection)info.GetValue("analogDefinitions", typeof(AnalogDefinitionCollection));
            m_digitalDefinitions = (DigitalDefinitionCollection)info.GetValue("digitalDefinitions", typeof(DigitalDefinitionCollection));
            m_nominalFrequency = (LineFrequency)info.GetValue("nominalFrequency", typeof(LineFrequency));
            m_revisionCount = info.GetUInt16("revisionCount");

        }

        protected ConfigurationCellBase(IConfigurationFrame parent, bool alignOnDWordBoundary, int maximumPhasors, int maximumAnalogs, int maximumDigitals)
            : base(parent, alignOnDWordBoundary)
        {


            m_phasorDefinitions = new PhasorDefinitionCollection(maximumPhasors);
            m_analogDefinitions = new AnalogDefinitionCollection(maximumAnalogs);
            m_digitalDefinitions = new DigitalDefinitionCollection(maximumDigitals);

        }

        protected ConfigurationCellBase(IConfigurationFrame parent, bool alignOnDWordBoundary, ushort idCode, LineFrequency nominalFrequency, int maximumPhasors, int maximumAnalogs, int maximumDigitals)
            : this(parent, alignOnDWordBoundary, maximumPhasors, maximumAnalogs, maximumDigitals)
        {

            this.IDCode = idCode;
            m_nominalFrequency = nominalFrequency;

        }

        protected ConfigurationCellBase(IConfigurationFrame parent, bool alignOnDWordBoundary, int maximumPhasors, int maximumAnalogs, int maximumDigitals, IConfigurationCellParsingState state, byte[] binaryImage, int startIndex)
            : this(parent, alignOnDWordBoundary, maximumPhasors, maximumAnalogs, maximumDigitals)
        {

            ParseBinaryImage(state, binaryImage, startIndex);

        }

        protected ConfigurationCellBase(IConfigurationFrame parent, bool alignOnDWordBoundary, ushort idCode, LineFrequency nominalFrequency, string stationName, string idLabel, PhasorDefinitionCollection phasorDefinitions, IFrequencyDefinition frequencyDefinition, AnalogDefinitionCollection analogDefinitions, DigitalDefinitionCollection digitalDefinitions)
            : base(parent, alignOnDWordBoundary, idCode)
        {


            m_nominalFrequency = nominalFrequency;
            this.StationName = stationName;
            this.IDLabel = idLabel;
            m_phasorDefinitions = phasorDefinitions;
            m_frequencyDefinition = frequencyDefinition;
            m_analogDefinitions = analogDefinitions;
            m_digitalDefinitions = digitalDefinitions;

        }

        // Final dervived classes must expose Public Sub New(ByVal parent As IChannelFrame, ByVal state As IChannelFrameParsingState, ByVal index As int, ByVal binaryImage As Byte(), ByVal startIndex As int)

        // Derived classes are expected to expose a Public Sub New(ByVal configurationCell As IConfigurationCell)
        protected ConfigurationCellBase(IConfigurationCell configurationCell)
            : this(configurationCell.Parent, configurationCell.AlignOnDWordBoundary, configurationCell.IDCode, configurationCell.NominalFrequency, configurationCell.StationName, configurationCell.IDLabel, configurationCell.PhasorDefinitions, configurationCell.FrequencyDefinition, configurationCell.AnalogDefinitions, configurationCell.DigitalDefinitions)
        {


        }

        public virtual new IConfigurationFrame Parent
        {
            get
            {
                return (IConfigurationFrame)base.Parent;
            }
        }

        public virtual string StationName
        {
            get
            {
                return m_stationName;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = "undefined";
                }

                value = value.Trim();

                if (value.Length > MaximumStationNameLength)
                {
                    throw (new OverflowException("Station name length cannot exceed " + MaximumStationNameLength));
                }
                else
                {
                    m_stationName = PhasorProtocols.Common.GetValidLabel(value);
                }
            }
        }

        public virtual byte[] StationNameImage
        {
            get
            {
                return Encoding.ASCII.GetBytes(m_stationName.PadRight(MaximumStationNameLength));
            }
        }

        public virtual int MaximumStationNameLength
        {
            get
            {
                // Typical station name length is 16 characters
                return 16;
            }
        }

        public virtual string IDLabel
        {
            get
            {
                return m_idLabel;
            }
            set
            {
                if (value == null)
                {
                    value = "";
                }

                if (value.Trim().Length > IDLabelLength)
                {
                    throw (new OverflowException("ID label must not be more than " + IDLabelLength + " characters in length"));
                }
                else
                {
                    m_idLabel = PhasorProtocols.Common.GetValidLabel(value).Trim();
                }
            }
        }

        public virtual byte[] IDLabelImage
        {
            get
            {
                if (IDLabelLength < int.MaxValue)
                {
                    return Encoding.ASCII.GetBytes(m_idLabel.PadRight(IDLabelLength));
                }
                else
                {
                    return Encoding.ASCII.GetBytes(m_idLabel);
                }
            }
        }

        public virtual int IDLabelLength
        {
            get
            {
                // We don't restrict this for most protocols...
                return int.MaxValue;
            }
        }

        public virtual PhasorDefinitionCollection PhasorDefinitions
        {
            get
            {
                return m_phasorDefinitions;
            }
        }

        public abstract CoordinateFormat PhasorCoordinateFormat
        {
            get;
            set;
        }

        public abstract DataFormat PhasorDataFormat
        {
            get;
            set;
        }

        public virtual IFrequencyDefinition FrequencyDefinition
        {
            get
            {
                return m_frequencyDefinition;
            }
            set
            {
                m_frequencyDefinition = value;
            }
        }

        public abstract DataFormat FrequencyDataFormat
        {
            get;
            set;
        }

        public virtual AnalogDefinitionCollection AnalogDefinitions
        {
            get
            {
                return m_analogDefinitions;
            }
        }

        public abstract DataFormat AnalogDataFormat
        {
            get;
            set;
        }

        public virtual DigitalDefinitionCollection DigitalDefinitions
        {
            get
            {
                return m_digitalDefinitions;
            }
        }

        public virtual LineFrequency NominalFrequency
        {
            get
            {
                return m_nominalFrequency;
            }
            set
            {
                m_nominalFrequency = value;
            }
        }

        public virtual short FrameRate
        {
            get
            {
                return Parent.FrameRate;
            }
        }

        public virtual ushort RevisionCount
        {
            get
            {
                return m_revisionCount;
            }
            set
            {
                m_revisionCount = value;
            }
        }

        public virtual int CompareTo(IConfigurationCell other)
        {

            // We sort configuration cells by ID code...
            return IDCode.CompareTo(other.IDCode);

        }

        public virtual int CompareTo(object obj)
        {

            IConfigurationCell other = obj as IConfigurationCell;

            if (other == null)
            {
                throw (new ArgumentException("ConfigurationCell can only be compared to other ConfigurationCells"));
            }
            else
            {
                return CompareTo(other);
            }

        }

        // Only the station name is common to configuration frame headers in IEEE protocols
        protected override ushort HeaderLength
        {
            get
            {
                return (ushort)MaximumStationNameLength;
            }
        }

        protected override byte[] HeaderImage
        {
            get
            {
                return StationNameImage;
            }
        }

        protected override void ParseHeaderImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
        {

            int length = Array.IndexOf(binaryImage, (byte)0, startIndex, MaximumStationNameLength) - startIndex;

            if (length < 0)
            {
                length = MaximumStationNameLength;
            }

            StationName = Encoding.ASCII.GetString(binaryImage, startIndex, length);

        }

        // Channel names of IEEE C37.118 and IEEE 1344 configuration frames are common in order and type - so they are defined in the base class
        protected override ushort BodyLength
        {
            get
            {
                return (ushort)(m_phasorDefinitions.BinaryLength + m_analogDefinitions.BinaryLength + m_digitalDefinitions.BinaryLength);
            }
        }

        protected override byte[] BodyImage
        {
            get
            {
                byte[] buffer = new byte[BodyLength];
                int index = 0;

                // Copy in common cell images (channel names)
                PhasorProtocols.Common.CopyImage(m_phasorDefinitions, buffer, ref index);
                PhasorProtocols.Common.CopyImage(m_analogDefinitions, buffer, ref index);
                PhasorProtocols.Common.CopyImage(m_digitalDefinitions, buffer, ref index);

                return buffer;
            }
        }

        protected override void ParseBodyImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
        {

            IConfigurationCellParsingState parsingState = (IConfigurationCellParsingState)state;
            int x;

            // By the very nature of the IEEE protocols supporting the same order of phasor, analog and digital labels
            // we are able to "automatically" parse this data out in the configuration cell base class - BEAUTIFUL!!!

            // Parse out phasor definitions
            for (x = 0; x <= parsingState.PhasorCount - 1; x++)
            {
                m_phasorDefinitions.Add(parsingState.CreateNewPhasorDefinitionFunction(this, binaryImage, startIndex));
                startIndex += m_phasorDefinitions[x].MaximumLabelLength;
            }

            // Parse out analog definitions
            for (x = 0; x <= parsingState.AnalogCount - 1; x++)
            {
                m_analogDefinitions.Add(parsingState.CreateNewAnalogDefinitionFunction(this, binaryImage, startIndex));
                startIndex += m_analogDefinitions[x].MaximumLabelLength;
            }

            // Parse out digital definitions
            for (x = 0; x <= parsingState.DigitalCount - 1; x++)
            {
                m_digitalDefinitions.Add(parsingState.CreateNewDigitalDefinitionFunction(this, binaryImage, startIndex));
                startIndex += m_digitalDefinitions[x].MaximumLabelLength;
            }

        }

        // Footer for IEEE protocols contains nominal frequency definition, so we use this to initialize frequency definition
        protected override ushort FooterLength
        {
            get
            {
                return 2;
            }
        }

        protected override byte[] FooterImage
        {
            get
            {
                return m_frequencyDefinition.BinaryImage;
            }
        }

        protected override void ParseFooterImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
        {

            m_frequencyDefinition = ((IConfigurationCellParsingState)state).CreateNewFrequencyDefinitionFunction(this, binaryImage, startIndex);

        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {

            base.GetObjectData(info, context);

            // Serialize configuration cell values
            info.AddValue("stationName", StationName);
            info.AddValue("idLabel", IDLabel);
            info.AddValue("phasorDefinitions", m_phasorDefinitions, typeof(PhasorDefinitionCollection));
            info.AddValue("frequencyDefinition", m_frequencyDefinition, typeof(IFrequencyDefinition));
            info.AddValue("analogDefinitions", m_analogDefinitions, typeof(AnalogDefinitionCollection));
            info.AddValue("digitalDefinitions", m_digitalDefinitions, typeof(DigitalDefinitionCollection));
            info.AddValue("nominalFrequency", m_nominalFrequency, typeof(LineFrequency));
            info.AddValue("revisionCount", m_revisionCount);

        }

        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Station Name", StationName);
                baseAttributes.Add("ID Label", IDLabel);
                baseAttributes.Add("Phasor Coordinate Format", (int)PhasorCoordinateFormat + ": " + PhasorCoordinateFormat);
                baseAttributes.Add("Phasor Data Format", (int)PhasorDataFormat + ": " + PhasorDataFormat);
                baseAttributes.Add("Frequency Data Format", (int)FrequencyDataFormat + ": " + FrequencyDataFormat);
                baseAttributes.Add("Analog Data Format", (int)AnalogDataFormat + ": " + AnalogDataFormat);
                baseAttributes.Add("Total Phasor Definitions", PhasorDefinitions.Count.ToString());
                baseAttributes.Add("Total Analog Definitions", AnalogDefinitions.Count.ToString());
                baseAttributes.Add("Total Digital Definitions", DigitalDefinitions.Count.ToString());
                baseAttributes.Add("Nominal Frequency", NominalFrequency + " Hz");
                baseAttributes.Add("Revision Count", RevisionCount.ToString());
                baseAttributes.Add("Maximum Station Name Length", MaximumStationNameLength.ToString());
                baseAttributes.Add("ID Label Length", IDLabelLength.ToString());

                return baseAttributes;
            }
        }

    }
}
