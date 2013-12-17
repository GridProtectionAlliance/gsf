#region [ Modification History ]
/*
 * 07/22/2012 Denis Kholine
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
using System.Collections.ObjectModel;
using System.Data;
using GSF.TimeSeries.Adapters;
using GSF.TestsSuite.TimeSeries.Wrappers; 
#endregion

namespace GSF.TestsSuite.TimeSeries.Cases
{
    /// <summary>
    /// Using CSV Adapter as default case
    /// </summary>
    public class IAllAdaptersCase
    {
        #region [ Members ]
        /// <summary>
        /// Output Stream Measurements test table
        /// </summary>
        private DataTable dtOutputStreamMeasurements;
        /// <summary>
        /// Action Adapter output data table
        /// </summary>
        private DataTable dtOutputStreamDevices;
        /// <summary>
        /// Action Adapter data table
        /// </summary>
        private DataTable dtActionAdapter;
        /// <summary>
        /// Action Adapter Active Measurments Active Measurements
        /// </summary>
        private DataTable dtActionAdapterActiveMeasurements;
        /// <summary>
        /// Action Adapter Statistic
        /// </summary>
        private DataTable dtActionAdapterStatistics;
        /// <summary>
        /// Input Adapter data table
        /// </summary>
        private DataTable dtInputAdapter;
        /// <summary>
        /// Active Measurements data table
        /// </summary>
        private DataTable dtInputAdapterActiveMeasurements;
        /// <summary>
        /// Input Adapter Statistics
        /// </summary>
        private DataTable dtInputAdapterStatistics;
        /// <summary>
        /// Container for test Measurements
        /// </summary>
        private DataTable dtMeasurement;
        /// <summary>
        /// Output Adapter data table
        /// </summary>
        private DataTable dtOutputAdapter;
        /// <summary>
        /// Output Adapter Active Measurements data table
        /// </summary>
        private DataTable dtOutputAdapterActiveMeasurements;
        /// <summary>
        /// Output Adapter Statistics
        /// </summary>
        private DataTable dtOutputAdapterStatistics;
        /// <summary>
        /// Statistics data table
        /// </summary>
        private DataTable dtStatistics;
        /// <summary>
        /// Action Adapter
        /// </summary>
        private IActionAdapterCase m_ActionAdapter;
        /// <summary>
        /// Measurement Case
        /// </summary>
        private IMeasurementCase m_IMeasurementCase;
        /// <summary>
        /// Measurements Case
        /// </summary>
        private IMeasurementsCase m_IMeasurementsCase;
        /// <summary>
        /// Input Adapter
        /// </summary>
        private IInputAdapterCase m_InputAdapter;
        /// <summary>
        /// Input Adapter Collection
        /// </summary>
        private IAdapterCollection m_InputAdapterCollection;
        /// <summary>
        /// Output Adapter
        /// </summary>
        private IOutputAdapterCase m_OutputAdapter;
        /// <summary>
        /// Input Adapter Collection
        /// </summary>
        private IAdapterCollection m_OutputAdapterCollection;
        /// <summary>
        /// Input Adapters Collection;
        /// </summary>
        private ReadOnlyCollection<IAdapter> m_ReadOnly_InputAdapterCollection;
        #endregion

        #region [ Properties ]
        public IAdapter ActionAdapter
        {
            get
            {
                return m_ActionAdapter;
            }
        }
        /// <summary>
        /// Input Adapter
        /// </summary>
        public IAdapter InputAdapter
        {
            get
            {
                return m_InputAdapter;
            }
        }

        /// <summary>
        /// Input Adapter Collection
        /// </summary>
        public IAdapterCollection InputAdapterCollection
        {
            get
            {
                return m_InputAdapterCollection;
            }
        }

        /// <summary>
        /// Output Adapter
        /// </summary>
        public IAdapter OutputAdapter
        {
            get
            {
                return m_OutputAdapter;
            }
        }

        /// <summary>
        /// Input Adapter Collection
        /// </summary>
        public IAdapterCollection OutputAdapterCollection
        {
            get
            {
                return m_OutputAdapterCollection;
            }
        }

        /// <summary>
        /// Provides testing interface for read only adapters collection
        /// </summary>
        public ReadOnlyCollection<IAdapter> ReadOnlyAdaptersCollection
        {
            get
            {
                return m_ReadOnly_InputAdapterCollection;
            }
        }

        #endregion

        #region [ Constructos ]
        public IAllAdaptersCase()
        {
            // Init Measurements Case
            m_IMeasurementCase = new IMeasurementCase();
            m_IMeasurementsCase = new IMeasurementsCase();

            // Declare Adapters
            m_InputAdapter = new IInputAdapterCase();
            m_ActionAdapter = new IActionAdapterCase();
            m_OutputAdapter = new IOutputAdapterCase();

            // Set IDs
            m_InputAdapter.InputMeasurementKeys = m_IMeasurementsCase.MeasurementKeys;
            m_InputAdapter.OutputSourceIDs = new string[] { m_IMeasurementCase.MeasurementKey.ToString() };

            m_ActionAdapter.InputMeasurementKeys = m_IMeasurementsCase.MeasurementKeys;
            m_ActionAdapter.InputSourceIDs = new string[] { m_IMeasurementCase.MeasurementKey.ToString() };

            m_OutputAdapter.InputMeasurementKeys = m_IMeasurementsCase.MeasurementKeys;
            m_OutputAdapter.InputSourceIDs = new string[] { m_IMeasurementCase.MeasurementKey.ToString() };

            // Set IDs
            m_InputAdapter.ID = 121;
            m_OutputAdapter.ID = 131;
            m_ActionAdapter.ID = 141;

            // Set Names
            m_InputAdapter.Name = "Unit Testing csv Input Adapter";
            m_OutputAdapter.Name = "Unit Testing csv Output Adapter";
            m_ActionAdapter.Name = "Unit Testing Alarm Action Adapter";

            // Add Data Source
            m_InputAdapter.DataSource = new DataSet("InputAdapter");
            m_OutputAdapter.DataSource = new DataSet("OutputAdapter");
            m_ActionAdapter.DataSource = new DataSet("ActionAdapter");

            /**********************************************************************/
            // Build OutputStreamDevices data table for action Adapter
            dtOutputStreamDevices = new DataTable("OutputStreamDevices");
            dtOutputStreamDevices.Columns.Add("ID");
            dtOutputStreamDevices.Columns.Add("Acronym");
            dtOutputStreamDevices.Columns.Add("LoadOrder");
            dtOutputStreamDevices.Columns.Add("ParentID");

            // Add row
            DataRow dtOutputStreamDevicesRow = dtOutputStreamDevices.NewRow();

            dtOutputStreamDevicesRow["ID"] = ("1").ToString();
            dtOutputStreamDevicesRow["Acronym"] = ("UnitTesting").ToString();
            dtOutputStreamDevicesRow["LoadOrder"] = ("1").ToString();
            dtOutputStreamDevicesRow["ParentID"] = ("1").ToString();

            // Populate
            dtOutputStreamDevices.Rows.Add(dtOutputStreamDevicesRow);

            /**********************************************************************/
            // Build output stream measurements
            dtOutputStreamMeasurements = new DataTable("OutputStreamMeasurements");
            dtOutputStreamMeasurements.Columns.Add("AdapterID");
            dtOutputStreamMeasurements.Columns.Add("SignalReference");
            dtOutputStreamMeasurements.Columns.Add("Historian");
            dtOutputStreamMeasurements.Columns.Add("PointID");

            // Add row
            DataRow dtOutputStreamMeasurementsRow = dtOutputStreamMeasurements.NewRow();

            dtOutputStreamMeasurementsRow["AdapterID"] = ("1").ToString();
            dtOutputStreamMeasurementsRow["SignalReference"] = ("UnitTesting").ToString();
            dtOutputStreamMeasurementsRow["Historian"] = ("1").ToString();
            dtOutputStreamMeasurementsRow["PointID"] = ("1").ToString();

            //Populate
            dtOutputStreamMeasurements.Rows.Add(dtOutputStreamMeasurementsRow);
			
            /**********************************************************************/
            // Build statistical information
            dtStatistics = new DataTable("Statistics");
            dtStatistics.Columns.Add("Enabled");
            dtStatistics.Columns.Add("Source");
            dtStatistics.Columns.Add("Name");
            dtStatistics.Columns.Add("SignalIndex");
            dtStatistics.Columns.Add("Arguments");
            dtStatistics.Columns.Add("AssemblyName");
            dtStatistics.Columns.Add("TypeName");
            dtStatistics.Columns.Add("MethodName");

            // Add data rows into statistics
            DataRow dtStatisticsRow = dtStatistics.NewRow();

            dtStatisticsRow["Enabled"] = ("1").ToString();
            dtStatisticsRow["Source"] = ("System").ToString();
            dtStatisticsRow["Name"] = ("CPU Usage").ToString();
            dtStatisticsRow["SignalIndex"] = ("1").ToString();
            dtStatisticsRow["Arguments"] = ("").ToString();
            dtStatisticsRow["AssemblyName"] = ("TimeSeriesFramework.dll").ToString();
            dtStatisticsRow["TypeName"] = ("TimeSeriesFramework.Statistics.PerformanceStatistics").ToString();
            dtStatisticsRow["MethodName"] = ("GetSystemStatistic_CPUUsage").ToString();

            dtStatistics.Rows.Add(dtStatisticsRow);

            /**********************************************************************/
            // Populate measurement table
            dtMeasurement = new DataTable("ActiveMeasurements");
            dtMeasurement.Columns.Add("ID");
            dtMeasurement.Columns.Add("SignalID");
            dtMeasurement.Columns.Add("PointTag");
            dtMeasurement.Columns.Add("Adder");
            dtMeasurement.Columns.Add("Multiplier");

            DataRow dtMeasurementRow = dtMeasurement.NewRow();

            dtMeasurementRow["ID"] = (10).ToString();
            dtMeasurementRow["SignalID"] = Guid.NewGuid().ToString();
            dtMeasurementRow["PointTag"] = "GPA_PMU_34:21";
            dtMeasurementRow["Adder"] = "UIUC\\dkholine";
            dtMeasurementRow["Multiplier"] = "0.033";

            dtMeasurement.Rows.Add(dtMeasurementRow);

            /**********************************************************************/
            // Manage Input Adapter;
            dtInputAdapter = new DataTable("InputAdapter");
            dtInputAdapterActiveMeasurements = dtMeasurement.Clone();
            dtInputAdapterStatistics = dtStatistics.Clone();

            // copy measurement rows
            foreach (DataRow row in dtMeasurement.Rows)
            {
                dtInputAdapterActiveMeasurements.ImportRow(row);
            }
            // copy statistics rows
            foreach (DataRow row in dtStatistics.Rows)
            {
                dtInputAdapterStatistics.ImportRow(row);
            }

            // Add adapter description
            dtInputAdapter.Columns.Add("ID");
            dtInputAdapter.Columns.Add("AdapterName");
            dtInputAdapter.Columns.Add("AssemblyName");
            dtInputAdapter.Columns.Add("TypeName");
            dtInputAdapter.Columns.Add("ConnectinString");

            DataRow dtInputAdapterRow = dtInputAdapter.NewRow();

            dtInputAdapterRow["ID"] = Guid.NewGuid().ToString();
            dtInputAdapterRow["AdapterName"] = "ActionAdapter";
            dtInputAdapterRow["AssemblyName"] = "PhasorProtocols.dll";
            dtInputAdapterRow["TypeName"] = "PhasorProtocols.IeeeC37_118.Concentrator";
            dtInputAdapterRow["ConnectinString"] = "requireAuthentication=false; allowSynchronizedSubscription=false; useBaseTimeOffsets=true";

            dtInputAdapter.Rows.Add(dtInputAdapterRow);

            m_InputAdapter.DataSource.Tables.Add(dtInputAdapter);
            m_InputAdapter.DataSource.Tables.Add(dtInputAdapterActiveMeasurements);
            m_InputAdapter.DataSource.Tables.Add(dtInputAdapterStatistics);

            /**********************************************************************/
            // Add Output Adapter Columns
            dtOutputAdapter = new DataTable("OutputAdapter");
            dtOutputAdapterActiveMeasurements = dtMeasurement.Clone();
            dtOutputAdapterStatistics = dtStatistics.Clone();

            // copy rows
            foreach (DataRow row in dtMeasurement.Rows)
            {
                dtOutputAdapterActiveMeasurements.ImportRow(row);
            }

            // copy statistics rows
            foreach (DataRow row in dtStatistics.Rows)
            {
                dtOutputAdapterStatistics.ImportRow(row);
            }

            // Add adapter description
            dtOutputAdapter.Columns.Add("ID");
            dtOutputAdapter.Columns.Add("AdapterName");

            DataRow dtOutputAdapterRow = dtOutputAdapter.NewRow();

            dtOutputAdapterRow["ID"] = Guid.NewGuid().ToString();
            dtOutputAdapterRow["AdapterName"] = "OutputAdapter";

            dtOutputAdapter.Rows.Add(dtOutputAdapterRow);

            m_OutputAdapter.DataSource.Tables.Add(dtOutputAdapter);
            m_OutputAdapter.DataSource.Tables.Add(dtOutputAdapterActiveMeasurements);
            m_OutputAdapter.DataSource.Tables.Add(dtOutputAdapterStatistics);

            /**********************************************************************/
            // Add Input Adapter Columns
            dtActionAdapter = new DataTable("OutputAdapter");
            dtActionAdapterActiveMeasurements = dtMeasurement.Clone();
            dtActionAdapterStatistics = dtStatistics.Clone();

            // copy rows
            foreach (DataRow row in dtMeasurement.Rows)
            {
                dtActionAdapterActiveMeasurements.ImportRow(row);
            }

            // copy statistics rows
            foreach (DataRow row in dtStatistics.Rows)
            {
                dtActionAdapterStatistics.ImportRow(row);
            }

            //Add adapter description
            dtActionAdapter.Columns.Add("ID");
            dtActionAdapter.Columns.Add("AdapterName");

            DataRow dtActionAdapterRow = dtActionAdapter.NewRow();

            dtActionAdapterRow["ID"] = Guid.NewGuid().ToString();
            dtActionAdapterRow["AdapterName"] = "ActionAdapter";

            dtActionAdapter.Rows.Add(dtActionAdapterRow);

            m_ActionAdapter.DataSource.Tables.Add(dtActionAdapter);
            m_ActionAdapter.DataSource.Tables.Add(dtActionAdapterActiveMeasurements);
            m_ActionAdapter.DataSource.Tables.Add(dtStatistics);

            // Output Stream Data
            m_ActionAdapter.DataSource.Tables.Add(dtOutputStreamDevices);
            m_ActionAdapter.DataSource.Tables.Add(dtOutputStreamMeasurements);

            /**********************************************************************/
            // Auto start adapters on request
            m_InputAdapter.AutoStart = true;
            m_OutputAdapter.AutoStart = true;
            m_ActionAdapter.AutoStart = true;

            // Dispose measurement table
            dtMeasurement.Clear();
            dtMeasurement.Dispose();

            //Input Adapter Collection Initialize
            m_InputAdapterCollection = new InputAdapterCollection();
            m_InputAdapterCollection.Add(m_InputAdapter);

            m_OutputAdapterCollection = new OutputAdapterCollection();
            m_OutputAdapterCollection.Add(OutputAdapter);

            //Initialize Input Read Only Collection
            m_ReadOnly_InputAdapterCollection = new ReadOnlyCollection<IAdapter>(m_InputAdapterCollection);
        }

        #endregion

        #region [ Dispose ]
        private bool isDisposed = false;

        ~IAllAdaptersCase()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (!isDisposed)
            {
                if (isDisposing)
                {
                    //Dispose Adapters
                    m_ActionAdapter.Dispose();
                    m_InputAdapter.Dispose();
                    m_OutputAdapter.Dispose();

                    //Dispose Adapters Collection
                    m_InputAdapterCollection.Dispose();
                }
                isDisposed = false;
            }
        }

        #endregion
    }
}