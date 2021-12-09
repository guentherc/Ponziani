# Tutorial - Adding an Evaluation Bar

Adding an evaluation bar is very simple
<pre>
@page "/"

&lt;Chessboard OnMovePlayed="OnMovePlayed"/&gt;
&lt;Engine @ref="engine" <strong>ShowEvaluationbar="true"</strong>/&gt;

@code {
    private Engine engine;

    void OnMovePlayed(MovePlayedInfo mpi)
    {
        engine.StartAnalysisAsync(mpi.NewFen);
    }
}
</pre>

By simply adding the attribute `ShowEvaluationbar="true"` to the Engine component you will get an 
evaluation bar endered below the engine info panel.

![Screenshot of application with evaluation bar](img\tutorial_1_3a.png) 

But with the simple approach the evaluation bar is fixed below the engine info panel. What if we would like 
to have it vertically oriented next to the chessboard. This can be achieved by using the EvaluationGauge 
component, which can be used to render the evaluation bar independent of the engine info panel.

    @page "/"
    <div class="container">
        <div class="row">
            <div class="col-md-auto">
                <Chessboard OnMovePlayed="OnMovePlayed" Size=400/>
            </div>
            <div class="col" style="height:100%;vertical-align:middle;">
                <EvaluationGauge Score="@Score" ScoreText="@ScoreText" Orientation="Orientation.Vertical" style="height:350px;padding-top:25px;"/>
            </div>
        </div>
        <div class="row">
            <Engine @ref="engine" OnEngineInfo="OnEngineInfo"/>
        </div>
    </div>

    @code {
        private Engine engine;

        private int Score { set; get; } = 0;
        private string ScoreText { set; get; } =  String.Empty;

        void OnMovePlayed(MovePlayedInfo mpi)
        {
            engine.StartAnalysisAsync(mpi.NewFen);
        }

        void OnEngineInfo(Info info)
        {
            if (info.MoveIndex == 1)
            {
                Score = engine.Score;
                ScoreText = engine.ScoreText(0);
            }
        }
    }

We have now the 3 components (Chessboard, EvaluationGauge and Engine) surrounded by some layout html.

The code block manages the communication between the 3 components:

* When the user applies a move via drag & drop event callback `OnMovePlayed` is triggered and `engine.StartAnalysisAsync` is called. 
  The engine worker will start analyzing the new position.
* Every time the engine issues an info message, the event callback `OnEngineInfo` is called and the properties Score and ScoreText are 
  set from the engine's info object
* These properties are passsed to the EvaluationGauge component as parameters, which uses them for rendering

![Screenshot of application with evaluation bar right of the chessboard](img/tutorial_1_3b.png) 

> [!div class="nextstepaction"]
> [Next](tutorial_1_4.md)