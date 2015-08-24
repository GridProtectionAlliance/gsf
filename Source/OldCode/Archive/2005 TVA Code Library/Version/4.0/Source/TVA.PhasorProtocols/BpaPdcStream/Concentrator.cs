


////*******************************************************************************************************
////  PdcConcentrator.vb - BPA PDcstream Phasor data concentrator
////  Copyright © 2009 - TVA, all rights reserved - Gbtc
////
////  Build Environment: VB.NET, Visual Studio 2008
////  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
////      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
////       Phone: 423/751-2827
////       Email: jrcarrol@tva.gov
////
////  Code Modification History:
////  -----------------------------------------------------------------------------------------------------
////  04/20/2007 - J. Ritchie Carroll
////       Initial version of source generated
////
////*******************************************************************************************************


//namespace TVASPDC
//{
//    public class BpaPdcConcentrator : PhasorDataConcentratorBase
//    {
		
		
		
//        private ConfigurationFrame m_configurationFrame;
//        private string m_iniFileName;
		
//        public BpaPdcConcentrator(New communicationServer, string name, int framesPerSecond, double lagTime, double leadTime, string iniFileName, New exceptionLogger) : base(communicationServer, name, framesPerSecond, lagTime, leadTime, exceptionLogger)
//        {
			
			
//            m_iniFileName = iniFileName;
			
//        }
		
//        //Protected Overrides Function CreateNewConfigurationFrame(ByVal baseConfigurationFrame As ConfigurationFrame) As IConfigurationFrame
		
//        //    'Dim x, y As Integer
		
//        //    '' We create a new IEEE C37.118 configuration frame 2 based on input configuration
//        //    'm_configurationFrame = New BpaPdcStream.ConfigurationFrame(IeeeC37_118.FrameType.ConfigurationFrame2, m_timeBase, baseConfiguration.IDCode, DateTime.UtcNow.Ticks, FramesPerSecond, m_version)
		
//        //    'For x = 0 To baseConfiguration.Cells.Count - 1
//        //    '    Dim baseCell As ConfigurationCell = baseConfiguration.Cells(x)
//        //    '    Dim newCell As New IeeeC37_118.ConfigurationCell(m_configurationFrame, baseCell.IDCode, baseCell.NominalFrequency)
		
//        //    '    ' Update other cell level attributes
//        //    '    newCell.StationName = baseCell.StationName
//        //    '    newCell.IDLabel = baseCell.IDLabel
		
//        //    '    ' Add phasor definitions
//        //    '    For y = 0 To baseCell.PhasorDefinitions.Count - 1
//        //    '        newCell.PhasorDefinitions.Add(New IeeeC37_118.PhasorDefinition(newCell, baseCell.PhasorDefinitions(y)))
//        //    '    Next
		
//        //    '    ' Add frequency definition
//        //    '    newCell.FrequencyDefinition = New IeeeC37_118.FrequencyDefinition(newCell, baseCell.FrequencyDefinition)
		
//        //    '    ' Add analog definitions
//        //    '    For y = 0 To baseCell.AnalogDefinitions.Count - 1
//        //    '        newCell.AnalogDefinitions.Add(New IeeeC37_118.AnalogDefinition(newCell, baseCell.AnalogDefinitions(y)))
//        //    '    Next
		
//        //    '    ' Add digital definitions
//        //    '    For y = 0 To baseCell.DigitalDefinitions.Count - 1
//        //    '        newCell.DigitalDefinitions.Add(New IeeeC37_118.DigitalDefinition(newCell, baseCell.DigitalDefinitions(y)))
//        //    '    Next
		
//        //    '    ' Add new PMU configuration (cell) to protocol specific configuration frame
//        //    '    m_configurationFrame.Cells.Add(newCell)
//        //    'Next
		
//        //    'm_configurationFrame
		
//        //End Function
		
//        protected override TVASPDC.BpaPdcConcentrator.CreateNewFrame CreateNewFrame(long ticks)
//        {
			
//            // We create a new BPA PDCstream data frame based on current configuration frame
//            BpaPdcStream.DataFrame dataFrame = new BpaPdcStream.DataFrame(); // (ticks, m_configurationFrame)
			
//            //For x As Integer = 0 To m_configurationFrame.Cells.Count - 1
//            //    dataFrame.Cells.Add(New BpaPdcStream.DataCell(dataFrame, m_configurationFrame.Cells(x), x))
//            //Next
			
//            return dataFrame;
			
//        }
		
//        public string IniFileName
//        {
//            get
//            {
//                if (m_configurationFrame == null)
//                {
//                    return m_iniFileName;
//                }
//                else
//                {
//                    return m_configurationFrame.ConfigurationFileName;
//                }
//            }
//        }
		
//    }
	
//}
