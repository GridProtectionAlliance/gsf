/*******************************************************************************************************\

	RealTimeEventDetection.cpp - Real-time Event Detection Module
	Copyright ?2007 - Washington State University, all rights reserved - Gbtc

	Build Environment: VB.NET, Visual Studio 2005

\*******************************************************************************************************/

#include "stdafx.h"
#include "RealTimeEventDetection.h"

using namespace System;
using namespace System::Text;
using namespace System::Threading;
using namespace System::Runtime::InteropServices;
using namespace TVA::Measurements;
using namespace TVA::Configuration;
using namespace RealTimeEventDetection;

// Constructor
EventDetectionAlgorithm::EventDetectionAlgorithm() {}

// Calculation initialization
void EventDetectionAlgorithm::Initialize(String^ calculationName, String^ configurationSection, cli::array<TVA::Measurements::IMeasurement^, 1>^ outputMeasurements, cli::array<TVA::Measurements::MeasurementKey, 1>^ inputMeasurementKeys, int minimumMeasurementsToUse, int expectedMeasurementsPerSecond, double lagTime, double leadTime)
{
	// Call base class initialization function
	//__super::Initialize(calculationName, configurationSection, outputMeasurements, inputMeasurementKeys, minimumMeasurementsToUse, expectedMeasurementsPerSecond, lagTime, leadTime);

	// Make sure configuration section parameter is defined - if not, use default
	if (String::IsNullOrEmpty(configurationSection)) ConfigurationSection = DefaultConfigSection;

	// Get categorized settings section from configuration file
	CategorizedSettingsElementCollection^ settings = TVA::Configuration::Common::CategorizedSettings[ConfigurationSection];

	// Make sure needed configuration variables exist - since configuration variables will
	// be added to config file of parent process we add them to a new configuration category
	settings->Add("MaximumChannels", "6", "Maximum allowed data channels per PMU");
	settings->Add("MaximumMissingPoints", "4", "Maximum allowed missing data points per channel per second");
	settings->Add("MaximumCrossChecks", "40", "Maximum allowed number of cross-checks");
	settings->Add("MaximumDisplayModes", "5", "Maximum allowed number of modes to display in each signal");
	settings->Add("EstimateTriggerThreshold", "4", "Number of consistent estimates needed to trigger warning signal");
	settings->Add("AnalysisWindow", "5", "Size of data sample window, in seconds");
	settings->Add("RemoveMeanValue", "True", "Remove mean value before analysis");
	settings->Add("NormalizeData", "True", "NormalizeData data before analysis");
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
	m_maximumChannels = Convert::ToInt32(settings["MaximumChannels"]->Value);
	m_maximumMissingPoints = Convert::ToInt32(settings["MaximumMissingPoints"]->Value);
	m_maximumCrossChecks = Convert::ToInt32(settings["MaximumCrossChecks"]->Value);
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

	//// Define CUMB measurements
	//MeasurementKey^ cumbBus1VM = gcnew MeasurementKey(1608, "P0");	// TVA_CUMB-BUS1:ABBV
	//MeasurementKey^ cumbBus1VA = gcnew MeasurementKey(1609, "P0");	// TVA_CUMB-BUS1:ABBVH
	//MeasurementKey^ cumbMarsIM = gcnew MeasurementKey(1612, "P0");	// TVA_CUMB-MARS:ABBI
	//MeasurementKey^ cumbMarsIA = gcnew MeasurementKey(1615, "P0");	// TVA_CUMB-MARS:ABBIH
	//MeasurementKey^ cumbJohnIM = gcnew MeasurementKey(1616, "P0");	// TVA_CUMB-JOHN:ABBI
	//MeasurementKey^ cumbJohnIA = gcnew MeasurementKey(1619, "P0");	// TVA_CUMB-JOHN:ABBIH
	//MeasurementKey^ cumbDavdIM = gcnew MeasurementKey(1620, "P0");	// TVA_CUMB-DAVD:ABBI
	//MeasurementKey^ cumbDavdIA = gcnew MeasurementKey(1623, "P0");	// TVA_CUMB-DAVD:ABBIH

	//m_channelType = gcnew List<ChannelType>;

	//m_channelType->Add(ChannelType::VM);
	//m_channelType->Add(ChannelType::VA);
	//m_channelType->Add(ChannelType::IM);
	//m_channelType->Add(ChannelType::IA);
	//m_channelType->Add(ChannelType::IM);
	//m_channelType->Add(ChannelType::IA);
	//m_channelType->Add(ChannelType::IM);
	//m_channelType->Add(ChannelType::IA);

	//// In this calculation, we manually initialize the input measurements to use for the base class since they are
 //   // a hard-coded set of inputs that will not change (i.e., no need to specifiy input measurements from SQL)
	//List<MeasurementKey>^ inputMeasurements = gcnew List<MeasurementKey>;

	//// Add CUMB channels to input measurements
	//inputMeasurements->Add(*cumbBus1VM);	// Measurment 0
	//inputMeasurements->Add(*cumbBus1VA);	// Measurment 1
	//inputMeasurements->Add(*cumbMarsIM);	// Measurment 2
	//inputMeasurements->Add(*cumbMarsIA);	// Measurment 3
	//inputMeasurements->Add(*cumbJohnIM);	// Measurment 4
	//inputMeasurements->Add(*cumbJohnIA);	// Measurment 5
	//inputMeasurements->Add(*cumbDavdIM);	// Measurment 6
	//inputMeasurements->Add(*cumbDavdIA);	// Measurment 7

	//List<int>^ cumbMeasurementIndicies = gcnew List<int>;
	//cumbMeasurementIndicies->Add(0);
	//cumbMeasurementIndicies->Add(1);
	//cumbMeasurementIndicies->Add(2);
	//cumbMeasurementIndicies->Add(3);
	//cumbMeasurementIndicies->Add(4);
	//cumbMeasurementIndicies->Add(5);
	//cumbMeasurementIndicies->Add(6);
	//cumbMeasurementIndicies->Add(7);
	// Define CUMB measurements

	MeasurementKey^ jdayBusV = gcnew MeasurementKey(1608, "P0");	//
	MeasurementKey^ jdayBusI1 = gcnew MeasurementKey(1609, "P0");	// 
	MeasurementKey^ jdayBusI2 = gcnew MeasurementKey(1610, "P0");	// 
	MeasurementKey^ jdayBusI3 = gcnew MeasurementKey(1611, "P0");	// 
	MeasurementKey^ jdayBusI4 = gcnew MeasurementKey(1612, "P0");	// 
	MeasurementKey^ jdayBusI5 = gcnew MeasurementKey(1613, "P0");	// 


	m_channelType = gcnew List<ChannelType>;

	m_channelType->Add(ChannelType::VM);
	m_channelType->Add(ChannelType::IM);
	m_channelType->Add(ChannelType::IM);
	m_channelType->Add(ChannelType::IM);
	m_channelType->Add(ChannelType::IM);
	m_channelType->Add(ChannelType::IM);

	// In this calculation, we manually initialize the input measurements to use for the base class since they are
    // a hard-coded set of inputs that will not change (i.e., no need to specifiy input measurements from SQL)
	List<MeasurementKey>^ inputMeasurements = gcnew List<MeasurementKey>;

	// Add CUMB channels to input measurements
	inputMeasurements->Add(*jdayBusV);	// Measurment 0
	inputMeasurements->Add(*jdayBusI1);	// Measurment 1
	inputMeasurements->Add(*jdayBusI2);	// Measurment 2
	inputMeasurements->Add(*jdayBusI3);	// Measurment 3
	inputMeasurements->Add(*jdayBusI4);	// Measurment 4
	inputMeasurements->Add(*jdayBusI5);	// Measurment 5


	List<int>^ jdayMeasurementIndicies = gcnew List<int>;
	jdayMeasurementIndicies->Add(0);
	jdayMeasurementIndicies->Add(1);
	jdayMeasurementIndicies->Add(2);
	jdayMeasurementIndicies->Add(3);
	jdayMeasurementIndicies->Add(4);
	jdayMeasurementIndicies->Add(5);


    InputMeasurementKeys = inputMeasurements->ToArray();
	MinimumMeasurementsToUse = inputMeasurements->Count;

	work = (double *) malloc(sizeof(double)*MaxLwork);

	m_localTasks = gcnew List<AnalysisTask^>; 

 	m_localTasks->Add(gcnew AnalysisTask(jdayMeasurementIndicies, "Prony"));			// Task 0
 	m_localTasks->Add(gcnew AnalysisTask(jdayMeasurementIndicies, "MatrixPencil"));		// Task 1
 	m_localTasks->Add(gcnew AnalysisTask(jdayMeasurementIndicies, "HTLS"));				// Task 2

	List<int>^ jdayTaskIndicies = gcnew List<int>;
	jdayTaskIndicies->Add(0);
	jdayTaskIndicies->Add(1);
	jdayTaskIndicies->Add(2);

	m_interAreaTasks = gcnew List<AnalysisTask^>;

	m_interAreaTasks->Add(gcnew AnalysisTask(nullptr, "Prony"));
	m_interAreaTasks->Add(gcnew AnalysisTask(nullptr, "MatrixPencil"));
	m_interAreaTasks->Add(gcnew AnalysisTask(nullptr, "HTLS"));

	m_localCrossChecks = gcnew List<CrossCheck^>; 

	m_localCrossChecks->Add(gcnew CrossCheck(jdayTaskIndicies, 0.02, 0.02));

	m_interAreaChecks = gcnew List<CrossCheck^>; 

	m_interAreaChecks->Add(gcnew CrossCheck(jdayTaskIndicies, 0.02, 0.02));

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
			throw gcnew ArgumentException("Error in opening output file");
		}
		free(outputFileName);

		outputFileName = StringToCharBuffer(String::Format("{0}interarea_task_details.txt", m_systemPath));
	    fout_inter_details=fopen(outputFileName,"w");
		if (fout_inter_details==NULL)
		{
			fprintf(fout_message,"Error in opening interarea detailed output file data.txt\n");
			throw gcnew ArgumentException("Error in opening output file");
		}
		free(outputFileName);
	}

	outputFileName = StringToCharBuffer(String::Format("{0}local_checks.txt", m_systemPath));
	fout_local_xcheck=fopen(outputFileName,"w");
	if (fout_local_xcheck==NULL)
	{
		fprintf(fout_message,"Error in opening local_crosscheck.txt\n");
		throw gcnew ArgumentException("Error in opening output file");
	}
	free(outputFileName);

	outputFileName = StringToCharBuffer(String::Format("{0}inteaarea.txt", m_systemPath));
	fout_inter_xcheck=fopen(outputFileName,"w");
	if (fout_inter_xcheck==NULL)
	{
		fprintf(fout_message,"Error in opening interarea_crosscheck.txt\n");
		throw gcnew ArgumentException("Error in opening output file");
	}
	free(outputFileName);

	outputFileName = StringToCharBuffer(String::Format("{0}inteaarea.txt", m_systemPath));
	fout_mov_local_checks=fopen(outputFileName,"w");
	if (fout_mov_local_checks==NULL)
	{
		fprintf(fout_message,"Error in opening output file moving_local_checks.txt\n");
		throw gcnew ArgumentException("Error in opening output file");
	}
	free(outputFileName);

	outputFileName = StringToCharBuffer(String::Format("{0}moving_interarea_checks.txt", m_systemPath));
	fout_mov_inter_checks=fopen(outputFileName,"w");
	if (fout_mov_inter_checks==NULL)
	{
		fprintf(fout_message,"Error in opening output file moving_interarea_checks.txt");
		throw gcnew ArgumentException("Error in opening output file");
	}
	free(outputFileName);

	// Define global channel count
	m_channelCount = InputMeasurementKeys->Length;

	// Calculate minimum needed sample size
	m_minimumSamples = (m_analysisWindow + (int)(m_repeatTime * (m_estimateTriggerThreshold - 1)) )* expectedMeasurementsPerSecond;

	// Intitalize our rolling window data buffer
	m_measurementMatrix = gcnew List<array<IMeasurement^, 1>^>;
}

