using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PonzianiComponents.Chesslib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static PonzianiComponents.Chesslib.UCIEngine;

namespace PonzianiComponents
{
    /// <summary>
    /// Blazor component wrapping an UCI compliant WebAssembly Chess Engine
    /// <para>The component uses the WebAssembly port of Nathan Rugg of ((<see href="https://github.com/nmrugg/stockfish.js"/>) Stockfish 14.1 </para>
    /// <para>NOTE: Stockfish.js 14.1 reqiures some of the latest features and does not work in every browser.<br/>
    /// As it uses the latest WebAssembly threading proposal it requires these HTTP headers on the top level response:
    /// <code>
    /// Cross-Origin-Embedder-Policy: require-corp
    /// Cross-Origin-Opener-Policy: same-origin
    /// </code>
    /// And the following header on the included files:
    /// <code> 
    /// Cross-Origin-Embedder-Policy: require-corp
    /// </code>
    /// </para>
    /// <para>
    /// The UI of the engine component consists of 3 parts:
    /// <list type="bullet">
    /// <item><description>Info Panel showing the engine's evaluation info (score, depth and principal variation)</description></item>
    /// <item><description>Evaluation Bar showing the current evaluation graphically</description></item>
    /// <item><description>Log Panel showing the uci communication</description></item>
    /// </list>
    /// Those 3 UI parts can be activated by the parameters <see cref="ShowEvaluationInfo"/>, <see cref="ShowEvaluationbar"/> and <see cref="ShowLog"/>. 
    /// It's possible to set all 3 of those parameters to false. In that case the engine can be run completely faceless.<br/>
    /// To have more flexibiliyt regarding the UI setup, the evaluation bar and the log panel are offered as independent components: <see cref="EvaluationGauge"/> 
    /// and <see cref="EngineLog"/> 
    /// </para>
    /// <para>
    /// There is only one Engine Process available per window. So by using 2 Engine components, you will not get 2 engine workers, but only one. All UCI commands you
    /// to one of your engine commands will reach the same engine.
    /// </para>
    /// </summary>
    public partial class Engine
    {
        /// <summary>
        /// Shows evaluation output (score, depth and principal variation)
        /// </summary>
        [Parameter]
        public bool ShowEvaluationInfo { set; get; } = true;
        /// <summary>
        /// Shows engine evaluation Bar
        /// </summary>
        /// <summary>
        /// Shows engine log
        /// </summary>
        [Parameter]
        public bool ShowLog { set; get; } = false;
        /// <summary>
        /// Shows engine evaluation Bar
        /// </summary>
        [Parameter]
        public bool ShowEvaluationbar { set; get; } = false;
        /// <summary>
        /// Engine reports new, updated analysis info
        /// </summary>
        [Parameter]
        public EventCallback<Info> OnEngineInfo { get; set; }
        /// <summary>
        /// Number of lines to be analyzed
        /// </summary>
        [Parameter]
        public int NumberOfLines
        {
            get { return _numberOfLines; }
            set
            {
#pragma warning disable CS4014
                if (value != _numberOfLines)
                {
                    Infos.Clear();
                    _numberOfLines = value;
                    for (int i = 0; i < _numberOfLines; ++i) Infos.Add(new Info());
                    if (State == EngineState.READY)

                        SendAsync($"setoption name MultiPV value {_numberOfLines}");
                    else if (State == EngineState.THINKING)
                    {
                        SendAsync($"stop");
                        SendAsync($"setoption name MultiPV value {_numberOfLines}");
                        SendAsync($"go");
                        State = EngineState.THINKING;
                    }

                }
#pragma warning restore CS4014
            }
        }
        /// <summary>
        /// Engine name (only available after engine has been initialized)
        /// </summary>
        public string Name { get; private set; } = string.Empty;
        /// <summary>
        /// Start analysing a position
        /// </summary>
        /// <param name="fen">Position to be analyzed in FEN representation</param>
        /// <returns></returns>
        public async Task<bool> StartAnalysisAsync(string fen)
        {
            if (State >= EngineState.READY)
            {
                await module.InvokeVoidAsync("analyze", new object[] { fen });
                position = new Chesslib.Position(fen);
                State = EngineState.THINKING;
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Other HTML Attributes, which are applied to the root element of the rendered scoresheet.
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object> OtherAttributes { get; set; }

        private IJSObjectReference module;
        private DotNetObjectReference<Engine> objRef;

        private enum EngineState { OFF, INITIALIZING, READY, THINKING }
        private EngineState State = EngineState.OFF;

        private EngineLog Log;

        private int _numberOfLines = 1;
        private List<Info> Infos = new List<Info>() { new Info() };
        private Chesslib.Position position;

        /// <summary>
        /// Score from white's point of view in centipawns. Mate scores are converted to very high scores
        /// </summary>
        public int Score
        {
            get { return GetScore(0); }
        }
        /// <summary>
        /// Score from white's point of view
        /// </summary>
        /// <param name="index">zero-based index of line</param>
        /// <returns>score in centipawns, mate scores are converted to very high scores</returns>
        public int GetScore(int index)
        {
            int factor = position == null || position.SideToMove == Side.WHITE ? 1 : -1;
            if (Infos[index].Type != Info.EvaluationType.Mate)
            {
                int result = factor * Infos[index].Evaluation;
                return result;
            }
            else
            {
                return factor * (int.MaxValue - 2 * Infos[0].MateDistance);
            }
        }

        /// <summary>
        /// Textual representation of Score (with representations for mate scores and upper-/lowerbound scores)
        /// </summary>
        /// <param name="index">zero-based index of line</param>
        /// <returns>Score as text</returns>
        public string ScoreText(int index)
        {
            int factor = position == null || position.SideToMove == Side.WHITE ? 1 : -1;
            if (Infos[index].Type == Info.EvaluationType.Exact) return $"{factor * Infos[index].Evaluation / 100.0}";
            else if (Infos[index].Type == Info.EvaluationType.Mate) return $"#{factor * Infos[index].MateDistance }";
            else if (Infos[index].Type == Info.EvaluationType.Upperbound) return factor > 1 ? $"<= {Infos[index].Evaluation / 100.0}" : $">= {-Infos[index].Evaluation / 100.0}";
            else if (Infos[index].Type == Info.EvaluationType.Lowerbound) return factor > 1 ? $">= {Infos[index].Evaluation / 100.0}" : $"<= {-Infos[index].Evaluation / 100.0}";
            else return String.Empty;
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender || module == null)
            {
                module = await js.InvokeAsync<IJSObjectReference>("import", "./_content/PonzianiComponents/ponziani.js");
                if (module != null)
                {
                    objRef = DotNetObjectReference.Create(this);
                    State = EngineState.INITIALIZING;
                    await module.InvokeVoidAsync("initEngine", new object[] { objRef });
                }
            }
        }

        [JSInvokable]
        public async Task EngineMessageAsync(string msg)
        {
            if (State == EngineState.INITIALIZING && msg == "uciok")
            {
                await SendAsync($"setoption name MultiPV value {_numberOfLines}");
                await SendAsync("isready");
            }
            else if (State == EngineState.INITIALIZING && msg == "readyok")
            {
                State = EngineState.READY;
            }
            else if (State == EngineState.INITIALIZING && msg.StartsWith("id name "))
            {
                Name = msg.Substring(8);
                StateHasChanged();
            }
            else if (msg.StartsWith("info ") && !msg.StartsWith("info string"))
            {
                int index = Info.Index(msg);
                Info info = Infos[index];
                bool evaluationUpdate = info.Update(msg, position != null ? position.SideToMove : Side.WHITE);
                Infos[index] = info;
                if (evaluationUpdate) await OnEngineInfo.InvokeAsync(info);
            }
            else if (State == EngineState.THINKING && msg.StartsWith("bestmove"))
            {
                State = EngineState.READY;
            }
        }
        /// <summary>
        /// Send UCI message to the engine process
        /// <para>Attention: Sending an incorrect or invalid UCI message might break the engine process, there is no input validation!</para>
        /// </summary>
        /// <param name="command">UCI message</param>
        /// <returns></returns>
        public async Task SendAsync(string command)
        {
            if (Log != null) Log.AddEngineInputMessage(command);
            await module.InvokeVoidAsync("send", new object[] { command });
        }

        private string PVToSAN(string pv)
        {
            if (position == null) return String.Empty;
            Game game = new Game(position.FEN);
            string[] moves = pv.Split();
            foreach (string move in moves) game.Add(new ExtendedMove(move));
            return game.SANNotation();
        }
    }
}
