using PonzianiComponents;

namespace PonzianiComponentsDemo.Client.Pages
{

    class EModel
    {
        public string Cmd { get; set; } = String.Empty;
        public string Fen { get; set; } = PonzianiComponents.Chesslib.Fen.INITIAL_POSITION;
        public int NumberOfLines { get; set; } = 1;
        public bool ShowLog { get; set; } = false;
    }
    public partial class EngineDemo
    {
        private readonly EModel Model = new();
        private Engine? engine;

        private async Task SendToEngineAsync()
        {
            await engine?.SendAsync(Model.Cmd);
        }

        private async Task AnalyzeAsync()
        {
            await engine?.StartAnalysisAsync(Model.Fen);
        }
    }
}
