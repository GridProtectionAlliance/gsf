//******************************************************************************************************
//  AnalogChannel.cs - Gbtc
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
//  05/19/2011 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
using GSF.PhasorProtocols;
using GSF.Units.EE;
using Newtonsoft.Json;

namespace GSF.COMTRADE
{
    /// <summary>
    /// Represents an analog channel definition of the <see cref="Schema"/>.
    /// </summary>
    public class AnalogChannel
    {
        #region [ Members ]

        // Fields
        private readonly int m_version;
        private string m_stationName;
        private string m_channelName;
        private char m_phaseDesignation;
        private SignalKind m_signalKind;
        private double m_nominalFrequency;
        private string m_circuitComponent;
        private string m_units;
        private char m_scalingIdentifier;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="AnalogChannel"/>.
        /// </summary>
        /// <param name="version">Target schema version.</param>
        public AnalogChannel(int version = 1999)
        {
            m_version = version;
            m_phaseDesignation = char.MinValue;
            m_signalKind = SignalKind.Analog;
            CoordinateFormat = CoordinateFormat.Polar;
            Multiplier = 0.04D;
            Adder = 0.0D;
            m_nominalFrequency = 60.0D;
            MinValue = -99999;
            MaxValue = 99998;
            PrimaryRatio = 1.0D;
            SecondaryRatio = 1.0D;
            m_scalingIdentifier = 'P';
        }

        /// <summary>
        /// Creates a new instance of the <see cref="AnalogChannel"/> from an existing line image.
        /// </summary>
        /// <param name="lineImage">Line image to parse.</param>
        /// <param name="version">Target schema version.</param>
        public AnalogChannel(string lineImage, int version = 1999)
        {
            // An,ch_id,ph,ccbm,uu,a,b,skew,min,max,primary,secondary,PS
            string[] parts = lineImage.Split(',');

            m_version = version;

            if (parts.Length == 10 || parts.Length == 13)
            {               
                Index = int.Parse(parts[0].Trim());
                Name = parts[1];
                PhaseID = parts[2];
                CircuitComponent = parts[3];
                Units = parts[4];
                Multiplier = double.Parse(parts[5].Trim());
                Adder = double.Parse(parts[6].Trim());
                Skew = double.Parse(parts[7].Trim());
                MinValue = double.Parse(parts[8].Trim());
                MaxValue = double.Parse(parts[9].Trim());

                if (parts.Length == 13)
                {
                    PrimaryRatio = double.Parse(parts[10].Trim());
                    SecondaryRatio = double.Parse(parts[11].Trim());
                    ScalingIdentifier = parts[12].Trim()[0];
                }
            }
            else
            {
                throw new InvalidOperationException($"Unexpected number of line image elements for analog channel definition: {parts.Length} - expected 10 or 13{Environment.NewLine}Image = {lineImage}");
            }

        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets index of this <see cref="AnalogChannel"/>.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets name of this <see cref="AnalogChannel"/> formatted as station_name:channel_name.
        /// </summary>
        /// <exception cref="FormatException">Name must be formatted as station_name:channel_name.</exception>
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(m_stationName))
                    return null;

                if (!string.IsNullOrEmpty(m_channelName))
                    return $"{m_stationName}:{m_channelName}";

