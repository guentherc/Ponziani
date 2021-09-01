using Microsoft.AspNetCore.Components.Forms;
using PonzianiComponents;
using PonzianiComponents.Chesslib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace PonzianiDemo.Pages
{
    public class SDModel
    {
        public string PGNText { set; get; }
        public int Height { set; get; } = 400;
        public bool InlineMode { set; get; } = false;
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

        private SDModel model { set; get; } = new SDModel();
        private Game game = new Game();

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
