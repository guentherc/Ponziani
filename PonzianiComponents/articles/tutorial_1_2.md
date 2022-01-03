# Tutorial - Simple Board with Automatic Egine Analysis

We will use the generated Pages\Index.razor page in the PonzianiComponents.Client project as home of our analyisis board. 
To do this we have to replace it's content with

    @page "/"

    <Chessboard OnMovePlayed="OnMovePlayed"/>
    <Engine @ref="engine"/>

    @code {
        private Engine engine;

        void OnMovePlayed(MovePlayedInfo mpi)
        {
            engine.StartAnalysisAsync(mpi.NewFen);
        }
    }

Let us have a short look at the code. With

`<Chessboard OnMovePlayed="OnMovePlayed"/>`

we are adding a chessboard component to our page. This will result in showing the interactive chessboard on your page. Whenever
the user is applying a move (by drag & drop) the event callback `OnMovePlayed` is triggered.
The next line adds an Engine component to the page:
    
`<Engine @ref="engine"/>`

This will set up the embedded engine. To interact with it we capture a component reference in variable `engine`.
Finally we have to connect the event callback with the engine: 

        void OnMovePlayed(MovePlayedInfo mpi)
        {
            engine.StartAnalysisAsync(mpi.NewFen);
        }

After every move we will send the new position as [FEN](https://en.wikipedia.org/wiki/Forsyth%E2%80%93Edwards_Notation)-string to the engine. The engine
will start to analyze the position and we will see the analysis output in the Engine Info Panel.

![Screenshot of application](/articles/img/tutorial_1_2.png) 

> [!div class="nextstepaction"]
> [Next - Adding an Evaluation Bar](tutorial_1_3.md)