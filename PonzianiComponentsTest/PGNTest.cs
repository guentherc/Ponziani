using Microsoft.VisualStudio.TestTools.UnitTesting;
using PonzianiComponents.Chesslib;
using System;

namespace PonzianiComponentsTest
{
    [TestClass]
    public class PGNTest
    {
        [TestMethod]
        public void ParseSingleLichessGame()
        {
            var games = PGN.Parse(Data.PGN_LICHESS_SINGLE_GAME);
            Assert.AreEqual(1, games.Count);
            Assert.AreEqual(43, games[0].Moves.Count);
            Assert.AreEqual(Result.WHITE_WINS, games[0].Result);
            Assert.AreEqual(ResultDetail.MATE, games[0].ResultDetail);
        }

        [TestMethod]
        public void ParseLichessLiveGame()
        {
            var games = PGN.Parse(Data.PGN_LICHESS_LIVE_GAME, true);
            Assert.AreEqual(1, games.Count);
            Assert.AreEqual(31, games[0].Moves.Count);
            Assert.AreEqual(Result.OPEN, games[0].Result);
            Assert.IsTrue(games[0].Moves[0].Clock.TotalSeconds > 100);
            for (int i = 2; i < games[0].Moves.Count; ++i)
            {
                Assert.IsTrue(games[0].Moves[i].Clock <= games[0].Moves[i - 2].Clock);
            }
        }

        [TestMethod]
        public void ParseLichessCommentedGame()
        {
            var games = PGN.Parse(Data.PGN_LICHESS_COMMENTED_GAME, true);
            Assert.AreEqual(1, games.Count);
            Assert.AreEqual(54, games[0].Moves.Count);
            Assert.AreEqual(Result.BLACK_WINS, games[0].Result);
            Assert.IsTrue(games[0].Moves[0].Clock.TotalSeconds > 100);
            for (int i = 2; i < games[0].Moves.Count; ++i)
            {
                Assert.IsTrue(games[0].Moves[i].Clock <= games[0].Moves[i - 2].Clock);
            }
            for (int i = 0; i < 32; ++i) Assert.IsTrue(Math.Abs(games[0].Moves[i].Evaluation) < 100);
            for (int i = 40; i < games[0].Moves.Count; ++i) Assert.IsTrue(Math.Abs(games[0].Moves[i].Evaluation) > 500);
        }

        [TestMethod]
        public void ParseChessguiGame()
        {
            var games = PGN.Parse(Data.PGN_CCRL_CHESSGUI_GAME, true);
            Assert.AreEqual(1, games.Count);
            for (int i = 0; i < 50; ++i) Assert.IsTrue(Math.Abs(games[0].Moves[i].Evaluation) < 100);
            for (int i = 50; i < games[0].Moves.Count; ++i) Assert.IsTrue(Math.Abs(games[0].Moves[i].Evaluation) > 300);
            for (int i = 0; i < 7; ++i) Assert.IsTrue(games[0].Moves[i].Depth == 1);
            for (int i = 7; i < games[0].Moves.Count; ++i) Assert.IsTrue(games[0].Moves[i].Depth > 10);
        }

        [TestMethod]
        public void ParseCutechessGame()
        {
            var games = PGN.Parse(Data.PGN_CUTECHESS, true);
            Assert.AreEqual(2, games.Count);
            for (int i = 0; i < games[1].Moves.Count; ++i)
            {
                Assert.IsTrue(games[1].Moves[i].Evaluation != 0);
                Assert.IsTrue(games[1].Moves[i].Depth > 0);
                Assert.IsTrue(games[1].Moves[i].UsedThinkTime > TimeSpan.Zero);
                Assert.IsTrue(games[1].Moves[i].UsedThinkTime < TimeSpan.FromSeconds(2));
            }
        }

        [TestMethod]
        public void ParseTCECGame()
        {
            var games = PGN.Parse(Data.PGN_TCEC, true);
            Assert.AreEqual(1, games.Count);
            for (int i = 10; i < games[0].Moves.Count; ++i)
            {
                Assert.IsTrue(games[0].Moves[i].Clock <= games[0].Moves[i - 2].Clock + TimeSpan.FromSeconds(5));
            }
            for (int i = 8; i < games[0].Moves.Count; ++i) Assert.IsTrue(games[0].Moves[i].Depth > 15);
        }

        [TestMethod]
        public void ParseChessResultGame()
        {
            var games = PGN.Parse(Data.PGN_CHESS_RESULTS, true);
            Assert.AreEqual(1, games.Count);
            Assert.AreEqual(Result.DRAW, games[0].Result);
            for (int i = 0; i < games[0].Moves.Count; ++i) Assert.IsTrue(games[0].Moves[i].UsedThinkTime > TimeSpan.Zero);
        }

        [TestMethod]
        public void ParseChess24Game()
        {
            var games = PGN.Parse(Data.PGN_CHESS24, true);
            Assert.AreEqual(1, games.Count);
            Assert.AreEqual(Result.OPEN, games[0].Result);
            for (int i = 0; i < games[0].Moves.Count; ++i)
                Assert.IsTrue(games[0].Moves[i].Clock > TimeSpan.FromHours(1));
        }

        [TestMethod]
        public void ParseLichessStudy()
        {
            var games = PGN.Parse(Data.PGN_LICHESS_STUDY, true, true);
            Assert.AreEqual(2, games.Count);
            Assert.AreEqual(30, games[0].Moves.Count);
            Assert.IsTrue(games[0].Moves[7].Comment.Length > 50);
            Assert.IsNotNull(games[0].Moves[12].Variations);
            Assert.AreEqual(2, games[0].Moves[20].Variations[0][4].Variations[0].Count);
        }

        [TestMethod]
        public void ParseScidFile()
        {
            var games = PGN.Parse(Data.PGN_SCID, true, true);
            Assert.AreEqual(1, games.Count);
            Assert.AreEqual(57, games[0].Moves.Count);
            Assert.AreEqual(3, games[0].Moves[10].Variations[0].Count);
            Assert.AreEqual(2, games[0].Moves[31].Variations.Count);
            string pgn = games[0].ToPGN(null, true);
            games = PGN.Parse(pgn, true, true);
            Assert.AreEqual(1, games.Count);
            Assert.AreEqual(57, games[0].Moves.Count);
            Assert.AreEqual(3, games[0].Moves[10].Variations[0].Count);
            Assert.AreEqual(2, games[0].Moves[31].Variations.Count);
        }


        [TestMethod]
        public void TestIntroductionComment()
        {
            var games = PGN.Parse(Data.PGN_LICHESS_STUDY, true, true);
            Assert.IsNotNull(games[0].IntroductionComment);
        }

    }
}
