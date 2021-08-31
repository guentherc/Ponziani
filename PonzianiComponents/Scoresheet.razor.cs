using Microsoft.AspNetCore.Components;
using PonzianiComponents.Chesslib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        /// The Height (in pixels) of the scoresheet (default 400)
        /// </summary>
        [Parameter]
        public int Height { get; set; } = 400;

    }
}
