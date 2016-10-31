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
using GSF.Collections;
using GSF.PhasorProtocols;
using GSF.Units.EE;

namespace GSF.COMTRADE
{
    /// <summary>
    /// Represents an analog channel definition of the <see cref="Schema"/>.
    /// </summary>
    public class AnalogChannel
    {
        #region [ Members ]

        // Fields
        private int m_index;
        private string m_stationName;
        private string m_channelName;
        private char m_phaseDesignation;
        private SignalKind m_signalKind;
        private PhasorType m_phasorType;
        private double m_nominalFrequency;
        private CoordinateFormat m_coordinateFormat;
        private string m_circuitComponent;
        private string m_units;
        private double m_multipler;
        private double m_adder;
        private double m_skew;
        private int m_minValue;
        private int m_maxValue;
        private double m_primaryRatio;
        private double m_secondaryRatio;
        private char m_scalingIdentifier;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="AnalogChannel"/>.
        /// </summary>
        public AnalogChannel()
        {
            m_phaseDesignation = char.MinValue;
            m_signalKind = SignalKind.Analog;
            m_coordinateFormat = CoordinateFormat.Polar;
            m_multipler = 0.04;
            m_adder = 0.0;
            m_nominalFrequency = 60.0D;
            m_minValue = -99999;
            m_maxValue = 99998;
            m_primaryRatio = 1.0;
            m_secondaryRatio = 1.0;
            m_scalingIdentifier = 'P';
        }

        /// <summary>
        /// Creates a new instance of the <see cref="AnalogChannel"/> from an existing line image.
        /// </summary>
        /// <param name="lineImage">Line image to parse.</param>
        public AnalogChannel(string lineImage)
        {
            // An,ch_id,ph,ccbm,uu,a,b,skew,min,max,primary,secondary,PS
            string[] parts = lineImage.Split(',');

            if(parts.Length == 13)
            {
                Index = int.Parse(parts[0].Trim());
                Name = parts[1];
                PhaseID = parts[2];
                CircuitComponent = parts[3];
                Units = parts[4];
                Multiplier = double.Parse(parts[5].Trim());
                Adder = double.Parse(parts[6].Trim());
                Skew = double.Parse(parts[7].Trim());
                MinValue = int.Parse(parts[8].Trim());
                MaxValue = int.Parse(parts[9].Trim());
                PrimaryRatio = double.Parse(parts[10].Trim());
                SecondaryRatio = double.Parse(parts[11].Trim());
                ScalingIdentifier = parts[12].Trim()[0];
            }
            else if(parts.Length == 10)
            {
                Index = int.Parse(parts[0].Trim());
                Name = parts[1];
                PhaseID = parts[2];
                CircuitComponent = parts[3];
                Units = parts[4];
                Multiplier = double.Parse(parts[5].Trim());
                Adder = double.Parse(parts[6].Trim());
                Skew = double.Parse(parts[7].Trim());
                MinValue = int.Parse(parts[8].Trim());
                MaxValue = int.Parse(parts[9].Trim());
                //PrimaryRatio = 1;
                //SecondaryRatio = 1;
                //ScalingIdentifier = parts[12].Trim()[0];


            }
            else
                throw new InvalidOperationException(string.Format("Unexpected number of line image elements for analog channel definition: {0} - expected 13\r\nImage = {1}", parts.Length, lineImage));

        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets index of this <see cref="AnalogChannel"/>.
        /// </summary>
        public int Index
        {
            get
            {
                return m_index;
            }
            set
            {
                m_index = value;
            }
        }

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
                    return string.Format("{0}:{1}", m_stationName, m_channelName);

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
                            if (m_coordinateFormat == CoordinateFormat.Rectangular)
                                return m_phaseDesignation + "r";

                            return m_phaseDesignation + "m";
                        }
                        break;
                    case SignalKind.Angle:
                        if (m_phaseDesignation != char.MinValue)
                        {
                            if (m_coordinateFormat == CoordinateFormat.Rectangular)
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
                    this.SignalKind = SignalKind.Analog;
                    m_coordinateFormat = CoordinateFormat.Polar;
                }
                else
                {
                    if (string.Compare(value, "F", true) == 0)
                    {
                        m_phaseDesignation = char.MinValue;
                        this.SignalKind = SignalKind.Frequency;
                        m_coordinateFormat = CoordinateFormat.Polar;
                    }
                    else if (string.Compare(value, "df", true) == 0)
                    {
                        m_phaseDesignation = char.MinValue;
                        this.SignalKind = SignalKind.DfDt;
                        m_coordinateFormat = CoordinateFormat.Polar;
                    }
                    else if (value.Length > 1)
                    {
                        this.PhaseDesignation = value[0].ToString();
                        char component = char.ToLower(value[1]);

                        switch (component)
                        {
                            case 'r':
                                this.SignalKind = SignalKind.Magnitude;
                                m_coordinateFormat = CoordinateFormat.Rectangular;
                                break;
                            case 'i':
                                this.SignalKind = SignalKind.Angle;
                                m_coordinateFormat = CoordinateFormat.Rectangular;
                                break;
                            case 'm':
                                this.SignalKind = SignalKind.Magnitude;
                                m_coordinateFormat = CoordinateFormat.Polar;
                                break;
                            case 'a':
                                this.SignalKind = SignalKind.Angle;
                                m_coordinateFormat = CoordinateFormat.Polar;
                                break;
                        }
                    }
                    else
                    {
                        this.PhaseDesignation = value[0].ToString();
                        this.SignalKind = SignalKind.Analog;
                        m_coordinateFormat = CoordinateFormat.Polar;
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
        public PhasorType PhasorType
        {
            get
            {
                return m_phasorType;
            }
            set
            {
                m_phasorType = value;
            }
        }

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
                    m_adder = (double)m_nominalFrequency;
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
                        m_multipler = 0.006;
                        m_adder = 0.0;
                        break;
                    case SignalKind.Magnitude:
                        if (m_phasorType == PhasorType.Current)
                            m_multipler = 0.4;
                        else
                            m_multipler = 0.04;
                        m_adder = 0.0;
                        break;
                    case SignalKind.Frequency:
                        m_multipler = 0.001;
                        m_adder = (double)m_nominalFrequency;
                        break;
                    case SignalKind.DfDt:
                        m_multipler = 0.01;
                        m_adder = 0.0;
                        break;
                    default:
                        m_multipler = 0.04;
                        m_adder = 0.0;
                        break;
                }
            }
        }