void EventDetectionAlgorithm::PublishFrame(TVA::Measurements::IFrame^ frame, int index)
{
	IMeasurement^ measurement;
	MeasurementKey measurementKey;
	array<MeasurementKey, 1>^ inputMeasurements = InputMeasurementKeys;
	array<IMeasurement^, 1>^ measurements = TVA::Common::CreateArray<IMeasurement^>(m_channelCount);
	double* data;

    bool eventFlag = false;						// event flag for each channel
	List<bool>^ taskEvent= gcnew List<bool>;    // event flag for one task
	List<ManualResetEvent^>^ manualEvents;		// Wait handles for threaded tasks
	ManualResetEvent^ manualEvent;				// Current wait handle
	ThreadState^ taskState;						// Task thread state information (thread parameters)

	List<bool>^ channelEvent = gcnew List<bool>;
	List<double>^ relativeDeviation = gcnew List<double>;
	int m_Nx=1,m_Ny=1;
	double maximumValue,minimumValue;
	double meanValue,maximumDeviation;
    int maximumIndex,minimumIndex;
	int m;                             // m is the number of channels in the task
	int N;

	AnalysisTask^ currentTask;
    CrossCheck^ currentCheck;

	List<double>^ crosscheckResultFrequency = gcnew List<double>;
	List<double>^ crosscheckResultRatio = gcnew List<double>;
	int numberSuccessLocalCheck,numberSuccessInterAreaCheck;
	double *movingLocalFrequency = NULL,*movingLocalRatio = NULL;
	double *movingInterAreaFrequency = NULL,*movingInterAreaRatio = NULL;

    // Loop through all input measurements to see if they exist in this frame
    for(int x = 0; x < m_channelCount; x++)
	{
		measurementKey = inputMeasurements[x];

		if (frame->Measurements->TryGetValue(measurementKey, measurement))
			measurements[x] = measurement;
		else
			measurements[x] = gcnew TVA::Measurements::Measurement(measurementKey.ID, measurementKey.Source, Double::NaN, frame->Ticks);
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
		/* event detection for each channel */
		for (int channel_no=0;channel_no<m_channelCount;channel_no++)
		{
			findmin(&data[0+channel_no*m_minimumSamples],m_minimumSamples,&minimumValue,&minimumIndex);
			findmax(&data[0+channel_no*m_minimumSamples],m_minimumSamples,&maximumValue,&maximumIndex);
			meanValue = findmean(&data[0+channel_no*m_minimumSamples],m_minimumSamples);
			maximumDeviation = max(abs(minimumValue-meanValue),abs(maximumValue-meanValue));
			relativeDeviation->Add(maximumDeviation/abs(meanValue));
			channelEvent->Add(false);

			if (m_channelType[channel_no] == ChannelType::VM)      // voltage magnitude
			{
				if (relativeDeviation[channel_no] > m_voltageThreshold)
				{
					channelEvent[channel_no] = true;
					eventFlag = true;
				}
				else
					channelEvent[channel_no] = false;
			}
			else if (m_channelType[channel_no] == ChannelType::IM)  // current magnitude
			{
				if (relativeDeviation[channel_no] > m_currentThreshold )
				{
					channelEvent[channel_no] = true;
					eventFlag = true;
				}
				else
					channelEvent[channel_no] = false;
			}
			//else   //other channels are not used in event detection
			//	channelEvent[channel_no] = 0;
		}

		if (!eventFlag)
		{
			fprintf(fout_message,"No event detected in all channels.\n\n");
			throw gcnew ArgumentException("No event detected in all channels");;
		}

		
		/* real time moving window loop */

		N = (int) (m_analysisWindow * FramesPerSecond);
		movingLocalFrequency = (double *) malloc (sizeof(double) * m_estimateTriggerThreshold * m_localCrossChecks->Count);     // used for storing local moving crosschecks
		movingLocalRatio = (double *) malloc (sizeof(double) * m_estimateTriggerThreshold * m_localCrossChecks->Count); // movingLocalFrequency[i][j] is j-th xcheck in iteration i
		movingInterAreaFrequency = (double *) malloc (sizeof(double) * m_estimateTriggerThreshold * m_maximumCrossChecks);     // used for storing inter-area moving crosschecks
		movingInterAreaRatio = (double *) malloc (sizeof(double) * m_estimateTriggerThreshold * m_maximumCrossChecks);
		
		for (int m_iter = 0; m_iter < m_estimateTriggerThreshold; m_iter++)
		{

//			/* iterate for each local task */
//			for (int task_no=0; task_no<m_localTasks->Count ;task_no++)
//			{
//				taskEvent->Add(false);
//				currentTask = m_localTasks[task_no];
//
//				/* set currentTask->data */
//				m = currentTask->channel->Count;
//				currentTask->m = m;
//				currentTask->N = N;
//				currentTask->type = "Local";
//				currentTask->data = (double *) malloc(sizeof(double) * N * m);
// 				for (int k = 0; k < currentTask->m; k++)
//				{
//					int j = currentTask->channel[k];
//					if (j>m_channelCount)    // Incorrect channel
//					{
//						fprintf(fout_message,"Invalid channel number.\n");
//						throw gcnew ArgumentException("Invalid channel number");
//					}
//					if (channelEvent[j])
//						taskEvent[task_no] = true;
//					for(int i = 0; i<N ;i++)
////						currentTask->data[i][k]=data[i+i_start][j-1];
//						currentTask->data[i+k*N] = data[(i+ m_iter * (int)(m_repeatTime * FramesPerSecond) + j * m_minimumSamples)];
//
//				}
//				
////				matrix_write(currentTask->data , currentTask->N,currentTask->m);
//
//				if (!taskEvent[task_no])   // if there is no event in the current task
//				{
//					currentTask->outflag = false;
//					m_localTasks[task_no] = currentTask;
//					if (m_displayDetail)
//					{
//						fprintf(fout_local_details,"Local Task No.%d\nFrom  s to s.\n%s\n\n",task_no+1,currentTask->method);
//						fprintf(fout_local_details,"No event is detected.\n\n");
//					}
//				}
//				else
//				{
//					/* execute an analysis task */
//					exe_task(currentTask,task_no);
//					m_localTasks[task_no] = currentTask;
//					free(currentTask->data);
//				}
//			}  
//			/* end of iteration for each local task */


			manualEvents = gcnew List<ManualResetEvent^>;

			/* iterate for each local task */
			for (int task_no=0; task_no < m_localTasks->Count ; task_no++)
			{
				taskEvent->Add(false);
				currentTask = m_localTasks[task_no];

				/* set currentTask->data */
				m = currentTask->channel->Count;
				currentTask->m = m;
				currentTask->N = N;
				currentTask->type = "Local";
				currentTask->data = (double *) malloc(sizeof(double) * N * m);

				// Assigning data to current task
 				for (int k = 0; k < currentTask->m; k++)
				{
					int j = currentTask->channel[k];
					if (j>m_channelCount)    // Incorrect channel
					{
						fprintf(fout_message,"Invalid channel number.\n");
						throw gcnew ArgumentException("Invalid channel number");
					}
					if (channelEvent[j])
						taskEvent[task_no] = true;

					for(int i = 0; i<N ;i++)
						currentTask->data[i+k*N] = data[(i+ m_iter * (int)(m_repeatTime * FramesPerSecond) + j * m_minimumSamples)];

				}
				
				if (!taskEvent[task_no])   // if there is no event in the current task
				{
					currentTask->outflag = false;
					if (m_displayDetail)
					{
						fprintf(fout_local_details,"Local Task No.%d\nFrom  s to s.\n%s\n\n",task_no+1,currentTask->method);
						fprintf(fout_local_details,"No event is detected.\n\n");
					}
				}
				else
				{
					// Define a new manually reset wait handle for task so we can wait for task to finish
					manualEvent = gcnew ManualResetEvent(false);
					manualEvents->Add(manualEvent);

					// Initialize thread parameters
					taskState = gcnew ThreadState(currentTask, task_no, manualEvent);
					
					// Queue analysis task for execution an on independent thread
					ThreadPool::QueueUserWorkItem(gcnew WaitCallback(this, &EventDetectionAlgorithm::exe_task), taskState);
				}				
			}

			// Wait for all tasks to complete...
			WaitHandle::WaitAll(manualEvents->ToArray());
	
			for (int task_no=0; task_no < m_localTasks->Count ; task_no++)
			{
				free(m_localTasks[task_no]->data);
			}

			/* end of iteration for each local task */

			
			/* crosscheck results from local tasks in each PMU*/
			numberSuccessLocalCheck = 0;                    // initialize the number of successful local crosschecks
			for (int check_no=0; check_no<m_localCrossChecks->Count ;check_no++)
			{
				currentCheck = m_localCrossChecks[check_no];
				currentCheck->flag = 1;
				currentCheck->task_count = currentCheck->task->Count;

				for (int j=0; j < currentCheck->task_count;j++)
				{
					int task_no = currentCheck->task[j];
					currentTask = m_localTasks[task_no];
					if (!currentTask->outflag)
					{
						currentCheck->flag = 0;            // local crosscheck is false because of bad task estimate
						break;
					}
					else
					{
						crosscheckResultFrequency->Add(currentTask->group_f);        // record freq estimates for this crosscheck
						crosscheckResultRatio->Add(currentTask->group_ratio);// record ratio estimates for this crosscheck
					}
				}

				movingLocalFrequency[m_iter%m_estimateTriggerThreshold+m_estimateTriggerThreshold*check_no] = 9999;       // 9999 stands for nonconsistent estimate
				movingLocalRatio[m_iter%m_estimateTriggerThreshold+m_estimateTriggerThreshold*check_no] = 9999;
				if (currentCheck->flag)
				{
					minimumValue = findmin(crosscheckResultFrequency);
					maximumValue = findmax(crosscheckResultFrequency);
					if (maximumValue-minimumValue<currentCheck->freq_range)   // the most dominant frequencies of all tasks are within a range
					{
						maximumValue = findmax(crosscheckResultRatio);
						minimumValue = findmin(crosscheckResultRatio);
						if (maximumValue-minimumValue<currentCheck->ratio_range)  // damping ratios of all tasks are within a range
						{
							currentCheck->freq = findmean(crosscheckResultFrequency);
							currentCheck->ratio = findmean(crosscheckResultRatio);
							/* print consistent current check */
/*							fprintf(fout_local_xcheck,"Local Cross Check No %d\nFrom %.3f s to %.3f s.\n Consistent estimates: freq = %.2f Hz, damping ratio = %.4f\n\n",
									check_no+1,prony_t_start,prony_t_end,currentCheck->freq,currentCheck->ratio);
	*/						/* record frequency and ratio used for consistent moving window check */
							movingLocalFrequency[m_iter%m_estimateTriggerThreshold+m_estimateTriggerThreshold*check_no] = currentCheck->freq;
							movingLocalRatio[m_iter%m_estimateTriggerThreshold+m_estimateTriggerThreshold*check_no] = currentCheck->ratio;
						}
						else
							currentCheck->flag = 0;   // damping ratios in this local crosscheck not within the specified range
					}
					else
					{
						currentCheck->flag =0;        // frequencies in this local crosscheck not within the specified range
					}
				}

				if (currentCheck->flag)	numberSuccessLocalCheck++;          // record successful local crosscheck
				m_localCrossChecks[check_no] = currentCheck;
			}   
			/* end of local crosscheck */

			/* inter-area crosscheck */
			for (int check_no=0;check_no<m_maximumCrossChecks;check_no++)
			{
				movingInterAreaFrequency[m_iter%m_estimateTriggerThreshold+m_estimateTriggerThreshold*check_no] = 9999;       // 9999 stands for nonconsistent estimate
				movingInterAreaRatio[m_iter%m_estimateTriggerThreshold+m_estimateTriggerThreshold*check_no] = 9999;		
			}
			if (numberSuccessLocalCheck>=2)  // more than one succssful local crosscheck, inter-area tasks are activated
			{
				/* activate inter-area tasks */
				List<int>^ interAreaTaskIndicies = gcnew List<int>;
				/* adding channels in inter-area tasks */
				int i = 0;
				for (int check_no=0;check_no<m_localCrossChecks->Count;check_no++)
				{
					currentCheck = m_localCrossChecks[check_no];
					if (!currentCheck->flag) 
						continue;
					else
					{
						int task_no = currentCheck->task[0];
						currentTask = m_localTasks[task_no];
						/* find the maximum deviation channel */
						int channel_no = currentTask->channel[0];
						maximumValue = relativeDeviation[channel_no];
						maximumIndex = 0;
						for(int k=1;k<m_maximumChannels;k++)
						{
							if (!currentTask->channel[k]) break;
							channel_no = currentTask->channel[k];
							if (maximumValue < relativeDeviation[channel_no])
							{
								maximumValue = relativeDeviation[channel_no];
								maximumIndex = k;
							}
						}
						m_interAreaTasks[0]->channel->Add(currentTask->channel[maximumIndex]);
						m_interAreaTasks[1]->channel->Add(currentTask->channel[maximumIndex]);
						m_interAreaTasks[2]->channel->Add(currentTask->channel[maximumIndex]);
						i++;
						if (i>= m_maximumChannels) break;
					}
				}
				/* end of adding channels in inter-area tasks */

	//			/* iteration for each inter-area task */
	//			for (int task_no = 0;task_no < m_interAreaTasks->Count;task_no++)
	//			{
	//				currentTask = m_interAreaTasks[task_no];
	//				/* set currentTask->data */
	//				currentTask->m = currentTask->channel->Count;
	//				currentTask->type = "InterArea";
	//				currentTask->data = (double *) malloc(sizeof(double) * N * currentTask->m);
	//				m = m_maximumChannels;    // m updated below is the number of channels in the task
	//				for (int k=0;k<m_maximumChannels;k++)
	//				{
	//					int j = currentTask->channel[k];
 //   					if (j!=0)
	//    				{
	//						for(int i = 0; i<N ;i++)
	//	//						currentTask->data[i][k]=data[i+i_start][j-1];
	//							currentTask->data[i+k*N] = data[(i+ m_iter * (int)(m_repeatTime * FramesPerSecond) + j*N)];

	////				for(i=i_start;i<i_end && i<m_minimumSamples;i++)
	//////					    	currentTask->data[i-i_start][k]=data[i][j];
 //// 								currentTask->data[i-i_start+k*N] = data[i+j*m_minimumSamples];
	//					}
	//					else
	//					{
	//						m = k;
	//						break;
 //   					}
	//				}
	//				/* execute an analysis task */

	//				// TODO: change this
	//				//exe_task(currentTask,task_no);
	//	    		m_interAreaTasks[task_no] = currentTask;
	//				free(currentTask->data);
	//			}
	//			/* end of iteration of each inter-area task */
				
				/* iteration for each inter-area task */
				manualEvents = gcnew List<ManualResetEvent^>;

				for (int task_no = 0;task_no < m_interAreaTasks->Count;task_no++)
				{
					currentTask = m_interAreaTasks[task_no];
					/* set currentTask->data */
					currentTask->m = currentTask->channel->Count;
					currentTask->type = "InterArea";
					currentTask->data = (double *) malloc(sizeof(double) * N * currentTask->m);
					m = m_maximumChannels;    // m updated below is the number of channels in the task
					for (int k=0;k<m_maximumChannels;k++)
					{
						int j = currentTask->channel[k];
    					if (j!=0)
	    				{
							for(int i = 0; i<N ;i++)
								currentTask->data[i+k*N] = data[(i+ m_iter * (int)(m_repeatTime * FramesPerSecond) + j*N)];
						}
						else
						{
							m = k;
							break;
    					}
					}
					/* execute an analysis task */

					// Define a new manually reset wait handle for task so we can wait for task to finish
					manualEvent = gcnew ManualResetEvent(false);
					manualEvents->Add(manualEvent);

					// Initialize thread parameters
					taskState = gcnew ThreadState(currentTask, task_no, manualEvent);
					
					// Queue analysis task for execution an on independent thread
					ThreadPool::QueueUserWorkItem(gcnew WaitCallback(this, &EventDetectionAlgorithm::exe_task), taskState);
				}

				// Wait for all tasks to complete...
				WaitHandle::WaitAll(manualEvents->ToArray());
		
				for (int task_no=0; task_no < m_interAreaTasks->Count ; task_no++)
				{
					free(m_interAreaTasks[task_no]->data);
				}

				/* end of iteration of each inter-area task */


				/* start inter-area crosscheck */
				numberSuccessInterAreaCheck = 0;
				for (int check_no=0;check_no< m_interAreaChecks->Count  ;check_no++)
				{
					currentCheck = m_interAreaChecks[check_no];
					currentCheck->flag = 1;
					//if (currentCheck->task[0] == 0)
					//{
					//	inter_check_count = check_no;    // count total number of inter-area crosschecks
					//	break;
					//}
	    //			else
    	//			{
	    				for (int j=0;j<currentCheck->task->Count;j++)
		    			{
			    			int task_no = currentCheck->task[j];
		    				if (task_no==0) 
		    	    		{
			    	    		currentCheck->task_count =  j;     // count total number of tasks in this crosscheck
				    			break;
							}
							currentTask = m_interAreaTasks[task_no];
							if (!currentTask->outflag)
							{
					    		currentCheck->flag = 0;            // local crosscheck is false because of bad task estimate
								break;
							}
							else
							{
								crosscheckResultFrequency->Add(currentTask->group_f);        // record freq estimates for this crosscheck
								crosscheckResultRatio->Add(currentTask->group_ratio);// record ratio estimates for this crosscheck
							}
						}
					//}

					if (currentCheck->flag)
					{
						minimumValue = findmin(crosscheckResultFrequency);
						maximumValue = findmax(crosscheckResultFrequency);
						if (maximumValue-minimumValue<currentCheck->freq_range)   // the most dominant frequencies of all tasks are within a range
						{
							maximumValue = findmax(crosscheckResultRatio);
							minimumValue = findmin(crosscheckResultRatio);
							if (maximumValue-minimumValue<currentCheck->ratio_range)  // damping ratios of all tasks are within a range
							{
								currentCheck->freq = findmean(crosscheckResultFrequency);
								currentCheck->ratio = findmean(crosscheckResultRatio);
								/* print consistent current check */
								//fprintf(fout_inter_xcheck,"Inter-area Cross Check No %d\nFrom %.3f s to %.3f s.\n Consistent estimates: freq = %.2f Hz, damping ratio = %.4f\n\n",
							 //   		check_no+1,prony_t_start,prony_t_end,currentCheck->freq,currentCheck->ratio);
								/* record frequency and ratio used for consistent moving window check */
								movingInterAreaFrequency[m_iter%m_estimateTriggerThreshold+m_estimateTriggerThreshold*check_no] = currentCheck->freq;
								movingInterAreaRatio[m_iter%m_estimateTriggerThreshold+m_estimateTriggerThreshold*check_no] = currentCheck->ratio;
							}
							else
								currentCheck->flag = 0;   // damping ratios in this local crosscheck not within the specified range
						}
						else
						{
							currentCheck->flag =0;        // frequencies in this local crosscheck not within the specified range
						}
					}
    				if (currentCheck->flag)	numberSuccessInterAreaCheck++;          // record successful local crosscheck
					m_interAreaChecks[check_no] = currentCheck;
				}   
				/* end of inter-area crosscheck */
			}

			/* test for moving window consistent estimation for local crosschecks */
			for (int check_no=0; check_no<m_localCrossChecks->Count; check_no++)
			{
				currentCheck = m_localCrossChecks[check_no];
				if (m_iter>=m_estimateTriggerThreshold-1 && currentCheck->flag)
				{
					findmin(&movingLocalFrequency[check_no*m_estimateTriggerThreshold],m_estimateTriggerThreshold,&minimumValue,&minimumIndex);
					findmax(&movingLocalFrequency[check_no*m_estimateTriggerThreshold],m_estimateTriggerThreshold,&maximumValue,&maximumIndex);
					if (maximumValue-minimumValue<m_consistentFrequencyRange)
					{
						findmin(&movingLocalRatio[check_no*m_estimateTriggerThreshold],m_estimateTriggerThreshold,&minimumValue,&minimumIndex);
						findmax(&movingLocalRatio[check_no*m_estimateTriggerThreshold],m_estimateTriggerThreshold,&maximumValue,&maximumIndex);
						//if (maximumValue-minimumValue<m_consistentRatioRange)
							//fprintf(fout_mov_local_checks,"Consistent Local CrossCheck No %d at %.3f s.\n Consistent estimates: freq = %.2f Hz, damping ratio = %.4f\n\n",
							//		check_no+1,prony_t_end,findmean(&movingLocalFrequency[check_no*m_estimateTriggerThreshold],m_estimateTriggerThreshold),findmean(&movingLocalRatio[check_no*m_estimateTriggerThreshold],m_estimateTriggerThreshold));
					}
				}
			} 
			
			/* test for moving window consistent estimation for inter-area crosschecks */
			for (int check_no=0;check_no<m_interAreaChecks->Count ;check_no++)
			{
				currentCheck = m_interAreaChecks[check_no];
				if (m_iter>=m_estimateTriggerThreshold-1 && currentCheck->flag)
				{
					findmin(&movingInterAreaFrequency[check_no*m_estimateTriggerThreshold],m_estimateTriggerThreshold,&minimumValue,&minimumIndex);
					findmax(&movingInterAreaFrequency[check_no*m_estimateTriggerThreshold],m_estimateTriggerThreshold,&maximumValue,&maximumIndex);
					if (maximumValue-minimumValue<m_consistentFrequencyRange)
					{
						findmin(&movingInterAreaRatio[check_no*m_estimateTriggerThreshold],m_estimateTriggerThreshold,&minimumValue,&minimumIndex);
						findmax(&movingInterAreaRatio[check_no*m_estimateTriggerThreshold],m_estimateTriggerThreshold,&maximumValue,&maximumIndex);
						//if (maximumValue-minimumValue<m_consistentRatioRange)
						//	fprintf(fout_mov_inter_checks,"Consistent Inter-area CrossCheck No %d at %.3f s.\n Consistent estimates: freq = %.2f Hz, damping ratio = %.4f\n\n",
						//			check_no+1,prony_t_end,findmean(&movingInterAreaFrequency[check_no*m_estimateTriggerThreshold],m_estimateTriggerThreshold),findmean(&movingInterAreaRatio[check_no*m_estimateTriggerThreshold],m_estimateTriggerThreshold));
					}
				}
			} 

			/* window move forward */
			m_iter++;
			//prony_t_start = prony_t_start + m_repeatTime;
			//prony_t_end = prony_t_end + m_repeatTime;
		}
		free(work);
		free(movingLocalFrequency);free(movingLocalRatio);
		free(movingInterAreaFrequency);free(movingInterAreaRatio);
		fcloseall();
	}
}

