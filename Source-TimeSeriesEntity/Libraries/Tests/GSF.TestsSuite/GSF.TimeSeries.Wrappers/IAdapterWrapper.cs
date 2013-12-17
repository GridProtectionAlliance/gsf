#region [ Modification History ]
/*
 * 07/07/2012 Denis Kholine
 *  Generated Original version of source code.
 *
 * 08/29/2012 Denis Kholine
 *  Update event handles
 */
#endregion

#region [ University of Illinois/NCSA Open Source License ]
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
using System.Collections.Generic;
using GSF.TestsSuite.TimeSeries.Cases;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
#endregion

namespace GSF.TestsSuite.TimeSeries.Wrappers
{
    public class IAdapterWrapper : IAdapter
    {
        #region [ Events ]
        public event EventHandler Disposed;
        public event EventHandler InputMeasurementKeysUpdated;
        public event EventHandler OutputMeasurementsUpdated;
        public event EventHandler<EventArgs<Exception>> ProcessException;
        public event EventHandler<EventArgs<string>> StatusMessage;
        #endregion

        #region [ Events Code ]
        /// <summary>
        /// On Adapter Disposed event
        /// </summary>
        public void OnDisposed()
        {
            if (Disposed != null)
            {
                EventArgs e = new EventArgs();
                Disposed(this, e);
            }
        }
        /// <summary>
        /// On Measurements Updated event
        /// </summary>
        public void OnMeasurementsUpdated()
        {
            if (OutputMeasurementsUpdated != null)
            {
                EventArgs e = new EventArgs();
                OutputMeasurementsUpdated(this, e);
            }
        }
        /// <summary>
        ///  On Output Measurement Keys Updated event
        /// </summary>
        public void OnInputMeasurementKeysUpdated()
        {
            if (InputMeasurementKeysUpdated != null)
            {
                EventArgs e = new EventArgs();
                InputMeasurementKeysUpdated(this, e);
            }
        }
        /// <summary>
        /// On Adapter Process Exception Event
        /// </summary>
        public void OnProcessException()
        {
            if (ProcessException!=null)
            {
                EventArgs<Exception> e = new EventArgs<Exception>();
                ProcessException(this,e);
            }
        }
        /// <summary>
        /// On Adapter Status Message Event
        /// </summary>
        public void OnStatusMessage()
        {
            if(StatusMessage!=null)
            {
                EventArgs<string> e = new EventArgs<string>();
                StatusMessage(this,e);
            }
        }
        #endregion

      

        /**/
        public bool AutoStart
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string ConnectionString
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public System.Data.DataSet DataSource
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool Enabled
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public uint ID
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int InitializationTimeout
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool Initialized
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public MeasurementKey[] InputMeasurementKeys
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string Name
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IMeasurement[] OutputMeasurements
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<IAdapter> Parent
        {
            get { throw new NotImplementedException(); }
        }

        public long ProcessedMeasurements
        {
            get { throw new NotImplementedException(); }
        }

        public int ProcessingInterval
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool ProcessMeasurementFilter
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public Dictionary<string, string> Settings
        {
            get { throw new NotImplementedException(); }
        }

        public DateTime StartTimeConstraint
        {
            get { throw new NotImplementedException(); }
        }

        public string Status
        {
            get { throw new NotImplementedException(); }
        }

        public DateTime StopTimeConstraint
        {
            get { throw new NotImplementedException(); }
        }

        public bool SupportsTemporalProcessing
        {
            get { throw new NotImplementedException(); }
        }

        public void AssignParentCollection(IAdapterCollection parent)
        {
            throw new NotImplementedException();
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }
        public System.Threading.AutoResetEvent GetExternalEventHandle(string name)
        {
            throw new NotImplementedException();
        }
        public string GetShortStatus(int maxLength)
        {
            throw new NotImplementedException();
        }
        public void Initialize()
        {
            throw new NotImplementedException();
        }
        public void SetTemporalConstraint(string startTime, string stopTime, string constraintParameters)
        {
            throw new NotImplementedException();
        }
        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
        public bool WaitForInitialize(int timeout)
        {
            throw new NotImplementedException();
        }

        public event EventHandler<EventArgs<IMeasurement>> Notify;

        public event EventHandler ConfigurationChanged;

        public long DependencyTimeout
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        event EventHandler<EventArgs<IMeasurement>> IAdapter.Notify
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        event EventHandler<EventArgs<string>> IAdapter.StatusMessage
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        event EventHandler<EventArgs<Exception>> IAdapter.ProcessException
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        event EventHandler IAdapter.InputMeasurementKeysUpdated
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        event EventHandler IAdapter.OutputMeasurementsUpdated
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        event EventHandler IAdapter.ConfigurationChanged
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        System.Data.DataSet IAdapter.DataSource
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        string IAdapter.ConnectionString
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        Dictionary<string, string> IAdapter.Settings
        {
            get { throw new NotImplementedException(); }
        }

        string IAdapter.Name
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        uint IAdapter.ID
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        bool IAdapter.Initialized
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        int IAdapter.InitializationTimeout
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        long IAdapter.DependencyTimeout
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        bool IAdapter.AutoStart
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        bool IAdapter.ProcessMeasurementFilter
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        MeasurementKey[] IAdapter.InputMeasurementKeys
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        IMeasurement[] IAdapter.OutputMeasurements
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        long IAdapter.ProcessedMeasurements
        {
            get { throw new NotImplementedException(); }
        }

        bool IAdapter.SupportsTemporalProcessing
        {
            get { throw new NotImplementedException(); }
        }

        DateTime IAdapter.StartTimeConstraint
        {
            get { throw new NotImplementedException(); }
        }

        DateTime IAdapter.StopTimeConstraint
        {
            get { throw new NotImplementedException(); }
        }

        int IAdapter.ProcessingInterval
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        void IAdapter.Start()
        {
            throw new NotImplementedException();
        }

        void IAdapter.Stop()
        {
            throw new NotImplementedException();
        }

        string IAdapter.GetShortStatus(int maxLength)
        {
            throw new NotImplementedException();
        }

        void IAdapter.SetTemporalConstraint(string startTime, string stopTime, string constraintParameters)
        {
            throw new NotImplementedException();
        }

        event EventHandler ISupportLifecycle.Disposed
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        void ISupportLifecycle.Initialize()
        {
            throw new NotImplementedException();
        }

        bool ISupportLifecycle.Enabled
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
        }

        string IProvideStatus.Name
        {
            get { throw new NotImplementedException(); }
        }

        string IProvideStatus.Status
        {
            get { throw new NotImplementedException(); }
        }
    }
}