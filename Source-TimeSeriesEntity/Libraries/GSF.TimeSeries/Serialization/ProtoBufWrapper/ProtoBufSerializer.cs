using System.IO;

namespace GSF.TimeSeries.Serialization.ProtoBufWrapper
{
    class Serializer : ISerializer
    {
        static Serializer()
        {
            var model = ProtoBuf.Meta.RuntimeTypeModel.Default;
            model.Add(typeof(MeasurementSurrogate<>), true);
            model.Add(typeof(Measurement<>), false).SetSurrogate(typeof(MeasurementSurrogate<>));
            model.Add(typeof(Ticks), true);
            model.Add(typeof(Ticks), false).SetSurrogate(typeof(TicksSurrogate));
        }

        // Here's what needs to be done to boost performance:
        // The major issue is almost definitely memory allocation and garbage
        // collection, just based on how the code is written.  So the right
        // solution is allocate the memory one time and then use that from
        // then on.  The problem is if we just use a single one, we lose
        // thread safety.  If we can guarantee that these are called on
        // only one thread, we're fine.  If we lock, we're fine.
        public Serializer()
        {
        }

        // I'm extremely worried about the performance of this due to the 
        // amount of memory allocation, but until it's been analyzed, this 
        // should work for now.
        public byte[] Serialize<T>(Measurement<T> measurement)
        {
            MemoryStream stream = new MemoryStream();
            
            ProtoBuf.Serializer.Serialize<MeasurementSurrogate<T>>(
                stream,
                measurement);
            return stream.ToArray();
        }

        public Measurement<T> Deserialize<T>(byte[] input)
        {
            MemoryStream stream = new MemoryStream(input);
            return ProtoBuf.Serializer.Deserialize<MeasurementSurrogate<T>>(
                stream);
        }
    }
}
