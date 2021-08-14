using Microsoft.AspNetCore.Components;
using PonzianiComponents.Chesslib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PonzianiComponents
{
    public partial class Chessboard
    {
        [Parameter]
        public string Id { get; set; } = "board";
        [Parameter]
        public string Fen
        {
            get
            {
                if (!SetupMode) setupFen = position.FEN;
                return setupFen;
            }
            set
            {
                setupFen = value;
                if (!SetupMode) position = new Position(value);
            }
        }
        [Parameter]
        public bool SetupMode { get; set; } = false;
        [Parameter]
        public int Size { get; set; } = 400;
        [Parameter]
        public bool ShowCoordinates { get; set; } = true;
        [Parameter]
        public bool Rotate { get; set; } = false;
        [Parameter]
        public string PathPieceImages { get; set; } = "_content/PonzianiComponents/img/chesspieces/wikipedia/";
        [Parameter]
        public bool HighlightLastAppliedMove { get; set; } = false;
        [Parameter]
        public EventCallback<MovePlayedInfo> OnMovePlayed { get; set; }
        [Parameter]
        public EventCallback<SetupChangedInfo> OnSetupChanged { get; set; }

        public bool ApplyMove(Move move)
        {
            if (SetupMode) return false;
            highlightedSquares = 0;
            if (position.GetMoves().FindIndex(m => m.From == move.From && m.To == move.To) < 0) return false;
            position.ApplyMove(move);
            if (HighlightLastAppliedMove)
            {
                SetHighlightSquare(move.From);
                SetHighlightSquare(move.To);
            }
            return true;
        }

        public void ClearHighlighting() => highlightedSquares = 0;

        public void SetHighlightSquare(Square s, bool highlight = true)
        {
            if (highlight) highlightedSquares |= 1ul << (int)s;
            else highlightedSquares &= ~(1ul << (int)s);
        }

        public bool IsHighlighted(Square s) => (highlightedSquares & (1ul << (int)s)) != 0;

        public int SquareSize => (Size - 4) / 8;

        private string SquareStyle => $"width: {SquareSize}px; height: {SquareSize}px";
        private string BoardStyle => $"width: {8 * SquareSize + 4}px";
        private int RankStart => Rotate ? 0 : 7;
        private int RankEnd => Rotate ? 7 : 0;
        private int RankStep => Rotate ? 1 : -1;
        private int FileStart => Rotate ? 7 : 0;
        private int FileStep => Rotate ? -1 : 1;

        private Position position = new Position();
        private Piece draggedPiece = Piece.BLANK;
        private Square draggedPieceSquare = Square.OUTSIDE;
        private Square draggedEnterPieceSquare = Square.OUTSIDE;
        private PieceType promoPiece = PieceType.NONE;
        private string setupFen = Chesslib.Fen.INITIAL_POSITION;
        private string clsShowModal { set; get; }
        private UInt64 highlightedSquares = 0ul;

        private string[] pieceImages = new string[] { "wQ", "bQ", "wR", "bR", "wB", "bB", "wN", "bN", "wP", "bP", "wK", "bK", "" };
        private string GetPieceImage(char pieceChar)
        {
            Piece p = Chesslib.Fen.ParsePieceChar(pieceChar);
            return pieceImages[(int)p];
        }

        private string GetPieceImageSource(char pieceChar) => $"{PathPieceImages}{GetPieceImage(pieceChar)}.png";

        private string SquareId(Square square) => $"square-{Chesslib.Chess.SquareToString(square)}";

        private string IsDraggable(Square square)
        {
            return SetupMode || (Side)((int)position.GetPiece(square) & 1) == position.SideToMove ? "true" : "false";
        }

        private List<Move> legalMoves = new List<Move>();

        private async Task HandleDropAsync()
        {
            if (SetupMode)
                await HandleDropSetupMode();
            else
                await HandleDropStandard();
        }

        private async Task HandleDropSetupMode()
        {
            var board = Chesslib.Fen.GetPieceArray(Fen);
            if (draggedPiece == Piece.BLANK && draggedPieceSquare != Square.OUTSIDE && draggedEnterPieceSquare != Square.OUTSIDE && draggedEnterPieceSquare != draggedPieceSquare)
            {
                //Piece moved within board
                board[(int)draggedEnterPieceSquare] = board[(int)draggedPieceSquare];
                board[(int)draggedPieceSquare] = Chesslib.Fen.PIECE_CHAR_NONE;

            }
            else if (draggedPieceSquare != Square.OUTSIDE && draggedEnterPieceSquare == Square.OUTSIDE)
            {
                board[(int)draggedPieceSquare] = Chesslib.Fen.PIECE_CHAR_NONE;
            }
            else if (draggedPiece != Piece.BLANK && draggedEnterPieceSquare != Square.OUTSIDE)
            {
                board[(int)draggedEnterPieceSquare] = Chesslib.Fen.PieceChar(draggedPiece);
            }
            draggedEnterPieceSquare = Square.OUTSIDE;
            draggedPieceSquare = Square.OUTSIDE;
            draggedPiece = Piece.BLANK;
            SetupChangedInfo sci = new();
            sci.OldFen = Fen;
            Fen = $"{Chesslib.Fen.FenPartFromBoard(board)} - - 0 1";
            sci.NewFen = Fen;
            await OnSetupChanged.InvokeAsync(sci);
        }

        private async Task HandleDropStandard()
        {
            draggedPiece = Piece.BLANK;
            legalMoves.Clear();
            if (draggedPieceSquare != draggedEnterPieceSquare)
            {
                //Check if move is valid
                var moves = position.GetMoves();
                int index = moves.FindIndex(m => m.From == draggedPieceSquare && m.To == draggedEnterPieceSquare && m.PromoteTo == promoPiece);
                if (index >= 0)
                {
                    highlightedSquares = 0;
                    MovePlayedInfo mpi = new MovePlayedInfo();
                    mpi.BoardId = Id;
                    mpi.Move = moves[index];
                    mpi.OldFen = position.FEN;
                    mpi.San = position.ToSAN(mpi.Move);
                    position.ApplyMove(moves[index]);
                    mpi.NewFen = position.FEN;
                    if (HighlightLastAppliedMove)
                    {
                        SetHighlightSquare(mpi.Move.From);
                        SetHighlightSquare(mpi.Move.To);
                    }
                    await OnMovePlayed.InvokeAsync(mpi);
                    draggedEnterPieceSquare = Square.OUTSIDE;
                    draggedPieceSquare = Square.OUTSIDE;
                    promoPiece = PieceType.NONE;
                    clsShowModal = "";
                }
                else
                {
                    index = moves.FindIndex(m => m.From == draggedPieceSquare && m.To == draggedEnterPieceSquare);
                    if (index >= 0) //Maybe promotion?
                    {
                        clsShowModal = " show-modal";
                    }
                }
            }
        }

        private void HandleDragEnter(Square square)
        {
            draggedEnterPieceSquare = square;
        }

        private void HandleDragEnterOutside()
        {
            draggedEnterPieceSquare = Square.OUTSIDE;
        }

        private void HandleDragStart(Square square)
        {
            draggedPieceSquare = square;
            draggedPiece = Piece.BLANK;
            legalMoves = position.GetMoves().Where(m => m.From == draggedPieceSquare).ToList();
        }

        private void HandleDragStartSparePieces(char pieceChar)
        {
            draggedPiece = Chesslib.Fen.ParsePieceChar(pieceChar);
            draggedPieceSquare = Square.OUTSIDE;
            legalMoves.Clear();
        }

        private async Task SetPromoPieceAsync(EventArgs eventArgs, PieceType pt)
        {
            promoPiece = pt;
            await HandleDropAsync();
        }

        private string SquareClass(int rank, int file)
        {
            Square sq = (Square)(8 * rank + file);
            string hc = IsHighlighted(sq) ? " pzHighlightSquare" : "";
            if (legalMoves.FindIndex(m => m.To == sq) < 0)
                return (IsDark(rank, file) ? "pzSquare pzDarkSquare" : "pzSquare pzLightSquare") + hc;
            else
                return (IsDark(rank, file) ? "pzSquare pzDarkSquareGrey" : "pzSquare pzLightSquareGrey") + hc;
        }

        private static string SquareName(int rank, int file)
        {
            return $"{FileChars[file]}{rank}";
        }

        private static bool IsDark(int rank, int file)
        {
            return ((rank & 1) == 0) == ((file & 1) == 0);
        }
        private static string FileChars = "abcdefgh";
    }

    public class SetupChangedInfo
    {
        public string NewFen { set; get; }
        public string OldFen { set; get; }
    }

    public class MovePlayedInfo
    {
        public string BoardId { set; get; }
        public Move Move { set; get; }
        public string NewFen { set; get; }
        public string OldFen { set; get; }
        public string San { set; get; }
    }
}