void EventDetectionAlgorithm::data_preprocess(double *prony_data,int N,int m,int m_removeMeanValue,int m_normalizeData)
{
	double mean,maxabs;
	int i,j;

	/* remove mean */
	if (m_removeMeanValue)
	{
		for (j=0;j<=m-1;j++)
		{
			for (mean=0,i=0;i<=N-1;i++)
				mean += prony_data[i+j*N];
			mean /= N;
			for (i=0;i<=N-1;i++)
				prony_data[i+j*N] -= mean;
		}
	}

	/* m_normalizeData */ 
	if (m_normalizeData)
	{
		for (j=0;j<=m-1;j++)
		{
			for (maxabs=0,i=0;i<=N-1;i++)
				if (maxabs<abs(prony_data[i+j*N]))
					maxabs = abs(prony_data[i+j*N]);
			if (abs(maxabs)>1e-6)
				for (i=0;i<=N-1;i++)
				prony_data[i+j*N] /= maxabs;
		}
	}
}


double *EventDetectionAlgorithm::prony_func(double *data,int N,int m,int n)
{
	/* Prony's method to find roots */
	double *A1, *A2;
	int A1_row, A1_col, A2_row, A2_col;
	int i,j,row,col;
    int info,lda,ldb,lwork,Mls,Nls,nrhs,rank;
    double rcond;
	int *iwork;
	double *S;
	int liwork, NLVL, smlsiz, minMN;
	double *c, *A3;
	char jobvl, jobvr;
    int ldvl, ldvr, Nev;
    double *vl, *vr, *wi, *wr;
    double *zi;

	/* Solve the linear prediction problem. i.e form least square problem A1*x=A2 */
	A1_row = (N-n)*m;
	A1_col = n;
	A2_row = A1_row;
	A2_col = 1;
	A1 = (double *) malloc (sizeof(double)*A1_row*A1_col); // A1 is (N-n)*m rows, n columns
	A2 = (double *) malloc (sizeof(double)*A2_row*A2_col);   // A2 is (N-n)*m rows, 1 column
	for (i=0;i<=m-1;i++)
		for (row=0;row<=N-n-1;row++)
			for (col=0;col<=n-1;col++)
			{
				//A1[row+i*(N-n)][col] = data[n-1+row-col][i];
				A1[(row+i*(N-n))+col*A1_row] = data[(n-1+row-col)+i*N];
				//A2[row+i*(N-n)] = data[n+row][i];
				A2[row+i*(N-n)] = data[(n+row)+i*N];
			}

	/* Solve least square problem using SVD with divide and conquer */
//  SUBROUTINE DGELSD( M, N, NRHS, A, LDA, B, LDB, S, RCOND, RANK, WORK, LWORK, IWORK, INFO )
//  DGELSD computes the minimum-norm solution to a real linear least
//  squares problem:      minimize 2-norm(| b - A*x | ) using the singular value decomposition (SVD) of A.

	Mls = A1_row;
	Nls = A1_col;
	nrhs = A2_col;
	lda = Mls;
	ldb = Mls;
	minMN = min(Mls,Nls);
	S = (double *) malloc (sizeof(double)*minMN);
	rcond = max(A1_row,A1_col)*POW2_52; //If RCOND < 0, machine precision is used instead.
	smlsiz = 25;
	NLVL = max(0,(int)(log((double)minMN/(double)(smlsiz+1))/log((double)2))+1);
	liwork = 3*minMN*NLVL+11*minMN;
	iwork = (int *) malloc(sizeof(int)*max(1,liwork));
	lwork = MaxLwork;
	dgelsd_(&Mls, &Nls, &nrhs, A1, &lda, A2, &ldb, S, &rcond, &rank, work, &lwork,iwork, &info);
	if (info!=0)
	{
		fprintf(fout_message,"dgelsd_ fails in prony_func.\n");
		return NULL;
	}

	/* Find roots by eigenvalue calculation*/
    c = (double *) malloc(sizeof(double)*(n+1));
	c[0] = 1;
	for (i=1;i<=n;i++)
		c[i] = -A2[i-1];
	A3 = (double *) malloc(sizeof(double)*n*n);
	for (i=0;i<n;i++)
		for (j=0;j<n;j++)
			if (i-j==1)
				//A3[i][j]=1;
				A3[i+j*n] = 1;
			else
				//A3[i][j]=0;
				A3[i+j*n] = 0;
	for (j=0;j<n;j++)
		//A3[0][j]=-c[j+1]/c[0];
		A3[0+j*n] = -c[j+1]/c[0];

//	SUBROUTINE DGEEV( JOBVL, JOBVR, N, A, LDA, WR, WI, VL, LDVL, VR, LDVR, WORK, LWORK, INFO )
//  DGEEV computes for an N-by-N real nonsymmetric matrix A, the
//  eigenvalues and, optionally, the left and/or right eigenvectors.
	jobvl = 'N';
	jobvr = 'N';
	Nev = n;
	lda = Nev;
	wr = (double *) malloc(sizeof(double)*Nev);
	wi = (double *) malloc(sizeof(double)*Nev);
	vr = (double *) malloc(sizeof(double));
	vl = (double *) malloc(sizeof(double));
	ldvl = Nev;
	ldvr = Nev;
	lwork = MaxLwork;
	dgeev_( &jobvl, &jobvr, &Nev, A3, &lda, wr, wi, vl, &ldvl, vr, &ldvr, work, &lwork, &info);
	if (info!=0)
	{	
		fprintf(fout_message,"dgeev_ fails in prony_func.\n");
		return NULL;
	}
	
	zi = (double *) malloc(sizeof(double)*n*2);
	for (i=0;i<n;i++)
	{
		zi[2*i] = wr[i];
		zi[2*i+1] = wi[i];
	}
	free(iwork);
	free(A1);
	free(A2);
    free(S);
	free(c); 
	free(A3);
    free(vl);free(vr);free(wi);free(wr);
	return zi;
}


