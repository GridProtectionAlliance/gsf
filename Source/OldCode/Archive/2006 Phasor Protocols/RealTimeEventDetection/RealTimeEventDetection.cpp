/*******************************************************************************************************\

	RealTimeEventDetection.cpp - Real-time Event Detection Module
	Copyright © 2007 - Washington State University, all rights reserved - Gbtc

	Build Environment: VB.NET, Visual Studio 2005

\*******************************************************************************************************/

#include "stdafx.h"
#include "RealTimeEventDetection.h"

using namespace System;
using namespace System::Text;
using namespace System::Runtime::InteropServices;
using namespace TVA::Measurements;
using namespace TVA::Configuration;
using namespace RealTimeEventDetection;

 /*localchecks[MaxCheck]={
	{{1,2,3},0.02,0.02,0,0},
    {{4,5,6},0.02,0.02,0,0},
	{{7,8,9},0.02,0.02,0,0},
    {{10,11,12},0.02,0.02,0,0},
	{{13,14,15},0.02,0.02,0,0},
    {{16,17,18},0.02,0.02,0,0},
	{{19,20,21},0.02,0.02,0,0},
    {{22,23,24},0.02,0.02,0,0},
};

struct CrossCheck interchecks[] = {
	{{1,2,3},0.02,0.02,0,0}
};*/


// Constructor
EventDetectionAlgorithm::EventDetectionAlgorithm() {}

