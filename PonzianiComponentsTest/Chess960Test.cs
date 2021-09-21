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
    public class Chess960Test
    {
        [TestMethod]
        public void TestPositionConstructor()
        {
            foreach (string fen in Data.SFEN_FRC_WCC_STARTPOS)
            {
                Position pos = new(fen);
                Assert.IsTrue(pos.Chess960);
            }
            foreach (string fen in Data.XFEN_ARENA_STARTPOS)
            {
                Position pos = new(fen);
                Assert.IsTrue(pos.Chess960);
            }
        }

        [TestMethod]
        public void TestGetMoves()
        {
            Assert.AreEqual(30, (new Position("nrbkn2r/pppp1p1p/4p1p1/3P4/6P1/P3B3/P1P1PP1P/qR1KNBQR w KQkq - 0 1")).Perft(1, true));
            foreach (var entry in Data.Chess960Perft2)
            {
                Assert.AreEqual(entry.Value, (new Position(entry.Key)).Perft(3), entry.Key, entry.Key);
            }
        }

        [TestMethod]
        public void TestXFen()
        {
            string xfen = "rn2k1r1/ppp1pp1p/3p2p1/5bn1/P7/2N2B2/1PPPPP2/2BNK1RR w Gkq - 4 11";
            Position pos = new Position(xfen);
            Assert.AreEqual(xfen, pos.FEN);
        }

        [TestMethod]
        public void TestPGN()
        {
            var games = PGN.Parse(Data.PGN_FRC_WCC, true);
            Assert.AreEqual(78, games.Count);
            foreach (var game in games)
            {
                Position pos = new Position(game.StartPosition);
                foreach (ExtendedMove m in game.Moves)
                {
                    pos.ApplyMove(m);
                    Position cpos = new Position(m.Comment.Trim());
                    Assert.AreEqual(cpos.FEN, pos.FEN, $"{game.Round} {game.White} - {game.Black}");
                    Assert.AreEqual(cpos.SFEN, pos.SFEN, $"{game.Round} {game.White} - {game.Black}");
                }
            }
        }
    }
}
