using Microsoft.AspNetCore.Components;
using PonzianiComponents.Chesslib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
        public NotationType NotatationType { set; get; } = NotationType.SAN;
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

        private Regex regexHeight = new Regex(@"height\:\s*([^;]+)");
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
