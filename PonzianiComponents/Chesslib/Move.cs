using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
                    if (regex == regexLichessComment) {
                        Clock = TimeSpan.Parse(m.Groups[1].Value);
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
                    else if (regex == regexGrahamComment)
                    {
                        Evaluation = Int32.Parse(m.Groups[1].Value);
                        Depth = Int32.Parse(m.Groups[2].Value);
                        try
                        {
                            UsedThinkTime = TimeSpan.Parse(m.Groups[3].Value);
                        }
                        catch (Exception)
                        {
                            string secondTry = m.Groups[3].Value.Replace(" ", string.Empty);
                            try
                            {
                                UsedThinkTime = TimeSpan.Parse(m.Groups[3].Value);
                            }
                            catch (Exception)
                            {

                            }
                        }
                    }
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

        static readonly Regex regexCutechessComment = new Regex(@"((?:\+|-)[\d\.]+)/(\d+)\s([\d\.]+)s");
        static readonly Regex regexGrahamComment = new Regex(@"\[\%eval (\-?\d+),(\d+)\]\s?\[\%emt (\d?\d\:\s?\d\d\:\d\d)\]");
        static readonly Regex regexTCECComment = new Regex(@"((\w+)=([^,]+),\s?)+");
        static readonly Regex regexLichessComment = new Regex(@"\[\%clk (\d?\d\:\s?\d\d\:\d\d)\]");

        static List<Regex> commentRegexList = new List<Regex>() { regexLichessComment, regexCutechessComment, regexGrahamComment, regexTCECComment };
    }
}
