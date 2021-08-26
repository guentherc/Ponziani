using Microsoft.AspNetCore.Components;
using PonzianiComponents.Chesslib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PonzianiComponents
{
    /// <summary>
    /// <para>Blazor Component for adding an interactive chessboard to a Blazor application. It was build as a Blazor version of <see href="https://chessboardjs.com/index.html">chessboard.js</see>
    /// and allows to use this functionality without the need to interop with javascript.</para>
    /// <para> There are however quite some differences:</para>
    /// <ul>
    /// <li>No animations</li>
    /// <li>It has chess knowledge and therefore provides legal move check out of the box</li>
    /// </ul>
    /// </summary>
    public partial class Chessboard
    {
        /// <summary>
        /// Id of the rendered HTML element
        /// </summary>
        [Parameter]
        public string Id { get; set; } = "board";
        /// <summary>
        /// Position displayed on the board in <see href="https://en.wikipedia.org/wiki/Forsyth%E2%80%93Edwards_Notation">Forsyth-Edwards-Notation</see>
        /// </summary>
        [Parameter]
        public string Fen
        {
            get
            {
                if (!SetupMode) _fen = position.FEN;
                return _fen;
            }
            set
            {
                _fen = value;
                if (!SetupMode) position = new Position(value);
            }
        }
        /// <summary>
        /// SetupMode can be used to allow the user to set up a new position. If SetupMode is active pieces can be freely moved, added and removed. 
        /// There are spare pieces shown outside the board. 
        /// If SetupMode isn't active only legal moves can be played.
        /// Default is false.
        /// </summary>
        [Parameter]
        public bool SetupMode { get { return _setupMode; } set { if (value) SwitchToSetupMode(); else ExitSetupMode(); } }
        /// <summary>
        /// The size (in pixels) of the board (default 400)
        /// </summary>
        [Parameter]
        public int Size { get; set; } = 400;
        /// <summary>
        /// If true, the file and rank labels are displayed
        /// </summary>
        [Parameter]
        public bool ShowCoordinates { get; set; } = true;
        /// <summary>
        /// If true the board's orientation is reversed
        /// </summary>
        [Parameter]
        public bool Rotate { get; set; } = false;
        /// <summary>
        /// Allowing to replace the current piece images with a different set. The piece images must follow the naming convention
        /// {color}{piecetype}.png, so wB.png for white Bishop, bP.png for black Pawn
        /// </summary>
        [Parameter]
        public string PathPieceImages { get; set; } = "_content/PonzianiComponents/img/chesspieces/wikipedia/";
        /// <summary>
        /// If true, the squares of the last applied Move get highlighted
        /// </summary>
        [Parameter]
        public bool HighlightLastAppliedMove { get; set; } = false;
        /// <summary>
        /// Is called whenever the user played a move on the board. Only active if <see cref="SetupMode"/> is false.
        /// </summary>
        [Parameter]
        public EventCallback<MovePlayedInfo> OnMovePlayed { get; set; }
        /// <summary>
        /// Is called whenever the user changed the board's setup be moving, adding or removing a piece in <see cref="SetupMode"/> 
        /// </summary>
        [Parameter]
        public EventCallback<SetupChangedInfo> OnSetupChanged { get; set; }
        /// <summary>
        /// Applies a move to the current board
        /// </summary>
        /// <param name="move">The move which shall be applied</param>
        /// <returns>true, if move could be applied</returns>
        public bool ApplyMove(Move move)
        {
            if (SetupMode) return false;
            highlightedSquares = 0;
            if (position.GetMoves().FindIndex(m => m.From == move.From && m.To == move.To) < 0) return false;
            position.ApplyMove(move);
            _fen = position.FEN;
            if (HighlightLastAppliedMove)
            {
                SetHighlightSquare(move.From);
                SetHighlightSquare(move.To);
            }
            InvokeAsync(() => StateHasChanged());
            return true;
        }
        /// <summary>
        /// Enter setup mode (API-call to set parameter <see cref="SetupMode"/> to true)
        /// </summary>
        public void SwitchToSetupMode()
        {
            _setupMode = true;
        }
        /// <summary>
        /// Leave setup mode (API-call to set parameter <see cref="SetupMode"/> to false)
        /// </summary>
        public void ExitSetupMode()
        {
            if (_setupMode)
            {
                addSI = new AdditionalSetupInfo(_fen);
                Position pos = new(_fen);
                string message;
                if (pos.CheckLegal(out message))
                {
                    SetupErrorMessage = null;
                    clsShowModalSetup = " show-modal";
                }
                else
                    SetupErrorMessage = message;
            }
        }
        /// <summary>
        /// Method to mark/highlight a square
        /// </summary>
        /// <param name="s">The square, which shall be highlighted</param>
        /// <param name="highlight">if true, square will be highlighted, if false highlight is switched off</param>
        public void SetHighlightSquare(Square s, bool highlight = true)
        {
            if (highlight) highlightedSquares |= 1ul << (int)s;
            else highlightedSquares &= ~(1ul << (int)s);
        }
        /// <summary>
        /// Removes all square highlights which have been sett by <see cref="SetHighlightSquare(Square, bool)"/>
        /// </summary>
        public void ClearHighlighting() => highlightedSquares = 0;
        /// <summary>
        /// Checks if a square is highlighted (marked) or not
        /// </summary>
        /// <param name="s">Square</param>
        /// <returns>true if highlighted</returns>
        public bool IsHighlighted(Square s) => (highlightedSquares & (1ul << (int)s)) != 0;
        /// <summary>
        /// Size (in pixels) of one square
        /// </summary>
        public int SquareSize => (Size - 4) / 8;
        /// <summary>
        /// API call to set the position
        /// </summary>
        /// <param name="fen">The new position in FEN-Notation</param>
        public void SetFen(string fen)
        {
            Fen = fen;
        }

        private string SetupErrorMessage { set; get; } = null;

        private async Task CloseSetupDialogAsync()
        {
            clsShowModalSetup = "";
            Position pos = new(addSI.Fen);
            string message;
            if (!pos.CheckLegal(out message))
            {
                SetupErrorMessage = message;
                return;
            }
            SetupChangedInfo sci = new SetupChangedInfo();
            sci.OldFen = Fen;
            Fen = addSI.Fen;
            sci.NewFen = Fen;
            position = pos;
            await OnSetupChanged.InvokeAsync(sci);
            _setupMode = false;
        }

        private bool _setupMode = false;
        private AdditionalSetupInfo addSI { set; get; } = new AdditionalSetupInfo(Chesslib.Fen.INITIAL_POSITION);
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
        private string _fen = Chesslib.Fen.INITIAL_POSITION;
        private string clsShowModalPromo { set; get; }
        private string clsShowModalSetup { set; get; } = "";
        private UInt64 highlightedSquares = 0ul;

        private string[] pieceImages = new string[] { "wQ", "bQ", "wR", "bR", "wB", "bB", "wN", "bN", "wP", "bP", "wK", "bK", "" };
        private string GetPieceImage(char pieceChar)
        {
            Piece p = Chesslib.Fen.ParsePieceChar(pieceChar);
            return pieceImages[(int)p];
        }

        private string GetPieceImageSource(char pieceChar) => $"{PathPieceImages}{GetPieceImage(pieceChar)}.png";

        private string SquareId(Square square) => $"square-{Chesslib.Chess.SquareToString(square)}";
        private string PromoDialogId => $"{Id}-promodialog";

        private string IsDraggable(Square square)
        {
            return SetupMode || (Side)((int)position.GetPiece(square) & 1) == position.SideToMove ? "true" : "false";
        }

        private List<Move> legalMoves = new List<Move>();

        private async Task HandleSetupSubmitAsync()
        {
            await CloseSetupDialogAsync();
        }

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
            Fen = $"{Chesslib.Fen.FenPartFromBoard(board)} w - - 0 1";
            sci.NewFen = Fen;
            await OnSetupChanged.InvokeAsync(sci);
            SetupErrorMessage = null;
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
                    _fen = mpi.NewFen;
                    await OnMovePlayed.InvokeAsync(mpi);
                    draggedEnterPieceSquare = Square.OUTSIDE;
                    draggedPieceSquare = Square.OUTSIDE;
                    promoPiece = PieceType.NONE;
                    clsShowModalPromo = "";
                }
                else
                {
                    index = moves.FindIndex(m => m.From == draggedPieceSquare && m.To == draggedEnterPieceSquare);
                    if (index >= 0) //Maybe promotion?
                    {
                        clsShowModalPromo = " show-modal";
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
    /// <summary>
    /// Callback information provided by <see cref="Chessboard.OnSetupChanged"/>
    /// </summary>
    public class SetupChangedInfo
    {
        /// <summary>
        /// Position after the last user interaction in <see href="https://en.wikipedia.org/wiki/Forsyth%E2%80%93Edwards_Notation">Forsyth-Edwards-Notation</see>
        /// <remark>During setup the positions are usually not legal, therefore don't expect that the value represents a legal position</remark>
        /// </summary>
        public string NewFen { set; get; }
        /// <summary>
        /// Position before the last user interaction in <see href="https://en.wikipedia.org/wiki/Forsyth%E2%80%93Edwards_Notation">Forsyth-Edwards-Notation</see>
        /// <remark>During setup the positions are usually not legal, therefore don't expect that the value represents a legal position</remark>
        /// </summary>
        public string OldFen { set; get; }
    }
    /// <summary>
    /// Callback information provided by <see cref="Chessboard.OnMovePlayed"/>
    /// </summary>
    public class MovePlayedInfo
    {
        /// <summary>
        /// Id of the board, where the move was played (needed in multiboard scenarios)
        /// </summary>
        public string BoardId { set; get; }
        /// <summary>
        /// The move which was played
        /// </summary>
        public Move Move { set; get; }
        /// <summary>
        /// Position after the move in <see href="https://en.wikipedia.org/wiki/Forsyth%E2%80%93Edwards_Notation">Forsyth-Edwards-Notation</see>
        /// </summary>
        public string NewFen { set; get; }
        /// <summary>
        /// Position before the move in <see href="https://en.wikipedia.org/wiki/Forsyth%E2%80%93Edwards_Notation">Forsyth-Edwards-Notation</see>
        /// </summary>
        public string OldFen { set; get; }
        /// <summary>
        /// Move in SAN (<see href="https://en.wikipedia.org/wiki/Algebraic_notation_(chess)"> Standard Algebraic Notation</see>)
        /// </summary>
        public string San { set; get; }
    }

    internal class AdditionalSetupInfo
    {
        public AdditionalSetupInfo(string fen)
        {
            _fen = fen.Trim();
            pboard = Chesslib.Fen.GetPieceArray(_fen);
            string[] token = _fen.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (token.Length < 2) return;
            Side = token[1];
            if (token.Length < 3) return;
            if (token[2] != "-")
            {
                CastlingWhiteKingside = token[2].Contains('K');
                CastlingWhiteQueenside = token[2].Contains('Q');
                CastlingBlackKingside = token[2].Contains('k');
                CastlingBlackQueenside = token[2].Contains('q');
            }
            if (token.Length < 4) return;
            EnPassantSquare = token[3];
            if (token.Length > 4) DrawPlyCount = int.Parse(token[4]);
            if (token.Length > 5) MoveNumber = int.Parse(token[5]);

        }

        public string Fen { get
            {
                StringBuilder fen = new StringBuilder();
                string[] token = _fen.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                fen.Append(token[0]).Append(' ').Append(Side);
                fen.Append(' ').Append(castlingString()).Append(' ');
                fen.Append(EnPassantSquare);
                fen.Append(' ').Append(DrawPlyCount).Append(' ').Append(MoveNumber);
                return fen.ToString();
            } }

        private string _fen;
        private char[] pboard;

        public string Side { set; get; } = "w";
        public bool CastlingWhiteKingside { set; get; } = false;
        public bool CastlingWhiteQueenside { set; get; } = false;
        public bool CastlingBlackKingside { set; get; } = false;
        public bool CastlingBlackQueenside { set; get; } = false;
        public bool CastlingWhiteKingsidePossible => pboard[(int)Square.E1] == Chesslib.Fen.PIECE_CHAR_WKING && pboard[(int)Square.H1] == Chesslib.Fen.PIECE_CHAR_WROOK;
        public bool CastlingWhiteQueensidePossible => pboard[(int)Square.E1] == Chesslib.Fen.PIECE_CHAR_WKING && pboard[(int)Square.A1] == Chesslib.Fen.PIECE_CHAR_WROOK;
        public bool CastlingBlackKingsidePossible => pboard[(int)Square.E8] == Chesslib.Fen.PIECE_CHAR_BKING && pboard[(int)Square.H8] == Chesslib.Fen.PIECE_CHAR_BROOK;
        public bool CastlingBlackQueensidePossible => pboard[(int)Square.E8] == Chesslib.Fen.PIECE_CHAR_BKING && pboard[(int)Square.A8] == Chesslib.Fen.PIECE_CHAR_BROOK;
        public string EnPassantSquare { get; set; } = "-";
        public int DrawPlyCount { get; set; } = 0;
        public int MoveNumber { get; set; } = 1;

        private string castlingString()
        {
            string cs = "";
            if (CastlingWhiteKingside) cs += 'K';
            if (CastlingWhiteQueenside) cs += 'Q';
            if (CastlingBlackKingside) cs += 'k';
            if (CastlingBlackQueenside) cs += 'q';
            if (cs.Length == 0) cs = "-";
            return cs;
        }

        public List<string> EnPassantSquares()
        {
            List<string> result = new List<string>();
            result.Add("-");
            for (int s = (int)Square.A4; s <= (int)Square.H4; ++s)
            {
                if (pboard[s] != Chesslib.Fen.PIECE_CHAR_BPAWN || pboard[s-8] != Chesslib.Fen.PIECE_CHAR_NONE) continue;
                if (s > (int)Square.A4 && pboard[s-1] == Chesslib.Fen.PIECE_CHAR_WPAWN)
                {
                    result.Add(Chess.SquareToString((Square)(s - 8)));
                    continue;
                }
                if (pboard[s] != Chesslib.Fen.PIECE_CHAR_BPAWN) continue;
                if (s < (int)Square.H4 && pboard[s + 1] == Chesslib.Fen.PIECE_CHAR_WPAWN)
                {
                    result.Add(Chess.SquareToString((Square)(s - 8)));
                    continue;
                }
            }
            for (int s = (int)Square.A5; s <= (int)Square.H5; ++s)
            {
                if (pboard[s] != Chesslib.Fen.PIECE_CHAR_WPAWN || pboard[s + 8] != Chesslib.Fen.PIECE_CHAR_NONE) continue;
                if (s > (int)Square.A5 && pboard[s - 1] == Chesslib.Fen.PIECE_CHAR_BPAWN)
                {
                    result.Add(Chess.SquareToString((Square)(s+8)));
                    continue;
                }
                if (pboard[s] != Chesslib.Fen.PIECE_CHAR_WPAWN) continue;
                if (s < (int)Square.H5 && pboard[s + 1] == Chesslib.Fen.PIECE_CHAR_BPAWN)
                {
                    result.Add(Chess.SquareToString((Square)(s+8)));
                    continue;
                }
            }
            return result;
        }
    }
}
