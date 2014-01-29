#region [ Modification History ]
/*
 * 07/22/2012 Denis Kholine
 *  Generated Original version of source code.
 *
 * 08/29/2012 Denis Kholine
 *  Relocate code into test suite
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
using System.Threading;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.TimeSeries.Transport;
#endregion

namespace GSF.TestsSuite.TimeSeries.Cases
{
    /// <summary>
    /// Data Publisher Unit Testing case
    /// </summary>
    public class IDataPublisherCase
    {
        #region [ Members ]
        private IActionAdapterCase adaptercollectioncase;

        //private IMeasurementCase m_measurement;
        private DataPublisher m_datapublisher;
        private IWaitHandlesCase m_waithandles;
        //    private Guid signalID;

        #endregion

        #region [ Properties ]
        public AutoResetEvent AutoResetEvent
        {
            get
            {
                return m_waithandles.AutoResetEvent;
            }
        }

        public DataPublisher DataPublisher
        {
            get
            {
                return m_datapublisher;
            }
        }

        public string Key
        {
            get
            {
                return m_waithandles.Key;
            }
        }

        #endregion

        #region [ Constructors ]
        public IDataPublisherCase()
        {
            m_waithandles = new IWaitHandlesCase();
            m_datapublisher = new DataPublisher();
           
           // m_dam_waithandles.waitHandles);
            adaptercollectioncase = new IActionAdapterCase();
        }

        #endregion

        #region [ Dispose ]
        private bool isDisposed = false;

        ~IDataPublisherCase()
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
                    m_waithandles.Dispose();
                   // m_datapublisher.Clear();
                    m_datapublisher.Dispose();
                }
                isDisposed = false;
            }
        }

        #endregion
    }
}