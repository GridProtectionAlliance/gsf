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
using System.Data;
#endregion

namespace GSF.TestsSuite.TimeSeries.Cases
{
    /// <summary>
    /// Support Iaon configuration
    /// </summary>
    public class IRealTimeConfigurationCase
    {
        #region [ Members ]
        private bool isDisposed = false;
        private DataColumn[] m_DataColumns;
        private DataRow m_DataRow;
        private DataTable m_OutputAdaptersTable;
        private DataTable m_realTimeSupport;
        private DataTable m_realTimeTable;
        private DataSet m_temporalConfiguation;
        private DataSet m_temporalSupport;
        #endregion

        #region [ Properties ]
        public IRealTimeConfigurationCase()
        {
            //Initialize Data Columns
            m_DataColumns = new DataColumn[1];
            m_DataColumns[0] = new DataColumn("ID");

            //Initialize Configuration Set
            m_temporalConfiguation = new DataSet("IaonTemporal");

            //Initialize Supprot Set
            m_temporalSupport = new DataSet();
            m_temporalSupport = m_temporalConfiguation;

            //Initialize Temporal Support
            m_realTimeTable = new DataTable("TemporalSupport");

            //Initialize Output Adapters
            m_OutputAdaptersTable = new DataTable("OutputAdapters");

            //Initialize Row
            m_realTimeTable.Columns.AddRange(m_DataColumns);
            m_DataRow = m_realTimeTable.NewRow();
            m_DataRow["ID"] = Guid.NewGuid().ToString();
            m_realTimeTable.Rows.Add(m_DataRow);

            //Init Iaon
            m_temporalSupport.Tables.Add(m_realTimeTable);

            m_realTimeSupport = new DataTable();
            m_realTimeSupport = m_realTimeTable;
        }

        ~IRealTimeConfigurationCase()
        {
            Dispose(false);
        }

        public DataTable RealTimeSupport
        {
            get
            {
                return m_realTimeSupport;
            }
        }

        public DataTable RealTimeTable
        {
            get
            {
                return m_realTimeTable;
            }
        }

        public DataSet TemporalConfiguation
        {
            get
            {
                return m_temporalConfiguation;
            }
        }

        public DataSet TemporalSupport
        {
            get
            {
                return m_temporalSupport;
            }
        }

        #endregion

        #region [ Constructors ]
        #endregion

        #region [ Methods ]
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
                    m_temporalConfiguation.Clear();
                    m_temporalConfiguation.Dispose();

                    m_temporalSupport.Clear();
                    m_temporalSupport.Dispose();

                    m_realTimeTable.Clear();
                    m_realTimeTable.Dispose();

                    m_realTimeSupport.Clear();
                    m_realTimeSupport.Dispose();

                    m_OutputAdaptersTable.Clear();
                    m_OutputAdaptersTable.Dispose();
                }
                isDisposed = false;
            }
        }

        #endregion
    }
}