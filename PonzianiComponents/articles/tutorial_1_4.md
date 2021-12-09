# Tutorial - Adding a Scoresheet

In this chapter we will add a scoresheet, allowing us to track and note down our interactive analysis.

This can be done by only adding a few lines (displayed in bold)
<pre>
@page "/"
&lt;div class="container"&gt;
    &lt;div class="row"&gt;
        &lt;div class="col-md-auto"&gt;
            &lt;Chessboard OnMovePlayed="OnMovePlayed" Size=400 /&gt;
        &lt;/div&gt;
        &lt;div class="col" style="height:100%;vertical-align:middle;"&gt;
            &lt;EvaluationGauge Score="@Score" ScoreText="@ScoreText" Orientation="Orientation.Vertical" style="height:350px;padding-top:25px;" /&gt;
        &lt;/div&gt;
    &lt;/div&gt;
    &lt;div class="row mt-2"&gt;
        &lt;Engine @ref="engine" OnEngineInfo="OnEngineInfo" /&gt;
    &lt;/div&gt;
    <strong>&lt;div class="row mt-2"&gt;
        &lt;div class="col-md-auto"&gt;
            &lt;Scoresheet Game="game" style="width:400px" /&gt;
        &lt;/div&gt;
    &lt;/div&gt;</strong>
&lt;/div&gt;

@code {
    private Engine engine;

    <strong>private Game game = new Game();</strong>

    private int Score { set; get; } = 0;
    private string ScoreText { set; get; } = String.Empty;



    void OnMovePlayed(MovePlayedInfo mpi)
    {
        engine.StartAnalysisAsync(mpi.NewFen);
        <strong>game.Add(new ExtendedMove(mpi.Move));</strong>
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
</pre>

We added a Scoresheet component and a Game object as attribute, which provides the content of the scoresheet.
We update the Game object in the event callback `OnMovePlayed`, which is triggered when the user applies a move.

![Screenshot of application with scoresheet](img/tutorial_1_4a.png) 

> [!div class="nextstepaction"]
> [Next](tutorial_1_5.md)