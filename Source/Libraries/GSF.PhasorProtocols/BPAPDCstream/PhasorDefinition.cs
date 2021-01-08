//******************************************************************************************************
//  PhasorDefinition.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  11/12/2004 - J. Ritchie Carroll
//       Initial version of source generated.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using GSF.Units.EE;

// ReSharper disable VirtualMemberCallInConstructor
namespace GSF.PhasorProtocols.BPAPDCstream
{
    /// <summary>
    /// Represents the BPA PDCstream implementation of a <see cref="IPhasorDefinition"/>.
    /// </summary>
    [Serializable]
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

            if (parent is null)
            {
                defaultPhasor = new PhasorDefinition(null);
            }
            else
            {
                ConfigurationFrame configFile = Parent.Parent;

                switch (entryType)
                {
                    case "V":
                        PhasorType = PhasorType.Voltage;
                        defaultPhasor = configFile.DefaultPhasorV;
                        break;
                    case "I":
                        PhasorType = PhasorType.Current;
                        defaultPhasor = configFile.DefaultPhasorI;
                        break;
                    default:
                        PhasorType = PhasorType.Voltage;
                        defaultPhasor = configFile.DefaultPhasorV;
                        break;
                }
            }

            if (entry.Length > 1 && double.TryParse(entry[1].Trim(), out double dValue))
                Ratio = dValue;
            else
                Ratio = defaultPhasor.Ratio;

            if (entry.Length > 2 && double.TryParse(entry[2].Trim(), out dValue))
                CalFactor = dValue;
            else
                CalFactor = defaultPhasor.CalFactor;

            if (entry.Length > 3 && double.TryParse(entry[3].Trim(), out dValue))
                Offset = dValue;
            else
                Offset = defaultPhasor.Offset;

            if (entry.Length > 4 && double.TryParse(entry[4].Trim(), out dValue))
                Shunt = dValue;
            else
                Shunt = defaultPhasor.Shunt;

            if (entry.Length > 5 && double.TryParse(entry[5].Trim(), out dValue))
                VoltageReferenceIndex = (int)dValue;
            else
                VoltageReferenceIndex = defaultPhasor.VoltageReferenceIndex;

            Label = entry.Length > 6 ? entry[6].Trim() : defaultPhasor.Label;

            Index = index;
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
        public new virtual ConfigurationCell Parent
        {
            get => base.Parent as ConfigurationCell;
            set => base.Parent = value;
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
            get => 1.0D;
            set { } // Ignore updates...
        }

        /// <summary>
        /// Gets or sets ratio of this <see cref="PhasorDefinition"/>.
        /// </summary>
        public double Ratio
        {
            get => m_ratio;
            set => m_ratio = value;
        }

        /// <summary>
        /// Gets or sets calibration factor of this <see cref="PhasorDefinition"/>.
        /// </summary>
        public double CalFactor
        {
            get => m_calFactor;
            set => m_calFactor = value;
        }

        /// <summary>
        /// Gets or sets shunt value of this <see cref="PhasorDefinition"/>.
        /// </summary>
        public double Shunt
        {
            get => m_shunt;
            set => m_shunt = value;
        }

        /// <summary>
        /// Gets or sets voltage reference index of this <see cref="PhasorDefinition"/>.
        /// </summary>
        public int VoltageReferenceIndex
        {
            get => m_voltageReferenceIndex;
            set => m_voltageReferenceIndex = value;
        }

        /// <summary>
        /// Gets the maximum length of the <see cref="ChannelDefinitionBase.Label"/> of this <see cref="PhasorDefinition"/>.
        /// </summary>
        public override int MaximumLabelLength => 256;

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
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
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
            if (definition is PhasorDefinition phasor)
                return (phasor.PhasorType == PhasorType.Voltage ? "V" : "I") + "," + phasor.Ratio + "," + phasor.CalFactor + "," + phasor.Offset + "," + phasor.Shunt + "," + phasor.VoltageReferenceIndex + "," + phasor.Label;

            if (definition is null)
                return "";

            if (definition.PhasorType == PhasorType.Voltage)
                return "V,4500.0,0.0060573,0,0,500," + definition.Label.ToNonNullString("Default 500kV");
                
            return "I,600.00,0.000040382,0,1,1," + definition.Label.ToNonNullString("Default Current");

        }

        // Delegate handler to create a new BPA PDCstream phasor definition
        internal static IPhasorDefinition CreateNewDefinition(IConfigurationCell parent, byte[] buffer, int startIndex, out int parsedLength)
        {
            IPhasorDefinition phasorDefinition = new PhasorDefinition(parent);

            parsedLength = phasorDefinition.ParseBinaryImage(buffer, startIndex, 0);

            return phasorDefinition;
        }

        #endregion
    }
}