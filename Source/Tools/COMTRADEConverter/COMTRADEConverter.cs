using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSF.COMTRADE;
using GSF.EMAX;
using GSF.PQDIF.Logical;
using GSF.SELEventParser;
using System.Collections.ObjectModel;

namespace COMTRADEConverter
{
    class COMTRADEConverterViewModel : ViewModelBase
    {
        #region [ Members ]

        // Nested Types
        private class ParsedChannel
        {
            public int Index;
            public string Name;
            public List<DateTime> TimeValues;
            public IList<object> XValues;
            public IList<object> YValues;
        }

        // Constants
        private static Dictionary<string, Delegate> m_fileTypeAssociations;

        // Delegates
        private delegate void ProcessFile();

        // Events

        // Fields
        private string m_currentFilePath;
        private string m_currentFileDirectory;
        private string m_currentFileRootName;

        private string m_exportPath;
        private ObservableCollection<string> m_files;
        private IList<ParsedChannel> m_channels;
        private List<Exception> m_exceptionList;

        #endregion

        #region [ Constructors ]

        public COMTRADEConverterViewModel()
        {
            ProcessFile COMTRADE = ProcessCOMTRADE;
            ProcessFile EMAX = ProcessEMAX;
            ProcessFile PQDIF = ProcessPQDIF;
            ProcessFile SELEVE = ProcessSELEVE;

            m_fileTypeAssociations = new Dictionary<string, Delegate>()
            {
                { ".dat", COMTRADE },
                { ".d00", COMTRADE },
                { ".rcd", EMAX },
                { ".rcl", EMAX },
                { ".pqd", PQDIF },
                { ".eve", SELEVE },
                { ".sel", SELEVE }
            };

            Files = new ObservableCollection<string>();
            Files.Add("Drag and drop files here or use the add files button");
            m_exceptionList = new List<Exception>();
            ExportPath = @"%userprofile%\Documents\COMTRADE Converter Test Data\Output";
        }

        #endregion

        #region [ Properties ]

        public string ExportPath
        {
            get
            {
                return m_exportPath;
            }
            set
            {
                SetProperty(ref m_exportPath, value);
            }
        }

        private string CurrentFilePath
        {
            get
            {
                return m_currentFilePath;
            }
            set
            {
                m_currentFilePath = value;
                m_currentFileDirectory = Path.GetDirectoryName(m_currentFilePath);
                m_currentFileRootName = Path.GetFileNameWithoutExtension(m_currentFilePath);
            }
        }

        public ObservableCollection<string> Files
        {
            get
            {
                return m_files;
            }

            private set
            {
                SetProperty(ref m_files, value);
            }
        }

        public List<Exception> ExceptionList
        {
            get
            {
                return m_exceptionList;
            }
            set
            {
                m_exceptionList = value;
            }
        }

        #endregion

        #region [ Methods ]

        #region Private Methods

        #region COMTRADE Methods

        private void ProcessCOMTRADE()
        {
            string[] files = Directory.GetFiles(m_currentFileDirectory, m_currentFileRootName + ".*");
            foreach (string file in files)
            {
                string dest = Path.Combine(m_exportPath, Path.GetFileName(file));
                File.Copy(file, dest);
            }
        }

        #endregion

        #region EMAX Methods

        private void ProcessEMAX()
        {
            // Parsing an EMAX file requires a CTL file. Make sure this exists
            string controlFileName = Path.Combine(m_currentFileDirectory, m_currentFileRootName + ".ctl");

            if (File.Exists(controlFileName))
            {
                ParseEMAX(controlFileName);
                WriteDataFile(WriteSchemaFile());
            }
        }

