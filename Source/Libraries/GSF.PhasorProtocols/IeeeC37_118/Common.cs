//*******************************************************************************************************
//  Common.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/12/2004 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
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

using GSF;
using System;

namespace GSF.PhasorProtocols.IeeeC37_118
{
    #region [ Enumerations ]

    /// <summary>
    /// IEEE C37.118 frame types enumeration.
    /// </summary>
    [Serializable()]
    public enum FrameType : ushort
    {
        /// <summary>
        /// 000 Data frame.
        /// </summary>
        DataFrame = (ushort)Bits.Nil,
        /// <summary>
        /// 001 Header frame.
        /// </summary>
        HeaderFrame = (ushort)Bits.Bit04,
        /// <summary>
        /// 010 Configuration frame 1.
        /// </summary>
        ConfigurationFrame1 = (ushort)Bits.Bit05,
        /// <summary>
        /// 011 Configuration frame 2.
        /// </summary>
        ConfigurationFrame2 = (ushort)(Bits.Bit04 | Bits.Bit05),
        /// <summary>
        /// 101 Configuration frame 3.
        /// </summary>
        ConfigurationFrame3 = (ushort)(Bits.Bit06 | Bits.Bit04),
        /// <summary>
        /// 100 Command frame.
        /// </summary>
        CommandFrame = (ushort)Bits.Bit06,
        /// <summary>.
        /// Reserved bit.
        /// </summary>
        Reserved = (ushort)Bits.Bit07,
        /// <summary>
        /// Version number mask.
        /// </summary>
        VersionNumberMask = (ushort)(Bits.Bit00 | Bits.Bit01 | Bits.Bit02 | Bits.Bit03)
    }

    /// <summary>
    /// Protocol draft revision numbers enumeration.
    /// </summary>
    [Serializable()]
    public enum DraftRevision : byte
    {
        /// <summary>
        /// Draft 6.0.
        /// </summary>
        Draft6 = 0,
        /// <summary>
        /// Draft 7.0 (Version 1.0).
        /// </summary>
        Draft7 = 1,
        /// <summary>
        /// Draft 8.0 (Version 8.0).
        /// </summary>
        Draft8 = 2
    }

    /// <summary>
    /// Data format flags enumeration.
    /// </summary>
    [Flags(), Serializable()]
    public enum FormatFlags : ushort
    {
        /// <summary>
        /// Frequency value format (Set = float, Clear = integer).
        /// </summary>
        Frequency = (ushort)Bits.Bit03,
        /// <summary>
        /// Analog value format (Set = float, Clear = integer).
        /// </summary>
        Analog = (ushort)Bits.Bit02,
        /// <summary>
        /// Phasor value format (Set = float, Clear = integer).
        /// </summary>
        Phasors = (ushort)Bits.Bit01,
        /// <summary>
        /// Phasor coordinate format (Set = polar, Clear = rectangular).
        /// </summary>
        Coordinates = (ushort)Bits.Bit00,
        /// <summary>
        /// Unsed format bits mask.
        /// </summary>
        UnusedMask = unchecked(ushort.MaxValue & (ushort)~(Bits.Bit00 | Bits.Bit01 | Bits.Bit02 | Bits.Bit03)),
        /// <summary>
        /// No flags.
        /// </summary>
        NoFlags = (ushort)Bits.Nil
    }

    /// <summary>
    /// Time quality flags enumeration.
    /// </summary>
    [Flags(), Serializable()]
    public enum TimeQualityFlags : uint
    {
        /// <summary>
        /// Reserved bit.
        /// </summary>
        Reserved = (uint)Bits.Bit31,
        /// <summary>
        /// Leap second direction – 0 for add, 1 for delete.
        /// </summary>
        LeapSecondDirection = (uint)Bits.Bit30,
        /// <summary>
        /// Leap second occurred – set in the first second after the leap second occurs and remains set for 24 hours.
        /// </summary>
        LeapSecondOccurred = (uint)Bits.Bit29,
        /// <summary>
        /// Leap second pending – set before a leap second occurs and cleared in the second after the leap second occurs.
        /// </summary>
        LeapSecondPending = (uint)Bits.Bit28,
        /// <summary>
        /// Time quality indicator code mask.
        /// </summary>
        TimeQualityIndicatorCodeMask = (uint)(Bits.Bit27 | Bits.Bit26 | Bits.Bit25 | Bits.Bit24),
        /// <summary>
        /// No flags.
        /// </summary>
        NoFlags = (uint)Bits.Nil
    }

