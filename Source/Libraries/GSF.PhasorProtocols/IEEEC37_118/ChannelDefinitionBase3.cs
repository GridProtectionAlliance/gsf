//******************************************************************************************************
//  ChannelDefinitionBase3.cs - Gbtc
//
//  Copyright © 2021, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  12/27/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

// ReSharper disable NonReadonlyMemberInGetHashCode
// ReSharper disable VirtualMemberCallInConstructor
namespace GSF.PhasorProtocols.IEEEC37_118
{
    /// <summary>
    /// Represents a base implementation for the IEEE C37.118 configuration 3 frame channel definition used by
    /// PhasorDefinition3, AnalogDefinition3 and DigitalDefinition3 for the purpose of parsing length-prefixed
    /// variable length channel names.
    /// </summary>
    [Serializable]
    public abstract class ChannelDefinitionBase3 : ChannelBase, IChannelDefinition
    {
        #region [ Members ]

        // Fields
        private string m_label;
        private byte[] m_labelImage;
        private uint m_scale;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ChannelDefinitionBase3"/>.
        /// </summary>
        /// <param name="parent">The <see cref="IConfigurationCell"/> parent of this <see cref="ChannelDefinitionBase3"/>.</param>
        protected ChannelDefinitionBase3(IConfigurationCell parent)
        {
            Parent = parent;
        }

        /// <summary>
        /// Creates a new <see cref="ChannelDefinitionBase3"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="IConfigurationCell"/> parent of this <see cref="ChannelDefinitionBase3"/>.</param>
        /// <param name="label">The label of this <see cref="ChannelDefinitionBase3"/>.</param>
        /// <param name="scale">The integer scaling value of this <see cref="ChannelDefinitionBase3"/>.</param>
        /// <param name="offset">The offset of this <see cref="ChannelDefinitionBase3"/>.</param>
        /// <param name="index">This index of this <see cref="ChannelDefinitionBase3"/>, if applicable.</param>
        protected ChannelDefinitionBase3(IConfigurationCell parent, string label, uint scale, double offset, int index = 0)
        {
            Parent = parent;
            Label = label;
            m_scale = scale;
            Offset = offset;
            Index = index;
        }