        /// <summary>
        /// Gets or sets coordinate format of this <see cref="AnalogChannel"/>.
        /// </summary>
        public CoordinateFormat CoordinateFormat
        {
            get
            {
                return m_coordinateFormat;
            }
            set
            {
                m_coordinateFormat = value;
            }
        }

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
                if (!string.IsNullOrWhiteSpace(value))
                    m_units = value.Trim();
                else
                    m_units = "";

                if (m_units.Length > 32)
                    m_units = m_units.Substring(0, 32);
            }
        }

        /// <summary>
        /// Gets or sets value multipler of this <see cref="AnalogChannel"/>.
        /// </summary>
        public double Multiplier
        {
            get
            {
                return m_multipler;
            }
            set
            {
                m_multipler = value;
            }
        }

        /// <summary>
        /// Gets or sets adder of this <see cref="AnalogChannel"/>.
        /// </summary>
        public double Adder
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
        /// Gets or sets time skew between channels of this <see cref="AnalogChannel"/>.
        /// </summary>
        public double Skew
        {
            get
            {
                return m_skew;
            }
            set
            {
                m_skew = value;
            }
        }

        /// <summary>
        /// Gets or sets minimum unscaled value of this <see cref="AnalogChannel"/>.
        /// </summary>
        public int MinValue
        {
            get
            {
                return m_minValue;
            }
            set
            {
                m_minValue = value;
            }
        }

        /// <summary>
        /// Gets or sets maximum unscaled value of this <see cref="AnalogChannel"/>.
        /// </summary>
        public int MaxValue
        {
            get
            {
                return m_maxValue;
            }
            set
            {
                m_maxValue = value;
            }
        }

        /// <summary>
        /// Gets or sets the channel voltage or current transformer ratio primary factor of this <see cref="AnalogChannel"/>.
        /// </summary>
        public double PrimaryRatio
        {
            get
            {
                return m_primaryRatio;
            }
            set
            {
                m_primaryRatio = value;
            }
        }

        /// <summary>
        /// Gets or sets the channel voltage or current transformer ratio secondary factor of this <see cref="AnalogChannel"/>.
        /// </summary>
        public double SecondaryRatio
        {
            get
            {
                return m_secondaryRatio;
            }
            set
            {
                m_secondaryRatio = value;
            }
        }

        /// <summary>
        /// Gets or sets the the primary or secondary data scaling identifer of this <see cref="AnalogChannel"/>.
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
                    throw new ArgumentException(value + " is not a valid primary or secondary data scaling identifer - must be either \'P\' or \'S\'.");

                m_scalingIdentifier = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Converts <see cref="AnalogChannel"/> to its string format.
        /// </summary>
        public override string ToString()
        {
            string[] values = new string[13];

            // An,ch_id,ph,ccbm,uu,a,b,skew,min,max,primary,secondary,PS
            values[0] = Index.ToString();
            values[1] = Name;
            values[2] = PhaseID;
            values[3] = CircuitComponent;
            values[4] = Units;
            values[5] = Multiplier.ToString();
            values[6] = Adder.ToString();
            values[7] = Skew.ToString();
            values[8] = MinValue.ToString();
            values[9] = MaxValue.ToString();
            values[10] = PrimaryRatio.ToString();
            values[11] = SecondaryRatio.ToString();
            values[12] = ScalingIdentifier.ToString();

            return values.ToDelimitedString(',');
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
