using HaE_King_Off_The_Hill.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KOTH_Tests
{
    [TestClass]
    public class PointCounterTests
    {
        [TestMethod]
        public void PointCounter_Regular_Add_Value()
        {
            const int POINTSTOADD = 10;
            const int POINTSEXPECTED = 10;

            PointCounter pointCounter = new PointCounter();

            pointCounter.AddScore(POINTSTOADD);

            Assert.AreEqual(pointCounter.Points, POINTSEXPECTED);
        }

        [TestMethod]
        public void PointCounter_Regular_Add_Value_Multiple()
        {
            const int POINTSTOADD = 10;
            const int POINTSLOOP = 10;
            const int POINTSEXPECTED = 100;

            PointCounter pointCounter = new PointCounter();

            for (int i = 0; i < POINTSLOOP; i++)
                pointCounter.AddScore(POINTSTOADD);

            Assert.AreEqual(pointCounter.Points, POINTSEXPECTED);
        }

        [TestMethod]
        public void PointCounter_Regular_Reduce_Value_Under_0()
        {
            const int POINTSTOADD = -10;
            const int POINTSEXPECTED = 0;

            PointCounter pointCounter = new PointCounter();

            pointCounter.AddScore(POINTSTOADD);

            Assert.AreEqual(pointCounter.Points, POINTSEXPECTED);
        }

        [TestMethod]
        public void PointCounter_Regular_Reduce_Value_Under_0_From_Points()
        {
            const int INITIALPOINTS = 5;
            const int POINTSTOADD = -10;
            const int POINTSEXPECTED = 0;

            PointCounter pointCounter = new PointCounter();

            pointCounter.AddScore(INITIALPOINTS);
            pointCounter.AddScore(POINTSTOADD);

            Assert.AreEqual(pointCounter.Points, POINTSEXPECTED);
        }
    }
}
