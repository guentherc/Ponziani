using Microsoft.JSInterop;
using PonzianiComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PonzianiDemo.Pages
{
    public partial class Multiple
    {
        public string Fen1 { set; get; } = "rnbqkb1r/1p2pppp/p2p1n2/8/3NP3/2N5/PPP2PPP/R1BQKB1R w KQkq - 0 6";
        public string Fen2 { set; get; } = "r1bqkb1r/1ppp1ppp/p1n2n2/4p3/B3P3/5N2/PPPP1PPP/RNBQ1RK1 b kq - 3 5";
        public string Fen3 { set; get; } = "rnbqk2r/ppp1ppbp/3p1np1/8/2PPP3/2N5/PP3PPP/R1BQKBNR w KQkq - 0 5";

        public void OnMovePlayed(MovePlayedInfo mpi)
        {
            int indx = int.Parse(mpi.BoardId.Substring(5));
            switch (indx)
            {
                case 1:
                    Fen1 = mpi.NewFen; break;
                case 2:
                    Fen2 = mpi.NewFen; break;
                case 3:
                    Fen3 = mpi.NewFen; break;
            }
        }
    }
}
