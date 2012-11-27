//*******************************************************************************************************
//  ConfigurationCell2.cs - Gbtc
//
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  09/08/2011 - Andrew Krohne
//       Generated original version of source code.
//*******************************************************************************************************

#region [ TVA Open Source Agreement ]
/*

 THIS OPEN SOURCE AGREEMENT ("AGREEMENT") DEFINES THE RIGHTS OF USE,REPRODUCTION, DISTRIBUTION,
 MODIFICATION AND REDISTRIBUTION OF CERTAIN COMPUTER SOFTWARE ORIGINALLY RELEASED BY THE
 TENNESSEE VALLEY AUTHORITY, A CORPORATE AGENCY AND INSTRUMENTALITY OF THE UNITED STATES GOVERNMENT
 ("GOVERNMENT AGENCY"). GOVERNMENT AGENCY IS AN INTENDED THIRD-PARTY BENEFICIARY OF ALL SUBSEQUENT
 DISTRIBUTIONS OR REDISTRIBUTIONS OF THE SUBJECT SOFTWARE. ANYONE WHO USES, REPRODUCES, DISTRIBUTES,
 MODIFIES OR REDISTRIBUTES THE SUBJECT SOFTWARE, AS DEFINED HEREIN, OR ANY PART THEREOF, IS, BY THAT
 ACTION, ACCEPTING IN FULL THE RESPONSIBILITIES AND OBLIGATIONS CONTAINED IN THIS AGREEMENT.

 Original Software Designation: openPDC
 Original Software Title: The TVA Open Source Phasor Data Concentrator
 User Registration Requested. Please Visit https://naspi.tva.com/Registration/
 Point of Contact for Original Software: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>

 1. DEFINITIONS

 A. "Contributor" means Government Agency, as the developer of the Original Software, and any entity
 that makes a Modification.

 B. "Covered Patents" mean patent claims licensable by a Contributor that are necessarily infringed by
 the use or sale of its Modification alone or when combined with the Subject Software.

 C. "Display" means the showing of a copy of the Subject Software, either directly or by means of an
 image, or any other device.

 D. "Distribution" means conveyance or transfer of the Subject Software, regardless of means, to
 another.

 E. "Larger Work" means computer software that combines Subject Software, or portions thereof, with
 software separate from the Subject Software that is not governed by the terms of this Agreement.

 F. "Modification" means any alteration of, including addition to or deletion from, the substance or
 structure of either the Original Software or Subject Software, and includes derivative works, as that
 term is defined in the Copyright Statute, 17 USC § 101. However, the act of including Subject Software
 as part of a Larger Work does not in and of itself constitute a Modification.

 G. "Original Software" means the computer software first released under this Agreement by Government
 Agency entitled openPDC, including source code, object code and accompanying documentation, if any.

 H. "Recipient" means anyone who acquires the Subject Software under this Agreement, including all
 Contributors.

 I. "Redistribution" means Distribution of the Subject Software after a Modification has been made.

 J. "Reproduction" means the making of a counterpart, image or copy of the Subject Software.

 K. "Sale" means the exchange of the Subject Software for money or equivalent value.

 L. "Subject Software" means the Original Software, Modifications, or any respective parts thereof.

 M. "Use" means the application or employment of the Subject Software for any purpose.

 2. GRANT OF RIGHTS

 A. Under Non-Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor,
 with respect to its own contribution to the Subject Software, hereby grants to each Recipient a
 non-exclusive, world-wide, royalty-free license to engage in the following activities pertaining to
 the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Modification

 5. Redistribution

 6. Display

 B. Under Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor, with
 respect to its own contribution to the Subject Software, hereby grants to each Recipient under Covered
 Patents a non-exclusive, world-wide, royalty-free license to engage in the following activities
 pertaining to the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Sale

 5. Offer for Sale

 C. The rights granted under Paragraph B. also apply to the combination of a Contributor's Modification
 and the Subject Software if, at the time the Modification is added by the Contributor, the addition of
 such Modification causes the combination to be covered by the Covered Patents. It does not apply to
 any other combinations that include a Modification. 

 D. The rights granted in Paragraphs A. and B. allow the Recipient to sublicense those same rights.
 Such sublicense must be under the same terms and conditions of this Agreement.

 3. OBLIGATIONS OF RECIPIENT

 A. Distribution or Redistribution of the Subject Software must be made under this Agreement except for
 additions covered under paragraph 3H. 

 1. Whenever a Recipient distributes or redistributes the Subject Software, a copy of this Agreement
 must be included with each copy of the Subject Software; and

 2. If Recipient distributes or redistributes the Subject Software in any form other than source code,
 Recipient must also make the source code freely available, and must provide with each copy of the
 Subject Software information on how to obtain the source code in a reasonable manner on or through a
 medium customarily used for software exchange.

 B. Each Recipient must ensure that the following copyright notice appears prominently in the Subject
 Software:

          No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.

 C. Each Contributor must characterize its alteration of the Subject Software as a Modification and
 must identify itself as the originator of its Modification in a manner that reasonably allows
 subsequent Recipients to identify the originator of the Modification. In fulfillment of these
 requirements, Contributor must include a file (e.g., a change log file) that describes the alterations
 made and the date of the alterations, identifies Contributor as originator of the alterations, and
 consents to characterization of the alterations as a Modification, for example, by including a
 statement that the Modification is derived, directly or indirectly, from Original Software provided by
 Government Agency. Once consent is granted, it may not thereafter be revoked.

 D. A Contributor may add its own copyright notice to the Subject Software. Once a copyright notice has
 been added to the Subject Software, a Recipient may not remove it without the express permission of
 the Contributor who added the notice.

 E. A Recipient may not make any representation in the Subject Software or in any promotional,
 advertising or other material that may be construed as an endorsement by Government Agency or by any
 prior Recipient of any product or service provided by Recipient, or that may seek to obtain commercial
 advantage by the fact of Government Agency's or a prior Recipient's participation in this Agreement.

 F. In an effort to track usage and maintain accurate records of the Subject Software, each Recipient,
 upon receipt of the Subject Software, is requested to register with Government Agency by visiting the
 following website: https://naspi.tva.com/Registration/. Recipient's name and personal information
 shall be used for statistical purposes only. Once a Recipient makes a Modification available, it is
 requested that the Recipient inform Government Agency at the web site provided above how to access the
 Modification.

 G. Each Contributor represents that that its Modification does not violate any existing agreements,
 regulations, statutes or rules, and further that Contributor has sufficient rights to grant the rights
 conveyed by this Agreement.

 H. A Recipient may choose to offer, and to charge a fee for, warranty, support, indemnity and/or
 liability obligations to one or more other Recipients of the Subject Software. A Recipient may do so,
 however, only on its own behalf and not on behalf of Government Agency or any other Recipient. Such a
 Recipient must make it absolutely clear that any such warranty, support, indemnity and/or liability
 obligation is offered by that Recipient alone. Further, such Recipient agrees to indemnify Government
 Agency and every other Recipient for any liability incurred by them as a result of warranty, support,
 indemnity and/or liability offered by such Recipient.

 I. A Recipient may create a Larger Work by combining Subject Software with separate software not
 governed by the terms of this agreement and distribute the Larger Work as a single product. In such
 case, the Recipient must make sure Subject Software, or portions thereof, included in the Larger Work
 is subject to this Agreement.

 J. Notwithstanding any provisions contained herein, Recipient is hereby put on notice that export of
 any goods or technical data from the United States may require some form of export license from the
 U.S. Government. Failure to obtain necessary export licenses may result in criminal liability under
 U.S. laws. Government Agency neither represents that a license shall not be required nor that, if
 required, it shall be issued. Nothing granted herein provides any such export license.

 4. DISCLAIMER OF WARRANTIES AND LIABILITIES; WAIVER AND INDEMNIFICATION

 A. No Warranty: THE SUBJECT SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTY OF ANY KIND, EITHER
 EXPRESSED, IMPLIED, OR STATUTORY, INCLUDING, BUT NOT LIMITED TO, ANY WARRANTY THAT THE SUBJECT
 SOFTWARE WILL CONFORM TO SPECIFICATIONS, ANY IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 PARTICULAR PURPOSE, OR FREEDOM FROM INFRINGEMENT, ANY WARRANTY THAT THE SUBJECT SOFTWARE WILL BE ERROR
 FREE, OR ANY WARRANTY THAT DOCUMENTATION, IF PROVIDED, WILL CONFORM TO THE SUBJECT SOFTWARE. THIS
 AGREEMENT DOES NOT, IN ANY MANNER, CONSTITUTE AN ENDORSEMENT BY GOVERNMENT AGENCY OR ANY PRIOR
 RECIPIENT OF ANY RESULTS, RESULTING DESIGNS, HARDWARE, SOFTWARE PRODUCTS OR ANY OTHER APPLICATIONS
 RESULTING FROM USE OF THE SUBJECT SOFTWARE. FURTHER, GOVERNMENT AGENCY DISCLAIMS ALL WARRANTIES AND
 LIABILITIES REGARDING THIRD-PARTY SOFTWARE, IF PRESENT IN THE ORIGINAL SOFTWARE, AND DISTRIBUTES IT
 "AS IS."

 B. Waiver and Indemnity: RECIPIENT AGREES TO WAIVE ANY AND ALL CLAIMS AGAINST GOVERNMENT AGENCY, ITS
 AGENTS, EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT. IF RECIPIENT'S USE
 OF THE SUBJECT SOFTWARE RESULTS IN ANY LIABILITIES, DEMANDS, DAMAGES, EXPENSES OR LOSSES ARISING FROM
 SUCH USE, INCLUDING ANY DAMAGES FROM PRODUCTS BASED ON, OR RESULTING FROM, RECIPIENT'S USE OF THE
 SUBJECT SOFTWARE, RECIPIENT SHALL INDEMNIFY AND HOLD HARMLESS  GOVERNMENT AGENCY, ITS AGENTS,
 EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT, TO THE EXTENT PERMITTED BY
 LAW.  THE FOREGOING RELEASE AND INDEMNIFICATION SHALL APPLY EVEN IF THE LIABILITIES, DEMANDS, DAMAGES,
 EXPENSES OR LOSSES ARE CAUSED, OCCASIONED, OR CONTRIBUTED TO BY THE NEGLIGENCE, SOLE OR CONCURRENT, OF
 GOVERNMENT AGENCY OR ANY PRIOR RECIPIENT.  RECIPIENT'S SOLE REMEDY FOR ANY SUCH MATTER SHALL BE THE
 IMMEDIATE, UNILATERAL TERMINATION OF THIS AGREEMENT.

 5. GENERAL TERMS

 A. Termination: This Agreement and the rights granted hereunder will terminate automatically if a
 Recipient fails to comply with these terms and conditions, and fails to cure such noncompliance within
 thirty (30) days of becoming aware of such noncompliance. Upon termination, a Recipient agrees to
 immediately cease use and distribution of the Subject Software. All sublicenses to the Subject
 Software properly granted by the breaching Recipient shall survive any such termination of this
 Agreement.

 B. Severability: If any provision of this Agreement is invalid or unenforceable under applicable law,
 it shall not affect the validity or enforceability of the remainder of the terms of this Agreement.

 C. Applicable Law: This Agreement shall be subject to United States federal law only for all purposes,
 including, but not limited to, determining the validity of this Agreement, the meaning of its
 provisions and the rights, obligations and remedies of the parties.

 D. Entire Understanding: This Agreement constitutes the entire understanding and agreement of the
 parties relating to release of the Subject Software and may not be superseded, modified or amended
 except by further written agreement duly executed by the parties.

 E. Binding Authority: By accepting and using the Subject Software under this Agreement, a Recipient
 affirms its authority to bind the Recipient to all terms and conditions of this Agreement and that
 Recipient hereby agrees to all terms and conditions herein.

 F. Point of Contact: Any Recipient contact with Government Agency is to be directed to the designated
 representative as follows: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>.

*/
#endregion

