## Programming 2: Breakthrough

![screenshot](breakthrough.png)

This is a C# implementation of the game of [Breakthrough](https://en.wikipedia.org/wiki/Breakthrough_(board_game)).  It includes a computer player named Clever that plays using a set of simple heuristics.  You may attempt to write your own agent that can defeat Clever.

The game builds using .NET, and includes a graphical interface in [GTK 3](https://github.com/GtkSharp/GtkSharp) that lets you play interactively against a computer player, or watch two computer players play each other.

The game also includes a command-line interface that lets you run a series of simulated games between two computer players to see which is stronger.

Breakthrough can be played on boards of various sizes, but in this implementation the board is always 7 x 7.

### Playing the game

If you run `dotnet run` with no command-line arguments, the graphical interface appears.  By default, the computer agent Clever will play the black pieces.  You may play white against Clever manually: click on the piece you'd like to move, then click where you'd like it to go.  Alternatively, press the space bar repeatedly to watch MyAgent (white) play against clever (Black).

The default implementation of MyAgent moves randomly.  It stands no chance against Clever, who will defeat it in every game.  You may attempt to improve MyAgent to be smart enough to beat Clever.

You can specify command-line options for Breakthrough like this:

```
$ dotnet run -- [options]
```

To run a series of games between MyAgent and Clever, use the -sim option to specify the number of games.  For example:

```
$ dotnet run -- -sim 5
playing 5 games
game 0: winner = Clever
game 1: winner = Clever
game 2: winner = Clever
game 3: winner = Clever
game 4: winner = Clever
MyAgent won 0, Clever won 5
MyAgent used 0.0 ms/move, Clever used 0.0 ms/move
$ 
```

Each game is played with a different random seed, which is the number displayed next to each game in the output above.  If you want to see the actual series of moves that were played in any of these games, run the executable and pass "-seed \<num>".  That will display the graphical interface and allow you to step through that particular game. You may use the 'z' key to undo the moves in order to study the gameplay better.

Clever plays somewhat randomly, so different random seeds will usually lead to different games even if your agent is deterministic.  In particular, Clever's first move is always random.

One more command line option is available: "-swap" swaps the players, so that MyAgent plays Black and Clever plays White.  This works either when launching the graphical interface or when running a series of simulated games.

### Writing an agent

To improve MyAgent, you will need to modify its chooseMove method, which receives the current game state (a Game object) and decides which move to make.  Read the source code for game.cs to learn about the public fields of the Game class.  Your code can examine these to learn about the current game state.