    /// <summary>
    /// Time quality indicator code enumeration.
    /// </summary>
    [Serializable()]
    public enum TimeQualityIndicatorCode : uint
    {
        /// <summary>
        /// 1111 - 0xF:	Fault--clock failure, time not reliable.
        /// </summary>
        Failure = (uint)(Bits.Bit27 | Bits.Bit26 | Bits.Bit25 | Bits.Bit24),
        /// <summary>
        /// 1011 - 0xB:	Clock unlocked, time within 10^1 s.
        /// </summary>
        UnlockedWithin10Seconds = (uint)(Bits.Bit27 | Bits.Bit25 | Bits.Bit24),
        /// <summary>
        /// 1010 - 0xA:	Clock unlocked, time within 10^0 s.
        /// </summary>
        UnlockedWithin1Second = (uint)(Bits.Bit27 | Bits.Bit25),
        /// <summary>
        /// 1001 - 0x9: Clock unlocked, time within 10^-1 s.
        /// </summary>
        UnlockedWithinPoint1Seconds = (uint)(Bits.Bit27 | Bits.Bit24),
        /// <summary>
        /// 1000 - 0x8: Clock unlocked, time within 10^-2 s.
        /// </summary>
        UnlockedWithinPoint01Seconds = (uint)Bits.Bit27,
        /// <summary>
        /// 0111 - 0x7: Clock unlocked, time within 10^-3 s.
        /// </summary>
        UnlockedWithinPoint001Seconds = (uint)(Bits.Bit26 | Bits.Bit25 | Bits.Bit24),
        /// <summary>
        /// 0110 - 0x6: Clock unlocked, time within 10^-4 s.
        /// </summary>
        UnlockedWithinPoint0001Seconds = (uint)(Bits.Bit26 | Bits.Bit25),
        /// <summary>
        /// 0101 - 0x5: Clock unlocked, time within 10^-5 s.
        /// </summary>
        UnlockedWithinPoint00001Seconds = (uint)(Bits.Bit26 | Bits.Bit24),
        /// <summary>
        /// 0100 - 0x4: Clock unlocked, time within 10^-6 s.
        /// </summary>
        UnlockedWithinPoint000001Seconds = (uint)Bits.Bit26,
        /// <summary>
        /// 0011 - 0x3: Clock unlocked, time within 10^-7 s.
        /// </summary>
        UnlockedWithinPoint0000001Seconds = (uint)(Bits.Bit25 | Bits.Bit24),
        /// <summary>
        /// 0010 - 0x2: Clock unlocked, time within 10^-8 s.
        /// </summary>
        UnlockedWithinPoint00000001Seconds = (uint)Bits.Bit25,
        /// <summary>
        /// 0001 - 0x1: Clock unlocked, time within 10^-9 s.
        /// </summary>
        UnlockedWithinPoint000000001Seconds = (uint)Bits.Bit24,
        /// <summary>
        /// 0000 - 0x0: Normal operation, clock locked.
        /// </summary>
        Locked = 0
    }

    /// <summary>
    /// Status flags enumeration.
    /// </summary>
    [Flags(), Serializable()]
    public enum StatusFlags : ushort
    {
        /// <summary>
        /// Data is valid (0 when device data is valid, 1 when invalid or device is in test mode).
        /// </summary>
        DataIsValid = (ushort)Bits.Bit15,
        /// <summary>
        /// Device error including configuration error, 0 when no error.
        /// </summary>
        DeviceError = (ushort)Bits.Bit14,
        /// <summary>
        /// Device synchronization error, 0 when in sync.
        /// </summary>
        DeviceSynchronizationError = (ushort)Bits.Bit13,
        /// <summary>
        /// Data sorting type, 0 by timestamp, 1 by arrival.
        /// </summary>
        DataSortingType = (ushort)Bits.Bit12,
        /// <summary>
        /// Device trigger detected, 0 when no trigger.
        /// </summary>
        DeviceTriggerDetected = (ushort)Bits.Bit11,
        /// <summary>
        /// Configuration changed, set to 1 for one minute when configuration changed.
        /// </summary>
        ConfigurationChanged = (ushort)Bits.Bit10,
        /// <summary>
        /// Reserved bits for security, presently set to 0.
        /// </summary>
        ReservedFlags = (ushort)(Bits.Bit09 | Bits.Bit08 | Bits.Bit07 | Bits.Bit06),
        /// <summary>
        /// Unlocked time mask.
        /// </summary>
        UnlockedTimeMask = (ushort)(Bits.Bit05 | Bits.Bit04),
        /// <summary>
        /// Trigger reason mask.
        /// </summary>
        TriggerReasonMask = (ushort)(Bits.Bit03 | Bits.Bit02 | Bits.Bit01 | Bits.Bit00),
        /// <summary>
        /// No flags.
        /// </summary>
        NoFlags = (ushort)Bits.Nil
    }

    /// <summary>
    /// Unlocked time enumeration.
    /// </summary>
    [Serializable()]
    public enum UnlockedTime : byte
    {
        /// <summary>
        /// Sync locked, best quality.
        /// </summary>
        SyncLocked = (byte)Bits.Nil,
        /// <summary>
        /// Unlocked for 10 seconds.
        /// </summary>
        UnlockedFor10Seconds = (byte)Bits.Bit04,
        /// <summary>
        /// Unlocked for 100 seconds.
        /// </summary>
        UnlockedFor100Seconds = (byte)Bits.Bit05,
        /// <summary>
        /// Unlocked for over 1000 seconds.
        /// </summary>
        UnlockedForOver1000Seconds = (byte)(Bits.Bit05 | Bits.Bit04)
    }

