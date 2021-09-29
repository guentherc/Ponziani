using Microsoft.VisualStudio.TestTools.UnitTesting;
using PonzianiComponents.Chesslib;
using System.Collections.Generic;

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

        [TestMethod]
        public void TestGetGame()
        {
            Eco eco = Eco.Get("rnbqkb1r/1p2pppp/p2p1n2/8/3NP3/2N5/PPP2PPP/R1BQKB1R w KQkq - 0 6");
            Game game = Eco.GetGame(eco);
            Assert.IsNotNull(game);
            Assert.AreEqual(10, game.Moves.Count);
            string uci = "e2e4 c7c5 g1f3 d7d6 d2d4 c5d4 f3d4 g8f6 b1c3 a7a6";
            string[] m = uci.Split(' ');
            for (int i = 0; i < m.Length; ++i)
            {
                Assert.IsTrue(((Move)game.Moves[i]).Equals(new Move(m[i])));
            }
        }

        [TestMethod]
        public void TestSubvariants()
        {
            Eco eco = Eco.Get("rnbqkbnr/pppppppp/8/8/6P1/8/PPPPPP1P/RNBQKBNR b KQkq g3 0 1");
            List<Eco> subvariants = Eco.Subvariants(eco);
            Assert.AreEqual(11, subvariants.Count);
        }

        [TestMethod]
        public void TestKeyvariants()
        {
            var keyvariants = Eco.Keyvariants();
            Assert.AreEqual(5859, keyvariants.Count);
            var bvariants = Eco.Keyvariants('B');
            Assert.AreEqual(1582, bvariants.Count);
        }

        [TestMethod]
        public void TestGet()
        {
            var ecos = Eco.Get("A02", "A04");
            Assert.AreEqual(110, ecos.Count);
        }

    }
}
