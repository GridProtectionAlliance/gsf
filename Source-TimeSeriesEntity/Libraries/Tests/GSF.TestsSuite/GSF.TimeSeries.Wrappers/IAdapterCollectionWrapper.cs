#region [ Modification History ]
/*
 * 07/07/2012 Denis Kholine
 *  Generated Original version of source code.
 *
 * 11/04/2012 Denis Kholine
 *  Change namespace
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
using System.Data;
using GSF.TestsSuite.TimeSeries.Wrappers;
using GSF.TestsSuite.TimeSeries.Cases;
using GSF.IO;
using GSF.Threading;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
#endregion

namespace GSF.TestsSuite.TimeSeries.Wrappers
{
    public class IAdapterCollectionWrapper : IAdapterCollection
    {
        #region [ Events ]
        public event EventHandler Disposed;
        public event EventHandler InputMeasurementKeysUpdated;
        public event EventHandler OutputMeasurementsUpdated;
        public event EventHandler<EventArgs<Exception>> ProcessException;
        public event EventHandler<EventArgs<string>> StatusMessage;
        public event EventHandler<EventArgs<IMeasurement>> Notify;
        public event EventHandler ConfigurationChanged;
        #endregion

        #region [ Events Handlers ]

        public void OnDisposed()
        {
            if (Disposed != null)
            {
                EventArgs e = new EventArgs();
                Disposed(this, e);
            }
        }
        public void OnInputMeasurementKeyUpdated()
        {
            if (InputMeasurementKeysUpdated != null)
            {
                EventArgs e = new EventArgs();
                InputMeasurementKeysUpdated(this, e);
            }
        }
        public void OnOutputMeasurementUpdated()
        {
            if (OutputMeasurementsUpdated != null)
            {
                EventArgs e = new EventArgs();
                OutputMeasurementsUpdated(this, e);
            }
        }
        public void OnProcessException()
        {
            if (ProcessException != null)
            {
                EventArgs<Exception> e = new EventArgs<Exception>();
                ProcessException(this, e);
            }
        }
        public void OnStatusMessage()
        {
            if (StatusMessage != null)
            {
                EventArgs<string> e = new EventArgs<string>();
                StatusMessage(this, e);
            }
        }

        #endregion

        #region [ Event Handles ]
        public void OnProcessException(EventArgs<Exception> e)
        { }
        public void OnStatusMessage(EventArgs<string> e)
        { }
        #endregion

        #region [ Properties ]
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

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public string DataMember
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

        public DataSet DataSource
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

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
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

        public IAdapter this[int index]
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

        #endregion

        #region [ Methods ]
        public void Add(IAdapter item)
        {
            throw new NotImplementedException();
        }
        public void AssignParentCollection(IAdapterCollection parent)
        {
            throw new NotImplementedException();
        }
        public void Clear()
        {
            throw new NotImplementedException();
        }
        public bool Contains(IAdapter item)
        {
            throw new NotImplementedException();
        }
        public void CopyTo(IAdapter[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }
        public IEnumerator<IAdapter> GetEnumerator()
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
        public int IndexOf(IAdapter item)
        {
            throw new NotImplementedException();
        }
        public void Initialize()
        {
            throw new NotImplementedException();
        }
        public void Insert(int index, IAdapter item)
        {
            throw new NotImplementedException();
        }
        public bool Remove(IAdapter item)
        {
            throw new NotImplementedException();
        }
        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }
        public void ResetStatistics()
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
        public bool TryCreateAdapter(System.Data.DataRow adapterRow, out IAdapter adapter)
        {
            throw new NotImplementedException();
        }
        public bool TryGetAdapterByID(uint id, out IAdapter adapter)
        {
            throw new NotImplementedException();
        }

        public bool TryGetAdapterByName(string name, out IAdapter adapter)
        {
            throw new NotImplementedException();
        }
        public bool TryInitializeAdapterByID(uint id)
        {
            throw new NotImplementedException();
        }
        public bool WaitForInitialize(int timeout)
        {
            throw new NotImplementedException();
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
        #endregion



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
    }
}