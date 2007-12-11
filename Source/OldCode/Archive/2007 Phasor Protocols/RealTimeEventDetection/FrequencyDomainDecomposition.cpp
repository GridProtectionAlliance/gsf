/*******************************************************************************************************\

	RealTimeEventDetection.cpp - Real-time Event Detection Module
	Copyright ?2007 - Washington State University, all rights reserved - Gbtc

	Build Environment: VB.NET, Visual Studio 2005

\*******************************************************************************************************/

#include "stdafx.h"
#include "FrequencyDomainDecomposition.h"


using namespace System;
using namespace System::Text;
using namespace System::Threading;
using namespace System::Runtime::InteropServices;
using namespace TVA::Measurements;
using namespace TVA::Configuration;
using namespace OscillationMonitoringSystem;

// Constructor
FrequencyDomainDecomposition::FrequencyDomainDecomposition() {}

// Calculation initialization
void FrequencyDomainDecomposition::Initialize(String^ calculationName, String^ configurationSection, cli::array<TVA::Measurements::IMeasurement^, 1>^ outputMeasurements, cli::array<TVA::Measurements::MeasurementKey, 1>^ inputMeasurementKeys, int minimumMeasurementsToUse, int expectedMeasurementsPerSecond, double lagTime, double leadTime)
{
	// Call base class initialization function
	__super::Initialize(calculationName, configurationSection, outputMeasurements, inputMeasurementKeys, minimumMeasurementsToUse, expectedMeasurementsPerSecond, lagTime, leadTime);

	// Make sure configuration section parameter is defined - if not, use default
	if (String::IsNullOrEmpty(configurationSection)) ConfigurationSection = DefaultConfigSection;

	// Get categorized settings section from configuration file
	CategorizedSettingsElementCollection^ settings = TVA::Configuration::Common::CategorizedSettings[ConfigurationSection];

	// Make sure needed configuration variables exist - since configuration variables will
	// be added to config file of parent process we add them to a new configuration category
	settings->Add("MaximumMissingPoints", "4", "Maximum allowed missing data points per channel per second");
	settings->Add("MaximumCrossChecks", "40", "Maximum allowed number of cross-checks");
	settings->Add("MaximumDisplayModes", "5", "Maximum allowed number of modes to ResultDisplay in each signal");
	settings->Add("EstimateTriggerThreshold", "4", "Number of consistent estimates needed to trigger warning signal");
	settings->Add("AnalysisWindow", "180", "Size of data sample window, in seconds");
	settings->Add("RemoveMeanValue", "True", "Remove mean value before analysis");
	settings->Add("NormalizeData", "True", "NormalizeData data before analysis");
	settings->Add("DisplayDetail", "True", "Detail ResultDisplay of result from each analysis");
	settings->Add("RepeatTime", "10", "Time window used repeat analysis, in seconds");
	settings->Add("ConsistentFrequencyRange", "0.02", "Frequency range for consistent estimate");
	settings->Add("ConsistentRatioRange", "0.02", "Ratio range for consistent estimate");
	settings->Add("VoltageThreshold", "0.005", "Threshold of voltage for event detection");
	settings->Add("CurrentThreshold", "0.012", "Threshold of current for event detection");
	settings->Add("EnergyDisplayThreshold", "0.5", "Relative energy threshold used for ResultDisplay");

	// Save updates to config file, if any
	TVA::Configuration::Common::SaveSettings();

	// Load algorithm parameters from configuration file
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
	MeasurementKey^ measurementKey0 = gcnew MeasurementKey(1, "P1");      // 1608, "P2" TVA_CUMB-BUS1:ABBV  VM
    MeasurementKey^ measurementKey1 = gcnew MeasurementKey(3, "P1");      // 1610, "P2" TVA_CUMB-BUS2:ABBV  VM
    MeasurementKey^ measurementKey2 = gcnew MeasurementKey(5, "P1");      // 1612, "P2" TVA_CUMB-MARS:ABBI  IM
    MeasurementKey^ measurementKey3 = gcnew MeasurementKey(7, "P1");      // 1616, "P2" TVA_CUMB-JOHN:ABBI  IM
    MeasurementKey^ measurementKey4 = gcnew MeasurementKey(9, "P1");      // 1620, "P2" TVA_CUMB-DAVD:ABBI  IM
	
	//MeasurementKey^ measurementKey0 = gcnew MeasurementKey(1600, "P0");	
	//MeasurementKey^ measurementKey1 = gcnew MeasurementKey(1601, "P0");	
	//MeasurementKey^ measurementKey2 = gcnew MeasurementKey(1602, "P0");	
	//MeasurementKey^ measurementKey3 = gcnew MeasurementKey(1603, "P0");	

	m_channelType = gcnew List<ChannelType>;

	m_channelType->Add(ChannelType::VM);
	m_channelType->Add(ChannelType::VM);
	m_channelType->Add(ChannelType::IM);
	m_channelType->Add(ChannelType::IM);
	m_channelType->Add(ChannelType::IM);

	// In this calculation, we manually initialize the input measurements to use for the base class since they are
    // a hard-coded set of inputs that will not change (i.e., no need to specifiy input measurements from SQL)
	List<MeasurementKey>^ inputMeasurements = gcnew List<MeasurementKey>;

	// Add CUMB channels to input measurements
	inputMeasurements->Add(*measurementKey0);
	inputMeasurements->Add(*measurementKey1);	
	inputMeasurements->Add(*measurementKey2);	
	inputMeasurements->Add(*measurementKey3);	
	inputMeasurements->Add(*measurementKey4);	

	List<int>^ cumbMeasurementIndicies = gcnew List<int>;

	cumbMeasurementIndicies->Add(0);
	cumbMeasurementIndicies->Add(1);
	cumbMeasurementIndicies->Add(2);
	cumbMeasurementIndicies->Add(3);
	cumbMeasurementIndicies->Add(4);

	//List<int>^ tempMeasurementIndicies = gcnew List<int>;
	//tempMeasurementIndicies->Add(0);
	//tempMeasurementIndicies->Add(1);
	//tempMeasurementIndicies->Add(2);

    InputMeasurementKeys = inputMeasurements->ToArray();
	MinimumMeasurementsToUse = inputMeasurements->Count;

	m_localTasks = gcnew List<AmbientTask^>; 

 	m_localTasks->Add(gcnew AmbientTask(cumbMeasurementIndicies, "FDD"));			// Task 0
	//m_localTasks->Add(gcnew AmbientTask(tempMeasurementIndicies, "FDD"));

	List<int>^ cumbTaskIndicies = gcnew List<int>;

	cumbTaskIndicies->Add(0);

//	m_interAreaChecks->Add(gcnew CrossCheck(jdayTaskIndicies, 0.02, 0.02));

	// Initialize system path
	m_systemPath = TVA::IO::FilePath::GetApplicationPath();

//	messageFile = NULL;
	localDetailsFile = NULL;
	interAreaDetailsFile = NULL;
	localCrosschecksFile = NULL;
	interAreaCrosschecksFile = NULL;
	movingLocalCrosschecksFile = NULL;
	movingInterAreaCrosschecksFile = NULL;
	previousProcessedFrameIndex = 0;

	// Define global channel count
	m_channelCount = InputMeasurementKeys->Length;

	// Calculate minimum needed sample size
//	m_minimumSamples = (int) ((m_analysisWindow + m_repeatTime * (m_estimateTriggerThreshold - 1))* expectedMeasurementsPerSecond);
	m_minimumSamples = (int) (m_analysisWindow * expectedMeasurementsPerSecond);

	// Intitalize our rolling window data buffer
	m_measurementMatrix = gcnew List<array<IMeasurement^, 1>^>;
	m_newMeasurementMatrix = gcnew List<array<IMeasurement^, 1>^>;
    
	// Open file to write
	if (m_displayDetail)
	{
		char* pstrLocalTaskFile = NULL;
		char* pstrMessageFile = NULL;

		__try
		{
			pstrLocalTaskFile = StringToCharBuffer(String::Concat(m_systemPath, "LocalTaskDetailsFile.txt"));
			pstrMessageFile = StringToCharBuffer(String::Concat(m_systemPath, "Message.txt"));
			
			localDetailsFile = fopen(pstrLocalTaskFile,"w");
			if (localDetailsFile==NULL)
				throw gcnew ArgumentException("Error in opening output file");

			messageFile=fopen(pstrMessageFile, "w");
			if (messageFile==NULL) 
				throw gcnew ArgumentException("Error in opening output message file");
		}
		__finally
		{
			if (pstrLocalTaskFile != NULL) free(pstrLocalTaskFile);
			if (pstrMessageFile != NULL) free(pstrMessageFile);
		}
	}
}

