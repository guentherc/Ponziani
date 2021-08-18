using Microsoft.VisualStudio.TestTools.UnitTesting;
using PonzianiComponents.Chesslib;

namespace PonzianiComponentsTest
{
    [TestClass]
    public class ChessTest
    {
        [TestMethod]
        public void TestGetPieceType()
        {
            Piece[] pieces = new Piece[13] {Piece.BQUEEN, Piece.BBISHOP, Piece.WKING, Piece.WKNIGHT, Piece.WQUEEN, Piece.WBISHOP, Piece.WPAWN, Piece.BPAWN, Piece.WROOK, Piece.BROOK,
                                           Piece.BKNIGHT, Piece.BKING, Piece.BLANK };
            PieceType[] pt = new PieceType[13] {PieceType.QUEEN, PieceType.BISHOP, PieceType.KING, PieceType.KNIGHT, PieceType.QUEEN, PieceType.BISHOP, PieceType.PAWN,
                                                PieceType.PAWN, PieceType.ROOK, PieceType.ROOK, PieceType.KNIGHT, PieceType.KING, PieceType.NONE };
            for (int i = 0; i < pieces.Length; ++i) Assert.AreEqual(pt[i], Chess.GetPieceType(pieces[i]));
        }

        [TestMethod]
        public void TestParsePieceTypeChar()
        {
            Assert.AreEqual(PieceType.QUEEN, Chess.ParsePieceTypeChar('Q'));
            Assert.AreEqual(PieceType.ROOK, Chess.ParsePieceTypeChar('R'));
            Assert.AreEqual(PieceType.BISHOP, Chess.ParsePieceTypeChar('B'));
            Assert.AreEqual(PieceType.KNIGHT, Chess.ParsePieceTypeChar('N'));
            Assert.AreEqual(PieceType.PAWN, Chess.ParsePieceTypeChar('P'));
            Assert.AreEqual(PieceType.KING, Chess.ParsePieceTypeChar('K'));
        }

        [TestMethod]
        public void TestGetColor()
        {
            Piece[] pieces = new Piece[12] {Piece.BQUEEN, Piece.BBISHOP, Piece.WKING, Piece.WKNIGHT, Piece.WQUEEN, Piece.WBISHOP, Piece.WPAWN, Piece.BPAWN, Piece.WROOK, Piece.BROOK,
                                           Piece.BKNIGHT, Piece.BKING };
            Side[] sides = new Side[12] { Side.BLACK, Side.BLACK, Side.WHITE, Side.WHITE, Side.WHITE, Side.WHITE, Side.WHITE, Side.BLACK, Side.WHITE, Side.BLACK,
                                          Side.BLACK, Side.BLACK };
            for (int i = 0; i < pieces.Length; ++i) Assert.AreEqual(sides[i], Chess.GetColor(pieces[i]));
        }

        [TestMethod]
        public void SquareToString()
        {
            string[] squares = new string[64] { "a1", "b1", "c1", "d1", "e1", "f1", "g1", "h1",
                                                "a2", "b2", "c2", "d2", "e2", "f2", "g2", "h2",
                                                "a3", "b3", "c3", "d3", "e3", "f3", "g3", "h3",
                                                "a4", "b4", "c4", "d4", "e4", "f4", "g4", "h4",
                                                "a5", "b5", "c5", "d5", "e5", "f5", "g5", "h5",
                                                "a6", "b6", "c6", "d6", "e6", "f6", "g6", "h6",
                                                "a7", "b7", "c7", "d7", "e7", "f7", "g7", "h7",
                                                "a8", "b8", "c8", "d8", "e8", "f8", "g8", "h8" };
            for (Square s = Square.A1; s <= Square.H8; ++s) Assert.AreEqual(squares[(int)s], Chess.SquareToString(s));
        }

    }
}