double *EventDetectionAlgorithm::matrix_pencil_func(double *data,int N,int m, int *n)
{
	/* Matrix Pencil method to find roots */
	int L, i, row, col, Y_row, Y_col;
	double *Y;
	char jobz;
	int info, lda, ldu, ldvt, lwork, Msvd, Nsvd;
    int *iwork;
	double *S, *U, *VT;
    int minMN;
	double svd_threshold = 1e-1;
	int M;
	double *V1_prime, *V2_prime;
	int V1prime_row,V1prime_col,V2prime_row,V2prime_col;
    double *A0,*A1;
	char transA,transB;
	int ldb,ldc,Mmul,Nmul,Kmul;
	double alpha,beta;
    char jobvl,jobvr;
    int Nev,ldvl,ldvr;
	double *wr,*wi,*vl,*vr;
	double *zi;

	/* Form matrix Y*/
	L = (int)N/2;
	Y_row = N - L;
	Y_col = (L+1)*m;
	Y = (double *) malloc(sizeof(double)*Y_row*Y_col);
    if (Y==NULL)
	{
		fprintf(fout_message,"Error in malloc in matrix_pencil_func.\n");
		return NULL;
	}
	for (i=0;i<m;i++)
		for (row=0;row<N-L;row++)
			for (col=0;col<L+1;col++)
				//Y[row][col+(L+1)*i] = data[row+col,i];
                Y[row+(col+(L+1)*i)*Y_row] = data[row+col+i*N];

//	SUBROUTINE DGESDD( JOBZ, M, N, A, LDA, S, U, LDU, VT, LDVT, WORK, LWORK, IWORK, INFO )
//  DGESDD computes the singular value decomposition (SVD) of a real
//  M-by-N matrix A, optionally computing the left and right singular
//  vectors. The SVD is written as A = U * SIGMA * transpose(V)
	jobz = 'S';
	Msvd = Y_row;
	Nsvd = Y_col;
	lda = Y_row;
	minMN = min(Msvd,Nsvd);
	S = (double *) malloc(sizeof(double)*minMN);
	ldu = Y_row;
	U = (double *) malloc(sizeof(double)*ldu*minMN);
	ldvt = Nsvd;
	VT = (double *) malloc(sizeof(double)*ldvt*Nsvd);
	if (VT==NULL)
	{
		fprintf(fout_message,"Error in malloc in matrix_pencil_func.\n");
		return NULL;
	}
	iwork = (int*)malloc(sizeof(int)*8*minMN);
	lwork = MaxLwork;
    dgesdd_( &jobz, &Msvd, &Nsvd, Y, &lda, S, U, &ldu, VT, &ldvt, work, &lwork, iwork, &info);
	if (info!=0)
	{
		fprintf(fout_message,"dgesdd_ fails in matrix_pencil_func\n");
		return NULL;
	}

	
	/* Determine M significant singular values */
	M = minMN;
	for (i=0; i<Y_row;i++)
		if (S[i]<S[0]*svd_threshold)
		{
			M = i;
			break;
		}
    /* Form V1_prime and V2_prime */
	V1prime_row = L*m;
	V1prime_col = M;
	V2prime_row = V1prime_row;
	V2prime_col = V1prime_col;
	V1_prime = (double *) malloc(sizeof(double)*V1prime_row*V1prime_col);
    V2_prime = (double *) malloc(sizeof(double)*V2prime_row*V2prime_col);
	if (V1_prime==NULL || V2_prime==NULL)
	{
		fprintf(fout_message,"Error in malloc in matrix_pencil_func.\n");
		return NULL;
	}
	for (i=0;i<m;i++)
		for (row=0;row<L;row++)
			for (col=0;col<M;col++)
			{
				//V1_prime[row+i*L][col] = V[row+i*(L+1)][col] = VT[col][row+i*(L+1)];
                //V1_prime[row+i*L+col*V1prime_row] = V[row+i*(L+1)+col*Nsvd];
				V1_prime[row+i*L+col*V1prime_row] = VT[col+(row+i*(L+1))*Nsvd];
				//V2_prime[row+i*L][col] = V[row+1+i*(L+1)][col] = VT[col][row+1+i*(L+1)];
				//V2_prime[row+i*L+col*V2prime_row] = V[row+1+i*(L+1)+col*Nsvd];
				V2_prime[row+i*L+col*V2prime_row] = VT[col+(row+1+i*(L+1))*Nsvd];
			}

	/* Matrix multiplication. A1=V2_prime'*pinv(V1_prime'); */
//  SUBROUTINE DGEMM(TRANSA,TRANSB,M,N,K,ALPHA,A,LDA,B,LDB,BETA,C,LDC)
//  Purpose
//  DGEMM  performs one of the matrix-matrix operations
//  C := alpha*op( A )*op( B ) + beta*C,
	A0 = pinv(matrix_transpose(V1_prime,V1prime_row,V1prime_col),V1prime_col, V1prime_row);
    transA = 'T';
	transB = 'N';
	Mmul = V2prime_col;
	Nmul = V1prime_col;
	Kmul = V2prime_row;
	alpha = 1;
	beta = 0;
	lda = V2prime_row;
	ldb = Kmul;
	ldc = Mmul;
	A1 = (double *) malloc(sizeof(double)*Mmul*Nmul);
	if (A1==NULL)
	{
		fprintf(fout_message,"Error in malloc in matrix_pencil_func.\n");
		return NULL;
	}
	dgemm_(&transA, &transB, &Mmul, &Nmul, &Kmul, &alpha, V2_prime, &lda, A0, &ldb, &beta, A1, &ldc);

	/* calculate zi's from eigenvalues of A1*/
	jobvl = 'N';
	jobvr = 'N';
	Nev = M;
	lda = Nev;
	wr = (double *) malloc(sizeof(double)*Nev);
	wi = (double *) malloc(sizeof(double)*Nev);
	vr = (double *) malloc(sizeof(double));
	vl = (double *) malloc(sizeof(double));
	ldvl = Nev;
	ldvr = Nev;
	lwork = MaxLwork;
    dgeev_( &jobvl, &jobvr, &Nev, A1, &lda, wr, wi, vl, &ldvl, vr, &ldvr, work, &lwork, &info);
	if (info!=0)
	{
		fprintf(fout_message,"dgeev_ fails in matrix_pencil_func.\n");
		return NULL;
	}

	zi = (double *) malloc(sizeof(double)*2*M);
	for (i=0;i<M;i++)
	{
		zi[2*i] = wr[i];
		zi[2*i+1] = wi[i];
	}
	*n = M;
    free(Y);
    free(iwork);
	free(S);
	free(U);
	free(VT);
	free(V1_prime);free(V2_prime);
    free(A0);free(A1);
	free(wr);free(wi);free(vl);free(vr);
	return zi;
}


