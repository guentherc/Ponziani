# PonzianiComponents #

### Blazor Components ###
PonzianiComponents offers a set of Blazor components allowing to create chess related Blazor apps.
So far these components are available:
1. [Chessboard](api/PonzianiComponents.Chessboard.html)
	a component allowing to show an interactive chessboard, where moves can be be played by drag&drop
2. [Scoresheet](api/PonzianiComponents.Scoresheet.html)
	a component which can be used to list the moves (including comments and variations) of a chess game either in tabular or inline mode

### Chess API ###
There is also a [Chess API](api/PonzianiComponents.Chesslib.html) included, offering chess and chess960 related functionality like legal move generation,
PGN parsing and ECO classification

## Installation ##

`dotnet add package PonzianiComponents --version 0.3.0`

## License ##

[GPL-3.0](../LICENSE)

## Demos ##
[https://ponziani.de/](https://ponziani.de/)

## Usage ##
Add a using reference in your **_Imports.razor**.

```
@using PonzianiComponents
@using PonzianiComponents.Chesslib
``` 

Then add a chessboard to your blazor application

```
<Chessboard/>
``` 

If you want to specify a position

```
<Chessboard Fen="r1bqkbnr/pppp1ppp/2n5/1B2p3/4P3/5N2/PPPP1PPP/RNBQK2R b KQkq - 3 3"/>
```

Adding a scoresheet requires to provide a [Game](api/PonzianiComponents.Chesslib.Game.html) object, which can be created from [PGN](api/PonzianiComponents.Chesslib.PGN.html)-Data

```
Game game = PGN.Parse(pgn, true, true, 1);

<Scoresheet Game="game" Comments="true" Variations="true"></Game>

```

## Acknowledgements ##
- Chris Oakman: The rendering and default colors were inspired by resp. copied from [https://chessboardjs.com/](chessboard.js) (MIT License)
- SCID: The ECO codes were taken from [http://scid.sourceforge.net/](SCID) (GPL v2)
- .NET Foundation: HSL Color determination and manipulation was copied and adjusted from .Net Core source code (MIT License)