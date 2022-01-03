# UCI Engine Communication - Advanced Topics #

In the [overview article](/articles/uciengine.html) it is described that it's possible to implement most use-cases by using method 
[`UCIEngine.AnalyzeAsync`](/api/PonzianiComponents.Chesslib.UCIEngine.html#PonzianiComponents_Chesslib_UCIEngine_AnalyzeAsync_System_String_System_TimeSpan_System_Collections_Generic_Dictionary_System_String_System_String__System_Collections_Generic_List_PonzianiComponents_Chesslib_Move__).
For special use-cases there are some lower-level methods available.

### Direct Communication with Engine Process ###

If you want to implement the GUI part of the UCI protocol (almost) completely on your own, you can use 
- [`UCIEngine.StartEngineAsync`](/api/PonzianiComponents.Chesslib.UCIEngine.html#PonzianiComponents_Chesslib_UCIEngine_StartEngineAsync) to start the engine,
- [`UCIEngine.SendToEngineAsync`](/api/PonzianiComponents.Chesslib.UCIEngine.html#PonzianiComponents_Chesslib_UCIEngine_SendToEngineAsync_System_String_) to send commands to the engine
- and Event [OnEngineOutput](/api/PonzianiComponents.Chesslib.UCIEngine.html#PonzianiComponents_Chesslib_UCIEngine_OnEngineOutput) to receive messages from the engine

Here is some very sloppy code to illustrate this scenario: 

    using (UCIEngine engine1 = new UCIEngine(enginePath))
    {
        RunAnalysis(engine1).Wait();
    }

    private async static Task<bool> RunAnalysis(UCIEngine engine1)
    {
        TaskCompletionSource<bool> tsc = new TaskCompletionSource<bool>();
        engine1.OnEngineOutput += async (sender, e) =>
        {
            if (e.Message == "uciok") await engine1.SendToEngineAsync("isready");
            else if (e.Message == "readyok")
            {
                await engine1.SendToEngineAsync("position startpos moves e2e4 d7d5 e4d5");
                await engine1.SendToEngineAsync("go movetime 1000");
            }
            else if (e.Message.StartsWith("bestmove"))
            {
                Console.WriteLine("Best Move:" + e.Message.Substring(9));
                tsc.SetResult(true);
            }
        };
        bool started = engine1.StartEngineAsync().Result;
        return await tsc.Task;
    }

**Note:** If you wonder why [`UCIEngine.SendToEngineAsync`](/api/PonzianiComponents.Chesslib.UCIEngine.html#PonzianiComponents_Chesslib_UCIEngine_SendToEngineAsync_System_String_) 
is async: The method doesn't send the command directly to the engine, but analyzes the command, updates the classes internal state and in some
cases, if the command is sent at the wrong point in time, tries to fix that. So e.g. if a 'go' command is sent, while the engine
is still thinking, UCIEngine first issues a 'stop' command, then waits for the engine to send it's 'bestmove' message and
only then the 'go' command is sent to the engine.
This approach makes it possible to offer some methods, which offer more control as the high-level 
[`AnalyzeAsync`](/api/PonzianiComponents.Chesslib.UCIEngine.html#PonzianiComponents_Chesslib_UCIEngine_AnalyzeAsync_PonzianiComponents_Chesslib_Game_System_Int32_PonzianiComponents_Chesslib_Side_System_Nullable_System_TimeSpan__System_Nullable_System_TimeSpan__System_Nullable_System_TimeSpan__System_Nullable_System_TimeSpan__System_Int32_System_Int32_System_Int64_System_Boolean_System_Collections_Generic_Dictionary_System_String_System_String__System_Collections_Generic_List_PonzianiComponents_Chesslib_Move__) overloads,
but without forcing you to implement the UCI protocol completely by yourself. These methods are described in the next section.

### Further Methods ###

- [SetOptionsAsync](/api/PonzianiComponents.Chesslib.UCIEngine.html#PonzianiComponents_Chesslib_UCIEngine_SetOptionsAsync) can be used to send the options stored in property [Parameters](/api/PonzianiComponents.Chesslib.UCIEngine.html#PonzianiComponents_Chesslib_UCIEngine_Parameters)
  to the engine
- [NewGameAsync](/api/PonzianiComponents.Chesslib.UCIEngine.html#PonzianiComponents_Chesslib_UCIEngine_NewGameAsync) will send the 'ucinewgame' followed by an 'isready' command.
- [SetPositionAsync](/api/PonzianiComponents.Chesslib.UCIEngine.html#PonzianiComponents_Chesslib_UCIEngine_SetPositionAsync_System_String_System_String_) will send the 'setposition' command.
- [StartAnalysisAsync](/api/PonzianiComponents.Chesslib.UCIEngine.html#PonzianiComponents_Chesslib_UCIEngine_StartAnalysisAsync_System_Collections_Generic_List_PonzianiComponents_Chesslib_Move__) will send the 'go' command and it will return once the analysis is
done (unless an infinite analysis started, in that case the method finishes once analysis is started)
- [StartThinkingAsync](/api/PonzianiComponents.Chesslib.UCIEngine.html#PonzianiComponents_Chesslib_UCIEngine_StartThinkingAsync_System_Nullable_System_TimeSpan__System_Nullable_System_TimeSpan__System_Nullable_System_TimeSpan__System_Nullable_System_TimeSpan__System_Int32_System_Int32_System_Int64_System_Boolean_System_Collections_Generic_List_PonzianiComponents_Chesslib_Move__) allows to start an engine analysis with all
options available with the 'go' command. 
- [StopThinkingAsync](/api/PonzianiComponents.Chesslib.UCIEngine.html#PonzianiComponents_Chesslib_UCIEngine_StopThinkingAsync) Sends the 'stop' command to tell the engine to stop the current analysis

All these methods are asynchronous. If you call them you simply have to wait until they finish. You don't have to listen to the engine's
output to determine the point in time when the engine is ready to process the next command.