double *EventDetectionAlgorithm::HTLStack_func(double *data,int N,int m, int *n)
{
	/* HLTStack method to find roots */
	int i,j,row,col;
	int M,L,K;
	int Hs_row,Hs_col;
	double *Hs;
	char jobz;
	int info, lda, ldu, ldvt, lwork, Msvd, Nsvd;
    int *iwork;
	double *S, *U, *VT;
    int minMN;
	double svd_threshold = 1e-1;
    double *U_hat_up,*U_hat_down;
	int Uhatup_row,Uhatup_col;
    double *A,*W,*W12,*W22,*C;
    char transA,transB;
	int Mmul,Nmul,Kmul,ldb,ldc;
	double alpha,beta;
	char jobvl,jobvr;
	int Nev,ldvl,ldvr;
	double *wr,*wi,*vr,*vl,*zi;
	double *invW22;
//  char jobu,jobvt;

	L = (int)N/2;
	M = N-L+1;
	Hs_row = L;
	Hs_col = M*m;
	Hs = (double *) malloc(sizeof(double)*Hs_row*Hs_col);
    if (Hs==NULL)
	{
		fprintf(fout_message,"Error in malloc in HTLStack_func.\n");
		return NULL;
	}
	for (i=0;i<m;i++)
		for (row=0;row<L;row++)
			for (col=0;col<M;col++)
				//Hs[row][col+M*i] = data[row+col][i];
                Hs[row+(col+M*i)*Hs_row] = data[row+col+i*N];

//	SUBROUTINE DGESDD( JOBZ, M, N, A, LDA, S, U, LDU, VT, LDVT, WORK, LWORK, IWORK, INFO )
//  DGESDD computes the singular value decomposition (SVD) of a real
//  M-by-N matrix A, optionally computing the left and right singular
//  vectors. The SVD is written as A = U * SIGMA * transpose(V)
	jobz = 'S';
	Msvd = Hs_row;
	Nsvd = Hs_col;
	lda = Hs_row;
	minMN = min(Msvd,Nsvd);
	S = (double *) malloc(sizeof(double)*minMN);
	ldu = Hs_row;
	U = (double *) malloc(sizeof(double)*ldu*minMN);
	ldvt = Nsvd;
	VT = (double *) malloc(sizeof(double)*ldvt*Nsvd);
	if (VT==NULL)
	{
		fprintf(fout_message,"Error in malloc HTLStack_func.\n");
		return NULL;
	}
	iwork = (int*)malloc(sizeof(int)*8*minMN);
	lwork = MaxLwork;
    dgesdd_( &jobz, &Msvd, &Nsvd, Hs, &lda, S, U, &ldu, VT, &ldvt, work, &lwork, iwork, &info);
	if (info!=0)
	{
		fprintf(fout_message,"dgesdd_ fails HTLStack_func. \n");
		return NULL;
	}

	/* Determine K significant singular values */
	K = minMN;
	for (i=0; i<Hs_row;i++)
		if (S[i]<S[0]*svd_threshold)
		{
			K = i;
			break;
		}
	*n = K;

	/* form U_hat_up and U_hat_down */
	Uhatup_row = L-1;
	Uhatup_col = K;
	U_hat_up = (double *) malloc(sizeof(double)*Uhatup_row*Uhatup_col);
	U_hat_down = (double *) malloc(sizeof(double)*Uhatup_row*Uhatup_col);
	for (i=0;i<Uhatup_row;i++)
		for (j=0;j<Uhatup_col;j++)
		{
			//U_hat_up[i][j] = U[i+1][j];
			U_hat_up[i+j*Uhatup_row] = U[i+1+j*ldu];
			U_hat_down[i+j*Uhatup_row] = U[i+j*ldu];
		}

    /* form A = [U_hat_down U_hat_up]; */
	A = (double *) malloc(sizeof(double)*Uhatup_row*Uhatup_col*2);
	for (i=0;i<Uhatup_row;i++)
	{
		for (j=0;j<Uhatup_col;j++)
			//A[i][j] = U_hat_down[i][j];
			A[i+j*Uhatup_row] = U_hat_down[i+j*Uhatup_row];
		for (j=Uhatup_col;j<Uhatup_col*2;j++)
			//A[i][j] = U_hat_up[i][j-Uhatup_col];
			A[i+j*Uhatup_row] = U_hat_up[i+(j-Uhatup_col)*Uhatup_row];
	}

	/* svd of [U_hat_down U_hat_up] */
	jobz = 'S';
	Msvd = Uhatup_row;
	Nsvd = Uhatup_col*2;
	lda = Uhatup_row;
	minMN = min(Msvd,Nsvd);
	S = (double *) malloc(sizeof(double)*minMN);
	ldu = Msvd;
	U = (double *) malloc(sizeof(double)*ldu*minMN);
	ldvt = Nsvd;
	VT = (double *) malloc(sizeof(double)*ldvt*Nsvd);
	if (VT==NULL)
	{
		fprintf(fout_message,"Error in malloc HTLStack_func.\n");
		return NULL;
	}
	iwork = (int*)malloc(sizeof(int)*8*minMN);
	lwork = MaxLwork;
    dgesdd_( &jobz, &Msvd, &Nsvd, A, &lda, S, U, &ldu, VT, &ldvt, work, &lwork, iwork, &info);
	if (info!=0)
	{
		fprintf(fout_message,"dgesdd_ fails HTLStack_func.\n");
		return NULL;
	}

	/* form W12 and W22*/
	W = matrix_transpose(VT,ldvt,ldvt);
	W12 = (double *) malloc(sizeof(double)*K*K);
	W22 = (double *) malloc(sizeof(double)*K*K);
	for (i=0;i<K;i++)
		for (j=0;j<K;j++)
			//W12[i][j] = W[i][j+K];   //W is 2K*2K
			W12[i+j*K] = W[i+(j+K)*2*K];
	for (i=0;i<K;i++)
		for (j=0;j<K;j++)
			//W22[i][j] = W[i+K][j+K];
			W22[i+j*K] = W[i+K+(j+K)*2*K];

//  matrix_write(W,2*K,2*K);
    transA = 'N';
	transB = 'N';
	Mmul = K;
	Nmul = K;
	Kmul = K;
	alpha = -1;
	beta = 0;
	lda = Mmul;
	ldb = Kmul;
	ldc = Mmul;
	C = (double *) malloc(sizeof(double)*Mmul*Nmul);
	invW22 = matrix_inverse(W22,K);
	if (invW22 == NULL)
	{
		free(Hs);
		free(iwork);
		free(S);
		free(U);free(VT);
		free(U_hat_up);free(U_hat_down);
		free(A);free(W);free(W12);free(W22);free(C);
		free(invW22);
		return NULL;
	}
	dgemm_(&transA, &transB, &Mmul, &Nmul, &Kmul, &alpha, W12, &lda, invW22, &ldb, &beta, C, &ldc);
	
	/* calculate zi's from eigenvalues of C,ie -W12*inv(W22)*/
	jobvl = 'N';
	jobvr = 'N';
	Nev = K;
	lda = Nev;
	wr = (double *) malloc(sizeof(double)*Nev);
	wi = (double *) malloc(sizeof(double)*Nev);
	vr = (double *) malloc(sizeof(double));
	vl = (double *) malloc(sizeof(double));
	ldvl = Nev;
	ldvr = Nev;
	lwork = MaxLwork;
    dgeev_( &jobvl, &jobvr, &Nev, C, &lda, wr, wi, vl, &ldvl, vr, &ldvr, work, &lwork, &info);
	if (info!=0)
	{
		fprintf(fout_message,"dgeev_ fails HTLStack_func.\n");
		return NULL;
	}

	zi = (double *) malloc(sizeof(double)*2*K);
	for (i=0;i<K;i++)
	{
		zi[2*i] = wr[i];
		zi[2*i+1] = wi[i];
	}
    free(Hs);
    free(iwork);
	free(S);
	free(U);free(VT);
    free(U_hat_up);free(U_hat_down);
    free(A);free(W);free(W12);free(W22);free(C);
	free(wr);free(wi);free(vr);free(vl);
	free(invW22);
	return zi;
}

