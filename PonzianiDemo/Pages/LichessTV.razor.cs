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
        private Game Game = new();
        private Dictionary<string, string> games = new();
        private string gameid = null;
        private Chessboard chessboard;
        readonly Dictionary<string, CancellationTokenSource> cts = new();
        private string selectedChannel = null;

        private string Clock
        {
            get
            {
                TimeSpan c0 = Game.Moves.Count > 0 ? Game.Moves.Last().Clock : TimeSpan.Zero;
                TimeSpan c1 = Game.Moves.Count > 1 ? Game.Moves[^2].Clock : TimeSpan.Zero;
                string format = "";
                if (c0 < new TimeSpan(0, 1, 0) && c1 < new TimeSpan(0, 1, 0)) format = @"s\.f";
                else if (c0 < new TimeSpan(1, 0, 0) && c1 < new TimeSpan(1, 0, 0)) format = @"m\:ss";
                string clock0 = c0.ToString(format);
                string clock1 = c1.ToString(format);
                if (Game.SideToMove == Side.BLACK) return $"{clock0} - {clock1}"; else return $"{clock1} - {clock0}";
            }
        }

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
        }

        private async Task OnChannelSelected(string channel)
        {
            Console.WriteLine($"Selected Channel: {channel}");
            if (selectedChannel != null && cts.ContainsKey(selectedChannel) && !cts[selectedChannel].IsCancellationRequested)
            {
                Stop();
            }
            if (channel == "off")
            {
                selectedChannel = null;
                return;
            }
            selectedChannel = channel;
            await Get(channel);
        }

        private async Task<Dictionary<string, string>> GetGameIdsAsync()
        {
            Dictionary<string, string> gameids = new();
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

        async Task Get(string channel)
        {
            games = await GetGameIdsAsync();
            gameid = games[channel];
            if (cts.ContainsKey(channel))
                cts[channel] = new CancellationTokenSource();
            else cts.Add(channel, new CancellationTokenSource());

            while (!cts[channel].Token.IsCancellationRequested)
            {
                string pgn = await HttpClient.GetStringAsync($"https://lichess.org/game/export/{gameid}");
                Game = PGN.Parse(pgn, true)[0];
                Game.TimeControl.AddThinkTimes(Game);
                if (Game.Result != Result.OPEN)
                {
                    games = await GetGameIdsAsync();
                    gameid = games[channel];

                }
                chessboard.ClearHighlighting();
                if (Game.Moves.Count > 0)
                {
                    chessboard.SetHighlightSquare(Game.Moves.Last().From);
                    chessboard.SetHighlightSquare(Game.Moves.Last().To);
                }
                StateHasChanged();
                await Task.Delay(1000);
            }
        }


        void Stop() => cts[selectedChannel].Cancel();

        private static readonly HashSet<string> supported = new() { "Bot", "UltraBullet", "Bullet", "Computer", "Rapid", "Top Rated", "Blitz", "Classical", "Chess960" };

        private string PairingString => Game.Tags.ContainsKey("WhiteElo") && Game.Tags.ContainsKey("BlackElo") ? $"{Game.White} ({Game.Tags["WhiteElo"]}) - {Game.Black} ({Game.Tags["BlackElo"]})" : $"{Game.White} - {Game.Black}";
    }
}
