//*******************************************************************************************************
//  PhasorDefinition.vb - PDCstream Phasor definition
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace PhasorProtocols.BpaPdcStream
{
    /// <summary>
    /// Represents the BPA PDCstream implementation of a <see cref="IPhasorDefinition"/>.
    /// </summary>
    [Serializable()]
    public class PhasorDefinition : PhasorDefinitionBase
    {
        #region [ Members ]

        // Fields
        private double m_ratio;
        private double m_calFactor;
        private double m_shunt;
        private int m_voltageReferenceIndex;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="PhasorDefinition"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="IConfigurationCell"/> parent of this <see cref="PhasorDefinition"/>.</param>
        public PhasorDefinition(IConfigurationCell parent)
            : base(parent)
        {
        }

        /// <summary>
        /// Creates a new <see cref="PhasorDefinition"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="ConfigurationCell"/> parent of this <see cref="PhasorDefinition"/>.</param>
        /// <param name="label">The label of this <see cref="PhasorDefinition"/>.</param>
        /// <param name="scale">The integer scaling value of this <see cref="PhasorDefinition"/>.</param>
        /// <param name="offset">The offset of this <see cref="PhasorDefinition"/>.</param>
        /// <param name="type">The <see cref="PhasorType"/> of this <see cref="PhasorDefinition"/>.</param>
        /// <param name="voltageReference">The associated <see cref="IPhasorDefinition"/> that represents the voltage reference (if any).</param>
        public PhasorDefinition(ConfigurationCell parent, string label, uint scale, double offset, PhasorType type, PhasorDefinition voltageReference)
            : base(parent, label, scale, offset, type, voltageReference)
        {
        }

        /// <summary>
        /// Creates a new <see cref="PhasorDefinition"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="ConfigurationCell"/> parent of this <see cref="PhasorDefinition"/>.</param>
        /// <param name="index">Index of phasor within INI based configuration file.</param>
        /// <param name="entryValue">The entry value from the INI based configuration file.</param>
        public PhasorDefinition(ConfigurationCell parent, int index, string entryValue)
            : base(parent)
        {
            string[] entry = entryValue.Split(',');
            string entryType = entry[0].Trim().Substring(0, 1).ToUpper();
            PhasorDefinition defaultPhasor;

            if (parent != null)
            {
                ConfigurationFrame configFile = this.Parent.Parent;

                if (entryType == "V")
                {
                    PhasorType = PhasorType.Voltage;
                    defaultPhasor = configFile.DefaultPhasorV;
                }
                else if (entryType == "I")
                {
                    PhasorType = PhasorType.Current;
                    defaultPhasor = configFile.DefaultPhasorI;
                }
                else
                {
                    PhasorType = PhasorType.Voltage;
                    defaultPhasor = configFile.DefaultPhasorV;
                }
            }
            else
            {
                defaultPhasor = new PhasorDefinition(null as ConfigurationCell);
            }

            if (entry.Length > 1)
                Ratio = double.Parse(entry[1].Trim());
            else
                Ratio = defaultPhasor.Ratio;

            if (entry.Length > 2)
                CalFactor = double.Parse(entry[2].Trim());
            else
                ConversionFactor = defaultPhasor.ConversionFactor;

            if (entry.Length > 3)
                Offset = double.Parse(entry[3].Trim());
            else
                Offset = defaultPhasor.Offset;

            if (entry.Length > 4)
                Shunt = double.Parse(entry[4].Trim());
            else
                Shunt = defaultPhasor.Shunt;

            if (entry.Length > 5)
                VoltageReferenceIndex = (int)double.Parse(entry[5].Trim());
            else
                VoltageReferenceIndex = defaultPhasor.VoltageReferenceIndex;

            if (entry.Length > 6)
                Label = entry[6].Trim();
            else
                Label = defaultPhasor.Label;

            this.Index = index;
        }

        /// <summary>
        /// Creates a new <see cref="PhasorDefinition"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected PhasorDefinition(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize phasor definition
            m_ratio = info.GetSingle("ratio");
            m_calFactor = info.GetSingle("calFactor");
            m_shunt = info.GetSingle("shunt");
            m_voltageReferenceIndex = info.GetInt32("voltageReferenceIndex");
        }


        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="ConfigurationCell"/> parent of this <see cref="PhasorDefinition"/>.
        /// </summary>
        public virtual new ConfigurationCell Parent
        {
            get
            {
                return base.Parent as ConfigurationCell;
            }
            set
            {
                base.Parent = value;
            }
        }

        /// <summary>
        /// Gets or sets the conversion factor of this <see cref="ChannelDefinitionBase"/>.
        /// </summary>
        /// <remarks>
        /// This method is hidden from the consumer since BPA PDCstream uses a custom conversion factor.
        /// Property always returns 1.0, updates are ignored.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override double ConversionFactor
        {
            // BPA PDCstream uses a custom conversion factor (see shared overload below)
            get
            {
                return 1.0D;
            }
            set
            {
                // Ignore updates...
            }
        }

        /// <summary>
        /// Gets or sets ratio of this <see cref="PhasorDefinition"/>.
        /// </summary>
        public double Ratio
        {
            get
            {
                return m_ratio;
            }
            set
            {
                m_ratio = value;
            }
        }

        /// <summary>
        /// Gets or sets calibration factor of this <see cref="PhasorDefinition"/>.
        /// </summary>
        public double CalFactor
        {
            get
            {
                return m_calFactor;
            }
            set
            {
                m_calFactor = value;
            }
        }

        /// <summary>
        /// Gets or sets shunt value of this <see cref="PhasorDefinition"/>.
        /// </summary>
        public double Shunt
        {
            get
            {
                return m_shunt;
            }
            set
            {
                m_shunt = value;
            }
        }

        /// <summary>
        /// Gets or sets voltage reference index of this <see cref="PhasorDefinition"/>.
        /// </summary>
        public int VoltageReferenceIndex
        {
            get
            {
                return m_voltageReferenceIndex;
            }
            set
            {
                m_voltageReferenceIndex = value;
            }
        }

        /// <summary>
        /// Gets the maximum length of the <see cref="ChannelDefinitionBase.Label"/> of this <see cref="PhasorDefinition"/>.
        /// </summary>
        public override int MaximumLabelLength
        {
            get
            {
                return 256;
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for this <see cref="PhasorDefinition"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Ratio", m_ratio.ToString());
                baseAttributes.Add("CalFactor", m_calFactor.ToString());
                baseAttributes.Add("Shunt", m_shunt.ToString());
                baseAttributes.Add("Voltage Reference Index", m_voltageReferenceIndex.ToString());

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize phasor definition
            info.AddValue("ratio", m_ratio);
            info.AddValue("calFactor", m_calFactor);
            info.AddValue("shunt", m_shunt);
            info.AddValue("voltageReferenceIndex", m_voltageReferenceIndex);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Calculates a BPA PDCstream phasor's custom conversion factor
        internal static double CustomConversionFactor(PhasorDefinition phasor)
        {
            if (phasor.PhasorType == PhasorType.Voltage)
                return phasor.CalFactor * phasor.Ratio;

            return phasor.CalFactor * phasor.Ratio / phasor.Shunt;
        }

        // Creates phasor information for an INI based BPA PDCstream configuration file
        internal static string ConfigFileFormat(IPhasorDefinition definition)
        {
            PhasorDefinition phasor = definition as PhasorDefinition;

            if (phasor != null)
                return (phasor.PhasorType == PhasorType.Voltage ? "V" : "I") + "," + phasor.Ratio + "," + phasor.CalFactor + "," + phasor.Offset + "," + phasor.Shunt + "," + phasor.VoltageReferenceIndex + "," + phasor.Label;

            return "";
        }

        // Delegate handler to create a new BPA PDCstream phasor definition
        internal static IPhasorDefinition CreateNewDefinition(IConfigurationCell parent, byte[] binaryImage, int startIndex, out int parsedLength)
        {
            IPhasorDefinition phasorDefinition = new PhasorDefinition(parent);

            parsedLength = phasorDefinition.Initialize(binaryImage, startIndex, 0);

            return phasorDefinition;
        }

        #endregion        
    }
}