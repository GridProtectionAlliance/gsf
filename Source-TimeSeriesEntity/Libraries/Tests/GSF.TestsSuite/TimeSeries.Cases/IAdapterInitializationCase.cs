#region [ Modification History ]
/*
 * 07/22/2012 Denis Kholine
 *  Generated Original version of source code.
 */

#endregion [ Modification History ]

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
using System.Data;
using System.Xml;
#endregion

namespace GSF.TestsSuite.TimeSeries.Cases
{
    /// <summary>
    /// Adapter Data Table Initialization
    /// </summary>
    public class IAdapterInitializationCase : IDisposable
    {
        #region [ Members ]
        private DataColumn[] m_DataColumns;
        private DataRow m_DataRow;
        private DataSet m_DataSet;
        private DataTable m_DataTable;
        #endregion [ Members ]

        #region [ Properties ]
        public DataRow adapterRow
        {
            get
            {
                return m_DataRow;
            }
        }

        public DataSet DataSource
        {
            get
            {
                return m_DataSet;
            }
        }

        #endregion [ Properties ]

        #region [ Constructors ]
        /// <summary>
        /// Constructs in memory data set as required by ActionAdapterBase abstract class
        /// to successfully initialize adapter
        /// </summary>
        public IAdapterInitializationCase()
        {
            m_DataColumns = new DataColumn[5];
            m_DataColumns[0] = new DataColumn("ID");
            m_DataColumns[1] = new DataColumn("AdapterName");
            m_DataColumns[2] = new DataColumn("AssemblyName");
            m_DataColumns[3] = new DataColumn("TypeName");
            m_DataColumns[4] = new DataColumn("ConnectinString");

            m_DataTable = new DataTable("ActionAdapters");
            m_DataTable.Columns.AddRange(m_DataColumns);
            m_DataRow = m_DataTable.NewRow();
            m_DataRow["ID"] = Guid.NewGuid().ToString();
            m_DataRow["AdapterName"] = "UnitTestingAdapter";
            m_DataRow["AssemblyName"] = "PhasorProtocols.dll";
            m_DataRow["TypeName"] = "PhasorProtocols.IeeeC37_118.Concentrator";
            m_DataRow["ConnectinString"] = "requireAuthentication=false; allowSynchronizedSubscription=false; useBaseTimeOffsets=true";

            m_DataTable.Rows.Add(m_DataRow);
            m_DataSet = new DataSet("ActionAdapters");
            m_DataSet.Tables.Add(m_DataTable);
        }

        #endregion [ Constructors ]

        #region [ Dispose ]
        private bool isDisposed = false;

        ~IAdapterInitializationCase()
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
                    m_DataTable.Clear();
                    m_DataTable.Dispose();
                    m_DataSet.Dispose();
                }
                isDisposed = false;
            }
        }

        #endregion [ Dispose ]
    }
}