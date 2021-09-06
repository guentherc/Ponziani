using Microsoft.AspNetCore.Components;
using PonzianiComponents.Chesslib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace PonzianiComponents
{
    public partial class Scoresheet
    {
        /// <summary>
        /// Display Mode for Move List
        /// </summary>
        public enum DisplayMode {
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
            INLINE };
        /// <summary>
        /// The output format used for the moves
        /// </summary>
        public enum NotationType { 
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
        /// Is called whenever the user selects a move by clicking it 
        /// </summary>
        [Parameter]
        public EventCallback<MoveSelectInfo> OnMoveSelected { get; set; }
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

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender || module == null)
            {
                module = await js.InvokeAsync<IJSObjectReference>("import", "./_content/PonzianiComponents/ponziani.js");
            }
            if (!firstRender && module != null && _tbody.Context != null && Mode == DisplayMode.TABULAR && scrolledMoveNumber != Game.Position.MoveNumber)
            {
                await module.InvokeVoidAsync("scrollToBottom", new object[] { _tbody });
                scrolledMoveNumber = Game.Position.MoveNumber;
            }
        }

        private async Task SelectMoveAsync(EventArgs eventArgs, int moveNumber, Side side)
        {
            MoveSelectInfo msi = new( Id, Game.GetPosition(moveNumber, side), Game.GetMove(moveNumber, side) );
            await OnMoveSelected.InvokeAsync(msi);
        }

        private string Height()
        {
            if (OtherAttributes.ContainsKey("style"))
            {
                Match m = regexHeight.Match((string)OtherAttributes["style"]);
                if (m.Success) return $"{m.Value.Trim()};";
            }
            return "";
        }

        private string Print(Game g, int moveIndex)
        {
            switch (Type)
            {
                case NotationType.SAN:
                    return g.Position.ToSAN(Game.Moves[moveIndex]);
                case NotationType.UCI:
                    return Game.Moves[moveIndex].ToUCIString();
                default:
                    return g.Position.ToSAN(Game.Moves[moveIndex]);
            }

        }

        private Regex regexHeight = new Regex(@"height\:\s*([^;]+)");
        private IJSObjectReference module = null;
        private int scrolledMoveNumber = 0;
        private ElementReference _tbody;

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


}
