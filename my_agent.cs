using System.Collections.Generic;

class MyAgent : Player {
    
    // This default implementation moves completely randomly.
    public Move chooseMove(Game game) {
        List<Move> possible = game.possibleMoves();
        return possible[game.random.next(possible.Count)];
    }
    
}
