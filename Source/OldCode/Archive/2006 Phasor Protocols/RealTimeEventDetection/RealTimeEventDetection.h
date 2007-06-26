/*******************************************************************************************************\

	RealTimeEventDetection.h - Header file for Real-time Event Detection Module
	Copyright © 2007 - Washington State University, all rights reserved - Gbtc

	Build Environment: VB.NET, Visual Studio 2005

\*******************************************************************************************************/

#pragma once

using namespace System;
using namespace System::Collections::Generic;
using namespace TVA::Measurements;
using namespace InterfaceAdapters;

#define DefaultConfigSection "RealTimeEventDetection"
#define PI Math.PI

char* StringToCharBuffer(String^ gcStr);

namespace RealTimeEventDetection
{
	public ref class EventDetectionAlgorithm : public CalculatedMeasurementAdapterBase
	{
	private:

		// Algorithm parameters
		int m_maximumTasks;					// Maximum allowed number of tasks
		int m_maximumChannels;				// Maximum allowed data channels per PMU
		int m_maximumCrossChecks;			// Maximum allowed number of cross-checks
		int m_maximumMissingPoints;			// Maximum allowed number of missing data points per channel per second
		int m_maximumDisplayModes;			// Maximum allowed number of modes to display in each signal
		int m_estimateTriggerThreshold;		// Number of consistent estimates needed to trigger warning signal
		int m_analysisWindow;				// Size of data sample window, in seconds
		bool m_removeMeanValue;				// Remove mean value before analysis
		bool m_normalizeData;				// Normalize data before analysis
		bool m_displayDetail;				// Detail display of result from each analysis
		double m_repeatTime;				// Time window used repeat analysis, in seconds
		double m_consistentFrequencyRange;	// Frequency range for consistent estimate
		double m_consistentRatioRange;		// Ratio range for consistent estimate
		double m_voltageThreshold;			// Threshold of voltage for event detection
		double m_currentThreshold;			// Threshold of current for event detection
		double m_energyDisplayThreshold;	// Relative energy threshold used for display

		// Calculated parameters
		int m_channelCount;
		int m_minimumSamples;
		String^ m_systemPath;
		Measurement^ m_missingMeasurement;
		List<array<IMeasurement^, 1>^>^ m_measurementMatrix;

		enum class ChannelType
		{
			VM = 1,
			VA = 2,
			IM = 3,
			IA = 4
		};
		
		List<ChannelType>^ m_channelType;

		ref class AnalysisTask
		{
		public:
			AnalysisTask(List<int>^ channel, String^ method)
			{
				if (channel == nullptr)
					this->channel = gcnew List<int>;	// Create a new list if one wasn't provided
				else
					this->channel = channel;

				this->method = method;
			}

			List<int>^ channel;		// input - signal channels for the task
			String^ method;			// input  - analysis method for the task
			double *data;			// input  -  data
			int m;					// output - number of signals in the task
			int n;					// output/input - number of modes in the task
			double *freq;			// output - vector containing dominant frequency for each channel
			double *ratio;			// output - vector containing dominant damping ratio for each channel
			double group_f;			// output - dominant frequency for all channels in the task
			double group_ratio;		// output - dominant damping ratio for all channels in the task
			int outflag;			// output - flag for successful detection of dominant signal
		};

		List<AnalysisTask^>^ m_localTasks;
		List<AnalysisTask^>^ m_interAreaTasks;

		ref class CrossCheck
		{
		public:
			CrossCheck(List<int>^ task, double freq_range, double ratio_range)
			{
				if (task == nullptr)
					this->task = gcnew List<int>;	// Create a new list if one wasn't provided
				else
					this->task = task;

				this->freq_range = freq_range;
				this->ratio_range = ratio_range;
			}

			List<int>^ task;		// input - tasks for crosscheck
			double freq_range;		// input -  freqquency range for consistent estimate
			double ratio_range;		// input -  ratio range for consistent estimate
			double freq;			// output -  mean frequency for consistent estimate
			double ratio;			// output -  mean damping ratio for consistent estimate
			int flag;				// output - flag of consistent estimate
			int task_count;			// output - number of tasks in the check
		};

		List<CrossCheck^>^ m_localCrossChecks;

		// Define output file streams
		FILE *fout_message,*fout_local_details,*fout_inter_details,*fout_local_xcheck,*fout_inter_xcheck,*fout_mov_local_checks,*fout_mov_inter_checks;

	public:

		EventDetectionAlgorithm();

		// We override this function to customize our initilization process
		virtual void Initialize(String^ calculationName, String^ configurationSection, cli::array<IMeasurement^, 1>^ outputMeasurements, cli::array<MeasurementKey, 1>^ inputMeasurementKeys, int minimumMeasurementsToUse, int expectedMeasurementsPerSecond, double lagTime, double leadTime) override;

	protected:

		// We override this this function to process frames of measurement data
		virtual void PublishFrame(IFrame^ frame, int index) override;
	};
}