    /// <summary>
    /// Trigger reason enumeration.
    /// </summary>
    [Serializable()]
    public enum TriggerReason : byte
    {
        /// <summary>
        /// 1111 Vendor defined trigger 8.
        /// </summary>
        VendorDefinedTrigger8 = (byte)(Bits.Bit03 | Bits.Bit02 | Bits.Bit01 | Bits.Bit00),
        /// <summary>
        /// 1110 Vendor defined trigger 7.
        /// </summary>
        VendorDefinedTrigger7 = (byte)(Bits.Bit03 | Bits.Bit02 | Bits.Bit01),
        /// <summary>
        /// 1101 Vendor defined trigger 6.
        /// </summary>
        VendorDefinedTrigger6 = (byte)(Bits.Bit03 | Bits.Bit02 | Bits.Bit00),
        /// <summary>
        /// 1100 Vendor defined trigger 5.
        /// </summary>
        VendorDefinedTrigger5 = (byte)(Bits.Bit03 | Bits.Bit02),
        /// <summary>
        /// 1011 Vendor defined trigger 4.
        /// </summary>
        VendorDefinedTrigger4 = (byte)(Bits.Bit03 | Bits.Bit01 | Bits.Bit00),
        /// <summary>
        /// 1010 Vendor defined trigger 3.
        /// </summary>
        VendorDefinedTrigger3 = (byte)(Bits.Bit03 | Bits.Bit01),
        /// <summary>
        /// 1001 Vendor defined trigger 2.
        /// </summary>
        VendorDefinedTrigger2 = (byte)(Bits.Bit03 | Bits.Bit00),
        /// <summary>
        /// 1000 Vendor defined trigger 1.
        /// </summary>
        VendorDefinedTrigger1 = (byte)Bits.Bit03,
        /// <summary>
        /// 0111 Digital.
        /// </summary>
        Digital = (byte)(Bits.Bit02 | Bits.Bit01 | Bits.Bit00),
        /// <summary>
        /// 0101 df/dt high.
        /// </summary>
        DfDtHigh = (byte)(Bits.Bit02 | Bits.Bit00),
        /// <summary>
        /// 0011 Phase angle difference.
        /// </summary>
        PhaseAngleDifference = (byte)(Bits.Bit01 | Bits.Bit00),
        /// <summary>
        /// 0001 Magnitude low.
        /// </summary>
        MagnitudeLow = (byte)Bits.Bit00,
        /// <summary>
        /// 0110 Reserved.
        /// </summary>
        Reserved = (byte)(Bits.Bit02 | Bits.Bit01),
        /// <summary>
        /// 0100 Frequency high/low.
        /// </summary>
        FrequencyHighOrLow = (byte)Bits.Bit02,
        /// <summary>
        /// 0010 Magnitude high.
        /// </summary>
        MagnitudeHigh = (byte)Bits.Bit01,
        /// <summary>
        /// 0000 Manual.
        /// </summary>
        Manual = (byte)Bits.Nil
    }

    #endregion

    /// <summary>
    /// Common IEEE C37.118 declarations and functions.
    /// </summary>
    public static class Common
    {
        /// <summary>
        /// Absolute maximum number of possible phasor values that could fit into a data frame.
        /// </summary>
        public const ushort MaximumPhasorValues = (ushort)(ushort.MaxValue / 4 - CommonFrameHeader.FixedLength - 8);

        /// <summary>
        /// Absolute maximum number of possible analog values that could fit into a data frame.
        /// </summary>
        public const ushort MaximumAnalogValues = (ushort)(ushort.MaxValue / 2 - CommonFrameHeader.FixedLength - 8);

        /// <summary>
        /// Absolute maximum number of possible digital values that could fit into a data frame.
        /// </summary>
        public const ushort MaximumDigitalValues = (ushort)(ushort.MaxValue / 2 - CommonFrameHeader.FixedLength - 8);

        /// <summary>
        /// Absolute maximum data length (in bytes) that could fit into any frame.
        /// </summary>
        public const ushort MaximumDataLength = (ushort)(ushort.MaxValue - CommonFrameHeader.FixedLength - 2);

        /// <summary>
        /// Absolute maximum number of bytes of extended data that could fit into a command frame.
        /// </summary>
        public const ushort MaximumExtendedDataLength = (ushort)(MaximumDataLength - 2);

        /// <summary>
        /// Time quality flags mask.
        /// </summary>
        public const uint TimeQualityFlagsMask = (uint)(Bits.Bit31 | Bits.Bit30 | Bits.Bit29 | Bits.Bit28 | Bits.Bit27 | Bits.Bit26 | Bits.Bit25 | Bits.Bit24);
    }
}