using Microsoft.VisualStudio.TestTools.UnitTesting;
using PonzianiComponents.Chesslib;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PonzianiComponentsTest
{
    [TestClass]
    public class UCIEngineTest
    {
        private static string enginePath = null;
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            enginePath = InstallStockfishAsync().Result;
            Console.WriteLine(enginePath);
        }

        [TestMethod]
        public void TestEngineStart()
        {
            if (enginePath == null) Assert.Fail("No Engine installed!");
            using (UCIEngine engine = new UCIEngine(enginePath))
            {
                engine.StartEngineAsync().Wait();
                Console.WriteLine(engine.Name + " " + engine.Author);
                Assert.IsNotNull(engine.Name);
                Assert.IsTrue(engine.Options.ContainsKey("Threads"));
                Assert.AreEqual(UCIEngine.Option.OptionType.SPIN, engine.Options["Threads"].Type);
                engine.Parameters.Add("Threads", "2");
                engine.Parameters.Add("Hash", "256");
                engine.SetOptionsAsync().Wait();
            }
        }

        [TestMethod]
        public void TestSimpleMate()
        {
            if (enginePath == null) Assert.Fail("No Engine installed!");
            using (UCIEngine engine = new UCIEngine(enginePath))
            {
                engine.StartEngineAsync().Wait();
                engine.SetOptionsAsync().Wait();
                engine.NewGameAsync().Wait();
                engine.SetPositionAsync("2bqkbn1/2pppp2/np2N3/r3P1p1/p2N2B1/5Q2/PPPPKPP1/RNB2r2 w - - 0 1").Wait();
                engine.StartAnalysisAsync(5).Wait();
                Assert.AreEqual(new Move("f3f7"), engine.BestMove);
            }
        }

        [TestMethod]
        public void TestInteractiveAnalysis()
        {
            if (enginePath == null) Assert.Fail("No Engine installed!");
            using (UCIEngine engine = new UCIEngine(enginePath))
            {
                engine.StartEngineAsync().Wait();
                engine.SetOptionsAsync().Wait();
                engine.NewGameAsync().Wait();
                //Initial position
                engine.SetPositionAsync(Fen.INITIAL_POSITION).Wait();
                //Start infinit analysis
                engine.StartAnalysisAsync().Wait();
                //Analyse for 100 ms
                Thread.Sleep(100);
                //Check result of analysis
                int score = engine.AnalysisInfo.Evaluation;
                Assert.IsTrue(score > 0);
                engine.SetPositionAsync(Fen.INITIAL_POSITION, "e2e4").Wait();
                engine.StartAnalysisAsync().Wait();
                //Analyse for 100 ms
                Thread.Sleep(100);
                score = engine.AnalysisInfo.Evaluation;
                Assert.IsTrue(score < 0);
                Game game = new Game();
                game.Add(new ExtendedMove("e2e4"));
                game.Add(new ExtendedMove("c7c5"));
                engine.SetPositionAsync(game, 2, Side.WHITE).Wait();
                engine.StartAnalysisAsync().Wait();
                //Analyse for 100 ms
                Thread.Sleep(100);
                score = engine.AnalysisInfo.Evaluation;
                Assert.IsTrue(score > 0);
            }
        }

        [TestMethod]
        public void TestInteractiveAnalysis2()
        {
            if (enginePath == null) Assert.Fail("No Engine installed!");
            using (UCIEngine engine = new UCIEngine(enginePath))
            {
                engine.PrepareEngineForAnalysisAsync().Wait();
                //Initial position
                engine.SetPositionAsync(Fen.INITIAL_POSITION).Wait();
                //Start infinit analysis
                engine.StartAnalysisAsync().Wait();
                //Analyse for 100 ms
                Thread.Sleep(100);
                //Check result of analysis
                int score = engine.AnalysisInfo.Evaluation;
                Assert.IsTrue(score > 0);
                engine.SetPositionAsync(Fen.INITIAL_POSITION, "e2e4").Wait();
                engine.StartAnalysisAsync().Wait();
                //Analyse for 100 ms
                Thread.Sleep(100);
                score = engine.AnalysisInfo.Evaluation;
                Assert.IsTrue(score < 0);
                Game game = new Game();
                game.Add(new ExtendedMove("e2e4"));
                game.Add(new ExtendedMove("c7c5"));
                engine.SetPositionAsync(game, 2, Side.WHITE).Wait();
                engine.StartAnalysisAsync().Wait();
                //Analyse for 100 ms
                Thread.Sleep(100);
                score = engine.AnalysisInfo.Evaluation;
                Assert.IsTrue(score > 0);
            }
        }

        [TestMethod]
        public void TestGameAnalysis()
        {
            if (enginePath == null) Assert.Fail("No Engine installed!");
            Game game = PGN.Parse(Data.PGN_TWIC)[1];
            using (UCIEngine engine = new UCIEngine(enginePath))
            {
                ExtendedMove beforeBlunder = engine.AnalyzeAsync(game, TimeSpan.FromMilliseconds(100), 25, Side.BLACK).Result;
                ExtendedMove afterBlunder = engine.AnalyzeAsync(game, TimeSpan.FromMilliseconds(100), 26, Side.WHITE).Result;
                int ScoreBefore = -beforeBlunder.Evaluation;
                int ScoreAfter = afterBlunder.Evaluation;
                Assert.IsTrue(ScoreAfter - ScoreBefore > 100);
            }
        }

        [TestMethod]
        public void TestEngineMatch()
        {
            string enginePath2 = @"D:\chrgu\OneDrive\Dokumente\Visual Studio 2019\Projekte\nemorino\x64\Release\nemorino.exe";
            using (UCIEngine engine1 = new UCIEngine(enginePath))
            {
                engine1.OnEngineInfoChanged += Engine_OnEngineInfoChanged;
                using (UCIEngine engine2 = new UCIEngine(enginePath2))
                {
                    engine2.OnEngineInfoChanged += Engine_OnEngineInfoChanged;
                    //Clocks in ms - we will play a 1+0 bullet game
                    int[] clocks = new int[] { 60000, 60000 };
                    Game game = new Game();
                    while (game.Result == Result.OPEN)
                    {
                        UCIEngine engine = game.Moves.Count % 2 == 0 ? engine1 : engine2;
                        ExtendedMove move = engine.AnalyzeAsync(game, game.Position.MoveNumber, game.Position.SideToMove,
                            TimeSpan.FromMilliseconds(clocks[0]), null, TimeSpan.FromMilliseconds(clocks[1]), null).Result;
                        //to keep the example simple we will trust the engine's time informatiom
                        clocks[game.Moves.Count % 2] -= (int)move.UsedThinkTime.TotalMilliseconds;
                        game.Add(move);
                    }
                    //engine.Name and Author are only available once the first AnalyseAsync 
                    game.White = engine1.Name; game.Black = engine2.Name;
                    Console.WriteLine(game.ToPGN(new CutechessCommenter()));
                }
            }
        }

        private static void Engine_OnEngineInfoChanged(object? sender, UCIEngine.EngineInfoEventArgs e)
        {
            Console.WriteLine("!!" + e.Info.Evaluation);
        }

        [TestMethod]
        public void LowLevel()
        {
            using (UCIEngine engine1 = new UCIEngine(enginePath))
            {
                bool x = RunAnalysis(engine1).Result;
                Assert.IsTrue(x);
            }
        }

        private async static Task<bool> RunAnalysis(UCIEngine engine1)
        {
            TaskCompletionSource<bool> tsc = new TaskCompletionSource<bool>();
            engine1.OnEngineOutput += async (sender, e) =>
            {
                if (e.Message == "uciok") await engine1.SendToEngineAsync("isready");
                else if (e.Message == "readyok")
                {
                    await engine1.SendToEngineAsync("position startpos moves e2e4 d7d5 e4d5");
                    await engine1.SendToEngineAsync("go movetime 1000");
                }
                else if (e.Message.StartsWith("bestmove"))
                {
                    Console.WriteLine("Best Move:" + e.Message.Substring(9));
                    tsc.SetResult(true);
                }
            };
            bool started = engine1.StartEngineAsync().Result;
            return await tsc.Task;
        }

        private async static Task<string> InstallStockfishAsync()
        {
            string path = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Path.Combine(AppDataFolder(), "stockfish_14.1_win_x64_avx2", "stockfish_14.1_win_x64_avx2.exe")
        : Path.Combine(AppDataFolder(), "stockfish_14.1_linux_x64_avx2", "stockfish_14.1_linux_x64_avx2");
            if (!File.Exists(path))
            {
                string url = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "https://stockfishchess.org/files/stockfish_14.1_win_x64_avx2.zip" : "https://stockfishchess.org/files/stockfish_14.1_linux_x64_avx2.zip";
                string archivePath = Path.Combine(AppDataFolder(), "sf.zip");
                bool downloadSuccessful = await DownloadFileAsync(url, archivePath);
                if (!downloadSuccessful) return null;
                ZipFile.ExtractToDirectory(archivePath, AppDataFolder());
            }
            return path;
        }

        private static string AppDataFolder()
        {
            var userPath = Environment.GetEnvironmentVariable(
              RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
              "LOCALAPPDATA" : "Home");
            var assy = System.Reflection.Assembly.GetEntryAssembly();
            var companyName = assy.GetCustomAttributes<AssemblyCompanyAttribute>()
              .FirstOrDefault();
            var path = System.IO.Path.Combine(userPath, companyName.Company, "Ponziani");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }

        private static async Task<bool> DownloadFileAsync(string uri, string outputPath)
        {
            Uri uriResult;

            if (!Uri.TryCreate(uri, UriKind.Absolute, out uriResult)) return false;
            if (File.Exists(outputPath)) return false;

            HttpClient client = new HttpClient();
            byte[] fileBytes = await client.GetByteArrayAsync(uri);
            File.WriteAllBytes(outputPath, fileBytes);
            return true;
        }

    }
}
