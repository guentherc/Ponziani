using Microsoft.VisualStudio.TestTools.UnitTesting;
using PonzianiComponents.Chesslib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PonzianiComponentsTest
{
    [TestClass]
    public class GameTest
    {
        [TestMethod]
        public void TestConstructor()
        {
            Game game = new Game();
            Assert.IsNull(game.White);
            Assert.IsNull(game.Black);
            Assert.IsNull(game.Event);
            Assert.IsNull(game.Site);
            Assert.IsNull(game.Date);
            Assert.IsNull(game.Round);
            Assert.IsNull(game.Eco);
            Assert.AreEqual(Fen.INITIAL_POSITION, game.StartPosition);
            Assert.AreEqual(Fen.INITIAL_POSITION, game.Position.FEN);
            Assert.AreEqual(Result.OPEN, game.Result);
            Assert.AreEqual(ResultDetail.UNKNOWN, game.ResultDetail);
            Assert.AreEqual(Side.WHITE, game.SideToMove);
            Assert.AreEqual(0, game.Moves.Count);
            Assert.AreEqual(0, game.Tags.Count);
            string fen = "rnbqr1k1/pp3pbp/3p1np1/2pP4/4P3/2N2N2/PPQ1BPPP/R1B2RK1 b - - 7 10";
            game = new Game(fen);
            Assert.IsNull(game.White);
            Assert.IsNull(game.Black);
            Assert.IsNull(game.Event);
            Assert.IsNull(game.Site);
            Assert.IsNull(game.Date);
            Assert.IsNull(game.Round);
            Assert.IsNull(game.Eco);
            Assert.AreEqual(fen, game.StartPosition);
            Assert.AreEqual(fen, game.Position.FEN);
            Assert.AreEqual(Result.OPEN, game.Result);
            Assert.AreEqual(ResultDetail.UNKNOWN, game.ResultDetail);
            Assert.AreEqual(Side.BLACK, game.SideToMove);
            Assert.AreEqual(0, game.Moves.Count);
            Assert.AreEqual(0, game.Tags.Count);
        }

        [TestMethod]
        public void PlayLegalTrap()
        {
            Game game = new Game();
            string[] moves = "e2e4 e7e5 g1f3 b8c6 f1c4 d7d6 b1c3 c8g4 h2h3 g4h5 f3e5 h5d1 c4f7 e8e7 c3d5".Split(' ');
            foreach (string m in moves) game.Add(new ExtendedMove(m));
            Assert.AreEqual(Result.WHITE_WINS, game.Result);
            Assert.AreEqual(ResultDetail.MATE, game.ResultDetail);
            Assert.AreEqual(moves.Length, game.Moves.Count);
            Assert.AreEqual(Fen.INITIAL_POSITION, game.StartPosition);
            Assert.AreEqual("r2q1bnr/ppp1kBpp/2np4/3NN3/4P3/7P/PPPP1PP1/R1BbK2R b KQ - 2 8", game.Position.FEN);
        }

        [TestMethod]
        public void TestToPGN()
        {
            List<Game> games = PGN.Parse(Data.PGN_TWIC);
            List<Game> copiedGames = new List<Game>();
            foreach (var game in games)
            {
                Game ngame = new Game(game.StartPosition);
                ngame.White = game.White;
                ngame.Black = game.Black;
                ngame.Event = game.Event;
                ngame.Site = game.Site;
                foreach (var key in game.Tags.Keys) ngame.Tags.Add(key, game.Tags[key]);
                ngame.Date = game.Date;
                ngame.Result = game.Result;
                ngame.ResultDetail = game.ResultDetail;
                ngame.Round = game.Round;
                foreach (var m in game.Moves) ngame.Add(m);
                copiedGames.Add(ngame);
            }
            for (int i = 0; i < copiedGames.Count; ++i)
            {
                Game ngame = PGN.Parse(copiedGames[i].ToPGN()).First();
                Game game = games[i];
                Assert.AreEqual(game.White, ngame.White);
                Assert.AreEqual(game.Black, ngame.Black);
                Assert.AreEqual(game.Event, ngame.Event);
                Assert.AreEqual(game.Site, ngame.Site);
                Assert.AreEqual(game.Date, ngame.Date);
                Assert.AreEqual(game.Result, ngame.Result);
                Assert.AreEqual(game.ResultDetail, ngame.ResultDetail);
                Assert.AreEqual(game.Round, ngame.Round);
                Assert.IsTrue(game.Tags.Count <= ngame.Tags.Count);
                foreach (var key in game.Tags.Keys) Assert.AreEqual(game.Tags[key], ngame.Tags[key]);
                Assert.AreEqual(game.Moves.Count, ngame.Moves.Count);
                for (int j = 0; j < game.Moves.Count; ++j) Assert.AreEqual(game.Moves[j].ToUCIString(), ngame.Moves[j].ToUCIString());
            }
        }

        [TestMethod]
        public void TestGetPosition()
        {
            Game game = new Game();
            string[] moves = "e2e4 e7e5 g1f3 b8c6 f1c4 d7d6 b1c3 c8g4 h2h3 g4h5 f3e5 h5d1 c4f7 e8e7 c3d5".Split(' ');
            foreach (string m in moves) game.Add(new ExtendedMove(m));
            Position pos1 = game.GetPosition(4, Side.BLACK);
            Position pos2 = new Position("r1bqkbnr/ppp2ppp/2np4/4p3/2B1P3/2N2N2/PPPP1PPP/R1BQK2R b KQkq - 1 4");
            Assert.AreEqual(pos2.PolyglotKey, pos1.PolyglotKey);
            pos1 = game.GetPosition(5, Side.WHITE);
            pos2 = new Position("r2qkbnr/ppp2ppp/2np4/4p3/2B1P1b1/2N2N2/PPPP1PPP/R1BQK2R w KQkq - 2 5");
            Assert.AreEqual(pos2.PolyglotKey, pos1.PolyglotKey);
        }

        [TestMethod]
        public void TestGetMove()
        {
            Game game = new Game();
            string[] moves = "e2e4 e7e5 g1f3 b8c6 f1c4 d7d6 b1c3 c8g4 h2h3 g4h5 f3e5 h5d1 c4f7 e8e7 c3d5".Split(' ');
            foreach (string m in moves) game.Add(new ExtendedMove(m));
            Assert.AreEqual("c8g4", game.GetMove(4, Side.BLACK).ToUCIString());
            Assert.AreEqual("h2h3", game.GetMove(5, Side.WHITE).ToUCIString());
        }

        [TestMethod]
        public void TestUndoMove()
        {
            List<string> pgns = new List<string>()
            {
                Data.PGN_CCRL_CHESSGUI_GAME,
                Data.PGN_CHESS24,
                Data.PGN_CHESS_RESULTS,
                Data.PGN_CUTECHESS,
                Data.PGN_LICHESS_COMMENTED_GAME,
                Data.PGN_LICHESS_LIVE_GAME,
                Data.PGN_LICHESS_STUYDY,
                Data.PGN_SCID,
                Data.PGN_TCEC,
                Data.PGN_TWIC,
                Data.PGN_3_EP_CAPTURES,
                Data.PGN_UNDERPROMOTION
            };
            foreach (var pgn in pgns)
            {
                var games = PGN.Parse(pgn);
                foreach (Game game in games)
                {
                    var key = (new Position(game.StartPosition)).PolyglotKey;
                    while (game.Moves.Count > 0)
                    {
                        game.UndoLastMove();
                    }
                    Assert.AreEqual(game.StartPosition, game.Position.FEN);
                    Assert.AreEqual(key, game.Position.PolyglotKey);
                }
            }
        }
    }

    [TestClass]
    public class TimeControlTest
    {
        [TestMethod]
        public void TestConstructor()
        {
            TimeControl tc = new TimeControl("300");
            Assert.AreEqual(1, tc.Controls[0].From);
            Assert.AreEqual(int.MaxValue, tc.Controls[0].To);
            Assert.AreEqual(TimeSpan.FromMinutes(5), tc.Controls[0].Time);
            Assert.AreEqual(TimeSpan.Zero, tc.Controls[0].Increment);
            tc = new TimeControl("40/300");
            Assert.AreEqual(1, tc.Controls[0].From);
            Assert.AreEqual(40, tc.Controls[0].To);
            Assert.AreEqual(TimeSpan.FromMinutes(5), tc.Controls[0].Time);
            Assert.AreEqual(TimeSpan.Zero, tc.Controls[0].Increment);
            tc = new TimeControl("40/300+10");
            Assert.AreEqual(1, tc.Controls[0].From);
            Assert.AreEqual(40, tc.Controls[0].To);
            Assert.AreEqual(TimeSpan.FromMinutes(5), tc.Controls[0].Time);
            Assert.AreEqual(TimeSpan.FromSeconds(10), tc.Controls[0].Increment);
            tc = new TimeControl("40/300+0.5");
            Assert.AreEqual(1, tc.Controls[0].From);
            Assert.AreEqual(40, tc.Controls[0].To);
            Assert.AreEqual(TimeSpan.FromMinutes(5), tc.Controls[0].Time);
            Assert.AreEqual(TimeSpan.FromSeconds(0.5), tc.Controls[0].Increment);
            tc = new TimeControl("180+2");
            Assert.AreEqual(1, tc.Controls[0].From);
            Assert.AreEqual(int.MaxValue, tc.Controls[0].To);
            Assert.AreEqual(TimeSpan.FromMinutes(3), tc.Controls[0].Time);
            Assert.AreEqual(TimeSpan.FromSeconds(2), tc.Controls[0].Increment);
            tc = new TimeControl("40/5400+30:3600+30");
            Assert.AreEqual(1, tc.Controls[0].From);
            Assert.AreEqual(40, tc.Controls[0].To);
            Assert.AreEqual(TimeSpan.FromMinutes(90), tc.Controls[0].Time);
            Assert.AreEqual(TimeSpan.FromSeconds(30), tc.Controls[0].Increment);
            Assert.AreEqual(41, tc.Controls[1].From);
            Assert.AreEqual(int.MaxValue, tc.Controls[1].To);
            Assert.AreEqual(TimeSpan.FromMinutes(60), tc.Controls[1].Time);
            Assert.AreEqual(TimeSpan.FromSeconds(30), tc.Controls[1].Increment);
        }

        [TestMethod]
        public void TestTotalAvailableTime()
        {
            TimeControl tc = new TimeControl("300");
            Assert.AreEqual(TimeSpan.FromMinutes(5), tc.TotalAvailableTime(10));
            Assert.AreEqual(TimeSpan.FromMinutes(5), tc.TotalAvailableTime(100));
            Assert.AreEqual(TimeSpan.FromMinutes(5), tc.TotalAvailableTime(1000));
            tc = new TimeControl("40/300");
            Assert.AreEqual(TimeSpan.FromMinutes(5), tc.TotalAvailableTime(10));
            Assert.AreEqual(TimeSpan.FromMinutes(15), tc.TotalAvailableTime(100));
            Assert.AreEqual(TimeSpan.FromMinutes(25), tc.TotalAvailableTime(200));
            Assert.AreEqual(TimeSpan.FromMinutes(30), tc.TotalAvailableTime(201));
            tc = new TimeControl("40/5400+30:3600+30");
            Assert.AreEqual(new TimeSpan(0, 90, 30), tc.TotalAvailableTime(1));
            Assert.AreEqual(new TimeSpan(0, 110, 0), tc.TotalAvailableTime(40));
            Assert.AreEqual(new TimeSpan(0, 170, 30), tc.TotalAvailableTime(41));
        }

        [TestMethod]
        public void TestAddThinkTimes()
        {
            var games = PGN.Parse(Data.PGN_LICHESS_LIVE_GAME, true);
            Assert.AreEqual(1, games.Count);
            Assert.AreEqual(1, games[0].TimeControl.Controls.Count);
            games[0].TimeControl.AddThinkTimes(games[0]);
            foreach (var m in games[0].Moves)
            {
                Assert.IsTrue(m.UsedThinkTime >= TimeSpan.Zero);
            }
            Assert.AreEqual(TimeSpan.FromSeconds(5), games[0].Moves[2].UsedThinkTime);
            Assert.AreEqual(TimeSpan.FromSeconds(8), games[0].Moves[3].UsedThinkTime);
        }
    }

}