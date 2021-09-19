using Microsoft.AspNetCore.Components;
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
    public class Model
    {
        public bool ShowCoordinates { set; get; } = true;
        public int Size { set; get; }
        public bool Rotate { set; get; } = false;
        public string Fen { set; get; } = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        public bool HighlightLastMove { set; get; } = false;
        public string OtherAttributes { set; get; }
        public string ColorDarkSquares { set; get; }
        public string ColorLightSquares { set; get; }
        public string HighlightColor { set; get; }
    }

    public partial class Index
    {
        private Model model = new Model();
        private string EventInfoText { set; get; } = "";
        private Chessboard chessboard;
        private bool SetupMode { 
            set { if (value && value != chessboard.SetupMode) chessboard.SwitchToSetupMode(); else chessboard.ExitSetupMode(); }
            get { return chessboard != null && chessboard.SetupMode; } 
        }

        private static Regex regexOtherAttributes = new Regex(@"(\w+)=\""([^\""]+)\""");
        private Dictionary<string, object> OtherAttributes
        {
            get
            {
                Dictionary<string, object> oo = new Dictionary<string, object>();
                if (model.OtherAttributes != null)
                {
                    MatchCollection mc = regexOtherAttributes.Matches(model.OtherAttributes);
                    foreach (Match m in mc)
                    {
                        oo.Add(m.Groups[1].Value, m.Groups[2].Value);
                    }
                }
                return oo;
            }
        }

        private void OnMovePlayed(MovePlayedInfo mpi)
        {
            EventInfoText = $"OnMovePlayed({JsonSerializer.Serialize(mpi)})";
            model.Fen = mpi.NewFen;
        }

        private void OnSetupChanged(SetupChangedInfo sci)
        {
            EventInfoText = $"OnSetupChanged({JsonSerializer.Serialize(sci)})";
            model.Fen = sci.NewFen;
        }

        private void ApplyMove(ChangeEventArgs e)
        {
            chessboard.ApplyMove(new Move(e.Value.ToString()));
            model.Fen = chessboard.Fen;
        }

        private void HandleSetupModeChanged(bool isChecked)
        {
            Console.WriteLine(isChecked);
        }
    }
}
