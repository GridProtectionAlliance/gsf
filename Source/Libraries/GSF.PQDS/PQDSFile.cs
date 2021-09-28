//******************************************************************************************************
//  PQDSFile.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  03/06/2020 - Christoph Lackner
//       Generated original version of source code.
//
//******************************************************************************************************


using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GSF.PQDS
{
    /// <summary>
    /// Class that represents a PQDS file.
    /// </summary>
    public class PQDSFile
    {
        #region[Properties]

        private List<MetaDataTag> m_metaData;
        private List<DataSeries> m_Data;
        private DateTime m_initialTS;

        #endregion[Properties]

        #region[Constructors]
        /// <summary>
        /// Creates a new PQDS file.
        /// </summary>
        /// <param name="dataSeries"> Measurment data to be included as <see cref="DataSeries"/></param>
        /// <param name="initialTimeStamp"> Timestamp used as the beginning of the PQDS file </param>
        /// <param name="metaData"> List of MetaData to be included in the PQDS file as <see cref="MetaDataTag{DataType}"/> </param>
        public PQDSFile(List<MetaDataTag> metaData, List<DataSeries> dataSeries, DateTime initialTimeStamp)
        {
            if (metaData is null) { this.m_metaData = new List<MetaDataTag>(); }
            else { this.m_metaData = metaData; }

            this.m_initialTS = initialTimeStamp;
            this.m_Data = dataSeries;
        }

        /// <summary>
        /// Creates a new PQDS file.
        /// </summary>
        public PQDSFile()
        {
            this.m_metaData = new List<MetaDataTag>();
            this.m_Data = new List<DataSeries>();
        }

        #endregion[Constructors]

        #region[Methods]

        private void GetStartTime()
        {
            DateTime result;
            int? day = null;
            int? month = null;
            int? year = null;

            if (this.m_metaData.Select(item => item.Key).Contains("eventdate"))
            {
                string val = ((MetaDataTag<String>)this.m_metaData.Find(item => item.Key == "eventdate")).Value;
                if (DateTime.TryParseExact(val, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                {
                    day = result.Day;
                    month = result.Month;
                    year = result.Year;
                }
            }
            if (day is null)
            {
                if (this.m_metaData.Select(item => item.Key).Contains("eventday"))
                {
                    day = ((MetaDataTag<int>)this.m_metaData.Find(item => item.Key == "eventday")).Value;
                }
                else
                {
                    day = DateTime.Now.Day;
                }
            }
            if (month is null)
            {
                if (this.m_metaData.Select(item => item.Key).Contains("eventmonth"))
                {
                    month = ((MetaDataTag<int>)this.m_metaData.Find(item => item.Key == "eventmonth")).Value;
                }
                else
                {
                    month = DateTime.Now.Month;
                }
            }
            if (year is null)
            {
                if (this.m_metaData.Select(item => item.Key).Contains("eventyear"))
                {
                    year = ((MetaDataTag<int>)this.m_metaData.Find(item => item.Key == "eventyear")).Value;
                }
                else
                {
                    year = DateTime.Now.Year;
                }
            }

            int? hour = null;
            int? minute = null;
            int? second = null;

            if (this.m_metaData.Select(item => item.Key).Contains("eventtime"))
            {
                string val = ((MetaDataTag<String>)this.m_metaData.Find(item => item.Key == "eventtime")).Value;
                if (DateTime.TryParseExact(val, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                {
                    hour = result.Hour;
                    minute = result.Minute;
                    second = result.Second;
                }
            }
            if (hour is null)
            {
                if (this.m_metaData.Select(item => item.Key).Contains("eventhour"))
                {
                    hour = ((MetaDataTag<int>)this.m_metaData.Find(item => item.Key == "eventhour")).Value;
                }
                else
                {
                    hour = DateTime.Now.Hour;
                }
            }
            if (minute is null)
            {
                if (this.m_metaData.Select(item => item.Key).Contains("eventminute"))
                {
                    minute = ((MetaDataTag<int>)this.m_metaData.Find(item => item.Key == "eventminute")).Value;
                }
                else
                {
                    minute = DateTime.Now.Minute;
                }
            }
            if (second is null)
            {
                if (this.m_metaData.Select(item => item.Key).Contains("eventsecond"))
                {
                    second = ((MetaDataTag<int>)this.m_metaData.Find(item => item.Key == "eventsecond")).Value;
                }
                else
                {
                    second = DateTime.Now.Second;
                }
            }


            result = new DateTime((int)year, (int)month, (int)day, (int)hour, (int)minute, (int)second);

            this.m_initialTS = result;
        }

        private MetaDataTag CreateMetaData(string[] flds)
        {

            string dataTypeString = flds[3].Trim().ToUpper();
            PQDSMetaDataType dataType;

            switch (dataTypeString)
            {
                case "N":
                    {
                        dataType = PQDSMetaDataType.Numeric;
                        break;
                    }
                case "E":
                    {
                        dataType = PQDSMetaDataType.Enumeration;
                        break;
                    }
                case "B":
                    {
                        dataType = PQDSMetaDataType.Binary;
                        break;
                    }
                case "A":
                    {
                        dataType = PQDSMetaDataType.AlphaNumeric;
                        break;
                    }
                default:
                    {
                        dataType = PQDSMetaDataType.Text;
                        break;
                    }
            }

            string key = flds[0].Trim().ToLower();
            string note = flds[4].Trim('"');
            string unit = flds[2].Trim('"');

            switch (dataType)
            {
                case (PQDSMetaDataType.AlphaNumeric):
                    {
                        string value = flds[1].Trim('"');
                        return new MetaDataTag<String>(key, value, dataType, unit, note);
                    }
                case (PQDSMetaDataType.Text):
                    {
                        string value = flds[1].Trim('"');
                        return new MetaDataTag<String>(key, value, dataType, unit, note);
                    }
                case (PQDSMetaDataType.Enumeration):
                    {
                        int value = Convert.ToInt32(flds[1].Trim('"'));
                        return new MetaDataTag<int>(key, value, dataType, unit, note);
                    }
                case (PQDSMetaDataType.Numeric):
                    {
                        double value = Convert.ToDouble(flds[1].Trim('"'));
                        return new MetaDataTag<double>(key, value, dataType, unit, note);
                    }
                case (PQDSMetaDataType.Binary):
                    {
                        Boolean value = Convert.ToBoolean(flds[1].Trim('"'));
                        return new MetaDataTag<Boolean>(key, value, dataType, unit, note);
                    }
                default:
                    {
                        string value = flds[1].Trim('"');
                        return new MetaDataTag<String>(key, value, dataType, unit, note);
                    }
            }


        }

        private Boolean IsDataHeader(string line)
        {
            if (!line.Contains(","))
                return false;
            String[] flds = line.Split(',');

            if (flds[0].ToLower().Trim() == "waveform-data")
                return true;

            return false;
        }

        /// <summary>
        /// List of included Metadata tags.
        /// </summary>
        public List<MetaDataTag> MetaData
        {
            get { return this.m_metaData; }
        }

        /// <summary>
        /// List of data included in PQDS file as <see cref="DataSeries"/>.
        /// </summary>
        public List<DataSeries> Data
        {
            get { return this.m_Data; }
        }

        /// <summary>
        /// Writes the content to a .csv file.
        /// </summary>
        /// <param name="stream"> The <see cref="StreamWriter"/> to write the data to. </param>
        /// <param name="progress"> <see cref="IProgress{Double}"/> Progress Token</param>
        public void WriteToStream(StreamWriter stream, IProgress<double> progress)
        {
            int n_data = this.Data.Select((item) => item.Length).Max();
            int n_total = n_data + n_data + this.m_metaData.Count() + 1;

            //create the metadata header
            List<string> lines = new List<string>();
            lines = this.m_metaData.Select(item => item.Write()).ToList();

            lines.AddRange(DataLines(n_total, progress));

            for (int i = 0; i < lines.Count(); i++)
            {
                stream.WriteLine(lines[i]);
                progress.Report((double)(n_data + i) / n_total);
            }
            

        }

        /// <summary>
        /// Writes the content to a .csv file.
        /// </summary>
        /// <param name="file"> file name </param>
        /// <param name="progress"> <see cref="IProgress{Double}"/> Progress Token</param>
        public void WriteToFile(string file, IProgress<double> progress)
        {
            // Open the file and write in each line
            using (StreamWriter fileWriter = new StreamWriter(File.OpenWrite(file)))
            {
                WriteToStream(fileWriter, progress);
            }

        }
        /// <summary>
        /// Writes the content to a .csv file.
        /// </summary>
        /// <param name="file"> file name </param>
        public void WriteToFile(string file)
        {
            Progress<double> prog = new Progress<double>();
            WriteToFile(file, prog);
        }

        /// <summary>
        /// Writes the content to an output Stream.
        /// </summary>
        /// <param name="stream"> The <see cref="StreamWriter"/> to write the data to. </param>
        public void WriteToStream(StreamWriter stream)
        {
            Progress<double> prog = new Progress<double>();
            WriteToStream(stream, prog);
        }



        /// <summary>
        /// Reads the content from a PQDS File.
        /// </summary>
        /// <param name="filename"> file name</param>
        public void ReadFromFile(string filename)
        {
            Progress<double> prog = new Progress<double>();
            ReadFromFile(filename, prog);
        }


        /// <summary>
        /// Reads the content from a PQDS File.
        /// </summary>
        /// <param name="filename"> file name</param>
        /// <param name="progress"> <see cref="IProgress{Double}"/> Progress Token </param>
        public void ReadFromFile(string filename, IProgress<double> progress)
        {
            List<string> lines = new List<string>();
            // Open the file and read each line
            using (StreamReader fileReader = new StreamReader(File.OpenRead(filename)))
            {
                while (!fileReader.EndOfStream)
                {
                    lines.Add(fileReader.ReadLine().Trim());
                }
            }

            int index = 0;
            String[] flds;
            // Parse MetaData Section
            this.m_metaData = new List<MetaDataTag>();

            while (!(IsDataHeader(lines[index])))
            {
                if (!lines[index].Contains(","))
                {
                    index++;
                    continue;
                }

                flds = lines[index].Split(',');

                if (flds.Count() < 5)
                {
                    index++;
                    continue;
                }
                this.m_metaData.Add(CreateMetaData(flds));
                index++;

                if (index == lines.Count())
                { throw new InvalidDataException("PQDS File not valid"); }
                progress.Report((double)index / (double)lines.Count());
            }

            //Parse Data  Header
            flds = lines[index].Split(',');

            if (flds.Count() < 2)
            {
                throw new InvalidDataException("PQDS File has invalid data section or no data");
            }

            this.m_Data = new List<DataSeries>();
            List<string> signals = new List<string>();
            List<List<DataPoint>> data = new List<List<DataPoint>>();


            for (int i = 1; i < flds.Count(); i++)
            {
                if (signals.Contains(flds[i].Trim().ToLower()))
                {
                    continue;
                }
                this.m_Data.Add(new DataSeries(flds[i].Trim().ToLower()));
                signals.Add(flds[i].Trim().ToLower());
                data.Add(new List<DataPoint>());
            }

            index++;
            //Parse Data
            GetStartTime();

            while (index < lines.Count())
            {
                if (!lines[index].Contains(","))
                {
                    index++;
                    continue;
                }

                flds = lines[index].Split(',');

                if (flds.Count() != (this.m_Data.Count() + 1))
                {
                    index++;
                    continue;
                }
                DateTime TS;
                double milliseconds;
                try
                {
                    milliseconds = Convert.ToDouble(flds[0].Trim());
                    double ticks = milliseconds * 10000;
                    TS = this.m_initialTS.AddTicks((int)ticks);
                }
                catch
                {
                    index++;
                    continue;
                }

                for (int i = 0; i < signals.Count(); i++)
                {
                    try
                    {
                        double value = Convert.ToDouble(flds[i + 1].Trim());
                        data[i].Add(new DataPoint() { Time = TS, Value = value, Milliseconds = milliseconds });
                    }
                    catch
                    {
                        continue;
                    }
                }

                progress.Report((double)index / (double)lines.Count());
                index++;
            }

            for (int i = 0; i < signals.Count(); i++)
            {
                int j = this.m_Data.FindIndex(item => item.Label == signals[i]);
                this.m_Data[j].Series = data[j];
            }
        }

        private List<String> DataLines(int n_total, IProgress<double> progress)
        {
            List<string> result = new List<string>();

            //ensure they all start at the same Time
            List<string> measurements = this.m_Data.Select(item => item.Label).ToList();
            DateTime initalStart = this.m_Data.Select(item => item.Series[0].Time).Min();
            List<TimeSpan> startTime = this.m_Data.Select(item => item.Series[0].Time - initalStart).ToList();

            //1 ms difference is ok
            if (startTime.Max().TotalMilliseconds > 1)
            {
                throw new Exception("The measurements start at different times");
            }

            //write the header
            result.Add("waveform-data," + String.Join(",", measurements));


            //write the Data
            // Logic for skipping datapoints if they don't have the same sampling rate
            List<int> samplingRates = m_Data.Select(item => item.Length).Distinct().ToList();

            int n_data = samplingRates.Max();

            Dictionary<int,  Func< int, DataSeries, double>> reSampling = new Dictionary<int, Func< int, DataSeries, double>>();

            if (samplingRates.Any(f => ((double)n_data / (double)f) % 1 != 0))
                throw new Exception("Sampling Rates in this File do not match and are not multiples of each other.");

            reSampling = samplingRates.Select(item => new KeyValuePair<int, Func<int, DataSeries, double>>(item, (int index, DataSeries ds) => {
                int n = n_data / item;
                if (index % n == 0)
                    return ds.Series[index / n].Value;
                else
                    return double.NaN;
            }))
                .ToDictionary(item => item.Key, item => item.Value);
            
            for (int i = 0; i < n_data; i++)
            {
                TimeSpan dT = m_Data[0].Series[i].Time - m_initialTS;
                result.Add(Convert.ToString(dT.TotalMilliseconds) + "," +
                    String.Join(",", m_Data.Select(item => {
                        double v = reSampling[item.Length](i, item);
                        if (double.IsNaN(v))
                            return "NaN".PadLeft(12);
                        return String.Format("{0:F12}", v);
                        }).ToList()));
                progress.Report((double)i / (double)n_total);
            }

            return result;

        }

        #endregion[Methods]

        #region [ Static ]
        /// <summary>
        /// Reads PQDS file from file path and returns <see cref="PQDSFile"/> object.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <returns>New <see cref="PQDSFile"/> using file contents</returns>
        public static PQDSFile Read(string filePath) {
            PQDSFile file = new PQDSFile();
            file.ReadFromFile(filePath);
            return file;
        }
        #endregion
    }


}