void FrequencyDomainDecomposition::PublishFrame(TVA::Measurements::IFrame^ frame, int index)
{
	IMeasurement^ measurement;
	MeasurementKey measurementKey;
	array<MeasurementKey, 1>^ inputMeasurements = InputMeasurementKeys;
	array<IMeasurement^, 1>^ measurements = TVA::Common::CreateArray<IMeasurement^>(m_channelCount);
	double* data;

	List<bool>^ channelEvent = gcnew List<bool>;
	List<int>^ missPointCount = gcnew List<int>;
	List<double>^ relativeDeviation = gcnew List<double>;
	List<int>^ successLocalCheck = gcnew List<int>;
	List<int>^ channelsExceedingThreshold = gcnew List<int>;

	int m;                             // m is the number of channels in the task
	int N;
	int i,j,k,task_no;
	bool activateMonitorFlag = false;   // activate the monitoring process
	int iPrevious, iNext;

	AmbientTask^ currentTask;

	// Loop through all input measurements to see if they exist in this frame
    for(i = 0; i < m_channelCount; i++)
	{
		measurementKey = inputMeasurements[i];

		if (frame->Measurements->TryGetValue(measurementKey, measurement))
			measurements[i] = measurement;
		else
			measurements[i] = gcnew TVA::Measurements::Measurement(measurementKey.ID, measurementKey.Source, Double::NaN, frame->Ticks);
	}

	// Maintain constant row-matrix length
	m_newMeasurementMatrix->Add (measurements);
	if (m_newMeasurementMatrix->Count < m_repeatTime * this->FramesPerSecond)
		return;

	for (i=0;i<m_newMeasurementMatrix->Count ;i++)
		m_measurementMatrix->Add(m_newMeasurementMatrix[i]);

	m_newMeasurementMatrix->Clear ();

	while (m_measurementMatrix->Count > m_minimumSamples)
		m_measurementMatrix->RemoveAt(0);

	// We don't start calculations until the needed matrix size is available
	if (m_measurementMatrix->Count >= m_minimumSamples)
	{
		// Handle data intialization
		activateMonitorFlag = true;
		missPointCount->Clear ();
		for (j = 0; j < m_channelCount; j++)
			missPointCount->Add (0);
		data = (double *) malloc (sizeof(double) * m_minimumSamples * m_channelCount);
		for (i = 0; i < m_minimumSamples && activateMonitorFlag ; i++)
		{
			// Get data row
			measurements = m_measurementMatrix[i];

			for (j = 0; j < m_channelCount; j++)
			{
				// Get data column
				measurement = measurements[j];

				if (Double::IsNaN(measurement->Value))
				{
					// handle data interpolation
					if ( i == 0 || i == m_minimumSamples - 1 )  // no interpolation performed if the missing data is the 1st row or last row
					{
						activateMonitorFlag = false;
						free(data);
						break;
					}
					else
					{
						data[i + j * m_minimumSamples] = 9999;   // needs interpolation later
				        missPointCount[j] ++;
						if (missPointCount[j] >= m_maximumMissingPoints)
						{
							activateMonitorFlag = false;
							free(data);
							break;
						}
					}
				}
				else
					// Store all 2-dimension matrix in Fortran format,column-wise
				    data[i + j * m_minimumSamples] = measurement->AdjustedValue;
			}
		}

		if (!activateMonitorFlag)
		{
			fprintf(messageFile,"\n\n%s.",frame->Timestamp.ToString ());
			fprintf(messageFile,"%s\n",frame->Timestamp.Millisecond.ToString());
			fprintf(messageFile,"Too many missing points.\n");
			return;
		}

		/* data intepolation for each channel */
		for (j = 0; j < m_channelCount; j++)
			for (i = 0; i < m_minimumSamples; i++)
				if (data[i + j * m_minimumSamples] == 9999)
				{
					iPrevious = i - 1;
					for ( k = i; k < m_minimumSamples; k++)
						if (data[k + j * m_minimumSamples] != 9999)
							break;
					iNext = k;
					for ( i = iPrevious + 1; i <= iNext - 1 ; i++)
						data[i + j * m_minimumSamples] =  data[iPrevious + j * m_minimumSamples] + 
						   (data[iNext + j * m_minimumSamples] - data[iPrevious + j * m_minimumSamples])/(iNext - iPrevious) * ( i - iPrevious) ;
				}

		/* iterate for each local task */
		N = (int) (m_analysisWindow * FramesPerSecond);
		if (m_displayDetail)
		{
			fprintf(localDetailsFile,"\n\n%s.",frame->Timestamp.ToString ());
			fprintf(localDetailsFile,"%s\n",frame->Timestamp.Millisecond.ToString());
		}
		for (task_no = 0; task_no < m_localTasks->Count ; task_no++)
		{
			currentTask = m_localTasks[task_no];
			/* initialize currentTask */
			m = currentTask->channel->Count;
			currentTask->m = m;
			currentTask->N = N;
			currentTask->type = "Local";
            
			// Assigning data to current task
			currentTask->data = (double *) malloc(sizeof(double) * currentTask->N *currentTask->m);
			for (k = 0; k < currentTask->m; k++)
			{
				j = currentTask->channel[k];
				if (j > m_channelCount)    // Incorrect channel
				{
					throw gcnew ArgumentException("Invalid channel number");
				}
				for(i = 0; i<N ;i++)
					currentTask->data[i+k*N] = data[(i+ j * m_minimumSamples)];

			}
			
			/* perform analysis task */
			ExecuteAmbientTask(currentTask,task_no);

			/* write results to files */
			if (m_displayDetail)
			{
				fprintf(localDetailsFile,"\nTask No. %d:\n",task_no);
				for (i=0; i<currentTask->ambientModeFlag->Count; i++)
				{
					if (currentTask->ambientModeFlag[i] == 1)
						fprintf(localDetailsFile,"Mode %d pass reverse arrangement test:\n",i);
					else if (currentTask->ambientModeFlag[i] == 2)
						fprintf(localDetailsFile,"Mode %d:\n",i);
					else if (currentTask->ambientModeFlag[i] == 3)
						fprintf(localDetailsFile,"Mode %d may be underestimated:\n",i);
					else
						fprintf(localDetailsFile,"Mode %d is not reliable:\n",i);
					fprintf(localDetailsFile,"Frequency = %.4f Hz, Damping Ratio = %.4f\n",currentTask->ambientModeFrequency [i],currentTask->ambientModeRatio[i]);
				}

				fflush(localDetailsFile);
				fflush(messageFile);
			}
		}  // end of /* iterate for each local task */
	}  // end of if (m_measurementMatrix->Count >= m_minimumSamples)
	//if (index == 10200)
	//{
	//	fclose(localDetailsFile);
	//	fclose(messageFile);
	//}
}

