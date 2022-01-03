using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PonzianiComponents.Chesslib
{
    /// <summary>
    /// Interface to a chess engine supporting <see href="https://en.wikipedia.org/wiki/Universal_Chess_Interface">UCI protocol </see>
    /// <para>The class allows to implement the GUI part of the UCI protocol manually by using method <see cref="SendToEngineAsync(string)"/> 
    ///  to send commands to the engine process and event <see cref="OnEngineOutput"/> to receive messages</para>
    /// <para>Nevertheless the recommended way is to use the more comfortable methods, which this class offers like <see cref="AnalyzeAsync(string, TimeSpan, Dictionary{string, string}, List{Move})"/> or
    /// <see cref="StartThinkingAsync(TimeSpan?, TimeSpan?, TimeSpan?, TimeSpan?, int, int, long, bool, List{Move})"/></para>
    /// <example>
    /// <code>
    /// Game game = PGN.Parse(pathToPGN)[0];
    /// using (UCIEngine engine = new UCIEngine(enginePath))
    /// {
    ///      ExtendedMove move = engine.AnalyseAsync(game, TimeSpan.FromMilliseconds(100), 25, Side.BLACK).Result;
    ///      Console.WriteLine($"Evaluation: {move.Evaluation}");   
    /// }
    /// </code>
    /// </example>
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    public sealed class UCIEngine : IDisposable
    {
        /// <summary>
        /// Event Arguments for the <see cref="OnEngineInfoChanged"/> event
        /// </summary>
        public class EngineInfoEventArgs : EventArgs
        {
            public EngineInfoEventArgs(Info info)
            {
                Info = info;
            }

            public Info Info { get; set; }
        }

        /// <summary>
        /// Event Arguments for the <see cref="OnEngineOutput"/> event
        /// </summary>
        public class EngineOutputEventArgs : EventArgs
        {
            public EngineOutputEventArgs(string message)
            {
                Message = message;
            }

            public string Message { get; set; }
        }

        /// <summary>
        /// Raised whenever the engine issues a "info" message (except "info string" messages), by which
        /// the engine sends information about the current state of analysis
        /// </summary>
        public event EventHandler<EngineInfoEventArgs> OnEngineInfoChanged;

        /// <summary>
        /// Raised whenever the engine outputs anything
        /// </summary>
        public event EventHandler<EngineOutputEventArgs> OnEngineOutput;

        /// <summary>
        /// Path to the engine executable
        /// </summary>
        public string Executable { get; init; }
        /// <summary>
        /// UCI options, passed via setoption command
        /// </summary>
        public Dictionary<string, string> Parameters { get; set; }
        /// <summary>
        /// Arguments passed on start of the engine executable
        /// </summary>
        public string Arguments { get; set; }
        /// <summary>
        /// Working directory of the engine process. If not set directory of <see cref="Executable"/>
        /// is used
        /// </summary>
        public string WorkingDirectory { get; set; }
        /// <summary>
        /// Engine Name (as given by engine process). Only available once engine process is started.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Engine Author (as given by engine process). Only available once engine process is started. 
        /// </summary>
        public string Author { get; private set; }
        /// <summary>
        /// Options as provided by the engine's option commands
        /// </summary>
        public Dictionary<string, Option> Options { get; private set; } = new Dictionary<string, Option>();

        public ExtendedMove BestMove { get; internal set; }
        public Move PonderMove { get; internal set; }
        /// <summary>
        /// Id of engine process (0 if process isn't started or stopped)
        /// </summary>
        public int ProcessId => process != null ? process.Id : 0;
        /// <summary>
        /// Get's the current Analysis result from the engine
        /// </summary>
        public Info AnalysisInfo => Infos[0];
        /// <summary>
        /// Get's the current Analysis result from the engine for a specified line (inMultiPV mode) 
        /// </summary>
        /// <param name="line">Line index (zero-based, so line = 0 will return the infor for the main line)</param>
        /// <returns>Engine's info for this line</returns>
        public Info GetAnalysisInfo(int line = 0)
        {
            return Infos[0];
        }
        /// <summary>
        /// Creates a new UCI engine
        /// </summary>
        /// <param name="executable">Path to the engine executable</param>
        /// <param name="parameters">Parameters, which will be passed to the engine later on by calling <see cref="SetOptionsAsync"/></param>
        /// <param name="arguments">Command line arguments passed to the engine process on startup</param>
        public UCIEngine(string executable, Dictionary<string, string> parameters = null, string arguments = null)
        {
            if (!File.Exists(executable)) throw new FileNotFoundException(executable);
            Executable = executable;
            if (parameters != null) Parameters = parameters; else Parameters = new();
            Arguments = arguments;

        }
        /// <summary>
        /// Sends a message (UIC command) to engine.
        /// <para><b>Handle with care: There is only partial input validation performed. Messages which don't
        /// fulfill the UCI protocol might break the engine process!</b></para>
        /// </summary>
        /// <param name="message">The message to be sent to the engine</param>
        /// <returns>true, if everything went right</returns>
        /// <exception cref="EngineException">thrown, if engine state doesn't allow to send the message</exception>
        public async Task<bool> SendToEngineAsync(string message)
        {
            message = message.Trim();
            if (message.StartsWith("position "))
            {
                if (message.StartsWith("position startpos"))
                {
                    message = message.Replace("startpos", "fen " + Fen.INITIAL_POSITION);
                }
                int indx = message.IndexOf(" moves ");
                if (indx >= 0)
                {
                    string fen = message.Substring(13, indx - 13);
                    string moves = message.Substring(indx + 6);
                    return await SetPositionAsync(fen, moves);
                }
                else
                {
                    string fen = message.Substring(13);
                    return await SetPositionAsync(fen);
                }

            }
            else if (message == "stop") StopThinkingAsync().Wait();
            else if (message == "go" || message == "go infinite") return await StartAnalysisAsync();
            else if (message.StartsWith("go "))
            {
                string[] token = message.Split(' ');
                if (token.Length == 3 && token[1] == "movetime") return await StartAnalysisAsync(TimeSpan.FromMilliseconds(int.Parse(token[2])));
                else if (token.Length == 3 && token[1] == "depth") return await StartAnalysisAsync(int.Parse(token[2]));
                else
                {
                    return await processGoAsync(message);
                }
            }
            else if (message == "ponderhit") Ponderhit();
            else if (message == "ucinewgame") NewGameAsync().Wait();
            else if (message.StartsWith("setoption name "))
            {
                int indx = message.IndexOf(" value ");
                if (indx > 0)
                {
                    int l = "setoption name ".Length;
                    string key = message.Substring(l, indx - l);
                    if (Parameters.ContainsKey(key)) Parameters[key] = message.Substring(indx+7);
                    else Parameters[key] = message.Substring(indx + 7);
                }
            }
            else Send(message);
            return true;
        }

        /// <summary>
        /// Stops engine analysis (sends UCI "stop" command)
        /// </summary>
        /// <returns>true, if everything went right</returns>
        /// <exception cref="EngineException">thrown if engine isn't analysing</exception>
        public async Task<bool> StopThinkingAsync()
        {
            if (_state != EngineState.THINKING)
                throw new EngineException($"Can't stop thinking - Engine state is { _state.ToString() }, should be { EngineState.THINKING.ToString() }");
            tcsStopThinking = new();
            Send("stop");
            return await tcsStopThinking.Task;
        }
        /// <summary>
        /// Set's the engine's position 
        /// </summary>
        /// <param name="fen">Start position in <see href="https://de.wikipedia.org/wiki/Forsyth-Edwards-Notation">FEN</see> representation</param>
        /// <param name="movelist">Moves leading from start position to position to be analysed, in UCI notation separated by spaces. 
        /// Example: <c>e2e4 e7e5 g1f3 b8c6 f1b5 a7a6</c></param>
        /// <returns>true, if everything went right</returns>
        /// <exception cref="EngineException">thrown if engine state doesn't allow to set position. Method must 
        /// not be called while engine is off, initializing or thinking</exception>
        public async Task<bool> SetPositionAsync(string fen = Fen.INITIAL_POSITION, string movelist = null)
        {
            bool result = true;
            if (_state == EngineState.THINKING) result = await StopThinkingAsync();
            if (_state != EngineState.READY)
                throw new EngineException($"Can't set position - Engine state is { _state.ToString() }, should be { EngineState.READY.ToString() }");
            _game = new(fen);
            if (movelist != null)
            {
                string[] moves = movelist.Split();
                foreach (string move in moves)
                {
                    _game.Add(new(move));
                }
            }
            _movenumber = _game.Position.MoveNumber;
            _side = _game.SideToMove;
            if (_game.Moves.Count > 0)
                Send($"position fen { fen } moves { movelist }");
            else Send($"position fen { fen }");
            return result;
        }
        /// <summary>
        /// Sets the engine's position
        /// </summary>
        /// <param name="position">Position to be analyzed</param>
        /// <returns>true, if everything went right</returns>
        /// <exception cref="EngineException">thrown if engine state doesn't allow to set position. Method must 
        /// not be called while engine is off, initializing or thinking</exception>
        public async Task<bool> SetPositionAsync(Position position)
        {
            bool result = true;
            if (_state == EngineState.THINKING) result = await StopThinkingAsync();
            if (_state != EngineState.READY)
                throw new EngineException($"Can't set position - Engine state is { _state.ToString() }, should be { EngineState.READY.ToString() }");
            _game = new(position.FEN);
            _movenumber = _game.Position.MoveNumber;
            _side = _game.SideToMove;
            Send($"position fen { position.FEN }");
            return result;
        }
        /// <summary>
        /// Sets the engine's position from a <see cref="Game"/>
        /// </summary>
        /// <param name="game">Game, from which position is taken</param>
        /// <param name="moveNumber">Movenumber of position</param>
        /// <param name="side">Side to move of position</param>
        /// <returns>true, if everything went right</returns>
        /// <exception cref="EngineException">thrown if engine state doesn't allow to set position. Method must 
        /// not be called while engine is off, initializing or thinking</exception>
        public async Task<bool> SetPositionAsync(Game game, int moveNumber, Side side)
        {
            bool result = true;
            if (_state == EngineState.THINKING) result = await StopThinkingAsync();
            if (_state != EngineState.READY)
                throw new EngineException($"Can't set position - Engine state is { _state.ToString() }, should be { EngineState.READY.ToString() }");
            _game = (Game)game.Clone();
            _movenumber = moveNumber;
            _side = side;
            StringBuilder sb = new StringBuilder();
            Position pos = new(_game.StartPosition);
            foreach (var move in _game.Moves)
            {
                pos.ApplyMove(move);
                sb.Append(move.ToUCIString()).Append(' ');
                if (pos.MoveNumber == moveNumber && pos.SideToMove == side) break;
            }
            if (sb.Length > 0)
                Send($"position fen { _game.StartPosition } moves { sb.ToString().Trim() }");
            else
                Send($"position fen { _game.StartPosition }");
            return result;
        }

        /// <summary>
        /// Starts an infinite analysis of the current position
        /// </summary>
        /// <param name="MovesToBeAnalyzed">List of moves, which shall be analyzed - if null, all moves will be analyzed</param>
        /// <returns>true, if everything went right</returns>
        /// <exception cref="EngineException">thrown if engine state doesn't allow to set position. Method must 
        /// not be called while engine is off, initializing or thinking</exception>
        public async Task<bool> StartAnalysisAsync(List<Move> MovesToBeAnalyzed = null)
        {
            bool result = true;
            if (_state == EngineState.THINKING) result = await StopThinkingAsync();
            if (_state != EngineState.READY)
                throw new EngineException($"Can't start analysis - Engine state is { _state.ToString() }, should be { EngineState.READY.ToString() }");
            _state = EngineState.THINKING;
            Send($"go infinite{ SearchMoveCommand(MovesToBeAnalyzed) }");
            return result;
        }

        /// <summary>
        /// Starts an analysis of the current position for a specified time
        /// </summary>
        /// <param name="thinkTime">the time the engine shall spend</param>
        /// <param name="MovesToBeAnalyzed">List of moves, which shall be analyzed - if null, all moves will be analyzed</param>
        /// <returns>true, if everything went right</returns>
        /// <exception cref="EngineException">thrown if engine state doesn't allow to set position. Method must 
        /// not be called while engine is off, initializing or thinking</exception>
        public async Task<bool> StartAnalysisAsync(TimeSpan thinkTime, List<Move> MovesToBeAnalyzed = null)
        {
            bool result = true;
            if (_state == EngineState.THINKING) result = await StopThinkingAsync();
            if (_state != EngineState.READY)
                throw new EngineException($"Can't start analysis - Engine state is { _state.ToString() }, should be { EngineState.READY.ToString() }");
            tcsFiniteAnalysis = new();
            Send($"go movetime {thinkTime.TotalMilliseconds}{ SearchMoveCommand(MovesToBeAnalyzed) }");
            result = await tcsFiniteAnalysis.Task && result;
            return result;
        }
        /// <summary>
        /// Starts an analysis of the current position up to a specified search depth
        /// </summary>
        /// <param name="depth">search depth (in plies)</param>
        /// <param name="MovesToBeAnalyzed">List of moves, which shall be analyzed - if null, all moves will be analyzed</param>
        /// <exception cref="EngineException">thrown if engine state doesn't allow to set position. Method must 
        /// not be called while engine is off, initializing or thinking</exception>
        public async Task<bool> StartAnalysisAsync(int depth, List<Move> MovesToBeAnalyzed = null)
        {
            bool result = true;
            if (_state == EngineState.THINKING) result = await StopThinkingAsync();
            if (_state != EngineState.READY)
                throw new EngineException($"Can't start analysis - Engine state is { _state.ToString() }, should be { EngineState.READY.ToString() }");
            tcsFiniteAnalysis = new();
            _state = EngineState.THINKING;
            Send($"go depth {depth}{ SearchMoveCommand(MovesToBeAnalyzed) }");
            result = await tcsFiniteAnalysis.Task && result;
            return result;
        }
        /// <summary>
        /// Starts an analysis of the current position in a match situation
        /// </summary>
        /// <param name="whiteTime">White's time on the clock</param>
        /// <param name="whiteIncrement">White's increment allocated after the move</param>
        /// <param name="blackTime">Black's time on the clock</param>
        /// <param name="blackIncrement">Black's increment allocated after the move</param>
        /// <param name="movesToGo">Moves until next time control</param>
        /// <param name="depth">Maximum search depth</param>
        /// <param name="nodes">Maximum nodes to be searched</param>
        /// <param name="ponder">Search shall be executed in ponder mode</param>
        /// <param name="MovesToBeAnalyzed">List of moves, which shall be analyzed - if null, all moves will be analyzed</param>
        /// <returns>true, if everything went right</returns>
        /// <exception cref="EngineException">thrown if engine state doesn't allow to set position. Method must 
        /// not be called while engine is off, initializing or thinking</exception>
        public async Task<bool> StartThinkingAsync(TimeSpan? whiteTime = null, TimeSpan? whiteIncrement = null,
                                  TimeSpan? blackTime = null, TimeSpan? blackIncrement = null,
                                  int movesToGo = 0, int depth = Int32.MaxValue, long nodes = 0, bool ponder = false, List<Move> MovesToBeAnalyzed = null)
        {
            bool result = true;
            if (_state == EngineState.THINKING) result = await StopThinkingAsync();
            if (_state != EngineState.READY)
                throw new EngineException($"Can't start analysis - Engine state is { _state.ToString() }, should be { EngineState.READY.ToString() }");
            tcsFiniteAnalysis = new();
            StringBuilder sb = new StringBuilder("go");
            if (ponder) sb.Append(" ponder");
            if (whiteTime != null && whiteTime.Value != TimeSpan.Zero) sb.Append($" wtime {whiteTime.Value.TotalMilliseconds}");
            if (whiteIncrement != null && whiteIncrement.Value != TimeSpan.Zero) sb.Append($" winc {whiteIncrement.Value.TotalMilliseconds}");
            if (blackTime != null && blackTime.Value != TimeSpan.Zero) sb.Append($" btime {blackTime.Value.TotalMilliseconds}");
            if (blackIncrement != null && blackIncrement.Value != TimeSpan.Zero) sb.Append($" binc {blackIncrement.Value.TotalMilliseconds}");
            if (movesToGo > 0) sb.Append($" movestogo { movesToGo }");
            if (depth < Int32.MaxValue) sb.Append($" depth { depth }");
            if (nodes > 0) sb.Append($" nodes { nodes }");
            sb.Append(SearchMoveCommand(MovesToBeAnalyzed));
            _state = EngineState.THINKING;
            Send(sb.ToString());
            result = await tcsFiniteAnalysis.Task && result;
            return result;
        }

        /// <summary>
        /// Starts the engine process
        /// </summary>
        /// <returns>true, if engine process could be started</returns>
        public async Task<bool> StartEngineAsync()
        {
            tcsStartEngine = new TaskCompletionSource<bool>();
            process = new Process();
            process.StartInfo.FileName = Executable;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.EnableRaisingEvents = true;
            if (Arguments != null) process.StartInfo.Arguments = Arguments;
            process.StartInfo.WorkingDirectory = WorkingDirectory == null || !Directory.Exists(Path.GetDirectoryName(Executable)) ? Path.GetDirectoryName(Executable) : WorkingDirectory;
            process.Exited += Process_Exited;
            process.Start();
            process.OutputDataReceived += Process_OutputDataReceived;
            process.ErrorDataReceived += Process_ErrorDataReceived;
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            _state = EngineState.INITIALIZING;
            Send("uci");
            return await tcsStartEngine.Task;
        }
        /// <summary>
        /// Calls the setoption commands used to configure the engine. The settings have to be provided before
        /// via the <see cref="Parameters"/> property
        /// </summary>
        /// <returns>true, if everything went right</returns>
        public async Task<bool> SetOptionsAsync()
        {
            tcsSetOptions = new TaskCompletionSource<bool>();
            foreach (var p in Parameters)
            {
                if (Options.ContainsKey(p.Key))
                {
                    Send($"setoption name {p.Key} value {p.Value}");
                }
                else
                {
                    Trace.WriteLine($"Unknown parameter {p.Key} will be ignored!");
                }
            }

            int multipv = Parameters.ContainsKey("MultiPV") ? int.Parse(Parameters["MultiPV"]) : 1;
            Infos = new();
            for (int i = 0; i < multipv; ++i) Infos.Add(new());
            Send("isready");
            return await tcsSetOptions.Task;
        }
        /// <summary>
        /// Sends the "ucinewgame" command to the engine
        /// <para> this shall besent to the engine when the next search (started with "position" and "go") will be from
        /// a different game.This can be a new game the engine should play or a new game it should analyse but
        /// also the next position from a testsuite with positions only.</para>
        /// <returns>true, if everything went right</returns>
        /// </summary>
        /// <exception cref="EngineException">thrown if engine state doesn't allow to set position. Method must 
        /// not be called while engine is off, initializing or thinking</exception>
        public async Task<bool> NewGameAsync()
        {
            if (_state == EngineState.THINKING) await StopThinkingAsync();
            if (_state != EngineState.READY)
                throw new EngineException($"Can't start analysis - Engine state is { _state.ToString() }, should be { EngineState.READY.ToString() }");
            tcsNewGame = new TaskCompletionSource<bool>();
            Send("ucinewgame");
            Send("isready");
            return await tcsNewGame.Task;
        }
        /// <summary>
        /// Sends the "ponderhit" command to the engine.
        /// <para>This shall be done to inform the engine, that the opponent has played the expected move. The engine will continue searching but switch 
        /// from pondering to normal search.</para>
        /// </summary>
        /// <exception cref="EngineException">thrown if engine state doesn't allow to set position. Method must 
        /// not be called while engine is not thinking</exception>
        public void Ponderhit()
        {
            if (_state != EngineState.THINKING)
                throw new EngineException($"Can't start analysis - Engine state is { _state.ToString() }, should be { EngineState.THINKING.ToString() }");
            Send("ponderhit");
        }

        /// <summary>
        /// Score from white's point of view in centipawns. Mate scores are converted to very high scores
        /// </summary>
        public int Score
        {
            get
            {
                int factor = _side == Side.WHITE ? 1 : -1;
                if (Infos[0].Type != Info.EvaluationType.Mate)
                {
                    int result = factor * Infos[0].Evaluation;
                    return result;
                }
                else
                {
                    return factor * (int.MaxValue - 2 * Infos[0].MateDistance);
                }
            }
        }
        /// <summary>
        /// Prepares the engine with one method call. It combines calls to <see cref="StartEngineAsync"/>,
        /// <see cref="SetOptionsAsync"/> and <see cref="NewGameAsync"/>.
        /// <code>
        ///  using (UCIEngine engine = new UCIEngine(enginePath))
        ///  {
        ///       // Start engine
        ///       engine.PrepareEngineForAnalysisAsync().Wait();
        ///       // Set position
        ///       engine.SetPositionAsync(Fen.INITIAL_POSITION).Wait();
        ///       // Analyze for one second
        ///       engine.StartAnalysisAsync(TimeSpan.FromSeconds(1)).Wait();
        ///       // Get best move
        ///       Console.WriteLine(engine.BestMove.ToUCIString());
        ///  }
        /// </code>
        /// </summary>
        /// <param name="parameter">Engine options, which will be sent to the engine using the "setoption" command</param>
        /// <returns>true, if everything went right</returns>
        public async Task<bool> PrepareEngineForAnalysisAsync(Dictionary<string, string> parameter = null)
        {
            bool result = true;
            if (_state == EngineState.THINKING) result = result && await StopThinkingAsync();
            else if (_state == EngineState.OFF) result = result && await StartEngineAsync();
            bool parameterChanged = false;
            if (parameter != null)
            {
                foreach (string key in parameter.Keys)
                {
                    if (Parameters.ContainsKey(key) && Parameters[key] != parameter[key])
                    {
                        Parameters[key] = parameter[key];
                        parameterChanged = true;
                    }
                    else if (!Parameters.ContainsKey(key))
                    {
                        Parameters.Add(key, parameter[key]);
                    }
                }
            }
            if (parameterChanged || _state < EngineState.READY)
            {
                result = result && await SetOptionsAsync();
                result = result && await NewGameAsync();
            }
            return result;
        }
        /// <summary>
        /// Prepares the engine and executes an analysis for a specified time
        /// </summary>
        /// <param name="fen">Position to be analyzed as <see href="https://en.wikipedia.org/wiki/Forsyth%E2%80%93Edwards_Notation">FEN</see></param>
        /// <param name="time">Time, the analysis shall take</param>
        /// <param name="parameter">Engine options, which will be sent to the engine using the "setoption" command</param>
        /// <param name="movesToBeAnalyzed">List of moves, which shall be analyzed - if null, all moves will be analyzed</param>
        /// <returns>The best move determined by the engine (includes Evaluation Info)</returns>
        public async Task<ExtendedMove> AnalyzeAsync(string fen, TimeSpan time, Dictionary<string, string> parameter = null, List<Move> movesToBeAnalyzed = null)
        {
            await PrepareEngineForAnalysisAsync(parameter);
            await SetPositionAsync(fen);
            await StartAnalysisAsync(time, movesToBeAnalyzed);
            return BestMove;
        }
        /// <summary>
        /// Prepares the engine and executes an analysis for a specified time
        /// </summary>
        /// <param name="fen">Startposition as <see href="https://en.wikipedia.org/wiki/Forsyth%E2%80%93Edwards_Notation">FEN</see></param>
        /// <param name="moves">Moves leading from start position to position to be analysed, in UCI notation separated by spaces.</param>
        /// <param name="time">Time, the analysis shall take</param>
        /// <param name="parameter">Engine options, which will be sent to the engine using the "setoption" command</param>
        /// <param name="movesToBeAnalyzed">List of moves, which shall be analyzed - if null, all moves will be analyzed</param>
        /// <returns>The best move determined by the engine (includes Evaluation Info)</returns>
        public async Task<ExtendedMove> AnalyzeAsync(string fen, string movelist, TimeSpan time, Dictionary<string, string> parameter = null, List<Move> movesToBeAnalyzed = null)
        {
            await PrepareEngineForAnalysisAsync(parameter);
            await SetPositionAsync(fen, movelist);
            await StartAnalysisAsync(time, movesToBeAnalyzed);
            return BestMove;
        }

        /// <summary>
        /// Prepares the engine and executes an analysis for a specified time
        /// </summary>
        /// <param name="fen">Startposition as <see href="https://en.wikipedia.org/wiki/Forsyth%E2%80%93Edwards_Notation">FEN</see></param>
        /// <param name="game">Game, from which position is taken</param>
        /// <param name="time">Time, the analysis shall take</param>
        /// <param name="moveNumber">Movenumber of position</param>
        /// <param name="side">Side to move of position</param>
        /// <param name="parameter">Engine options, which will be sent to the engine using the "setoption" command</param>
        /// <param name="movesToBeAnalyzed">List of moves, which shall be analyzed - if null, all moves will be analyzed</param>
        /// <returns>The best move determined by the engine (includes Evaluation Info)</returns>
        public async Task<ExtendedMove> AnalyzeAsync(Game game, TimeSpan time, int moveNumber = 1, Side side = Side.WHITE, Dictionary<string, string> parameter = null, List<Move> movesToBeAnalyzed = null)
        {
            await PrepareEngineForAnalysisAsync(parameter);
            await SetPositionAsync(game, moveNumber, side);
            await StartAnalysisAsync(time, movesToBeAnalyzed);
            return BestMove;
        }
        /// <summary>
        /// Prepares the engine and executes an analysis undermatch conditions
        /// </summary>
        /// <param name="game">The current game</param>
        /// <param name="moveNumber">Movenumber of position</param>
        /// <param name="side">Side to move of position</param>
        /// <param name="whiteTime">White's time on the clock</param>
        /// <param name="whiteIncrement">White's increment allocated after the move</param>
        /// <param name="blackTime">Black's time on the clock</param>
        /// <param name="blackIncrement">Black's increment allocated after the move</param>
        /// <param name="movesToGo">Moves until next time control</param>
        /// <param name="depth">Maximum search depth</param>
        /// <param name="nodes">Maximum nodes to be searched</param>
        /// <param name="ponder">Search shall be executed in ponder mode</param>
        /// <param name="parameter">Engine options, which will be sent to the engine using the "setoption" command</param>
        /// <param name="MovesToBeAnalyzed">List of moves, which shall be analyzed - if null, all moves will be analyzed</param>
        /// <returns>The engine's best move (including evaluation) info</returns>
        /// <exception cref="EngineException">thrown if engine state doesn't allow to set position. Method must 
        /// not be called while engine is off, initializing or thinking</exception>
        public async Task<ExtendedMove> AnalyzeAsync(Game game, int moveNumber = 1, Side side = Side.WHITE, TimeSpan? whiteTime = null, TimeSpan? whiteIncrement = null,
                                  TimeSpan? blackTime = null, TimeSpan? blackIncrement = null, int movesToGo = 0, int depth = Int32.MaxValue, long nodes = 0,
                                  bool ponder = false, Dictionary<string, string> parameter = null, List < Move> MovesToBeAnalyzed = null)
        {
            await PrepareEngineForAnalysisAsync(parameter);
            await SetPositionAsync(game, moveNumber, side);
            await StartThinkingAsync(whiteTime, whiteIncrement, blackTime, blackIncrement, movesToGo, depth, nodes, ponder, MovesToBeAnalyzed);
            return BestMove;
        }

        /// <inheritdoc cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            Exit().Wait();
        }

        private Process process;
        private TaskCompletionSource<bool> tcsStartEngine = null;
        private TaskCompletionSource<bool> tcsExitEngine = null;
        private TaskCompletionSource<bool> tcsSetOptions = null;
        private TaskCompletionSource<bool> tcsStopThinking = null;
        private TaskCompletionSource<bool> tcsFiniteAnalysis = null;
        private TaskCompletionSource<bool> tcsNewGame = null;

        private List<Info> Infos = new List<Info>() { new Info() };

        private Game _game = new();
        private int _movenumber = 1;
        private Side _side = Side.WHITE;

        private enum EngineState { OFF, INITIALIZING, READY, THINKING }
        private EngineState _state = EngineState.OFF;

        private async Task<bool> Exit()
        {
            tcsExitEngine = new TaskCompletionSource<bool>();
            Send("quit");
            return await tcsExitEngine.Task;
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Trace.WriteLine("<= " + e.Data);
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null) return;
            Trace.WriteLine("<= " + e.Data);
            if (e.Data.StartsWith("info string"))
            {

            }
            else if (e.Data.StartsWith("info "))
            {
                int index = Info.Index(e.Data);
                Info info = Infos[index];
                bool evaluationUpdate = info.Update(e.Data, _side);
                Infos[index] = info;
                if (evaluationUpdate)
                OnEngineInfoChanged?.Invoke(this, new EngineInfoEventArgs(info));
            }
            else if (e.Data.StartsWith("bestmove "))
            {
                _state = EngineState.READY;
                string[] token = e.Data.Split();
                BestMove = new(token[1]);
                BestMove.UsedThinkTime = TimeSpan.FromMilliseconds(Infos[0].Time);
                BestMove.Depth = Infos[0].Depth;
                BestMove.Evaluation = Infos[0].Evaluation;
                BestMove.SideToMove = _side;
                if (token.Length > 3)
                    PonderMove = new(token[3]);
                if (tcsStopThinking != null && !tcsStopThinking.Task.IsCompleted)
                    tcsStopThinking.SetResult(true);
                if (tcsFiniteAnalysis != null && !tcsFiniteAnalysis.Task.IsCompleted)
                    tcsFiniteAnalysis.SetResult(true);
            }
            else if (e.Data == "readyok")
            {
                if (_state < EngineState.READY) _state = EngineState.READY;
                if (tcsSetOptions != null && !tcsSetOptions.Task.IsCompleted)
                {
                    _state = EngineState.READY;
                    tcsSetOptions.SetResult(true);
                }
                if (tcsNewGame != null && !tcsNewGame.Task.IsCompleted)
                {
                    tcsNewGame.SetResult(true);
                }
            }
            else if (e.Data == "uciok")
            {
                tcsStartEngine.SetResult(true);
            }
            else if (e.Data.StartsWith("id "))
            {
                ProcessIdCommand(e.Data);
            }
            else if (e.Data.StartsWith("option "))
            {
                Option option = Option.Create(e.Data);
                if (Options.ContainsKey(option.Name)) Options[option.Name] = option;
                else Options.Add(option.Name, option);
            }
            OnEngineOutput?.Invoke(this, new EngineOutputEventArgs(e.Data));
        }

        private void Send(string message)
        {
            Trace.WriteLine("=> " + message);
            process.StandardInput.WriteLine(message);
        }

        private object SearchMoveCommand(List<Move> movesToBeAnalyzed)
        {
            if (movesToBeAnalyzed == null || movesToBeAnalyzed.Count == 0) return string.Empty;
            StringBuilder sb = new StringBuilder(" searchmoves");
            foreach (var move in movesToBeAnalyzed)
            {
                sb.Append(" ").Append(move.ToUCIString());
            }
            return sb.ToString();
        }

        private async Task<bool> processGoAsync(string message)
        {
            bool ponder = false;
            TimeSpan? wTime = null;
            TimeSpan? bTime = null;
            TimeSpan? wInc = null;
            TimeSpan? bInc = null;
            int movestogo = 0;
            int depth = Int32.MaxValue;
            long nodes = 0;
            string[] token = message.Split(' ');
            List<Move> moves = null;
            for (int i = 1; i < token.Length; ++i)
            {
                if (token[i] == "ponder") ponder = true;
                else if (token[i] == "wtime")
                {
                    ++i;
                    wTime = TimeSpan.FromMilliseconds(int.Parse(token[i]));
                }
                else if (token[i] == "winc")
                {
                    ++i;
                    wInc = TimeSpan.FromMilliseconds(int.Parse(token[i]));
                }
                else if (token[i] == "btime")
                {
                    ++i;
                    bTime = TimeSpan.FromMilliseconds(int.Parse(token[i]));
                }
                else if (token[i] == "binc")
                {
                    ++i;
                    bInc = TimeSpan.FromMilliseconds(int.Parse(token[i]));
                }
                else if (token[i] == "wtime")
                {
                    ++i;
                    wTime = TimeSpan.FromMilliseconds(int.Parse(token[i]));
                }
                else if (token[i] == "movestogo")
                {
                    ++i;
                    movestogo = int.Parse(token[i]);
                }
                else if (token[i] == "depth")
                {
                    ++i;
                    depth = int.Parse(token[i]);
                }
                else if (token[i] == "nodes")
                {
                    ++i;
                    nodes = int.Parse(token[i]);
                }
                else if (moves != null)
                    moves.Add(new Move(token[i]));
                else if (token[i] == "searchmoves")
                {
                    moves = new List<Move>();
                }
            }
            return await StartThinkingAsync(wTime, bTime, wInc, bInc, movestogo, depth, nodes, ponder, moves);
        }

        private void ProcessIdCommand(string data)
        {
            if (data.StartsWith("id name ")) Name = data.Substring(8);
            else if (data.StartsWith("id author ")) Author = data.Substring(9);
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            if (!tcsExitEngine.Task.IsCompleted) tcsExitEngine?.SetResult(true);
            Trace.WriteLine("Process exited!");
        }

        /// <summary>
        /// Information, sent by the engine during analyzing
        /// </summary>
        public struct Info
        {
            /// <summary>
            /// Determines the MultiPV (resp. line) Index from an engine's info message
            /// </summary>
            /// <param name="message">The info message issued by the engine</param>
            /// <returns>the 0-based line index (multipv 1 will give 0)</returns>
            public static int Index(string message)
            {
                int index = message.IndexOf("multipv");
                if (index == -1) return 0;
                string[] token = message.Substring(index + 8).Split();
                return int.Parse(token[0]) - 1;
            }
            /// <summary>
            /// Updates an existing Info objekt from a new info message
            /// </summary>
            /// <param name="message">The info message issued by the engine</param>
            /// <param name="side">Side to Move</param>
            /// <returns>true, if new evaluation information was part of the info message</returns>
            public bool Update(string message, Side side = Side.WHITE)
            {
                bool result = false;
                string[] token = message.Split();
                bool pv = false;
                for (int i = 1; i < token.Length; ++i)
                {
                    if (token[i] == "depth")
                    {
                        ++i; Depth = int.Parse(token[i]);
                    }
                    else if (token[i] == "seldepth")
                    {
                        ++i; MaxDepth = int.Parse(token[i]);
                    }
                    else if (token[i] == "time")
                    {
                        ++i; Time = int.Parse(token[i]);
                    }
                    else if (token[i] == "nodes")
                    {
                        ++i; Nodes = long.Parse(token[i]);
                    }
                    else if (token[i] == "nps")
                    {
                        ++i; NodesPerSecond = long.Parse(token[i]);
                    }
                    else if (token[i] == "tbhits")
                    {
                        ++i; TableBaseHits = long.Parse(token[i]);
                    }
                    else if (token[i] == "multipv")
                    {
                        ++i; MoveIndex = int.Parse(token[i]);
                    }
                    else if (token[i] == "multipv")
                    {
                        ++i; MoveIndex = int.Parse(token[i]);
                    }
                    else if (token[i] == "currmovenumber")
                    {
                        ++i; CurrentMoveNumber = int.Parse(token[i]);
                    }
                    else if (token[i] == "currmove")
                    {
                        ++i; CurrentMove = token[i];
                    }
                    else if (token[i] == "score")
                    {
                        ++i;
                        if (token[i] == "cp") Type = EvaluationType.Exact;
                        else if (token[i] == "mate") Type = EvaluationType.Mate;
                        else if (token[i] == "upperbound") Type = EvaluationType.Upperbound;
                        else if (token[i] == "lowerbound") Type = EvaluationType.Lowerbound;
                        ++i;
                        if (Type == EvaluationType.Mate) MateDistance = int.Parse(token[i]);
                        else Evaluation = int.Parse(token[i]);
                        result = true;
                    }
                    else if (token[i] == "pv")
                    {
                        ++i; PrincipalVariation = token[i];
                        pv = true;
                        continue;
                    }
                    else if (pv)
                    {
                        PrincipalVariation += " " + token[i];
                        continue;
                    }
                    pv = false;
                }
                return result;
            }

            /// <summary>
            /// Evaluation Type
            /// <para><see cref="Evaluation"/> might not always be exact. Sometimes engines output a lower or a upper bound only</para>
            /// </summary>
            public enum EvaluationType { Exact, Upperbound, Lowerbound, Mate };
            /// <summary>
            /// Engine score in Centipawns (from engine's point of view)
            /// </summary>
            public int Evaluation { get; private set; } = 0;
            /// <summary>
            /// Evaluation Type
            /// </summary>
            public EvaluationType Type { get; private set; } = EvaluationType.Exact;
            /// <summary>
            /// Current analysis depth
            /// </summary>
            public int Depth { get; private set; } = 0;
            /// <summary>
            /// Current maximal analysis depth reached for selected search branches
            /// </summary>
            public int MaxDepth { get; private set; } = 0;
            /// <summary>
            /// Current Search Time (in milliseconds)
            /// </summary>
            public int Time { get; private set; } = 0;
            /// <summary>
            /// Number of Nodes searched
            /// </summary>
            public long Nodes { get; private set; } = 0;
            /// <summary>
            /// Search speed in (nodes/second)
            /// </summary>
            public long NodesPerSecond { get; private set; } = 0;
            /// <summary>
            /// Principal Variation 
            /// </summary>
            public string PrincipalVariation { get; private set; } = string.Empty;
            /// <summary>
            /// Mate distance (Mate in x moves)
            /// </summary>
            public int MateDistance { get; private set; } = int.MaxValue;
            /// <summary>
            /// If engine runs in MultiPV mode (analyzing more than one next move) the move's index
            /// </summary>
            public int MoveIndex { get; private set; } = 1;
            /// <summary>
            /// Currently analyzed move (in UCI notation)
            /// </summary>
            public string CurrentMove { get; private set; } = string.Empty;
            /// <summary>
            /// Currently searching move number x, for the first move x should be 1 not 0
            /// </summary>
            public int CurrentMoveNumber { get; private set; } = 0;
            /// <summary>
            /// Number of table base hits
            /// </summary>
            public long TableBaseHits { get; private set; } = 0;

        }

        public class EngineException : System.Exception
        {
            public EngineException() : base() { }

            public EngineException(string message) : base(message) { }

            public EngineException(string message, Exception innerException) : base(message, innerException)
            {
            }
        }

        /// <summary>
        /// Represents an engine option
        /// </summary>
        public class Option
        {
            /// <summary>
            /// Creates an engine option object from an engine's UCI option command
            /// </summary>
            /// <param name="ucicommand">UCI option command, as specified in UCI protocol</param>
            /// <exception cref="ArgumentException">thrown, when option command doesn't fulfill UCI protocol</exception>
            internal Option(string ucicommand)
            {
                Parse(ucicommand);
            }

            protected virtual void Parse(string ucicommand)
            {
                Match m = regexOption.Match(ucicommand);
                if (!m.Success) throw new ArgumentException("Invalid Option Command");
                Name = m.Groups[1].Value.Trim();
                Type = (OptionType)Enum.Parse(typeof(OptionType), m.Groups[2].Value.ToUpperInvariant());
            }

            public enum OptionType { CHECK, SPIN, COMBO, BUTTON, STRING }
            /// <summary>
            /// Name of option
            /// </summary>
            public string Name { get; internal set; }
            /// <summary>
            /// Option type
            /// </summary>
            public OptionType Type { get; internal set; }

            private static Regex regexOption = new Regex(@"option\sname\s(\S+\s)+type\s(\S+)");

            internal static Option Create(string ucicommand)
            {
                if (ucicommand.IndexOf("type check") >= 0) return new OptionCheck(ucicommand);
                else if (ucicommand.IndexOf("type spin") >= 0) return new OptionSpin(ucicommand);
                else return new Option(ucicommand);
            }

        }
        /// <summary>
        /// Option, which can be represented as checkbox in a GUI
        /// </summary>
        public class OptionCheck : Option
        {
            internal OptionCheck(string ucicommand) : base(ucicommand) { }

            protected override void Parse(string ucicommand)
            {
                Match m = regexOption.Match(ucicommand);
                if (!m.Success) throw new ArgumentException("Invalid Option Command");
                Name = m.Groups[1].Value.Trim();
                Type = (OptionType)Enum.Parse(typeof(OptionType), m.Groups[2].Value.ToUpperInvariant());
                if (m.Groups.Count > 3 && m.Groups[3].Success)
                    Default = Boolean.Parse(m.Groups[3].Value);
            }
            /// <summary>
            /// The option's default value
            /// </summary>
            public bool Default { internal set; get; }

            private static Regex regexOption = new Regex(@"option\sname\s(\S+\s)+type\s(\S+)(?:\sdefault\s(.*))?");

        }
        /// <summary>
        /// Option for a string field (represented by a text box in a GUI)
        /// </summary>
        public class OptionString : Option
        {
            internal OptionString(string ucicommand) : base(ucicommand) { }

            protected override void Parse(string ucicommand)
            {
                Match m = regexOption.Match(ucicommand);
                if (!m.Success) throw new ArgumentException("Invalid Option Command");
                Name = m.Groups[1].Value.Trim();
                Type = (OptionType)Enum.Parse(typeof(OptionType), m.Groups[2].Value.ToUpperInvariant());
                if (m.Groups.Count > 3 && m.Groups[3].Success)
                    Default = m.Groups[3].Value;
            }
            /// <summary>
            /// The option's default value
            /// </summary>
            public string Default { internal set; get; }

            private static Regex regexOption = new Regex(@"option\sname\s(\S+\s)+type\s(\S+)(?:\sdefault\s(.*))?");

        }
        /// <summary>
        /// An integer engine option
        /// </summary>
        public class OptionSpin : Option
        {
            internal OptionSpin(string ucicommand) : base(ucicommand) { }

            protected override void Parse(string ucicommand)
            {
                base.Parse(ucicommand);

                string[] token = ucicommand.Split();
                for (int i = 0; i <= token.Length - 1; ++i)
                {
                    if (token[i] == "default")
                    {
                        ++i;
                        Default = int.Parse(token[i]);
                    }
                    else if (token[i] == "max")
                    {
                        ++i;
                        Max = int.Parse(token[i]);
                    }
                    else if (token[i] == "min")
                    {
                        ++i;
                        Min = int.Parse(token[i]);
                    }
                }
            }
            /// <summary>
            /// The option's default value
            /// </summary>
            public int Default { internal set; get; }
            /// <summary>
            /// The option's maximum value
            /// </summary>
            public int Max { internal set; get; }
            /// <summary>
            /// The option's minimum value
            /// </summary>
            public int Min { internal set; get; }
        }
        /// <summary>
        /// Multi-value engine option (represented by a combo box in a GUI)
        /// </summary>
        public class OptionCombo : Option
        {
            internal OptionCombo(string ucicommand) : base(ucicommand) { }

            protected override void Parse(string ucicommand)
            {
                base.Parse(ucicommand);

                Values = new();
                string[] token = ucicommand.Split();
                for (int i = 0; i <= token.Length - 1; ++i)
                {
                    if (token[i] == "default")
                    {
                        ++i;
                        Default = token[i];
                    }
                    else if (token[i] == "var")
                    {
                        ++i;
                        Values.Add(token[i]);
                    }
                }
            }

            public string Default { internal set; get; }

            public List<string> Values { internal set; get; }
        }
    }
}