        // Shamelessly copy pasted from the XDAWaveFormDataParser
        // Thanks other Stephen
        private void ParseEMAX(string controlFileName)
        {
            DateTime? startTime = null;
            DateTime timestamp;

            using (CorrectiveParser parser = new CorrectiveParser())
            {
                parser.ControlFile = new ControlFile(controlFileName);
                parser.FileName = m_currentFilePath;

                // Open EMAX data file
                parser.OpenFiles();

                // Parse EMAX control file into channels
                m_channels = parser.ControlFile.AnalogChannelSettings.Values
                    .Select(channel => new ParsedChannel()
                    {
                        Index = Convert.ToInt32(channel.chanlnum),
                        Name = channel.title,
                        TimeValues = new List<DateTime>(),
                        XValues = new List<object>(),
                        YValues = new List<object>()
                    })
                    .OrderBy(channel => channel.Index)
                    .ToList();

                // Read values from EMAX data file
                while (parser.ReadNext())
                {
                    timestamp = parser.CalculatedTimestamp;

                    // If this is the first frame, store this frame's
                    // timestamp as the start time of the file
                    if ((object)startTime == null)
                        startTime = timestamp;

                    // Read the values from this frame into
                    // x- and y-value collections for each channel
                    for (int i = 0; i < m_channels.Count; i++)
                    {
                        m_channels[i].TimeValues.Add(timestamp);
                        m_channels[i].XValues.Add(timestamp.Subtract(startTime.Value).TotalSeconds);
                        m_channels[i].YValues.Add(parser.CorrectedValues[i]);
                    }
                }
            }
        }

        #endregion

        #region PQDIF Methods

        private void ProcessPQDIF()
        {
            ParsePQDIF();
            WriteDataFile(WriteSchemaFile());
        }

        // Shamelessly copy pasted from the XDAWaveFormDataParser
        // Thanks other Stephen
        private void ParsePQDIF()
        {
            List<ObservationRecord> observationRecords;

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                // Parse PQDif File Data
                using (LogicalParser logicalParser = new LogicalParser(m_currentFilePath))
                {
                    observationRecords = new List<ObservationRecord>();
                    logicalParser.Open();

                    while (logicalParser.HasNextObservationRecord())
                        observationRecords.Add(logicalParser.NextObservationRecord());
                }

                // Convert to common channel format
                m_channels = observationRecords
                    .SelectMany(observation => observation.ChannelInstances)
                    .Where(channel => channel.Definition.QuantityTypeID == QuantityType.WaveForm)
                    .Select(MakeParsedChannel)
                    .ToList();
            }
        }

        #endregion

        #region SELEVE Methods

        private void ProcessSELEVE()
        {

        }

        #endregion

        #region Common Methods

        private Schema WriteSchemaFile()
        {
            DateTime? startTime = m_channels[0].TimeValues[0];
            int sampleCount = m_channels[0].YValues.Count;
            IEnumerable<ChannelMetadata> metadata = m_channels.Select(ConvertToChannelMetadata);

            string configFileName = Path.Combine(m_exportPath, m_currentFileRootName + ".cfg");
            StreamWriter configFileWriter = new StreamWriter(new FileStream(configFileName, FileMode.Create, FileAccess.Write), Encoding.ASCII);

            Schema schema = Writer.CreateSchema(metadata, "Station Name", "DeviceID", startTime.GetValueOrDefault(), sampleCount, includeFracSecDefinition: false, isBinary: false);

            configFileWriter.Write(schema.FileImage);
            configFileWriter.Flush();

            return schema;
        }

        private void WriteDataFile(Schema schema)
        {
            int sampleCount = m_channels[0].YValues.Count;

            string dataFileName = Path.Combine(m_exportPath, m_currentFileRootName + ".dat");
            StreamWriter dataFileWriter = new StreamWriter(new FileStream(dataFileName, FileMode.Create, FileAccess.Write), Encoding.ASCII);
            FileStream dataFileStream = (FileStream)dataFileWriter.BaseStream;

            for (int sample = 0; sample < sampleCount; sample++)
            {
                double[] frame = new double[m_channels.Count];
                int channelIndex = 0;
                foreach (ParsedChannel channel in m_channels)
                {
                    try
                    {
                        if (channel.YValues.Count > sample)
                        frame[channelIndex++] = (double)channel.YValues[sample];
                        else
                            frame[channelIndex++] = 0.0;
                    }
                    catch(Exception e)
                    {
                        m_exceptionList.Add(e);
                    }
                }
                // TODO: Ask Other Stephen how this ever worked when I had sample++ in
                Writer.WriteNextRecordAscii(dataFileWriter, schema, m_channels[0].TimeValues[sample], frame, (uint)sample, injectFracSecValue: false);
            }
            dataFileWriter.Flush();
        }

