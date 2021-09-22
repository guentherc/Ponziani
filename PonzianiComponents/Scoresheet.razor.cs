using Microsoft.AspNetCore.Components;
using PonzianiComponents.Chesslib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using System.Globalization;

namespace PonzianiComponents
{
    /// <summary>
    /// Blazor component to list the moves from a chess game
    /// <para>It offers <see cref="DisplayMode">2 display modes, Tabular and Inline</see></para>
    /// <para>You can select if moves shall be outputted in <see cref="NotationType.SAN">SAN-</see> or in
    /// <see cref="NotationType.UCI">UCI-Notation</see> (as used in UCI protocol for engines)</para>
    /// <para>The component is able to output <see cref="Comments">comments</see> and <see cref="Variations">variations</see></para>
    /// </summary>
    public partial class Scoresheet
    {
        /// <summary>
        /// Display Mode for Move List
        /// </summary>
        public enum DisplayMode
        {
            /// <summary>
            /// Moves are arranged in a table with one column for white's moves and one cloumn for black's moves
            /// </summary>
            TABULAR,
            /// <summary>
            /// Moves are arranged in continous text (like in PGN). 
            /// <example>
            /// 1. e4 c5 2. Nf3 d6 3. d4 cxd4 4. Nxd4 Nf6 5. Nc3 a6
            /// </example>
            /// </summary>
            INLINE
        };
        /// <summary>
        /// The output format used for the moves
        /// </summary>
        public enum NotationType
        {
            /// <summary>
            /// Standard algebraic notation <example>Nf3, e3, O-O</example>
            /// </summary>
            SAN,
            /// <summary>
            /// Notation as used in UCI protocol <see href="http://wbec-ridderkerk.nl/html/UCIProtocol.html"/> <example>g1f3, e2e3, e8g8</example>
            /// </summary>
            UCI
        };
        /// <summary>
        /// Id of the rendered HTML element
        /// </summary>
        [Parameter]
        public string Id { get; set; } = "scoresheet";
        /// <summary>
        /// The game whose moves are listed in the scoresheet
        /// </summary>
        [Parameter]
        public Game Game { set; get; } = new Game();
        /// <summary>
        /// Display Mode (Tabular or inline)
        /// </summary>
        [Parameter]
        public DisplayMode Mode { set; get; } = DisplayMode.TABULAR;
        /// <summary>
        /// Notation Type
        /// </summary>
        [Parameter]
        public NotationType Type { set; get; } = NotationType.SAN;
        /// <summary>
        /// If true and extended move info is available (think time, evaluation or depth) this info will be outputted in tabular mode
        /// </summary>
        [Parameter]
        public bool ExtendedMoveInfo { set; get; } = false;
        /// <summary>
        /// If true comments are displayed
        /// </summary>
        [Parameter]
        public bool Comments { set; get; } = false;
        /// <summary>
        /// If true, variations are displayed
        /// </summary>
        [Parameter]
        public bool Variations { set; get; } = false;
        /// <summary>
        /// Text color for comments in inline modes and for comments within variations in tabular mode
        /// </summary>
        [Parameter]
        public string ColorCommentText { set; get; }
        /// <summary>
        /// Background color for comments in tabular mode
        /// </summary>
        [Parameter]
        public string ColorCommentBackground { set; get; }
        /// <summary>
        /// Background color for variations in tabular mode
        /// </summary>
        [Parameter]
        public string ColorVariationBackground { set; get; }
        /// <summary>
        /// Is called whenever the user selects a move by clicking it 
        /// </summary>
        [Parameter]
        public EventCallback<MoveSelectInfo> OnMoveSelected { get; set; }
        /// <summary>
        /// If true, each variation starts on a new line, subvariations are indented
        /// </summary>
        [Parameter]
        public bool HierarchicalDisplay { set; get; } = false;
        /// <summary>
        /// ISO 639-1 two-letter language code 
        /// </summary>
        [Parameter]
        public string Language { set; get; } = "en";
        /// <summary>
        /// <para>Other HTML Attributes, which are applied to the root element of the rendered scoresheet. Depending
        /// on <see cref="Mode"/> this is either a &lt;table&gt; or a &lt;div&gt; </para>
        /// <para>With this mechanism it's possible e.g. to set the width of the scoresheet (in inline mode)</para>
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object> OtherAttributes { get; set; }
        /// <summary>
        /// API Method to add a move to the scoresheet
        /// </summary>
        /// <param name="move">Move to be added</param>
        /// <returns>true, if move was added successful, false if not</returns>
        public async Task<bool> AddMoveAsync(ExtendedMove move)
        {
            bool result = Game.Add(move);
            //await resultElement.FocusAsync();
            StateHasChanged();
            return result;
        }
        /// <summary>
        /// API Method to add a move to the scoresheet
        /// </summary>
        /// <param name="move">Move to be added</param>
        /// <returns>true, if move was added successful, false if not</returns>
        public async Task<bool> AddMoveAsync(Move move)
        {
            bool result = Game.Add(new ExtendedMove(move));
            //await resultElement.FocusAsync();
            StateHasChanged();
            return result;
        }