using System;

namespace GSF.PhasorProtocols.IeeeC37_118
{
    /// <summary>
    /// Represents the IEEE C37.118 implementation of a <see cref="IConfigurationCell"/> that can be sent or received.
    /// </summary>
    [Serializable()]
    public class ConfigurationCell3
    {
        #region [ Members ]

        // Fields
        //private FormatFlags m_formatFlags;

        #endregion

        #region [ Constructors ]
        /*
        /// <summary>
        /// Creates a new <see cref="ConfigurationCell3"/>.
        /// </summary>
        /// <param name="parent">The reference to parent <see cref="IConfigurationFrame"/> of this <see cref="ConfigurationCell3"/>.</param>
        public ConfigurationCell3(IConfigurationFrame parent) : base(parent, 0, Common.MaximumPhasorValues, Common.MaximumAnalogValues, Common.MaximumDigitalValues)
        {
            
            // Define new parsing state which defines constructors for key configuration values
            State = new ConfigurationCellParsingState(
                IeeeC37_118.PhasorDefinition.CreateNewDefinition,
                IeeeC37_118.FrequencyDefinition.CreateNewDefinition,
                IeeeC37_118.AnalogDefinition.CreateNewDefinition,
                IeeeC37_118.DigitalDefinition.CreateNewDefinition);
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationCell3"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The reference to parent <see cref="ConfigurationFrame1"/> of this <see cref="ConfigurationCell3"/>.</param>
        /// <param name="idCode">The numeric ID code for this <see cref="ConfigurationCell3"/>.</param>
        /// <param name="nominalFrequency">The nominal <see cref="LineFrequency"/> of the <see cref="FrequencyDefinition"/> of this <see cref="ConfigurationCell3"/>.</param>
        public ConfigurationCell3(ConfigurationFrame1 parent, ushort idCode, LineFrequency nominalFrequency) //FIXME: Does this need to use config frame 3?
            : this(parent)
        {
            IDCode = idCode;
            NominalFrequency = nominalFrequency;
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationCell3"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ConfigurationCell3(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize configuration cell
            m_formatFlags = (FormatFlags)info.GetValue("formatFlags", typeof(FormatFlags));
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets a reference to the parent <see cref="ConfigurationFrame1"/> for this <see cref="ConfigurationCell"/>.
        /// </summary>
        public new ConfigurationFrame1 Parent
        {
            get
            {
                return base.Parent as ConfigurationFrame1;
            }
            set
            {
                base.Parent = value;
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="HeaderImage"/>.
        /// </summary>
        protected override int HeaderLength
        {
            get
            {
                return base.HeaderLength + 10;
            }
        }

        /// <summary>
        /// Gets the binary header image of the <see cref="ConfigurationCell"/> object.
        /// </summary>
        protected override byte[] HeaderImage
        {
            get
            {
                byte[] buffer = new byte[HeaderLength];
                int index = 0;

                base.HeaderImage.CopyImage(buffer, ref index, base.HeaderLength);
                EndianOrder.BigEndian.CopyBytes(IDCode, buffer, index);
                EndianOrder.BigEndian.CopyBytes(StationName.Length, buffer, index);
                EndianOrder.BigEndian.CopyBytes(StationName, buffer, index);
                EndianOrder.BigEndian.CopyBytes(State.G_PMU_ID, buffer, index);
                EndianOrder.BigEndian.CopyBytes((ushort)m_formatFlags, buffer, index + 2);
                EndianOrder.BigEndian.CopyBytes((ushort)PhasorDefinitions.Count, buffer, index + 4);
                EndianOrder.BigEndian.CopyBytes((ushort)AnalogDefinitions.Count, buffer, index + 6);
                EndianOrder.BigEndian.CopyBytes((ushort)DigitalDefinitions.Count, buffer, index + 8);


                return buffer;
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="FooterImage"/>.
        /// </summary>
        protected override int FooterLength
        {
            get
            {
                return base.FooterLength + PhasorDefinitions.Count * PhasorDefinition.ConversionFactorLength + AnalogDefinitions.Count * AnalogDefinition.ConversionFactorLength + DigitalDefinitions.Count * DigitalDefinition.ConversionFactorLength + (Parent.DraftRevision > DraftRevision.Draft6 ? 2 : 0);
            }
        }

        /// <summary>
        /// Gets the binary footer image of the <see cref="ConfigurationCell"/> object.
        /// </summary>
        protected override byte[] FooterImage
        {
            get
            {
                byte[] buffer = new byte[FooterLength];
                PhasorDefinition phasorDefinition;
                AnalogDefinition analogDefinition;
                DigitalDefinition digitalDefinition;
                int x, index = 0;

                // Include conversion factors in configuration cell footer
                for (x = 0; x < PhasorDefinitions.Count; x++)
                {
                    phasorDefinition = PhasorDefinitions[x] as PhasorDefinition;

                    if (phasorDefinition != null)
                        phasorDefinition.ConversionFactorImage.CopyImage(buffer, ref index, PhasorDefinition.ConversionFactorLength);
                }

                for (x = 0; x < AnalogDefinitions.Count; x++)
                {
                    analogDefinition = AnalogDefinitions[x] as AnalogDefinition;

                    if (analogDefinition != null)
                        analogDefinition.ConversionFactorImage.CopyImage(buffer, ref index, AnalogDefinition.ConversionFactorLength);
                }

                for (x = 0; x < DigitalDefinitions.Count; x++)
                {
                    digitalDefinition = DigitalDefinitions[x] as DigitalDefinition;

                    if (digitalDefinition != null)
                        digitalDefinition.ConversionFactorImage.CopyImage(buffer, ref index, DigitalDefinition.ConversionFactorLength);
                }

                // Include nominal frequency
                base.FooterImage.CopyImage(buffer, ref index, base.FooterLength);

                // Include configuration count (new for version 7.0)
                if (Parent.DraftRevision > DraftRevision.Draft6)
                    EndianOrder.BigEndian.CopyBytes(RevisionCount, buffer, index);

                return buffer;
            }
        }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="ConfigurationCell"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Format Flags", (int)m_formatFlags + ": " + m_formatFlags);

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        string getLengthPrependedString(byte[] buffer, int index, out int length)
        {
            length = EndianOrder.BigEndian.ToInt16(buffer, index);
            return ByteEncoding.ASCII.GetString(buffer, index + 2, length);
        }

        /// <summary>
        /// Parses the binary header image.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="buffer"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        protected override int ParseHeaderImage(byte[] buffer, int startIndex, int length)
        {
            IConfigurationCellParsingState state = State;
            int index = startIndex;

            // Parse out station name
            //index += base.ParseHeaderImage(buffer, startIndex, length); //This is wrong. Should call ConfigCellBase
            String StationName;
            int len;
            StationName = getLengthPrependedString(buffer, index, out len);
            index += len + 2;
            IDCode = EndianOrder.BigEndian.ToUInt16(buffer, index);
            State.G_PMU_ID = EndianOrder.BigEndian.ToGuid(buffer, index + 2); 
            m_formatFlags = (FormatFlags)EndianOrder.BigEndian.ToUInt16(buffer, index + 16 + 2); //left as 16+x for clarity while editing, FIXME
            // Parse out total phasors, analogs and digitals defined for this device
            State.PhasorCount = EndianOrder.BigEndian.ToUInt16(buffer, index + 16+4); 
            State.AnalogCount = EndianOrder.BigEndian.ToUInt16(buffer, index + 16+6);
            State.DigitalCount = EndianOrder.BigEndian.ToUInt16(buffer, index + 16+8);
            

            index += 10+16; //FIXME: merge

            return (index - startIndex);
        }

        /// <summary>
        /// Parses the binary body image.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="buffer"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        protected override int ParseBodyImage(byte[] buffer, int startIndex, int length)
        {
            //FIXME: magic goes here
            int index = startIndex;
            int strLength;
            int x;
            //CHNAM, length-prepended string
            for (x = 0; x < State.PhasorCount; x++)
            { 
                State.PhasorName[x] = getLengthPrependedString(buffer, index, out strLength); //Need to store this somewhere...
                index += 2 + strLength; // for simplicity
            }
            for (x = 0; x < State.AnalogCount; x++)
            {
                State.AnalogName[x] = getLengthPrependedString(buffer, index, out strLength); 
                index += 2 + strLength; 
            }
            for (x = 0; x < State.DigitalCount; x++)
            {
                State.DigitalName[x] = getLengthPrependedString(buffer, index, out strLength); 
                index += 2 + strLength; 
            }

            //PHSCALE, 12 bytes of data flags, x PHNMR
            for (x = 0; x < State.PhasorCount; x++)
            {
                State.PhasorScale[x] = buffer.BlockCopy(index, index + 12);
                index += 12;
            }

            //ANSCALE, 8 bytes x ANNMR
            for (x = 0; x < State.AnalogCount; x++)
            {
                State.ANSCALE[x] = buffer.BlockCopy(index, index + 8);
                index += 12;

            }
            //DIGUNIT, 4 x DGNMR
            for (x = 0; x < State.DigitalCount; x++)
            {
                State.DigitalStatus[x] = buffer.BlockCopy(index, index + 4);
                index += 4;
            }
            //PMU_LAT, 4 bytes, IEEE float, -90.0 to +90.0
            State.DeviceLatitude = EndianOrder.BigEndian.ToSingle(buffer, index);
            //PMU_LON, 4 bytes, IEEE float, -179.9... to +180
            State.DeviceLongitude = EndianOrder.BigEndian.ToSingle(buffer, index + 4);
            //PMU_ELEV, 4 bytes, IEEE float, infinity for unknown
            State.DeviceElevation = EndianOrder.BigEndian.ToSingle(buffer, index + 8);
            //SVC_CLASS, 1 ASCII char
            State.ServiceClass = EndianOrder.BigEndian.ToChar(buffer, index + 9);
            //WINDOW, 4 bytes, signed int
            State.MeasurementWindow = EndianOrder.BigEndian.ToInt32(buffer, index + 13);
            //GRP_DLY, 4 bytes, signed int
            State.GroupDelay = EndianOrder.BigEndian.ToInt32(buffer, index + 17);
            //FNOM, 2 bytes, unsigned int, Bit 0 is flag (50/60 hz)
            State.FNOM = EndianOrder.BigEndian.ToUInt16(buffer, index + 19);
            //CFGCNT, 2 bytes
            State.CFGCNT = EndianOrder.BigEndian.ToUInt16(buffer, index + 21);

            return startIndex; //FIXME
        }
        /// <summary>
        /// Parses the binary footer image.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="buffer"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        protected override int ParseFooterImage(byte[] buffer, int startIndex, int length)
        {

            int index = startIndex;

            // Parse nominal frequency
            index += base.ParseFooterImage(buffer, index, length);

            // Parse out configuration count (new for version 7.0)
            if (Parent.DraftRevision > DraftRevision.Draft6)
            {
                RevisionCount = EndianOrder.BigEndian.ToUInt16(buffer, index);
                index += 2;
            }

            return (index - startIndex);
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize configuration cell
            info.AddValue("formatFlags", m_formatFlags, typeof(FormatFlags));
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Delegate handler to create a new IEEE C37.118 configuration cell
        internal static IConfigurationCell CreateNewCell(IChannelFrame parent, IChannelFrameParsingState<IConfigurationCell> state, int index, byte[] buffer, int startIndex, out int parsedLength)
        {
            ConfigurationCell configCell = new ConfigurationCell(parent as IConfigurationFrame);

            parsedLength = configCell.ParseBinaryImage(buffer, startIndex, 0);

            return configCell;
        }
        */
        #endregion
    }
}