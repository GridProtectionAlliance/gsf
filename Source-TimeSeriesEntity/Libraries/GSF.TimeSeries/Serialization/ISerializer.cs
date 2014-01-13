using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSF.TimeSeries.Serialization
{
    interface ISerializer
    {
        byte[] Serialize<T>(Measurement<T> measurement);

        Measurement<T> Deserialize<T>(byte[] input);
    }
}