double *EventDetectionAlgorithm::matrix_transpose(double *matrix, int m, int n)
{
	int i,j;
	double *result;
	// matrix is m*n, result is n*m
	result = (double *) malloc(sizeof(double)*m*n);
	if (result==NULL)
	{
		fprintf(fout_message,"Error in malloc in matrix_transpose.\n");
		return NULL;
	}
	for (i=0;i<n;i++)
		for (j=0;j<m;j++)
			//result[i][j] = matrix[j][i];
            result[i+j*n] = matrix[j+i*m];
	return result;
}

double *EventDetectionAlgorithm::matrix_inverse(double *A, int N)
{
//	  SUBROUTINE DGESV( N, NRHS, A, LDA, IPIV, B, LDB, INFO )
//    DGESV computes the solution to a real system of linear equations A * X = B,
    int info, lda, ldb, nrhs;
    int *ipiv;
	double *B;
	int i,j;
	B = (double *) malloc(sizeof(double)*N*N);
	if (B==NULL)
	{
		fprintf(fout_message,"Error in malloc in matrix_inverse.\n");
		return NULL;
	}
	for (i=0;i<N;i++)
		for (j=0;j<N;j++)
			if (i==j)
				//B[i][j] = 1;
				B[i+j*N] = 1;
			else
				//B[i][j] = 0;
				B[i+j*N] = 0;
    ipiv = (int*)malloc(sizeof(int)*N);
	lda = N;
	ldb = N;
	nrhs = N;
	dgesv_( &N, &nrhs, A, &lda, ipiv, B, &ldb, &info);
	free(ipiv);
    if (info!=0)
	{
		fprintf(fout_message,"dgesv_ fails when finding matrix inverse\n");
		return NULL;
	}
	else
		return B;

}

double *EventDetectionAlgorithm::pinv(double *A,int m, int n)
{
    /* find pseudo-inverse of matrix A */
//  matlab code for pinv
//	function B = pinv(A)
//  [U,S,V] = svd(A,0); % if m>n, only compute first n cols of
//  s = diag(S);
//  r = sum(s > tol); % rank
//  w = diag(ones(r,1) ./ s(1:r));
//  B = V(:,1:r) * w * U(:,1:r)¡¯;

	/* calculate pseudo-inverse of A */ 
	double *result;
	char jobz;
	int info, lda, ldu, ldvt, lwork, Msvd, Nsvd;
    int *iwork;
	double *S, *U, *VT;
	double norm_A,threshold;
	int i,j,minMN,r;
	double *W;
    double alpha,beta;
	int ldb,ldc,Mmul,Nmul,Kmul;
    char transA,transB;
    double *C;

    /* Singular value decompostion */
	jobz = 'S';
	Msvd = m;
	Nsvd = n;
	lda = m;
	minMN = min(Msvd,Nsvd);
	S = (double *) malloc(sizeof(double)*minMN);
	ldu = m;
	U = (double *) malloc(sizeof(double)*ldu*minMN);
	ldvt = Nsvd;
	VT = (double *) malloc(sizeof(double)*ldvt*Nsvd);
	if (VT==NULL)
	{
		fprintf(fout_message,"Error in malloc in pinv.\n");
		return NULL;
	}
	iwork = (int*)malloc(sizeof(int)*8*minMN);
	lwork = MaxLwork;
    dgesdd_( &jobz, &Msvd, &Nsvd, A, &lda, S, U, &ldu, VT, &ldvt, work, &lwork, iwork, &info);
	if (info!=0)
	{
		fprintf(fout_message,"dgesdd_ fails in pinv.\n");
		return NULL;
	}

	/* calculate pseudo-inverse */ 
    norm_A = S[0];
	threshold = max(m,n)*norm_A*POW2_52;
    r = minMN;
	for (i=0;i<minMN;i++)
		if (S[i]<threshold)
		{
			r = i;
			break;
		}
    W = (double *) malloc(sizeof(double)*r*r);
	for (i=0;i<r;i++)
		for (j=0;j<r;j++)
			if(i==j)
				//W[i][j] = 1/S[i];
				W[i+j*r] = 1/S[i];
			else
				//W[i][j] = 0;
                W[i+j*r] = 0;

    /* Matrix multiplication of Vr and W.*/
	// Vr is an n*r matrix, the first r columns of V, i.e. the first r rows of VT.
	// W is an r*r matrix.
    transA = 'T';
	transB = 'N';
	Mmul = n;   // row dimension after transpose
	Nmul = r;
	Kmul = r;
	alpha = 1;
	beta = 0;
	lda = n;
	ldb = Kmul;
	ldc = Mmul;
	C = (double *) malloc(sizeof(double)*Mmul*Nmul);
	dgemm_(&transA, &transB, &Mmul, &Nmul, &Kmul, &alpha, VT, &lda, W, &ldb, &beta, C, &ldc);
	/* Matrix multiplication of C and Ur.*/
	// C is an n*r matrix, Ur is an r*m matrix.
	// Ur = matrix_transpose(U,m,r), i.e, transpose of first r columns of U; U is an m*n matrix.
	transA = 'N';
	transB = 'T';
	Mmul = n;
	Nmul = m;
	Kmul = r;
	alpha = 1;
	beta = 0;
	lda = Mmul;
	ldb = m;
	ldc = Mmul;
	result = (double *) malloc(sizeof(double)*Mmul*Nmul);
	dgemm_(&transA, &transB, &Mmul, &Nmul, &Kmul, &alpha, C, &lda, U, &ldb, &beta, result, &ldc);
    free(iwork);
 	free(S);free(U);free(VT);
    free(W);
    free(C);
	return result;
}


