#region [ Modification History ]
/*
 * 11/01/2012 Denis Kholine
 *  Generated Original version of source code.
 */
#endregion

#region  [ UIUC NCSA Open Source License ]
/*
Copyright © <2012> <University of Illinois>
All rights reserved.

Developed by: <ITI>
<University of Illinois>
<http://www.iti.illinois.edu/>
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal with the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
• Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimers.
• Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimers in the documentation and/or other materials provided with the distribution.
• Neither the names of <Name of Development Group, Name of Institution>, nor the names of its contributors may be used to endorse or promote products derived from this Software without specific prior written permission.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE CONTRIBUTORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS WITH THE SOFTWARE.
*/
#endregion

#region [ Using ]
using System;
using GSF.TestsSuite.TimeSeries.Cases;
using GSF.PhasorProtocols.IEEEC37_118;
using GSF.PhasorProtocols;
using GSF;

using PhasorProtocolAdapters.IeeeC37_118;
#endregion

namespace GSF.TestsSuite.PhasorProtocols.Cases.IEEE.C37118
{
    public class ConfigurationCellCase
    {
        #region [ Fields ]
        private ushort idCode = 1;
        private Ticks timestamp = DateTime.Parse("2012-11-05 16:27:00.000");
        private ushort frameRate = 30;
        private string StationName = "PMU.001";
        private double lagTime = 1000;
        private double leadTime =1000;
        private string dataChannel =";dataChannel={port=8888; Clients=localhost:8989; Interface=0.0.0.0}";
        private string commandChannel = "commandChannel";
        #endregion

        #region [ Members ]
        /// <summary>
        /// Typical data stream synchrnonization byte.
        /// </summary>
        private const byte SyncByte = 0xAA;
        private IConfigurationCell m_IConfigurationCell;
        private IConfigurationFrame m_IConfigurationFrame;
        private AnalogDefinition m_AnalogDefinition;
        private FrequencyDefinition m_FrequencyDefinition;
        private AnalogValue m_AnalogValue;
        private FrequencyValue m_FrequencyValue;
        private PhasorDefinition m_PhasorDefinition;
        private DigitalDefinition m_DigitalDefinition;
        private ConfigurationFrame1 m_ConfigurationFrame1;
        private ConfigurationFrame2 m_ConfigurationFrame2;
        private CommonFrameHeader m_CommonFrameHeader;
        private PhasorValue m_PhasorValue;
        private global::GSF.PhasorProtocols.Anonymous.ConfigurationFrame m_ConfigurationFrame;
        private DigitalValue m_DigitalValue;
        private DataCell m_DataCell;
        private DataFrame m_DataFrame;
        private Concentrator m_Concentrator;
        private IMeasurementCase m_IMeasurementCase;
        private IMeasurementsCase m_IMeasurementsCase;
        private IAllAdaptersCase m_IAllAdaptersCase;
        private PhasorValueCollection m_PhasorValueCollection;
        private AnalogValueCollection m_AnalogValueCollection;
        private DigitalValueCollection m_DigitalValueCollection;
        #endregion

