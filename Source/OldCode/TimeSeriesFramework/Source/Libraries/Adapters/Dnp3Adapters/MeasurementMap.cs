using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dnp3Adapters
{
    public class Mapping
    {
        public Mapping()
        {
            this.tsfId = "";
            this.dnpIndex = 0;
        }

        public Mapping(String id, UInt32 index)
        {
            this.tsfId = id;
            this.dnpIndex = index;
        }

        public string tsfId;
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
