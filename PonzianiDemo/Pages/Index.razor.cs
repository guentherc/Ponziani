using Microsoft.AspNetCore.Components;
using PonzianiComponents;
using PonzianiComponents.Chesslib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace PonzianiDemo.Pages
{
    public class Model
    {
        public bool ShowCoordinates { set; get; } = true;
        public int Size { set; get; } = 400;
        public bool Rotate { set; get; } = false;
        public string Fen { set; get; } = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        public bool HighlightLastMove { set; get; } = false;
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