void EventDetectionAlgorithm::cal_output(double *data, int N, int m, int n, double *zi, double dt, double *out_amp, double *out_pha,
				double *out_damp, double *out_f, double *out_dpratio)
{
	/* calculate modal analysis outputs such as frequency and damping ratios. */
	int i,j;
	double *fai,*rhs;
    int info, lda, ldb, lwork, Mls, Nls, nrhs, rank;
    double rcond;
    int *iwork;
    double *rwork,*S;
	int smlsiz,minMN,NLVL,liwork,lrwork;
    double re_lambda,im_lambda;

	/* Solve Vandermonde problem by least square */
	fai = (double *) malloc(sizeof(double)*N*n*2);  //fai is N*n complex matrix
	rhs = (double *) malloc(sizeof(double)*N*m*2);      //rhs is N*m complex matrix
	for(i=0;i<N;i++)
		for(j=0;j<n;j++)
			if (i==0)
			{
				//fai[i][j] = 1;
				fai[2*i+j*2*N] = 1;    //real part
				fai[2*i+1+j*2*N] = 0;  //imag part
			}
			else
			{
				//fai[i][j] = z[j]^i; or fai[i][j] = fai[i-1][j]*z[j];
				fai[2*i+j*2*N] = fai[2*(i-1)+j*2*N]*zi[2*j] - fai[2*(i-1)+1+j*2*N]*zi[2*j+1]; //real part
				fai[2*i+1+j*2*N] = fai[2*(i-1)+j*2*N]*zi[2*j+1] + zi[2*j]*fai[2*(i-1)+1+j*2*N]; //imag part
			}

	for(i=0;i<N;i++)
		for(j=0;j<m;j++)
		{
			rhs[2*i+j*2*N] = data[i+j*N];  //data is N*m real matrix
			rhs[2*i+1+j*2*N] = 0;
		}

//  SUBROUTINE ZGELSD( M, N, NRHS, A, LDA, B, LDB, S, RCOND, RANK, WORK, LWORK, RWORK, IWORK, INFO )
	Mls = N;
	Nls = n;
	nrhs = m;
	lda = Mls;
	ldb = Mls;
	rcond = max(Mls,Nls)*POW2_52;
	smlsiz = 25;
	minMN = min(Mls,Nls);
	NLVL = max(0,(int)(log((double)(minMN/(smlsiz+1))/log((double)2))+1));
	liwork = 3*minMN*NLVL+11*minMN;
	iwork = (int *) malloc(sizeof(int)*max(1,liwork));
    lrwork = 10*Nls+2*Nls*smlsiz+8*Nls*NLVL +3*smlsiz*nrhs+(smlsiz+1)*(smlsiz+1);
	rwork = (double *) malloc(sizeof(double)*max(1,lrwork));
	if (iwork==NULL ||rwork==NULL)
	{
		fprintf(fout_message,"Error in malloc in cal_output.\n");
		throw gcnew ArgumentException("Error in malloc in cal_output");
	}
	S = (double *) malloc(sizeof(double)*minMN);
	lwork = MaxLwork;
    zgelsd_(&Mls, &Nls, &nrhs, fai, &lda, rhs, &ldb, S, &rcond, &rank, work, &lwork, rwork, iwork, &info);
	if (info!=0)
	{
		fprintf(fout_message,"zgelsd_ fails in cal_output\n");
		throw gcnew ArgumentException("zgelsd_ fails in cal_output");
	}
    
	/* Modal outputs calculation */
	for (i=0;i<n;i++)
	{
		for (j=0;j<m;j++)
		{
			//out_amp[i][j] = sqrt(rhs[2*i][j]*rhs[2*i][j]+rhs[2*i+1][j]*rhs[2*i+1][j]);
			out_amp[i+j*n] = sqrt(rhs[2*i+j*2*N]*rhs[2*i+j*2*N]+rhs[2*i+1+j*2*N]*rhs[2*i+1+j*2*N]);
			//out_pha[i][j] = atan2(rhs[2*i+1][j],rhs[2*i][j];
			out_pha[i+j*n] = atan2(rhs[2*i+1+j*2*N],rhs[2*i+j*2*N])/PI*180;
		}
		re_lambda = log(zi[2*i]*zi[2*i]+zi[2*i+1]*zi[2*i+1])/2; // lambda = log(z) = log(abs(z)) + i*atan2(im(z),re(z))
		im_lambda = atan2(zi[2*i+1],zi[2*i]);
		out_damp[i] = re_lambda/dt;
		out_f[i] = im_lambda/2/PI/dt;
		out_dpratio[i] = -re_lambda/sqrt(re_lambda*re_lambda+im_lambda*im_lambda);
	}
    free(fai);free(rhs);
    free(iwork);free(rwork);free(S);
}

void EventDetectionAlgorithm::display(double *out_amp,double *out_pha,double *out_damp,double *out_f,double *out_dpratio,double *rel_energy,int m_displayDetail, AnalysisTask^ currentTask)
{
	int i,j;
	int m_Nx,m_Ny;
	double *sort_amp;
	int k;
	int disp_n;
	int n = currentTask->n;
	int m = currentTask->m;
	String^ type = currentTask->type;

	m_Nx = 1;
	m_Ny = 1;
	sort_amp = (double *) malloc(sizeof(double)*n);
	for (i=0;i<m;i++)
	{
		if (String::Compare(type,"Local") == 0)
			fprintf(fout_local_details,"Signal No %d\n",i+1);
		else
			fprintf(fout_inter_details,"Signal No %d\n",i+1);
		dcopy_(&n,&rel_energy[i*n],&m_Nx,sort_amp,&m_Ny);   // copy rel_energy(:,i) to sort_amp
		qsort(sort_amp,n,sizeof(double),comp_nums);
		disp_n = 0;
		if (sort_amp[0]<1e-6)
		{
			if (String::Compare(type,"Local") == 0)
			{
				currentTask->badEstimation = true;
				fprintf(fout_local_details,"Bad estimates.\n\n\n");
			}
			else
			{
				currentTask->badEstimation = true;
				fprintf(fout_inter_details,"Bad estimates.\n\n\n");
			}
		}
		for (j=0;sort_amp[j]>m_energyDisplayThreshold;j++)
		{
			k = findnum(sort_amp[j],&rel_energy[i*n],n);
			if (k==-1) 	continue;
			disp_n++;
			if (disp_n>m_maximumDisplayModes) break;
			if (m_displayDetail)
			{
				if (String::Compare(type,"Local") == 0)
					fprintf(fout_local_details,"amplitude = %.4f, phase = %.4f, damp = %.4f, frequency = %.4f Hz, damping ratio = %.4f, relatvie energy = %.4f\n\n\n",
					out_amp[i*n+k],out_pha[i*n+k], out_damp[k], out_f[k],out_dpratio[k], rel_energy[i*n+k]);
				else
					fprintf(fout_inter_details,"amplitude = %.4f, phase = %.4f, damp = %.4f, frequency = %.4f Hz, damping ratio = %.4f, relatvie energy = %.4f\n\n\n",
					out_amp[i*n+k],out_pha[i*n+k], out_damp[k], out_f[k],out_dpratio[k], rel_energy[i*n+k]);
			}
		}
	}
	free(sort_amp);
}

void EventDetectionAlgorithm::findmax(double *v, int N, double *max, int *k)
{
	/* find the maximum from vectro v, return the maximum value max and location k */
	int i;
	if (N<1) 
	{
		fprintf(fout_message,"N must be larger than 1 in function 'findmax'.\n");
		throw gcnew ArgumentException("Error in findmax");
	}
	*max = v[0];
	*k = 0;
	for (i=1;i<N;i++)
	{
		if (v[i]>*max)
		{
			*max = v[i];
			*k = i;
		}
	}
}

double EventDetectionAlgorithm::findmax(List<double>^ v)
{
	return TVA::Collections::Common::Maximum<double>(v);
}

void EventDetectionAlgorithm::findmin(double *v, int N, double *min, int *k)
{
	/* find the minimum from vectro v, return the minmum value max and location k */
	int i;
	if (N<1) 
	{
		fprintf(fout_message,"N must be larger than 1 in function 'findmin'.\n");
		throw gcnew ArgumentException("Error in findmin");
	}
	*min = v[0];
	*k = 0;
	for (i=1;i<N;i++)
	{
		if (v[i]<*min)
		{
			*min = v[i];
			*k = i;
		}
	}
}

double EventDetectionAlgorithm::findmin(List<double>^ v)
{
	return TVA::Collections::Common::Minimum<double>(v);
}

