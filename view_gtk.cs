using Cairo;
using Gdk;
using Gtk;
using System;
using System.Collections.Generic;
using CairoHelper = Gdk.CairoHelper;
using Window = Gtk.Window;



class View : Window {
    Game game;
    Player[] players;
    Move lastMove = null;
    Pos moveFrom = null;
    bool wasCapture;
    Stack<(Move, bool)> undoStack = new Stack<(Move, bool)>();
    bool undone = false, isSimulation = false;
    const int Square = 80;  // square size in pixels
    Pixbuf blackPawn = new Pixbuf("black_pawn.png"),
           whitePawn = new Pixbuf("white_pawn.png");

    public View(Game game, Player[] players) : base("") {
        this.game = game;
        this.players = players;
        isSimulation = game.seed > 0;
        Resize(Square * Game.Size, Square * Game.Size);
        AddEvents((int) (EventMask.KeyPressMask | EventMask.ButtonPressMask));
        setTitle();
    }
    
    void setTitle() {
        string seed = game.seed >= 0 ? $"({game.seed})" : "";
        string name1 = players[1] == null ? "Human" : players[1].GetType().Name;
        string name2 = players[2].GetType().Name;
        Title = $"Breakthrough{seed}: white = {name1} | black = {name2}";
    }
    
    void move() {
        undone = false;
        lastMove = players[game.turn].chooseMove(game);
        wasCapture = game.move(lastMove);
        undoStack.Push((lastMove, wasCapture));
        QueueDraw();
    }

    void unmove() {
        if (isSimulation) {
            if (undoStack.Count == 0) Application.Quit();
            else {
                undone = true;
                var oldMove = undoStack.Pop();
                Move undoMove = oldMove.Item1;
                bool undoCapture = oldMove.Item2;
                game.unmove(undoMove, undoCapture);
                QueueDraw();
            }
        }
    }
    
    static RGBA color(string name) {
        RGBA c = new RGBA();
        if (!c.Parse(name))
            throw new Exception("unknown color");
        return c;
    }
    
    static void drawLine(Context c, RGBA color, int lineWidth, int x1, int y1, int x2, int y2) {
        CairoHelper.SetSourceRgba(c, color);
        c.LineWidth = lineWidth;
        c.MoveTo(x1, y1);
        c.LineTo(x2, y2);
        c.Stroke();
    }
    
    static void drawRectangle(Context c, RGBA color, int lineWidth, int x, int y, int width, int height) {
        CairoHelper.SetSourceRgba(c, color);
        c.LineWidth = lineWidth;
        c.Rectangle(x, y, width, height);
        c.Stroke();
    }
    
    static void fillRectangle(Context c, RGBA color, int x, int y, int width, int height) {
        CairoHelper.SetSourceRgba(c, color);
        c.Rectangle(x, y, width, height);
        c.Fill();
    }
    
    static void drawImage(Context c, Pixbuf pixbuf, int x, int y) {
        CairoHelper.SetSourcePixbuf(c, pixbuf, x, y);
        c.Paint();
    }
    
    void highlight(Context c, RGBA color, int x, int y) {
        drawRectangle(c, color, 4, Square * x, Square * y, Square, Square);
    }
    
    protected override bool OnDrawn(Context c) {
        RGBA peru = color("peru"), paleGoldenrod = color("paleGoldenrod"),
              lightGreen = color("lightGreen"), darkGray = color("darkGray"),
              green = color("green");
        
        for (int x = 0 ; x < Game.Size ; ++x)
            for (int y = 0 ; y < Game.Size ; ++y) {
                fillRectangle(c, (x + y) % 2 == 0 ? paleGoldenrod : peru,
                                Square * x, Square * y, Square, Square);
                if (game.winner > 0 && game.squares[x, y] == game.winner)
                    fillRectangle(c, lightGreen,
                                    Square * x + 4, Square * y + 4, Square - 8, Square - 8);
                if (lastMove != null && (!undone) && wasCapture &&
                    x == lastMove.to.x && y == lastMove.to.y) {
                        drawLine(c, darkGray, 4, Square * x + 4, Square * y + 4,
                                        Square * (x + 1) - 4, Square * (y + 1) - 4);
                        drawLine(c, darkGray, 4, Square * x + 4, Square * (y + 1) - 4,
                                        Square * (x + 1) - 4, Square * y + 4);
                }
                if (game.squares[x, y] > 0)
                    drawImage(c, game.squares[x, y] == 1 ? whitePawn : blackPawn,
                                Square * x, Square * y);
            }
        
        if (!undone && moveFrom != null)
            highlight(c, green, moveFrom.x, moveFrom.y);
        else if (!undone && lastMove != null) {
            highlight(c, green, lastMove.from.x, lastMove.from.y);
            highlight(c, green, lastMove.to.x, lastMove.to.y);
        }
        
        return true;
    }

    bool gameOver() {
        if (game.winner > 0) {
            Application.Quit();
            return true;
        }
        return false;
    }

    protected override bool OnKeyPressEvent (EventKey e) {
        if (e.Key == Gdk.Key.z) {
            unmove();
            return true;
        }
        if (gameOver() || players[1] == null)
            return true;
        
        if (e.Key == Gdk.Key.space)
            move();
        
        return true;
    }
    
    protected override bool OnButtonPressEvent (EventButton e) {
        if (gameOver())
            return true;
            
        if (game.moves == 0) {
            players[1] = null;
            setTitle();
        } else if (players[1] != null)
            return true;

        int x = (int) e.X / Square, y = (int) e.Y / Square;
        
        if (moveFrom != null) {
            Move move = new Move(moveFrom, new Pos(x, y));
            if (game.validMove(move)) {
                moveFrom = null;
                game.move(move);
                QueueDraw();
                if (game.winner == 0)
                    this.move();  // opponent move
                return true;
            }
        }
        if (game.squares[x, y] == 1) {
            moveFrom = new Pos(x, y);
            QueueDraw();
        }
        
        return true;
    }
    
    protected override bool OnDeleteEvent(Event ev) {
        Application.Quit();
        return true;
    }
    
    public static void run(Game game, Player[] players) {
        Application.Init();
        View v = new View(game, players);
        v.ShowAll();
        Application.Run();
    }
}