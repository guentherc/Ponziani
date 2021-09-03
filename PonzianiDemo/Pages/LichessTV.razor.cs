using PonzianiComponents;
using PonzianiComponents.Chesslib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PonzianiDemo.Pages
{
    public partial class LichessTV
    {
        private Game Game = new Game();
        private Dictionary<string, string> games = null;
        private string gameid = null;
        private Chessboard chessboard;
        CancellationTokenSource cts;

        private string Clock { get
            {
                string clock0 = Game.Moves.Count > 0 ? Game.Moves.Last().Clock.ToString() : "0";
                string clock1 = Game.Moves.Count > 1 ? Game.Moves[Game.Moves.Count - 2].Clock.ToString() : "0";
                if (Game.SideToMove == Side.BLACK) return $"{clock0} - {clock1}"; else return $"{clock1} - {clock0}";
            } }

        protected override async Task OnInitializedAsync()
        {
            if (gameid == null || games == null)
            {
                games = await GetGameIdsAsync();
                foreach (var g in games)
                {
                    Console.WriteLine($"{g.Key}: {g.Value}");
                }
            }
            gameid = games["Top Rated"];
            Get();
        }

        private async Task Update()
        {
            Console.WriteLine("Update!");
            string pgn = await HttpClient.GetStringAsync($"https://lichess.org/game/export/{gameid}");
            Game = PGN.Parse(pgn)[0];
            StateHasChanged();
        }

        private async Task<Dictionary<string, string>> GetGameIdsAsync() {
            Dictionary<string, string> gameids = new Dictionary<string, string>();
            string json = await HttpClient.GetStringAsync("https://lichess.org/api/tv/channels");
            var doc = JsonDocument.Parse(json);
            foreach (var property in doc.RootElement.EnumerateObject().Where(it => supported.Contains(it.Name)))
            {
                string key = property.Name;

                JsonElement element = property.Value;
                string id = element.EnumerateObject().Where(it => it.Name == "gameId").First().Value.ToString();
                gameids.Add(key, id);
            }
            return gameids;
        }

        async Task Get()
        {
            cts = new CancellationTokenSource();

            while (!cts.Token.IsCancellationRequested)
            {
                Console.WriteLine("Update!");
                string pgn = await HttpClient.GetStringAsync($"https://lichess.org/game/export/{gameid}");
                Game = PGN.Parse(pgn, true)[0];
                if (Game.Result != Result.OPEN)
                {
                    games = await GetGameIdsAsync();
                    gameid = games["Top Rated"];

                }
                chessboard.ClearHighlighting();
                chessboard.SetHighlightSquare(Game.Moves.Last().From);
                chessboard.SetHighlightSquare(Game.Moves.Last().To);
                StateHasChanged();
                await Task.Delay(1000);
            }
        }


        void Stop() => cts?.Cancel();

        private static HashSet<string> supported = new HashSet<string>() { "Bot", "UltraBullet", "Bullet", "Computer", "Rapid", "Top Rated", "Blitz", "Classical" };

        private string PairingString => Game.Tags.ContainsKey("WhiteElo") && Game.Tags.ContainsKey("BlackElo") ? $"{Game.White} ({Game.Tags["WhiteElo"]}) - {Game.Black} ({Game.Tags["BlackElo"]})" : $"{Game.White} - {Game.Black}";
    }
}
