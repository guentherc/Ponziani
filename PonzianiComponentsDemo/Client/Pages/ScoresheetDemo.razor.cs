using Microsoft.AspNetCore.Components.Forms;
using PonzianiComponents;
using PonzianiComponents.Chesslib;
using System.Text.Json;
using System.Text.RegularExpressions;
using static PonzianiComponents.Scoresheet;

namespace PonzianiComponentsDemo.Client.Pages
{
    public class SDModel
    {
        public string PGNKey { set; get; } = "Lichess Study";
        public string? PGNText { set; get; }
        public int Height { set; get; } = 400;
        public NotationType NotationType { set; get; } = NotationType.SAN;
        public bool InlineMode { set; get; } = false;
        public bool Comments { set; get; } = false;
        public bool Variations { set; get; } = false;
        public bool HierarchicalDisplay { set; get; } = false;
        public string OtherAttributes { set; get; } = @"style=""width: 800px; height: 400px""";
        public string ColorCommentText { set; get; } = "blue";
        public string ColorCommentBackground { set; get; } = "#FFEBE4";
        public string ColorVariationBackground { set; get; } = "#EBE4FF";
        public string Language { set; get; } = "en";
        public int MinimumRowCount { set; get; } = 20;
    }

    public partial class ScoresheetDemo
    {
        public string PGN
        {
            set
            {
                var games = PonzianiComponents.Chesslib.PGN.Parse(value, true, true, 1);
                if (games != null && games.Count > 0) game = games[0]; else game = new Game();
            }
            get { return game.ToPGN(); }
        }

        public Scoresheet.DisplayMode DisplayMode => Model.InlineMode ? Scoresheet.DisplayMode.INLINE : Scoresheet.DisplayMode.TABULAR;

        private string EventInfoText { set; get; } = "";

