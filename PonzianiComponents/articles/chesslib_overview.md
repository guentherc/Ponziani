# Chess Library - Overview #

PonzianiComponents contains a chess library helping to create chess-related applications. This library is 
independent of Blazor and can be used in any .Net 6 based application.

The library is contained in namespace [PonzianiComponents.Chesslib](/api/PonzianiComponents.Chesslib.html) 

Here are some examples, how to use it:

### Move generation ###

    using PonzianiComponents.Chesslib;

    //Create a position object from FEN string
    Position pos = new("rnbqkbnr/pp2pppp/3p4/1Bp5/4P3/5N2/PPPP1PPP/RNBQK2R b KQkq - 1 3");
    //Get all legal moves
    var moves = pos.GetMoves();
    //Output moves in uci- and in SAN-Notation
    foreach(var move in moves)
    {
        Console.WriteLine(move.ToUCIString() + " " + pos.ToSAN(move));
    }
    
The output should be something like

    b8c6 Nc6
    b8d7 Nd7
    c8d7 Bd7
    d8d7 Qd7

### Handling Chess Games ###

The Game object allows to process complete chess games:

    using PonzianiComponents.Chesslib;

    //Creates a new game 
    Game game = new();

    //Apply moves (first 3 moves from the immortal game https://en.wikipedia.org/wiki/Immortal_Game)
    game.Add(new("e2e4"));
    game.Add(new("e7e5"));
    game.Add(new("f2f4"));
    game.Add(new("e5f4"));
    game.Add(new("f1c4"));
    game.Add(new("d8h4"));

    //It's now white's turn to make the 4th move
    Console.WriteLine($"Movenumber:         { game.Position.MoveNumber }");
    Console.WriteLine($"SideToMove:         { game.SideToMove }");

    //White is in check, this can be tested with the API
    Console.WriteLine($"Checked:            { game.Position.IsCheck }");

    //It's now white's turn to make the 4th move
    Console.WriteLine($"Movenumber:         { game.Position.MoveNumber }");

    //Let's get the movetext in SAN-Notation
    Console.WriteLine($"Startposition:      { game.StartPosition }");
    Console.WriteLine($"Movetext:           { game.SANNotation() }");

    //Get the position from 2nd move after white played 2. f2f4
    Position pos = game.GetPosition(2, Side.BLACK);
    Console.WriteLine($"Fen after 2. f2-f4: { pos.FEN }");

    //We can also output the position as ASCII graph
    Console.WriteLine(pos.ASCII());

This should give 

    Movenumber:         4
    SideToMove:         WHITE
    Checked:            True
    Movenumber:         4
    Startposition:      rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1
    Movetext:           1. e4 e5 2. f4 exf4 3. Bc4 Qh4+ *
    Fen after 2. f2-f4: rnbqkbnr/pppp1ppp/8/4p3/4PP2/8/PPPP2PP/RNBQKBNR b KQkq - 0 2
      a   b   c   d   e   f   g   h
    ---------------------------------
    | r | n | b | q | k | b | n | r | 8
    ---------------------------------
    | p | p | p | p |   | p | p | p | 7
    ---------------------------------
    |   |   |   |   |   |   |   |   | 6
    ---------------------------------
    |   |   |   |   | p |   |   |   | 5
    ---------------------------------
    |   |   |   |   | P | P |   |   | 4
    ---------------------------------
    |   |   |   |   |   |   |   |   | 3
    ---------------------------------
    | P | P | P | P |   |   | P | P | 2
    ---------------------------------
    | R | N | B | Q | K | B | N | R | 1
    ---------------------------------


### Working with PGN files ###

