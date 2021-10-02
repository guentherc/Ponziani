using PonzianiComponents;
using PonzianiComponents.Chesslib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PonzianiDemo.Pages
{
    public partial class Puzzles
    {
        private List<Tuple<string, string>> PuzzleData = null;
        private int PuzzleIndex = 0;
        private Chessboard CB;
        private Side Side => (Game.Position.SideToMove == Side.WHITE) == ((Game.Moves.Count & 1) == 0) ? Side.BLACK : Side.WHITE;
        private bool Rotate => Side == Side.BLACK;
        private string Message = string.Empty;
        private string StatusMessage = string.Empty;
        private string StatusClass = "alert alert-success";
        private bool PuzzleDone = false;
        private int Solved = 0;
        private int Errors = 0;
        List<Move> WrongMoves;


        public Game Game { set; get; } = new Game();

        protected override async Task OnInitializedAsync()
        {
            if (PuzzleData == null)
            {
                PuzzleIndex = 0;
                var data = await HttpClient.GetStringAsync("puzzles.txt");
                PuzzleData = new();
                string[] lines = data.Split('\n');
                foreach (string line in lines)
                {
                    string[] parts = line.Split('|');
                    if (parts.Length == 2)
                    {
                        PuzzleData.Add(new (parts[0], parts[1]));
                    }
                }
                PuzzleData = PuzzleData.OrderBy(x => Guid.NewGuid()).ToList();
                Game = new Game(PuzzleData[PuzzleIndex].Item1);
                string[] sides = { "white", "black" };
                Message = $"You are playing with {sides[(int)Side]} - Find the best move!";
                WrongMoves = new();
            }
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (PuzzleData != null && Game.Moves.Count == 0)
            {
                string[] moves = PuzzleData[PuzzleIndex].Item2.Split(' ');
                Move move = new(moves[0]);
                Game.Add(new ExtendedMove(move));
                CB.ApplyMove(move);
            }
        }

        private void OnMoveSelected(MoveSelectInfo msi)
        {
        }

        private void NextPuzzle()
        {
            PuzzleIndex++;
            Game = new Game(PuzzleData[PuzzleIndex].Item1);
            StatusMessage = string.Empty;
            PuzzleDone = false;
            Errors = 0; 
            string[] sides = { "white", "black" };
            Message = $"You are playing with {sides[(int)Side]} - Find the best move!";
            WrongMoves = new();
        }

        private void ShowSolution()
        {
            if (Game.Position.SideToMove == Side)
            {
                StatusMessage = "Not solved!";
                StatusClass = "alert alert-warning";
                string[] moves = PuzzleData[PuzzleIndex].Item2.Split(' ');
                Move solution = new Move(moves[Game.Moves.Count]);
                Errors += 1;
                string solutionMove = Game.Position.ToSAN(solution);
                Game.Add(new ExtendedMove(solution));
                Game.Moves.Last().Variations = new();
                foreach (Move m in WrongMoves)
                {
                    Game.Moves.Last().Variations.Add(new() { new ExtendedMove(m) });
                }
                CB.ApplyMove(solution);
                if (moves.Length > Game.Moves.Count)
                {
                    Message = $"Best Move is { solutionMove }";
                    Move next = new Move(moves[Game.Moves.Count]);
                    CB.ApplyMove(next);
                    Game.Add(new ExtendedMove(next));
                }
                else
                {
                    PuzzleDone = true;
                    Message = $"Exercise done with {Errors} errors";
                }
                WrongMoves.Clear();
            }
        }

        private void OnMovePlayed(MovePlayedInfo mpi)
        {
            if (Game.Position.SideToMove == Side)
            {
                string[] moves = PuzzleData[PuzzleIndex].Item2.Split(' ');
                if (moves.Length > Game.Moves.Count) {
                    Move solution = new Move(moves[Game.Moves.Count]);
                    if (solution.Equals(mpi.Move))
                    {
                        StatusClass = Errors > 0 ? "alert alert-warning" : "alert alert-success";
                        if (moves.Length - 1 <= Game.Moves.Count)
                        {
                            StatusMessage = Errors > 0 ? "Done!" : "Solved!";
                            Game.Add(new ExtendedMove(solution));
                            Game.Moves.Last().Variations = new();
                            foreach (Move m in WrongMoves)
                            {
                                Game.Moves.Last().Variations.Add(new() { new ExtendedMove(m) });
                            }
                            PuzzleDone = true;
                            if (Errors > 0) Message = $"Exercise done with {Errors} errors"; else
                            {
                                Message = "Congratulations - you solved this puzzle";
                                ++Solved;
                            }
                        } else
                        {
                            string[] sides = { "Black", "White" };
                            string playedMove = Game.Position.ToSAN(solution);
                            StatusMessage = $"{playedMove} is the best move!";
                            Game.Add(new ExtendedMove(solution));
                            Game.Moves.Last().Variations = new();
                            foreach (Move m in WrongMoves)
                            {
                                Game.Moves.Last().Variations.Add(new() { new ExtendedMove(m) });
                            }
                            Move next = new Move(moves[Game.Moves.Count]);
                            string nextMove = Game.Position.ToSAN(next);
                            CB.ApplyMove(next);
                            Game.Add(new ExtendedMove(next));
                            Message = $"{playedMove} is correct! {sides[(int)Side]} replied {nextMove}! What is the best continuation?";
                        }
                        WrongMoves.Clear();

                    } else
                    {
                        StatusClass = "alert alert-danger";
                        Message = $"{Game.Position.ToSAN(mpi.Move)} is not the best move - try again!";
                        StatusMessage = $"{Game.Position.ToSAN(mpi.Move)} is not the best move!";
                        WrongMoves.Add(mpi.Move);
                        Errors++;
                    }
                }
            } else 
                Game.Add(new ExtendedMove(mpi.Move));
        }
    }
}