        /// <summary>
        /// Creates a new <see cref="ChannelDefinitionBase3"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ChannelDefinitionBase3(SerializationInfo info, StreamingContext context)
        {
            // Deserialize channel definition
            Parent = (IConfigurationCell)info.GetValue("parent", typeof(IConfigurationCell));
            Index = info.GetInt32("index");
            Label = info.GetString("label");
            m_scale = info.GetUInt32("scale");
            Offset = info.GetDouble("offset");
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="IConfigurationCell"/> parent of this <see cref="ChannelDefinitionBase3"/>.
        /// </summary>
        public virtual IConfigurationCell Parent { get; set; }

        /// <summary>
        /// Gets the <see cref="DataFormat"/> of this <see cref="ChannelDefinitionBase3"/>.
        /// </summary>
        public abstract DataFormat DataFormat { get; }

        /// <summary>
        /// Gets or sets the index of this <see cref="ChannelDefinitionBase3"/>.
        /// </summary>
        /// <remarks>
        /// Index is automatically maintained by <see cref="ChannelDefinitionCollectionBase{T}"/>.
        /// </remarks>
        public virtual int Index { get; set; }

        /// <summary>
        /// Gets or sets the offset of this <see cref="ChannelDefinitionBase3"/>.
        /// </summary>
        public virtual double Offset { get; set; }

        /// <summary>
        /// Gets or sets the conversion factor of this <see cref="ChannelDefinitionBase3"/>.
        /// </summary>
        public virtual double ConversionFactor
        {
            get => ScalingValue * ScalePerBit;
            set
            {
                unchecked
                {
                    ScalingValue = (uint)(value / ScalePerBit);
                }
            }
        }

        /// <summary>
        /// Gets the scale/bit for the <see cref="ScalingValue"/> of this <see cref="ChannelDefinitionBase3"/>.
        /// </summary>
        /// <remarks>
        /// The base implementation assumes scale/bit of 10^-5.
        /// </remarks>
        public virtual double ScalePerBit => 0.00001D; // Typical scale/bit is 10^-5

        /// <summary>
        /// Gets or sets the integer scaling value of this <see cref="ChannelDefinitionBase3"/>.
        /// </summary>
        public virtual uint ScalingValue
        {
            get => m_scale;
            set
            {
                if (value > MaximumScalingValue)
                    throw new OverflowException($"Scaling value cannot exceed {MaximumScalingValue}");

                m_scale = value;
            }
        }

        /// <summary>
        /// Gets the maximum value for the <see cref="ScalingValue"/> of this <see cref="ChannelDefinitionBase3"/>.
        /// </summary>
        /// <remarks>
        /// The base implementation accommodates maximum scaling factor of 24-bits of space.
        /// </remarks>
        public virtual uint MaximumScalingValue => UInt24.MaxValue; // Typical scaling/conversion factors should fit within 3 bytes (i.e., 24 bits) of space

        /// <summary>
        /// Gets or sets the label of this <see cref="ChannelDefinitionBase3"/>.
        /// </summary>
        public virtual string Label
        {
            get => m_label;
            set
            {
                if (string.IsNullOrEmpty(value))
                    value = "undefined";

                value = value.GetValidLabel();

                if (value.Length > MaximumLabelLength)
                    throw new OverflowException($"Label length cannot exceed {MaximumLabelLength} characters.");

                m_label = value;
                m_labelImage = ConfigurationCell3.EncodeLengthPrefixedString(value);
            }
        }

        /// <summary>
        /// Gets the binary image of the <see cref="Label"/> of this <see cref="ChannelDefinitionBase3"/>.
        /// </summary>
        public virtual byte[] LabelImage => m_labelImage;

        /// <summary>
        /// Gets the maximum length of the <see cref="Label"/> of this <see cref="ChannelDefinitionBase3"/>.
        /// </summary>
        public virtual int MaximumLabelLength => byte.MaxValue; // Typical label length is 16 characters

        /// <summary>
        /// Gets the length of the <see cref="BodyImage"/>.
        /// </summary>
        protected override int BodyLength => LabelImage.Length;

        /// <summary>
        /// Gets the binary body image of the <see cref="ChannelDefinitionBase3"/> object.
        /// </summary>
        protected override byte[] BodyImage => LabelImage;

        /// <summary>
        /// Gets a <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for this <see cref="ChannelDefinitionBase3"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Label", Label);
                baseAttributes.Add("Index", Index.ToString());
                baseAttributes.Add("Offset", Offset.ToString());
                baseAttributes.Add("Data Format", $"{(int)DataFormat}: {DataFormat}");
                baseAttributes.Add("Conversion Factor", ConversionFactor.ToString());
                baseAttributes.Add("Scaling Value", ScalingValue.ToString());
                baseAttributes.Add("Scale per Bit", ScalePerBit.ToString());
                baseAttributes.Add("Maximum Scaling Value", MaximumScalingValue.ToString());
                baseAttributes.Add("Maximum Label Length", MaximumLabelLength.ToString());

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses the binary body image.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="buffer"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        /// <remarks>
        /// The base implementation assumes that all channel definitions begin with a label as this is
        /// the general case, override functionality if this is not the case.
        /// </remarks>
        protected override int ParseBodyImage(byte[] buffer, int startIndex, int length)
        {
            // Length is validated at a frame level well in advance so that low level parsing routines do not have
            // to re-validate that enough length is available to parse needed information as an optimization...
            int index = startIndex;

            Label = ConfigurationCell3.DecodeLengthPrefixedString(buffer, ref index);
                
            return index - startIndex;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">An object to compare, or null.</param>
        /// <returns>
        /// True if obj is an instance of <see cref="IChannelDefinition"/> and equals the value of this instance;
        /// otherwise, False.
        /// </returns>
        /// <exception cref="ArgumentException">value is not an <see cref="IChannelDefinition"/>.</exception>
        public override bool Equals(object obj) => 
            Equals(obj as IChannelDefinition);

        /// <summary>
        /// Returns a value indicating whether this instance is equal to specified <see cref="IChannelDefinition"/> value.
        /// </summary>
        /// <param name="other">A <see cref="IChannelDefinition"/> object to compare to this instance.</param>
        /// <returns>
        /// True if <paramref name="other"/> has the same value as this instance; otherwise, False.
        /// </returns>
        public bool Equals(IChannelDefinition other) => 
            CompareTo(other) == 0;

        /// <summary>
        /// Compares this instance to a specified object and returns an indication of their relative values.
        /// </summary>
        /// <param name="obj">An object to compare, or null.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value. Returns less than zero
        /// if this instance is less than value, zero if this instance is equal to value, or greater than zero
        /// if this instance is greater than value.
        /// </returns>
        /// <exception cref="ArgumentException">value is not an <see cref="IChannelDefinition"/>.</exception>
        public virtual int CompareTo(object obj)
        {
            if (obj is IChannelDefinition other)
                return CompareTo(other);

            throw new ArgumentException("ChannelDefinition can only be compared to other IChannelDefinitions");
        }

        /// <summary>
        /// Compares this instance to a specified <see cref="IChannelDefinition"/> object and returns an indication of their
        /// relative values.
        /// </summary>
        /// <param name="other">A <see cref="IChannelDefinition"/> object to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value. Returns less than zero
        /// if this instance is less than value, zero if this instance is equal to value, or greater than zero
        /// if this instance is greater than value.
        /// </returns>
        public int CompareTo(IChannelDefinition other) => 
            Index.CompareTo(other.Index); // We sort channel definitions by index

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode() => 
            Index.GetHashCode();

        /// <summary>
        /// Gets the string representation of this <see cref="ChannelDefinitionBase3"/>.
        /// </summary>
        /// <returns>String representation of this <see cref="ChannelDefinitionBase3"/>.</returns>
        public override string ToString() => 
            Label;

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Serialize channel definition
            info.AddValue("parent", Parent, typeof(IConfigurationCell));
            info.AddValue("index", Index);
            info.AddValue("label", Label);
            info.AddValue("scale", m_scale);
            info.AddValue("offset", Offset);
        }

        #endregion
    }
}