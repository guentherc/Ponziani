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
    public class PGNTest
    {
        [TestMethod]
        public void ParseSingleLichessGame()
        {
            var games = PGN.Parse(Data.PGN_LICHESS_SINGLE_GAME);
            Assert.AreEqual(1, games.Count);
            Assert.AreEqual(43, games[0].Moves.Count);
            Assert.AreEqual(Result.WHITE_WINS, games[0].Result);
            Assert.AreEqual(ResultDetail.MATE, games[0].ResultDetail);
        }

        [TestMethod]
        public void ParseLichessLiveGame()
        {
            var games = PGN.Parse(Data.PGN_LICHESS_LIVE_GAME);
            Assert.AreEqual(1, games.Count);
            Assert.AreEqual(31, games[0].Moves.Count);
            Assert.AreEqual(Result.OPEN, games[0].Result);
        }
    }
}
