//*******************************************************************************************************
//  ConfigurationFrame.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  04/26/2009 - James R. Carroll
//       Generated original version of source code.
//  08/07/2009 - Josh Patterson
//      Edited Comments
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace TVA.PhasorProtocols.SelFastMessage
{
    /// <summary>
    /// Represents the SEL Fast Message implementation of a <see cref="IConfigurationFrame"/> that can be sent or received.
    /// </summary>
    [Serializable()]
    public class ConfigurationFrame : ConfigurationFrameBase
    {
        #region [ Members ]

        // Fields
        private FrameSize m_frameSize;
        private MessagePeriod m_messagePeriod;
        private uint m_idCode;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConfigurationFrame"/>.
        /// </summary>
        /// <remarks>
        /// This constructor is used by a consumer to generate a SEL Fast Message configuration frame.
        /// </remarks>
        /// <param name="frameSize">A <see cref="FrameSize"/> object.</param>
        /// <param name="idCode">An <see cref="UInt32"/> as the id code.</param>
        /// <param name="messagePeriod">A <see cref="MessagePeriod"/> object.</param>
        public ConfigurationFrame(FrameSize frameSize, MessagePeriod messagePeriod, uint idCode)
            : base(0, new ConfigurationCellCollection(), 0, 0)
        {
            m_frameSize = frameSize;
            m_messagePeriod = messagePeriod;
            IDCode = idCode;
            ConfigurationCell configCell = new ConfigurationCell(this);

            // Assign station name
            configCell.StationName = "SEL Unit - " + idCode;

            // Add a single frequency definition
            configCell.FrequencyDefinition = new FrequencyDefinition(configCell, "Line frequency");

            // Add phasors based on frame size
            switch (frameSize)
            {
                case FrameSize.V1:
                    // Add a single positive sequence voltage phasor definition
                    configCell.PhasorDefinitions.Add(new PhasorDefinition(configCell, "V1", PhasorType.Voltage, null));
                    break;
                case FrameSize.V:
                    // Add three-phase and positive sequence voltage phasors
                    configCell.PhasorDefinitions.Add(new PhasorDefinition(configCell, "VA", PhasorType.Voltage, null));
                    configCell.PhasorDefinitions.Add(new PhasorDefinition(configCell, "VB", PhasorType.Voltage, null));
                    configCell.PhasorDefinitions.Add(new PhasorDefinition(configCell, "VC", PhasorType.Voltage, null));
                    configCell.PhasorDefinitions.Add(new PhasorDefinition(configCell, "V1", PhasorType.Voltage, null));
                    break;
                case FrameSize.A:
                    // Add three-phase and positive sequence voltage and current phasors
                    configCell.PhasorDefinitions.Add(new PhasorDefinition(configCell, "VA", PhasorType.Voltage, null));
                    configCell.PhasorDefinitions.Add(new PhasorDefinition(configCell, "VB", PhasorType.Voltage, null));
                    configCell.PhasorDefinitions.Add(new PhasorDefinition(configCell, "VC", PhasorType.Voltage, null));
                    configCell.PhasorDefinitions.Add(new PhasorDefinition(configCell, "V1", PhasorType.Voltage, null));
                    configCell.PhasorDefinitions.Add(new PhasorDefinition(configCell, "IA", PhasorType.Current, configCell.PhasorDefinitions[0] as PhasorDefinition));
                    configCell.PhasorDefinitions.Add(new PhasorDefinition(configCell, "IB", PhasorType.Current, configCell.PhasorDefinitions[1] as PhasorDefinition));
                    configCell.PhasorDefinitions.Add(new PhasorDefinition(configCell, "IC", PhasorType.Current, configCell.PhasorDefinitions[2] as PhasorDefinition));
                    configCell.PhasorDefinitions.Add(new PhasorDefinition(configCell, "I1", PhasorType.Current, configCell.PhasorDefinitions[3] as PhasorDefinition));
                    break;
            }

            // SEL Fast Message protocol sends data for one device
            Cells.Add(configCell);

            // Define message rate (best-fit)
            switch (messagePeriod)
            {
                case MessagePeriod.DefaultRate:
                case MessagePeriod.TwentyPerSecond:
                    FrameRate = 20;
                    break;
                case MessagePeriod.TenPerSecond:
                    FrameRate = 10;
                    break;
                case MessagePeriod.FivePerSecond:
                    FrameRate = 5;
                    break;
                case MessagePeriod.FourPerSecond:
                    FrameRate = 4;
                    break;
                case MessagePeriod.TwoPerSecond:
                    FrameRate = 2;
                    break;
                case MessagePeriod.OnePerSecond:
                    FrameRate = 1;
                    break;
                default:
                    FrameRate = 0;
                    break;
            }
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationFrame"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ConfigurationFrame(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize configuration frame
            m_frameSize = (FrameSize)info.GetValue("frameSize", typeof(FrameSize));
            m_messagePeriod = (MessagePeriod)info.GetValue("messagePeriod", typeof(MessagePeriod));
            m_idCode = info.GetUInt32("idCode32Bit");
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets reference to the <see cref="ConfigurationCellCollection"/> for this <see cref="ConfigurationFrame"/>.
        /// </summary>
        public new ConfigurationCellCollection Cells
        {
            get
            {
                return base.Cells as ConfigurationCellCollection;
            }
        }

        /// <summary>
        /// Gets the SEL Fast Message frame size of this <see cref="ConfigurationFrame"/>.
        /// </summary>
        public FrameSize FrameSize
        {
            get
            {
                return m_frameSize;
            }
        }

        /// <summary>
        /// Gets defined <see cref="SelFastMessage.MessagePeriod"/> for SEL device.
        /// </summary>
        public MessagePeriod MessagePeriod
        {
            get
            {
                return m_messagePeriod;
            }
        }

        /// <summary>
        /// Gets or sets the ID code of this <see cref="ConfigurationFrame"/>.
        /// </summary>
        public new uint IDCode
        {
            get
            {
                return m_idCode;
            }
            set
            {
                m_idCode = value;

                // Base classes constrain maximum value to 65535
                base.IDCode = value > ushort.MaxValue ? ushort.MaxValue : (ushort)value;
            }
        }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="ConfigurationFrame"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Frame Size", (byte)FrameSize + ": " + FrameSize);
                baseAttributes.Add("Defined Message Period", (ushort)MessagePeriod + ": " + MessagePeriod);
                baseAttributes.Add("32-Bit ID Code", IDCode.ToString());

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Determines if checksum in the <paramref name="buffer"/> is valid.
        /// </summary>
        /// <param name="buffer">Buffer image to validate.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to perform checksum.</param>
        /// <returns>Flag that determines if checksum over <paramref name="buffer"/> is valid.</returns>
        /// <remarks>
        /// SEL Fast Message doesn't define a binary configuration frame - so no checksum is defined - this always returns true.
        /// </remarks>
        protected override bool ChecksumIsValid(byte[] buffer, int startIndex)
        {
            return true;
        }

        /// <summary>
        /// Method is not implemented.
        /// </summary>
        /// <exception cref="NotImplementedException">SEL Fast Message doesn't define a binary configuration frame - so no checksum is defined.</exception>
        /// <returns>An <see cref="UInt16"/> value for the checksum.</returns>
        /// <param name="buffer">A <see cref="Byte"/> buffer to read data from.</param>
        /// <param name="length">An <see cref="Int32"/> number of bytes to read.</param>
        /// <param name="offset">An <see cref="Int32"/> offset to read from.</param>
        protected override ushort CalculateChecksum(byte[] buffer, int offset, int length)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            
            // Serialize configuration frame
            info.AddValue("frameSize", m_frameSize, typeof(FrameSize));
            info.AddValue("messagePeriod", m_messagePeriod, typeof(MessagePeriod));
            info.AddValue("idCode32Bit", m_idCode);
        }

        #endregion
    }
}