void FrequencyDomainDecomposition::Stop()
{
	__super::Stop();
	fclose(localDetailsFile);
	fclose(messageFile);
}

void FrequencyDomainDecomposition::DataPreprocess(double *prony_data,int N,int m,int m_removeMeanValue,int m_normalizeData)
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


double *FrequencyDomainDecomposition::PronyFunction(double *data,int N,int m,int n)
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
	double *work = (double *) malloc(sizeof(double)*MaxLwork);

	if (work == NULL)
	{
		fprintf(messageFile,"Error in malloc in PronyFunction.\n");
		free(work);
		return NULL;
	}
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
		fprintf(messageFile,"dgelsd_ fails in PronyFunction.\n");
		free(work);
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
		fprintf(messageFile,"dgeev_ fails in PronyFunction.\n");
		free(work);
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
	free(work);
	return zi;
}


double *FrequencyDomainDecomposition::MatrixPencilFunction(double *data,int N,int m, int *n)
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
	double *work = (double *) malloc(sizeof(double)*MaxLwork);
	if (work == NULL)
	{
		fprintf(messageFile,"Error in malloc in MatrixPencilFunction.\n");
		free(work);
		return NULL;
	}

	/* Form matrix Y*/
	L = (int)N/2;
	Y_row = N - L;
	Y_col = (L+1)*m;
	Y = (double *) malloc(sizeof(double)*Y_row*Y_col);
    if (Y==NULL)
	{
		fprintf(messageFile,"Error in malloc in MatrixPencilFunction.\n");
		free(work);free(Y);
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
		fprintf(messageFile,"Error in malloc in MatrixPencilFunction.\n");
		free(work);free(Y);free(S);free(U);free(VT);
		return NULL;
	}
	iwork = (int*)malloc(sizeof(int)*8*minMN);
	lwork = MaxLwork;
    dgesdd_( &jobz, &Msvd, &Nsvd, Y, &lda, S, U, &ldu, VT, &ldvt, work, &lwork, iwork, &info);
	if (info!=0)
	{
		fprintf(messageFile,"dgesdd_ fails in MatrixPencilFunction\n");
		free(work);free(Y);free(S);free(U);free(VT);free(iwork);
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
		fprintf(messageFile,"Error in malloc in MatrixPencilFunction.\n");
		free(work);free(Y);free(S);free(U);free(VT);free(iwork);free(V1_prime);free(V2_prime);
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

	/* Matrix multiplication. A1=V2_prime'*PseudoInverse(V1_prime'); */
//  SUBROUTINE DGEMM(TRANSA,TRANSB,M,N,K,ALPHA,A,LDA,B,LDB,BETA,C,LDC)
//  Purpose
//  DGEMM  performs one of the matrix-matrix operations
//  C := alpha*op( A )*op( B ) + beta*C,
	A0 = PseudoInverse(MatrixTranspose(V1_prime,V1prime_row,V1prime_col),V1prime_col, V1prime_row);
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
		fprintf(messageFile,"Error in malloc in MatrixPencilFunction.\n");
		free(work);free(Y);free(S);free(U);free(VT);free(iwork);free(V1_prime);free(V2_prime);free(A1);
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
		fprintf(messageFile,"dgeev_ fails in MatrixPencilFunction.\n");
		free(work);free(Y);free(S);free(U);free(VT);free(iwork);free(V1_prime);free(V2_prime);free(A1);free(wr);free(wi);
		free(vr);free(vl);
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
	free(work);
	return zi;
}


