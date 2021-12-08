using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PonzianiComponents
{
    public enum Orientation { Horizontal, Vertical }
    /// <summary>
    /// Engine Evaluation Bar
    /// </summary>
    public partial class EvaluationGauge
    {
        /// <summary>
        /// Score in centipawns from white's point of view
        /// </summary>
        [Parameter]
        public int Score { get; set; } = 0;
        /// <summary>
        /// Orientation of Evaluation bar
        /// </summary>
        [Parameter]
        public Orientation Orientation{ get; set; } = Orientation.Horizontal;

        /// <summary>
        /// Text representation for Score
        /// </summary>
        [Parameter]
        public string ScoreText { get; set; } = string.Empty;

        /// <summary>
        /// Other HTML Attributes, which are applied to the root element of the rendered scoresheet.
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object> OtherAttributes { get; set; }

        private string Width => FormattableString.Invariant($"{100 * WinExpectation():F1}%");

        private string Height => FormattableString.Invariant($"{100 - (100 * WinExpectation()):F1}%");

        private double WinExpectation() => 1 / (1 + Math.Pow(10, -1.305 / 400 * Score));

        private static double WinExpectation(int score) => 1.0 / (1 + Math.Pow(10, -1.305 / 400 * score));

        private static MarkupString _tickbarSVg;
        private static MarkupString _tickbarSVgV;
        private static MarkupString TickBarSVG()
        {
            if (_tickbarSVg.Value == null)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<svg style=\"width:100%;\">");
                for (int score = -400; score <= 400; score += 100)
                {
                    string label = FormattableString.Invariant($"{100 * WinExpectation(score):F1}%");
                    sb.Append($"<line x1=\"{label}\" y1=\"0%\" x2=\"{label}\" y2=\"5%\" style=\"stroke: rgb(0, 0, 0); stroke-width:2\"/>");
                    sb.Append($"<text x=\"{label}\" y=\"15%\" text-anchor=\"middle\">{score / 100}</text>");
                }
                sb.Append("</svg>");
                _tickbarSVg = new MarkupString(sb.ToString());
            }
            return _tickbarSVg;
        }

        private static MarkupString TickBarSVGV()
        {
            if (_tickbarSVgV.Value == null)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<svg style=\"width:100%;height:100%\">");
                for (int score = -400; score <= 400; score += 100)
                {
                    string label = FormattableString.Invariant($"{100 - (100 * WinExpectation(score)):F1}%");
                    sb.Append($"<line x1=\"70%\" y1=\"{label}\" x2=\"100%\" y2=\"{label}\" style=\"stroke: rgb(0, 0, 0); stroke-width:2\"/>");
                    sb.Append($"<text x=\"50%\" y=\"{label}\" text-anchor=\"end\">{score / 100}</text>");
                }
                sb.Append("</svg>");
                _tickbarSVgV = new MarkupString(sb.ToString());
            }
            return _tickbarSVgV;
        }

    }
}
