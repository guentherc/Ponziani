using PonzianiComponents;

namespace PonzianiComponentsDemo.Client.Pages
{

    class EModel
    {
        public string Cmd { get; set; } = String.Empty;
        public string Fen { get; set; } = PonzianiComponents.Chesslib.Fen.INITIAL_POSITION;
        public int NumberOfLines { get; set; } = 1;
        public bool ShowLog { get; set; } = false;
        public bool ShowEvaluationBar { get; set; } = false;
        public Orientation Orientation { get; set; } = Orientation.Horizontal;
    }
    public partial class EngineDemo
    {
        private readonly EModel Model = new();
        private Engine? engine;
        private EvaluationGauge? evaluationBar;


        private void OnEngineInfo(Info info)
        {
            if (info.MoveIndex == 1)
            {
                evaluationBar.Score = engine.Score;
                evaluationBar.ScoreText = engine.ScoreText(0);
            }
        }

        private async Task SendToEngineAsync()
        {
            await engine?.SendAsync(Model.Cmd);
        }

        private async Task AnalyzeAsync()
        {
            Console.WriteLine("AnalyzeAsync(" + Model.Fen + ")");
            await engine?.StartAnalysisAsync(Model.Fen);
        }
    }
}