double *FrequencyDomainDecomposition::HTLStackFunction(double *data,int N,int m, int *n)
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
	double *work = (double *) malloc(sizeof(double)*MaxLwork);
	if (work == NULL)
	{
		fprintf(messageFile,"Error in malloc in HTLStackFunction.\n");
		free(work);
		return NULL;
	}

	L = (int)N/2;
	M = N-L+1;
	Hs_row = L;
	Hs_col = M*m;
	Hs = (double *) malloc(sizeof(double)*Hs_row*Hs_col);
    if (Hs==NULL)
	{
		fprintf(messageFile,"Error in malloc in HTLStackFunction.\n");
		free(work);free(Hs);
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
		fprintf(messageFile,"Error in malloc HTLStackFunction.\n");
		free(work);free(Hs);free(S);free(U);free(VT);
		return NULL;
	}
	iwork = (int*)malloc(sizeof(int)*8*minMN);
	lwork = MaxLwork;
    dgesdd_( &jobz, &Msvd, &Nsvd, Hs, &lda, S, U, &ldu, VT, &ldvt, work, &lwork, iwork, &info);
	if (info!=0)
	{
		fprintf(messageFile,"dgesdd_ fails HTLStackFunction. \n");
		free(work);free(Hs);free(S);free(U);free(VT);free(iwork);
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
	free(S);free(U);free(VT);
	S = (double *) malloc(sizeof(double)*minMN);
	ldu = Msvd;
	U = (double *) malloc(sizeof(double)*ldu*minMN);
	ldvt = Nsvd;
	VT = (double *) malloc(sizeof(double)*ldvt*Nsvd);
	if (VT==NULL)
	{
		fprintf(messageFile,"Error in malloc HTLStackFunction.\n");
		free(work);free(Hs);free(S);free(U);free(VT);free(iwork);free(A);free(U_hat_up);free(U_hat_down);
		return NULL;
	}
	iwork = (int*)malloc(sizeof(int)*8*minMN);
	lwork = MaxLwork;
    dgesdd_( &jobz, &Msvd, &Nsvd, A, &lda, S, U, &ldu, VT, &ldvt, work, &lwork, iwork, &info);
	if (info!=0)
	{
		fprintf(messageFile,"dgesdd_ fails HTLStackFunction.\n");
		free(work);free(Hs);free(S);free(U);free(VT);free(iwork);free(A);free(U_hat_up);free(U_hat_down);free(iwork);
		return NULL;
	}

	/* form W12 and W22*/
	W = MatrixTranspose(VT,ldvt,ldvt);
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

//  WriteMatrixToFile(W,2*K,2*K);
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
	invW22 = MatrixInverse(W22,K);
	if (invW22 == NULL)
	{
		free(work);free(Hs);free(iwork);free(S);free(U);free(VT);free(U_hat_up);free(U_hat_down);
		free(A);free(W);free(W12);free(W22);free(C);free(invW22);
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
		fprintf(messageFile,"dgeev_ fails HTLStackFunction.\n");
		free(work);free(Hs);free(iwork);free(S);free(U);free(VT);free(U_hat_up);free(U_hat_down);
		free(A);free(W);free(W12);free(W22);free(C);free(invW22);free(wr);free(wi);free(vr);free(vl);
		return NULL;
	}

	zi = (double *) malloc(sizeof(double)*2*K);
	for (i=0;i<K;i++)
	{
		zi[2*i] = wr[i];
		zi[2*i+1] = wi[i];
	}
    free(Hs);free(iwork);free(S);
	free(U);free(VT);
    free(U_hat_up);free(U_hat_down);
    free(A);free(W);free(W12);free(W22);free(C);
	free(wr);free(wi);free(vr);free(vl);
	free(invW22);
	free(work);
	return zi;
}

double *FrequencyDomainDecomposition::MatrixTranspose(double *matrix, int m, int n)
{
	int i,j;
	double *result;
	// matrix is m*n, result is n*m
	result = (double *) malloc(sizeof(double)*m*n);
	if (result==NULL)
	{
		fprintf(messageFile,"Error in malloc in MatrixTranspose.\n");
		free(result);
		return NULL;
	}
	for (i=0;i<n;i++)
		for (j=0;j<m;j++)
			//result[i][j] = matrix[j][i];
            result[i+j*n] = matrix[j+i*m];
	return result;
}

