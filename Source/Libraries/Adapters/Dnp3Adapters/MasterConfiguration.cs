using DNP3.Interface;
using System;

namespace Dnp3Adapters
{
    public class MasterConfiguration
    {
        /// <summary>
        /// All of the settings for the connection
        /// </summary>
        public TcpClientConfig client = new TcpClientConfig();

        /// <summary>
        /// All of the settings for the master
        /// </summary>
        public MasterStackConfig master = new MasterStackConfig();
    }

    public class TcpClientConfig
    {
        /// <summary>
        /// IP address of host
        /// </summary>
        public String address = "127.0.0.1";

        /// <summary>
        /// TCP port for connection
        /// </summary>
        public UInt16 port;

        /// <summary>
        /// Connection retry interval in milliseconds
        /// </summary>
        public UInt64 retryMs;

        /// <summary>
        /// DNP3 filter level for port messages
        /// </summary>
        public FilterLevel level;
    }
}
