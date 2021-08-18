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
    public class PositionTest
    {
        [TestMethod]
        public void TestGetMoves()
        {
            Assert.AreEqual(4865609, (new Position()).Perft(5));
            Assert.AreEqual(4085603, (new Position("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1")).Perft(4));
        }

        [TestMethod]
        public void TestPolyglotKey()
        {
            Assert.AreEqual(0xc3ce103f01d15e1dul, (new Position("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1")).PolyglotKey);
            Assert.AreEqual(0x5f625bebc9266ba2ul, (new Position("1r3r1k/2qR2pp/5p2/1pP1p3/1p2P1Q1/P6P/6P1/5RK1 b - - 1 26")).PolyglotKey);
            Assert.AreEqual(0x0cf2546d521f1fd8ul, (new Position("6k1/1p3p1p/2p2bp1/p7/4PP2/6P1/PP4KP/R1Br4 b - - 0 25")).PolyglotKey);
            Assert.AreEqual(0xaa3db808baa4172eul, (new Position("8/1p3K2/p4q2/2P3b1/6k1/8/PP6/8 w - - 9 51")).PolyglotKey);
            Assert.AreEqual(0x463b96181691fc9cul, (new Position()).PolyglotKey);
        }

        [TestMethod]
        public void TestFen()
        {
            foreach (string fen in fens)
            {
                Position pos = new Position(fen);
                Assert.AreEqual(fen, pos.FEN);
            }
        }

        [TestMethod]
        public void TestEPD()
        {
            foreach (string fen in fens)
            {
                Position pos = new Position(fen);
                Assert.IsTrue(fen.StartsWith(pos.EPD));
            }
        }

        [TestMethod]
        public void TestEPSquare()
        {
            foreach (string fen in fens)
            {
                Position pos = new Position(fen);
                Assert.AreEqual(Square.OUTSIDE, pos.EPSquare);
            }
            Assert.AreEqual(Square.D6, (new Position("rnbqkbnr/ppp1p2p/6p1/3pPp2/3P4/8/PPP2PPP/RNBQKBNR w KQkq d6 0 4")).EPSquare);
            {
                Position pos = new("rnbqkbnr/ppp1p2p/6p1/3pP3/1P1P1p2/8/P1P2PPP/RNBQKBNR w KQkq - 0 5");
                pos.ApplyMove("g2g4");
                Assert.AreEqual(Square.G3, pos.EPSquare);
            }
        }

        [TestMethod]
        public void TestSideToMove()
        {
            string[] s = new string[] { " w ", " b " };
            foreach (string fen in fens)
            {
                Position pos = new Position(fen);
                Assert.IsTrue(fen.IndexOf(s[(int)pos.SideToMove]) > 0);
            }
            {
                string movelist = "d2d4 e7e6 d1d3 f8d6 g1f3 d8a5 c1d2 a5h5 e2e4 b7b6 e4e5 c8a6 c2c4 d6c7 e5f6 g7f6 b1c3 c7d6 c3e4 e8e7 e4d6 e7d6 g2g4 h5g6 d2f4 d6e7 d3e3 g6c2 f1d3 c2c3 e1e2 d7d6 e3c1 c3c1 a1c1 b8d7 d3e4 e6e5 f4d2 d6d5 e4f5 e5e4 f3h4 h7h5 f5d7 e7d7 h1g1 d5c4 b3c4 b6b5 c4b5 a6b5 e2e1 h5g4 g1g4 a8g8 g4g8 h8g8 d2f4 g8g1 e1d2 g1c1 d2c1 b5c4 a2a4 c4e6 f2f3 e4f3 h4f3 e6h3 f4b8 a7a6 b8f4 h3f5 c1d1 d7e7 d1e1 e7d8 e1f1 d8e7 f1g1 e7d8 g1h1 d8e7 h1g2 e7d8 g2g1 f5d3 g1h1 d3e2 h1g2 e2c4 g2h2 c4b3 a4a5 b3c4 h2g3 d8d7 g3h3 d7e8 h3g4 e8e7 g4h4 e7d8 f3e1 d8c8 e1c2 c8d8 c2b4 d8d7 h4g4 c4e6 g4f3 e6c4 f3e4 d7e6 e4e3 c4b5 e3f3 b5c4 f4g3 f6f5 f3f4 c4b5 f4g5 f7f6 g5g6 b5f1 b4c6 f1b5 c6b4 b5e8 g6g7 e8b5 d4d5 e6e7 g3f4 b5c4 d5d6 e7e6 b4c6 e6d7 c6d4 c4d5 g7f6 d7e8 f6e5 d5c4 d4e6 c4e2 e6c5 e2b5 e5f5 e8d8 f4g5 d8c8 f5g4 b5c6 g4h3 c6e8 g5e7 e8b5 e7f6 b5c6 h3g4 c6d7 g4g5 d7e8 g5f4 e8c6 f4f5 c6b5 f5e6 b5c4 e6e7 c4b5 c5a6 c8b7 a6c5 b7a7 d6d7 b5d7 e7d7 a7a8 c5a6 a8a7 a6c7 a7b7 f6d4 b7b8";
                Position pos = new("rnbqkb1r/pp1ppppp/2p2n2/8/8/1P5P/P1PPPPP1/RNBQKBNR w KQkq - 0 1");
                Assert.AreEqual(Side.WHITE, pos.SideToMove);
                string[] mstring = movelist.Split(' ');
                Side side = pos.SideToMove;
                foreach (string m in mstring)
                {
                    pos.ApplyMove(m);
                    side = (Side)((int)side ^ 1);
                    Assert.AreEqual(side, pos.SideToMove);
                }
            }
        }

        [TestMethod]
        public void TestIsCheck()
        {
            string[] s = new string[] { " w ", " b " };
            foreach (string fen in fens)
            {
                //make null move
                string sfen = fen.Replace(s[0], s[1]);
                if (sfen == fen) sfen = fen.Replace(s[1], s[0]);
                Position pos = new Position(sfen);
                //if null move results in legal position, position must no be in check
                if (pos.CheckLegal(out string message))
                {
                    Assert.IsFalse((new Position(fen)).IsCheck);
                }
            }
            //Let's check some concrete examples
            foreach (string fen in fenChecked)
            {
                Assert.IsTrue((new Position(fen)).IsCheck);
            }

        }

        [TestMethod]
        public void TestIsDoubleCheck()
        {
            foreach (string fen in fens) Assert.IsFalse((new Position(fen)).IsDoubleCheck);
            foreach (string fen in fenChecked) Assert.IsFalse((new Position(fen)).IsDoubleCheck);
            foreach (string fen in fenDoubleChecked) Assert.IsTrue((new Position(fen)).IsDoubleCheck);
        }

        [TestMethod]
        public void TestIsMate()
        {
            foreach (string fen in fens) Assert.IsFalse((new Position(fen)).IsMate, $"{fen} is Mate!");
            foreach (string fen in fenChecked) Assert.IsFalse((new Position(fen)).IsMate, $"{fen} is Mate!");
            foreach (string fen in fenStalemate) Assert.IsFalse((new Position(fen)).IsMate, $"{fen} is Mate!");
            foreach (string fen in fenMate) Assert.IsTrue((new Position(fen)).IsMate, $"{fen} isn't Mate!");
        }

        [TestMethod]
        public void TestIsStaleMate()
        {
            foreach (string fen in fens) Assert.IsFalse((new Position(fen)).IsStalemate, $"{fen} is Stalemate!");
            foreach (string fen in fenChecked) Assert.IsFalse((new Position(fen)).IsStalemate, $"{fen} is Stalemate!");
            foreach (string fen in fenDoubleChecked) Assert.IsFalse((new Position(fen)).IsStalemate, $"{fen} is Stalemate!");
            foreach (string fen in fenMate) Assert.IsFalse((new Position(fen)).IsStalemate, $"{fen} is Stalemate!");
            foreach (string fen in fenStalemate) Assert.IsTrue((new Position(fen)).IsStalemate, $"{fen} isn't Stalemate!");
        }

        private static string[] fenStalemate = new string[10]
        {
            "8/p7/P7/8/8/2k5/2p5/2K5 w - - 2 60",
            "8/8/2KN4/7p/1P1k3P/6Q1/8/8 b - - 4 65",
            "8/8/8/8/8/7k/7p/7K w - - 2 64",
            "5K2/3r4/4q3/5k2/8/8/8/8 w - - 4 75",
            "8/8/6P1/8/8/1Q1K4/8/2k5 b - - 0 65",
            "1b6/8/8/8/8/7k/7p/7K w - - 6 87",
            "8/8/8/8/p5K1/k6p/7P/1Q6 b - - 0 61",
            "K7/P1k5/8/8/8/8/8/8 w - - 1 62",
            "8/1p6/6p1/p5P1/P6K/5q2/1b3k2/8 w - - 10 65",
            "r3k3/p7/Pb5p/1P6/5p2/2p3p1/6P1/7K w - - 0 43" };

        private static string[] fenMate = new string[10]
        {
            "8/7k/1r1p1p1P/p1p1p1p1/2P1P1P1/q2PQP2/R1n5/K2R4 w - - 2 37",
            "r3k2r/pb2bppp/1p2p3/8/3N1B2/3B4/PP2QPqP/R4RK1 w kq - 0 14",
            "3Q4/6b1/3Nbkpp/4pp2/2P5/1Pq1B3/P5PP/3R2K1 b - - 6 29",
            "r1b2B1k/pp2Np1p/3P1Qp1/2p1n3/8/3q3P/PP4PK/R7 b - - 1 22",
            "3r2k1/4bppp/4p3/4p1PP/Qp6/8/PP6/K1r2BR1 w - - 0 26",
            "8/8/8/3b4/6qK/5k2/8/8 w - - 9 77",
            "5Q1k/7p/p3p1p1/4Np2/3Pb3/4B2P/q4PP1/6K1 b - - 3 26",
            "5r2/1p4p1/p2p2k1/2pP3Q/q1Pn2P1/4B1P1/1P4K1/5R2 b - - 5 33",
            "r2r3k/pp5R/3P2Qb/q1p1p3/8/8/PP4PP/R5K1 b - - 4 27",
            "k1RQ1b1r/pp1N2p1/5p2/4r2p/4b3/8/PP3PPP/5RK1 b - - 0 24" };


        private static string[] fenDoubleChecked = new string[6]
        {
            "k1q5/1pp5/1N6/8/8/8/8/R5K1 b - - 1 1",
            "r3k2r/ppp2pp1/2np4/2B1p3/2B1P1N1/3P2n1/PPP2PP1/RN1Q1R1K w kq - 1 13",
            "rn1k1b1r/pp2pp1p/2p2p2/B7/8/5N2/qPP2PPP/2KR3R b - - 1 14",
            "4bk2/ppp3p1/2nB3p/2b5/2B3nq/2N5/PP4PP/4RR1K b - - 0 19",
            "3r2k1/pp5p/6p1/2Ppq3/4N3/4B2b/PP2Pr1K/R1Q1R2B w - - 1 27",
            "r6r/2qknP1p/p5p1/1p2pB2/8/2P1B3/P1PR2PP/5RK1 b - - 2 22" };

        private static string[] fenChecked = new string[10]
        {
            "r1b1k1nr/pp3ppp/1qn1p3/3pP3/1b1P4/N4N2/PP3PPP/R1BQKB1R w KQkq - 1 8",
            "r2q1rk1/pppbNppp/3p1n2/1P3P2/3N4/8/1PP3PP/R1BQ1RK1 b - - 0 15",
            "r1bqk2r/p1n1ppbp/1pp2Np1/2p1P3/8/5N2/PPPP1PPP/R1BQR1K1 b kq - 1 10",
            "rnbk2nr/1p4pp/p2bp3/5pB1/8/1N3NP1/PPP4P/R3KB1R b KQ - 2 11",
            "rnb1kb1r/pp2pppp/2p1q3/8/7Q/5N2/PPPP1PPP/R1B1KB1R w KQkq - 3 8",
            "r3qrk1/p6p/1pbbpp2/2pp4/3P1PQ1/2PBP3/PP1N3P/R3K2R b KQ - 0 17",
            "r1b2rk1/1pq2pp1/2p2nnp/p7/3NP3/P1N1B2P/1PQ1BPPb/R2R2K1 w - - 5 17",
            "1B6/5p2/p1k1p3/2PpQpP1/P7/1P2b3/K2q3P/8 w - - 18 53",
            "8/8/8/4p1p1/6P1/4kP2/1R1r2K1/8 w - - 1 53",
            "3RK3/5p1k/2q1bQpp/1p6/3B4/6P1/5P2/8 w - - 18 63"
        };

        private static string[] fens = new string[100] { "rn5Q/pp3kp1/2p5/2bpr2p/7q/2P2PN1/PPB2P2/R3R1K1 b - - 0 1",
            "8/6kp/6p1/8/4pp2/5P1P/2r1q1P1/5RK1 w - - 0 1",
            "3r2k1/5pp1/4pn1p/2b1N3/Q7/2P3NP/Bq3PP1/5K2 w - - 0 1",
            "6k1/pp6/3p1p2/8/2KPp3/1P2P3/P7/7q b - - 1 1",
            "5rk1/pp6/3p1p2/3p4/3P4/4P2p/PP1K1PqN/4R3 w - - 0 1",
            "1R3bk1/5p1p/q1p3p1/8/3Pp3/4P2P/5PP1/2Q3K1 w - - 0 1",
            "6k1/1p6/p4p2/2pp4/8/1K6/P7/8 b - - 0 1",
            "1R6/p4pkp/4p1p1/8/8/7P/q1r2PP1/5RK1 b - - 0 1",
            "r4r2/p1p1k1p1/1p1p3p/1Q3Pn1/8/4B3/P5P1/R4RK1 w - - 0 1",
            "7r/3rk2p/2Q2pb1/1B4p1/1P6/3P4/P1P2PPP/R5K1 b - - 0 2",
            "4r1k1/5ppp/1q1Pp3/1p1p4/1Qr2PP1/P4R2/1P4KP/R7 w - - 0 1",
            "6rk/pR4bp/6p1/8/4P3/2q1B1PP/4Q1K1/8 b - - 0 1",
            "7R/pkp5/bp6/3p1r2/3P4/N3RP2/PP6/1K6 b - - 0 1",
            "5r2/p4pkp/4p1p1/1R1b4/P4q2/3B4/5PPP/1R4K1 w - - 0 1",
            "8/8/3k4/8/1N1P3R/1P1K4/P5r1/8 b - - 0 1",
            "r4k2/1p6/p1p1QP2/8/2P5/5P2/PP6/2KR4 b - - 0 1",
            "3r2k1/5ppp/4p3/1pq5/4QPP1/P5K1/1P1p3P/3R4 w - - 0 1",
            "5k2/6p1/2B4p/2P5/P2K4/8/r5PP/8 b - - 0 1",
            "2r5/8/8/P2P4/kPN5/2K5/8/8 w - - 0 1",
            "1k6/1pp2p2/p7/3pP3/PP1P1B2/2P4p/8/6K1 b - - 0 2",
            "3R4/8/5kpp/8/5p2/4pP1P/2q5/5RK1 b - - 0 1",
            "r1b2rk1/p1q2pp1/2pb3p/n2nN3/3P4/4pPB1/PPP1B1PP/RN1Q1RK1 b - - 0 1",
            "r2q1rk1/3b1ppp/p3p3/3pPn2/3Q1P2/P1B2N2/1P4PP/R4RK1 w - - 0 1",
            "6k1/ppR4p/3p2p1/3q3n/4r3/8/PPP2QP1/6K1 w - - 0 3",
            "3k4/3P3p/5pp1/8/1bpR4/4P2P/3p1PP1/3K4 b - - 0 1",
            "7k/6p1/7p/1p6/P3B3/1P1K4/2P3PP/4r3 b - - 0 1",
            "3r4/p4k1p/3P4/1pp5/8/8/PP3KPP/3R4 b - - 0 1",
            "6k1/1b4pp/p7/8/3p4/1P5K/P4Q1P/3q4 b - - 1 1",
            "8/5p2/1R2p1k1/3r2p1/PP6/6K1/8/8 b - - 0 1",
            "r5k1/1R3pbp/q1p3p1/8/1Q1Pp3/4P3/2P2PPP/5RK1 w - - 0 1",
            "rn1q2nr/1p1kpQ2/p1p5/4P1N1/2P5/5P2/PP1B2p1/2KRR3 w - - 1 3",
            "1k4rr/1ppb1pq1/p1n1p2p/3pP3/3P4/2PBP3/PP3QPN/R1B2RK1 w - - 0 1",
            "5b2/ppk3p1/2p3N1/3p2rp/8/2P2P2/PPB2P1K/5R2 b - - 0 1",
            "r3r1k1/pp3pbp/6p1/3N4/2n5/4P1PP/PP3PK1/1RBR4 w - - 0 1",
            "8/pp1R4/2p5/4k1pp/5p2/2P1p2P/PP2K1P1/8 b - - 0 1",
            "1k5r/1pp2p2/p1n5/3pP3/3P4/2P3rp/PP5N/R1B2R1K b - - 0 1",
            "2r2r2/6kp/4Q3/1p2Pp1p/p7/2q5/P4PPP/4R1K1 w - - 0 1",
            "1rb2rk1/pp1p1p2/6pQ/q2p4/3P4/2N3PP/PP2PP2/R3K2R w KQ - 0 1",
            "1k4rr/1ppbqp2/p1nbpn1p/3p4/3P1P2/2PBPN2/PP1N1QPP/R1B2RK1 w - - 0 1",
            "6k1/5pp1/4p2p/7P/3r1PP1/P3K3/1P1p4/3R4 b - - 0 1",
            "8/4Qpk1/5p2/7p/5P1P/4P3/5PK1/8 b - - 0 1",
            "r4b1r/p2k1ppp/2n1pn2/8/2NP4/6P1/1P2PPBP/q1BQK2R b K - 0 1",
            "2Q5/P3kp2/5p1p/8/8/4PP2/r4PKP/8 w - - 0 1",
            "r3k1nr/ppp4p/2nq1p2/4p1p1/2B1P3/4BQ2/PPP2PPP/R4RK1 b kq - 0 1",
            "2kr4/pbpqp1b1/1pn3Q1/3p2n1/3P1N2/N1P2P2/PP1B4/1K1R3R b - - 0 1",
            "2r2rk1/p3bppp/q3p3/3n4/3P4/4P1P1/1P1N1PBP/2BQR1K1 w - - 0 1",
            "8/8/4K2p/6pk/8/5r2/8/4R3 b - - 0 1",
            "4rk2/ppq4r/2p1p1Qp/2b1P1p1/5p2/2N4P/PPP3PK/R4R2 b - - 0 1",
            "8/p1p3pp/3k4/1p2p3/4K3/7n/8/8 w - - 0 1",
            "8/8/7p/R1prpkp1/8/2K3P1/7P/8 w - - 0 1",
            "r6k/ppQ4p/7q/4p3/8/2P5/PP4PP/5RK1 b - - 0 1",
            "8/b7/p2p3k/1p2p3/1P1N3N/PB1p4/2P2n1K/5R2 w - - 0 1",
            "1rr3k1/pp3pq1/3p2p1/3p4/3P3Q/5NP1/PP2PP2/5RK1 w - - 0 1",
            "6R1/1ppk4/p1p2p1p/5p2/5b2/1P3n2/r4P2/3rBK2 w - - 0 1",
            "1r3k2/5ppp/p1p1p3/N2n4/1P1P4/P5P1/5P1P/2R3K1 b - - 0 1",
            "8/p1p1k1pp/8/1n2p3/8/P6P/6P1/6K1 w - - 0 1",
            "r1bq1rk1/ppp2ppp/2np1n2/8/2B5/2B2Q1P/PP2NPP1/3R1RK1 b - - 0 1",
            "2r2rk1/p4pp1/1p2pn1p/8/1P1R3P/3Q4/P4qP1/1B1R3K w - - 0 1",
            "2kr4/1pp5/p4pnp/3p1p2/1B3b2/1P6/P1r2P2/3R2K1 w - - 0 1",
            "8/p4p1p/3rkp2/8/8/4PQ2/P4PPP/6K1 b - - 0 1",
            "3r4/4k1p1/p1p2p1p/4p3/3nNP2/P3K1P1/1P5P/2R5 b - - 0 1",
            "4r1k1/5pp1/1p2pn1p/1P6/2Q4P/8/3q2PK/1B3R2 b - - 0 1",
            "8/4R1pp/3K4/8/8/3B1k1P/P7/8 b - - 0 1",
            "rnb1N3/3p2p1/p7/kp1Qp1N1/1b5P/4B3/PP3PP1/2R2K1R b - - 0 1",
            "8/6R1/8/8/5K1p/3B4/P6k/8 b - - 0 1",
            "r2q2k1/pp1n2pp/2p5/2bp2r1/8/2P2PN1/PPB2P1P/R2QR1K1 b - - 0 1",
            "2k1r3/1pp1r1pp/p1pb2n1/5pN1/3P1P2/1P6/P1P2P2/1RB2K2 w - - 0 1",
            "r3k2r/1b4pp/pq1pPb2/1p6/P1p2P2/2P5/1P2B1PP/RN1Q1R1K b kq - 0 1",
            "r6k/pp1r2pp/8/2b5/2B2p2/5RPP/PPK5/R7 b - - 1 2",
            "1r4k1/5pbp/2R2np1/q2pp3/8/P3P3/1P2QPPP/5RK1 w - - 0 1",
            "1r3rk1/p3bppp/2q1pn2/8/2NPP3/6P1/1PQB1PBP/2R3K1 w - - 0 1",
            "r1b2k2/ppp3pp/8/3np3/8/7P/PP2NPP1/2R3K1 w - - 0 1",
            "r2q1rk1/1p1n1ppp/2pNp3/3n4/p2P4/2P3N1/P1Q2PPP/R3R1K1 b - - 0 1",
            "r2q1rk1/1p1nbppp/8/p2NP2Q/P2BP3/1P6/1P4PP/3R1R1K b - - 0 1",
            "r2qr1k1/p4pbp/5np1/3pp3/NB6/P3P3/1P2QPPP/2R2RK1 w - - 0 1",
            "4r1k1/5ppp/2p5/1p3P2/1P1Q2P1/2P1r2P/2B2RK1/8 w - - 0 1",
            "r4rk1/1bqn1ppp/pb2p3/3pP3/8/2B2N2/PP2NPPP/R2Q1RK1 b - - 0 1",
            "8/pp2k3/2prp2p/6p1/5p2/2R4P/PPP3P1/5R1K b - - 0 1",
            "r3k2r/pp1b1ppp/2n1pn2/1B6/3P4/5N2/PP1N1PPP/R4RK1 b kq - 0 1",
            "r4rk1/p4ppp/1p2p3/2n2q2/1PBRn3/2N1P3/P1Q2PPP/R5K1 b - - 0 1",
            "r6k/3r2pp/p7/6b1/4R3/7P/PP6/1K2R3 b - - 0 1",
            "r1bkr3/pp5Q/8/3p4/5q2/2P3R1/P5PP/R5K1 w - - 0 1",
            "3q1rk1/6pp/2b1P3/1p6/2p2P2/N7/4B1PP/4Q1K1 w - - 0 2",
            "1k1r3r/pq3p1p/3bpp2/Q1p5/8/1P2PN2/P4PPP/R4RK1 b - - 0 1",
            "r1bq3k/bpp4n/p1np1r1Q/4pp2/1PB1P3/P1NP1N1P/2P2PP1/R4RK1 w - - 0 1",
            "r2qr1kb/pp2p2p/3p1npB/3P4/6b1/2N2P2/PPPQBP2/2KR3R b - - 0 1",
            "3r2k1/2R3bp/3pq1p1/4p3/1Q2B3/5P2/6P1/5K2 w - - 0 1",
            "3r2k1/pp1Pr1bp/4p1p1/q3p3/B7/2N5/PPPQ1PP1/2K4R b - - 0 1",
            "7R/8/3k2p1/7p/2K2P2/1r6/8/8 b - - 0 1",
            "7Q/1b1p4/2q1p3/1p4N1/8/3k4/6PP/5RK1 w - - 0 1",
            "2rq2k1/1b1p4/4pR2/1p4P1/6p1/3Q3N/6PP/4R1K1 w - - 1 3",
            "N1b4k/b7/p1np1n2/1p2p1r1/1P2p2N/PB1P3P/2P2PP1/R4RK1 w - - 0 1",
            "2R1rk2/6pp/3p1p2/4p3/P7/8/4BPPP/6K1 w - - 0 1",
            "rn1q1rk1/4b2p/2p1b1p1/1p2Pp2/1PpP1P2/2N1B3/2Q1N1PP/1R3RK1 b - - 0 1",
            "4B3/1p4k1/p7/3P2Pp/8/2P5/PP6/1K4R1 b - - 0 3",
            "1r4k1/6bN/3rp1p1/p3p3/8/1B2K3/1PP2PP1/7R b - - 0 1",
            "6R1/3B4/1p5k/p7/2P5/1P6/P1K5/8 b - - 0 1",
            "4r1k1/p1q3p1/b1pn3p/5r2/5Q2/1PN2B2/P1P2BPP/R5K1 w - - 0 1",
            "1r3rk1/1p3ppp/p1p2q2/3p1P2/5P2/1PP2Q1P/P1B3P1/5RK1 b - - 0 1",
            "3r2k1/pp6/2bNp2p/P1K1P1p1/1P6/8/7P/8 w - - 0 1" };
    }

}