// Calculation initialization
void EventDetectionAlgorithm::Initialize(String^ calculationName, String^ configurationSection, cli::array<TVA::Measurements::IMeasurement^, 1>^ outputMeasurements, cli::array<TVA::Measurements::MeasurementKey, 1>^ inputMeasurementKeys, int minimumMeasurementsToUse, int expectedMeasurementsPerSecond, double lagTime, double leadTime)
{
	// Call base class initialization function
	__super::Initialize(calculationName, configurationSection, outputMeasurements, inputMeasurementKeys, minimumMeasurementsToUse, expectedMeasurementsPerSecond, lagTime, leadTime);

	// Make sure configuration section parameter is defined - if not, use default
	if (String::IsNullOrEmpty(configurationSection)) ConfigurationSection = DefaultConfigSection;

	// Get categorized settings section from configuration file
	CategorizedSettingsElementCollection^ settings = TVA::Configuration::Common::CategorizedSettings[ConfigurationSection];

	// Make sure needed configuration variables exist - since configuration variables will
	// be added to config file of parent process we add them to a new configuration category
	settings->Add("MaximumTasks", "40", "Maximum allowed number of tasks");
	settings->Add("MaximumChannels", "6", "Maximum allowed data channels per PMU");
	settings->Add("MaximumCrossChecks", "40", "Maximum allowed number of cross-checks");
	settings->Add("MaximumMissingPoints", "4", "Maximum allowed missing data points per channel per second");
	settings->Add("MaximumDisplayModes", "5", "Maximum allowed number of modes to display in each signal");
	settings->Add("EstimateTriggerThreshold", "4", "Number of consistent estimates needed to trigger warning signal");
	settings->Add("AnalysisWindow", "10", "Size of data sample window, in seconds");
	settings->Add("RemoveMeanValue", "True", "Remove mean value before analysis");
	settings->Add("NormalizeData", "True", "Normalize data before analysis");
	settings->Add("DisplayDetail", "True", "Detail display of result from each analysis");
	settings->Add("RepeatTime", "1", "Time window used repeat analysis, in seconds");
	settings->Add("ConsistentFrequencyRange", "0.02", "Frequency range for consistent estimate");
	settings->Add("ConsistentRatioRange", "0.02", "Ratio range for consistent estimate");
	settings->Add("VoltageThreshold", "0.005", "Threshold of voltage for event detection");
	settings->Add("CurrentThreshold", "0.01", "Threshold of current for event detection");
	settings->Add("EnergyDisplayThreshold", "0.5", "Relative energy threshold used for display");

	// Save updates to config file, if any
	TVA::Configuration::Common::SaveSettings();

	// Load algorithm parameters from configuration file
	m_maximumTasks = Convert::ToInt32(settings["MaximumTasks"]->Value);
	m_maximumChannels = Convert::ToInt32(settings["MaximumChannels"]->Value);
	m_maximumCrossChecks = Convert::ToInt32(settings["MaximumCrossChecks"]->Value);
	m_maximumMissingPoints = Convert::ToInt32(settings["MaximumMissingPoints"]->Value);
	m_maximumDisplayModes = Convert::ToInt32(settings["MaximumDisplayModes"]->Value);
	m_estimateTriggerThreshold = Convert::ToInt32(settings["EstimateTriggerThreshold"]->Value);
	m_analysisWindow = Convert::ToInt32(settings["AnalysisWindow"]->Value);
	m_removeMeanValue = TVA::Text::Common::ParseBoolean(settings["RemoveMeanValue"]->Value);
	m_normalizeData = TVA::Text::Common::ParseBoolean(settings["NormalizeData"]->Value);
	m_displayDetail = TVA::Text::Common::ParseBoolean(settings["DisplayDetail"]->Value);
	m_repeatTime = Convert::ToDouble(settings["RepeatTime"]->Value);
	m_consistentFrequencyRange = Convert::ToDouble(settings["ConsistentFrequencyRange"]->Value);
	m_consistentRatioRange = Convert::ToDouble(settings["ConsistentRatioRange"]->Value);
	m_voltageThreshold = Convert::ToDouble(settings["VoltageThreshold"]->Value);
	m_currentThreshold = Convert::ToDouble(settings["CurrentThreshold"]->Value);
	m_energyDisplayThreshold = Convert::ToDouble(settings["EnergyDisplayThreshold"]->Value);

	// Initialize the input measurements needed to perform this calculation

	// Define CUMB measurements
	MeasurementKey^ cumbBus1VM = gcnew MeasurementKey(1608, "P0");	// TVA_CUMB-BUS1:ABBV
	MeasurementKey^ cumbBus1VA = gcnew MeasurementKey(1609, "P0");	// TVA_CUMB-BUS1:ABBVH
	MeasurementKey^ cumbMarsIM = gcnew MeasurementKey(1612, "P0");	// TVA_CUMB-MARS:ABBI
	MeasurementKey^ cumbMarsIA = gcnew MeasurementKey(1615, "P0");	// TVA_CUMB-MARS:ABBIH
	MeasurementKey^ cumbJohnIM = gcnew MeasurementKey(1616, "P0");	// TVA_CUMB-JOHN:ABBI
	MeasurementKey^ cumbJohnIA = gcnew MeasurementKey(1619, "P0");	// TVA_CUMB-JOHN:ABBIH
	MeasurementKey^ cumbDavdIM = gcnew MeasurementKey(1620, "P0");	// TVA_CUMB-DAVD:ABBI
	MeasurementKey^ cumbDavdIA = gcnew MeasurementKey(1623, "P0");	// TVA_CUMB-DAVD:ABBIH

	m_channelType = gcnew List<ChannelType>;

	m_channelType->Add(ChannelType::VM);
	m_channelType->Add(ChannelType::VA);
	m_channelType->Add(ChannelType::IM);
	m_channelType->Add(ChannelType::IA);
	m_channelType->Add(ChannelType::IM);
	m_channelType->Add(ChannelType::IA);
	m_channelType->Add(ChannelType::IM);
	m_channelType->Add(ChannelType::IA);

	// In this calculation, we manually initialize the input measurements to use for the base class since they are
    // a hard-coded set of inputs that will not change (i.e., no need to specifiy input measurements from SQL)
	List<MeasurementKey>^ inputMeasurements = gcnew List<MeasurementKey>;

	// Add CUMB channels to input measurements
	inputMeasurements->Add(*cumbBus1VM);	// Measurment 0
	inputMeasurements->Add(*cumbBus1VA);	// Measurment 1
	inputMeasurements->Add(*cumbMarsIM);	// Measurment 2
	inputMeasurements->Add(*cumbMarsIA);	// Measurment 3
	inputMeasurements->Add(*cumbJohnIM);	// Measurment 4
	inputMeasurements->Add(*cumbJohnIA);	// Measurment 5
	inputMeasurements->Add(*cumbDavdIM);	// Measurment 6
	inputMeasurements->Add(*cumbDavdIA);	// Measurment 7

	List<int>^ cumbMeasurementIndicies = gcnew List<int>;
	cumbMeasurementIndicies->Add(0);
	cumbMeasurementIndicies->Add(1);
	cumbMeasurementIndicies->Add(2);
	cumbMeasurementIndicies->Add(3);
	cumbMeasurementIndicies->Add(4);
	cumbMeasurementIndicies->Add(5);
	cumbMeasurementIndicies->Add(6);
	cumbMeasurementIndicies->Add(7);

    InputMeasurementKeys = inputMeasurements->ToArray();
	MinimumMeasurementsToUse = inputMeasurements->Count;

	m_localTasks = gcnew List<AnalysisTask^>;

 	m_localTasks->Add(gcnew AnalysisTask(cumbMeasurementIndicies, "Prony"));			// Task 0
 	m_localTasks->Add(gcnew AnalysisTask(cumbMeasurementIndicies, "MatrixPencil"));		// Task 1
 	m_localTasks->Add(gcnew AnalysisTask(cumbMeasurementIndicies, "HTLS"));				// Task 2

	List<int>^ cumbTaskIndicies = gcnew List<int>;
	cumbMeasurementIndicies->Add(0);
	cumbMeasurementIndicies->Add(1);
	cumbMeasurementIndicies->Add(2);

	m_interAreaTasks = gcnew List<AnalysisTask^>;

	m_interAreaTasks->Add(gcnew AnalysisTask(nullptr, "Prony"));
	m_interAreaTasks->Add(gcnew AnalysisTask(nullptr, "MatrixPencil"));
	m_interAreaTasks->Add(gcnew AnalysisTask(nullptr, "HTLS"));

	m_localCrossChecks = gcnew List<CrossCheck^>;

	m_localCrossChecks->Add(gcnew CrossCheck(cumbTaskIndicies, 0.02, 0.02));

	// Initialize system path
	m_systemPath = TVA::IO::FilePath::GetApplicationPath();

	char* outputFileName;

	// Open output files
	outputFileName = StringToCharBuffer(String::Format("{0}message.txt", m_systemPath));
	fout_message=fopen(outputFileName, "w");
	free(outputFileName);
	if (fout_message==NULL) throw gcnew ArgumentException("Error in opening output message file");
    
	// TODO: replace with structured exception handling (use throw gcnew)
	if (m_displayDetail)
	{
		outputFileName = StringToCharBuffer(String::Format("{0}local_task_details.txt", m_systemPath));

	    fout_local_details=fopen(outputFileName,"w");

		if (fout_local_details==NULL)
		{
			fprintf(fout_message,"Error in opening local detailed output file data.txt\n");
			return;
		}

		free(outputFileName);

		outputFileName = StringToCharBuffer(String::Format("{0}interarea_task_details.txt", m_systemPath));

	    fout_inter_details=fopen(outputFileName,"w");
		if (fout_inter_details==NULL)
		{
			fprintf(fout_message,"Error in opening interarea detailed output file data.txt\n");
			return;
		}

		free(outputFileName);
	}

	//strcpy(file_name,directory); 
	//fout_local_xcheck=fopen(strcat(file_name,"local_checks.txt"),"w");
	//if (fout_local_xcheck==NULL)
	//{
	//	fprintf(fout_message,"Error in opening local_crosscheck.txt\n");
	//	return;
	//}

	//strcpy(file_name,directory); 
	//fout_inter_xcheck=fopen(strcat(file_name,"interarea_checks.txt"),"w");
	//if (fout_inter_xcheck==NULL)
	//{
	//	fprintf(fout_message,"Error in opening interarea_crosscheck.txt\n");
	//	return;
	//}

	//strcpy(file_name,directory); 
	//fout_mov_local_checks=fopen(strcat(file_name,"moving_local_checks.txt"),"w");
	//if (fout_mov_local_checks==NULL)
	//{
	//	fprintf(fout_message,"Error in opening output file moving_local_checks.txt\n");
	//	return;
	//}

	//strcpy(file_name,directory); 
	//fout_mov_inter_checks=fopen(strcat(file_name,"moving_interarea_checks.txt"),"w");
	//if (fout_mov_inter_checks==NULL)
	//{
	//	fprintf(fout_message,"Error in opening output file moving_interarea_checks.txt");
	//	return;
	//}

	// Define global channel count
	m_channelCount = InputMeasurementKeys->Length;

	// Calculate minimum needed sample size
	m_minimumSamples = m_analysisWindow * expectedMeasurementsPerSecond;

	// Define a missing measurement
	m_missingMeasurement = gcnew TVA::Measurements::Measurement();

	m_missingMeasurement->ID = -1;
	m_missingMeasurement->Source = "_missing";
	m_missingMeasurement->Value = Double::NaN;

	// Intitalize our rolling window data buffer
	m_measurementMatrix = gcnew List<array<IMeasurement^, 1>^>;
}