        #region [ Properties ]
        /// <summary>
        /// Test values for PhasorValueCollection
        /// </summary>
        public PhasorValueCollection PhasorValueCollection
        {
            get
            {
                return m_PhasorValueCollection;
            }
        }
        /// <summary>
        /// Test values for AnalogValueCollection
        /// </summary>
        public AnalogValueCollection AnalogValueCollection
        {
            get
            {
                return m_AnalogValueCollection;
            }
        }
        /// <summary>
        /// Test values for DigitalValueCollection collection
        /// </summary>
        public DigitalValueCollection DigitalValueCollection
        {
            get
            {
                return m_DigitalValueCollection;
            }
        }
        /// <summary>
        /// This is basic connection string for concentrator originated from ActionAdapterBase abstract class.
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return "IDCode="+idCode.ToString()+dataChannel+";framesPerSecond=" + frameRate.ToString() + ";lagTime=" + lagTime.ToString() + ";leadTime=" + leadTime.ToString();
            }
        }
        public double LagTime
        {
            get
            {
                return lagTime;
            }
        }
        public double LeadTime
        {
            get
            {
                return leadTime;
            }
        }
        public ushort FrameRate
        {
            get
            {
                return frameRate;
            }
        }
        public Ticks TimeStamp
        {
            get
            {
                return timestamp;
            }
        }
        public DataCell DataCell
        {
            get
            {
                return m_DataCell;
            }
        }
        public DataFrame DataFrame
        {
            get
            {
                return m_DataFrame;
            }
        }
        public global::GSF.PhasorProtocols.Anonymous.ConfigurationFrame ConfigurationFrame
        {
            get
            {
                return m_ConfigurationFrame;
            }
        }
        public PhasorValue PhasorValue
        {
            get
            {
                return m_PhasorValue;
            }
        }
        public ConfigurationCell ConfigurationCell
        {
            get
            {
                return (ConfigurationCell)m_IConfigurationCell;
            }
        }
        public IConfigurationFrame ConfigurationFrame2
        {
            get
            {
                return m_IConfigurationFrame;
            }
        }
        public AnalogDefinition AnalogDefinition
        {
            get
            {
                return m_AnalogDefinition;
            }
        }
        public AnalogValue AnalogValue
        {
            get
            {
                return m_AnalogValue;
            }
        }
        public FrequencyDefinition FrequencyDefinition
        {
            get
            {
                return m_FrequencyDefinition;
            }
        }
        public PhasorDefinition PhasorDefinition
        {
            get
            {
                return m_PhasorDefinition;
            }
        }
        public DigitalDefinition DigitalDefinition
        {
            get
            {
                return m_DigitalDefinition;
            }
        }
        public ConfigurationFrame1 ConfigurationFrame1
        {
            get
            {
                return m_ConfigurationFrame1;
            }
        }
        public CommonFrameHeader CommonFrameHeader
        {
            get
            {
                return m_CommonFrameHeader;

            }
        }
        public DigitalValue DigitalValue
        {
            get
            {
                return m_DigitalValue;
            }
        }

        public FrequencyValue FrequencyValue
        {
            get
            {
                return m_FrequencyValue;
            }
        }
        public Concentrator Concentrator
        {
            get
            {
                return m_Concentrator;
            }
        }
        #endregion

        #region [ Constructors ]
        public ConfigurationCellCase()
        {
            #region [ Adapters ]
            m_IAllAdaptersCase = new IAllAdaptersCase();
            #endregion

            #region [ Measurements  ]
            m_IMeasurementCase = new IMeasurementCase();
            m_IMeasurementsCase = new IMeasurementsCase();
            #endregion

            #region [ Concentrator ]
            m_Concentrator = new Concentrator();
            m_Concentrator.ConnectionString = ConnectionString;
            m_Concentrator.FramesPerSecond = frameRate;
            m_Concentrator.LagTime = lagTime;
            m_Concentrator.LeadTime = leadTime;
            //m_Concentrator.Initialize();
            // Using test measurement keys for concentrator.
            //m_Concentrator.InputMeasurementKeys = m_IMeasurementsCase.MeasurementKeys;
            //m_Concentrator.RequestedOutputMeasurementKeys = m_IMeasurementsCase.MeasurementKeys;
            // Fill Concentrator with data source
            m_Concentrator.DataSource = m_IAllAdaptersCase.ActionAdapter.DataSource;
            m_Concentrator.Initialize();
            #endregion

            #region [ ConfigurationFrame Anonymous ]
            m_ConfigurationFrame = new global::GSF.PhasorProtocols.Anonymous.ConfigurationFrame(idCode, timestamp, frameRate);
            #endregion

            #region [ ConfigurationFrame1 ]
            byte[] buffer = new byte[256];
            int startIndex = 0;
            buffer[startIndex] = SyncByte;
            m_ConfigurationFrame1 = new ConfigurationFrame1();
            m_ConfigurationFrame1.Timebase = 10;
            m_ConfigurationFrame1.IDCode = 1;
            m_ConfigurationFrame1.FrameRate = 30;
            m_CommonFrameHeader = new CommonFrameHeader(m_ConfigurationFrame1, buffer, startIndex);
            m_CommonFrameHeader.TypeID = FrameType.CommandFrame; //TODO Required for correct synchronization
            m_ConfigurationFrame1.CommonHeader = m_CommonFrameHeader;
            #endregion

            #region [ ConfigurationFrame2 ]
            m_ConfigurationFrame2 = new ConfigurationFrame2();
            m_ConfigurationFrame2.IDCode = 1;
            m_ConfigurationFrame2.Published = false;
            m_ConfigurationFrame2.Timestamp = timestamp;
            m_ConfigurationFrame2.FrameRate = frameRate;
            #endregion

            #region [ DataFrame  Uses configuration frame 1]
            m_DataFrame = new global::GSF.PhasorProtocols.IEEEC37_118.DataFrame(timestamp,ConfigurationFrame1);
            m_DataFrame.Published = false;
            #endregion

            #region [ Configuration Cell ]
            m_IConfigurationCell = new ConfigurationCell(m_IConfigurationFrame);
            m_IConfigurationCell.IDCode = 1;
            m_IConfigurationCell.StationName = StationName;
            m_IConfigurationCell.IDLabel = "configCell";
            #endregion

            #region [ Configuration Cell Set ]
            m_IConfigurationCell.PhasorCoordinateFormat = CoordinateFormat.Polar;
            m_IConfigurationCell.PhasorDataFormat = DataFormat.FloatingPoint;
            m_IConfigurationCell.FrequencyDataFormat = DataFormat.FloatingPoint;
            m_IConfigurationCell.AnalogDataFormat = DataFormat.FloatingPoint;
            m_IConfigurationCell.NominalFrequency = LineFrequency.Hz60;
            m_IConfigurationCell.RevisionCount = 0;
            #endregion

            #region [ Analog Definition ]
            m_AnalogDefinition = new AnalogDefinition(m_IConfigurationCell);
            m_AnalogDefinition.Label = "AnalogDefinition";
            m_AnalogDefinition.Index = 0;
            m_AnalogDefinition.Offset = 0;
            m_AnalogDefinition.ConversionFactor = 0;
            m_AnalogDefinition.ScalingValue = 0;
            #endregion

            #region [ Analog Value ]
            m_AnalogValue = new AnalogValue(m_DataCell, m_AnalogDefinition);
            m_AnalogValue.Value = 10;
            #endregion

            #region [ Frequency Definition ]
            m_FrequencyDefinition = new FrequencyDefinition(m_IConfigurationCell);
            m_FrequencyDefinition.Label = "freqDef";
            m_FrequencyDefinition.Index = 0;
            //m_FrequencyDefinition.Offset = 60; verify: Offset is read only.
            m_FrequencyDefinition.ConversionFactor = 0.01;
            m_FrequencyDefinition.ScalingValue = 1000;
            #endregion

            #region [ Frequency Value ]
            m_FrequencyValue = new FrequencyValue(m_DataCell, m_FrequencyDefinition);
            m_FrequencyValue.UnscaledFrequency = 60;
            #endregion

            #region [ Phasor Definition ]
            m_PhasorDefinition = new PhasorDefinition(m_IConfigurationCell);
            m_PhasorDefinition.Label = "PZR.CV";
            m_PhasorDefinition.Index = 5;
            m_PhasorDefinition.Offset = 0;
            m_PhasorDefinition.ConversionFactor = 0;
            m_PhasorDefinition.ScalingValue = 0;
            #endregion

            #region [ Phasor Value ]
            m_PhasorValue = new global::GSF.PhasorProtocols.IEEEC37_118.PhasorValue(m_DataCell, (IPhasorDefinition)m_PhasorDefinition);
            m_PhasorValue.Angle = new global::GSF.Units.Angle(10);
            m_PhasorValue.Magnitude = 10;
            #endregion
            
            #region [ Digital Definition ]
            m_DigitalDefinition = new DigitalDefinition(m_IConfigurationCell);
            m_DigitalDefinition.Label = "digitallDef";
            m_DigitalDefinition.Index = 1;
            m_DigitalDefinition.Offset = 0;
            m_DigitalDefinition.Tag = "digitalDefTag";
            m_DigitalDefinition.ValidInputs = 1;
            #endregion

            #region [ Digital Value ]
            m_DigitalValue = new global::GSF.PhasorProtocols.IEEEC37_118.DigitalValue((IDataCell)m_DataCell,(IDigitalDefinition)m_DigitalDefinition);
            #endregion

            #region [ Phasor Value Collection ]
            m_PhasorValueCollection = new PhasorValueCollection(1);
            m_PhasorValueCollection.Add(m_PhasorValue);
            #endregion

            #region [ Analog Value Collection ]
            m_AnalogValueCollection = new AnalogValueCollection(2);
            m_AnalogValueCollection.Add(m_AnalogValue);
            //m_AnalogValueCollection
            #endregion

            #region [ Digital Value Collection ]
            m_DigitalValueCollection = new DigitalValueCollection(3);
            m_DigitalValueCollection.Add(m_DigitalValue);
            #endregion

            #region [ Initialize Configuration Frame & Cell]
            m_IConfigurationCell.AnalogDefinitions.Add(m_AnalogDefinition);
            m_IConfigurationCell.FrequencyDefinition = m_FrequencyDefinition;
            m_IConfigurationCell.PhasorDefinitions.Add(m_PhasorDefinition);
            m_IConfigurationCell.DigitalDefinitions.Add(m_DigitalDefinition);
            m_ConfigurationFrame1.Cells.Add(m_IConfigurationCell);
            m_ConfigurationFrame2.Cells.Add(m_IConfigurationCell);
            // Using configuration frame 1 as a default test instance
            //m_Concentrator.ConfigurationFrame.Cells.Add( = new ConfigurationFrame1( m_ConfigurationFrame1;
            // Using as a type reference
            m_IConfigurationFrame = (IConfigurationFrame)m_ConfigurationFrame1;
            #endregion
        }
        #endregion
    }
}
