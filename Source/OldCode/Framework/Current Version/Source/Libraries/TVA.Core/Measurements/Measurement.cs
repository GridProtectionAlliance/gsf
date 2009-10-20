//*******************************************************************************************************
//  Measurement.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  12/8/2005 - J. Ritchie Carroll
//       Initial version of source generated.
//  09/16/2008 - J. Ritchie Carroll
//       Converted to C#.
//  08/06/2009 - Josh L. Patterson
//       Edited Comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//
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
using System.ComponentModel;

namespace TVA.Measurements
{
    /// <summary>
    /// Implementation of a basic measurement.
    /// </summary>
    [CLSCompliant(false)]
    public class Measurement : IMeasurement
    {
        #region [ Members ]

        // Fields
        private uint m_id;
        private string m_source;
        private MeasurementKey m_key;
        private Guid m_signalID;
        private string m_tagName;
        private Ticks m_timestamp;
        private double m_value;
        private double m_adder;
        private double m_multiplier;
        private bool m_valueQualityIsGood;
        private bool m_timestampQualityIsGood;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new <see cref="Measurement"/> using default settings.
        /// </summary>
        public Measurement()
            : this(uint.MaxValue, "__", Guid.Empty, double.NaN, 0.0, 1.0, 0)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="Measurement"/> given the specified parameters.
        /// </summary>
        /// <param name="id">Numeric ID of the new measurement.</param>
        /// <param name="source">Source name of the new measurement.</param>
        public Measurement(uint id, string source)
            : this(id, source, Guid.Empty, double.NaN, 0.0, 1.0, 0)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="Measurement"/> given the specified parameters.
        /// </summary>
        /// <param name="signalID"><see cref="Guid"/> based signal ID of the new measurement.</param>
        public Measurement(Guid signalID)
            : this(uint.MaxValue, "__", signalID, double.NaN, 0.0, 1.0, 0)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="Measurement"/> given the specified parameters.
        /// </summary>
        /// <param name="id">Numeric ID of the new measurement.</param>
        /// <param name="source">Source name of the new measurement.</param>
        /// <param name="value">Value of the new measurement.</param>
        /// <param name="timestamp">Timestamp, in ticks, of the new measurement.</param>
        public Measurement(uint id, string source, double value, Ticks timestamp)
            : this(id, source, Guid.Empty, value, 0.0, 1.0, timestamp)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="Measurement"/> given the specified parameters.
        /// </summary>
        /// <param name="signalID"><see cref="Guid"/> based signal ID of the new measurement.</param>
        /// <param name="value">Value of the new measurement.</param>
        /// <param name="timestamp">Timestamp, in ticks, of the new measurement.</param>
        public Measurement(Guid signalID, double value, Ticks timestamp)
            : this(uint.MaxValue, "__", signalID, value, 0.0, 1.0, timestamp)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="Measurement"/> given the specified parameters.
        /// </summary>
        /// <param name="id">Numeric ID of the new measurement.</param>
        /// <param name="source">Source name of the new measurement.</param>
        /// <param name="tagName">Text based tag name of the new measurement.</param>
        /// <param name="adder">Defined adder to apply to the new measurement.</param>
        /// <param name="multiplier">Defined multiplier to apply to the new measurement.</param>
        public Measurement(uint id, string source, string tagName, double adder, double multiplier)
            : this(id, source, Guid.Empty, double.NaN, adder, multiplier, 0)
        {
            m_tagName = tagName;
        }