        private static readonly Regex regexOtherAttributes = new(@"(\w+)=\""([^\""]+)\""");
        private SDModel Model { set; get; } = new SDModel();
        private Game game = new();
        private Dictionary<string, object> OtherAttributes
        {
            get
            {
                Dictionary<string, object> oo = new();
                MatchCollection mc = regexOtherAttributes.Matches(Model.OtherAttributes);
                foreach (Match m in mc)
                {
                    oo.Add(m.Groups[1].Value, m.Groups[2].Value);
                }
                return oo;
            }
        }

        private async Task LoadFile(InputFileChangeEventArgs e)
        {
            Model.PGNText = await new StreamReader(e.File.OpenReadStream()).ReadToEndAsync();
            PGN = Model.PGNText;
        }

        private void HandleValidSubmit()
        {
            if (SampleData.PGNSamples.ContainsKey(Model.PGNKey))
                PGN = SampleData.PGNSamples[Model.PGNKey];
            else PGN = Model.PGNText;
        }

        private void OnMoveSelected(MoveSelectInfo msi)
        {
            EventInfoText = $"OnMoveSelected({JsonSerializer.Serialize(msi)})";
        }
    }

    public static class SampleData
    {
        public static readonly Dictionary<string, string> PGNSamples = new()
        {
            { "Lichess Study", @"[Event ""🏆 Nimzo/Bogo Indian Repertoire 🏆: London System""]
[Site ""https://lichess.org/study/DeAekads/TzNlHqMZ""]
[Result ""*""]
[Annotator ""https://lichess.org/@/Mr_Penings""]
[UTCDate ""2020.06.23""]
[UTCTime ""18:24:23""]
[Variant ""Standard""]
[ECO ""A46""]
[Opening ""Indian Defense: London System""]

{ The infamous London System has brought fear to many players. I will teach you how to beat it Nimzo-Indian style. See my Kings Indian guide for my favorite way to counter the London. This guide is aimed at Nimzo Indian players. }
1. d4 Nf6 2. Nf3 e6 3. Bf4 b6 4. e3 Bb7 { Generally, the London System is less effective if Black hasn't played d5. Here, the goal is to restrict the Bf4 by playing d6 and playing a Hedgehog structure, which prefers flexibility. } 5. Nbd2 c5 6. c3 Be7 7. h3 (7. Bd3 Nh5!? { Guarantees the bishop pair since White does not have h2 to retreat to. This is a tricky move order and happens quite often. } 8. Bg3 d6 { No rush in playing Nxg3. Wait for White to castle first } 9. O-O Nxg3 10. hxg3 O-O) 7... O-O 8. Bd3 d6 { The goal is to play a Hedgehog structure, which lacks space but has no weaknesses. See below for possible continuations. } 9. O-O Nbd7 10. Qe2 cxd4 { It is fine to exchange center pawns because White's knight is already developed to d2. If it was still on b1, White can recapture cxd4 and play Nc3, its ideal spot. } 11. exd4 (11. cxd4 Nd5 12. Bg3 Nb4 13. Bc4 (13. Bb1 Ba6) 13... d5 14. Bb5 a6 15. Ba4 b5 16. Bb3 Rc8 { Black's doing fine here. Likely a lot of trades on the c-file leading to an even endgame. }) 11... a6 { To prevent White from playing Ba6, offering a bishop trade. You may want to keep your bishop since it has activity down the main diagonal. } 12. Rfe1 Re8 13. a4 Qc7 14. Bh2 Bf8 15. Rab1 g6 { See Kamsky - Carlsen 0-1 below for a sample game in this line. } *" },
            { "SCID Export", @"[Event ""Wch11""]
[Site ""Berlin GER""]
[Date ""1910.??.??""]
[Round ""5""]
[White ""Lasker, Emanuel""]
[Black ""Janowski, Dawid""]
[Result ""1-0""]
[ECO ""D32""]
[EventDate ""1910.??.??""]

{This time the money for the world championship was no problem. The 
Berliner Schachgesellschaft supplied 2500 Mark and Nardus 5000 Frank.} 
1.d4 d5 2.c4 e6 3.Nc3 c5 4.cxd5 exd5 5.Nf3 Be6 6.e4 $6 ( 6.Bg5 Nf6 7.e3 {
led to a variation of the Tarrasch (Lasker-Lawrence, Cambridge Springs 
1904).} ) 6...dxe4 7.Nxe4 Nc6 $1 8.Be3 ( 8.Nxc5 Bxc5 9.dxc5 Qxd1+ 10.Kxd1 
O-O-O+ {Black has the better ending.} ) 8...cxd4 9.Nxd4 ( 9.Bxd4 Bb4+ $1 (
9...Nxd4 {(Tarrasch)} ) 10.Bc3 Qe7 ) 9...Qa5+ 10.Nc3 $2 ( 10.Qd2 Bb4 11.
Nc3 O-O-O 12.Nxc6 $1 bxc6 13.Qc1 Nf6 14.a3 {and a hard defence.} ) 10...
O-O-O $1 11.a3 {White is lost, because knight d4 lacks a sufficient 
support.} 11...Nh6 $2 ( 11...Bc5 $1 12.b4 Bxd4 $1 13.Bxd4 Qg5 14.h4 ( 14.
Ne2 Nxd4 15.Nxd4 Qe5+ ) 14...Qg6 $19 {(Tarrasch).} ) 12.b4 Qe5 ( 12...Bxb4
$6 13.axb4 Qxb4 14.Qc1 Rxd4 15.Ra4 $5 ( {or} 15.Bxd4 Qxd4 16.Be2 ) 15...
Rd1+ $1 16.Qxd1 Qxc3+ 17.Bd2 Qe5+ 18.Be2 {White takes the initiative.} ) 
13.Ncb5 Nf5 $1 ( 13...a6 14.Qc1 $1 axb5 15.Nxc6 bxc6 16.Qxc6+ Qc7 17.Qa8+ 
Qb8 $1 ( 17...Kd7 18.Bxb5+ {is risky} ) 18.Qc6+ $1 $11 ( 18.Rc1+ $2 Bc4 ) 
) 14.Rc1 $1 Nxe3 15.fxe3 Qxe3+ 16.Be2 Be7 $1 ( 16...Rxd4 $6 17.Nxd4 Bxb4+ 
{sacrifices too much.} 18.axb4 Rd8 19.b5 Rxd4 20.Qc2 Rc4 21.Qxc4 Bxc4 22.
Rxc4 {and 23.bxc6 is good for White.} ) ( 16...Bb3 $2 17.Nxa7+ $1 ) 17.Rc3
$1 ( 17.Nxa7+ Kb8 18.Naxc6+ bxc6 19.Rxc6 Kb7 20.Qc2 $1 Rc8 $1 ( 20...Bh4+ 
$11 ) 21.Qc3 Qxc3+ 22.Rxc3 Kb6 {leads to a favourable endgame for Black.} 
) 17...Bh4+ $6 ( 17...Qxc3+ $1 18.Nxc3 Nxd4 {and a fine game (Tarrasch).} 
) 18.g3 Qe4 $2 ( 18...Qxc3+ {is still okay.} ) 19.O-O Bf6 20.Rxf6 $1 {
Suddenly White strikes.} 20...gxf6 21.Bf3 Qe5 22.Nxa7+ Kc7 23.Naxc6 bxc6 
24.Rxc6+ Kb8 25.Rb6+ Kc8 26.Qc1+ Kd7 27.Nxe6 fxe6 28.Rb7+ Ke8 29.Bc6+ {
Lasker came back from a lost opening.} 1-0

" }
        };
    }

}
