using System.IO;
using System.Xml.Serialization;

using DNP3Adapters;

namespace DNP3ConfigGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var ser = new XmlSerializer(typeof(MasterConfiguration));
            var stream = new StreamWriter("device.xml");
            try
            {
                var config = new MasterConfiguration();
                ser.Serialize(stream, config);
            }
            finally
            {
                stream.Close();
            }
        }
    }
}