double EventDetectionAlgorithm::findmean(double *v, int N)
{
	/* find the mean value from vectro v */
	int i;
	double mean;
	if (N<1) 
	{	fprintf(fout_message,"N must be larger than 1 in function 'findmin'.\n");return 0;}
    mean =0;
	for (i=0;i<N;i++)
		mean += v[i];
	return mean/N;
}

double EventDetectionAlgorithm::findmean(List<double>^ v)
{
	return TVA::Math::Common::Average(v);
}

int EventDetectionAlgorithm::findnum(double num, double *list, int n)
{
	/* look for num in list, return the index. */
	int i;
	for (i=0;i<n;i++)
		if (num==list[i])
			return i;
	return -1;
}

void EventDetectionAlgorithm::matrix_write(double *A,int m,int n)
{
	/* write matrix A into message.txt for debugging purpose */
	int i,j;
	for (i=0;i<m;i++)
	{
		for (j=0;j<n;j++)
			fprintf(fout_message,"\t%.16f",A[i+j*m]);
		fprintf(fout_message,"\n");
	}
	fclose(fout_message);
}

void EventDetectionAlgorithm::exe_task(System::Object^ state)
{
	/* This fuction performs one single analysis task. The results are stored in currentTask */ 
	ThreadState^ taskState = safe_cast<ThreadState^>(state);
	AnalysisTask^ p_task = taskState->p_task;
	int task_no = taskState->task_no;
	ManualResetEvent^ manualEvent = taskState->manualEvent;
	int i,j,k; 
	int m_Nx=1,m_Ny=1;
	double* sort_amp = (double*)malloc(sizeof(double) * m_channelCount);
	int sig_count;
	double sig_f;
	double maximumValue;
    int maximumIndex;
	AnalysisTask^ currentTask;
	int n;                             // model order
	double *zi;                        // identified eigenvalues 
	double *out_amp,*out_pha,*out_damp,*out_f,*out_dpratio;   // outputs
	double *rel_energy;
	int N;
	int m;
	String^ type;

	currentTask = p_task;
	N = currentTask->N;
	m = currentTask->m;
	type = currentTask->type;
	currentTask->badEstimation = false;
	/* data preprocessing */
    data_preprocess(currentTask->data,N,m,m_removeMeanValue,m_normalizeData);
		
	/* roots calculation */
	if (String::Compare(currentTask->method,"Prony")==0)     //Prony's Method
	{
		if (currentTask->n ==0)
			n = (int) floor(N/2.0*11/12)+1;
		else
			n = currentTask->n;
		if (n>128)	n = 128;
		zi = prony_func(currentTask->data,N,m,n);
    }
	else if (String::Compare(currentTask->method,"MatrixPencil")==0)  //Matrix Pencil
    {
		zi = matrix_pencil_func(currentTask->data,N,m,&n);  // n is output, which is the parameter M in matrix pencil
    }
	else if (String::Compare(currentTask->method,"HTLS")==0) // HTLS
    {
		zi = HTLStack_func(currentTask->data,N,m,&n);
    }
	else
	{	fprintf(fout_message,"Invalid analysis method.");
        throw gcnew ArgumentException("Invalid analysis method");
    }

    if (zi == NULL)
	{
		currentTask->badEstimation = true;
		currentTask->outflag = false;
		p_task= currentTask;
		if (m_displayDetail)
		{
			if (String::Compare(currentTask->type,"Local")==0)
			{
				//fprintf(fout_local_details,"%s Task No.%d\nFrom  %.3f s to %.3f s.\n%s\n\n",type,task_no+1,prony_t_start,prony_t_end,currentTask->method);
	            fprintf(fout_local_details," Bad estimates. \n\n");
			}
			else
			{
				//fprintf(fout_inter_details,"%s Task No.%d\nFrom  %.3f s to %.3f s.\n%s\n\n",type,task_no+1,prony_t_start,prony_t_end,currentTask->method);
	            fprintf(fout_inter_details," Bad estimates. \n\n");
			}
		}
	}

	/* modal parameters calculation */
    out_amp = (double *) malloc(sizeof(double)*n*m);
    out_pha = (double *) malloc(sizeof(double)*n*m);
    out_damp = (double *) malloc(sizeof(double)*n);
    out_f = (double *) malloc(sizeof(double)*n);
    out_dpratio = (double *) malloc(sizeof(double)*n);
    cal_output(currentTask->data,N,m,n,zi,1.0/FramesPerSecond,out_amp,out_pha,out_damp,out_f,out_dpratio); //out_* are outputs
	
	/* find the most dominant mode in each channel by relative energy */
	currentTask->freq = (double *) malloc(sizeof(double)*m);
	currentTask->ratio = (double *) malloc(sizeof(double)*m);
	rel_energy = (double *) malloc(sizeof(double)*n*m);
  	for (i=0;i<m;i++)
	{
		for (j=0;j<n;j++)
		{
		//	rel_energy[j][i] = 0;
			rel_energy[j+i*n] = 0;
		    if (out_f[j]<1e-6) continue;  //skip negative and zero frequency
			if (out_amp[j+i*n]>1e6 || out_amp[j+i*n]<1e-6) continue;

			for (k=0;k<N;k++)
			{
				//	rel_energy[j][i]+= out_amp[j][i]*exp(out_damp[j]*k*1.0/expectedMeasurementsPerSecond)*cos(2*PI*out_f[j]*k*1.0/expectedMeasurementsPerSecond+out_pha[j][i]);
				rel_energy[j+i*n]+= out_amp[j+i*n]*exp(out_damp[j]*k*1.0/FramesPerSecond)*2*cos(2*PI*out_f[j]*k*1.0/FramesPerSecond+out_pha[j+i*n]*PI/180)
				               *out_amp[j+i*n]*exp(out_damp[j]*k*1.0/FramesPerSecond)*2*cos(2*PI*out_f[j]*k*1.0/FramesPerSecond+out_pha[j+i*n]*PI/180);
		    }
			rel_energy[j+i*n]/= N;
		}
		findmax(&rel_energy[i*n], n, &maximumValue, &maximumIndex);    // find maximum mode energy in each channel
		if (maximumValue!=0)
		{
			for (j=0;j<n;j++)
				rel_energy[j+i*n]/= maximumValue;         // calculate relative energy
		}
		else
		{
			currentTask->freq[i] = 9999;
			currentTask->ratio[i] = 9999;
			continue;
		}
		currentTask->freq[i] = abs(out_f[maximumIndex]);
	    currentTask->ratio[i] = out_dpratio[maximumIndex];
	}

	/* detailed result display */
   	if (m_displayDetail)
    {
		//if (strcmp(type,"Local")==0)
		//	fprintf(fout_local_details,"%s Task No.%d\nFrom  %.3f s to %.3f s.\n%s\n\n",type,task_no+1,prony_t_start,prony_t_end,currentTask->method);
		//else
		//	fprintf(fout_inter_details,"%s Task No.%d\nFrom  %.3f s to %.3f s.\n%s\n\n",type,task_no+1,prony_t_start,prony_t_end,currentTask->method);
		display(out_amp,out_pha,out_damp,out_f,out_dpratio,rel_energy,m_displayDetail,currentTask);
    }

	/* find whether there is a dominant signal in all channels of the current task */
	if (m==1)
	{   // only one channel
		currentTask->group_f = sort_amp[i];
		currentTask->group_ratio = currentTask->ratio[findnum(sort_amp[i],currentTask->freq,m)];
		currentTask->outflag = true;
	}
	else
	{
		dcopy_(&m,currentTask->freq,&m_Nx,sort_amp,&m_Ny);  // copy currentTask->freq to sort_amp
		qsort(sort_amp,m,sizeof(double),comp_nums);
		sig_count = 0;
		sig_f = 0;
		for (i=0;i<m;i++)
		{
			if (sort_amp[i] == 9999) continue;
			if (abs(sig_f-sort_amp[i])>1e-6)
			{
				sig_f = sort_amp[i];
				sig_count = 1;
			}
			else
			{
				sig_count++;
			}
			if (m==2 && sig_count==2)
			{   // two channels
				currentTask->group_f = sort_amp[i];
				currentTask->group_ratio = currentTask->ratio[findnum(sort_amp[i],currentTask->freq,m)];
			    currentTask->outflag = true;
			    break;
			}
			if (sig_count>=m/2 && m>2)         // if more than half of the channels have the same dominant mode
			{   // more than two channels
				currentTask->group_f = sort_amp[i];
				currentTask->group_ratio = currentTask->ratio[findnum(sort_amp[i],currentTask->freq,m)];
			    currentTask->outflag = true;
			    break;
			}
		}
	}
	currentTask->n = n;
	//p_task= currentTask;
    free(zi);free(out_amp);free(out_pha);free(out_damp);free(out_f);free(out_dpratio); 
	free(rel_energy);
	free(sort_amp);

	// Notify waiting thread that task is complete.
	manualEvent->Set();
}

int comp_nums(const void *param1, const void *param2)
{
	double *num1 = (double *)param1;
	double *num2 = (double *)param2;

    /* compare numbers for use in qsort */
	//char c='D';   //descending
	//if (c=='A') // ascending
		if (*num1 <  *num2)
			return -1;
	    else if (*num1 == *num2) 
			return  0;
	    else
			return  1;
	//else // descending
	//	if (*num1 >  *num2)
	//		return -1;
	//    else if (*num1 == *num2) 
	//		return  0;
	//    else
	//		return  1;
}

// Caller responsible for calling "free()" on returned buffer...
char* StringToCharBuffer(String^ gcStr)
{
	array<unsigned char, 1>^ gcBytes = System::Text::Encoding::Default->GetBytes(gcStr);
	char* str = (char*)malloc(gcStr->Length + 1);
	if (str == NULL) throw gcnew OutOfMemoryException();
	Marshal::Copy(gcBytes, 0, (IntPtr)str, gcStr->Length);
	str[gcStr->Length] = '\0';
	return str;
}