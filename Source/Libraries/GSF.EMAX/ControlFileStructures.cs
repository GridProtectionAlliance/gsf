//******************************************************************************************************
//  ControlFileStructures.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
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
//  08/05/2014 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.IO;
using System.Runtime.InteropServices;

// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable NotAccessedField.Local
// ReSharper disable UnusedField.Compiler
namespace GSF.EMAX
{
    // All control structures are little-endian encoded
    // All control structures only occur once within the control file

    public enum DataSize : byte
    {
        Bits12 = 0xAF,
        Bits16 = 0xAB
    }

    public enum StructureType : ushort
    {
        EndOfStructures = 0x0000,
        SYSTEM_PARAMETERS = 0x0001,
        A_E_RSLTS = 0x0003,
        ANALOG_GROUP = 0x0004,
        EVENT_GROUP = 0x0005,
        ANLG_CHNL_NEW = 0x0006,
        EVNT_CHNL_NEW = 0x0007,
        ANLG_CHNLS = 0x0008,
        SYS_SETTINGS = 0x000B,
        SHORT_HEADER = 0x000D,
        FAULT_HEADER = 0x010D, // (with RMS)
        EVENT_DISPLAY = 0x000E,
        IDENTSTRING = 0x0010,
        A_SELECTION = 0x0040,
        E_GROUP_SELECT = 0x0041,
        PHASOR_GROUP = 0x0042,
        LINE_CONSTANTS = 0x0043,
        LINE_NAMES = 0x0044,
        FAULT_LOCATION = 0x0045,
        SENS_RSLTS = 0x0050,
        SEQUENCE_CHANNELS = 0x0051,
        TPwrRcd = 0x0052,
        BoardAnalogEventChannels = 0x0060,
        BREAKER_TRIP_TIMES = 0x0061,
        Unknown = 0xFFFF
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CTL_HEADER
    {
        [MarshalAs(UnmanagedType.U2)]
        public ushort id;                   // System ID
        [MarshalAs(UnmanagedType.U4)]
        public uint time;                   // Time of CTL file creation
        [MarshalAs(UnmanagedType.U1)]
        public byte num_of_structs;         // Number of Structures in CTL file

        public DateTime Timestamp
        {
            get
            {
                return new UnixTimeTag(time).ToDateTime();
            }
        }
    }

    public struct CTL_FILE_STRUCT
    {
        public StructureType type;
        public uint offset;

