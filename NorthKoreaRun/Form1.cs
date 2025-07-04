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
        private List<Rectangle>? walls = null, ghostsRects = null;
        private List<Point>? points = null;
        private List<Ghost>? ghosts = null;
        private Rectangle player;
        private int playerSpeed = 8, playerDx, playerDy, wantedDx, wantedDy, cellSize = 40, mazeSize = 15, currentLevel;
        private bool inGame, gameOver, gameWon, kimMode, showPoster, cheatActive, posterUsedThisLevel;
        private bool[] levelCompleted = new bool[3];
        private string eggBuffer = "";
        private DateTime playerLastMoved = DateTime.Now, cheatStart = DateTime.MinValue;
        private Button? btnFührer = null, btnEgal = null;
        private Image? startMenuImage;
        private Image? keyImage;
        private Rectangle exitDoor;
        private bool doorOpen;
        private Image? winImage;
        private Image? gameOverImage;
        private Image? endCongratsImage;

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
            int btnWidth = 160;
            int btnCount = 3;
            int space = 38;
            int totalWidth = btnCount * btnWidth + (btnCount - 1) * space;
            int startX = (ClientSize.Width - totalWidth) / 2 + 50;

            endCongratsImage = Image.FromFile("Bilder/ende.png");
            gameOverImage = Image.FromFile("Bilder/gameover.png");
            winImage = Image.FromFile("Bilder/Abschluss gelungen.png");
            keyImage = Image.FromFile("Bilder/schluessel.png");
            startMenuImage = Image.FromFile("Bilder/startmenu.png");

            Size = new Size(cellSize * mazeSize + 16, cellSize * mazeSize + 39);
            DoubleBuffered = true;
            Paint += new PaintEventHandler(DrawGameWindow);
            KeyDown += new KeyEventHandler(HandleKeyDown);
            PreviewKeyDown += (s, e) => { e.IsInputKey = true; };

            gameTimer = new System.Windows.Forms.Timer { Interval = 50 };
            gameTimer.Tick += new EventHandler(GameTimer_Tick);

            InitializeStartScreen();
        }

        private void InitializeStartScreen()
        {
            RemovePosterButtons();
            posterUsedThisLevel = gameOver = gameWon = inGame = showPoster = cheatActive = false;
            Controls.Clear();
            eggBuffer = "";

            int btnWidth = 155, btnHeight = 72, btnY = 410;
            int[] btnX = { 33, 222, 410 };

            for (int i = 0; i < 3; i++)
            {
                Button btn = new Button()
                {
                    Text = "",
                    Location = new Point(btnX[i], btnY),
                    Size = new Size(btnWidth, btnHeight),
                    Font = new Font("Segoe UI", 24, FontStyle.Bold),
                    Tag = i,
                    BackColor = Color.Transparent,
                    FlatStyle = FlatStyle.Flat,
                    TabStop = false,
                    ForeColor = Color.Transparent,
                    Visible = true
                };

                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = Color.Transparent;
                btn.FlatAppearance.MouseDownBackColor = Color.Transparent;

                btn.Click += (s, e) =>
                {
                    int tag = (int)btn.Tag;
                    if (tag == 1 && !levelCompleted[0])
                    {
                        MessageBox.Show(
                            "Du musst erst Abschnitt 1 abschließen, bevor du Abschnitt 2 spielen kannst!",
                            "Abschnitt gesperrt", MessageBoxButtons.OK, MessageBoxIcon.Information
                        );
                        return;
                    }
                    if (tag == 2 && !levelCompleted[1])
                    {
                        MessageBox.Show(
                            "Du musst erst Abschnitt 2 abschließen, bevor du Abschnitt 3 spielen kannst!",
                            "Abschnitt gesperrt", MessageBoxButtons.OK, MessageBoxIcon.Information
                        );
                        return;
                    }
                    StartLevel(tag);
                };

                Controls.Add(btn);
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

            int btnX = 114, btnY = 485, btnW = 370, btnH = 90;
            Button retry = new Button()
            {
                Text = "",
                Size = new Size(btnW, btnH),
                Location = new Point(btnX, btnY),
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

        private bool allLevelsCompleted = false;

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

            int imgBaseWidth = 768, imgBaseHeight = 1152;
            int btnOrigX = 268, btnOrigY = 604, btnOrigW = 617, btnOrigH = 98;
            float scaleX = (float)ClientSize.Width / imgBaseHeight;
            float scaleY = (float)ClientSize.Height / imgBaseWidth;
            int btnX = (int)(btnOrigX * scaleX);
            int btnY = (int)(btnOrigY * scaleY);
            int btnW = (int)(btnOrigW * scaleX);
            int btnH = (int)(btnOrigH * scaleY);

            Button weiter = new Button()
            {
                Text = "",
                Size = new Size(btnW, btnH),
                Location = new Point(btnX, btnY),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                TabStop = false,
                ForeColor = Color.Transparent
            };
            weiter.FlatAppearance.BorderSize = 0;
            weiter.FlatAppearance.MouseOverBackColor = Color.Transparent;
            weiter.FlatAppearance.MouseDownBackColor = Color.Transparent;
            weiter.Click += (s, e) => InitializeStartScreen();

            Controls.Add(weiter);
            Invalidate();
        }

        private void StartLevel(int mazeIndex)
        {
            RemovePosterButtons();
            posterUsedThisLevel = false;
            if ((mazeIndex == 1 && !levelCompleted[0]) ||
                (mazeIndex == 2 && !levelCompleted[1]))
            {
                MessageBox.Show("Du musst erst den vorherigen Abschnitt schaffen!", "Abschnitt gesperrt");
                return;
            }

            walls = new List<Rectangle>();
            points = new List<Point>();
            ghosts = new List<Ghost>();
            Controls.Clear();
            currentLevel = mazeIndex;

            var maze = mazes[mazeIndex];
            List<Point> emptyCells = new List<Point>();
            for (int y = 0; y < maze.Count; y++)
                for (int x = 0; x < maze[y].Length; x++)
                    if (maze[y][x] == '1')
                        walls.Add(new Rectangle(x * cellSize, y * cellSize, cellSize, cellSize));
                    else
                        emptyCells.Add(new Point(x, y));

            var spawnPlayer = emptyCells[0];
            player = new Rectangle(
                spawnPlayer.X * cellSize + 5,
                spawnPlayer.Y * cellSize + 5,
                cellSize - 10,
                cellSize - 10
            );
            playerDx = playerDy = wantedDx = wantedDy = 0;

            Random rnd = new Random();
            var freeForPoints = emptyCells
                .Where(p => p != spawnPlayer)
                .ToList();
            var chosenPoints = new HashSet<int>();
            while (chosenPoints.Count < 5 && chosenPoints.Count < freeForPoints.Count)
                chosenPoints.Add(rnd.Next(freeForPoints.Count));
            points = chosenPoints
                .Select(i => new Point(
                    freeForPoints[i].X * cellSize,
                    freeForPoints[i].Y * cellSize))
                .ToList();

            var forGhosts = emptyCells
                .Where(p => p != spawnPlayer)
                .Where(p => !points.Any(pt => pt.X / cellSize == p.X && pt.Y / cellSize == p.Y))
                .ToList();
            int ghostSize = cellSize - 10;
            if (forGhosts.Count >= 3)
            {
                ghosts.Add(new Ghost(
                    forGhosts[^1].X * cellSize + 5,
                    forGhosts[^1].Y * cellSize + 5,
                    ghostSize, 3));
                ghosts.Add(new Ghost(
                    forGhosts[forGhosts.Count / 2].X * cellSize + 5,
                    forGhosts[forGhosts.Count / 2].Y * cellSize + 5,
                    ghostSize, 3));
                ghosts.Add(new Ghost(
                    forGhosts[forGhosts.Count / 3].X * cellSize + 5,
                    forGhosts[forGhosts.Count / 3].Y * cellSize + 5,
                    ghostSize, 3));
            }
            else
            {
                for (int i = 0; i < Math.Min(3, forGhosts.Count); i++)
                    ghosts.Add(new Ghost(
                        forGhosts[rnd.Next(forGhosts.Count)].X * cellSize + 5,
                        forGhosts[rnd.Next(forGhosts.Count)].Y * cellSize + 5,
                        ghostSize, 3));
            }

            inGame = true;
            gameOver = gameWon = showPoster = cheatActive = false;
            playerLastMoved = DateTime.Now;

            exitDoor = new Rectangle(13 * cellSize, 13 * cellSize, cellSize, cellSize);
            doorOpen = false;

            Invalidate();
            gameTimer.Start();
        }

        private void GameTimer_Tick(object? sender, EventArgs? e)
        {
            if (!inGame) return;

            if (!posterUsedThisLevel
                && !showPoster
                && (DateTime.Now - playerLastMoved).TotalSeconds > 10)
            {
                showPoster = true;
                ShowPosterButtons();
                posterUsedThisLevel = true;
                Invalidate();
                return;
            }
            if (showPoster) return;

            if (cheatActive
                && (DateTime.Now - cheatStart).TotalSeconds > 10)
            {
                cheatActive = false;
                playerSpeed = 8;
            }

            var wantedPos = new Rectangle(
                player.X + wantedDx * playerSpeed,
                player.Y + wantedDy * playerSpeed,
                player.Width, player.Height);
            if (!(walls?.Any(w => w.IntersectsWith(wantedPos)) ?? false))
            {
                if (playerDx != wantedDx || playerDy != wantedDy)
                    playerLastMoved = DateTime.Now;
                playerDx = wantedDx;
                playerDy = wantedDy;
            }

            var nextPos = new Rectangle(
                player.X + playerDx * playerSpeed,
                player.Y + playerDy * playerSpeed,
                player.Width, player.Height);
            if (!(walls?.Any(w => w.IntersectsWith(nextPos)) ?? false))
            {
                if (playerDx != 0 || playerDy != 0)
                    playerLastMoved = DateTime.Now;
                player = nextPos;
            }

            int keySize = 32;
            int offset = (cellSize - keySize) / 2;
            points?.RemoveAll(p =>
                new Rectangle(p.X + offset, p.Y + offset, keySize, keySize)
                .IntersectsWith(player));

            if (ghosts != null)
            {
                foreach (var ghost in ghosts)
                    ghost.MoveTowards(player, walls ?? new List<Rectangle>(), kimMode);
                if (!cheatActive)
                    foreach (var ghost in ghosts)
                        if (ghost.Rect.IntersectsWith(player))
                        {
                            ShowGameOverScreen();
                            return;
                        }
            }

            if (!doorOpen && (points?.Count == 0))
                doorOpen = true;

            if (doorOpen && exitDoor.IntersectsWith(player))
            {
                cheatActive = false;
                playerSpeed = 8;
                ShowWinScreen(currentLevel);
                return;
            }

            Invalidate();
        }

        private void ShowPosterButtons()
        {
            if (btnFührer != null || btnEgal != null) return;
            int w = 220, h = 45;
            btnFührer = new Button()
            {
                Text = "Dem Führer stellen",
                Width = w,
                Height = h,
                Font = new Font("Arial", 14, FontStyle.Bold),
                BackColor = Color.DarkRed,
                ForeColor = Color.White,
                Left = (ClientSize.Width - w) / 2,
                Top = ClientSize.Height / 2 + 30
            };
            btnEgal = new Button()
            {
                Text = "Ist mir egal!",
                Width = w,
                Height = h,
                Font = new Font("Arial", 14, FontStyle.Bold),
                BackColor = Color.DarkGreen,
                ForeColor = Color.White,
                Left = btnFührer.Left,
                Top = btnFührer.Top + h + 16
            };
            btnFührer.Click += (s, e) =>
            {
                RemovePosterButtons();
                showPoster = false;
                ShowGameOverScreen();
            };
            btnEgal.Click += (s, e) =>
            {
                RemovePosterButtons();
                showPoster = false;
                Invalidate();
            };
            Controls.Add(btnFührer);
            Controls.Add(btnEgal);
            btnFührer.BringToFront();
            btnEgal.BringToFront();
        }

        private void RemovePosterButtons()
        {
            if (btnFührer != null)
            {
                Controls.Remove(btnFührer);
                btnFührer.Dispose();
                btnFührer = null;
            }
            if (btnEgal != null)
            {
                Controls.Remove(btnEgal);
                btnEgal.Dispose();
                btnEgal = null;
            }
        }

        private void DrawGameWindow(object? sender, PaintEventArgs? e)
        {
            if (e == null) return;
            Graphics g = e.Graphics;

            if (inGame)
            {
                if (showPoster)
                {
                    g.FillRectangle(Brushes.DarkRed, 0, 0, ClientSize.Width, ClientSize.Height);
                    string msg = "Großer Führer sieht alles!";
                    Font f = new Font("Arial", 36, FontStyle.Bold);
                    SizeF sz = g.MeasureString(msg, f);
                    g.DrawString(msg, f, Brushes.Gold,
                        (ClientSize.Width - sz.Width) / 2,
                        (ClientSize.Height - sz.Height) / 2 - 60);
                    f.Dispose();
                    return;
                }

                walls?.ForEach(w => g.FillRectangle(Brushes.DarkRed, w));

                int ks = 32, off = (cellSize - ks) / 2;
                points?.ForEach(pt =>
                {
                    if (keyImage != null)
                        g.DrawImage(keyImage, new Rectangle(pt.X + off, pt.Y + off, ks, ks));
                    else
                    {
                        g.FillEllipse(Brushes.Gold, new Rectangle(pt.X + off, pt.Y + off, ks, ks));
                        g.DrawEllipse(new Pen(Color.Gray, 2), new Rectangle(pt.X + off, pt.Y + off, ks, ks));
                    }
                });

                if (doorOpen)
                {
                    g.FillRectangle(Brushes.SaddleBrown, exitDoor);
                    g.DrawRectangle(new Pen(Color.Yellow, 4), exitDoor);
                    int knobR = 6;
                    g.FillEllipse(Brushes.Gold,
                        exitDoor.X + exitDoor.Width - 14,
                        exitDoor.Y + exitDoor.Height / 2 - knobR / 2,
                        knobR, knobR);
                }

                if (kimMode)
                    DrawPlayerAsKimHead(g, player);
                else
                    DrawPlayerAsHumanHead(g, player);

                ghosts?.ForEach(ghost => DrawGhostAsFlag(g, ghost.Rect));

                if (cheatActive)
                {
                    string cheatMsg = "SUPER-SPEED + UNVERWUNDBAR!";
                    Font f2 = new Font("Arial", 18, FontStyle.Bold);
                    SizeF sz2 = g.MeasureString(cheatMsg, f2);
                    g.DrawString(cheatMsg, f2, Brushes.Yellow,
                        (ClientSize.Width - sz2.Width) / 2, 8);
                    f2.Dispose();
                }
            }
            else if (gameOver)
            {
                g.Clear(Color.Black);
                if (gameOverImage != null)
                    g.DrawImage(gameOverImage, 0, 0, ClientSize.Width, ClientSize.Height);
            }
            else if (gameWon)
            {
                g.Clear(Color.Black);
                if (allLevelsCompleted)
                {
                    if (endCongratsImage != null)
                        g.DrawImage(endCongratsImage, 0, 0, ClientSize.Width, ClientSize.Height);

                    int bw = 240, bh = 60;
                    int bx = (ClientSize.Width - bw) / 2, by = ClientSize.Height - 110;
                    Button btnEnd = new Button
                    {
                        Text = "",
                        Size = new Size(bw, bh),
                        Location = new Point(bx, by),
                        BackColor = Color.Transparent,
                        FlatStyle = FlatStyle.Flat,
                        TabStop = false,
                        ForeColor = Color.Transparent
                    };
                    btnEnd.FlatAppearance.BorderSize = 0;
                    btnEnd.FlatAppearance.MouseOverBackColor = Color.Transparent;
                    btnEnd.FlatAppearance.MouseDownBackColor = Color.Transparent;
                    btnEnd.Click += (s, e2) => Application.Exit();

                    if (!Controls.OfType<Button>().Any(b => b.Text == ""))
                        Controls.Add(btnEnd);
                }
                else if (winImage != null)
                {
                    g.DrawImage(winImage, 0, 0, ClientSize.Width, ClientSize.Height);
                }
            }
            else
            {
                g.Clear(Color.Black);
                if (startMenuImage != null)
                    g.DrawImage(startMenuImage, 0, 0, ClientSize.Width, ClientSize.Height);
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

        private void DrawPlayerAsHumanHead(Graphics g, Rectangle rect)
        {
            using var skin = new SolidBrush(Color.Peru);
            g.FillEllipse(skin, rect);
            g.FillEllipse(Brushes.SaddleBrown, new Rectangle(rect.X, rect.Y, rect.Width, rect.Height / 2));
            int cx = rect.X + rect.Width / 2, cy = rect.Y + rect.Height / 2;
            int eyeR = Math.Max(2, rect.Width / 10);
            g.FillEllipse(Brushes.Black, cx - eyeR - 3, cy - 3, eyeR, eyeR);
            g.FillEllipse(Brushes.Black, cx + 3, cy - 3, eyeR, eyeR);
            g.DrawArc(new Pen(Color.Black, 2), cx - 8, cy + 4, 16, 8, 20, 140);
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

        private PointF[] StarPoints(float cx, float cy, float outerR, float innerR, int numPoints, float startAngleDeg)
        {
            var pts = new List<PointF>();
            double angle = startAngleDeg * Math.PI / 180.0, step = Math.PI / numPoints;
            for (int i = 0; i < numPoints * 2; i++)
            {
                double r = (i % 2 == 0) ? outerR : innerR;
                pts.Add(new PointF(cx + (float)(Math.Cos(angle) * r), cy + (float)(Math.Sin(angle) * r)));
                angle += step;
            }
            return pts.ToArray();
        }

        private void HandleKeyDown(object? sender, KeyEventArgs? e)
        {
            if (e == null) return;
            if (!inGame)
            {
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
                switch (e.KeyCode)
                {
                    case Keys.Left: wantedDx = -1; wantedDy = 0; break;
                    case Keys.Right: wantedDx = 1; wantedDy = 0; break;
                    case Keys.Up: wantedDx = 0; wantedDy = -1; break;
                    case Keys.Down: wantedDx = 0; wantedDy = 1; break;
                }
                if (e.Control && e.Alt && e.KeyCode == Keys.L && !cheatActive)
                {
                    cheatActive = true;
                    cheatStart = DateTime.Now;
                    playerSpeed = 11;
                    Invalidate();
                }
            }
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
            public void MoveTowards(Rectangle player, List<Rectangle> walls, bool kimMode)
            {
                int effectiveSpeed = kimMode ? speed * 2 : speed;
                var dirs = new[] { new Point(-1, 0), new Point(1, 0), new Point(0, -1), new Point(0, 1) };
                double bestDist = double.MaxValue;
                Point bestDir = new Point();
                foreach (var d in dirs)
                {
                    var next = Rect;
                    next.X += d.X * effectiveSpeed;
                    next.Y += d.Y * effectiveSpeed;
                    if (!walls.Any(w => w.IntersectsWith(next)))
                    {
                        double dist = Math.Sqrt(
                            Math.Pow(player.X - next.X, 2) +
                            Math.Pow(player.Y - next.Y, 2)
                        );
                        if (dist < bestDist)
                        {
                            bestDist = dist;
                            bestDir = d;
                        }
                    }
                }
                Rect = new Rectangle(
                    Rect.X + bestDir.X * effectiveSpeed,
                    Rect.Y + bestDir.Y * effectiveSpeed,
                    Rect.Width,
                    Rect.Height
                );
            }
        }
    }
}