double *FrequencyDomainDecomposition::MatrixInverse(double *A, int N)
{
//	  SUBROUTINE DGESV( N, NRHS, A, LDA, IPIV, B, LDB, INFO )
//    DGESV computes the solution to a real system of linear equations A * X = B,
    int info, lda, ldb, nrhs;
    int *ipiv;
	double *B = (double *) malloc(sizeof(double)*N*N);
	int i,j;

	if (B==NULL)
	{
		fprintf(messageFile,"Error in malloc in MatrixInverse.\n");
		free(B);
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
		fprintf(messageFile,"dgesv_ fails when finding matrix inverse\n");
		free(B);
		return NULL;
	}
	else
		return B;
}

double *FrequencyDomainDecomposition::PseudoInverse(double *A,int m, int n)
{
    /* find pseudo-inverse of matrix A */
//  matlab code for PseudoInverse
//	function B = PseudoInverse(A)
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
	double *work = (double *) malloc(sizeof(double)*MaxLwork);
	if (work == NULL)
	{
		fprintf(messageFile,"Error in malloc in PseudoInverse.\n");
		free(work);
		return NULL;
	}

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
		fprintf(messageFile,"Error in malloc in PseudoInverse.\n");
		free(work);free(S);free(U);free(VT);
		return NULL;
	}
	iwork = (int*)malloc(sizeof(int)*8*minMN);
	lwork = MaxLwork;
    dgesdd_( &jobz, &Msvd, &Nsvd, A, &lda, S, U, &ldu, VT, &ldvt, work, &lwork, iwork, &info);
	if (info!=0)
	{
		fprintf(messageFile,"dgesdd_ fails in PseudoInverse.\n");
		free(work);free(S);free(U);free(VT);free(iwork);
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
	// Ur = MatrixTranspose(U,m,r), i.e, transpose of first r columns of U; U is an m*n matrix.
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
	free(work);
	return result;
}


void FrequencyDomainDecomposition::CalculateOutput(double *data, int N, int m, int n, double *zi, double dt, double *outputAmplitude, double *outputPhase,
				double *outputDamping, double *outputFrequency, double *outputDampRatio)
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
	double *work = (double *) malloc(sizeof(double)*MaxLwork);
	if (work == NULL)
	{
		fprintf(messageFile,"Error in malloc in CalculateOutput.\n");
		free(work);
		return;
	}

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
		fprintf(messageFile,"Error in malloc in CalculateOutput.\n");
		free(work);free(iwork);free(rwork);free(fai);free(rhs);
		throw gcnew ArgumentException("Error in malloc in CalculateOutput");
	}
	S = (double *) malloc(sizeof(double)*minMN);
	lwork = MaxLwork;
    zgelsd_(&Mls, &Nls, &nrhs, fai, &lda, rhs, &ldb, S, &rcond, &rank, work, &lwork, rwork, iwork, &info);
	if (info!=0)
	{
		fprintf(messageFile,"zgelsd_ fails in CalculateOutput\n");
		free(work);free(iwork);free(rwork);free(fai);free(rhs);free(S);
		throw gcnew ArgumentException("zgelsd_ fails in CalculateOutput");
	}
    
	/* Modal outputs calculation */
	for (i=0;i<n;i++)
	{
		for (j=0;j<m;j++)
		{
			//outputAmplitude[i][j] = sqrt(rhs[2*i][j]*rhs[2*i][j]+rhs[2*i+1][j]*rhs[2*i+1][j]);
			outputAmplitude[i+j*n] = sqrt(rhs[2*i+j*2*N]*rhs[2*i+j*2*N]+rhs[2*i+1+j*2*N]*rhs[2*i+1+j*2*N]);
			//outputPhase[i][j] = atan2(rhs[2*i+1][j],rhs[2*i][j];
			outputPhase[i+j*n] = atan2(rhs[2*i+1+j*2*N],rhs[2*i+j*2*N])/PI*180;
		}
		re_lambda = log(zi[2*i]*zi[2*i]+zi[2*i+1]*zi[2*i+1])/2; // lambda = log(z) = log(abs(z)) + i*atan2(im(z),re(z))
		im_lambda = atan2(zi[2*i+1],zi[2*i]);
		outputDamping[i] = re_lambda/dt;
		outputFrequency[i] = im_lambda/2/PI/dt;
		outputDampRatio[i] = -re_lambda/sqrt(re_lambda*re_lambda+im_lambda*im_lambda);
	}
    free(fai);free(rhs);
    free(iwork);free(rwork);free(S);
	free(work);
}