                return m_stationName;
            }
            set
            {
                string[] parts = value.Split(':');

                if (parts.Length == 2)
                {
                    m_stationName = parts[0].Trim();
                    m_channelName = parts[1].Trim();
                }
                else
                {
                    m_stationName = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets station name component of this <see cref="AnalogChannel"/>.
        /// </summary>
        public string StationName
        {
            get
            {
                return m_stationName;
            }
            set
            {
                m_stationName = value.Replace(":", "_").Trim();
            }
        }

        /// <summary>
        /// Gets or sets channel name component of this <see cref="AnalogChannel"/>.
        /// </summary>
        public string ChannelName
        {
            get
            {
                return m_channelName;
            }
            set
            {
                m_channelName = value.Replace(":", "_").Trim();
            }
        }

        /// <summary>
        /// Gets or sets the 2-character phase identifier for this <see cref="AnalogChannel"/>.
        /// </summary>
        public string PhaseID
        {
            get
            {
                switch (m_signalKind)
                {
                    case SignalKind.Magnitude:
                        if (m_phaseDesignation != char.MinValue)
                        {
                            if (CoordinateFormat == CoordinateFormat.Rectangular)
                                return m_phaseDesignation + "r";

                            return m_phaseDesignation + "m";
                        }
                        break;
                    case SignalKind.Angle:
                        if (m_phaseDesignation != char.MinValue)
                        {
                            if (CoordinateFormat == CoordinateFormat.Rectangular)
                                return m_phaseDesignation + "i";

                            return m_phaseDesignation + "a";
                        }
                        break;
                    case SignalKind.Frequency:
                        return "F";
                    case SignalKind.DfDt:
                        return "df";
                }

                return PhaseDesignation;
            }
            set
            {
                value = value.Trim();

                if (string.IsNullOrEmpty(value))
                {
                    m_phaseDesignation = char.MinValue;
                    SignalKind = SignalKind.Analog;
                    CoordinateFormat = CoordinateFormat.Polar;
                }
                else
                {
                    if (string.Compare(value, "F", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        m_phaseDesignation = char.MinValue;
                        SignalKind = SignalKind.Frequency;
                        CoordinateFormat = CoordinateFormat.Polar;
                    }
                    else if (string.Compare(value, "df", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        m_phaseDesignation = char.MinValue;
                        SignalKind = SignalKind.DfDt;
                        CoordinateFormat = CoordinateFormat.Polar;
                    }
                    else if (value.Length > 1)
                    {
                        PhaseDesignation = value[0].ToString();
                        char component = char.ToLowerInvariant(value[1]);

                        switch (component)
                        {
                            case 'r':
                                SignalKind = SignalKind.Magnitude;
                                CoordinateFormat = CoordinateFormat.Rectangular;
                                break;
                            case 'i':
                                SignalKind = SignalKind.Angle;
                                CoordinateFormat = CoordinateFormat.Rectangular;
                                break;
                            case 'm':
                                SignalKind = SignalKind.Magnitude;
                                CoordinateFormat = CoordinateFormat.Polar;
                                break;
                            case 'a':
                                SignalKind = SignalKind.Angle;
                                CoordinateFormat = CoordinateFormat.Polar;
                                break;
                        }
                    }
                    else
                    {
                        PhaseDesignation = value[0].ToString();
                        SignalKind = SignalKind.Analog;
                        CoordinateFormat = CoordinateFormat.Polar;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets phase designation of this <see cref="AnalogChannel"/>.
        /// </summary>
        /// <exception cref="ArgumentException">Value is not a valid phase designation.</exception>
        public string PhaseDesignation
        {
            get
            {
                return m_phaseDesignation.ToString().RemoveNull();
            }
            set
            {
                value = value.Trim();

                if (string.IsNullOrEmpty(value))
                {
                    m_phaseDesignation = char.MinValue;
                }
                else
                {
                    char phaseDesignation = char.ToUpper(value[0]);

                    switch (phaseDesignation)
                    {
                        case 'A':
                        case 'R':
                        case '1':
                            m_phaseDesignation = 'A';
                            break;
                        case 'B':
                        case 'S':
                        case '2':
                            m_phaseDesignation = 'B';
                            break;
                        case 'C':
                        case 'T':
                        case '3':
                            m_phaseDesignation = 'C';
                            break;
                        case 'P':
                        case '+':
                            m_phaseDesignation = 'P';
                            break;
                        case 'N':
                        case '-':
                            m_phaseDesignation = '-';
                            break;
                        case 'Z':
                        case '0':
                            m_phaseDesignation = '0';
                            break;
                        default:
                            m_phaseDesignation = char.MinValue;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets phasor type of this <see cref="AnalogChannel"/>, if applicable.
        /// </summary>
        public PhasorType PhasorType { get; set; }

        /// <summary>
        /// Gets or sets nominal frequency of this <see cref="AnalogChannel"/>.
        /// </summary>
        public double NominalFrequency
        {
            get
            {
                return m_nominalFrequency;
            }
            set
            {
                m_nominalFrequency = value;

                if (m_signalKind == SignalKind.Frequency)
                    Adder = (double)m_nominalFrequency;
            }
        }

        /// <summary>
        /// Gets or sets signal kind of this <see cref="AnalogChannel"/>.
        /// </summary>
        /// <exception cref="ArgumentException">Value is not a valid analog signal kind.</exception>
        public SignalKind SignalKind
        {
            get
            {
                return m_signalKind;
            }
            set
            {
                if (s_validAnalogSignalKinds.BinarySearch(value) < 0)
                    throw new ArgumentException(value + " is not a valid analog signal kind.");

                m_signalKind = value;

                switch (m_signalKind)
                {
                    case SignalKind.Angle:
                        Multiplier = 0.006D;
                        Adder = 0.0D;
                        break;
                    case SignalKind.Magnitude:
                        if (PhasorType == PhasorType.Current)
                            Multiplier = 0.4D;
                        else
                            Multiplier = 0.04D;
                        Adder = 0.0D;
                        break;
                    case SignalKind.Frequency:
                        Multiplier = 0.001D;
                        Adder = m_nominalFrequency;
                        break;
                    case SignalKind.DfDt:
                        Multiplier = 0.01D;
                        Adder = 0.0D;
                        break;
                    default:
                        Multiplier = 0.04D;
                        Adder = 0.0D;
                        break;
                }
            }
        }

        /// <summary>
        /// Gets or sets coordinate format of this <see cref="AnalogChannel"/>.
        /// </summary>
        public CoordinateFormat CoordinateFormat { get; set; }

        /// <summary>
        /// Gets or sets circuit component of this <see cref="AnalogChannel"/>.
        /// </summary>
        public string CircuitComponent
        {
            get
            {
                return m_circuitComponent;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                    m_circuitComponent = value.Trim();
                else
                    m_circuitComponent = "";

                if (m_circuitComponent.Length > 64)
                    m_circuitComponent = m_circuitComponent.Substring(0, 64);
            }
        }

        /// <summary>
        /// Gets or sets units of this <see cref="AnalogChannel"/>.
        /// </summary>
        public string Units
        {
            get
            {
                return m_units;
            }
            set
            {
                m_units = string.IsNullOrWhiteSpace(value) ? "" : value.Trim();

                if (m_units.Length > 32)
                    m_units = m_units.Substring(0, 32);
            }
        }

        /// <summary>
        /// Gets or sets value multiplier of this <see cref="AnalogChannel"/>.
        /// </summary>
        public double Multiplier { get; set; }

        /// <summary>
        /// Gets or sets adder of this <see cref="AnalogChannel"/>.
        /// </summary>
        public double Adder { get; set; }

        /// <summary>
        /// Gets or sets time skew between channels of this <see cref="AnalogChannel"/>.
        /// </summary>
        public double Skew { get; set; }

        /// <summary>
        /// Gets or sets minimum unscaled value of this <see cref="AnalogChannel"/>.
        /// </summary>
        public double MinValue { get; set; }

        /// <summary>
        /// Gets or sets maximum unscaled value of this <see cref="AnalogChannel"/>.
        /// </summary>
        public double MaxValue { get; set; }

        /// <summary>
        /// Gets or sets the channel voltage or current transformer ratio primary factor of this <see cref="AnalogChannel"/>.
        /// </summary>
        public double PrimaryRatio { get; set; }

        /// <summary>
        /// Gets or sets the channel voltage or current transformer ratio secondary factor of this <see cref="AnalogChannel"/>.
        /// </summary>
        public double SecondaryRatio { get; set; }

        /// <summary>
        /// Gets or sets the the primary or secondary data scaling identifier of this <see cref="AnalogChannel"/>.
        /// </summary>
        public char ScalingIdentifier
        {
            get
            {
                return m_scalingIdentifier;
            }
            set
            {
                value = char.ToUpper(value);

                if (value != 'P' && value != 'S')
                    throw new ArgumentException(value + " is not a valid primary or secondary data scaling identifier - must be either \'P\' or \'S\'.");

                m_scalingIdentifier = value;
            }
        }

        /// <summary>
        /// Gets target schema version.
        /// </summary>
        [JsonIgnore]
        public int Version => m_version;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Converts <see cref="AnalogChannel"/> to its string format.
        /// </summary>
        public override string ToString()
        {
            // An,ch_id,ph,ccbm,uu,a,b,skew,min,max
            List<string> values = new List<string>
            {
                Index.ToString(),
                Name,
                PhaseID,
                CircuitComponent,
                Units,
                Multiplier.ToString(CultureInfo.InvariantCulture),
                Adder.ToString(CultureInfo.InvariantCulture),
                Skew.ToString(CultureInfo.InvariantCulture),
                MinValue.ToString(CultureInfo.InvariantCulture),
                MaxValue.ToString(CultureInfo.InvariantCulture)
            };

            // ...,primary,secondary,PS
            if (m_version >= 1999)
            {
                values.Add(PrimaryRatio.ToString(CultureInfo.InvariantCulture));
                values.Add(SecondaryRatio.ToString(CultureInfo.InvariantCulture));
                values.Add(ScalingIdentifier.ToString());
            }

            return string.Join(",", values);
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly List<SignalKind> s_validAnalogSignalKinds;

        // Static Constructor
        static AnalogChannel()
        {
            s_validAnalogSignalKinds = new List<SignalKind>(new[] { SignalKind.Analog, SignalKind.Angle, SignalKind.Calculation, SignalKind.DfDt, SignalKind.Frequency, SignalKind.Magnitude, SignalKind.Statistic });
            s_validAnalogSignalKinds.Sort();
        }

        #endregion
    }
}
