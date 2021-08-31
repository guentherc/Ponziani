using Microsoft.AspNetCore.Components.WebAssembly.Http;
using PonzianiComponents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PonzianiDemo.Pages
{

    public partial class LichessTV
    {
        string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        CancellationTokenSource cts;
        Chessboard cb;
        string white = "";
        string black = "";
        string wtime = "";
        string btime = "";

        async Task Get()
        {
            cts = new CancellationTokenSource();

            using var request = new HttpRequestMessage(HttpMethod.Get, "https://lichess.org/api/tv/feed");
            request.SetBrowserResponseStreamingEnabled(true); // Enable response streaming

            // Be sure to use HttpCompletionOption.ResponseHeadersRead
            using var response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            using var stream = await response.Content.ReadAsStreamAsync();

            // Read the response chunk by chunk and count the number of bytes
            var bytes = new byte[1024];
            StringBuilder sb = new StringBuilder();
            while (!cts.Token.IsCancellationRequested)
            {
                var read = await stream.ReadAsync(bytes, cts.Token);
                if (read == 0) // End of stream
                    return;

                sb.Append(Encoding.UTF8.GetString(bytes, 0, bytes.Length));
                var rawlines = sb.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
                var lines = new List<string>();
                foreach (var raw in rawlines)
                {
                    int indx;
                    if ((indx = raw.IndexOf(@"}}{""t")) > 0)
                    {
                        lines.Add(raw.Substring(0, indx + 2));
                        lines.Add(raw.Substring(indx + 2));
                    }
                    else lines.Add(raw);
                }
                foreach (var l in lines) Console.WriteLine(l);
                lines = lines.Where(l => l != null && l.Trim().Length > 50 && l.StartsWith(@"{""t""")).ToList();
                if (lines.Count > 1)
                {
                    cb.ClearHighlighting();
                    for (int i = 0; i < lines.Count; ++i)
                    {
                        string line = lines[i].Trim();
                        Console.WriteLine($"Line: {line}");
                        if (line == null || line.Length < 10) continue;
                        try
                        {
                            LichessTVRecord ltv = JsonSerializer.Deserialize<LichessTVRecord>(line);
                            lines.Clear();
                            if (ltv.d.lm != null && ltv.d.lm.Length >= 4)
                            {
                                PonzianiComponents.Chesslib.Move move = new(ltv.d.lm);
                                cb.SetHighlightSquare(move.From);
                                cb.SetHighlightSquare(move.To);
                                wtime = $"{ltv.d.wc / 60}:{ltv.d.wc % 60}";
                                btime = $"{ltv.d.bc / 60}:{ltv.d.bc % 60}";

                            }
                            if (ltv.d.players != null && ltv.d.players.Count == 2)
                            {
                                white = $"{ltv.d.players[0].user.title} {ltv.d.players[0].user.name} ({ltv.d.players[0].rating})".Trim();
                                black = $"{ltv.d.players[1].user.title} {ltv.d.players[1].user.name} ({ltv.d.players[1].rating})".Trim();
                            }
                            Console.WriteLine(ltv.d.fen);
                            fen = ltv.d.fen.Trim();
                            fen = fen.IndexOf(' ') > 0 ? $"{fen} - - 0 1" : $"{fen} w - - 0 1";
                            StateHasChanged();
                            // Update the UI
                            await Task.Delay(100);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    sb.Clear();
                    if (lines.Count > 0) sb.Append(lines.Last().Trim());
                }
            }
        }


        void Stop() => cts?.Cancel();
    }

    internal class LichessTVRecord
    {
        public string t { set; get; }
        public LichessD d { set; get; }
    }

    internal class LichessD
    {
        public string id { set; get; }
        public string orientation { set; get; }
        public string fen { set; get; }
        public string lm { set; get; }
        public int wc { set; get; }
        public int bc { set; get; }
        public List<LichessPlayer> players { set; get; }
    }

    internal class LichessPlayer
    {
        public string color { set; get; }
        public int rating { set; get; }
        public LichessUser user { set; get; }
    }

    internal class LichessUser
    {
        public string name { set; get; }
        public string title { set; get; }
        public string id { set; get; }
    }

    internal static class HttpContentNdjsonExtensions
    {
        private static readonly JsonSerializerOptions _serializerOptions
            = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

        public static async IAsyncEnumerable<TValue> ReadFromNdjsonAsync<TValue>(this HttpContent content)
        {
            if (content is null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            string? mediaType = content.Headers.ContentType?.MediaType;

            if (mediaType is null || !mediaType.Equals("application/x-ndjson", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException();
            }

            Stream contentStream = await content.ReadAsStreamAsync().ConfigureAwait(false);

            using (contentStream)
            {
                using (StreamReader contentStreamReader = new StreamReader(contentStream))
                {
                    while (!contentStreamReader.EndOfStream)
                    {
                        yield return JsonSerializer.Deserialize<TValue>(await contentStreamReader.ReadLineAsync()
                            .ConfigureAwait(false), _serializerOptions);
                    }
                }
            }
        }
    }
}
