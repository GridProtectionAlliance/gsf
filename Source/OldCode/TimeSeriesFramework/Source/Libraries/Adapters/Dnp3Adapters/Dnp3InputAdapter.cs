using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.ComponentModel;

using TimeSeriesFramework;
using TimeSeriesFramework.Adapters;
using DNP3.Interface;
using DNP3.Adapter;

namespace Dnp3Adapters
{
    [Description("DNP3: Reads measurements from a remote dnp3 endpoint")]
    public class Dnp3InputAdapter : InputAdapterBase
    {
        /// <summary>
        /// DNP3 manager shared across all of the DNP3 input adapters
        /// </summary>
        private static StackManager m_Manager = new StackManager();

        /// <summary>
        /// The filename for the communication configuration file
        /// </summary>
        private String m_commsFileName;

        /// <summary>
        /// The filename for the measurement mapping configuration file
        /// </summary>
        private String m_mappingFileName;

        /// <summary>
        /// Configuration for the master. Set during the Initialize call.
        /// </summary>
        private MasterConfiguration m_MasterConfig;

        /// <summary>
        /// Configuration for the measurement map. Set during the Initialize call.
        /// </summary>
        private MeasurementMap m_MeasMap;

        /// <summary>
        /// Gets set during the AttemptConnection() call and using during AttemptDisconnect()
        /// </summary>
        private String m_portName;

        /// <summary>
        /// A counter of the number of measurements received
        /// </summary>
        private int m_numMeasurementsReceived;

        /// <summary>
        /// Gets or sets the name of the xml file from which the communication parameters will be read
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the name of the XML file from which the communication configuration will be read"),
        DefaultValue("comms1.xml")]
        public string CommsFileName
        {
            get
            {
                return m_commsFileName;
            }
            set
            {
                m_commsFileName = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the xml file from which the measurement mapping is read
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the name of the XML file from which the communication configuration will be read"),
        DefaultValue("device1.xml")]
        public string MappingFileName
        {
            get
            {
                return m_mappingFileName;
            }
            set
            {
                m_mappingFileName = value;
            }
        }

       
        public override void Initialize()
        {
            base.Initialize();
                        
            this.Settings.TryGetValue("commsFile", out this.m_commsFileName);
            this.Settings.TryGetValue("mappingFile", out this.m_mappingFileName);                                  

            try
            {
                if (this.m_commsFileName == null) throw new ArgumentException("The required commsFile parameter was not specified");
                if (this.m_mappingFileName == null) throw new ArgumentException("The required mappingFile parameter was not specified");

                this.m_MasterConfig = this.ReadConfig<MasterConfiguration>(this.CommsFileName);
                this.m_MeasMap = this.ReadConfig<MeasurementMap>(this.MappingFileName);                
            }
            catch (Exception ex)
            {                
                this.OnProcessException(ex);                
            }            
        }       

        private T ReadConfig<T>(string path)
        {
            var ser = new System.Xml.Serialization.XmlSerializer(typeof(T));
            var stream = new StreamReader(path);
            try
            {
                return (T)ser.Deserialize(stream);
            }
            finally
            {
                stream.Close();
            }
        }
        

        protected override bool UseAsyncConnect
        {
            get { return false; }
        }

        protected override void AttemptConnection()
        {            
            var tcp = this.m_MasterConfig.client;
            var master = this.m_MasterConfig.master;
            this.m_portName = tcp.address + ":" + tcp.port;
            m_Manager.AddTCPClient(m_portName, tcp.level, tcp.retryMs, tcp.address, tcp.port);            
            var adapter = new TsfDataObserver(new MeasurementLookup(this.m_MeasMap));
            adapter.NewMeasurements += new TsfDataObserver.OnNewMeasurements(adapter_NewMeasurements);
            adapter.NewMeasurements += new TsfDataObserver.OnNewMeasurements(this.OnNewMeasurements);           
            var acceptor = m_Manager.AddMaster(m_portName, this.Name, FilterLevel.LEV_WARNING, adapter, m_MasterConfig.master);
        }

        void adapter_NewMeasurements(ICollection<IMeasurement> measurements)
        {
            this.m_numMeasurementsReceived += measurements.Count;
            if (measurements.Count > 0)
            {
                foreach (IMeasurement m in measurements)
                {
                    this.OnStatusMessage(m.ToString());
                }
            }
        }        

        protected override void AttemptDisconnection()
        {             
            //removes the port and the stack
            m_Manager.RemovePort(m_portName);        
        }

        public override bool SupportsTemporalProcessing
        {
            get { return false; }
        }

        public override string GetShortStatus(int maxLength)
        {
            return "The adapter has received " + this.m_numMeasurementsReceived + " measurements";
        }
    }    
    
}
