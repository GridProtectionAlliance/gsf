using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSF.Geo.Tests
{
    [TestClass]
    public class DistanceTests
    {
        #region [ Constants ]
        
        private const double GPALongitude = 35.043695517962426;
        private const double GPALattitude = -85.30907977980011;

        private const double TVALongitude = 35.043413104632556;
        private const double TVALattitude = -85.30980478148788;

        
        private const double SDGELongitude = 32.82440734588638;
        private const double SDGELattitude = -117.14289125148301;

        private const double SydneyLongitude = -33.856834016746;
        private const double SydneyLattitude = 151.21531449799255;

        private const double GPAtoTVA = 73;
        private const double GPAtoSDGE = 2934814;
        private const double GPAtoSydney = 14902366;

        // Accuracy within 100m should be ok
        private const double threshold = 100.0;
        #endregion

        #region [ Members ]

        private GeoCoordinate GPAOffices = new GeoCoordinate(GPALattitude, GPALongitude);
        private GeoCoordinate TVAOffices = new GeoCoordinate(TVALattitude, TVALongitude);
        private GeoCoordinate SDGEOffices = new GeoCoordinate(SDGELattitude, SDGELongitude);
        private GeoCoordinate Sydney = new GeoCoordinate(SydneyLattitude, SydneyLongitude);
        #endregion

        [TestMethod]
        public void LattitudeTest()
        {
            Assert.IsTrue(GPAOffices.Latitude == GPALattitude);
            Assert.IsTrue(Sydney.Latitude == SydneyLattitude);
        }

        [TestMethod]
        public void LongitudeTest()
        {
            Assert.IsTrue(GPAOffices.Longitude == GPALongitude);
            Assert.IsTrue(Sydney.Longitude == SydneyLongitude);
        }

        [TestMethod]
        public void DistanceTest()
        {
            double distance1 = GPAOffices.Distance(TVAOffices);

            Assert.IsTrue(Math.Abs(GPAtoTVA - distance1) < threshold);

        }

        
    }
}
