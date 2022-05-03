using System;
using System.Collections.Generic;
using System.Diagnostics;

using static System.Console;
using static System.Math;

class Pos {
    public int x, y;
    
    public Pos(int x, int y) { this.x = x; this.y = y; }
    
    public override string ToString() => $"({x}, {y})";
}

class Move {
    public Pos from, to;
    
    public Move(Pos from, Pos to) { this.from = from; this.to = to; }
    
    public override string ToString() => $"{from} -> {to}";
}

// A game of Breakthrough.
class Game {
    public const int Size = 7;
    
    // squares[x, y] holds the piece at square (x, y):
    // 0 = empty, 1 = player 1 (white), 2 = player 2 (black)
    public int[,] squares = new int[Size, Size];
    
    // pieces[p] is the number of pieces currently owned by player p.
    // (The dummy entry pieces[0] is unused.)
    public int[] pieces = new int[3];
    
    // The player whose turn it is to play.
    public int turn = 1;
    
    // The number of moves that have been made in the game so far.
    public int moves = 0;
    
    // The player who has won, or 0 if nobody has won yet.
    public int winner = 0;
    
    // The random seed used to start this game, or -1 if none.
    public int seed;
    public SRandom random;
    
    public Game(int seed) {
        this.seed = seed;
        random = new SRandom(seed);

        for (int x = 0 ; x < Size ; ++x) {
            squares[x, 0] = squares[x, 1] = 2;
            squares[x, Size - 2] = squares[x, Size - 1] = 1;
        }
        pieces[1] = pieces[2] = 2 * Size;
    }
    
    // Create an independent copy of the game state.
    public Game clone() {
        Game g = (Game) MemberwiseClone();
        g.squares = (int[,]) squares.Clone();
        g.pieces = (int[]) pieces.Clone();
        return g;
    }
    
    public int dir() => turn == 1 ? -1 : 1;
    
    bool valid(Pos pos) => pos.x >= 0 && pos.x < Game.Size &&
                           pos.y >= 0 && pos.y < Game.Size;
    
    // Return true if the given move is valid.
    public bool validMove(Move move) {
        Pos from = move.from, to = move.to;
        
        return valid(from) && valid(to) &&
               squares[from.x, from.y] == turn &&
               to.y - from.y == dir() &&
               Abs(to.x - from.x) <= 1 &&
               squares[to.x, to.y] != turn &&
               !(from.x == to.x && squares[to.x, to.y] > 0);
    }
    
    // Return a list of all possible moves for the current player.
    public List<Move> possibleMoves() {
        var ret = new List<Move>();
        for (int x = 0 ; x < Size ; ++x)
            for (int y = 0 ; y < Size ; ++y)
                if (squares[x, y] == turn) {
                    Pos from = new Pos(x, y);
                    for (int x1 = x - 1; x1 <= x + 1 ; ++x1) {
                        Move move = new Move(from, new Pos(x1, y + dir()));
                        if (validMove(move))
                            ret.Add(move);
                    }
                }
        return ret;
    }
    
    // Update the game by having the current player make the given move.
    // Returns true if a capture was made.
    public bool move(Move m) {
        if (validMove(m)) {
            Pos from = m.from, to = m.to;
            bool capture = (squares[to.x, to.y] > 0);
            squares[from.x, from.y] = 0;
            squares[to.x, to.y] = turn;
            if (capture)
                pieces[3 - turn] -= 1;
            if (turn == 1 && to.y == 0 || turn == 2 && to.y == Size - 1 ||
                pieces[3 - turn] == 0)
                winner = turn;
            turn = 3 - turn;
            moves += 1;
            return capture;
        } else throw new Exception("illegal move");
    }
    
    // Reverse a previous move that was made.  When calling this method,
    // wasCapture must be true if the move was a capture.
    public void unmove(Move m, bool wasCapture) {
        moves -= 1;
        turn = 3 - turn;
        winner = 0;
        if (wasCapture)
            pieces[3 - turn] += 1;
        squares[m.to.x, m.to.y] = wasCapture ? 3 - turn : 0;
        squares[m.from.x, m.from.y] = turn;
    }
}

// A Player is a strategy for playing the game.  Given any game state,
// the Player's chooseMove method decides what move to make.
interface Player {
    Move chooseMove(Game game);
}

class Program {
    static void simulate(Player?[] players, int games) {
        string name(int p) => players[p]!.GetType().Name;
        
        WriteLine($"playing {games} games");
        int[] moves = new int[3];
        long[] elapsed = new long[3];
        int[] wins = new int[3];
        
        for (int x = 0 ; x < games ; ++x) {
            Game game = new Game(x);
            while (game.winner == 0) {
                Stopwatch sw = Stopwatch.StartNew();
                Move move = players[game.turn]!.chooseMove(game.clone());
                sw.Stop();
                moves[game.turn] += 1;
                elapsed[game.turn] += sw.ElapsedMilliseconds;
                game.move(move);
            }
            WriteLine($"game {x}: winner = {name(game.winner)}");
            wins[game.winner] += 1;
        }
        
        WriteLine($"{name(1)} won {wins[1]}, {name(2)} won {wins[2]}");
        
        double avg1 = 1.0 * elapsed[1] / moves[1],
               avg2 = 1.0 * elapsed[2] / moves[2];
        WriteLine($"{name(1)} used {avg1:f1} ms/move, {name(2)} used {avg2:f1} ms/move");
    }
    
    [STAThread]
    static void Main(string[] args) {
        Player?[] players = { null, new MyAgent(), new Clever() };
        int seed = -1;

        for (int i = 0 ; i < args.Length ; ++i)
            switch (args[i]) {
                case "-seed":
                    seed = int.Parse(args[i + 1]);
                    i += 1;
                    break;
                case "-sim":
                    simulate(players, int.Parse(args[i + 1]));
                    return;
                case "-swap":
                    (players[1], players[2]) = (players[2], players[1]);
                    break;
                default:
                    WriteLine("unknown option: " + args[i]);
                    WriteLine("usage: breakthrough [-seed <num>] [-swap] [-sim <num>]");
                    return;
            }
            
        View.run(new Game(seed), players);
    }
}
