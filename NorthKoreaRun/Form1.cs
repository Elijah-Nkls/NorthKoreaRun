using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Northkorea_Run
{
    public partial class Form1 : Form
    {
        private Timer gameTimer;
        private List<Rectangle> walls;
        private List<Point> points;
        private Rectangle exitDoor;
        private bool doorOpen;
        private List<Ghost> ghosts;
        private int cellSize = 40, mazeSize = 15;
        private Rectangle player;
        private int playerSpeed = 8;
        private int playerDx, playerDy;
        private int wantedDx, wantedDy;
        private int currentLevel;
        private bool[] levelCompleted = new bool[3];

        // ? Commit 9: Cheat-Code-Felder
        private bool cheatActive;
        private DateTime cheatStart;

        private List<List<string>> mazes = new List<List<string>>
        {
            new List<string>
            {
                "111111111111111",
                "100000100100001",
                "101110101110101",
                "100000000000101",
                "101011101110101",
                "100000000010001",
                "111011011011101",
                "100010000000001",
                "101011101110101",
                "101000000000101",
                "101110110110101",
                "100000100000001",
                "111011101111101",
                "100000000000001",
                "111111111111111"
            },
            new List<string>
            {
                "111111111111111",
                "100000000100001",
                "101111010101101",
                "101000010001001",
                "101011010101101",
                "101010000000101",
                "100011011110101",
                "101000000010101",
                "101111111000101",
                "100000000010001",
                "101110111110101",
                "100010000000001",
                "101111111111101",
                "100000000000001",
                "111111111111111"
            },
            new List<string>
            {
                "111111111111111",
                "100000000000001",
                "101111010111101",
                "101000010000101",
                "101011111110101",
                "100000000010001",
                "111011111010101",
                "100010000000101",
                "101111111110101",
                "101000000000101",
                "101110111110101",
                "100000100000001",
                "101110111110111",
                "100000000000001",
                "111111111111111"
            }
        };

        public Form1()
        {
            InitializeComponent();

            // ? Commit 9: Cheat-Code initialisieren
            cheatActive = false;
            cheatStart = DateTime.MinValue;

            StartGame();
        }

        private void InitializeComponent()
        {
            this.gameTimer = new Timer { Interval = 50 };
            this.gameTimer.Tick += GameTimer_Tick;
            this.Size = new Size(cellSize * mazeSize, cellSize * mazeSize);
            this.DoubleBuffered = true;
            this.Paint += DrawGameWindow;
            this.KeyDown += HandleKeyDown;
            this.PreviewKeyDown += (s, e) => e.IsInputKey = true;
        }

        private void StartGame()
        {
            walls = new List<Rectangle>();
            points = new List<Point>();
            ghosts = new List<Ghost>();
            Controls.Clear();

            var maze = mazes[currentLevel];
            var emptyCells = new List<Point>();

            // Wände und leere Zellen ermitteln
            for (int y = 0; y < maze.Count; y++)
                for (int x = 0; x < maze[y].Length; x++)
                    if (maze[y][x] == '1')
                        walls.Add(new Rectangle(x * cellSize, y * cellSize, cellSize, cellSize));
                    else
                        emptyCells.Add(new Point(x, y));

            // Spieler an erster leeren Zelle platzieren
            var spawn = emptyCells[0];
            player = new Rectangle(spawn.X * cellSize + 5,
                                   spawn.Y * cellSize + 5,
                                   cellSize - 10,
                                   cellSize - 10);
            playerDx = playerDy = wantedDx = wantedDy = 0;

            // Sammelobjekte (5 Stück) zufällig verteilen
            var rnd = new Random();
            var free = emptyCells.Where(p => !p.Equals(spawn)).ToList();
            var chosen = new HashSet<int>();
            while (chosen.Count < 5 && chosen.Count < free.Count)
                chosen.Add(rnd.Next(free.Count));
            foreach (var i in chosen)
                points.Add(new Point(free[i].X * cellSize, free[i].Y * cellSize));

            // Ausgangstür setzen
            exitDoor = new Rectangle(13 * cellSize, 13 * cellSize, cellSize, cellSize);
            doorOpen = false;

            // Geister spawnen (bis zu 3)
            int ghostSize = cellSize - 10;
            for (int i = 0; i < Math.Min(3, free.Count); i++)
            {
                var c = free[free.Count - 1 - i];
                ghosts.Add(new Ghost(c.X * cellSize + 5, c.Y * cellSize + 5, ghostSize, 3));
            }

            gameTimer.Start();
        }

        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left: wantedDx = -1; wantedDy = 0; break;
                case Keys.Right: wantedDx = 1; wantedDy = 0; break;
                case Keys.Up: wantedDx = 0; wantedDy = -1; break;
                case Keys.Down: wantedDx = 0; wantedDy = 1; break;
            }

            // ? Commit 9: Cheat aktivieren mit Strg+Alt+L
            if (e.Control && e.Alt && e.KeyCode == Keys.L && !cheatActive)
            {
                cheatActive = true;
                cheatStart = DateTime.Now;
                playerSpeed = 11;
                Invalidate();
            }
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            // ? Commit 9: Cheat deaktivieren nach 10 Sekunden
            if (cheatActive && (DateTime.Now - cheatStart).TotalSeconds > 10)
            {
                cheatActive = false;
                playerSpeed = 8;
            }

            // Richtung wechseln, wenn frei
            var wantPos = new Rectangle(
                player.X + wantedDx * playerSpeed,
                player.Y + wantedDy * playerSpeed,
                player.Width, player.Height
            );
            if (!walls.Any(w => w.IntersectsWith(wantPos)))
                (playerDx, playerDy) = (wantedDx, wantedDy);

            // Spieler bewegen
            var nextPos = new Rectangle(
                player.X + playerDx * playerSpeed,
                player.Y + playerDy * playerSpeed,
                player.Width, player.Height
            );
            if (!walls.Any(w => w.IntersectsWith(nextPos)))
                player = nextPos;

            // Punkte einsammeln
            int keySize = 32, offset = (cellSize - keySize) / 2;
            points.RemoveAll(p => new Rectangle(
                p.X + offset, p.Y + offset, keySize, keySize
            ).IntersectsWith(player));

            // Ausgang öffnen
            if (!doorOpen && points.Count == 0)
                doorOpen = true;

            // Geister bewegen & Kollision
            foreach (var gh in ghosts)
                gh.MoveTowards(player, walls);
            foreach (var gh in ghosts)
                if (gh.Rect.IntersectsWith(player))
                {
                    gameTimer.Stop();
                    MessageBox.Show("Game Over!");
                    StartGame();
                    return;
                }

            // Level abschließen
            if (doorOpen && exitDoor.IntersectsWith(player))
            {
                gameTimer.Stop();
                MessageBox.Show("Level completed!");
                StartGame();
            }

            Invalidate();
        }

        private void DrawGameWindow(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.Black);

            // Wände
            foreach (var w in walls)
                g.FillRectangle(Brushes.DarkRed, w);

            // Punkte
            int keySize = 32, offset = (cellSize - keySize) / 2;
            foreach (var p in points)
            {
                g.FillEllipse(Brushes.Gold, new Rectangle(p.X + offset, p.Y + offset, keySize, keySize));
                g.DrawEllipse(Pens.Gray, new Rectangle(p.X + offset, p.Y + offset, keySize, keySize));
            }

            // Ausgang
            if (doorOpen)
            {
                g.FillRectangle(Brushes.SaddleBrown, exitDoor);
                g.DrawRectangle(Pens.Yellow, exitDoor);
                int knobR = 6;
                g.FillEllipse(Brushes.Gold,
                    exitDoor.X + exitDoor.Width - 14,
                    exitDoor.Y + exitDoor.Height / 2 - knobR / 2,
                    knobR, knobR);
            }

            // Spieler
            g.FillEllipse(Brushes.Gold, player);

            // Geister als Flaggen
            foreach (var gh in ghosts)
                DrawGhostAsFlag(g, gh.Rect);

            // ? Commit 9: Cheat-Status einblenden
            if (cheatActive)
            {
                string cheatMsg = "SUPER-SPEED + UNVERWUNDBAR!";
                Font f = new Font("Arial", 18, FontStyle.Bold);
                SizeF sz = g.MeasureString(cheatMsg, f);
                g.DrawString(cheatMsg, f, Brushes.Yellow, (ClientSize.Width - sz.Width) / 2, 8);
                f.Dispose();
            }
        }

        private void DrawGhostAsFlag(Graphics g, Rectangle rect)
        {
            g.FillEllipse(Brushes.Red, rect);
            var star = StarPoints(
                rect.X + rect.Width / 2,
                rect.Y + rect.Height / 2,
                rect.Width / 2.5f,
                rect.Width / 5.5f,
                5, -90
            );
            g.FillPolygon(Brushes.Gold, star);
        }

        private PointF[] StarPoints(
            float cx, float cy,
            float outerR, float innerR,
            int numPoints, float startAngleDeg
        )
        {
            var pts = new List<PointF>();
            double angle = startAngleDeg * Math.PI / 180.0;
            double step = Math.PI / numPoints;
            for (int i = 0; i < numPoints * 2; i++)
            {
                double r = (i % 2 == 0) ? outerR : innerR;
                pts.Add(new PointF(
                    cx + (float)(Math.Cos(angle) * r),
                    cy + (float)(Math.Sin(angle) * r)
                ));
                angle += step;
            }
            return pts.ToArray();
        }

        class Ghost
        {
            public Rectangle Rect;
            private int speed;
            public Ghost(int x, int y, int size, int speed = 2)
            {
                Rect = new Rectangle(x, y, size, size);
                this.speed = speed;
            }
            public void MoveTowards(Rectangle player, List<Rectangle> walls)
            {
                int eff = speed;
                var dirs = new[] { new Point(-1, 0), new Point(1, 0), new Point(0, -1), new Point(0, 1) };
                double bestDist = double.MaxValue;
                Point bestDir = new Point();
                foreach (var d in dirs)
                {
                    var next = Rect;
                    next.X += d.X * eff;
                    next.Y += d.Y * eff;
                    if (!walls.Any(w => w.IntersectsWith(next)))
                    {
                        double dist = Math.Sqrt(
                            Math.Pow(player.X - next.X, 2) + Math.Pow(player.Y - next.Y, 2)
                        );
                        if (dist < bestDist)
                        {
                            bestDist = dist;
                            bestDir = d;
                        }
                    }
                }
                Rect = new Rectangle(
                    Rect.X + bestDir.X * eff,
                    Rect.Y + bestDir.Y * eff,
                    Rect.Width, Rect.Height
                );
            }
        }
    }
}