void RealTimeEventDetection::EventDetectionAlgorithm::PublishFrame(TVA::Measurements::IFrame^ frame, int index)
{
	IMeasurement^ measurement;
	array<IMeasurement^, 1>^ measurements = TVA::Common::CreateArray<IMeasurement^>(m_channelCount);
	double* data;

    // Loop through all input measurements to see if they exist in this frame
    for(int x = 0; x < m_channelCount; x++)
	{
       if (frame->Measurements->TryGetValue(InputMeasurementKeys[x], measurement))
			measurements[x] = measurement;
	   else
			measurements[x] = m_missingMeasurement;
	}

	// Maintain constant row-matrix length
	m_measurementMatrix->Add(measurements);

	while (m_measurementMatrix->Count > m_minimumSamples)
		m_measurementMatrix->RemoveAt(0);

	// We don't start calculations until the needed matrix size is available
	if (m_measurementMatrix->Count >= m_minimumSamples)
	{
		// Handle data intialization
		data = (double *) malloc (sizeof(double) * m_minimumSamples * m_channelCount);
		for (int i = 0; i < m_minimumSamples; i++)
		{
			// Get data row
			measurements = m_measurementMatrix[i];

			for (int j = 0; j < m_channelCount; j++)
			{
				// Get data column
				measurement = measurements[j];

				if (Double::IsNaN(measurement->Value))
				{
					// TODO: handle data interpolation
				}

				// Store all 2-dimension matrix in Fortran format,column-wise
				data[i + j * m_minimumSamples] = measurement->AdjustedValue;
			}
		}

		// TODO: finish algorithm steps...
	}
}

// Caller responsible for calling "free()" on returned buffer...
char* StringToCharBuffer(String^ gcStr)
{
	cli:array<unsigned char, 1>^ gcBytes = System::Text::Encoding::Default->GetBytes(gcStr);
	char* str = (char*)malloc(gcStr->Length + 1);
	if (str == NULL) throw gcnew OutOfMemoryException();
	Marshal::Copy(gcBytes, 0, (IntPtr)str, gcStr->Length);
	str[gcStr->Length] = '\0';
	return str;
}