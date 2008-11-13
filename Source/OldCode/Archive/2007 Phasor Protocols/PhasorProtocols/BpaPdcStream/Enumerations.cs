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

namespace PCS.PhasorProtocols
{
    namespace BpaPdcStream
    {

        /// <summary>Stream type</summary>
        [Serializable()]
        public enum StreamType : byte
        {
            /// <summary>Standard full data stream</summary>
            Legacy = 0,
            // <summary>Full data stream with PMU ID's and offsets removed from data packet</summary>
            Compact = 1
        }

        /// <summary>Stream revision number</summary>
        [Serializable()]
        public enum RevisionNumber : byte
        {
            /// <summary>Original revision for all to June 2002, use NTP timetag (start count 1900)</summary>
            Revision0 = 0,
            /// <summary>July 2002 revision for std. 37.118, use UNIX timetag (start count 1970)</summary>
            Revision1 = 1,
            /// <summary>May 2005 revision for std. 37.118, change ChanFlag for added data types</summary>
            Revision2 = 2
        }

        /// <summary>Channel flags</summary>
        [Flags(), Serializable()]
        public enum ChannelFlags : byte
        {
            /// <summary>Valid if not set (yes = 0)</summary>
            DataIsValid = Bit.Bit7,
            /// <summary>Errors if set (yes = 1)</summary>
            TransmissionErrors = Bit.Bit6,
            /// <summary>Not sync'd if set (yes = 0)</summary>
            PMUSynchronized = Bit.Bit5,
            /// <summary>Data out of sync if set (yes = 1)</summary>
            DataSortedByArrival = Bit.Bit4,
            /// <summary>Sorted by timestamp if not set (yes = 0)</summary>
            DataSortedByTimestamp = Bit.Bit3,
            /// <summary>PDC format if set (yes = 1)</summary>
            PDCExchangeFormat = Bit.Bit2,
            /// <summary>Macrodyne or IEEE format (Macrodyne = 1)</summary>
            MacrodyneFormat = Bit.Bit1,
            /// <summary>Timestamp included if not set (yes = 0)</summary>
            TimestampIncluded = Bit.Bit0
        }

        /// <summary>Reserved flags</summary>
        [Flags(), Serializable()]
        public enum ReservedFlags : byte
        {
            Reserved0 = Bit.Bit7,
            Reserved1 = Bit.Bit6,
            AnalogWordsMask = Bit.Bit0 | Bit.Bit1 | Bit.Bit2 | Bit.Bit3 | Bit.Bit4 | Bit.Bit5
        }

        /// <summary>IEEE format flags</summary>
        [Flags(), Serializable()]
        public enum IEEEFormatFlags : byte
        {
            /// <summary>Frequency data format: Set = float, Clear = integer</summary>
            Frequency = Bit.Bit7,
            /// <summary>Analog data format: Set = float, Clear = integer</summary>
            Analog = Bit.Bit6,
            /// <summary>Phasor data format: Set = float, Clear = integer</summary>
            Phasors = Bit.Bit5,
            /// <summary>Phasor coordinate format: Set = polar, Clear = rectangular</summary>
            Coordinates = Bit.Bit4,
            /// <summary>Digital words mask</summary>
            DigitalWordsMask = Bit.Bit0 | Bit.Bit1 | Bit.Bit2 | Bit.Bit3
        }

        /// <summary>Frame type</summary>
        [Serializable()]
        public enum FrameType : byte
        {
            /// <summary>Configuration frame</summary>
            ConfigurationFrame,
            /// <summary>Data frame</summary>
            DataFrame
        }

        /// <summary>PMU status flags</summary>
        [Flags(), Serializable()]
        public enum PMUStatusFlags : byte
        {
            /// <summary>Synchonization is invalid</summary>
            SyncInvalid = Bit.Bit0,
            /// <summary>Data is invalid</summary>
            DataInvalid = Bit.Bit1
        }

    }
}
