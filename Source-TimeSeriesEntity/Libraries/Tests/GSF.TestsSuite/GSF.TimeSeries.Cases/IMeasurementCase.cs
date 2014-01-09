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
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
#endregion

namespace GSF.TestsSuite.TimeSeries.Cases
{
    /// <summary>
    /// Measurement Case
    /// </summary>
    public class IMeasurementCase : IDisposable
    {
        #region [ Members ]
        private DateTime datetime1;
        private bool isDisposed = false;
        private Measurement<double> measurement1;
        private MeasurementKey measurementkey1;
        private Guid signalid1;

        #endregion

        #region [ Properties ]
        public IMeasurementCase()
        {
            datetime1 = DateTime.UtcNow;
            signalid1 = Guid.NewGuid();
           // measurementkey1 = new MeasurementKey(signalid1, 10, "UnitTest");
            measurement1 = new Measurement<double>(signalid1,datetime1.Ticks,MeasurementStateFlags.Normal,10);
           // measurement1.ID = measurementkey1;
           // measurement1.StateFlags = MeasurementStateFlags.Normal;
           // measurement1.Value = 10;
            //measurement1.Timestamp = datetime1;
           // measurement1.ID = Guid.NewGuid();
        }

        ~IMeasurementCase()
        {
            Dispose(false);
        }

        public Measurement<double> Measurement
        {
            get
            {
                return measurement1;
            }
        }

        public MeasurementKey MeasurementKey
        {
            get
            {
                return measurementkey1;
            }
        }

        public Guid SignalID
        {
            get
            {
                return signalid1;
            }
        }

        #endregion

        #region [ Constructors ]
        #endregion

        #region [ Dispose ]
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
                }
                isDisposed = false;
            }
        }

        #endregion
    }
}