        /// <summary>
        /// Constructs a new <see cref="Measurement"/> given the specified parameters.
        /// </summary>
        /// <param name="id">Numeric ID of the new measurement.</param>
        /// <param name="source">Source name of the new measurement.</param>
        /// <param name="signalID"><see cref="Guid"/> based signal ID of the new measurement.</param>
        /// <param name="value">Value of the new measurement.</param>
        /// <param name="adder">Defined adder to apply to the new measurement.</param>
        /// <param name="multiplier">Defined multiplier to apply to the new measurement.</param>
        /// <param name="timestamp">Timestamp, in ticks, of the new measurement.</param>
        public Measurement(uint id, string source, Guid signalID, double value, double adder, double multiplier, Ticks timestamp)
        {
            m_id = id;
            m_source = source;
            m_key = new MeasurementKey(m_id, m_source);
            m_signalID = signalID;
            m_value = value;
            m_adder = adder;
            m_multiplier = multiplier;
            m_timestamp = timestamp;
            m_valueQualityIsGood = true;
            m_timestampQualityIsGood = true;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the numeric ID of this <see cref="Measurement"/>.
        /// </summary>
        /// <remarks>
        /// <para>In most implementations, this will be a required field.</para>
        /// <para>Note that this field, in addition to <see cref="Source"/>, typically creates the primary key for a <see cref="Measurement"/>.</para>
        /// </remarks>
        public virtual uint ID
        {
            get
            {
                return m_id;
            }
            set
            {
                if (m_id != value)
                {
                    m_id = value;
                    m_key = new MeasurementKey(m_id, m_source);
                }
            }
        }

        /// <summary>
        /// Gets or sets the source of this <see cref="Measurement"/>.
        /// </summary>
        /// <remarks>
        /// <para>In most implementations, this will be a required field.</para>
        /// <para>Note that this field, in addition to <see cref="ID"/>, typically creates the primary key for a <see cref="Measurement"/>.</para>
        /// <para>This value is typically used to track the archive name in which measurement is stored.</para>
        /// </remarks>
        public virtual string Source
        {
            get
            {
                return m_source;
            }
            set
            {
                if (m_source != value)
                {
                    m_source = value;
                    m_key = new MeasurementKey(m_id, m_source);
                }
            }
        }

        /// <summary>
        /// Gets the primary key (a <see cref="MeasurementKey"/>, of this <see cref="Measurement"/>.
        /// </summary>
        public virtual MeasurementKey Key
        {
            get
            {
                return m_key;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Guid"/> based signal ID of this <see cref="Measurement"/>, if available.
        /// </summary>
        public virtual Guid SignalID
        {
            get
            {
                return m_signalID;
            }
            set
            {
                m_signalID = value;
            }
        }

        /// <summary>
        /// Gets or sets the text based tag name of this <see cref="Measurement"/>.
        /// </summary>
        public virtual string TagName
        {
            get
            {
                return m_tagName;
            }
            set
            {
                m_tagName = value;
            }
        }

        /// <summary>
        /// Gets or sets the raw measurement value that is not offset by <see cref="Adder"/> and <see cref="Multiplier"/>.
        /// </summary>
        /// <returns>Raw value of this <see cref="Measurement"/> (i.e., value that is not offset by <see cref="Adder"/> and <see cref="Multiplier"/>).</returns>
        public virtual double Value
        {
            get
            {
                return m_value;
            }
            set
            {
                m_value = value;
            }
        }

        /// <summary>
        /// Gets the adjusted numeric value of this measurement, taking into account the specified <see cref="Adder"/> and <see cref="Multiplier"/> offsets.
        /// </summary>
        /// <remarks>
        /// Note that returned value will be offset by <see cref="Adder"/> and <see cref="Multiplier"/>.
        /// </remarks>
        /// <returns><see cref="Value"/> offset by <see cref="Adder"/> and <see cref="Multiplier"/> (i.e., <c><see cref="Value"/> * <see cref="Multiplier"/> + <see cref="Adder"/></c>).</returns>
        public virtual double AdjustedValue
        {
            get
            {
                return m_value * m_multiplier + m_adder;
            }
        }

        /// <summary>
        /// Gets or sets an offset to add to the measurement value. This defaults to 0.0.
        /// </summary>
        [DefaultValue(0.0)]
        public virtual double Adder
        {
            get
            {
                return m_adder;
            }
            set
            {
                m_adder = value;
            }
        }

        /// <summary>
        /// Defines a mulplicative offset to apply to the measurement value. This defaults to 1.0.
        /// </summary>
        [DefaultValue(1.0)]
        public virtual double Multiplier
        {
            get
            {
                return m_multiplier;
            }
            set
            {
                m_multiplier = value;
            }
        }

        /// <summary>
        /// Gets or sets exact timestamp, in ticks, of the data represented by this <see cref="Measurement"/>.
        /// </summary>
        /// <remarks>
        /// The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001.
        /// </remarks>
        public virtual Ticks Timestamp
        {
            get
            {
                return m_timestamp;
            }
            set
            {
                m_timestamp = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value determining if the quality of the numeric value of this <see cref="Measurement"/> is good.
        /// </summary>
        public virtual bool ValueQualityIsGood
        {
            get
            {
                return m_valueQualityIsGood;
            }
            set
            {
                m_valueQualityIsGood = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value determining if the quality of the timestamp of this <see cref="Measurement"/> is good.
        /// </summary>
        public virtual bool TimestampQualityIsGood
        {
            get
            {
                return m_timestampQualityIsGood;
            }
            set
            {
                m_timestampQualityIsGood = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="Measurement"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="Measurement"/>.</returns>
        public override string ToString()
        {
            return Measurement.ToString(this);
        }

        /// <summary>
        /// Determines whether the specified <see cref="IMeasurement"/> is equal to the current <see cref="Measurement"/>.
        /// </summary>
        /// <param name="other">The <see cref="IMeasurement"/> to compare with the current <see cref="Measurement"/>.</param>
        /// <returns>
        /// true if the specified <see cref="IMeasurement"/> is equal to the current <see cref="Measurement"/>;
        /// otherwise, false.
        /// </returns>
        public bool Equals(IMeasurement other)
        {
            return (CompareTo(other) == 0);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is equal to the current <see cref="Measurement"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="Measurement"/>.</param>
        /// <returns>
        /// true if the specified <see cref="Object"/> is equal to the current <see cref="Measurement"/>;
        /// otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="obj"/> is not an <see cref="IMeasurement"/>.</exception>
        public override bool Equals(object obj)
        {
            IMeasurement other = obj as IMeasurement;
            
            if ((object)other != null)
                return Equals(other);
            
            throw new ArgumentException("Object is not a Measurement");
        }

        /// <summary>
        /// Compares the <see cref="Measurement"/> with an <see cref="IMeasurement"/>.
        /// </summary>
        /// <param name="other">The <see cref="IMeasurement"/> to compare with the current <see cref="Measurement"/>.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        /// <remarks>This implementation of a basic measurement compares itself by value.</remarks>
        public int CompareTo(IMeasurement other)
        {
            if ((object)other != null)
                return m_value.CompareTo(other.Value);

            return 1;
        }

        /// <summary>
        /// Compares the <see cref="Measurement"/> with the specified <see cref="Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="Measurement"/>.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        /// <exception cref="ArgumentException"><paramref name="obj"/> is not an <see cref="IMeasurement"/>.</exception>
        /// <remarks>This implementation of a basic measurement compares itself by value.</remarks>
        public int CompareTo(object obj)
        {
            IMeasurement other = obj as IMeasurement;
            
            if ((object)other != null)
                return CompareTo(other);

            throw new ArgumentException("Measurement can only be compared with other IMeasurements");
        }

        /// <summary>
        /// Serves as a hash function for the current <see cref="Measurement"/>.
        /// </summary>
        /// <returns>A hash code for the current <see cref="Measurement"/>.</returns>
        /// <remarks>Hash code based on value of measurement.</remarks>
        public override int GetHashCode()
        {
            return m_value.GetHashCode();
        }

        #endregion

        #region [ Operators ]

        /// <summary>
        /// Compares two <see cref="Measurement"/> values for equality.
        /// </summary>
        /// <param name="measurement1">A <see cref="Measurement"/> left hand operand.</param>
        /// <param name="measurement2">A <see cref="Measurement"/> right hand operand.</param>
        /// <returns>A boolean representing the result.</returns>
        public static bool operator ==(Measurement measurement1, Measurement measurement2)
        {
            return measurement1.Equals(measurement2);
        }

        /// <summary>
        /// Compares two <see cref="Measurement"/> values for inequality.
        /// </summary>
        /// <param name="measurement1">A <see cref="Measurement"/> left hand operand.</param>
        /// <param name="measurement2">A <see cref="Measurement"/> right hand operand.</param>
        /// <returns>A boolean representing the result.</returns>
        public static bool operator !=(Measurement measurement1, Measurement measurement2)
        {
            return !measurement1.Equals(measurement2);
        }

        /// <summary>
        /// Returns true if left <see cref="Measurement"/> value is greater than right <see cref="Measurement"/> value.
        /// </summary>
        /// <param name="measurement1">A <see cref="Measurement"/> left hand operand.</param>
        /// <param name="measurement2">A <see cref="Measurement"/> right hand operand.</param>
        /// <returns>A boolean representing the result.</returns>
        public static bool operator >(Measurement measurement1, Measurement measurement2)
        {
            return measurement1.CompareTo(measurement2) > 0;
        }

        /// <summary>
        /// Returns true if left <see cref="Measurement"/> value is greater than or equal to right <see cref="Measurement"/> value.
        /// </summary>
        /// <param name="measurement1">A <see cref="Measurement"/> left hand operand.</param>
        /// <param name="measurement2">A <see cref="Measurement"/> right hand operand.</param>
        /// <returns>A boolean representing the result.</returns>
        public static bool operator >=(Measurement measurement1, Measurement measurement2)
        {
            return measurement1.CompareTo(measurement2) >= 0;
        }

        /// <summary>
        /// Returns true if left <see cref="Measurement"/> value is less than right <see cref="Measurement"/> value.
        /// </summary>
        /// <param name="measurement1">A <see cref="Measurement"/> left hand operand.</param>
        /// <param name="measurement2">A <see cref="Measurement"/> right hand operand.</param>
        /// <returns>A boolean representing the result.</returns>
        public static bool operator <(Measurement measurement1, Measurement measurement2)
        {
            return measurement1.CompareTo(measurement2) < 0;
        }

        /// <summary>
        /// Returns true if left <see cref="Measurement"/> value is less than or equal to right <see cref="Measurement"/> value.
        /// </summary>
        /// <param name="measurement1">A <see cref="Measurement"/> left hand operand.</param>
        /// <param name="measurement2">A <see cref="Measurement"/> right hand operand.</param>
        /// <returns>A boolean representing the result.</returns>
        public static bool operator <=(Measurement measurement1, Measurement measurement2)
        {
            return measurement1.CompareTo(measurement2) <= 0;
        }

        #endregion

        #region [ Static ]

        /// <summary>
        /// Creates a copy of the specified measurement.
        /// </summary>
        /// <param name="measurementToClone">Specified measurement to clone.</param>
        /// <returns>A copy of the <see cref="Measurement"/> object.</returns>
        public static Measurement Clone(IMeasurement measurementToClone)
        {
            return new Measurement(measurementToClone.ID, measurementToClone.Source, measurementToClone.SignalID, measurementToClone.Value, measurementToClone.Adder, measurementToClone.Multiplier, measurementToClone.Timestamp);
        }

        /// <summary>
        /// Creates a copy of the specified measurement using a new timestamp.
        /// </summary>
        /// <param name="measurementToClone">Specified measurement to clone.</param>
        /// <param name="timestamp">New timestamp, in ticks, for cloned measurement.</param>
        /// <returns>A copy of the <see cref="Measurement"/> object.</returns>
        public static Measurement Clone(IMeasurement measurementToClone, Ticks timestamp)
        {
            return new Measurement(measurementToClone.ID, measurementToClone.Source, measurementToClone.SignalID, measurementToClone.Value, measurementToClone.Adder, measurementToClone.Multiplier, timestamp);
        }

        /// <summary>
        /// Creates a copy of the specified measurement using a new value and timestamp.
        /// </summary>
        /// <param name="measurementToClone">Specified measurement to clone.</param>
        /// <param name="value">New value for cloned measurement.</param>
        /// <param name="timestamp">New timestamp, in ticks, for cloned measurement.</param>
        /// <returns>A copy of the <see cref="Measurement"/> object.</returns>
        public static Measurement Clone(IMeasurement measurementToClone, double value, Ticks timestamp)
        {
            return new Measurement(measurementToClone.ID, measurementToClone.Source, measurementToClone.SignalID, value, measurementToClone.Adder, measurementToClone.Multiplier, timestamp);
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents the specified <see cref="IMeasurement"/>.
        /// </summary>
        /// <param name="measurement"><see cref="IMeasurement"/> to convert to a <see cref="String"/> representation.</param>
        /// <returns>A <see cref="String"/> that represents the specified <see cref="IMeasurement"/>.</returns>
        public static string ToString(IMeasurement measurement)
        {
            if (measurement == null)
            {
                return "Undefined";
            }
            else
            {
                string tagName = measurement.TagName;
                string keyText = measurement.Key.ToString();

                if (string.IsNullOrEmpty(tagName))
                    return keyText;
                else
                    return string.Format("{0} [{1}]", tagName, keyText);
            }
        }

        #endregion
    }
}