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

//*******************************************************************************************************
//  Enumerations.vb - Global enumerations for this namespace
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
//  07/11/2007 - J. Ritchie Carroll
//       Moved all namespace level enumerations into "Enumerations.vb" file
//
//*******************************************************************************************************

namespace PhasorProtocols
{
    namespace IeeeC37_118
    {

        /// <summary>Protocol draft revision number</summary>
        [Serializable()]
        public enum DraftRevision : byte
        {
            /// <summary>Draft 6.0</summary>
            Draft6 = 0,
            /// <summary>Draft 7.0 (Version 1.0)</summary>
            Draft7 = 1
        }

        /// <summary>Data format flags</summary>
        [Flags(), Serializable()]
        public enum FormatFlags : short
        {
            /// <summary>Frequency value format (Set = float, Clear = integer)</summary>
            Frequency = Bit.Bit3,
            /// <summary>Analog value format (Set = float, Clear = integer)</summary>
            Analog = Bit.Bit2,
            /// <summary>Phasor value format (Set = float, Clear = integer)</summary>
            Phasors = Bit.Bit1,
            /// <summary>Phasor coordinate format (Set = polar, Clear = rectangular)</summary>
            Coordinates = Bit.Bit0,
            /// <summary>Unsed format bits mask</summary>
            UnusedMask = short.MaxValue & ~(Bit.Bit0 | Bit.Bit1 | Bit.Bit2 | Bit.Bit3)
        }

        /// <summary>Time quality flags</summary>
        [Flags(), Serializable()]
        public enum TimeQualityFlags : int
        {
            /// <summary>Reserved</summary>
            Reserved = Bit.Bit31,
            /// <summary>Leap second direction – 0 for add, 1 for delete</summary>
            LeapSecondDirection = Bit.Bit30,
            /// <summary>Leap second occurred – set in the first second after the leap second occurs and remains set for 24 hours</summary>
            LeapSecondOccurred = Bit.Bit29,
            /// <summary>Leap second pending – set before a leap second occurs and cleared in the second after the leap second occurs</summary>
            LeapSecondPending = Bit.Bit28,
            /// <summary>Time quality indicator code mask</summary>
            TimeQualityIndicatorCodeMask = Bit.Bit27 | Bit.Bit26 | Bit.Bit25 | Bit.Bit24
        }

        /// <summary>Time quality indicator code</summary>
        [Serializable()]
        public enum TimeQualityIndicatorCode : int
        {
            /// <summary>1111 - F:	Fault--clock failure, time not reliable</summary>
            Failure = Bit.Bit27 | Bit.Bit26 | Bit.Bit25 | Bit.Bit24,
            /// <summary>1011 - B:	Clock unlocked, time within 10^1 s</summary>
            UnlockedWithin10Seconds = Bit.Bit27 | Bit.Bit25 | Bit.Bit24,
            /// <summary>1010 - A:	Clock unlocked, time within 10^0 s</summary>
            UnlockedWithin1Second = Bit.Bit27 | Bit.Bit25,
            /// <summary>1001 - 9: Clock unlocked, time within 10^-1 s</summary>
            UnlockedWithinPoint1Seconds = Bit.Bit27 | Bit.Bit24,
            /// <summary>1000 - 8: Clock unlocked, time within 10^-2 s</summary>
            UnlockedWithinPoint01Seconds = Bit.Bit27,
            /// <summary>0111 - 7: Clock unlocked, time within 10^-3 s</summary>
            UnlockedWithinPoint001Seconds = Bit.Bit26 | Bit.Bit25 | Bit.Bit24,
            /// <summary>0110 - 6: Clock unlocked, time within 10^-4 s</summary>
            UnlockedWithinPoint0001Seconds = Bit.Bit26 | Bit.Bit25,
            /// <summary>0101 - 5: Clock unlocked, time within 10^-5 s</summary>
            UnlockedWithinPoint00001Seconds = Bit.Bit26 | Bit.Bit24,
            /// <summary>0100 - 4: Clock unlocked, time within 10^-6 s</summary>
            UnlockedWithinPoint000001Seconds = Bit.Bit26,
            /// <summary>0011 - 3: Clock unlocked, time within 10^-7 s</summary>
            UnlockedWithinPoint0000001Seconds = Bit.Bit25 | Bit.Bit24,
            /// <summary>0010 - 2: Clock unlocked, time within 10^-8 s</summary>
            UnlockedWithinPoint00000001Seconds = Bit.Bit25,
            /// <summary>0001 - 1: Clock unlocked, time within 10^-9 s</summary>
            UnlockedWithinPoint000000001Seconds = Bit.Bit24,
            /// <summary>0000 - 0: Normal operation, clock locked</summary>
            Locked = 0
        }

        /// <summary>Frame type</summary>
        [Serializable()]
        public enum FrameType : short
        {
            /// <summary>000 Data frame</summary>
            DataFrame = Bit.Nill,
            /// <summary>001 Header frame</summary>
            HeaderFrame = Bit.Bit4,
            /// <summary>010 Configuration frame 1</summary>
            ConfigurationFrame1 = Bit.Bit5,
            /// <summary>011 Configuration frame 2</summary>
            ConfigurationFrame2 = Bit.Bit4 | Bit.Bit5,
            /// <summary>100 Command frame</summary>
            CommandFrame = Bit.Bit6,
            /// <summary>Reserved bit</summary>
            Reserved = Bit.Bit7,
            /// <summary>Version number mask</summary>
            VersionNumberMask = Bit.Bit0 | Bit.Bit1 | Bit.Bit2 | Bit.Bit3
        }