void FrequencyDomainDecomposition::FindMaximum(double *v, int N, double *max, int *k)
{
	/* find the maximum from vectro v, return the maximum value max and location k */
	int i;
	if (N<1) 
	{
		fprintf(messageFile,"N must be larger than 1 in function 'FindMaximum'.\n");
		throw gcnew ArgumentException("Error in FindMaximum");
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

double FrequencyDomainDecomposition::FindMaximum(List<double>^ v)
{
	return TVA::Collections::Common::Maximum<double>(v);
}

void FrequencyDomainDecomposition::FindMinimum(double *v, int N, double *min, int *k)
{
	/* find the minimum from vectro v, return the minmum value max and location k */
	int i;
	if (N<1) 
	{
		fprintf(messageFile,"N must be larger than 1 in function 'FindMinimum'.\n");
		throw gcnew ArgumentException("Error in FindMinimum");
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

double FrequencyDomainDecomposition::FindMinimum(List<double>^ v)
{
	return TVA::Collections::Common::Minimum<double>(v);
}

double FrequencyDomainDecomposition::FindMeanValue(double *v, int N)
{
	/* find the mean value from vectro v */
	int i;
	double mean;
	if (N<1) 
	{
		fprintf(messageFile,"N must be larger than 1 in function 'FindMinimum'.\n");
		throw gcnew ArgumentException("Error in FindMeanValue");
	}
    mean =0;
	for (i=0;i<N;i++)
		mean += v[i];
	return mean/N;
}

double FrequencyDomainDecomposition::FindMeanValue(List<double>^ v)
{
	return TVA::Math::Common::Average(v);
}

int FrequencyDomainDecomposition::FindNumber(double num, double *list, int n)
{
	/* look for num in list, return the index. */
	int i;
	for (i=0;i<n;i++)
		if (num==list[i])
			return i;
	return -1;
}

void FrequencyDomainDecomposition::WriteMatrixToFile(double *A,int m,int n)
{
	/* write matrix A into message.txt for debugging purpose */
	int i,j;
	for (i=0;i<m;i++)
	{
		for (j=0;j<n;j++)
			fprintf(messageFile,"\t%.16f",A[i+j*m]);
		fprintf(messageFile,"\n");
	}
	fclose(messageFile);
}

//void FrequencyDomainDecomposition::ExecuteTask(System::Object^ state)
//{
//	/* This fuction performs one single analysis task. The results are stored in currentTask */ 
//	ThreadState^ taskState = safe_cast<ThreadState^>(state);
//	AnalysisTask^ p_task = taskState->p_task;
//	int task_no = taskState->task_no;
//	ManualResetEvent^ manualEvent = taskState->manualEvent;


int CompareNumbers(const void *param1, const void *param2)
{
	double *num1 = (double *)param1;
	double *num2 = (double *)param2;

    /* compare numbers for use in qsort */
    // descending
	if (*num1 >  *num2)
		return -1;
    else if (*num1 == *num2) 
		return  0;
    else
		return  1;
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

void FrequencyDomainDecomposition::ExecuteAmbientTask(AmbientTask ^p_task, int task_no)
{
	// some parameter setting for FDD
	int nfft = 8192;   // n for FFT
	double freqLowerBound = 0.2;
	double freqUpperBound = 1.5;
	double MACthreshold = 0.9;
	double truncateLevel1 = 0.2;
	double truncateLevel2 = 0.5;   // truncation ratio higher than this level is classified as bad estimate
	// end of parameter setting

	int i,j,k,ii;                                                          
	AmbientTask^ currentTask;
	fftw_complex *in,*out;
    fftw_plan p;
	double *y;
	double *Y,*P;                                          
	int N;                                                              
	int m;                                                              
	String^ type;  
	double dt;
	double *frequency;   //frequency
	double Fs = 30; //sampling frequency
	int iLower = 0;
	int iUpper = 0;
	double *Pxy;
	char jobz;
	int Msvd,Nsvd,lda,minMN,ldu,ldvt,lwork,info;
	double *S,*U,*VT,*rwork;
	int *iwork;
	double *work;
	double *singularValue,*singularVector;
	double maximumSingularValue;                                                
    int maximumIndex; 
	int maxModeNumber = 4;
	List<double>^ modeLargestSingularValue = gcnew List<double>;
    List<int>^ modeLargestSingularValueIndex = gcnew List<int>;
	List<double>^ modeShape = gcnew List<double>;
	List<double>^ fai1 = gcnew List<double>;
	List<double>^ fai2 = gcnew List<double>;
	double MAC;
	int index1,index2;
	double realPart,imagPart;
	double *psdSDOF;
	double minSingularValue,truncateRatio;
	int minIndex;
	double *autoCorrelation,*zi;
	int n;                             // model order  
	double outputFrequency,outputRatio;
	double realLambda,imagLambda;
	double localMaximumSingularValue;
	int localMaximumIndex;
	bool nearbyModeFlag;
	double *data;

	currentTask = p_task;                                               
	N = currentTask->N;                                                 
	m = currentTask->m;                                                 
	type = currentTask->type;
	currentTask->ambientModeFlag = gcnew List<int>;
	currentTask->ambientModeFrequency = gcnew List<double>;
	currentTask->ambientModeRatio = gcnew List<double>;
//    dt = 1.0/expectedMeasurementsPerSecond;         // delta t, which is 1/Fs
    dt = 1.0/Fs;


	/* reverse arrangement test */
	ReverseArrangementTest(currentTask,task_no);

	/* data preprocessing */                                            
    DataPreprocess(currentTask->data,N,m,m_removeMeanValue,m_normalizeData);
	if (String::Compare(currentTask->method,"FDD")==0)     //Frequency Domain Decomposition (FDD)
	{
		y = (double *) malloc(sizeof(double) * nfft);          
		Y = (double *) malloc(sizeof(double) * (nfft * 2 * 6) * m);
		out = (fftw_complex*) fftw_malloc(sizeof(fftw_complex) * nfft);
		for (j=0;j<m;j++)      // for each signal
		{
			for (i=0;i<nfft*6;i++)
			{
				Y[2*i + j*nfft*2*6] = 0;      //real part
				Y[2*i+1 + j*nfft*2*6] = 0;    //imag part
			}
			for (k=0;k<6;k++)   // for each taper
			{
				/* multiplied by slepian tapers */
				for (i=0;i<nfft;i++)
				{
					if (i < N)
						y[i] = currentTask->data[i+j*N] * coefficientDPSS[i+k*N];//data[i][j]* coef[i][k], jth signal,kth taper
					else
						y[i] = 0;
				}
				/* FFT */
		        p = fftw_plan_dft_r2c_1d(nfft, y, out, FFTW_ESTIMATE);
		        fftw_execute(p);
				for (i=0;i<nfft;i++)
				{
					Y[2*i + k*nfft*2 + j*nfft*2*6] = out[i][0];
					Y[2*i+1 + k*nfft*2 + j*nfft*2*6] = out[i][1];
				}
			}  // end of each taper
		}    // end of each signal
		fftw_destroy_plan(p);
        fftw_free(out);
        
		/* calculate auto- and cross-spectrum */
		P = (double *) malloc(sizeof(double) * (nfft * 2) * (m * m));
		for (i=0;i<m;i++)
			for (j=0;j<m;j++)
			{
				for (k=0;k<nfft;k++)
				{
					P[2*k + (i+j*m)*2*nfft] = 0;
					P[2*k+1 + (i+j*m)*2*nfft] = 0;
				}
				for (ii=0;ii<6;ii++)
					for (k=0;k<nfft;k++)
					{   // store P as [ P11 P21 ...Pm1   P12 P22... Pm2  P1m... Pmm]
						P[2*k + (i+j*m)*2*nfft] += 1.0/6 * (Y[2*k + ii*2*nfft + i*2*nfft*6] * Y[2*k + ii*2*nfft + j*2*nfft*6] + Y[2*k+1 + ii*2*nfft + i*2*nfft*6] * Y[2*k+1 +ii*2*nfft + j*2*nfft*6]); 
						P[2*k+1 + (i+j*m)*2*nfft] += 1.0/6 * (Y[2*k+1 + ii*2*nfft + i*2*nfft*6] * Y[2*k + ii*2*nfft + j*2*nfft*6] - Y[2*k + ii*2*nfft + i*2*nfft*6] * Y[2*k+1 +ii*2*nfft + j*2*nfft*6]); 
					}
			}
		free(Y);
		free(y);
        
		/* svd on spetral matrix */
		frequency = (double *) malloc (sizeof(double) * nfft);
		for (i=0;i<nfft;i++)
		{
			frequency[i] = (double)i/nfft *Fs;
			if (iLower == 0 && frequency[i]>freqLowerBound)
				iLower = i;
			else if (iUpper ==0 && frequency[i]>freqUpperBound)
				iUpper = i -1;
		}
		work = (double *) malloc(sizeof(double)*MaxLwork);
		jobz = 'S';
		Msvd = m;
		Nsvd = m;
		lda = m;
		minMN = min(Msvd,Nsvd);
		S = (double *) malloc(sizeof(double)*minMN);
		ldu = m;
		U = (double *) malloc(sizeof(double)*ldu*minMN*2);
		ldvt = Nsvd;
		VT = (double *) malloc(sizeof(double)*ldvt*Nsvd*2);
		iwork = (int*)malloc(sizeof(int)*8*minMN);
		rwork = (double *) malloc(sizeof(double)*(5*minMN*minMN + 7*minMN));
		lwork = MaxLwork;
		Pxy = (double *) malloc (sizeof(double) * m * m * 2);
		singularValue = (double *) malloc(sizeof(double)*(iUpper-iLower+1));
		singularVector = (double *) malloc(sizeof(double)*(iUpper-iLower+1)*m*2);
		for (ii=iLower;ii<=iUpper;ii++)
		{
			for (i=0;i<m;i++)          // row index
				for (j=0;j<m;j++)      // column index
				{
					Pxy[2*i + j*2*m] = P[2*ii + (i+j*m)*nfft*2];
					Pxy[2*i+1 + j*2*m] = P[2*ii+1 + (i+j*m)*nfft*2];
				}
			/* svd for each specutrm line*/
			zgesdd_( &jobz, &Msvd, &Nsvd, Pxy, &lda, S, U, &ldu, VT, &ldvt, work, &lwork, rwork, iwork, &info);
			if (info!=0)
			{
				fprintf(messageFile,"zgesdd_ fails FDD Function. \n");
				free(work);free(S);free(U);free(VT);free(iwork);free(rwork);
				return;
			}
			singularValue[ii-iLower] = S[0];
			for (i=0;i<m;i++)
			{
				singularVector[2*i + (ii-iLower)*2*m] = U[2*i];
				singularVector[2*i+1 + (ii-iLower)*2*m] = U[2*i+1];
			}
		}
	    FindMaximum(singularValue,iUpper-iLower+1,&maximumSingularValue,&maximumIndex); 
		free(P);free(Pxy);free(work);free(S);
		free(U);
		free(VT);
		free(iwork);
		free(rwork);

		/* mode identification */
		in = (fftw_complex*) fftw_malloc(sizeof(fftw_complex) * nfft);
		autoCorrelation = (double *) malloc(sizeof(double) * nfft);
		psdSDOF = (double *) malloc(sizeof(double) * nfft);
		data = (double *) malloc(sizeof(double) * (int) (10*Fs +1));
		for (i=0;i<maxModeNumber;i++)
		{
			if (i==0)
			{
				modeLargestSingularValue->Add (maximumSingularValue);
				modeLargestSingularValueIndex->Add (maximumIndex);
				for (j=0;j<2*m;j++)
					modeShape->Add (singularVector[j + maximumIndex*2*m]);
    		}
			else
			{
				FindMaximum(singularValue,iUpper-iLower+1,&localMaximumSingularValue,&localMaximumIndex);
				if (localMaximumSingularValue < maximumSingularValue/3 )
					break;
				nearbyModeFlag = false;
				for (j=0;j<i;j++)
					if (abs(frequency[localMaximumIndex+iLower]-currentTask->ambientModeFrequency[j]) < 0.075)
					{
						nearbyModeFlag = true;
						break;               // if the next peak is too close to the previous peak, break
					}
				if (nearbyModeFlag)
					break;
				else
				{
					modeLargestSingularValue->Add (localMaximumSingularValue);
					modeLargestSingularValueIndex->Add (localMaximumIndex);
					for (j=0;j<2*m;j++)
						modeShape->Add (singularVector[j + localMaximumIndex*2*m]);
				}
			}
			fai1->Clear ();
			for (j=0;j<2*m;j++)
				fai1->Add(modeShape[j+i*2*m]);

			// look for lower bound index for a mode
			index1 = modeLargestSingularValueIndex[i];
			while (1)
			{
				index1--;
				if (index1<0)
				{
					index1++;
					break;
				}
				fai2->Clear ();
				for (j=0;j<2*m;j++)
					fai2->Add(singularVector[j+index1*2*m]);
				realPart = 0;
				imagPart = 0;
				for (j=0;j<m;j++)
				{
					realPart += fai1[2*j] * fai2[2*j] + fai1[2*j+1] * fai2[2*j+1];
					imagPart += fai1[2*j] * fai2[2*j+1] - fai1[2*j+1] * fai2[2*j];
				}
				MAC = realPart * realPart + imagPart * imagPart;
				//vectorNorm = 0;
				//for (j=0;j<m;j++)
				//	vectorNorm += fai1[2*j] * fai1[2*j] + fai1[2*j+1] *fai1[2*j+1];
				//MAC /= vectorNorm;
				//vectorNorm = 0;
				//for (j=0;j<m;j++)
				//	vectorNorm += fai2[2*j] * fai2[2*j] + fai2[2*j+1] *fai2[2*j+1];
				//MAC /= vectorNorm;
				if (MAC < MACthreshold)
				{
					index1++;
					break;
				}
			}

			// look for upper bound index for a mode
			index2 = modeLargestSingularValueIndex[i];
			while (1)
			{
				index2++;
				if (index2>iUpper)
				{
					index2--;
					break;
				}
				fai2->Clear ();
				for (j=0;j<2*m;j++)
					fai2->Add(singularVector[j+index2*2*m]);
				realPart = 0;
				imagPart = 0;
				for (j=0;j<m;j++)
				{
					realPart += fai1[2*j] * fai2[2*j] + fai1[2*j+1] * fai2[2*j+1];
					imagPart += fai1[2*j] * fai2[2*j+1] - fai1[2*j+1] * fai2[2*j];
				}
				MAC = realPart * realPart + imagPart * imagPart;
				if (MAC < MACthreshold)
				{
					index2--;
					break;
				}
			}

			/* inverse FFT */
			for (j=0;j<nfft;j++)
			{
				if (j>=index1+iLower && j<=index2+iLower)
					psdSDOF[j] = singularValue[j-iLower];
				else if (j>= nfft-(index2+iLower) && j<=nfft-(index1+iLower))
					psdSDOF[j] = singularValue[nfft-j-iLower];
				else
					psdSDOF[j] = 0;
				in[j][0] = psdSDOF[j];
				in[j][1]  = 0;
			}

			FindMinimum(&psdSDOF[index1+iLower],index2-index1+1,&minSingularValue,&minIndex);
			truncateRatio = minSingularValue / modeLargestSingularValue[i];
			if (truncateRatio > truncateLevel2)
				currentTask->ambientModeFlag ->Add (4);
			else if (truncateRatio > truncateLevel1)
			    currentTask->ambientModeFlag ->Add (3);
			else
			{
				// check the reverse arrangement test
				for (j=0;j<currentTask->reverseArrTestFlag ->Count ;j++)
					if (!currentTask->reverseArrTestFlag[j])
					{
						currentTask->ambientModeFlag ->Add (2);
						break;
					}
				if (j==currentTask->reverseArrTestFlag ->Count)
					currentTask->ambientModeFlag->Add (1);

			}

			p = fftw_plan_dft_c2r_1d(nfft, in, autoCorrelation, FFTW_ESTIMATE);   // autoCorrelation * nfft is the actual inverse
		    fftw_execute(p);
			
			/* prony analysis and calculate output*/
			for (j=49;j<=49+Fs*10;j++)
				data[j-49] = autoCorrelation[j];
			zi = MatrixPencilFunction(data,(int) (10*Fs + 1),1,&n);
			if (n!=2)
				zi = PronyFunction(data,(int) (10*Fs + 1),1,2);
			realLambda = log(zi[0]*zi[0]+zi[1]*zi[1])/2; // lambda = log(z) = log(abs(z)) + i*atan2(im(z),re(z))
			imagLambda = atan2(zi[1],zi[0]);
			outputFrequency = abs(imagLambda) / 2 / PI /dt;
			outputRatio = -realLambda / sqrt(realLambda * realLambda + imagLambda * imagLambda);
			currentTask->ambientModeFrequency ->Add (outputFrequency);
			currentTask->ambientModeRatio ->Add (outputRatio);
            
			/* prepare for the next mode */
			for (j=index1;j<=index2;j++)
				singularValue[j] = 0;
		}
		fftw_destroy_plan(p);
        fftw_free(in);
		free(psdSDOF);
		free(autoCorrelation);
		free(data);
		// end of mode identification

		free(frequency);
		free(singularValue);
		free(singularVector);
	} // end of FDD
}

void FrequencyDomainDecomposition::ReverseArrangementTest(AmbientTask ^p_task, int task_no)
{
	int N = 100;
	int A[100];
	double x[100];
	int i,j,k;
	int n;
	int sumA;
	double *y;
	int L;

	p_task->reverseArrTestFlag = gcnew List<bool>;

	y = p_task->data;
    L = p_task->N;
	n = L/N;
	for (i=0;i<p_task->m ;i++)
	{
		for (j=0;j<N;j++)
			x[j] = y[j*n + i * p_task->N];
		for (j=0;j<N;j++)
		{
			A[j] = 0;
			for(k=j+1;k<N;k++)
			{
				if (x[k]<x[j])
					A[j]++;
			}
		}
		sumA = 0;
		for (j=0;j<N;j++)
			sumA += A[j];
	
		if (N==20)
		{
			if (sumA > 125 || sumA < 64)
			{
				p_task->reverseArrTestFlag->Add (false);   // do not pass test
			}
			else
			{
				p_task->reverseArrTestFlag->Add (true);    // pass the test
			}
		}
		else if (N==30)
		{
			if (sumA > 272 || sumA < 162)
			{
				p_task->reverseArrTestFlag->Add (false);   // do not pass test
			}
			else
			{
				p_task->reverseArrTestFlag->Add (true);    // pass the test
			}
		}
		else if (N==100)
		{
			if (sumA > 2804 || sumA < 2145)
			{
				p_task->reverseArrTestFlag->Add (false);   // do not pass test
			}
			else
			{
				p_task->reverseArrTestFlag->Add (true);    // pass the test
			}
		}
	}
}