        // Has no references because functionality moved to WriteDataFile, but keeping it for now just in case.
        private List<double[]> ConvertChannelsToFrames() //Maybe there is a better term than "frames"?
        {
            List<double[]> frames = new List<double[]>();
            int sampleCount = m_channels[0].YValues.Count;

            for (int sampleNumber = 0; sampleNumber < sampleCount; sampleNumber++)
            {
                double[] frame = new double[m_channels.Count];
                int i = 0;
                foreach (ParsedChannel channel in m_channels)
                {
                    frame[i] = (double)channel.YValues[sampleNumber];
                    i++;
                }
                frames.Add(frame);
            }

            return frames;
        }

        private ParsedChannel MakeParsedChannel(ChannelInstance channel)
        {
            // Get the time series and value series for the given channel
            SeriesInstance timeSeries = channel.SeriesInstances.Single(series => series.Definition.ValueTypeID == SeriesValueType.Time);
            SeriesInstance valuesSeries = channel.SeriesInstances.Single(series => series.Definition.ValueTypeID == SeriesValueType.Val);

            // Set up parsed channel to be returned
            ParsedChannel parsedChannel = new ParsedChannel()
            {
                Name = channel.Definition.ChannelName,
                YValues = valuesSeries.OriginalValues.Select(Convert.ToDouble).Select(value => (object)value).ToList()
            };

            if (timeSeries.Definition.QuantityUnits == QuantityUnits.Seconds)
            {
                // If time series is in seconds from start time of the observation record,
                // TimeValues must be calculated by adding values to start time
                parsedChannel.TimeValues = timeSeries.OriginalValues
                    .Select(Convert.ToDouble)
                    .Select(seconds => (long)(seconds * TimeSpan.TicksPerSecond))
                    .Select(TimeSpan.FromTicks)
                    .Select(timeSpan => channel.ObservationRecord.StartTime + timeSpan)
                    .ToList();

                parsedChannel.XValues = timeSeries.OriginalValues;
            }
            else if (timeSeries.Definition.QuantityUnits == QuantityUnits.Timestamp)
            {
                // If time series is a collection of absolute time, seconds from start time
                // must be calculated by subtracting the start time of the observation record
                parsedChannel.TimeValues = timeSeries.OriginalValues.Cast<DateTime>().ToList();

                parsedChannel.XValues = timeSeries.OriginalValues
                    .Cast<DateTime>()
                    .Select(time => time - channel.ObservationRecord.StartTime)
                    .Select(timeSpan => timeSpan.TotalSeconds)
                    .Cast<object>()
                    .ToList();
            }

            return parsedChannel;
        }

        // Shamelessly copy pasted from XDAWaveFormDataParser
        ChannelMetadata ConvertToChannelMetadata(ParsedChannel parsedChannel)
        {
            ChannelMetadata channelRecord = new ChannelMetadata
            {
                Name = parsedChannel.Name,

                // Remove these assumptions at a later stage
                IsDigital = false,
                SignalType = GSF.Units.EE.SignalType.ALOG
            };

            return channelRecord;
        }

        #endregion

        #endregion

        #region Public Methods

        public void AddFilesWithDialog()
        {
            string filter = "Disturbance Files|*.dat;*.d00;*.rcd;*.rcl;*.pqd;*.eve;*.sel" 
                + "COMTRADE Files|*.dat;*.d00|EMAX Files|*.rcd;*.rcl|PQDIF Files|*.pqd|SEL Files|*.eve;*.sel|All Files|*.*";
            OpenFileDialog dialog = new OpenFileDialog() { Multiselect = true, Filter = filter };
            if (dialog.ShowDialog() == DialogResult.Cancel)
                return;

            AddFiles(dialog.FileNames);
        }

        public void AddFiles(string[] files)
        {
            foreach (string file in files)
            {
                Files.Add(file);
            }
        }

        public void ProcessFiles()
        {
            foreach (string file in Files)
            {
                string extension = Path.GetExtension(file).ToLower();
                if (m_fileTypeAssociations.ContainsKey(extension))
                {
                    CurrentFilePath = file;
                    m_fileTypeAssociations[extension].DynamicInvoke();
                }
            }

            Files.Clear();
        }

        public void ClearFileList()
        {
            Files.Clear();
        }

        #endregion

        #endregion

        #region [ Operators ]

        #endregion

        #region [ Static ]

        // Static Fields

        // Static Constructor

        // Static Properties

        // Static Methods

        #endregion
    }
}
