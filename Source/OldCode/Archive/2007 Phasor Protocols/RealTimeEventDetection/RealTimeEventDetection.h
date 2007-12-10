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

int CompareNumbers(const void *num1, const void *num2);        // function used in qsort
char* StringToCharBuffer(String^ gcStr);

namespace RealTimeEventDetection
{
	public ref class EventDetectionAlgorithm : public CalculatedMeasurementAdapterBase
	{
	private:

		// Algorithm parameters
		int m_maximumCrossChecks;			// Maximum allowed number of cross-checks
		int m_maximumMissingPoints;			// Maximum allowed number of missing data points per channel per second
		int m_maximumDisplayModes;			// Maximum allowed number of modes to ResultDisplay in each signal
		int m_estimateTriggerThreshold;		// Number of consistent estimates needed to trigger warning signal
		int m_analysisWindow;				// Size of data sample window, in seconds
		bool m_removeMeanValue;				// Remove mean value before analysis
		bool m_normalizeData;				// m_normalizeData data before analysis
		bool m_displayDetail;				// Detail ResultDisplay of result from each analysis
		double m_repeatTime;				// Time window used repeat analysis, in seconds
		double m_consistentFrequencyRange;	// Frequency range for consistent estimate
		double m_consistentRatioRange;		// Ratio range for consistent estimate
		double m_voltageThreshold;			// Threshold of voltage for event detection
		double m_currentThreshold;			// Threshold of current for event detection
		double m_energyDisplayThreshold;	// Relative energy threshold used for ResultDisplay

		// Calculated parameters
		int m_channelCount;
		int m_minimumSamples;
		String^ m_systemPath;
		List<array<IMeasurement^, 1>^>^ m_measurementMatrix;
		List<array<IMeasurement^, 1>^>^ m_newMeasurementMatrix;
//		double *work;

		enum class ChannelType
		{
			VM = 1,
			VA = 2,
			IM = 3,
			IA = 4
		};
		
		List<ChannelType>^ m_channelType;

		ref class AmbientTask
		{
		public:
			AmbientTask(List<int>^ channel, String^ method)
			{
				if (channel == nullptr)
					this->channel = gcnew List<int>;	// Create a new list if one wasn't provided
				else
					this->channel = channel;

				this->method = method;
			}

			List<int>^ channel;						// input - signal channels for the task
			String^ method;							// input  - analysis method for the task
			String^ type;							// input - type of the analysis task: local or inter-area
			double *data;							// input  - data
			int m;									// output - number of channels in the task
			int N;									// output - number of data points in each channel
			List<double>^ ambientModeFrequency;
			List<double>^ ambientModeRatio;
			List<int>^ ambientModeFlag;
			List<bool>^ reverseArrTestFlag;
		};

		List<AmbientTask^>^ m_localTasks;
		int previousProcessedFrameIndex;  

		//ref class ThreadState
		//{
		//public:
		//	AnalysisTask^ p_task;
		//	int task_no;
		//	ManualResetEvent^ manualEvent;
		//	
		//	ThreadState(AnalysisTask^ p_task, int task_no, ManualResetEvent^ manualEvent) :
		//		p_task( p_task ), task_no( task_no ), manualEvent( manualEvent )
		//	{}
		//};

		// Define output file streams
		FILE *messageFile,*localDetailsFile,*interAreaDetailsFile,*localCrosschecksFile,*interAreaCrosschecksFile,*movingLocalCrosschecksFile,*movingInterAreaCrosschecksFile;

		void DataPreprocess(double *prony_data,int N,int m,int rm_mean,int normalize);
		double *PronyFunction(double *data,int N,int m,int n);   // function for Prony's method
		double *MatrixPencilFunction(double *data,int N,int m, int *n);  // function for matrix pencil
		double *HTLStackFunction(double *data,int N,int m, int *n);       // function for HTLStack
//		void ExecuteTask(System::Object^ state);								// execute analysis task - expected to executed on an independent thread
//		void ExecuteTask(AnalysisTask ^p_task, int task_no);

		double *MatrixTranspose(double *matrix, int m, int n);    // matrix transpose
		double *MatrixInverse(double *A, int N);        // matrix inverse
		double *PseudoInverse(double *A,int m, int n);           // calculate pseudo-inverse of a matrix
		void CalculateOutput(double *data, int N, int m, int n, double *zi, double dt, double *outputAmplitude, double *outputPhase,
						double *outputDamping, double *outputFrequency, double *outputDampRatio);   // calculate output modal paramters
//		void EventDetectionAlgorithm::ResultDisplay(double *outputAmplitude,double *outputPhase,double *outputDamping,double *outputFrequency,double *outputDampRatio,double *relativeEnergy,int m_displayDetail, AnalysisTask^ currentTask);
		void FindMaximum(double *v, int N, double *max, int *k);       // find maximum value
		double FindMaximum(List<double>^ v);                             // find maximum value
		void FindMinimum(double *v, int N, double *min, int *k);       // find minimum value
		double FindMinimum(List<double>^ v);                             // find minimum value
		double FindMeanValue(double *v, int N);                         // find mean value
		double FindMeanValue(List<double>^ v);                            // find mean value
		int FindNumber(double num, double *list, int n);               // find a number in a list
		void WriteMatrixToFile(double *A,int m,int n);   // write matrix into a file for debugging purpose
		void ExecuteAmbientTask(AmbientTask ^p_task, int task_no);
		void ReverseArrangementTest(AmbientTask ^p_task, int task_no);

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
extern "C" const double coefficientDPSS[];