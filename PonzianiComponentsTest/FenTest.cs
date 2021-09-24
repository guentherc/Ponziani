using Microsoft.VisualBasic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PonzianiComponents.Chesslib;
using System;
using System.Linq;
using System.Text;

namespace PonzianiComponentsTest
{
    [TestClass]
    public class FenTest
    {
        [TestMethod]
        public void TestPieceChar()
        {
            Piece[] pieces = new Piece[13] {Piece.BQUEEN, Piece.BBISHOP, Piece.WKING, Piece.WKNIGHT, Piece.WQUEEN, Piece.WBISHOP, Piece.WPAWN, Piece.BPAWN, Piece.WROOK, Piece.BROOK,
                                           Piece.BKNIGHT, Piece.BKING, Piece.BLANK };
            char[] pstr = new char[13] { 'q', 'b', 'K', 'N', 'Q', 'B', 'P', 'p', 'R', 'r', 'n', 'k', ' ' };
            for (int i = 0; i < pieces.Length; ++i) Assert.AreEqual(pstr[i], Fen.PieceChar(pieces[i]));
        }

        [TestMethod]
        public void TestParsePieceChar()
        {
            Piece[] pieces = new Piece[13] {Piece.BQUEEN, Piece.BBISHOP, Piece.WKING, Piece.WKNIGHT, Piece.WQUEEN, Piece.WBISHOP, Piece.WPAWN, Piece.BPAWN, Piece.WROOK, Piece.BROOK,
                                           Piece.BKNIGHT, Piece.BKING, Piece.BLANK };
            char[] pstr = new char[13] { 'q', 'b', 'K', 'N', 'Q', 'B', 'P', 'p', 'R', 'r', 'n', 'k', ' ' };
            for (int i = 0; i < pieces.Length; ++i) Assert.AreEqual(pieces[i], Fen.ParsePieceChar(pstr[i]));
        }

        [TestMethod]
        public void TestParseSquare()
        {
            string[] squares = new string[64] { "a1", "b1", "c1", "d1", "e1", "f1", "g1", "h1",
                                                "a2", "b2", "c2", "d2", "e2", "f2", "g2", "h2",
                                                "a3", "b3", "c3", "d3", "e3", "f3", "g3", "h3",
                                                "a4", "b4", "c4", "d4", "e4", "f4", "g4", "h4",
                                                "a5", "b5", "c5", "d5", "e5", "f5", "g5", "h5",
                                                "a6", "b6", "c6", "d6", "e6", "f6", "g6", "h6",
                                                "a7", "b7", "c7", "d7", "e7", "f7", "g7", "h7",
                                                "a8", "b8", "c8", "d8", "e8", "f8", "g8", "h8" };
            for (Square s = Square.A1; s <= Square.H8; ++s) Assert.AreEqual(s, Fen.ParseSquare(squares[(int)s]));
        }

        static string[] fens = new string[]
        {
                "rn1qkbnr/pp3ppp/4p3/3p4/Q5b1/8/PP1PPPBP/RNB1K1NR b KQkq - 1 6",
                "8/8/8/8/6K1/Bpb3P1/2k2P2/8 b - - 1 73",
                "8/8/4b3/2k2p2/p1P5/1rN1p1P1/K1R4P/8 b - - 5 51",
                "8/3n3p/R1kr2p1/5p2/8/2P4P/P1K2P2/R7 b - - 2 30",
                "2k1r2r/pp1n1bp1/2pb4/3p2P1/3P4/2N2p1B/PPP1N3/R3K2R w KQ - 0 21"
        };

        [TestMethod]
        public void TestFenPartFromBoard()
        {
            foreach (string fen in fens)
            {
                var pa = Fen.GetPieceArray(fen);
                string part = Fen.FenPartFromBoard(pa);
                Assert.IsTrue(fen.StartsWith(part));
            }
        }

        [TestMethod]
        public void TestGetPieceArray()
        {

            foreach (string fen in fens)
            {
                var pa = Fen.GetPieceArray(fen);
                var ps = ArrayString(fen);
                for (int i = 0; i < 64; ++i)
                {
                    Assert.AreEqual(ps[i], pa[i]);
                }
            }
        }

        private string ArrayString(string fen)
        {
            string part = fen.Substring(0, fen.IndexOf(' '));
            string[] ranks = part.Split('/');
            Assert.AreEqual(8, ranks.Length);
            StringBuilder sb = new StringBuilder();
            foreach (string r in ranks.Reverse())
            {
                foreach (char c in r)
                {
                    if (Char.IsDigit(c)) sb.Append(Strings.Space((int)c - (int)'0'));
                    else sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}
