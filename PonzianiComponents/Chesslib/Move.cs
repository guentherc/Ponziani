using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PonzianiComponents.Chesslib
{
    /// <summary>
    /// Representation of a Chess Move
    /// </summary>
    public class Move
    {
        /// <summary>
        /// King-side castle white
        /// </summary>
        public static Move W0_0 = new Move("e1g1");
        /// <summary>
        /// Queen-side castle white
        /// </summary>
        public static Move W0_0_0 = new Move("e1c1");
        /// <summary>
        /// King-side castle black
        /// </summary>
        public static Move B0_0 = new Move("e8g8");
        /// <summary>
        /// Queen-side castle black
        /// </summary>
        public static Move B0_0_0 = new Move("e8c8");
        /// <summary>
        /// Null move
        /// </summary>
        public static Move NULL = new Move(Square.A1, Square.A1);
        /// <summary>
        /// From square of the Move
        /// </summary>
        public Square From { set; get; }
        /// <summary>
        /// To square of the Move
        /// </summary>
        public Square To { set; get; }
        /// <summary>
        /// If the move is a promotion move the piece type to which the pawn get's promoted
        /// </summary>
        public PieceType PromoteTo { set; get; } = PieceType.NONE;

        /// <summary>
        /// Creates a Move from it's notation as used in UCI protocol
        /// </summary>
        /// <param name="uciString">Move in UCI notation</param>
        public Move(string uciString)
        {
            try
            {
                From = (Square)Enum.Parse(typeof(Square), uciString.Substring(0, 2).ToUpper());
                To = (Square)Enum.Parse(typeof(Square), uciString.Substring(2, 2).ToUpper());
                if (uciString.Length > 4)
                {
                    PromoteTo = PieceTypeByChar[uciString[4]];
                }
            }
            catch (Exception)
            {
                From = Square.A1;
                To = Square.A1;
                PromoteTo = PieceType.NONE;
            }
        }
        /// <summary>
        /// Creates a Move
        /// </summary>
        /// <param name="from">Index of from square (0..63)</param>
        /// <param name="to">Index of to square (0..63)</param>
        /// <param name="promoteTo">Promotion piece type</param>
        public Move(int from, int to, PieceType promoteTo = PieceType.NONE)
        {
            From = (Square)from;
            To = (Square)to;
            PromoteTo = promoteTo;
        }

        /// <summary>
        /// Creates a Move
        /// </summary>
        /// <param name="from">From Square</param>
        /// <param name="to">To Square</param>
        /// <param name="promoteTo">Promotion piece type</param>
        public Move(Square from, Square to, PieceType promoteTo = PieceType.NONE)
        {
            From = from;
            To = to;
            PromoteTo = promoteTo;
        }
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            Move move = (Move)obj;
            return move.From == From && move.To == To && move.PromoteTo == PromoteTo;
        }
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return (int)From + (((int)To) << 6) + (((int)PromoteTo) << 12);
        }
        /// <summary>
        /// Converts the move to a string using the notation needed for UCI engine communication. E.g. "e2e4" or "b7b8q"
        /// </summary>
        /// <returns>The move in UCI notation</returns>
        public string ToUCIString()
        {
            if (PromoteTo == PieceType.NONE)
                return Chess.SquareToString(From) + Chess.SquareToString(To);
            else return Chess.SquareToString(From) + Chess.SquareToString(To) + "qrbn"[(int)PromoteTo];
        }

        private static Dictionary<char, PieceType> PieceTypeByChar = new Dictionary<char, PieceType>()
                             { { 'q', PieceType.QUEEN }, { 'r', PieceType.ROOK }, { 'b', PieceType.BISHOP }, { 'n', PieceType.KNIGHT }, { 'p', PieceType.PAWN}, { 'k', PieceType.KING },
                               { 'Q', PieceType.QUEEN }, { 'R', PieceType.ROOK }, { 'B', PieceType.BISHOP }, { 'N', PieceType.KNIGHT }, { 'P', PieceType.PAWN}, { 'K', PieceType.KING }};


    }
    /// <summary>
    /// Represents a move with additional information attached to it
    /// </summary>
    public class ExtendedMove : Move
    {
        /// <summary>
        /// Creates a Move from it's notation as used in UCI protocol
        /// </summary>
        /// <param name="uciString">Move in UCI notation</param>
        public ExtendedMove(string uciString) : base(uciString) { }
        /// <summary>
        /// Creates a Move
        /// </summary>
        /// <param name="from">Index of from square (0..63)</param>
        /// <param name="to">Index of to square (0..63)</param>
        /// <param name="promoteTo">Promotion piece type</param>
        public ExtendedMove(int from, int to, PieceType promoteTo = PieceType.NONE) : base(from, to, promoteTo) { }
        /// <summary>
        /// Creates an extended move from a simple move
        /// </summary>
        /// <param name="move"></param>
        public ExtendedMove(Move move) : base(move.From, move.To, move.PromoteTo) { }
        /// <summary>
        /// Think time the player used for making this move
        /// </summary>
        public TimeSpan UsedThinkTime { set; get; } = TimeSpan.Zero;
        /// <summary>
        /// The time the chess clock shows after the move
        /// </summary>
        public TimeSpan Clock { set; get; } = TimeSpan.Zero;
        /// <summary>
        /// Engine evaluation in centipawns
        /// </summary>
        public int Evaluation { set; get; } = 0;
        /// <summary>
        /// Engine search depth
        /// </summary>
        public int Depth { set; get; } = 0;
        /// <summary>
        /// Move was taken from an opening book
        /// </summary>
        public bool IsBookMove { set; get; } = false;
        /// <summary>
        /// Move was taken from tablebase
        /// </summary>
        public bool IsTablebaseMove { set; get; } = false;
        /// <summary>
        /// Comment attached to move
        /// </summary>
        public string Comment { set; get; } = null;
        /// <summary>
        /// The Side which played the move
        /// </summary>
        public Side SideToMove { set; get; } = Side.WHITE;
        /// <summary>
        /// Variations
        /// </summary>
        public List<List<ExtendedMove>> Variations { set; get; }

        internal UndoInfo UndoInfo { set; get; }

        //Tries to get infos from Comment
        public void ParseComment()
        {
            if (Comment == null || Comment.Trim().Length == 0) return;
            int indx = 0;
            foreach (Regex regex in commentRegexList)
            {
                Match m = regex.Match(Comment);
                if (m.Success)
                {
                    if (regex == regexLichessComment)
                    {
                        for (int i = 0; i < m.Groups[1].Captures.Count; ++i)
                        {
                            string key = m.Groups[1].Captures[i].Value;
                            string value = m.Groups[2].Captures[i].Value.Trim();
                            if (key == "clk") Clock = TimeSpan.Parse(value);
                            else if (key == "eval")
                            {
                                var token = value.Split(',');
                                Evaluation = token[0].IndexOf('.') >= 0 ? (int)(100 * Double.Parse(token[0], CultureInfo.InvariantCulture)) : int.Parse(token[0]);
                                if (token.Length > 1) Depth = Int32.Parse(token[1]);
                            }
                            else if (key == "emt")
                            {
                                if (value.IndexOf(':') > 0) UsedThinkTime = TimeSpan.Parse(value);
                                else UsedThinkTime = TimeSpan.FromSeconds(Double.Parse(value, CultureInfo.InvariantCulture));
                            }
                        }
                    }
                    else if (regex == regexTCECComment)
                    {
                        for (int i = 0; i < m.Groups[2].Captures.Count; ++i)
                        {
                            string key = m.Groups[2].Captures[i].Value;
                            if (key == "d") Depth = Int32.Parse(m.Groups[3].Captures[i].Value);
                            else if (key == "mt") UsedThinkTime = TimeSpan.FromMilliseconds(Int32.Parse(m.Groups[3].Captures[i].Value));
                            else if (key == "tl") Clock = TimeSpan.FromMilliseconds(Int32.Parse(m.Groups[3].Captures[i].Value));
                        }
                    }
                    else if (regex == regexCutechessComment)
                    {
                        Evaluation = (int)(100 * Double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture));
                        Depth = Int32.Parse(m.Groups[2].Value);
                        UsedThinkTime = TimeSpan.FromSeconds(Double.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture));
                    }
                    Comment = (Comment.Substring(0, m.Index) + Comment.Substring(m.Index + m.Length)).Trim();
                    break;
                }
                indx++;
            }
            if (indx > 0 && indx < commentRegexList.Count)
            {
                Regex tmp = commentRegexList[0];
                commentRegexList[0] = commentRegexList[indx];
                commentRegexList[indx] = tmp;
            }
        }

        static readonly Regex regexCutechessComment = new Regex(@"((?:\+|-)?[\d\.]+)/(\d+)[\s\r\n]+([\d\.]+)s", RegexOptions.Compiled);
        static readonly Regex regexTCECComment = new Regex(@"((\w+)=([^,]+),\s?)+", RegexOptions.Compiled);
        static readonly Regex regexLichessComment = new Regex(@"(?:\[\%(\w+)([^\]]+)\]\s?)+", RegexOptions.Compiled);

        static List<Regex> commentRegexList = new List<Regex>() { regexLichessComment, regexCutechessComment, regexTCECComment };
    }

    internal struct UndoInfo
    {
        private Int32 data;

        public UndoInfo(int drawPlyCount, Piece capturedPiece, Square epSquare, int castles, bool isPromotion = false) : this()
        {
            data = (drawPlyCount & 0xFF) + (((int)capturedPiece) << 8) + (((int)epSquare) << 16) + ((castles) << 24);
            if (isPromotion) data |= 0x1000;
        }

        public int DrawPlyCount => (int)(data & 0xFF);
        public Piece CapturedPiece => (Piece)((data >> 8) & 0xF);
        public Square EPSquare => (Square)((data >> 16) & 0xFF);
        public int Castles => (int)((data >> 24) & 0xFF);
        public bool IsPromotion => (data & 0x1000) != 0;
    }
}