        public CTL_FILE_STRUCT(BinaryReader reader)
        {
            ushort ushortValue = BigEndian.ToUInt16(reader.ReadBytes(2), 0);

            if (!Enum.IsDefined(typeof(StructureType), ushortValue))
                type = StructureType.Unknown;
            else
                type = (StructureType)ushortValue;

            offset = reader.ReadUInt32() >> 8;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct SYSTEMTIME
    {
        [MarshalAs(UnmanagedType.U2)]
        public ushort Year;
        [MarshalAs(UnmanagedType.U2)]
        public ushort Month;
        [MarshalAs(UnmanagedType.U2)]
        public ushort DayOfWeek;
        [MarshalAs(UnmanagedType.U2)]
        public ushort Day;
        [MarshalAs(UnmanagedType.U2)]
        public ushort Hour;
        [MarshalAs(UnmanagedType.U2)]
        public ushort Minute;
        [MarshalAs(UnmanagedType.U2)]
        public ushort Second;
        [MarshalAs(UnmanagedType.U2)]
        public ushort Milliseconds;

        public SYSTEMTIME(DateTime dt)
        {
            dt = dt.ToUniversalTime(); // SetSystemTime expects the SYSTEMTIME in UTC
            Year = (ushort)dt.Year;
            Month = (ushort)dt.Month;
            DayOfWeek = (ushort)dt.DayOfWeek;
            Day = (ushort)dt.Day;
            Hour = (ushort)dt.Hour;
            Minute = (ushort)dt.Minute;
            Second = (ushort)dt.Second;
            Milliseconds = (ushort)dt.Millisecond;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct TIME_ZONE_INFORMATION
    {
        [MarshalAs(UnmanagedType.I4)]
        public Int32 Bias;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string StandardName;
        public SYSTEMTIME StandardDate;
        [MarshalAs(UnmanagedType.I4)]
        public Int32 StandardBias;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DaylightName;
        public SYSTEMTIME DaylightDate;
        [MarshalAs(UnmanagedType.I4)]
        public Int32 DaylightBias;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct SYSTEM_PARAMETERS
    {
        [MarshalAs(UnmanagedType.U2)]
        public ushort samples_per_second;
        [MarshalAs(UnmanagedType.I1)]
        public sbyte frequency;
        [MarshalAs(UnmanagedType.I1)]
        public sbyte vXtndFlt;
        [MarshalAs(UnmanagedType.I2)]
        public short channel_offset;
        [MarshalAs(UnmanagedType.I2)]
        public short prefault_samples;
        [MarshalAs(UnmanagedType.I2)]
        public short prefault_smpls_disp;       /* set for the number to be displayed */
        [MarshalAs(UnmanagedType.I4)]
        public int postfault_samples;           /* normal single length record from samples per second times postfault_time */
        [MarshalAs(UnmanagedType.I4)]
        public int postfault_samples_max;       /* maximum length of extended records times postfault_time_max */
        [MarshalAs(UnmanagedType.I2)]
        public short postfault_time;            /* normal minimum record included in 32kword record */
        [MarshalAs(UnmanagedType.I2)]
        public short postfault_time_max;
        [MarshalAs(UnmanagedType.I2)]
        public short record_fill;               /* 1 = fill 2=double_pixel 4 = dot_join (or for combinations 8 = polarity inversion */
        [MarshalAs(UnmanagedType.I2)]
        public short event_groups;
        [MarshalAs(UnmanagedType.I2)]
        public short analog_groups;
        [MarshalAs(UnmanagedType.I2)]
        public short sequence;                  /* fault number sequence */
        [MarshalAs(UnmanagedType.I2)]
        public short time_flag1;                /* (determines if irig is set to on) */
        [MarshalAs(UnmanagedType.U1)]
        public byte board1_scan_multiplier;
        [MarshalAs(UnmanagedType.U1)]
        public byte board2_scan_multiplier;
        [MarshalAs(UnmanagedType.U4)]
        public uint time_rtc;                   /* accumulator for system time in seconds real time clock */
        [MarshalAs(UnmanagedType.U4)]
        public uint time_fault;                 /* time in seconds of the triggering fault */
        [MarshalAs(UnmanagedType.U4)]
        public uint time_sys;                   /* general timekeeping position */
        [MarshalAs(UnmanagedType.I2)]
        public short mS_time;                   /* milliseconds of the triggering fault */
        [MarshalAs(UnmanagedType.I2)]
        public short flag;                      /* TRUE if not processed */
        [MarshalAs(UnmanagedType.I4)]
        public int rcd_sample_count;            /* actual count of samples in record(banks * 1024) */
        [MarshalAs(UnmanagedType.I2)]
        public short start_offset_samples;      /* count of samples up to the fault time (500 or 1000) */
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 22)]
        public string local_ident;
        [MarshalAs(UnmanagedType.U4)]
        public uint time_startup;               /* time of system startup */
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string time_zone;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string version_string;
        public TIME_ZONE_INFORMATION time_zone_information;

        public DateTime FaultTime
        {
            get
            {
                return new UnixTimeTag(time_fault).ToDateTime();
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct SYS_SETTINGS
    {
        [MarshalAs(UnmanagedType.I2)]
        public short postfault_time;
        [MarshalAs(UnmanagedType.I2)]
        public short postfault_time_max;
        [MarshalAs(UnmanagedType.I2)]
        public short prefault_smpls_disp;
        [MarshalAs(UnmanagedType.U2)]
        public ushort samples_per_second;
        [MarshalAs(UnmanagedType.I1)]
        public sbyte frequency;
        [MarshalAs(UnmanagedType.I1)]
        public sbyte vXtndFlt;
        [MarshalAs(UnmanagedType.I2)]
        public short record_fill;
        [MarshalAs(UnmanagedType.I2)]
        public short event_groups;
        [MarshalAs(UnmanagedType.I2)]
        public short analog_groups;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 22)]
        public string local_ident;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string time_zone;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string version_string;
        public TIME_ZONE_INFORMATION time_zone_information;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct A_E_RESULTS
    {
        [MarshalAs(UnmanagedType.U2)]
        public ushort flag_rtn_r;
        [MarshalAs(UnmanagedType.U2)]
        public ushort flag_fault_r;
        [MarshalAs(UnmanagedType.U2)]
        public ushort flag_on_r;
        [MarshalAs(UnmanagedType.U2)]
        public ushort limit_count_r;
        [MarshalAs(UnmanagedType.R8)]
        public double running_sum;
        [MarshalAs(UnmanagedType.U2)]
        public ushort rms_counter;
    }

    // Since this structure includes dynamically sized arrays, we manually parse structure
    public struct A_E_RSLTS
    {
        public short sequence_r;
        public ushort flag_r;
        public ushort offset_r;
        public ushort channel_r;
        public ushort mSec_r;
        public uint time_fault_r;
        public int rcd_sample_count_r;        /* length of record in lines */
        public A_E_RESULTS[] aer;

        public A_E_RSLTS(BinaryReader reader, int channels, int analogGroups)
        {
            sequence_r = reader.ReadInt16();
            flag_r = reader.ReadUInt16();
            offset_r = reader.ReadUInt16();
            channel_r = reader.ReadUInt16();
            mSec_r = reader.ReadUInt16();
            time_fault_r = reader.ReadUInt32();
            rcd_sample_count_r = reader.ReadInt32();

            aer = new A_E_RESULTS[channels * analogGroups];

            for (int i = 0; i < aer.Length; i++)
            {
                aer[i] = reader.ReadStructure<A_E_RESULTS>();
            }
        }
    }

    // Since this structure includes dynamically sized arrays, we manually parse structure
    public struct ANALOG_GROUP
    {
        public ushort on;                   /* if any mask is on */
        public ushort flag_high;            /* for 1 cycle excess */
        public ushort flag_high_end;        /* for end of bank test for excess */
        public ushort mask_high;            /* a 1 indicates the channel is active */
        public ushort flag_low;             /* for 1 cycle under */
        public ushort flag_low_end;
        public ushort mask_low;
        public ushort flag_slope;
        public ushort mask_slope;
        public ushort flag_limit;
        public ushort mask_limit;
        public ushort[] limit_set;
        public ushort[] limit_count;
        public ushort[] sign_flag;
        public short[] high_set;
        public ushort[] avg_phasor;
        public ushort[] low_set;
        public ushort[] triggerdelayunder;
        public ushort[] avg_phasor_count;
        public short[][] cycle_value;
        public short[] cycle_compare;
        public short[] slope_rate;

        public ANALOG_GROUP(BinaryReader reader, int channels)
        {
            limit_set = new ushort[channels];
            limit_count = new ushort[channels];
            sign_flag = new ushort[channels];
            high_set = new short[channels];
            avg_phasor = new ushort[channels];
            low_set = new ushort[channels];
            triggerdelayunder = new ushort[channels];
            avg_phasor_count = new ushort[channels];
            cycle_value = new short[channels][];
            cycle_compare = new short[channels];
            slope_rate = new short[channels];

            for (int i = 0; i < cycle_value.Length; i++)
            {
                cycle_value[i] = new short[2];
            }

            on = reader.ReadUInt16();
            flag_high = reader.ReadUInt16();
            flag_high_end = reader.ReadUInt16();
            mask_high = reader.ReadUInt16();
            flag_low = reader.ReadUInt16();
            flag_low_end = reader.ReadUInt16();
            mask_low = reader.ReadUInt16();
            flag_slope = reader.ReadUInt16();
            mask_slope = reader.ReadUInt16();
            flag_limit = reader.ReadUInt16();
            mask_limit = reader.ReadUInt16();

            for (int i = 0; i < limit_set.Length; i++)
            {
                limit_set[i] = reader.ReadUInt16();
            }

            for (int i = 0; i < limit_count.Length; i++)
            {
                limit_count[i] = reader.ReadUInt16();
            }

            for (int i = 0; i < sign_flag.Length; i++)
            {
                sign_flag[i] = reader.ReadUInt16();
            }

            for (int i = 0; i < high_set.Length; i++)
            {
                high_set[i] = reader.ReadInt16();
            }

            for (int i = 0; i < avg_phasor.Length; i++)
            {
                avg_phasor[i] = reader.ReadUInt16();
            }

            for (int i = 0; i < low_set.Length; i++)
            {
                low_set[i] = reader.ReadUInt16();
            }

            for (int i = 0; i < triggerdelayunder.Length; i++)
            {
                triggerdelayunder[i] = reader.ReadUInt16();
            }

            for (int i = 0; i < avg_phasor_count.Length; i++)
            {
                avg_phasor_count[i] = reader.ReadUInt16();
            }

            for (int i = 0; i < cycle_value.Length; i++)
            {
                for (int j = 0; j < cycle_value[i].Length; j++)
                {
                    cycle_value[i][j] = reader.ReadInt16();
                }
            }

            for (int i = 0; i < cycle_compare.Length; i++)
            {
                cycle_compare[i] = reader.ReadInt16();
            }

            for (int i = 0; i < slope_rate.Length; i++)
            {
                slope_rate[i] = reader.ReadInt16();
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct EVENT_GROUP
    {
        [MarshalAs(UnmanagedType.U2)]
        public ushort on;	            /* the or of mask_rtn and mask_fault */
        [MarshalAs(UnmanagedType.U2)]
        public ushort flag_rtn;         /* set when in fault at end of time */
        [MarshalAs(UnmanagedType.U2)]
        public ushort mask_rtn;
        [MarshalAs(UnmanagedType.U2)]
        public ushort flag_fault;
        [MarshalAs(UnmanagedType.U2)]
        public ushort mask_fault;
        [MarshalAs(UnmanagedType.U2)]
        public ushort previous;
        [MarshalAs(UnmanagedType.U2)]
        public ushort dir_change;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct IDENTSTRING
    {
        [MarshalAs(UnmanagedType.I2)]
        public short length;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 163)]
        public string value;
    }

    // Since this structure includes dynamically sized arrays, we manually parse structure
    public struct BREAKER_TRIP_TIMES
    {
        public float[] breaker_trip_time;
        public float[] accumulated_breaker_trip_time;
        public float[] zero_trip_percent;
        public float[] zero_trip_time;

        public BREAKER_TRIP_TIMES(BinaryReader reader, int channels, int analogGroups)
        {
            breaker_trip_time = ReadArray(reader, channels, analogGroups);
            accumulated_breaker_trip_time = ReadArray(reader, channels, analogGroups);
            zero_trip_percent = ReadArray(reader, channels, analogGroups);
            zero_trip_time = ReadArray(reader, channels, analogGroups);
        }

        private static float[] ReadArray(BinaryReader reader, int channels, int analogGroups)
        {
            float[] floats = new float[channels * analogGroups];

            for (int i = 0; i < floats.Length; i++)
            {
                floats[i] = reader.ReadSingle();
            }

            return floats;
        }
    }

    public struct ANLG_CHNL_NEW
    {
        public string chanlnum;
        public string title;
        public string type;
        public string primary;
        public string secondary;
        public string cal_in;
        public string cal_ref;
        public string shunt;
        public string display_location;
        public string display_scale;
        public string limit_set;
        public string limit_on_off;
        public string over_set;
        public string over_on_off;
        public string under_set;
        public string under_on_off;
        public string rate_set;
        public string rate_on_off;
        public string mult_factor;
        public string ct_ratio;
        public string peak_cal;
        public string cr_lf;

        public int ChannelNumber
        {
            get
            {
                int number;

                if (!int.TryParse(chanlnum, out number))
                    return -1;

                return number - 1;  // Prefer zero based indexes
            }
        }

        public double ScalingFactor
        {
            get
            {
                double d_cal_in, d_cal_ref, d_secondary, d_primary;

                if (!double.TryParse(cal_in, out d_cal_in))
                    d_cal_in = 1.0D;

                if (!double.TryParse(cal_ref, out d_cal_ref) || d_cal_ref == 0.0D)
                    d_cal_ref = 1.0D;

                if (!double.TryParse(secondary, out d_secondary))
                    d_secondary = 1.0D;

                if (!double.TryParse(primary, out d_primary))
                    d_primary = 1.0D;

                return 5.0D * d_cal_in / d_cal_ref * d_primary / d_secondary;
            }
        }

        public override string ToString()
        {
            return title.ToNonNullString().Trim();
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct ANLG_CHNL_NEW1
    {
        private const int TYPE_LEN = 1;
        private const int TITLE_LEN = 20;
        private const int CHANLNUM_LEN = 2;
        private const int PRIMARY_LEN = 6;
        private const int SECONDARY_LEN = 4;
        private const int CT_RATIO_LEN = 4;
        private const int SHUNT_LEN = 4;
        private const int LIMIT_SET_LEN = 3;
        private const int SET_LEN = 3;
        private const int ON_OFF_LEN = 3;
        private const int SCALE_FACTOR_LEN = 2;
        private const int ZERO_LINE_LEN = 4;
        private const int MULT_FACT_LEN = 8;
        private const int CAL_LEN = 4;
        private const int PEAK_CAL_LEN = 4;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CHANLNUM_LEN + 1)]
        public string chanlnum;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = TITLE_LEN + 1)]
        public string title;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = TYPE_LEN + 1)]
        public string type;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = PRIMARY_LEN + 1)]
        public string primary;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SECONDARY_LEN + 1)]
        public string secondary;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CAL_LEN + 1)]
        public string cal_in;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CAL_LEN + 1)]
        public string cal_ref;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SHUNT_LEN + 1)]
        public string shunt;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ZERO_LINE_LEN + 1)]
        public string display_location;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SCALE_FACTOR_LEN + 1)]
        public string display_scale;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = LIMIT_SET_LEN + 1)]
        public string limit_set;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ON_OFF_LEN + 1)]
        public string limit_on_off;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SET_LEN + 1)]
        public string over_set;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ON_OFF_LEN + 1)]
        public string over_on_off;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SET_LEN + 1)]
        public string under_set;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ON_OFF_LEN + 1)]
        public string under_on_off;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SET_LEN + 1)]
        public string rate_set;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ON_OFF_LEN + 1)]
        public string rate_on_off;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MULT_FACT_LEN + 1)]
        public string mult_factor;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CT_RATIO_LEN + 1)]
        public string ct_ratio;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = PEAK_CAL_LEN + 1)]
        public string peak_cal;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 3)]
        public string cr_lf;

        public ANLG_CHNL_NEW ToAnlgChnlNew()
        {
            return new ANLG_CHNL_NEW()
            {
                chanlnum = chanlnum,
                title = title,
                type = type,
                primary = primary,
                secondary = secondary,
                cal_in = cal_in,
                cal_ref = cal_ref,
                shunt = shunt,
                display_location = display_location,
                display_scale = display_scale,
                limit_set = limit_set,
                limit_on_off = limit_on_off,
                over_set = over_set,
                over_on_off = over_on_off,
                under_set = under_set,
                under_on_off = under_on_off,
                rate_set = rate_set,
                rate_on_off = rate_on_off,
                mult_factor = mult_factor,
                ct_ratio = ct_ratio,
                peak_cal = peak_cal,
                cr_lf = cr_lf
            };
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct ANLG_CHNL_NEW2
    {
        private const int TYPE_LEN = 1;
        private const int TITLE_LEN = 64;
        private const int CHANLNUM_LEN = 2;
        private const int PRIMARY_LEN = 6;
        private const int SECONDARY_LEN = 4;
        private const int CT_RATIO_LEN = 4;
        private const int SHUNT_LEN = 4;
        private const int LIMIT_SET_LEN = 3;
        private const int SET_LEN = 3;
        private const int ON_OFF_LEN = 3;
        private const int SCALE_FACTOR_LEN = 2;
        private const int ZERO_LINE_LEN = 4;
        private const int MULT_FACT_LEN = 8;
        private const int CAL_LEN = 4;
        private const int PEAK_CAL_LEN = 4;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CHANLNUM_LEN + 1)]
        public string chanlnum;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = TITLE_LEN + 1)]
        public string title;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = TYPE_LEN + 1)]
        public string type;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = PRIMARY_LEN + 1)]
        public string primary;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SECONDARY_LEN + 1)]
        public string secondary;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CAL_LEN + 1)]
        public string cal_in;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CAL_LEN + 1)]
        public string cal_ref;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SHUNT_LEN + 1)]
        public string shunt;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ZERO_LINE_LEN + 1)]
        public string display_location;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SCALE_FACTOR_LEN + 1)]
        public string display_scale;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = LIMIT_SET_LEN + 1)]
        public string limit_set;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ON_OFF_LEN + 1)]
        public string limit_on_off;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SET_LEN + 1)]
        public string over_set;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ON_OFF_LEN + 1)]
        public string over_on_off;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SET_LEN + 1)]
        public string under_set;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ON_OFF_LEN + 1)]
        public string under_on_off;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SET_LEN + 1)]
        public string rate_set;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ON_OFF_LEN + 1)]
        public string rate_on_off;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MULT_FACT_LEN + 1)]
        public string mult_factor;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CT_RATIO_LEN + 1)]
        public string ct_ratio;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = PEAK_CAL_LEN + 1)]
        public string peak_cal;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 3)]
        public string cr_lf;

        public ANLG_CHNL_NEW ToAnlgChnlNew()
        {
            return new ANLG_CHNL_NEW()
            {
                chanlnum = chanlnum,
                title = title,
                type = type,
                primary = primary,
                secondary = secondary,
                cal_in = cal_in,
                cal_ref = cal_ref,
                shunt = shunt,
                display_location = display_location,
                display_scale = display_scale,
                limit_set = limit_set,
                limit_on_off = limit_on_off,
                over_set = over_set,
                over_on_off = over_on_off,
                under_set = under_set,
                under_on_off = under_on_off,
                rate_set = rate_set,
                rate_on_off = rate_on_off,
                mult_factor = mult_factor,
                ct_ratio = ct_ratio,
                peak_cal = peak_cal,
                cr_lf = cr_lf
            };
        }
    }

    public struct EVNT_CHNL_NEW
    {
        public string eventnum;
        public string e_title;
        public string nc_no;
        public string alarm_on_off;
        public string rtn_on_off;
        public ushort eveentdebouncems;

        public int EventNumber
        {
            get
            {
                int number;

                if (!int.TryParse(eventnum, out number))
                    return -1;

                return number - 1;  // Prefer zero based indexes
            }
        }

        public override string ToString()
        {
            return e_title.ToNonNullString().Trim();
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct EVNT_CHNL_NEW1
    {
        private const int EVENTNUM_LEN = 2;
        private const int E_TITLE_LEN = 20;
        private const int NC_NO_LEN = 2;
        private const int E_ON_OFF_LEN = 3;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = EVENTNUM_LEN + 1)]
        public string eventnum;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = E_TITLE_LEN + 1)]
        public string e_title;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NC_NO_LEN + 1)]
        public string nc_no;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = E_ON_OFF_LEN + 1)]
        public string alarm_on_off;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = E_ON_OFF_LEN + 1)]
        public string rtn_on_off;
        [MarshalAs(UnmanagedType.I2)]
        public ushort eveentdebouncems;

        public EVNT_CHNL_NEW ToEvntChnlNew()
        {
            return new EVNT_CHNL_NEW()
            {
                eventnum = eventnum,
                e_title = e_title,
                nc_no = nc_no,
                alarm_on_off = alarm_on_off,
                rtn_on_off = rtn_on_off,
                eveentdebouncems = eveentdebouncems
            };
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct EVNT_CHNL_NEW2
    {
        private const int EVENTNUM_LEN = 2;
        private const int E_TITLE_LEN = 64;
        private const int NC_NO_LEN = 2;
        private const int E_ON_OFF_LEN = 3;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = EVENTNUM_LEN + 1)]
        public string eventnum;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = E_TITLE_LEN + 1)]
        public string e_title;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NC_NO_LEN + 1)]
        public string nc_no;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = E_ON_OFF_LEN + 1)]
        public string alarm_on_off;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = E_ON_OFF_LEN + 1)]
        public string rtn_on_off;
        [MarshalAs(UnmanagedType.I2)]
        public ushort eveentdebouncems;

        public EVNT_CHNL_NEW ToEvntChnlNew()
        {
            return new EVNT_CHNL_NEW()
            {
                eventnum = eventnum,
                e_title = e_title,
                nc_no = nc_no,
                alarm_on_off = alarm_on_off,
                rtn_on_off = rtn_on_off,
                eveentdebouncems = eveentdebouncems
            };
        }
    }

    // Since this structure includes dynamically sized arrays, we manually parse structure
    public struct EVENT_DISPLAY
    {
        public short[] values;

        public EVENT_DISPLAY(BinaryReader reader, int eventGroups)
        {
            values = new short[eventGroups];

            for (int i = 0; i < values.Length; i++)
            {
                values[i] = reader.ReadInt16();
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SEQ_RSLTS
    {
        [MarshalAs(UnmanagedType.U2)]
        public ushort flag_seq;
        [MarshalAs(UnmanagedType.R4)]
        public float mag_zero_o;
        [MarshalAs(UnmanagedType.R4)]
        public float mag_pos_o;
        [MarshalAs(UnmanagedType.R4)]
        public float mag_pos_u;
        [MarshalAs(UnmanagedType.R4)]
        public float mag_neg_o;
        [MarshalAs(UnmanagedType.I2)]
        public short offset_zero_seq;
        [MarshalAs(UnmanagedType.I2)]
        public short offset_pos_seq_over;
        [MarshalAs(UnmanagedType.I2)]
        public short offset_pos_seq_under;
        [MarshalAs(UnmanagedType.I2)]
        public short offset_neg_seq;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SWING_RSLTS
    {
        [MarshalAs(UnmanagedType.U2)]
        public ushort s_flag_r;
        [MarshalAs(UnmanagedType.R4)]
        public float s_val_max0;
        [MarshalAs(UnmanagedType.R4)]
        public float s_val_min0;
        [MarshalAs(UnmanagedType.I2)]
        public short s_offset;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FREQUENCY_RSLTS
    {
        [MarshalAs(UnmanagedType.U2)]
        public ushort f_flag;
        [MarshalAs(UnmanagedType.R4)]
        public float f_result;
        [MarshalAs(UnmanagedType.I2)]
        public short f_offset;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct THD_RSLTS
    {
        [MarshalAs(UnmanagedType.U2)]
        public ushort thd_flag;
        [MarshalAs(UnmanagedType.R4)]
        public float thd_result;
        [MarshalAs(UnmanagedType.I2)]
        public short thd_offset;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TFreqRslts
    {
        [MarshalAs(UnmanagedType.U2)]
        public ushort f_flag;
        [MarshalAs(UnmanagedType.R4)]
        public float f_result;
        [MarshalAs(UnmanagedType.I2)]
        public short f_offset;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SENS_RSLTS
    {
        private const int SEQUENCE_GROUPS = 32;
        private const int SWING_CHANNELS_MAX = 1;
        private const int MAX_FREQ_CHNLS = 128;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SEQUENCE_GROUPS)]
        public SEQ_RSLTS[] seq_rslts;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SWING_CHANNELS_MAX)]
        public SWING_RSLTS[] swing_rslts;
        public FREQUENCY_RSLTS freq_rslts;
        public THD_RSLTS thd_rslts;
        [MarshalAs(UnmanagedType.I2)]
        public short vNFreqChnls;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_FREQ_CHNLS)]
        public TFreqRslts[] vFreqRslts;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct TPwrRslt
    {
        [MarshalAs(UnmanagedType.I2)]
        public short vLineGp;
        [MarshalAs(UnmanagedType.R4)]
        public float vPwr;
        [MarshalAs(UnmanagedType.I2)]
        public short vTrigFlg;
        [MarshalAs(UnmanagedType.I2)]
        public short vOfs;
        [MarshalAs(UnmanagedType.I2)]
        public short vBlkCnt;
        [MarshalAs(UnmanagedType.I2)]
        public short vIsLim;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TPwrRcd
    {
        private const int MAX_PWR_TRIGS = 16;

        [MarshalAs(UnmanagedType.I2)]
        public short vRcdSize;
        [MarshalAs(UnmanagedType.U2)]
        public ushort vRevId;
        [MarshalAs(UnmanagedType.I2)]
        public short vNPwrTrigs;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_PWR_TRIGS)]
        public TPwrRslt[] vaPwrRslt;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BoardAnalogEventChannels
    {
        [MarshalAs(UnmanagedType.I2)]
        public short analyze_board1_analog_channels;
        [MarshalAs(UnmanagedType.I2)]
        public short analyze_board2_analog_channels;
        [MarshalAs(UnmanagedType.I2)]
        public short analyze_board1_event_channels;
        [MarshalAs(UnmanagedType.I2)]
        public short analyze_board2_event_channels;
    }

    // Since this structure includes a dynamically sized string, we manually parse structure
    public struct A_SELECTION
    {
        public short size;
        public byte[] value;

        public A_SELECTION(BinaryReader reader)
        {
            size = reader.ReadInt16();
            value = reader.ReadBytes(size);
        }
    }

    // Since this structure includes a dynamically sized string, we manually parse structure
    public struct E_GRP_SELECT
    {
        public short size;
        public byte[] value;

        public E_GRP_SELECT(BinaryReader reader)
        {
            size = reader.ReadInt16();
            value = reader.ReadBytes(size);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PHASOR_GROUP
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public short[] values;       // channels Va, Vb, Vc, Ia, Ib, Ic, In in order 
    }

    // Since this structure includes a dynamically sized array, we manually parse structure
    public struct PHASOR_GROUPS
    {
        public short length;
        public PHASOR_GROUP[] groups;

        public PHASOR_GROUPS(BinaryReader reader)
        {
            length = reader.ReadInt16();
            groups = new PHASOR_GROUP[length / Marshal.SizeOf(typeof(PHASOR_GROUP))];

            for (int i = 0; i < groups.Length; i++)
            {
                groups[i] = reader.ReadStructure<PHASOR_GROUP>();
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct _complex
    {
        /* real and imaginary parts */
        [MarshalAs(UnmanagedType.R8)]
        public double x;
        [MarshalAs(UnmanagedType.R8)]
        public double y;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct LINE_CONSTANT
    {
        public _complex z0;
        public _complex z1;
        public _complex zaa;
        public _complex zbb;
        public _complex zcc;
        public _complex zab;
        public _complex zbc;
        public _complex zca;
        [MarshalAs(UnmanagedType.R4)]
        public float line_len;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 6)]
        public string units;
        [MarshalAs(UnmanagedType.R4)]
        public float fault_percent_of_full_scale;
    }

    // Since this structure includes a dynamically sized array, we manually parse structure
    public struct LINE_CONSTANTS
    {
        public short length;
        public LINE_CONSTANT[] constants;

        public LINE_CONSTANTS(BinaryReader reader)
        {
            length = reader.ReadInt16();
            constants = new LINE_CONSTANT[length / Marshal.SizeOf(typeof(LINE_CONSTANT))];

            for (int i = 0; i < constants.Length; i++)
            {
                constants[i] = reader.ReadStructure<LINE_CONSTANT>();
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct LINE_NAME
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string value;
    }

    // Since this structure includes a dynamically sized array, we manually parse structure
    public struct LINE_NAMES
    {
        public short length;
        public LINE_NAME[] names;

        public LINE_NAMES(BinaryReader reader)
        {
            length = reader.ReadInt16();
            names = new LINE_NAME[length / Marshal.SizeOf(typeof(LINE_NAME))];

            for (int i = 0; i < names.Length; i++)
            {
                names[i] = reader.ReadStructure<LINE_NAME>();
            }
        }
    }

    [Flags]
    public enum FAULT_LIST_FLAGS : ushort
    {
        MASK_UNDER = 0x0010,
        MASK_OVER = 0x0020,
        MASK_SLOPE = 0x0040,    // (Rate)
        MASK_LIMIT = 0x0080,
        MASK_FAULT = 0x0001,
        MSK_ZERO = 0x0100,
        MSK_POS_OVER = 0x0200,
        MSK_POS_UNDER = 0x0400,
        MSK_NEG = 0x0800,
        MSK_FREQ = 0x1000,
        MSK_HARM = 0x2000,
        MSK_SWING = 0x4000,
        MSK_PWR = 0x8000
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FAULT_LIST
    {
        [MarshalAs(UnmanagedType.R4)]
        public float fvalue;
        [MarshalAs(UnmanagedType.U2)]
        public FAULT_LIST_FLAGS flag;

        /*  chanlnum represents analog channel for over,under
            slope and frequency.
            For sequence triggers, chanlnum = sequence group – 1.
            For power triggers, chanlnum = line group – 1.
            For event triggers, chanlnum = -event channel.
       */
        [MarshalAs(UnmanagedType.I2)]
        public short chanlnum;
        [MarshalAs(UnmanagedType.I2)]
        public short duration_sec;
        [MarshalAs(UnmanagedType.I2)]
        public short duration_mS;
        [MarshalAs(UnmanagedType.I2)]
        public short fault_sec;
        [MarshalAs(UnmanagedType.I2)]
        public short fault_mS;
        [MarshalAs(UnmanagedType.I4)]
        public int rms_offset;
        [MarshalAs(UnmanagedType.R4)]
        public float rms_value;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FAULT_LOCATION
    {
        [MarshalAs(UnmanagedType.I2)]
        public short type;
        [MarshalAs(UnmanagedType.R4)]
        public float k;
    };

    // Since this structure includes a dynamically sized array, we manually parse structure
    public struct FAULT_LOCATIONS
    {
        public short length;
        public FAULT_LOCATION[] locations;

        public FAULT_LOCATIONS(BinaryReader reader)
        {
            length = reader.ReadInt16();
            locations = new FAULT_LOCATION[length / Marshal.SizeOf(typeof(FAULT_LOCATION))];

            for (int i = 0; i < locations.Length; i++)
            {
                locations[i] = reader.ReadStructure<FAULT_LOCATION>();
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SEQUENCE_CHANNEL
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public short[] values;
    }

    // Since this structure includes a dynamically sized array, we manually parse structure
    public struct SEQUENCE_CHANNELS
    {
        public short length;
        public SEQUENCE_CHANNEL[] channels;

        public SEQUENCE_CHANNELS(BinaryReader reader)
        {
            length = reader.ReadInt16();
            channels = new SEQUENCE_CHANNEL[length / Marshal.SizeOf(typeof(SEQUENCE_CHANNEL))];

            for (int i = 0; i < channels.Length; i++)
            {
                channels[i] = reader.ReadStructure<SEQUENCE_CHANNEL>();
            }
        }
    }

    public static class ControlFileStructureExtensions
    {
        public static TimeZoneInfo GetTimeZoneInfo(this SYSTEM_PARAMETERS systemParameters)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(systemParameters.time_zone_information.StandardName);
            }
            catch
            {
                return TimeZoneInfo.Local;
            }
        }
    }
}
