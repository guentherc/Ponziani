# Tutorial - Make the Scoresheet Interactive

Now that we have the scoresheet, why not use it to go back to previous moves, add variations, ...

The Scoresheet component offers an event callback `OnMoveSelected` which is triggered whenever the user
clicks on a move within the scoresheet. We will use this callback to make the scoresheet interactive

This requires some changes to the existing code:

<pre>
@page "/"
&lt;div class="container"&gt;
    &lt;div class="row"&gt;
        &lt;div class="col-md-auto"&gt;
            &lt;Chessboard OnMovePlayed="OnMovePlayed" Size=400 <strong>@ref="board"</strong> /&gt;
        &lt;/div&gt;
        &lt;div class="col" style="height:100%;vertical-align:middle;"&gt;
            &lt;EvaluationGauge Score="@Score" ScoreText="@ScoreText" Orientation="Orientation.Vertical" style="height:350px;padding-top:25px;" /&gt;
        &lt;/div&gt;
    &lt;/div&gt;
    &lt;div class="row mt-2"&gt;
        &lt;Engine @ref="engine" OnEngineInfo="OnEngineInfo" /&gt;
    &lt;/div&gt;
    &lt;div class="row mt-2"&gt;
        &lt;div class="col-md-auto"&gt;
            &lt;Scoresheet Game="game" style="width:400px" <strong>OnMoveSelected="OnMoveSelected" Variations="true" HierarchicalDisplay="true"</strong>/&gt;
        &lt;/div&gt;
    &lt;/div&gt;
&lt;/div&gt;

@code {
    private Engine engine;
    private Chessboard board;

    private Game game = new Game();

    private int Score { set; get; } = 0;
    private string ScoreText { set; get; } = String.Empty;
    <strong>private List<ExtendedMove> vmoves = null;

    void OnMoveSelected(MoveSelectInfo moveSelectInfo)
    {
        //moveSelectInfo.Position.FEN provides the fen of the position before the clicked move
        //As usually the position after the clicked move is expected we have to calculate the fen 
        //after the move
        Position pos = new(moveSelectInfo.Position.FEN);
        pos.ApplyMove(moveSelectInfo.Move);
        board.Fen = pos.FEN;
        engine.StartAnalysisAsync(board.Fen);
        vmoves = new List<ExtendedMove>(moveSelectInfo.Game.Moves.GetRange(0, moveSelectInfo.MoveIndex + 1));
    }</strong>

    void OnMovePlayed(MovePlayedInfo mpi)
    {
        engine.StartAnalysisAsync(mpi.NewFen);
        <del>game.Add(new ExtendedMove(mpi.Move));</del>
        <strong>if (vmoves == null)
            game.Add(new ExtendedMove(mpi.Move));
        else
        {
            vmoves.Add(new ExtendedMove(mpi.Move));
            game.AddVariation(vmoves);
        }</strong>
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

The main addition is the event callback `OnMoveSelected` which is triggered whenever the
user clicks on a move within the scoresheet. In this method we determine the position after
the clicked move and pass it to the chessboard component.

In `vmoves` we store the list of moves, which lead to this position. We need this list to add 
variations, when based on the current position a new move is made.

![Screenshot of application with scoresheet with variations](../articles/img/tutorial_1_5a.png) 

You can see the result of the tutorial live at [https://ponziani.de/analysisboard](https://ponziani.de/analysisboard)