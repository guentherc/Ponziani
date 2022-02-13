# PonzianiComponents #

### Chess API ###
A [Chess API](api/PonzianiComponents.Chesslib.html), offering chess and chess960 related functionality like legal move generation,
PGN parsing, interacting with UCI chess engines and ECO classification


### Blazor Components ###
PonzianiComponents also offers a set of Blazor components allowing to create chess related Blazor apps.
So far these components are available:
1. [Chessboard](api/PonzianiComponents.Chessboard.html)
	a component allowing to show an interactive chessboard, where moves can be be played by drag&drop
2. [Scoresheet](api/PonzianiComponents.Scoresheet.html)
	a component which can be used to list the moves (including comments and variations) of a chess game either in tabular or inline mode
3. [Engine](api/PonzianiComponents.Engine.html)
	a component which allows to integrate an engine (Stockfish 14.1 WASM) into your Blazor App
    * [EngineLog](api/PonzianiComponents.EngineLog.html) 
		a component to output the engine's log
	* [EvaluationGauge](api/PonzianiComponents.EvaluationGauge.html) 
		a component rendering an evaluation bar

## Installation ##

`dotnet add package PonzianiComponents --version 0.5.1`

## License ##

[GPL-3.0](../LICENSE)

## Demos ##
[https://ponziani.de/](https://ponziani.de/)

## Usage ##

Please have a look at the [API introduction](articles/chesslib_overview.html) and at the [tutorial](articles/tutorial_1_1.html)

## Acknowledgements ##
- Chris Oakman: The rendering and default colors were inspired by resp. copied from [chessboard.js](https://chessboardjs.com/) (MIT License)
- SCID: The ECO codes were taken from [SCID](http://scid.sourceforge.net/) (GPL v2)
- .NET Foundation: HSL Color determination and manipulation was copied and adjusted from .Net Core source code (MIT License)
- [Stockfish Team](https://github.com/official-stockfish/Stockfish) for the great chess engine and [Nathan Rugg](https://github.com/nmrugg/stockfish.js) for the port to WebAssembly (GPLv3)