        protected override async Task OnParametersSetAsync()
        {
            CultureInfo ci = null;
            try
            {
                ci = Language == null ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo(Language);
            }
            catch (CultureNotFoundException)
            {
                ci = CultureInfo.InvariantCulture;
            }
            if (!Chess.PieceChars.ContainsKey(ci)) ci = CultureInfo.InvariantCulture;
            chessPieceStringProvider = Chess.PieceChars[ci];
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender || module == null)
            {
                module = await js.InvokeAsync<IJSObjectReference>("import", "./_content/PonzianiComponents/ponziani.js");
            }
            await StyleAsync();
            if (!firstRender && module != null && _tbody.Context != null && Mode == DisplayMode.TABULAR && scrolledMoveNumber != Game.Position.MoveNumber)
            {
                await module.InvokeVoidAsync("scrollToBottom", new object[] { _tbody });
                scrolledMoveNumber = Game.Position.MoveNumber;
            }
        }

        private async Task StyleAsync()
        {
            var el = Mode == DisplayMode.INLINE ? _div : _tbody;
            if (ColorCommentText != null)
                await module.InvokeVoidAsync("setCSSProperty", new object[] { el, ".pzMoveTextComment", "--move_text_comment_color", ColorCommentText });
            if (ColorCommentBackground != null)
                await module.InvokeVoidAsync("setCSSProperty", new object[] { el, ".pzComment", "--comment_background_color", ColorCommentBackground });
            if (ColorVariationBackground != null)
                await module.InvokeVoidAsync("setCSSProperty", new object[] { el, ".pzVariation", "--variation_background_color", ColorVariationBackground });
        }

        private bool EvaluationAvailable => Game.Moves.FindIndex(m => m.Evaluation != 0) >= 0;
        private bool ThinkTimeAvailable => Game.Moves.FindIndex(m => m.UsedThinkTime > TimeSpan.Zero) >= 0;
        private bool DepthAvailable => Game.Moves.FindIndex(m => m.Depth > 0) >= 0;
        private IChessPieceStringProvider chessPieceStringProvider = null;

        private async Task SelectMoveAsync(EventArgs eventArgs, int moveNumber, Side side)
        {
            MoveSelectInfo msi = new(Id, Game.GetPosition(moveNumber, side), Game.GetMove(moveNumber, side));
            await OnMoveSelected.InvokeAsync(msi);
        }

        private string Height()
        {
            if (OtherAttributes != null && OtherAttributes.ContainsKey("style"))
            {
                Match m = regexHeight.Match((string)OtherAttributes["style"]);
                if (m.Success) return $"{m.Value.Trim()};";
            }
            return "";
        }

        private string Print(Game g, int moveIndex)
        {
            StringBuilder sb;
            switch (Type)
            {
                case NotationType.SAN:
                    sb = new StringBuilder(g.Position.ToSAN(Game.Moves[moveIndex], chessPieceStringProvider));
                    break;
                case NotationType.UCI:
                    return Game.Moves[moveIndex].ToUCIString();
                default:
                    return g.Position.ToSAN(Game.Moves[moveIndex]);
            }
            if (Mode == DisplayMode.TABULAR && ExtendedMoveInfo)
            {
                List<string> sbs = new List<string>();
                sb.Append(" (");
                if (ThinkTimeAvailable) sbs.Add(Game.Moves[moveIndex].UsedThinkTime.ToString(@"m\:ss"));
                if (DepthAvailable) sbs.Add(Game.Moves[moveIndex].Depth.ToString());
                if (EvaluationAvailable) sbs.Add(Game.Moves[moveIndex].Evaluation.ToString());
                for (int i = 0; i < sbs.Count; ++i)
                {
                    if (i > 0) sb.Append(" ");
                    sb.Append(sbs[i]);
                }
                sb.Append(")");
            }
            return sb.ToString();
        }

        private Regex regexHeight = new Regex(@"height\:\s*([^;]+)");
        private IJSObjectReference module = null;
        private int scrolledMoveNumber = 0;
        private ElementReference _tbody;
        private ElementReference _div;

    }

    public class MoveSelectInfo
    {
        public MoveSelectInfo(string scoresheetId, Position position, ExtendedMove move)
        {
            ScoresheetId = scoresheetId;
            Position = position;
            Move = move;
        }

        /// <summary>
        /// Id of the scoresheet, where the move was selected (needed in multiboard scenarios)
        /// </summary>
        public string ScoresheetId { set; get; }
        /// <summary>
        /// Position object corresponding to the situation before the selected move has been played
        /// </summary>
        public Position Position { set; get; }
        /// <summary>
        /// The selected Move
        /// </summary>
        public ExtendedMove Move { set; get; }
    }

    internal static class StringExtensions
    {
        public static string Repeat(this string s, int n)
        {
            return new StringBuilder(s.Length * n)
                            .AppendJoin(s, new string[n + 1])
                            .ToString();
        }
    }

}
