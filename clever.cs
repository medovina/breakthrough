using System.Collections.Generic;
using static System.Console;

class Clever : Player {
    bool match(Game game, int within, int x, int y, int player) {
        for (int x1 = x - within; x1 <= x + within ; ++x1)
            if (x1 >= 0 && x1 < Game.Size && game.squares[x1, y] == player)
                return true;
        return false;
    }
    
    public Move chooseMove(Game game) {
        List<Move> possibleMoves = game.possibleMoves();
        
        if (game.moves < 2) {  // my first move
            int count = possibleMoves.Count;
            int index = game.seed >= 0 ? game.seed % count : game.random.next(count);
            return possibleMoves[index];
        }
        int me = game.turn, opp = 3 - me, dir = game.dir();
        
        int adjY(int y) => dir > 0 ? y : Game.Size - 1 - y;
        
        List<Move> best = new List<Move>();
        int bestVal = int.MinValue;
        
        int forwardY = dir > 0 ? int.MinValue : int.MaxValue;
        int forwardOppY = dir > 0 ? int.MaxValue : int.MinValue;
        
        for (int x = 0 ; x < Game.Size ; ++x)
            for (int y = 0 ; y < Game.Size ; ++y) {
                if (game.squares[x, y] == me && (dir > 0 ? y > forwardY : y < forwardY))
                    forwardY = y;
                if (game.squares[x, y] == opp && (dir > 0 ? y < forwardOppY : y > forwardOppY))
                    forwardOppY = y;
            }
        
        foreach (Move m in possibleMoves) {
            int val;
            int gain = 0, lose = 0;
            if (m.to.y == 0 || m.to.y == Game.Size - 1)
                val = 100;
            else {
                int attack = 0, defend = 0;
                for (int dx = -1; dx <= 1 ; dx += 2) {
                    int x1 = m.to.x + dx;
                    if (x1 >= 0 && x1 < Game.Size) {
                        if (game.squares[x1, m.to.y + dir] == opp)
                            attack += 1;
                        if (x1 != m.from.x &&
                            game.squares[x1, m.from.y] == me)
                                defend += 1;
                    }
                }
                if (attack <= defend)
                    lose = gain = attack;
                else {
                    lose = defend + 1;
                    gain = defend;
                }
                bool capture = game.squares[m.to.x, m.to.y] > 0;
                int toY = adjY(m.to.y);
                    
                val = gain * 10 - lose * 10 + toY;
                if (capture)
                    val += toY == 1 ? 30 : 15;
                    
                if (match(game, 1, m.to.x, forwardY, me))
                    val += 3;
                    
                if (adjY(m.from.y) < adjY(forwardOppY) &&
                    match(game, 1, m.from.x, forwardOppY, opp))
                    val -= 2;
                else if (adjY(m.to.y) < adjY(forwardOppY) &&
                         match(game, 1, m.to.x, forwardOppY, opp))
                    val += 2;
                
                if (m.to.x == 0 || m.to.x == Game.Size - 1)
                    val += 1;
            }
            
            if (val > bestVal) {
                best = new List<Move>();
                best.Add(m);
                bestVal = val;
            } else if (val == bestVal)
                best.Add(m);
        }
        
        return best[game.random.next(best.Count)];
    }
}
