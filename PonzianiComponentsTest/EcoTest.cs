using Microsoft.VisualStudio.TestTools.UnitTesting;
using PonzianiComponents.Chesslib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PonzianiComponentsTest
{
    [TestClass]
    public class EcoTest
    {
        [TestMethod]
        public void TestGetByFen()
        {
            Eco eco = Eco.Get("rnbqkbnr/pppppppp/8/8/8/4P3/PPPP1PPP/RNBQKBNR b KQkq - 0 1");
            Assert.AreEqual("A00k", eco.Key);
            Assert.AreEqual("Van Kruijs", eco.Text);
            eco = Eco.Get("rnbqr1k1/pp3pbp/3p1np1/2pP4/4P3/2N2N2/PPQ1BPPP/R1B2RK1 b - - 7 10");
            Assert.AreEqual("A76", eco.Key);
            Assert.AreEqual("Benoni: Classical, Main Line, 10.Qc2", eco.Text);
        }
    }
}
