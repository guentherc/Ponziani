using Microsoft.AspNetCore.Components.Forms;
using PonzianiComponents;
using PonzianiComponents.Chesslib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PonzianiDemo.Pages
{
    public class SDModel
    {
        public string PGNText { set; get; }
        public int Height { set; get; } = 400;
        public bool InlineMode { set; get; } = false;
        public string OtherAttributes { set; get; } = @"style=""width: 800px; height: 400px""";
    }

    public partial class ScoresheetDemo
    {
        public string PGN
        {
            set
            {
                var games = PonzianiComponents.Chesslib.PGN.Parse(value);
                if (games != null && games.Count > 0) game = games[0]; else game = new Game();
            }
            get { return game.ToPGN(); }
        }

        public Scoresheet.DisplayMode DisplayMode => model.InlineMode ? Scoresheet.DisplayMode.INLINE : Scoresheet.DisplayMode.TABULAR;

        private string EventInfoText { set; get; } = "";

        private static Regex regexOtherAttributes = new Regex(@"(\w+)=\""([^\""]+)\""");
        private SDModel model { set; get; } = new SDModel();
        private Game game = new Game();
        private Dictionary<string, object> OtherAttributes
        {
            get
            {
                Dictionary<string, object> oo = new Dictionary<string, object>();
                MatchCollection mc = regexOtherAttributes.Matches(model.OtherAttributes);
                foreach (Match m in mc)
                {
                    oo.Add(m.Groups[1].Value, m.Groups[2].Value);
                }
                return oo;
            }
        }

        private void HandleValidSubmit()
        {
            PGN = model.PGNText;
        }

        private void OnMoveSelected(MoveSelectInfo msi)
        {
            EventInfoText = $"OnMoveSelected({JsonSerializer.Serialize(msi)})";
        }
    }
}
