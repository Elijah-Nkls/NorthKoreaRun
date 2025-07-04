using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Northkorea_Run
{
    public partial class Form1 : Form
    {
        private System.Windows.Forms.Timer gameTimer;
        private List<Rectangle> walls;
        private List<Point> points;
        private List<Ghost> ghosts;
        private Rectangle exitDoor, player;
        private int cellSize = 40, mazeSize = 15;
        private int playerSpeed = 8, playerDx, playerDy, wantedDx, wantedDy;
        private bool doorOpen, showPoster, posterUsedThisLevel, cheatActive, kimMode, inGame, gameOver, gameWon;
        private bool[] levelCompleted = new bool[3];
        private bool allLevelsCompleted;
        private string eggBuffer = "";
        private DateTime cheatStart = DateTime.MinValue, playerLastMoved = DateTime.Now;
        private Button btnFührer, btnEgal;
        private Image startMenuImage, gameOverImage, winImage, endCongratsImage;

        private List<List<string>> mazes = new List<List<string>>
        {
            new List<string>
            {
                // Level 1...
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
                // Level 2...
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
                // Level 3...
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

            // Bilder laden
            startMenuImage = Image.FromFile("Bilder/startmenu.png");
            gameOverImage = Image.FromFile("Bilder/gameover.png");
            winImage = Image.FromFile("Bilder/Abschluss gelungen.png");
            endCongratsImage = Image.FromFile("Bilder/ende.png");

            InitializeStartScreen();
        }

        private void InitializeComponent()
        {
            this.gameTimer = new System.Windows.Forms.Timer { Interval = 50 };
            gameTimer.Tick += GameTimer_Tick;

            this.Size = new Size(cellSize * mazeSize, cellSize * mazeSize);
            this.DoubleBuffered = true;
            this.Paint += DrawGameWindow;
            this.KeyDown += HandleKeyDown;
            this.PreviewKeyDown += (s, e) => e.IsInputKey = true;
        }

        private void InitializeStartScreen()
        {
            // Reset aller Zustände
            RemovePosterButtons();
            inGame = gameOver = gameWon = showPoster = posterUsedThisLevel = cheatActive = kimMode = false;
            allLevelsCompleted = false;
            Controls.Clear();
            eggBuffer = "";

            // Drei unsichtbare Buttons für Level-Auswahl
            int btnW = 155, btnH = 72, btnY = 410;
            int[] btnX = { 33, 222, 410 };
            for (int i = 0; i < 3; i++)
            {
                var btn = new Button
                {
                    Text = "",
                    Tag = i,
                    Size = new Size(btnW, btnH),
                    Location = new Point(btnX[i], btnY),
                    BackColor = Color.Transparent,
                    FlatStyle = FlatStyle.Flat,
                    TabStop = false,
                    ForeColor = Color.Transparent,
                    Font = new Font("Segoe UI", 24, FontStyle.Bold)
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseDownBackColor = btn.FlatAppearance.MouseOverBackColor = Color.Transparent;
                btn.Click += (s, e) =>
                {
                    int lvl = (int)btn.Tag;
                    if (lvl > 0 && !levelCompleted[lvl - 1])
                    {
                        MessageBox.Show(
                            $"Du musst Level {lvl} abschließen, um Level {lvl + 1} freizuschalten!",
                            "Level gesperrt", MessageBoxButtons.OK, MessageBoxIcon.Information
                        );
                        return;
                    }
                    StartLevel(lvl);
                };
                Controls.Add(btn);
            }

            Invalidate();
        }

        private void StartLevel(int mazeIndex)
        {
            RemovePosterButtons();
            posterUsedThisLevel = false;
            inGame = true;
            currentLevel = mazeIndex;

            walls = new List<Rectangle>();
            points = new List<Point>();
            ghosts = new List<Ghost>();
            Controls.Clear();

            var maze = mazes[mazeIndex];
            var empty = new List<Point>();
            for (int y = 0; y < maze.Count; y++)
                for (int x = 0; x < maze[y].Length; x++)
                    if (maze[y][x] == '1')
                        walls.Add(new Rectangle(x * cellSize, y * cellSize, cellSize, cellSize));
                    else
                        empty.Add(new Point(x, y));

            // Spieler-Spawn
            var spawn = empty[0];
            player = new Rectangle(spawn.X * cellSize + 5, spawn.Y * cellSize + 5, cellSize - 10, cellSize - 10);
            playerDx = playerDy = wantedDx = wantedDy = 0;
            playerLastMoved = DateTime.Now;

            // Punkte verteilen
            var rnd = new Random();
            var free = empty.Skip(1).ToList();
            var chosen = new HashSet<int>();
            while (chosen.Count < 5 && chosen.Count < free.Count)
                chosen.Add(rnd.Next(free.Count));
            foreach (var i in chosen)
                points.Add(new Point(free[i].X * cellSize, free[i].Y * cellSize));

            // Ausgang
            exitDoor = new Rectangle(13 * cellSize, 13 * cellSize, cellSize, cellSize);
            doorOpen = false;

            // Geister
            int gSize = cellSize - 10;
            foreach (var c in free.Take(3))
                ghosts.Add(new Ghost(c.X * cellSize + 5, c.Y * cellSize + 5, gSize, 3));

            gameTimer.Start();
        }

        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            if (!inGame)
            {
                // Easter Egg Kim-Mode
                if (e.KeyCode >= Keys.A && e.KeyCode <= Keys.Z)
                {
                    eggBuffer += (char)e.KeyCode;
                    if (eggBuffer.Length > 6) eggBuffer = eggBuffer[^6..];
                    if (eggBuffer.ToUpper().EndsWith("KIM")) { kimMode = true; Invalidate(); }
                    if (eggBuffer.ToUpper().EndsWith("RESET")) { kimMode = false; Invalidate(); }
                }
            }
            else
            {
                // Bewegung
                switch (e.KeyCode)
                {
                    case Keys.Left: wantedDx = -1; wantedDy = 0; break;
                    case Keys.Right: wantedDx = 1; wantedDy = 0; break;
                    case Keys.Up: wantedDx = 0; wantedDy = -1; break;
                    case Keys.Down: wantedDx = 0; wantedDy = 1; break;
                }
                // Cheat
                if (e.Control && e.Alt && e.KeyCode == Keys.L && !cheatActive)
                {
                    cheatActive = true;
                    cheatStart = DateTime.Now;
                    playerSpeed = 11;
                    Invalidate();
                }
            }
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (!inGame) return;

            // Poster-Idle
            if (!showPoster && !posterUsedThisLevel && (DateTime.Now - playerLastMoved).TotalSeconds > 10)
            {
                showPoster = true;
                ShowPosterButtons();
                posterUsedThisLevel = true;
                Invalidate();
                return;
            }
            if (showPoster) return;

            // Cheat auslaufen
            if (cheatActive && (DateTime.Now - cheatStart).TotalSeconds > 10)
            {
                cheatActive = false;
                playerSpeed = 8;
            }

            // Richtungswechsel
            var wish = new Rectangle(player.X + wantedDx * playerSpeed,
                                     player.Y + wantedDy * playerSpeed,
                                     player.Width, player.Height);
            if (!walls.Any(w => w.IntersectsWith(wish)))
                (playerDx, playerDy) = (wantedDx, wantedDy);

            // Bewegung
            var next = new Rectangle(player.X + playerDx * playerSpeed,
                                     player.Y + playerDy * playerSpeed,
                                     player.Width, player.Height);
            if (!walls.Any(w => w.IntersectsWith(next)))
            {
                player = next;
                playerLastMoved = DateTime.Now;
            }

            // Punkte
            int keySize = 32, off = (cellSize - keySize) / 2;
            points.RemoveAll(p => new Rectangle(p.X + off, p.Y + off, keySize, keySize).IntersectsWith(player));

            // Tür öffnen
            if (!doorOpen && points.Count == 0)
                doorOpen = true;

            // Geister bewegen & Kollision
            foreach (var gh in ghosts) gh.MoveTowards(player, walls);
            if (ghosts.Any(gh => gh.Rect.IntersectsWith(player)))
            {
                ShowGameOverScreen();
                return;
            }

            // Level geschafft?
            if (doorOpen && exitDoor.IntersectsWith(player))
            {
                gameTimer.Stop();
                ShowWinScreen(currentLevel);
                return;
            }

            Invalidate();
        }

        private void ShowGameOverScreen()
        {
            RemovePosterButtons();
            gameOver = true;
            inGame = false;
            gameTimer.Stop();
            Controls.Clear();
            var retry = new Button
            {
                Text = "",
                Size = new Size(370, 90),
                Location = new Point((ClientSize.Width - 370) / 2, (ClientSize.Height - 90) / 2),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                TabStop = false,
                ForeColor = Color.Transparent
            };
            retry.FlatAppearance.BorderSize = 0;
            retry.FlatAppearance.MouseOverBackColor = retry.FlatAppearance.MouseDownBackColor = Color.Transparent;
            retry.Click += (s, e) => InitializeStartScreen();
            Controls.Add(retry);
            Invalidate();
        }

        private void ShowWinScreen(int levelIndex)
        {
            RemovePosterButtons();
            gameWon = true;
            inGame = false;
            gameTimer.Stop();
            Controls.Clear();

            levelCompleted[levelIndex] = true;
            if (levelCompleted.All(x => x))
            {
                allLevelsCompleted = true;
                Invalidate();
                return;
            }

            var weiter = new Button
            {
                Text = "",
                Size = new Size(617, 98),
                Location = new Point((ClientSize.Width - 617) / 2, (ClientSize.Height - 98) / 2),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                TabStop = false,
                ForeColor = Color.Transparent
            };
            weiter.FlatAppearance.BorderSize = 0;
            weiter.FlatAppearance.MouseOverBackColor = weiter.FlatAppearance.MouseDownBackColor = Color.Transparent;
            weiter.Click += (s, e) => InitializeStartScreen();
            Controls.Add(weiter);
            Invalidate();
        }

        private void DrawGameWindow(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            if (inGame)
            {
                // ... Spielfeld nach oben ...
                foreach (var w in walls) g.FillRectangle(Brushes.DarkRed, w);
                int ks = 32, off = (cellSize - ks) / 2;
                foreach (var p in points) g.FillEllipse(Brushes.Gold, new Rectangle(p.X + off, p.Y + off, ks, ks));
                if (doorOpen)
                {
                    g.FillRectangle(Brushes.SaddleBrown, exitDoor);
                    g.DrawRectangle(Pens.Yellow, exitDoor);
                }
                if (kimMode) DrawPlayerAsKimHead(g, player);
                else g.FillEllipse(Brushes.Gold, player);
                foreach (var gh in ghosts) DrawGhostAsFlag(g, gh.Rect);
                if (cheatActive)
                {
                    string msg = "SUPER-SPEED + UNVERWUNDBAR!";
                    Font f = new Font("Arial", 18, FontStyle.Bold);
                    var sz = g.MeasureString(msg, f);
                    g.DrawString(msg, f, Brushes.Yellow, (ClientSize.Width - sz.Width) / 2, 8);
                    f.Dispose();
                }
                return;
            }
            // Nicht im Spiel: GameOver oder Win
            g.Clear(Color.Black);
            if (gameOver && gameOverImage != null)
                g.DrawImage(gameOverImage, 0, 0, ClientSize.Width, ClientSize.Height);
            else if (gameWon)
            {
                if (allLevelsCompleted && endCongratsImage != null)
                    g.DrawImage(endCongratsImage, 0, 0, ClientSize.Width, ClientSize.Height);
                else if (winImage != null)
                    g.DrawImage(winImage, 0, 0, ClientSize.Width, ClientSize.Height);
            }
            else if (startMenuImage != null)
                g.DrawImage(startMenuImage, 0, 0, ClientSize.Width, ClientSize.Height);
        }

        private void ShowPosterButtons()
        {
            // ...
        }

        private void RemovePosterButtons()
        {
            // ...
        }

        private void DrawGhostAsFlag(Graphics g, Rectangle rect)
        {
            g.FillEllipse(Brushes.Red, rect);
            var star = StarPoints(rect.X + rect.Width / 2, rect.Y + rect.Height / 2,
                                  rect.Width / 2.5f, rect.Width / 5.5f, 5, -90);
            g.FillPolygon(Brushes.Gold, star);
        }

        private PointF[] StarPoints(float cx, float cy, float outerR, float innerR,
                                   int numPoints, float startAngleDeg)
        {
            var pts = new List<PointF>();
            double angle = startAngleDeg * Math.PI / 180.0, step = Math.PI / numPoints;
            for (int i = 0; i < numPoints * 2; i++)
            {
                double r = (i % 2 == 0) ? outerR : innerR;
                pts.Add(new PointF((float)(cx + Math.Cos(angle) * r), (float)(cy + Math.Sin(angle) * r)));
                angle += step;
            }
            return pts.ToArray();
        }

        private void DrawPlayerAsKimHead(Graphics g, Rectangle rect)
        {
            g.FillEllipse(Brushes.Gold, rect);
            g.FillEllipse(Brushes.Black, new Rectangle(rect.X, rect.Y, rect.Width, rect.Height / 2));
            int cx = rect.X + rect.Width / 2, cy = rect.Y + rect.Height / 2, r = rect.Width / 5;
            g.FillEllipse(Brushes.Black, cx - r - 8, cy - 3, r * 2, r);
            g.FillEllipse(Brushes.Black, cx + 8 - r, cy - 3, r * 2, r);
            g.FillRectangle(Brushes.Black, cx - 4, cy - 1, 8, 2);
            g.DrawArc(new Pen(Color.Red, 2), cx - 8, cy + 4, 16, 8, 20, 140);
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
                double best = double.MaxValue;
                Point bestDir = new Point();
                foreach (var d in dirs)
                {
                    var nxt = Rect;
                    nxt.X += d.X * eff; nxt.Y += d.Y * eff;
                    if (!walls.Any(w => w.IntersectsWith(nxt)))
                    {
                        double dist = Math.Sqrt(Math.Pow(player.X - nxt.X, 2) + Math.Pow(player.Y - nxt.Y, 2));
                        if (dist < best) { best = dist; bestDir = d; }
                    }
                }
                Rect = new Rectangle(Rect.X + bestDir.X * eff, Rect.Y + bestDir.Y * eff, Rect.Width, Rect.Height);
            }
        }
    }
}