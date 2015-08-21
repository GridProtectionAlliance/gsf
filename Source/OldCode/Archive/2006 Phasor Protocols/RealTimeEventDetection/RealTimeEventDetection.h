/*******************************************************************************************************\

	RealTimeEventDetection.h - Header file for Real-time Event Detection Module
	Copyright ?2007 - Washington State University, all rights reserved - Gbtc

	Build Environment: VB.NET, Visual Studio 2005

\*******************************************************************************************************/

#pragma once

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Threading;
using namespace TVA::Measurements;
using namespace InterfaceAdapters;

#define DefaultConfigSection "RealTimeEventDetection"
#define PI System::Math::PI
#define MaxLwork 100000
#define POW2_52 2.2204460492503130808472633361816e-16

int comp_nums(const void *num1, const void *num2);        // function used in qsort
char* StringToCharBuffer(String^ gcStr);

namespace RealTimeEventDetection
{
	public ref class EventDetectionAlgorithm : public CalculatedMeasurementAdapterBase
	{
	private:

		// Algorithm parameters
		int m_maximumChannels;				// Maximum allowed data channels per PMU
		int m_maximumCrossChecks;			// Maximum allowed number of cross-checks
		int m_maximumMissingPoints;			// Maximum allowed number of missing data points per channel per second
		int m_maximumDisplayModes;			// Maximum allowed number of modes to display in each signal
		int m_estimateTriggerThreshold;		// Number of consistent estimates needed to trigger warning signal
		int m_analysisWindow;				// Size of data sample window, in seconds
		bool m_removeMeanValue;				// Remove mean value before analysis
		bool m_normalizeData;				// m_normalizeData data before analysis
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
		List<array<IMeasurement^, 1>^>^ m_measurementMatrix;
		double *work;

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
			String^ type;           // input - type of the analysis task: local or inter-area
			double *data;			// input  - data
			int m;					// output - number of channels in the task
			int N;                  // output - number of data points in each channel
			int n;					// output/input - number of modes in the task
			double *freq;			// output - vector containing dominant frequency for each channel
			double *ratio;			// output - vector containing dominant damping ratio for each channel
			double group_f;			// output - dominant frequency for all channels in the task
			double group_ratio;		// output - dominant damping ratio for all channels in the task
			bool badEstimation;     // output - bad estimation
			bool outflag;			// output - flag for successful detection of dominant signal
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

		List<CrossCheck^>^ m_interAreaChecks;

		ref class ThreadState
		{
		public:
			AnalysisTask^ p_task;
			int task_no;
			ManualResetEvent^ manualEvent;
			
			ThreadState(AnalysisTask^ p_task, int task_no, ManualResetEvent^ manualEvent) :
				p_task( p_task ), task_no( task_no ), manualEvent( manualEvent )
			{}
		};

		// Define output file streams
		FILE *fout_message,*fout_local_details,*fout_inter_details,*fout_local_xcheck,*fout_inter_xcheck,*fout_mov_local_checks,*fout_mov_inter_checks;

		void data_preprocess(double *prony_data,int N,int m,int rm_mean,int normalize);
		double *prony_func(double *data,int N,int m,int n);   // function for Prony's method
		double *matrix_pencil_func(double *data,int N,int m, int *n);  // function for matrix pencil
		double *HTLStack_func(double *data,int N,int m, int *n);       // function for HTLStack
		void exe_task(System::Object^ state);								// execute analysis task - expected to executed on an independent thread
		double *matrix_transpose(double *matrix, int m, int n);    // matrix transpose
		double *matrix_inverse(double *A, int N);        // matrix inverse
		double *pinv(double *A,int m, int n);           // calculate pseudo-inverse of a matrix
		void cal_output(double *data, int N, int m, int n, double *zi, double dt, double *out_amp, double *out_pha,
						double *out_damp, double *out_f, double *out_dpratio);   // calculate output modal paramters
		void EventDetectionAlgorithm::display(double *out_amp,double *out_pha,double *out_damp,double *out_f,double *out_dpratio,double *rel_energy,int m_displayDetail, AnalysisTask^ currentTask);
		void findmax(double *v, int N, double *max, int *k);       // find maximum value
		double findmax(List<double>^ v);                             // find maximum value
		void findmin(double *v, int N, double *min, int *k);       // find minimum value
		double findmin(List<double>^ v);                             // find minimum value
		double findmean(double *v, int N);                         // find mean value
		double findmean(List<double>^ v);                            // find mean value
		int findnum(double num, double *list, int n);               // find a number in a list
		void matrix_write(double *A,int m,int n);   // write matrix into a file for debugging purpose

	public:

		EventDetectionAlgorithm();
		void TestAlgorithm(IFrame^ frame, int index)
		{
			PublishFrame(frame, index); 
		}

		// We override this function to customize our initilization process
		virtual void Initialize(String^ calculationName, String^ configurationSection, cli::array<IMeasurement^, 1>^ outputMeasurements, cli::array<MeasurementKey, 1>^ inputMeasurementKeys, int minimumMeasurementsToUse, int expectedMeasurementsPerSecond, double lagTime, double leadTime) override;

	protected:

		// We override this this function to process frames of measurement data
		virtual void PublishFrame(IFrame^ frame, int index) override;
	};
}
