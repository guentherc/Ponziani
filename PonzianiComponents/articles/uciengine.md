# Communication with an UCI Chess Engine #

The class [UCIEngine](../api/PonzianiComponents.Chesslib.UCIEngine.html) offers options to control and interact with chess engines
which support the [UCI protocol](https://en.wikipedia.org/wiki/Universal_Chess_Interface)

### Analyzing Single Positions ###

The Example shows 2 sequential positions from the game Short - Kasparov (London, 1993)

    using (UCIEngine engine = new UCIEngine(enginePath))
    {
        string fen = "rn2kb1r/pbpp1p1p/5n1q/1B4p1/3PPp2/2N2N2/PPP3PP/R1BQ1K1R w kq - 1 9";
        ExtendedMove bestMove = engine.AnalyzeAsync(fen, TimeSpan.FromSeconds(2)).Result;
        Console.WriteLine($"Best Move: {(new Position(fen)).ToSAN(bestMove)}  Evaluation: {bestMove.Evaluation}");

        // update fen to position after playing h4 (the best move) 
        fen = "rn2kb1r/pbpp1p1p/5n1q/1B4p1/3PPp1P/2N2N2/PPP3P1/R1BQ1K1R b kq - 0 9";
        bestMove = engine.AnalyzeAsync(fen, TimeSpan.FromSeconds(2)).Result;
        Console.WriteLine($"Best Move: {(new Position(fen)).ToSAN(bestMove)}  Evaluation: {bestMove.Evaluation}");
    }

will output something like

    Best Move: h4  Evaluation: 128
    Best Move: g4  Evaluation: -113 

**Note:** The evaluation changed it's sign although the best move has been played between the 2 positions. This 
is because the `ExtendedMove.Evaluation` gives the Evaluation from engine's point of view 

### Selfplay ###

    using (UCIEngine engine = new UCIEngine(enginePath))
    {
        Game game = new Game();
        while (game.Result == Result.OPEN)
        {
            ExtendedMove move = engine.AnalyzeAsync(game, TimeSpan.FromMilliseconds(100)).Result;
            game.Add(move);
        }
        //engine.Name and Author are only available once the first AnalyseAsync 
        game.White = engine.Name; game.Black = engine.Name;
        Console.WriteLine(game.ToPGN(new CutechessCommenter()));
    }

This will create a selfplay bullet game of the loaded engine. The Parameter `new CutechessCommenter()`
passed to `game.ToPGN` will add the engine depth, score and thinktime as comment to the PGN output.

### Engine-Engine Match ###

Creating an engine match between 2 different engines is just as simple

    using (UCIEngine engine1 = new UCIEngine(enginePath1))
    {
        using (UCIEngine engine2 = new UCIEngine(enginePath2))
        {
            Game game = new Game();
            while (game.Result == Result.OPEN)
            {
                ExtendedMove move = game.Moves.Count % 2 == 0 ? engine1.AnalyzeAsync(game, TimeSpan.FromMilliseconds(100)).Result
                                                                : engine2.AnalyzeAsync(game, TimeSpan.FromMilliseconds(100)).Result;
                game.Add(move);
            }
            //engine.Name and Author are only available once the first AnalyseAsync 
            game.White = engine1.Name; game.Black = engine2.Name;
            Console.WriteLine(game.ToPGN(new CutechessCommenter()));
        }
    }

Usually engine-engine matches aren't played with fix move times. Instead engine's have their own time management
and the GUI is only responsible of managing the clocks.
To make this possible there is an overload of [`AnalyzeAsync`](../api/PonzianiComponents.Chesslib.UCIEngine.html#PonzianiComponents_Chesslib_UCIEngine_AnalyzeAsync_PonzianiComponents_Chesslib_Game_System_Int32_PonzianiComponents_Chesslib_Side_System_Nullable_System_TimeSpan__System_Nullable_System_TimeSpan__System_Nullable_System_TimeSpan__System_Nullable_System_TimeSpan__System_Int32_System_Int32_System_Int64_System_Boolean_System_Collections_Generic_Dictionary_System_String_System_String__System_Collections_Generic_List_PonzianiComponents_Chesslib_Move__),
which allows to pass the clock information to the engines.

Here is the code for a 1+0 bullet game between 2 different engines

    using (UCIEngine engine1 = new UCIEngine(enginePath))
    {
        using (UCIEngine engine2 = new UCIEngine(enginePath2))
        {
            //Clocks in ms
            int[] clocks = new int[] { 60000, 60000 };
            Game game = new Game();
            while (game.Result == Result.OPEN)
            {
                UCIEngine engine = game.Moves.Count % 2 == 0 ? engine1 : engine2;
                ExtendedMove move = engine.AnalyzeAsync(game, game.Position.MoveNumber, game.Position.SideToMove, 
                    TimeSpan.FromMilliseconds(clocks[0]), null, TimeSpan.FromMilliseconds(clocks[1]), null).Result;
                //to keep the example simple we will trust the engine's time information
                clocks[game.Moves.Count % 2] -= (int)move.UsedThinkTime.TotalMilliseconds;
                game.Add(move);
            }
            //engine.Name and Author are only available once the first AnalyseAsync 
            game.White = engine1.Name; game.Black = engine2.Name;
            Console.WriteLine(game.ToPGN(new CutechessCommenter()));
        }
    }

If you run this code, there will be no output for about 2 minutes. In the next section you can see, how to fix this.

### Events ###

UCIEngine offers 2 events:

1. **[OnEngineInfoChanged](../api/PonzianiComponents.Chesslib.UCIEngine.html#PonzianiComponents_Chesslib_UCIEngine_OnEngineInfoChanged)** which is issued whenever the engine outputs a new evaluation
2. **[OnEngineOutput](../api/PonzianiComponents.Chesslib.UCIEngine.html#PonzianiComponents_Chesslib_UCIEngine_OnEngineOutput)** which is raised at each message from the engine

The [OnEngineInfoChanged](../api/PonzianiComponents.Chesslib.UCIEngine.html#PonzianiComponents_Chesslib_UCIEngine_OnEngineInfoChanged) event provides the engine information parsed as
[Info](../api/PonzianiComponents.Chesslib.UCIEngine.Info.html) object, while the [OnEngineOutput](../api/PonzianiComponents.Chesslib.UCIEngine.html#PonzianiComponents_Chesslib_UCIEngine_OnEngineOutput) passes
the raw message as issued from the engine. 

For the purpose to get ongoing evaluation information during an engine-engine match the [OnEngineInfoChanged](../api/PonzianiComponents.Chesslib.UCIEngine.html#PonzianiComponents_Chesslib_UCIEngine_OnEngineInfoChanged) event is sufficient.

<pre>
    using (UCIEngine engine1 = new UCIEngine(enginePath))
    {
<strong>
        engine1.OnEngineInfoChanged += (sender, e) => { 
            if (e.Info.Depth > 8) Console.Write($"\r{e.Info.Evaluation} {engine1.Name}                       "); 
        };
</strong>
        using (UCIEngine engine2 = new UCIEngine(enginePath2))
        {
<strong>
            engine2.OnEngineInfoChanged += (sender, e) => { 
                if (e.Info.Depth > 8) Console.Write($"\r{-e.Info.Evaluation} {engine2.Name}                     "); 
            };
</strong>
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
            Console.WriteLine();
            Console.WriteLine(game.ToPGN(new CutechessCommenter()));
        }
    }
<pre>