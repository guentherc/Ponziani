using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PonzianiComponents.Chesslib
{
    /// <summary>
    /// Representation of a chess game
    /// </summary>
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
            if (count == 4) StartPosition += " 0 1";
            else if (count == 5) StartPosition += " 1";
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
        public string SANNotation(bool withComments = true, bool withVariations = false)
        {
            Position pos = new(StartPosition);
            StringBuilder sb = GetMoveText(pos, moves, withComments, withVariations);
            sb.Append(PGN.ResultToString(Result));
            return sb.ToString().Trim();
        }
        /// <summary>
        /// TimeControl used whan game was played (usually null)
        /// </summary>
        public TimeControl TimeControl { set; get; } = null;

        private StringBuilder GetMoveText(Position pos, List<ExtendedMove> _moves, bool withComments, bool withVariations)
        {
            StringBuilder sb = new();
            if (pos.SideToMove == Side.BLACK) sb.Append($"{pos.MoveNumber}... ");
            foreach (ExtendedMove m in _moves)
            {
                if (pos.SideToMove == Side.WHITE) sb.Append($"{pos.MoveNumber}. ");
                sb.Append(pos.ToSAN(m)).Append(' ');
                if (withComments && m.Comment != null && m.Comment.Length > 0)
                {
                    sb.Append('{').Append(m.Comment).Append("} ");
                }
                if (withVariations && m.Variations != null)
                {
                    foreach (var variation in m.Variations)
                    {
                        sb.Append($"( {GetMoveText((Position)pos.Clone(), variation, withComments, withVariations)} ) ");
                    }
                }
                pos.ApplyMove(m);
            }
            return sb;
        }

        /// <summary>
        /// Outputs the Game in PGN (Portable Game Notation) format
        /// </summary>
        /// <param name="formatter">A <see cref="IPGNOutputFormatter"/>, which creates PGN comments for each move</param>
        /// <returns>A string containing the pgn formatted gamse</returns>
        public string ToPGN(IPGNOutputFormatter formatter = null, bool withVariations = false)
        {
            if (formatter != null) foreach (ExtendedMove m in moves) m.Comment = formatter.Comment(m);
            StringBuilder sb = new(PGNTagSection());
            sb.AppendLine();
            //split movetext to lines with max 80 characters
            string movetext = SANNotation(true, withVariations);
            while (movetext.Length > 80)
            {
                string m80 = movetext.Substring(0, 81);
                int indx = m80.LastIndexOf(' ');
                sb.AppendLine(movetext.Substring(0, indx));
                movetext = movetext[(indx + 1)..];
            }
            sb.AppendLine(movetext);
            sb.AppendLine();
            return sb.ToString();
        }
        /// <summary>
        /// Position for a given move within the game
        /// </summary>
        /// <param name="moveNumber">Move number for which the position should be determined</param>
        /// <param name="side">Side (White/Black) for which the position should be determined</param>
        /// <returns>The position at that point within the game</returns>
        public Position GetPosition(int moveNumber, Side side)
        {
            Position pos = new(StartPosition);
            int i = 0;
            while (i < Moves.Count && (pos.MoveNumber < moveNumber || pos.SideToMove != side))
            {
                pos.ApplyMove(Moves[i]);
                ++i;
            }
            return (pos.MoveNumber == moveNumber && pos.SideToMove == side) ? pos : null;
        }
        /// <summary>
        /// Move for a given movenumber and side
        /// </summary>
        /// <param name="moveNumber">Move number for which the position should be determined</param>
        /// <param name="side">Side (White/Black) for which the position should be determined</param>
        /// <returns>The move at that point within the game</returns>
        public ExtendedMove GetMove(int moveNumber, Side side)
        {
            int targetPly = PlyIndex(moveNumber, side);
            int currentPly = PlyIndex(position.MoveNumber, position.SideToMove);
            int indx = Moves.Count - (currentPly - targetPly);
            return indx >= 0 && indx < Moves.Count ? Moves[indx] : null;
        }

        public static int PlyIndex(int moveNumber, Side side)
        {
            return 2 * (moveNumber - 1) + (int)side;
        }

        private string PGNTagSection()
        {
            StringBuilder sb = new();
            sb.AppendLine($"[Event \"{Event}\"]");
            sb.AppendLine($"[Site \"{Site}\"]");
            sb.AppendLine($"[Date \"{Date}\"]");
            if (position.Chess960)
            {
                sb.AppendLine($"[Variant \"Chess960\"]");
            }
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
            else if (tag == "TimeControl")
            {
                TimeControl = new TimeControl(value);
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
            bool isPromotion = (extendedMove.To >= Square.A8 || extendedMove.To <= Square.H1) && position.GetPiece(extendedMove.From) == (Piece)(8 + (int)position.SideToMove);
            extendedMove.UndoInfo = new UndoInfo(position.DrawPlyCount, position.GetPiece(extendedMove.To), position.EPSquare, position.castlings, isPromotion);
            extendedMove.SideToMove = position.SideToMove;
            position.ApplyMove(extendedMove);
            moves.Add(extendedMove);
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
        /// Undos the last applied move from the game
        /// </summary>
        /// <returns>true if successful</returns>
        public bool UndoLastMove()
        {
            if (position.UndoMove(Moves.Last(), hashes[^2]))
            {
                Result = Result.OPEN;
                hashes.RemoveAt(hashes.Count - 1);
                Moves.RemoveAt(Moves.Count - 1);
            }
            else
            {
                Debug.Assert(false); //Shouldn't happen!
                return false;
            }
            return true;
        }
        /// <summary>
        /// Checks if current position has been 3-fold repeated
        /// </summary>
        /// <returns>true, if position has already repeated 3 times</returns>
        private bool Check3FoldRepetition()
        {
            if (Position.DrawPlyCount < 8) return false;
            ulong checkHash = hashes.Last();
            int repetitions = 0;
            for (int i = hashes.Count - Position.DrawPlyCount; i < hashes.Count; ++i)
            {
                if (hashes[i] == checkHash) ++repetitions;
            }
            return repetitions >= 3;
        }
        /// <inheritdoc/>
        public object Clone()
        {
            Game game = new(StartPosition)
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

        internal List<Game> VariationGames()
        {
            List<Game> vgames = new();
            if (Moves.Last().Variations != null)
            {
                foreach (var variation in Moves.Last().Variations)
                {
                    Game g = (Game)Clone();
                    g.UndoLastMove();
                    foreach (var move in variation)
                    {
                        g.Add(move);
                    }
                    vgames.Add(g);
                }
            }
            return vgames;
        }

        private List<ExtendedMove> moves = new();
        private Position position = null;
        private List<UInt64> hashes = new();

        public override string ToString()
        {
            return $"{White}-{Black} {PGN.ResultToString(Result)} ({Event} {Round})";
        }

    }
    /// <summary>
    /// Class reprsenting time control settings of a game
    /// </summary>
    public class TimeControl
    {
        /// <summary>
        /// List of Time Controls
        /// </summary>
        public List<Entry> Controls = new();

        /// <summary>
        /// Creates a time control object
        /// </summary>
        /// <param name="tc">Time Control represented as PGN TimeControl Tag-Value</param>
        public TimeControl(string tc = "")
        {
            if (tc == "") return;
            //9.6.1: Tag: TimeControl

            //This uses a list of one or more time control fields.Each field contains a
            //descriptor for each time control period; if more than one descriptor is present
            //then they are separated by the colon character(":").The descriptors appear
            //in the order in which they are used in the game.  The last field appearing is
            //considered to be implicitly repeated for further control periods as needed.

            //There are six kinds of TimeControl fields.

            //The first kind is a single question mark("?") which means that the time
            //control mode is unknown.When used, it is usually the only descriptor present.

            //The second kind is a single hyphen("-") which means that there was no time
            //control mode in use.When used, it is usually the only descriptor present.

            //The third Time control field kind is formed as two positive integers separated
            //by a solidus("/") character.The first integer is the number of moves in the
            //period and the second is the number of seconds in the period.Thus, a time
            //control period of 40 moves in 2 1 / 2 hours would be represented as "40/9000".

            //The fourth TimeControl field kind is used for a "sudden death" control period.
            //It should only be used for the last descriptor in a TimeControl tag value.It
            //is sometimes the only descriptor present.The format consists of a single
            //integer that gives the number of seconds in the period.Thus, a blitz game
            //would be represented with a TimeControl tag value of "300".

            //The fifth TimeControl field kind is used for an "incremental" control period.
            //It should only be used for the last descriptor in a TimeControl tag value and
            //is usually the only descriptor in the value.The format consists of two
            //positive integers separated by a plus sign("+") character.The first integer
            //gives the minimum number of seconds allocated for the period and the second
            //integer gives the number of extra seconds added after each move is made.So,
            //an incremental time control of 90 minutes plus one extra minute per move would
            //be given by "4500+60" in the TimeControl tag value.

            //The sixth TimeControl field kind is used for a "sandclock" or "hourglass"
            //control period.It should only be used for the last descriptor in a
            //TimeControl tag value and is usually the only descriptor in the value.The
            //format consists of an asterisk("*") immediately followed by a positive
            //integer.The integer gives the total number of seconds in the sandclock
            //period.The time control is implemented as if a sandclock were set at the
            //start of the period with an equal amount of sand in each of the two chambers
            //and the players invert the sandclock after each move with a time forfeit
            //indicated by an empty upper chamber.  Electronic implementation of a physical
            //sandclock may be used.An example sandclock specification for a common three
            //minute egg timer sandclock would have a tag value of "*180".

            string[] token = tc.Split(':');
            foreach (var t in token)
            {
                int time;
                int to = int.MaxValue;
                double inc = 0;
                int from = Controls.Count == 0 ? 1 : Controls.Last().To + 1;
                int indx1 = t.IndexOf("/");
                int indx2 = t.IndexOf("+");
                if (indx1 < 0 && indx2 < 0)
                {
                    Debug.Assert(Controls.Count == 0 || Controls.Last().To < int.MaxValue);
                    if (int.TryParse(t, out time))
                    {
                        Controls.Add(new Entry(from, int.MaxValue, TimeSpan.FromSeconds(time), TimeSpan.Zero));
                        continue;
                    }
                }
                else
                {
                    if (indx1 > 0) to = int.Parse(t.Substring(0, indx1)) + from - 1;
                    if (indx2 > 0) inc = double.Parse(t[(indx2 + 1)..], CultureInfo.InvariantCulture);
                    string t1 = indx2 > 0 ? t.Substring(0, indx2) : t;
                    if (indx1 > 0) t1 = t1[(indx1 + 1)..];
                    time = int.Parse(t1);
                    Controls.Add(new Entry(from, to, TimeSpan.FromSeconds(time), TimeSpan.FromSeconds(inc)));
                }
            }
        }
        /// <summary>
        /// Calculates the total available think time for all moves so far
        /// </summary>
        /// <param name="movenumber">Move number for which the total available time shall be calculated</param>
        public TimeSpan TotalAvailableTime(int movenumber)
        {
            TimeSpan total = TimeSpan.Zero;
            foreach (var entry in Controls)
            {
                if (movenumber > entry.To)
                {
                    total += entry.Time + (entry.To - entry.From + 1) * entry.Increment;
                }
                else
                {
                    total += entry.Time + (movenumber - entry.From + 1) * entry.Increment;
                    break;
                }
            }
            if (movenumber > Controls.Last().To)
            {
                //special logic for repeating last increment
                int from = Controls.Last().To + 1;
                while (true)
                {
                    int to = from + Controls.Last().To - Controls.Last().From;
                    if (movenumber > to)
                    {
                        total += Controls.Last().Time + (to - from + 1) * Controls.Last().Increment;
                    }
                    else
                    {
                        total += Controls.Last().Time + (movenumber - from + 1) * Controls.Last().Increment;
                        break;
                    }
                    from = to + 1;
                }
            }
            return total;
        }
        /// <summary>
        /// Enhances Think Times, if only Clock values are available
        /// </summary>
        /// <param name="game">Game which will be enhanced</param>
        public void AddThinkTimes(Game game)
        {
            string[] ftoken = game.StartPosition.Split(' ');
            Side side = ftoken[1] == "w" ? Side.WHITE : Side.BLACK;
            int movenumber = int.Parse(ftoken[5]);
            if (side == Side.WHITE && movenumber == 1)
            {
                game.Moves[0].UsedThinkTime = TotalAvailableTime(1) - game.Moves[0].Clock;
                game.Moves[1].UsedThinkTime = TotalAvailableTime(1) - game.Moves[1].Clock;
            }
            for (int i = 2; i < game.Moves.Count; ++i)
            {
                game.Moves[i].UsedThinkTime = (game.Moves[i - 2].Clock - game.Moves[i].Clock) - (TotalAvailableTime(movenumber) - TotalAvailableTime(movenumber - 1));
                side = (Side)((int)side ^ 1);
                if (side == Side.WHITE) ++movenumber;
            }
        }

        /// <summary>
        /// Single TimeControl 
        /// </summary>
        public class Entry
        {
            public Entry(int from, int to, TimeSpan time, TimeSpan increment)
            {
                From = from;
                To = to;
                Time = time;
                Increment = increment;
            }
            /// <summary>
            /// Validity of Time Control: Move number from which the time control is valid
            /// </summary>
            public int From { set; get; } = 1;
            /// <summary>
            /// Validity of Time Control: Last Move number for which the time control is valid
            /// </summary>
            public int To { set; get; } = int.MaxValue;
            /// <summary>
            /// Available Time 
            /// </summary>
            public TimeSpan Time { set; get; }
            /// <summary>
            /// Increment added for each move within this time interval
            /// </summary>
            public TimeSpan Increment { set; get; } = TimeSpan.Zero;
        }
    }
}
