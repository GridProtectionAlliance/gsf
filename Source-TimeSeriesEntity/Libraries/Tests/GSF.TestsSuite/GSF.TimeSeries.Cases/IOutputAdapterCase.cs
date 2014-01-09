#region [ Modification History ]
/*
 * 07/22/2012 Denis Kholine
 *  Generated Original version of source code.
 *
 * 08/29/2012 Denis Kholine
 *  Add csv output adater case methods
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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text; 
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
#endregion

namespace GSF.TestsSuite.TimeSeries.Cases
{
    /// <summary>
    /// Represents an output adapter that writes measurements to a CSV file.
    /// </summary>
    [Description("CSV: archives measurements to a CSV file.")]
    public class IOutputAdapterCase : OutputAdapterBase
    {
        #region [ Members ]
        // Fields
        private string m_fileName;
        private int m_measurementCount;
        private StreamWriter m_outStream;
        #endregion

        #region [ Constructors ]
        /// <summary>
        /// Initializes a new instance of the <see cref="CsvOutputAdapter"/> class.
        /// </summary>
        public IOutputAdapterCase()
        {
            m_fileName = "measurements.csv";
            m_measurementCount = 0;
        }

        #endregion

        #region [ Properties ]
        /// <summary>
        /// Gets or sets the name of the CSV file.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the name of the CSV file to which measurements will be archived."),
        DefaultValue("measurements.csv")]
        public string FileName
        {
            get
            {
                return m_fileName;
            }
            set
            {
                m_fileName = value;
            }
        }

        /// <summary>
        /// Returns a flag that determines if measurements sent to this
        /// <see cref="CsvOutputAdapter"/> are destined for archival.
        /// </summary>
        public override bool OutputIsForArchive
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns the detailed status of this <see cref="CsvInputAdapter"/>.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append(base.Status);
                status.AppendFormat("                 File name: {0}", m_fileName);
                status.AppendLine();

                return status.ToString();
            }
        }

        /// <summary>
        /// Gets a flag that determines if this <see cref="CsvOutputAdapter"/>
        /// uses an asynchronous connection.
        /// </summary>
        protected override bool UseAsyncConnect
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region [ Methods ]
        /// <summary>
        /// Gets a short one-line status of this <see cref="CsvOutputAdapter"/>.
        /// </summary>
        /// <param name="maxLength">Maximum length of the status message.</param>
        /// <returns>Text of the status message.</returns>
        public override string GetShortStatus(int maxLength)
        {
            return string.Format("Archived {0} measurements to CSV file.", m_measurementCount).CenterText(maxLength);
        }

        /// <summary>
        /// Initializes this <see cref="CsvOutputAdapter"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;
            string setting;

            // Load optional parameters

            if (settings.TryGetValue("fileName", out setting))
                m_fileName = setting;
        }

        /// <summary>
        /// Attempts to connect to this <see cref="CsvOutputAdapter"/>.
        /// </summary>
        protected override void AttemptConnection()
        {
            m_outStream = new StreamWriter(m_fileName);
            m_outStream.WriteLine(s_fileHeader);
        }

        /// <summary>
        /// Attempts to disconnect from this <see cref="CsvOutputAdapter"/>.
        /// </summary>
        protected override void AttemptDisconnection()
        {
            m_outStream.Close();
        }
        protected override void ProcessEntities(ITimeSeriesEntity[] entities)
        {
            StringBuilder builder = new StringBuilder();

            foreach (IMeasurement measurement in entities)
            {
                builder.Append(measurement.ID);
                builder.Append(',');
                builder.Append((long)measurement.Timestamp);
                builder.Append(System.Environment.NewLine);
            }

            m_outStream.Write(builder.ToString());
            m_measurementCount += entities.Length;
        }
        #endregion

        #region [ Static ]
        // Static Fields
        private static string s_fileHeader = "Signal ID,Measurement Key,Timestamp,Value";

        #endregion [ Static ]


    }
}