using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PonzianiComponents.Chesslib
{
    public class Game : ICloneable
    {
        /// <summary>
        /// Creates a new Game 
        /// </summary>
        /// <param name="startposition">The start position of the game. If skipped the standard initial position is used</param>
        public Game(string startposition = Fen.INITIAL_POSITION) : base()
        {
            StartPosition = startposition;
            int count = StartPosition.Split(new char[] { ' ' }).Length;
            if (count == 4) StartPosition = StartPosition + " 0 1";
            else if (count == 5) StartPosition = StartPosition + " 1";
            position = new Position(StartPosition);
            hashes.Add(position.PolyglotKey);
        }

        /// <summary>
        /// Name of the player playing the white pieces
        /// </summary>
        public string White { set; get; } = null;
        /// <summary>
        /// Name of the player playing the black pieces
        /// </summary>
        public string Black { set; get; } = null;
        /// <summary>
        /// The name of the tournament or match event
        /// </summary>
        public string Event { set; get; } = null;
        /// <summary>
        /// The location of the event
        /// </summary>
        public string Site { set; get; } = null;
        /// <summary>
        /// The starting date of the game
        /// </summary>
        public string Date { set; get; } = null;
        /// <summary>
        /// The playing round ordinal of the game
        /// </summary>
        public string Round { set; get; } = null;
        /// <summary>
        /// The result of the game
        /// </summary>
        public Result Result { set; get; } = Result.OPEN;
        /// <summary>
        /// The result of the game
        /// </summary>
        public ResultDetail ResultDetail { set; get; } = ResultDetail.UNKNOWN;
        /// <summary>
        /// The start position of the game (in FEN representation)
        /// </summary>
        public string StartPosition { private set; get; }
        /// <summary>
        /// Additional Tags
        /// </summary>
        public Dictionary<string, string> Tags { set; get; } = new Dictionary<string, string>();
        /// <summary>
        /// The Eco classification of the game
        /// </summary>
        public Eco Eco { get { return Eco.Get(this); } }
        /// <summary>
        /// The Side to Move at the end of the current move list
        /// </summary>
        public Side SideToMove { get { return position.SideToMove; } }
        /// <summary>
        /// Enumeration of the moves of the game
        /// </summary>
        public List<ExtendedMove> Moves { get { return moves; } }
        ///<summary>
        ///Get's the current Position (after the last move)
        ///</summary>
        public Position Position { get { return (Position)position.Clone(); } }
        /// <summary>
        /// Outputs the Game in SAN (Standard Algebraic Notation) Notation
        /// </summary>
        /// <returns>A string containing the notation</returns>
        public string SANNotation(bool withComments = true)
        {
            Position pos = new Position(StartPosition);
            StringBuilder sb = new StringBuilder();
            if (pos.SideToMove == Side.BLACK) sb.Append($"{pos.MoveNumber}... ");
            foreach (ExtendedMove m in moves)
            {
                if (pos.SideToMove == Side.WHITE) sb.Append($"{pos.MoveNumber}. ");
                sb.Append(pos.ToSAN(m)).Append(" ");
                if (withComments && m.Comment != null && m.Comment.Length > 0)
                {
                    sb.Append("{").Append(m.Comment).Append("} ");
                }
                pos.ApplyMove(m);
            }
            sb.Append(PGN.ResultToString(Result));
            return sb.ToString().Trim();
        }
        /// <summary>
        /// Outputs the Game in PGN (Portable Game Notation) format
        /// </summary>
        /// <param name="formatter">A <see cref="IPGNOutputFormatter"/>, which creates PGN comments for each move</param>
        /// <returns>A string containing the pgn formatted gamse</returns>
        public string ToPGN(IPGNOutputFormatter formatter = null)
        {
            if (formatter != null) foreach (ExtendedMove m in moves) m.Comment = formatter.Comment(m);
            StringBuilder sb = new StringBuilder(PGNTagSection());
            sb.AppendLine();
            //split movetext to lines with max 80 characters
            string movetext = SANNotation();
            while (movetext.Length > 80)
            {
                string m80 = movetext.Substring(0, 81);
                int indx = m80.LastIndexOf(' ');
                sb.AppendLine(movetext.Substring(0, indx));
                movetext = movetext.Substring(indx + 1);
            }
            sb.AppendLine(movetext);
            sb.AppendLine();
            return sb.ToString();
        }

        public string PGNTagSection()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"[Event \"{Event}\"]");
            sb.AppendLine($"[Site \"{Site}\"]");
            sb.AppendLine($"[Date \"{Date}\"]");
            sb.AppendLine($"[Round \"{Round}\"]");
            sb.AppendLine($"[White \"{White}\"]");
            sb.AppendLine($"[Black \"{Black}\"]");
            sb.AppendLine($"[Result \"{PGN.ResultToString(Result)}\"]");
            if (StartPosition == Fen.INITIAL_POSITION)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Eco eco = Eco;
                    sb.AppendLine($"[ECO \"{eco.Key}\"]");
                    sb.AppendLine($"[Opening \"{eco.Text}\"]");
                }
            }
            else
            {
                sb.AppendLine($"[SetUp \"1\"]");
                sb.AppendLine($"[FEN \"{StartPosition}\"]");
            }
            string termination = (new DetailedResult(Result, ResultDetail)).Termination;
            if (termination != null) sb.AppendLine($"[Termination \"{termination}\"]");
            foreach (string tag in Tags.Keys) sb.AppendLine($"[{tag} \"{Tags[tag]}\"]");
            return sb.ToString();
        }

        /// <summary>
        /// Set's the game's result from it's string representation
        /// </summary>
        /// <param name="resultString">The result as string, either "*", "1-0", "0-1", or "1/2-1/2"</param>
        /// <returns>true, if valid result string has been passed</returns>
        public bool SetResult(string resultString)
        {
            for (int i = 0; i < PGN.resultStrings.Length; ++i)
            {
                if (PGN.resultStrings[i] == resultString)
                {
                    Result = (Result)i;
                    ResultDetail = ResultDetail.UNKNOWN;
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Set's a PGN Tag value
        /// </summary>
        /// <param name="tag">The tag's name</param>
        /// <param name="value">The tag's value</param>
        public void SetTag(string tag, string value)
        {
            if (tag == "Event") Event = value;
            else if (tag == "Site") Site = value;
            else if (tag == "Date") Date = value;
            else if (tag == "Round") Round = value;
            else if (tag == "White") White = value;
            else if (tag == "Black") Black = value;
            else if (tag == "Result") SetResult(value);
            else if (tag == "FEN")
            {
                if (Moves == null || Moves.Count == 0)
                {
                    StartPosition = value;
                    position = new Position(value);
                    hashes.Clear();
                    hashes.Add(position.PolyglotKey);
                }
                Tags[tag] = value;
            }
            else
            {
                Tags[tag] = value;
            }
        }
        /// <summary>
        /// Adds a new Move to the game
        /// </summary>
        /// <param name="extendedMove"></param>
        /// <returns>true, if move is legal</returns>
        public bool Add(ExtendedMove extendedMove)
        {
            List<Move> legalMoves = position.GetMoves();
            bool legal = false;
            foreach (Move move in legalMoves)
            {
                if (move.Equals(extendedMove))
                {
                    legal = true;
                    break;
                }
            }
            if (!legal) return false;
            extendedMove.SideToMove = position.SideToMove;
            position.ApplyMove(extendedMove);
            moves.Add(extendedMove);
            if (position.DrawPlyCount == 0) hashes.Clear();
            hashes.Add(position.PolyglotKey);
            if (position.IsMate)
            {
                Result = SideToMove == Side.WHITE ? Result.BLACK_WINS : Result.WHITE_WINS;
                ResultDetail = ResultDetail.MATE;
            }
            else if (position.IsStalemate)
            {
                Result = Result.DRAW;
                ResultDetail = ResultDetail.STALEMATE;
            }
            else if (position.DrawPlyCount >= 100)
            {
                Result = Result.DRAW;
                ResultDetail = ResultDetail.FIFTY_MOVES;
            }
            else if (Check3FoldRepetition())
            {
                Result = Result.DRAW;
                ResultDetail = ResultDetail.THREE_FOLD_REPETITION;
            }
            else if (position.IsDrawnByInsufficientMatingMaterial())
            {
                Result = Result.DRAW;
                ResultDetail = ResultDetail.NO_MATING_MATERIAL;
            }
            return true;
        }
        /// <summary>
        /// Checks if current position has been 3-fold repeated
        /// </summary>
        /// <returns>true, if position has already repeated 3 times</returns>
        private bool Check3FoldRepetition()
        {
            if (hashes.Count < 8) return false;
            ulong checkHash = hashes.Last();
            int repetitions = 0;
            foreach (ulong hash in hashes)
            {
                if (hash == checkHash) ++repetitions;
            }
            return repetitions >= 3;
        }
        /// <inheritdoc/>
        public object Clone()
        {
            Game game = new Game(StartPosition)
            {
                position = (Position)position.Clone(),
                hashes = new List<ulong>(hashes),
                Black = Black,
                Date = Date,
                Event = Event,
                moves = new List<ExtendedMove>(moves),
                Result = Result,
                ResultDetail = ResultDetail,
                Round = Round,
                Site = Site,
                Tags = new Dictionary<string, string>(Tags),
                White = White
            };
            return game;
        }

        private List<ExtendedMove> moves = new List<ExtendedMove>();
        private Position position = null;
        private List<UInt64> hashes = new List<ulong>();

        public override string ToString()
        {
            return $"{White}-{Black} {PGN.ResultToString(Result)} ({Event} {Round})";
        }

    }
}
