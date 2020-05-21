using System.Drawing;
using System.Windows.Forms;

class View : Form {
    Game game;
    Player[] players;
    Move lastMove = null;
    Pos moveFrom = null;
    bool wasCapture;
    
    const int Square = 80;  // square size in pixels
    Bitmap blackPawn = new Bitmap("black_pawn.png"),
           whitePawn = new Bitmap("white_pawn.png");
    
    public View(Game game, Player[] players) {
        this.game = game;
        this.players = players;
        DoubleBuffered = true;
        ClientSize = new Size(Square * Game.Size, Square * Game.Size);
        StartPosition = FormStartPosition.CenterScreen;
        setTitle();
    }
    
    void setTitle() {
        string seed = game.seed >= 0 ? $"({game.seed})" : "";
        string name1 = players[1] == null ? "Human" : players[1].GetType().Name;
        string name2 = players[2].GetType().Name;
        Text = $"Breakthrough{seed}: white = {name1} | black = {name2}";
    }
    
    void move() {
        lastMove = players[game.turn].chooseMove(game);
        wasCapture = game.move(lastMove);
        Invalidate();
    }
    
    void highlight(Graphics g, int x, int y) {
        Pen highlight = new Pen(Color.Green, 4);
        g.DrawRectangle(highlight, Square * x, Square * y, Square, Square);
    }
    
    protected override void OnPaint(PaintEventArgs args) {
        Graphics g = args.Graphics;
        
        for (int x = 0 ; x < Game.Size ; ++x)
            for (int y = 0 ; y < Game.Size ; ++y) {
                g.FillRectangle((x + y) % 2 == 0 ? Brushes.PaleGoldenrod : Brushes.Peru,
                                Square * x, Square * y, Square, Square);
                if (game.winner > 0 && game.squares[x, y] == game.winner)
                    g.FillRectangle(Brushes.LightGreen,
                                    Square * x + 4, Square * y + 4, Square - 8, Square - 8);
                if (lastMove != null && wasCapture &&
                    x == lastMove.to.x && y == lastMove.to.y) {
                        Pen p = new Pen(Color.DarkGray, 4);
                        g.DrawLine(p, Square * x + 4, Square * y + 4,
                                      Square * (x + 1) - 4, Square * (y + 1) - 4);
                        g.DrawLine(p, Square * x + 4, Square * (y + 1) - 4,
                                      Square * (x + 1) - 4, Square * y + 4);
                }
                if (game.squares[x, y] > 0)
                    g.DrawImage(game.squares[x, y] == 1 ? whitePawn : blackPawn,
                                Square * x, Square * y);
            }
        
        if (moveFrom != null)
            highlight(g, moveFrom.x, moveFrom.y);
        else if (lastMove != null) {
            highlight(g, lastMove.from.x, lastMove.from.y);
            highlight(g, lastMove.to.x, lastMove.to.y);
        }
    }
    
    bool gameOver() {
        if (game.winner > 0) {
            Application.Exit();
            return true;
        }
        return false;
    }
    
    protected override void OnKeyDown (KeyEventArgs e) {
        if (gameOver()  || players[1] == null)
            return;
            
        if (e.KeyCode == Keys.Space)
            move();
    }
    
    protected override void OnMouseDown (MouseEventArgs e) {
        if (gameOver())
            return;

        if (game.moves == 0) {
            players[1] = null;
            setTitle();
        } else if (players[1] != null)
            return;

        int x = e.X / Square, y = e.Y / Square;
        
        if (moveFrom != null) {
            Move move = new Move(moveFrom, new Pos(x, y));
            if (game.validMove(move)) {
                moveFrom = null;
                game.move(move);
                Invalidate();
                if (game.winner == 0)
                    this.move();  // opponent move
                return;
            }
        }
        if (game.squares[x, y] == 1) {
            moveFrom = new Pos(x, y);
            Invalidate();
        }
    }
    
    public static void run(Game game, Player[] players) {
        Application.Run(new View(game, players));
    }
}
