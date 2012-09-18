using System;
using System.Collections.Generic;

namespace Dnp3Adapters
{
    public class Mapping
    {
        public Mapping()
        {
            this.tsfId = 0;
            this.dnpIndex = 0;
        }

        public Mapping(uint id, String source, UInt32 index)
        {
            this.tsfId = id;
            this.tsfSource = source;
            this.dnpIndex = index;
        }

        public uint tsfId;
        public String tsfSource;        
        public UInt32 dnpIndex;
    }

    public class MeasurementMap
    {
        public List<Mapping> binaryMap = new List<Mapping>();
        public List<Mapping> analogMap = new List<Mapping>();
        public List<Mapping> counterMap = new List<Mapping>();
        public List<Mapping> controlStatusMap = new List<Mapping>();
        public List<Mapping> setpointStatusMap = new List<Mapping>();
    }

   
}
