using Microsoft.VisualStudio.TestTools.UnitTesting;
using PonzianiComponents.Chesslib;

namespace PonzianiComponentsTest
{
    [TestClass]
    public class MoveTest
    {
        [TestMethod]
        public void TestEquals()
        {
            Move m1 = new("e2e4");
            Move m2 = new(Square.E2, Square.E4);
            Assert.AreEqual(m1, m2);
            Move m3 = new Move(12, 28);
            Assert.AreEqual(m1, m3);
            Assert.AreEqual(m2, m3);
            m1 = new("d7e8Q");
            m2 = new(Square.D7, Square.E8, PieceType.QUEEN);
            Assert.AreEqual(m1, m2);
            m3 = new Move(51, 60, PieceType.QUEEN);
            Assert.AreEqual(m1, m3);
            Assert.AreEqual(m2, m3);
            string movelist = "d2d4 e7e6 d1d3 f8d6 g1f3 d8a5 c1d2 a5h5 e2e4 b7b6 e4e5 c8a6 c2c4 d6c7 e5f6 g7f6 b1c3 c7d6 c3e4 e8e7 e4d6 e7d6 g2g4 h5g6 d2f4 d6e7 d3e3 g6c2 f1d3 c2c3 e1e2 d7d6 e3c1 c3c1 a1c1 b8d7 d3e4 e6e5 f4d2 d6d5 e4f5 e5e4 f3h4 h7h5 f5d7 e7d7 h1g1 d5c4 b3c4 b6b5 c4b5 a6b5 e2e1 h5g4 g1g4 a8g8 g4g8 h8g8 d2f4 g8g1 e1d2 g1c1 d2c1 b5c4 a2a4 c4e6 f2f3 e4f3 h4f3 e6h3 f4b8 a7a6 b8f4 h3f5 c1d1 d7e7 d1e1 e7d8 e1f1 d8e7 f1g1 e7d8 g1h1 d8e7 h1g2 e7d8 g2g1 f5d3 g1h1 d3e2 h1g2 e2c4 g2h2 c4b3 a4a5 b3c4 h2g3 d8d7 g3h3 d7e8 h3g4 e8e7 g4h4 e7d8 f3e1 d8c8 e1c2 c8d8 c2b4 d8d7 h4g4 c4e6 g4f3 e6c4 f3e4 d7e6 e4e3 c4b5 e3f3 b5c4 f4g3 f6f5 f3f4 c4b5 f4g5 f7f6 g5g6 b5f1 b4c6 f1b5 c6b4 b5e8 g6g7 e8b5 d4d5 e6e7 g3f4 b5c4 d5d6 e7e6 b4c6 e6d7 c6d4 c4d5 g7f6 d7e8 f6e5 d5c4 d4e6 c4e2 e6c5 e2b5 e5f5 e8d8 f4g5 d8c8 f5g4 b5c6 g4h3 c6e8 g5e7 e8b5 e7f6 b5c6 h3g4 c6d7 g4g5 d7e8 g5f4 e8c6 f4f5 c6b5 f5e6 b5c4 e6e7 c4b5 c5a6 c8b7 a6c5 b7a7 d6d7 b5d7 e7d7 a7a8 c5a6 a8a7 a6c7 a7b7 f6d4 b7b8";
            Position pos = new("rnbqkb1r/pp1ppppp/2p2n2/8/8/1P5P/P1PPPPP1/RNBQKBNR w KQkq - 0 1");
            string[] mstring = movelist.Split(' ');
            foreach (string m in mstring)
            {
                Move move = new(m);
                Assert.IsTrue(pos.GetMoves().FindIndex(mv => mv.Equals(move)) >= 0);
                pos.ApplyMove(move);
            }
        }

