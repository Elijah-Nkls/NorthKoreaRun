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
        private List<Ghost> ghosts;
        private Rectangle exitDoor, player;
        private int cellSize = 40, mazeSize = 15;
        private int playerSpeed = 8, playerDx, playerDy, wantedDx, wantedDy;
        private bool doorOpen, showPoster, posterUsedThisLevel, cheatActive, kimMode, inGame;
        private bool[] levelCompleted = new bool[3];
        private string eggBuffer = "";
        private DateTime cheatStart = DateTime.MinValue, playerLastMoved = DateTime.Now;
        private Button btnFührer, btnEgal;
        private Image startMenuImage;
        private Image gameOverImage;  // ? neu in Commit 13

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

            // Lade Startmenu-Bild
            startMenuImage = Image.FromFile("Bilder/startmenu.png");
            // ? neu in Commit 13: Game-Over-Bild laden
            gameOverImage = Image.FromFile("Bilder/gameover.png");

            InitializeStartScreen();
        }

        private void InitializeComponent()
        {
            gameTimer = new Timer { Interval = 50 };
            gameTimer.Tick += GameTimer_Tick;

            this.Size = new Size(cellSize * mazeSize, cellSize * mazeSize);
            this.DoubleBuffered = true;
            this.Paint += DrawGameWindow;
            this.KeyDown += HandleKeyDown;
            this.PreviewKeyDown += (s, e) => e.IsInputKey = true;
        }

        private void InitializeStartScreen()
        {
            RemovePosterButtons();
            showPoster = posterUsedThisLevel = cheatActive = kimMode = inGame = false;
            Controls.Clear();

            // Drei Buttons für Level 1–3
            int btnWidth = 155, btnHeight = 72, btnY = 410;
            int[] btnX = { 33, 222, 410 };
            for (int i = 0; i < 3; i++)
            {
                var btn = new Button
                {
                    Text = "",
                    Location = new Point(btnX[i], btnY),
                    Size = new Size(btnWidth, btnHeight),
                    Font = new Font("Segoe UI", 24, FontStyle.Bold),
                    Tag = i,
                    BackColor = Color.Transparent,
                    FlatStyle = FlatStyle.Flat,
                    TabStop = false,
                    ForeColor = Color.Transparent
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseDownBackColor = Color.Transparent;
                btn.FlatAppearance.MouseOverBackColor = Color.Transparent;
                btn.Click += (s, e) =>
                {
                    int level = (int)btn.Tag;
                    if (level > 0 && !levelCompleted[level - 1])
                    {
                        MessageBox.Show(
                            $"Du musst zuerst Level {level} abschließen, um Level {level + 1} freizuschalten!",
                            "Level gesperrt", MessageBoxButtons.OK, MessageBoxIcon.Information
                        );
                        return;
                    }
                    StartLevel(level);
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

            var spawn = empty[0];
            player = new Rectangle(spawn.X * cellSize + 5, spawn.Y * cellSize + 5, cellSize - 10, cellSize - 10);
            playerDx = playerDy = wantedDx = wantedDy = 0;
            playerLastMoved = DateTime.Now;

            var rnd = new Random();
            var freeCells = empty.Skip(1).ToList();
            var chosen = new HashSet<int>();
            while (chosen.Count < 5 && chosen.Count < freeCells.Count)
                chosen.Add(rnd.Next(freeCells.Count));
            foreach (var i in chosen)
                points.Add(new Point(freeCells[i].X * cellSize, freeCells[i].Y * cellSize));

            exitDoor = new Rectangle(13 * cellSize, 13 * cellSize, cellSize, cellSize);
            doorOpen = false;

            int gSize = cellSize - 10;
            foreach (var c in freeCells.Take(3))
                ghosts.Add(new Ghost(c.X * cellSize + 5, c.Y * cellSize + 5, gSize, 3));

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
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (!inGame) return;

            var wish = new Rectangle(
                player.X + wantedDx * playerSpeed,
                player.Y + wantedDy * playerSpeed,
                player.Width, player.Height
            );
            if (!walls.Any(w => w.IntersectsWith(wish)))
                (playerDx, playerDy) = (wantedDx, wantedDy);

            var next = new Rectangle(
                player.X + playerDx * playerSpeed,
                player.Y + playerDy * playerSpeed,
                player.Width, player.Height
            );
            if (!walls.Any(w => w.IntersectsWith(next)))
                player = next;

            int keySize = 32, off = (cellSize - keySize) / 2;
            points.RemoveAll(p => new Rectangle(p.X + off, p.Y + off, keySize, keySize).IntersectsWith(player));

            if (!doorOpen && points.Count == 0)
                doorOpen = true;

            // Geister bewegen
            foreach (var gh in ghosts)
                gh.MoveTowards(player, walls);

            // Kollision: statt MessageBox nun Game-Over-Screen
            if (ghosts.Any(gh => gh.Rect.IntersectsWith(player)))
            {
                ShowGameOverScreen();
                return;
            }

            if (doorOpen && exitDoor.IntersectsWith(player))
            {
                gameTimer.Stop();
                levelCompleted[currentLevel] = true;
                MessageBox.Show("Level completed!");
                InitializeStartScreen();
            }

            Invalidate();
        }

        // Neue Methode aus Commit 13
        private void ShowGameOverScreen()
        {
            RemovePosterButtons();
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
            retry.FlatAppearance.MouseOverBackColor = Color.Transparent;
            retry.FlatAppearance.MouseDownBackColor = Color.Transparent;
            retry.Click += (s, e) => InitializeStartScreen();

            Controls.Add(retry);
            Invalidate();
        }

        private void DrawGameWindow(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.Black);

            if (!inGame)
            {
                // Startmenü oder Game-Over: wenn Controls leer sind, ShowGameOverScreen war aktiv
                if (Controls.Count == 1 && gameOverImage != null)
                    g.DrawImage(gameOverImage, 0, 0, ClientSize.Width, ClientSize.Height);
                else if (startMenuImage != null)
                    g.DrawImage(startMenuImage, 0, 0, ClientSize.Width, ClientSize.Height);
                return;
            }

            // während des Spiels
            foreach (var w in walls)
                g.FillRectangle(Brushes.DarkRed, w);

            int keySz = 32, offSz = (cellSize - keySz) / 2;
            foreach (var p in points)
                g.DrawImage(null, new Rectangle(p.X + offSz, p.Y + offSz, keySz, keySz));

            if (doorOpen)
            {
                g.FillRectangle(Brushes.SaddleBrown, exitDoor);
                g.DrawRectangle(Pens.Yellow, exitDoor);
            }

            g.FillEllipse(Brushes.Gold, player);

            foreach (var gh in ghosts)
                DrawGhostAsFlag(g, gh.Rect);
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

        private PointF[] StarPoints(float cx, float cy, float outerR, float innerR, int numPoints, float startAngleDeg)
        {
            var pts = new List<PointF>();
            double ang = startAngleDeg * Math.PI / 180.0, step = Math.PI / numPoints;
            for (int i = 0; i < numPoints * 2; i++)
            {
                double r = (i % 2 == 0) ? outerR : innerR;
                pts.Add(new PointF(cx + (float)(Math.Cos(ang) * r), cy + (float)(Math.Sin(ang) * r)));
                ang += step;
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
                double best = double.MaxValue;
                Point bestDir = new Point();
                foreach (var d in dirs)
                {
                    var nxt = Rect; nxt.X += d.X * eff; nxt.Y += d.Y * eff;
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