Ponziani also offers methods to read and write [PGN (Portable Game Notation)](http://www.saremba.de/chessgml/standards/pgn/pgn-complete.htm) files.

Reading is simple. You can either read a PGN-File from disk with
    
    PGN pgn = new PGN(fileName);
    var games = pgn.LoadAsync().Result;

or get the pgn from somewhere else as string

    //Get the last 10 games of Magnus Carlsen (alias DrNykterstein) as pgn from lichess
    HttpClient client = new HttpClient();
    string pgn = client.GetStringAsync("https://lichess.org/api/games/user/DrNykterstein?tags=true&clocks=false&evals=false&opening=false&max=10&perfType=ultraBullet%2Cbullet%2Cblitz%2Crapid%2Cclassical").Result;
    //Parse pgn
    var games = PGN.Parse(pgn);
    //Output some header information for each game
    foreach (var game in games)
    {
        Console.WriteLine($"{ game.White } - { game.Black } {game.Result} Moves: {game.Position.MoveNumber}");
    }
    //Get final position of last game
    Position pos = games.Last().Position;
    Console.WriteLine($"Final Position: {pos.FEN}");
    //and output it as ASCII
    Console.WriteLine(pos.ASCII());

This code snippet should output something like 

    DrNykterstein - may6enexttime WHITE_WINS Moves: 28
    may6enexttime - DrNykterstein BLACK_WINS Moves: 61
    DrNykterstein - may6enexttime WHITE_WINS Moves: 69
    may6enexttime - DrNykterstein BLACK_WINS Moves: 38
    DrNykterstein - may6enexttime WHITE_WINS Moves: 27
    may6enexttime - DrNykterstein BLACK_WINS Moves: 29
    DrNykterstein - may6enexttime WHITE_WINS Moves: 34
    may6enexttime - DrNykterstein BLACK_WINS Moves: 60
    DrNykterstein - may6enexttime WHITE_WINS Moves: 36
    may6enexttime - DrNykterstein BLACK_WINS Moves: 34
    Final Position: 6k1/5pp1/1pp4p/8/1P1N4/3Q4/3p1PPP/2q1rRK1 w - - 4 34
      a   b   c   d   e   f   g   h
    ---------------------------------
    |   |   |   |   |   |   | k |   | 8
    ---------------------------------
    |   |   |   |   |   | p | p |   | 7
    ---------------------------------
    |   | p | p |   |   |   |   | p | 6
    ---------------------------------
    |   |   |   |   |   |   |   |   | 5
    ---------------------------------
    |   | P |   | N |   |   |   |   | 4
    ---------------------------------
    |   |   |   | Q |   |   |   |   | 3
    ---------------------------------
    |   |   |   | p |   | P | P | P | 2
    ---------------------------------
    |   |   | q |   | r | R | K |   | 1
    ---------------------------------

To write PGN the Game class offers method [ToPGN()](/api/PonzianiComponents.Chesslib.Game.html#PonzianiComponents_Chesslib_Game_ToPGN_PonzianiComponents_Chesslib_IPGNOutputFormatter_System_Boolean_)
which returns the PGN as string.

    //Creates a new game 
    Game game = new();
    //Set game header data
    game.White = "Adolf Anderssen";
    game.Black = "Lionel Kieseritzky";
    game.Date = new DateTime(1851, 6, 21).ToShortDateString();
    //Apply moves (first 3 moves from the immortal game https://en.wikipedia.org/wiki/Immortal_Game)
    game.Add(new("e2e4"));
    game.Add(new("e7e5"));
    game.Add(new("f2f4"));
    game.Add(new("e5f4"));
    game.Add(new("f1c4"));
    game.Add(new("d8h4"));
    //export to PGN
    string pgn = game.ToPGN();
    Console.WriteLine(pgn);

This will print 

    [Site ""]
    [Date "21.06.1851"]
    [Round ""]
    [White "Adolf Anderssen"]
    [Black "Lionel Kieseritzky"]
    [Result "*"]
    [ECO "C33"]
    [Opening "KGA: Bishop's Gambit"]
    [Termination "unterminated"]

    1. e4 e5 2. f4 exf4 3. Bc4 Qh4+ *

to the console. 

**Please note:** The ECO and Opening tags haven't been set automatically.
They were added automatically. ECO classification is another feature of the API.