        [TestMethod]
        public void TestGetHashCode()
        {
            Move m1 = new("e2e4");
            Move m2 = new(Square.E2, Square.E4);
            Assert.AreEqual(m1.GetHashCode(), m2.GetHashCode());
            Move m3 = new Move(12, 28);
            Assert.AreEqual(m1.GetHashCode(), m3.GetHashCode());
            Assert.AreEqual(m2.GetHashCode(), m3.GetHashCode());
            m1 = new("d7e8q");
            m2 = new(Square.D7, Square.E8, PieceType.QUEEN);
            Assert.AreEqual(m1.GetHashCode(), m2.GetHashCode());
            m3 = new Move(51, 60, PieceType.QUEEN);
            Assert.AreEqual(m1.GetHashCode(), m3.GetHashCode());
            Assert.AreEqual(m2.GetHashCode(), m3.GetHashCode());
            string movelist = "d2d4 e7e6 d1d3 f8d6 g1f3 d8a5 c1d2 a5h5 e2e4 b7b6 e4e5 c8a6 c2c4 d6c7 e5f6 g7f6 b1c3 c7d6 c3e4 e8e7 e4d6 e7d6 g2g4 h5g6 d2f4 d6e7 d3e3 g6c2 f1d3 c2c3 e1e2 d7d6 e3c1 c3c1 a1c1 b8d7 d3e4 e6e5 f4d2 d6d5 e4f5 e5e4 f3h4 h7h5 f5d7 e7d7 h1g1 d5c4 b3c4 b6b5 c4b5 a6b5 e2e1 h5g4 g1g4 a8g8 g4g8 h8g8 d2f4 g8g1 e1d2 g1c1 d2c1 b5c4 a2a4 c4e6 f2f3 e4f3 h4f3 e6h3 f4b8 a7a6 b8f4 h3f5 c1d1 d7e7 d1e1 e7d8 e1f1 d8e7 f1g1 e7d8 g1h1 d8e7 h1g2 e7d8 g2g1 f5d3 g1h1 d3e2 h1g2 e2c4 g2h2 c4b3 a4a5 b3c4 h2g3 d8d7 g3h3 d7e8 h3g4 e8e7 g4h4 e7d8 f3e1 d8c8 e1c2 c8d8 c2b4 d8d7 h4g4 c4e6 g4f3 e6c4 f3e4 d7e6 e4e3 c4b5 e3f3 b5c4 f4g3 f6f5 f3f4 c4b5 f4g5 f7f6 g5g6 b5f1 b4c6 f1b5 c6b4 b5e8 g6g7 e8b5 d4d5 e6e7 g3f4 b5c4 d5d6 e7e6 b4c6 e6d7 c6d4 c4d5 g7f6 d7e8 f6e5 d5c4 d4e6 c4e2 e6c5 e2b5 e5f5 e8d8 f4g5 d8c8 f5g4 b5c6 g4h3 c6e8 g5e7 e8b5 e7f6 b5c6 h3g4 c6d7 g4g5 d7e8 g5f4 e8c6 f4f5 c6b5 f5e6 b5c4 e6e7 c4b5 c5a6 c8b7 a6c5 b7a7 d6d7 b5d7 e7d7 a7a8 c5a6 a8a7 a6c7 a7b7 f6d4 b7b8";
            Position pos = new("rnbqkb1r/pp1ppppp/2p2n2/8/8/1P5P/P1PPPPP1/RNBQKBNR w KQkq - 0 1");
            string[] mstring = movelist.Split(' ');
            foreach (string m in mstring)
            {
                Move move = new(m);
                Assert.IsTrue(pos.GetMoves().FindIndex(mv => mv.GetHashCode() == move.GetHashCode()) >= 0);
                pos.ApplyMove(move);
            }
        }

        [TestMethod]
        public void TestToUCIString()
        {
            string movelist = "d2d4 e7e6 d1d3 f8d6 g1f3 d8a5 c1d2 a5h5 e2e4 b7b6 e4e5 c8a6 c2c4 d6c7 e5f6 g7f6 b1c3 c7d6 c3e4 e8e7 e4d6 e7d6 g2g4 h5g6 d2f4 d6e7 d3e3 g6c2 f1d3 c2c3 e1e2 d7d6 e3c1 c3c1 a1c1 b8d7 d3e4 e6e5 f4d2 d6d5 e4f5 e5e4 f3h4 h7h5 f5d7 e7d7 h1g1 d5c4 b3c4 b6b5 c4b5 a6b5 e2e1 h5g4 g1g4 a8g8 g4g8 h8g8 d2f4 g8g1 e1d2 g1c1 d2c1 b5c4 a2a4 c4e6 f2f3 e4f3 h4f3 e6h3 f4b8 a7a6 b8f4 h3f5 c1d1 d7e7 d1e1 e7d8 e1f1 d8e7 f1g1 e7d8 g1h1 d8e7 h1g2 e7d8 g2g1 f5d3 g1h1 d3e2 h1g2 e2c4 g2h2 c4b3 a4a5 b3c4 h2g3 d8d7 g3h3 d7e8 h3g4 e8e7 g4h4 e7d8 f3e1 d8c8 e1c2 c8d8 c2b4 d8d7 h4g4 c4e6 g4f3 e6c4 f3e4 d7e6 e4e3 c4b5 e3f3 b5c4 f4g3 f6f5 f3f4 c4b5 f4g5 f7f6 g5g6 b5f1 b4c6 f1b5 c6b4 b5e8 g6g7 e8b5 d4d5 e6e7 g3f4 b5c4 d5d6 e7e6 b4c6 e6d7 c6d4 c4d5 g7f6 d7e8 f6e5 d5c4 d4e6 c4e2 e6c5 e2b5 e5f5 e8d8 f4g5 d8c8 f5g4 b5c6 g4h3 c6e8 g5e7 e8b5 e7f6 b5c6 h3g4 c6d7 g4g5 d7e8 g5f4 e8c6 f4f5 c6b5 f5e6 b5c4 e6e7 c4b5 c5a6 c8b7 a6c5 b7a7 d6d7 b5d7 e7d7 a7a8 c5a6 a8a7 a6c7 a7b7 f6d4 b7b8";
            string[] mstring = movelist.Split(' ');
            foreach (string m in mstring)
            {
                Move move = new(m);
                Assert.AreEqual(m, move.ToUCIString());
            }
        }
    }
}
