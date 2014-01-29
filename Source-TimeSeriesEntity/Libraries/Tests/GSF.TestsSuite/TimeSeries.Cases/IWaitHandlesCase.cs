#region [ Modification History ]
/*
 * 07/22/2012 Denis Kholine
 *  Generated Original version of source code.
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
using System.Collections.Concurrent;
using System.Threading;
#endregion

namespace GSF.TestsSuite.TimeSeries.Cases
{
    /// <summary>
    /// Wait Handles Helper
    /// </summary>
    public class IWaitHandlesCase : IDisposable
    {
        #region [ Members ]
        private static AutoResetEvent m_ARE;
        private static ManualResetEvent m_MRE;
        private AutoResetEvent[] m_AREs;
        private string m_Key;
        private ManualResetEvent[] m_MREs;
        private ConcurrentDictionary<string, AutoResetEvent> m_waitHandles;
        #endregion

        #region [ Properties ]
        /// <summary>
        /// Auto Reset Events
        /// </summary>
        public AutoResetEvent AutoResetEvent
        {
            get
            {
                return m_ARE;
            }
        }

        public AutoResetEvent[] AutoResetEvents
        {
            get
            {
                return m_AREs;
            }
        }

        /// <summary>
        /// Key
        /// </summary>
        public string Key
        {
            get
            {
                return m_Key;
            }
        }

        /// <summary>
        /// Manual Reset Events
        /// </summary>
        public ManualResetEvent ManualResetEvent
        {
            get
            {
                return m_MRE;
            }
        }

        public ManualResetEvent[] ManualResetEvents
        {
            get
            {
                return m_MREs;
            }
        }

        /// <summary>
        /// Wait Handles
        /// </summary>
        public ConcurrentDictionary<string, AutoResetEvent> waitHandles
        {
            get
            {
                return m_waitHandles;
            }
        }

        #endregion

        #region [ Constructors ]
        /// <summary>
        /// Wait Handles Case
        /// </summary>
        public IWaitHandlesCase()
        {
            m_Key = Guid.NewGuid().ToString();
            m_waitHandles = new ConcurrentDictionary<string, AutoResetEvent>();

            // Test Reset Events
            m_ARE = new AutoResetEvent(false);
            m_MRE = new ManualResetEvent(false);

            m_AREs = new AutoResetEvent[1];
            m_AREs[0] = m_ARE;

            m_MREs = new ManualResetEvent[1];
            m_MREs[0] = m_MRE;

            m_waitHandles.AddOrUpdate(m_Key, m_func_ARE(m_Key),
            (string Key, AutoResetEvent m_ResetEvt) =>
            {
                m_ARE.Set();
                m_ARE.Dispose();
                m_ARE = new AutoResetEvent(false);
                return m_ARE;
            });
        }

        #endregion

        #region [ Methods ]
        /// <summary>
        /// Auto Reset Event handler
        /// </summary>
        private Func<string, AutoResetEvent> m_func_ARE = r =>
        {
            return m_ARE;
        };

        /// <summary>
        /// Auto Reset Events updater
        /// </summary>
        private Func<string, AutoResetEvent, AutoResetEvent> m_func_ARE_Updater = (in1, in2) =>
        {
            m_ARE.Set();
            return m_ARE;
        };

        /// <summary>
        /// Manual Reset Events handler
        /// </summary>
        private Func<string, ManualResetEvent> m_func_MRE = r =>
        {
            return m_MRE;
        };
        /// <summary>
        /// Manual Reset Events updater
        /// </summary>
        private Func<string, ManualResetEvent, ManualResetEvent> m_func_MRE_Updater = (in1, in2) =>
        {
            m_MRE.Set();
            return m_MRE;
        };
        #endregion

        #region [ Dispose ]
        /// <summary>
        /// Disposed function
        /// </summary>
        private bool isDisposed = false;

        /// <summary>
        /// Wait Handles Case
        /// </summary>
        ~IWaitHandlesCase()
        {
            Dispose(false);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="isDisposing">Dispose</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (!isDisposed)
            {
                if (isDisposing)
                {
                    m_ARE.Dispose();
                    m_MRE.Dispose();
                    m_waitHandles.Clear();
                }
                isDisposed = false;
            }
        }

        #endregion
    }
}