        /// <summary>Status flags</summary>
        [Flags(), Serializable()]
        public enum StatusFlags : short
        {
            /// <summary>Data is valid (0 when PMU data is valid, 1 when invalid or PMU is in test mode)</summary>
            DataIsValid = Bit.Bit15,
            /// <summary>PMU error including configuration error, 0 when no error</summary>
            PmuError = Bit.Bit14,
            /// <summary>PMU synchronization error, 0 when in sync</summary>
            PmuSynchronizationError = Bit.Bit13,
            /// <summary>Data sorting type, 0 by timestamp, 1 by arrival</summary>
            DataSortingType = Bit.Bit12,
            /// <summary>PMU trigger detected, 0 when no trigger</summary>
            PmuTriggerDetected = Bit.Bit11,
            /// <summary>Configuration changed, set to 1 for one minute when configuration changed</summary>
            ConfigurationChanged = Bit.Bit10,
            /// <summary>Reserved bits for security, presently set to 0</summary>
            ReservedFlags = Bit.Bit9 | Bit.Bit8 | Bit.Bit7 | Bit.Bit6,
            /// <summary>Unlocked time mask</summary>
            UnlockedTimeMask = Bit.Bit5 | Bit.Bit4,
            /// <summary>Trigger reason mask</summary>
            TriggerReasonMask = Bit.Bit3 | Bit.Bit2 | Bit.Bit1 | Bit.Bit0
        }

        /// <summary>Unlocked time</summary>
        [Serializable()]
        public enum UnlockedTime : byte
        {
            /// <summary>Sync locked, best quality</summary>
            SyncLocked = Bit.Nill,
            /// <summary>Unlocked for 10 seconds</summary>
            UnlockedFor10Seconds = Bit.Bit4,
            /// <summary>Unlocked for 100 seconds</summary>
            UnlockedFor100Seconds = Bit.Bit5,
            /// <summary>Unlocked for over 1000 seconds</summary>
            UnlockedForOver1000Seconds = Bit.Bit5 | Bit.Bit4
        }

        /// <summary>Trigger reason</summary>
        [Serializable()]
        public enum TriggerReason : byte
        {
            /// <summary>1111 Vendor defined trigger 7</summary>
            VendorDefinedTrigger8 = Bit.Bit3 | Bit.Bit2 | Bit.Bit1 | Bit.Bit0,
            /// <summary>1110 Vendor defined trigger 7</summary>
            VendorDefinedTrigger7 = Bit.Bit3 | Bit.Bit2 | Bit.Bit1,
            /// <summary>1101 Vendor defined trigger 6</summary>
            VendorDefinedTrigger6 = Bit.Bit3 | Bit.Bit2 | Bit.Bit0,
            /// <summary>1100 Vendor defined trigger 5</summary>
            VendorDefinedTrigger5 = Bit.Bit3 | Bit.Bit2,
            /// <summary>1011 Vendor defined trigger 4</summary>
            VendorDefinedTrigger4 = Bit.Bit3 | Bit.Bit1 | Bit.Bit0,
            /// <summary>1010 Vendor defined trigger 3</summary>
            VendorDefinedTrigger3 = Bit.Bit3 | Bit.Bit1,
            /// <summary>1001 Vendor defined trigger 2</summary>
            VendorDefinedTrigger2 = Bit.Bit3 | Bit.Bit0,
            /// <summary>1000 Vendor defined trigger 1</summary>
            VendorDefinedTrigger1 = Bit.Bit3,
            /// <summary>0111 Digital</summary>
            Digital = Bit.Bit2 | Bit.Bit1 | Bit.Bit0,
            /// <summary>0101 df/dt high</summary>
            DfDtHigh = Bit.Bit2 | Bit.Bit0,
            /// <summary>0011 Phase angle difference</summary>
            PhaseAngleDifference = Bit.Bit1 | Bit.Bit0,
            /// <summary>0001 Magnitude low</summary>
            MagnitudeLow = Bit.Bit0,
            /// <summary>0110 Reserved</summary>
            Reserved = Bit.Bit2 | Bit.Bit1,
            /// <summary>0100 Frequency high/low</summary>
            FrequencyHighOrLow = Bit.Bit2,
            /// <summary>0010 Magnitude high</summary>
            MagnitudeHigh = Bit.Bit1,
            /// <summary>0000 Manual</summary>
            Manual = Bit.Nill
        }

        /// <summary>Analog types</summary>
        [Serializable()]
        public enum AnalogType : byte
        {
            /// <summary>Single point-on-wave</summary>
            SinglePointOnWave = 0,
            /// <summary>RMS of analog input</summary>
            RmsOfAnalogInput = 1,
            /// <summary>Peak of analog input</summary>
            PeakOfAnalogInput = 2
        }

    }
}
