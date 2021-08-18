using AngleSharp.Dom;
using Bunit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PonzianiComponents;
using PonzianiComponents.Chesslib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PonzianiComponentsTest
{
    public partial class ChessboardTest: BunitTestContext
    {
        public string GetId(IRenderedComponent<Chessboard> cb)
        {
            var cboard = cb.Nodes.Where(n => n is IElement && ((IElement)n).ClassList.Contains("pzChessboard"));
            Assert.AreEqual(1, cboard.Count());
            if (cboard.Count() == 1) return ((IElement)cboard.First()).Id; else return null;
        }

        public IRefreshableElementCollection<IElement>  GetSquareDivs(IRenderedComponent<Chessboard> cb) => cb.FindAll("div.pzSquare");

        public string GetBoardPartFromComponent(IRenderedComponent<Chessboard> cb)
        {
            var pieceImages = cb.FindAll("div.pzSquare > img.pzPieceImage");
            char[] board = new char[64];
            Array.Fill(board, Fen.PIECE_CHAR_NONE);
            foreach (var pieceImage in pieceImages)
            {
                string imgSrc = ((IElement)pieceImage).GetAttribute("src");
                string squareId = ((IElement)pieceImage.Parent).Id;
                int len = imgSrc.Length;
                string ps = imgSrc.Substring(len - 6, 2);
                string ss = squareId.Substring(squareId.Length - 2);
                char pieceChar = Char.ToLower(ps[0]) == 'w' ? Char.ToUpper(ps[1]) : Char.ToLower(ps[1]);
                int square = (int)Fen.ParseSquare(ss);
                board[square] = pieceChar;
            }
            return Fen.FenPartFromBoard(board);
        }
    }
}
