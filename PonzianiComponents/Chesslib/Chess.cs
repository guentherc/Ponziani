using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PonzianiComponents.Chesslib
{
#pragma warning disable CS1591
    /// <summary>
    /// Square of a standard chess board. Ponziani uses "Little-Endian Rank-File Mapping"
    /// (see https://chessprogramming.wikispaces.com/Square+Mapping+Considerations).
    /// </summary>
    public enum Square
    {

        A1, B1, C1, D1, E1, F1, G1, H1,
        A2, B2, C2, D2, E2, F2, G2, H2,
        A3, B3, C3, D3, E3, F3, G3, H3,
        A4, B4, C4, D4, E4, F4, G4, H4,
        A5, B5, C5, D5, E5, F5, G5, H5,
        A6, B6, C6, D6, E6, F6, G6, H6,
        A7, B7, C7, D7, E7, F7, G7, H7,
        A8, B8, C8, D8, E8, F8, G8, H8, OUTSIDE = 64
    }
    /// <summary>
    /// Enumeration for the different Pieces
    /// </summary>
    public enum Piece
    {
        WQUEEN, BQUEEN, WROOK, BROOK, WBISHOP, BBISHOP, WKNIGHT, BKNIGHT, WPAWN, BPAWN, WKING, BKING, BLANK
    }
    /// <summary>
    /// The Piece Types
    /// </summary>
    public enum PieceType
    {
        QUEEN, ROOK, BISHOP, KNIGHT, PAWN, KING, NONE
    }
    /// <summary>
    /// Side or Color
    /// </summary>
    public enum Side { WHITE, BLACK }

    /// <summary>
    /// Castling Options
    /// </summary>
    public enum CastleFlag
    {
        /// <summary>
        /// No castlings, neither white nor black possible
        /// </summary>
        NONE = 0,
        /// <summary>
        /// White king-side
        /// </summary>
        W0_0 = 1,
        /// <summary>
        /// White capturingQueen-side
        /// </summary>
        W0_0_0 = 2,
        /// <summary>
        /// Black king-side
        /// </summary>
        B0_0 = 4,
        /// <summary>
        /// Black capturingQueen side
        /// </summary>
        B0_0_0 = 8
    }

    /// <summary>
    /// Possible outcomes of a chess game
    /// </summary>
    public enum Result { 
        /// <summary>
        /// Game is still ongoing, resp. result is unknown
        /// </summary>
        OPEN, 
        WHITE_WINS, BLACK_WINS, DRAW, 
        /// <summary>
        /// Game has been abandoned (for example in engine matches by a crash
        /// </summary>
        ABANDONED }
    /// <summary>
    /// More detailed info about game result
    /// </summary>
    public enum ResultDetail
    {
        UNKNOWN, MATE, ABANDONED, TIME_FORFEIT, ILLEGAL_MOVE, THREE_FOLD_REPETITION, FIFTY_MOVES,
        NO_MATING_MATERIAL, ADJUDICATION_WIN, ADJUDICATION_DRAW, STALEMATE
    }

    /// <summary>
    /// The detailed result, containing the information about the game's outcome
    /// </summary>
    public struct DetailedResult
    {
        /// <summary>
        /// Creates a new Detailed result
        /// </summary>
        /// <param name="result">The result (white wins, black wins or draw)</param>
        /// <param name="detail">The result's detail giving the reason for the result</param>
        /// <param name="additionalInfo">additional info (like which illegal move was played)</param>
        public DetailedResult(Result result, ResultDetail detail = ResultDetail.UNKNOWN, object additionalInfo = null) { Result = result; Detail = detail; AdditionalInfo = additionalInfo; }
        /// <summary>
        /// The result (white wins, black wins or draw)
        /// </summary>
        public Result Result { private set; get; }
        /// <summary>
        /// The result's detail giving the reason for the result
        /// </summary>
        public ResultDetail Detail { private set; get; }
        /// <summary>
        /// Additional info (like which illegal move was played)
        /// </summary>
        public object AdditionalInfo { private set; get; }
        /// <summary>
        /// Creates a string representation, mimicing the result info from cutechess
        /// </summary>
        /// <returns>the string representation</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(PGN.ResultToString(Result));
            if (Result != Result.OPEN && Detail != ResultDetail.UNKNOWN)
            {
                string resultPhrase = ResultPhrase[(int)Detail];
                if (Result == Result.WHITE_WINS)
                {
                    resultPhrase = resultPhrase.Replace("<winner>", "White");
                    resultPhrase = resultPhrase.Replace("<looser>", "Black");
                }
                else if (Result == Result.BLACK_WINS)
                {
                    resultPhrase = resultPhrase.Replace("<winner>", "Black");
                    resultPhrase = resultPhrase.Replace("<looser>", "White");
                }
                if (AdditionalInfo != null) resultPhrase = resultPhrase.Replace("<info>", AdditionalInfo.ToString());
                sb.Append(resultPhrase);
            }
            return sb.ToString();
        }
        /// <summary>
        /// The termination tag value as used in PGN Termination tag
        /// </summary>
        public string Termination
        {
            get
            {
                if (Result == Result.OPEN) return "unterminated";
                else return PGNTerminationTerms[Detail];
            }
        }

        private static readonly string[] ResultPhrase = new string[11] { "", " {<winner> mates}", " {<looser> resigns}", " {<looser> loses on time}", " {<looser> makes an illegal move: <info>}",
                                                                  " {Draw by 3-fold repetition}", " {Draw by fifty moves rule}", " {Draw by insufficient mating material}",
                                                                  " {<winner> wins by adjudication}", " {Draw by adjudication}", " {Draw by stalemate}"};
        private static readonly Dictionary<ResultDetail, string> PGNTerminationTerms = new Dictionary<ResultDetail, string>() { { ResultDetail.ABANDONED, "abandoned" },
                { ResultDetail.ADJUDICATION_DRAW, "adjudication" }, { ResultDetail.ADJUDICATION_WIN, "adjudication" }, { ResultDetail.TIME_FORFEIT, "time forfeit" },
                { ResultDetail.ILLEGAL_MOVE, "rules infraction" }, { ResultDetail.MATE, "normal" }, { ResultDetail.STALEMATE, "normal" }, { ResultDetail.FIFTY_MOVES, "normal" },
                { ResultDetail.NO_MATING_MATERIAL, "normal" }, { ResultDetail.THREE_FOLD_REPETITION, "normal" }, { ResultDetail.UNKNOWN, null }};
    }

    /// <summary>
    /// This class contains static methods useful when dealing with chess
    /// </summary>
    public class Chess
    {
        /// <summary>
        /// Provides the type of a given piece
        /// </summary>
        /// <param name="piece">A chess piece</param>
        /// <returns>The type of this chess piece</returns>
        public static PieceType GetPieceType(Piece piece) { return (PieceType)((int)piece >> 1); }
        /// <summary>
        /// Parses a PieceType Character ('Q', 'R', 'B', 'N', 'K' or 'P')
        /// </summary>
        /// <param name="piecetypechar">The Piece type character</param>
        /// <returns>the piece type value</returns>
        public static PieceType ParsePieceTypeChar(char piecetypechar)
        {
            return PieceTypeMapping[piecetypechar];
        }
        /// <summary>
        /// Determines the color of a piecce
        /// </summary>
        /// <param name="piece">The piece</param>
        /// <returns>The color of the piece</returns>
        public static Side GetColor(Piece piece) { return (Side)(((int)piece) & 1); }
        /// <summary>
        /// Get's the string representation (e.g. "a1" or "d5") of a square
        /// </summary>
        /// <param name="square">The square</param>
        /// <returns>The square's string representation</returns>
        public static string SquareToString(Square square)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("abcdefgh"[(int)square & 7]).Append(((int)square >> 3) + 1);
            return sb.ToString();
        }
        /// <summary>
        /// Creates a Piece from <see cref="PieceType"/> and <see cref="Side"/>
        /// </summary>
        /// <param name="pt">Piece Type</param>
        /// <param name="side">Side</param>
        /// <returns>Piece</returns>
        public static Piece GetPiece(PieceType pt, Side side)
        {
            return (Piece)(2*(int)pt + (int)side);
        }
        /// <summary>
        /// List of supported languages 
        /// </summary>
        public static List<string> SupportedLanguages => PieceChars.Keys.Select(ci => ci.TwoLetterISOLanguageName).Distinct().ToList();

        private static readonly Dictionary<char, PieceType> PieceTypeMapping = new Dictionary<char, PieceType>() {
            { 'Q', PieceType.QUEEN}, { 'R', PieceType.ROOK},  { 'B', PieceType.BISHOP},
            { 'N', PieceType.KNIGHT},{ 'K', PieceType.KING},{ 'P', PieceType.PAWN}
        };

        internal static readonly Dictionary<CultureInfo, IChessPieceStringProvider> PieceChars = new Dictionary<CultureInfo, IChessPieceStringProvider>()
        {
            { CultureInfo.InvariantCulture, new CharPieceStringProvider("QRBNPK") },
            { new CultureInfo("en"), new CharPieceStringProvider("QRBNPK") },
            { new CultureInfo("de"), new CharPieceStringProvider("DTLSBK") },
            { new CultureInfo("fr"), new CharPieceStringProvider("DTFCPR") },
            { new CultureInfo("es"), new CharPieceStringProvider("DTACPR") },
            { new CultureInfo("it"), new CharPieceStringProvider("DTACPR") },
            { new CultureInfo("ru"), new StringArrayPieceStringProvider(new string[] {"Ф", "Л", "С", "К", "П", "Кр" }) }
        };
    }

    internal interface IChessPieceStringProvider
    {
        public string Get(PieceType pt, Side side = Side.WHITE);
        public string Get(Piece p);
    }

    internal class CharPieceStringProvider: IChessPieceStringProvider
    {
        string piecechars;

        public CharPieceStringProvider(string piecechars)
        {
            this.piecechars = piecechars;
        }

        public string Get(PieceType pt, Side side = Side.WHITE)
        {
            return side == Side.WHITE ? piecechars[(int)pt].ToString() : Get(Chess.GetPiece(pt, side)); 
        }

        public string Get(Piece p)
        {
            return ((int)p & 1) == 0 ? piecechars[(int)p / 2].ToString() : Char.ToLower(piecechars[(int)p / 2]).ToString();
        }
    }

    internal class StringArrayPieceStringProvider : IChessPieceStringProvider
    {
        string[] piecestrings;
        public StringArrayPieceStringProvider(string[] piecestrings)
        {
            this.piecestrings = piecestrings;
        }

        public string Get(PieceType pt, Side side = Side.WHITE)
        {
            return side == Side.WHITE ? piecestrings[(int)pt].ToString() : Get(Chess.GetPiece(pt, side));
        }

        public string Get(Piece p)
        {
            return ((int)p & 1) == 0 ? piecestrings[(int)p / 2] : piecestrings[(int)p / 2].ToLower();
        }
    }

    internal class FigurinePieceStringProvider : IChessPieceStringProvider
    {
        char[] upiece = { '\u2655', '\u265b', '\u2656', '\u265c', '\u2657', '\u265d', '\u2658', '\u265e', '\u2659', '\u265f', '\u2654', '\u265a', '\u0020' };

        public string Get(PieceType pt, Side side = Side.WHITE)
        {
            return upiece[2 * (int)pt + (int)side].ToString();
        }

        public string Get(Piece p)
        {
            return upiece[(int)p].ToString();
        }

        public static IChessPieceStringProvider Instance { get; private set; } = new FigurinePieceStringProvider();
    }
}
