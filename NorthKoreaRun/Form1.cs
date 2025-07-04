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
        private List<Rectangle>? walls = null;
        private List<Point>? points = null;
        private List<Ghost>? ghosts = null;
        private Rectangle player;
        private int playerSpeed = 8;
        private int playerDx, playerDy;       
        private int wantedDx, wantedDy;       
        private int cellSize = 40;
        private int mazeSize = 15;
        private int currentLevel;
        private bool inGame, gameOver, gameWon;
        private bool kimMode;               
        private bool showPoster;            
        private bool cheatActive;             
        private bool posterUsedThisLevel;    
        private bool[] levelCompleted = new bool[3];
        private string eggBuffer = string.Empty;
        private DateTime playerLastMoved = DateTime.Now;
        private DateTime cheatStart = DateTime.MinValue;
        private Button? btnFührer = null;
        private Button? btnEgal = null;
        private Image? startMenuImage;
        private Image? keyImage;
        private Image? winImage;
        private Image? gameOverImage;
        private Image? endCongratsImage;
        private Rectangle exitDoor;
        private bool doorOpen;
        private readonly List<List<string>> mazes = new List<List<string>>
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


        /// Konstruktor: Initialisiert das Fenster, lädt Bilder und startet Startbildschirm

        public Form1()
        {
            // Lade Bilder aus dem Verzeichnis "Bilder"
            endCongratsImage = Image.FromFile("Bilder/ende.png");
            gameOverImage = Image.FromFile("Bilder/gameover.png");
            winImage = Image.FromFile("Bilder/Abschluss gelungen.png");
            keyImage = Image.FromFile("Bilder/schluessel.png");
            startMenuImage = Image.FromFile("Bilder/startmenu.png");

            // Setze Fenstergröße basierend auf Zellgröße und Labyrinthdimension
            Size = new Size(cellSize * mazeSize + 16, cellSize * mazeSize + 39);
            DoubleBuffered = true;  // Verhindert Flackern

            // Ereignishandler für Zeichnen und Tastatureingaben
            Paint += DrawGameWindow;
            KeyDown += HandleKeyDown;
            PreviewKeyDown += (s, e) => { e.IsInputKey = true; };

            // Initialisiere Timer mit 50ms Interval
            gameTimer = new System.Windows.Forms.Timer { Interval = 50 };
            gameTimer.Tick += GameTimer_Tick;

            // Zeige Startmenü
            InitializeStartScreen();
        }


        // Zeigt das Startmenü mit drei Level-Buttons

        private void InitializeStartScreen()
        {
            RemovePosterButtons();
            // Setze Flags und Puffer zurück
            posterUsedThisLevel = gameOver = gameWon = inGame = showPoster = cheatActive = false;
            eggBuffer = string.Empty;
            Controls.Clear();

            // Position und Größe der unsichtbaren Buttons
            int btnWidth = 155, btnHeight = 72, btnY = 410;
            int[] btnX = { 33, 222, 410 };
            for (int i = 0; i < 3; i++)
            {
                var btn = new Button
                {
                    Size = new Size(btnWidth, btnHeight),
                    Location = new Point(btnX[i], btnY),
                    Tag = i,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.Transparent,
                    ForeColor = Color.Transparent,
                    TabStop = false
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = Color.Transparent;
                btn.FlatAppearance.MouseDownBackColor = Color.Transparent;
                btn.Click += (s, e) =>
                {
                    int tag = (int)btn.Tag;
                    // Sperrmechanismus für Level 2 und 3
                    if (tag > 0 && !levelCompleted[tag - 1])
                    {
                        MessageBox.Show(
                            $"Du musst erst Abschnitt {tag} abschließen, bevor du Abschnitt {tag + 1} spielen kannst!",
                            "Abschnitt gesperrt", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    StartLevel(tag);
                };
                Controls.Add(btn);
            }
            Invalidate();
        }


        // Zeigt Bildschirm bei Spielende (Verloren) mit Retry-Button

        private void ShowGameOverScreen()
        {
            RemovePosterButtons();
            gameOver = true;
            inGame = false;
            gameTimer.Stop();
            Controls.Clear();
            var retry = new Button
            {
                Size = new Size(370, 90),
                Location = new Point(114, 485),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.Transparent,
                TabStop = false
            };
            retry.FlatAppearance.BorderSize = 0;
            retry.FlatAppearance.MouseOverBackColor = Color.Transparent;
            retry.FlatAppearance.MouseDownBackColor = Color.Transparent;
            retry.Click += (s, e) => InitializeStartScreen();
            Controls.Add(retry);
            Invalidate();
        }

        private bool allLevelsCompleted = false;

        // Zeigt Bildschirm bei Levelgewinn und verwaltet Fortschritt

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
            // Berechne Button-Position basierend auf Skalierung
            int imgW = 768, imgH = 1152;
            int bX = 268, bY = 604, bW = 617, bH = 98;
            float sX = (float)ClientSize.Width / imgH;
            float sY = (float)ClientSize.Height / imgW;
            var weiter = new Button
            {
                Size = new Size((int)(bW * sX), (int)(bH * sY)),
                Location = new Point((int)(bX * sX), (int)(bY * sY)),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.Transparent,
                TabStop = false
            };
            weiter.FlatAppearance.BorderSize = 0;
            weiter.FlatAppearance.MouseOverBackColor = Color.Transparent;
            weiter.FlatAppearance.MouseDownBackColor = Color.Transparent;
            weiter.Click += (s, e) => InitializeStartScreen();
            Controls.Add(weiter);
            Invalidate();
        }


        // Startet ein Level: Initialisiert Wände, Punkte und Geister

        private void StartLevel(int mazeIndex)
        {
            RemovePosterButtons();
            posterUsedThisLevel = false;
            if (mazeIndex > 0 && !levelCompleted[mazeIndex - 1])
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
            var emptyCells = new List<Point>();
            for (int y = 0; y < maze.Count; y++)
                for (int x = 0; x < maze[y].Length; x++)
                    if (maze[y][x] == '1') walls.Add(new Rectangle(x * cellSize, y * cellSize, cellSize, cellSize));
                    else emptyCells.Add(new Point(x, y));
            // Spieler-Einstieg
            var spawn = emptyCells[0];
            player = new Rectangle(spawn.X * cellSize + 5, spawn.Y * cellSize + 5, cellSize - 10, cellSize - 10);
            playerDx = playerDy = wantedDx = wantedDy = 0;
            var rnd = new Random();
            // Punkte platzieren
            var freePts = emptyCells.Where(p => p != spawn).ToList();
            var chosen = new HashSet<int>();
            while (chosen.Count < 5 && chosen.Count < freePts.Count) chosen.Add(rnd.Next(freePts.Count));
            points = chosen.Select(i => new Point(freePts[i].X * cellSize, freePts[i].Y * cellSize)).ToList();
            // Geister platzieren
            var ghostCells = emptyCells.Where(p => p != spawn).Where(p => !points.Any(pt => pt.X / cellSize == p.X && pt.Y / cellSize == p.Y)).ToList();
            int gSize = cellSize - 10;
            if (ghostCells.Count >= 3)
            {
                ghosts.Add(new Ghost(ghostCells[^1].X * cellSize + 5, ghostCells[^1].Y * cellSize + 5, gSize, 3));
                ghosts.Add(new Ghost(ghostCells[ghostCells.Count / 2].X * cellSize + 5, ghostCells[ghostCells.Count / 2].Y * cellSize + 5, gSize, 3));
                ghosts.Add(new Ghost(ghostCells[ghostCells.Count / 3].X * cellSize + 5, ghostCells[ghostCells.Count / 3].Y * cellSize + 5, gSize, 3));
            }
            else for (int i = 0; i < Math.Min(3, ghostCells.Count); i++) ghosts.Add(new Ghost(ghostCells[rnd.Next(ghostCells.Count)].X * cellSize + 5, ghostCells[rnd.Next(ghostCells.Count)].Y * cellSize + 5, gSize, 3));
            inGame = true; gameOver = gameWon = showPoster = cheatActive = false; playerLastMoved = DateTime.Now;
            exitDoor = new Rectangle(13 * cellSize, 13 * cellSize, cellSize, cellSize); doorOpen = false;
            Invalidate(); gameTimer.Start();
        }


        // Spiel-Loop, ausgeführt bei jedem Timer-Tick

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            if (!inGame) return;
            // Poster-Mechanik nach 10s Inaktivität
            if (!posterUsedThisLevel && !showPoster && (DateTime.Now - playerLastMoved).TotalSeconds > 10)
            {
                showPoster = true; ShowPosterButtons(); posterUsedThisLevel = true; Invalidate(); return;
            }
            if (showPoster) return;
            // Cheat deaktivieren nach 10s
            if (cheatActive && (DateTime.Now - cheatStart).TotalSeconds > 10)
            { cheatActive = false; playerSpeed = 8; }
            // Wunschbewegung prüfen
            var wantRect = new Rectangle(player.X + wantedDx * playerSpeed, player.Y + wantedDy * playerSpeed, player.Width, player.Height);
            if (!(walls?.Any(w => w.IntersectsWith(wantRect)) ?? false))
            { if (playerDx != wantedDx || playerDy != wantedDy) playerLastMoved = DateTime.Now; playerDx = wantedDx; playerDy = wantedDy; }
            // tatsächliche Bewegung
            var next = new Rectangle(player.X + playerDx * playerSpeed, player.Y + playerDy * playerSpeed, player.Width, player.Height);
            if (!(walls?.Any(w => w.IntersectsWith(next)) ?? false))
            { if (playerDx != 0 || playerDy != 0) playerLastMoved = DateTime.Now; player = next; }
            // Punkte einsammeln
            int ks = 32, off = (cellSize - ks) / 2;
            points?.RemoveAll(p => new Rectangle(p.X + off, p.Y + off, ks, ks).IntersectsWith(player));
            // Geister bewegen und Kollision prüfen
            if (ghosts != null)
            {
                foreach (var g in ghosts) g.MoveTowards(player, walls ?? new List<Rectangle>(), kimMode);
                if (!cheatActive) foreach (var g in ghosts) if (g.Rect.IntersectsWith(player)) { ShowGameOverScreen(); return; }
            }
            // Tür öffnen bei leer gesammelten Punkten
            if (!doorOpen && points?.Count == 0) doorOpen = true;
            // Ziel erreicht
            if (doorOpen && exitDoor.IntersectsWith(player)) { cheatActive = false; playerSpeed = 8; ShowWinScreen(currentLevel); return; }
            Invalidate();
        }


        // Zeigt Buttons für Poster-Auswahl

        private void ShowPosterButtons()
        {
            if (btnFührer != null || btnEgal != null) return;
            int w = 220, h = 45;
            btnFührer = new Button { Text = "Dem Führer stellen", Width = w, Height = h, Font = new Font("Arial", 14, FontStyle.Bold), BackColor = Color.DarkRed, ForeColor = Color.White, Left = (ClientSize.Width - w) / 2, Top = ClientSize.Height / 2 + 30 };
            btnEgal = new Button { Text = "Ist mir egal!", Width = w, Height = h, Font = new Font("Arial", 14, FontStyle.Bold), BackColor = Color.DarkGreen, ForeColor = Color.White, Left = btnFührer.Left, Top = btnFührer.Top + h + 16 };
            btnFührer.Click += (s, e) => { RemovePosterButtons(); showPoster = false; ShowGameOverScreen(); };
            btnEgal.Click += (s, e) => { RemovePosterButtons(); showPoster = false; Invalidate(); };
            Controls.Add(btnFührer); Controls.Add(btnEgal);
            btnFührer.BringToFront(); btnEgal.BringToFront();
        }


        // Entfernt Poster-Buttons von der Form

        private void RemovePosterButtons()
        {
            if (btnFührer != null) { Controls.Remove(btnFührer); btnFührer.Dispose(); btnFührer = null; }
            if (btnEgal != null) { Controls.Remove(btnEgal); btnEgal.Dispose(); btnEgal = null; }
        }


        // Zeichnet alle Spielobjekte auf die Form

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
                    using var f = new Font("Arial", 36, FontStyle.Bold);
                    var sz = g.MeasureString(msg, f);
                    g.DrawString(msg, f, Brushes.Gold, (ClientSize.Width - sz.Width) / 2, (ClientSize.Height - sz.Height) / 2 - 60);
                    return;
                }
                // Zeichne Wände
                walls?.ForEach(w => g.FillRectangle(Brushes.DarkRed, w));
                // Zeichne Punkte
                int ks2 = 32, off2 = (cellSize - ks2) / 2;
                points?.ForEach(p => {
                    if (keyImage != null) g.DrawImage(keyImage, new Rectangle(p.X + off2, p.Y + off2, ks2, ks2));
                    else { g.FillEllipse(Brushes.Gold, new Rectangle(p.X + off2, p.Y + off2, ks2, ks2)); g.DrawEllipse(new Pen(Color.Gray, 2), new Rectangle(p.X + off2, p.Y + off2, ks2, ks2)); }
                });
                // Zeichne Tür
                if (doorOpen)
                {
                    g.FillRectangle(Brushes.SaddleBrown, exitDoor);
                    g.DrawRectangle(new Pen(Color.Yellow, 4), exitDoor);
                    int r = 6; g.FillEllipse(Brushes.Gold, exitDoor.X + exitDoor.Width - 14, exitDoor.Y + exitDoor.Height / 2 - r / 2, r, r);
                }
                // Spieler zeichnen
                if (kimMode) DrawPlayerAsKimHead(g, player); else DrawPlayerAsHumanHead(g, player);
                // Geister
                ghosts?.ForEach(gst => DrawGhostAsFlag(g, gst.Rect));
                // Cheat-Status
                if (cheatActive)
                {
                    string cm = "SUPER-SPEED + UNVERWUNDBAR!";
                    using var f2 = new Font("Arial", 18, FontStyle.Bold);
                    var sz2 = g.MeasureString(cm, f2);
                    g.DrawString(cm, f2, Brushes.Yellow, (ClientSize.Width - sz2.Width) / 2, 8);
                }
            }
            else if (gameOver)
            {
                g.Clear(Color.Black);
                if (gameOverImage != null) g.DrawImage(gameOverImage, 0, 0, ClientSize.Width, ClientSize.Height);
            }
            else if (gameWon)
            {
                g.Clear(Color.Black);
                if (allLevelsCompleted)
                {
                    if (endCongratsImage != null) g.DrawImage(endCongratsImage, 0, 0, ClientSize.Width, ClientSize.Height);
                    int bw = 240, bh = 60;
                    int bx = (ClientSize.Width - bw) / 2, by = ClientSize.Height - 110;
                    if (!Controls.OfType<Button>().Any())
                    {
                        var btnEnd = new Button { Size = new Size(bw, bh), Location = new Point(bx, by), FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, ForeColor = Color.Transparent, TabStop = false };
                        btnEnd.FlatAppearance.BorderSize = 0; btnEnd.FlatAppearance.MouseOverBackColor = Color.Transparent; btnEnd.FlatAppearance.MouseDownBackColor = Color.Transparent;
                        btnEnd.Click += (s, e) => Application.Exit(); Controls.Add(btnEnd);
                    }
                }
                else if (winImage != null)
                {
                    g.DrawImage(winImage, 0, 0, ClientSize.Width, ClientSize.Height);
                }
            }
            else
            {
                g.Clear(Color.Black);
                if (startMenuImage != null) g.DrawImage(startMenuImage, 0, 0, ClientSize.Width, ClientSize.Height);
            }
        }


        // Zeichnet den Geist als rote Flagge mit Stern

        private void DrawGhostAsFlag(Graphics g, Rectangle rect)
        {
            g.FillEllipse(Brushes.Red, rect);
            var star = StarPoints(rect.X + rect.Width / 2, rect.Y + rect.Height / 2, rect.Width / 2.5f, rect.Width / 5.5f, 5, -90);
            g.FillPolygon(Brushes.Gold, star);
        }


        // Zeichnet den Spieler als menschlichen Kopf

        private void DrawPlayerAsHumanHead(Graphics g, Rectangle rect)
        {
            using var skin = new SolidBrush(Color.Peru);
            g.FillEllipse(skin, rect);
            g.FillEllipse(Brushes.SaddleBrown, new Rectangle(rect.X, rect.Y, rect.Width, rect.Height / 2));
            int cx = rect.X + rect.Width / 2, cy = rect.Y + rect.Height / 2;
            int r = Math.Max(2, rect.Width / 10);
            g.FillEllipse(Brushes.Black, cx - r - 3, cy - 3, r, r); g.FillEllipse(Brushes.Black, cx + 3, cy - 3, r, r);
            g.DrawArc(new Pen(Color.Black, 2), cx - 8, cy + 4, 16, 8, 20, 140);
        }


        // Zeichnet den Spieler im "Kim Mode" als goldenen Kopf

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


        // Erzeugt Punkte für den Stern auf der Flagge

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


        // Verarbeitet Tasteneingaben: Bewegung, Cheat und EasterEgg

        private void HandleKeyDown(object? sender, KeyEventArgs e)
        {
            if (!inGame)
            {
                // EasterEgg-Modus: "KIM" und "RESET"
                if (e.KeyCode >= Keys.A && e.KeyCode <= Keys.Z)
                {
                    eggBuffer += e.KeyCode.ToString();
                    if (eggBuffer.EndsWith("KIM", StringComparison.OrdinalIgnoreCase)) { kimMode = true; Invalidate(); }
                    if (eggBuffer.EndsWith("RESET", StringComparison.OrdinalIgnoreCase)) { kimMode = false; Invalidate(); }
                    if (eggBuffer.Length > 6) eggBuffer = eggBuffer[^6..];
                }
            }
            else
            {
                // Bewegungstasten
                switch (e.KeyCode)
                {
                    case Keys.Left: wantedDx = -1; wantedDy = 0; break;
                    case Keys.Right: wantedDx = 1; wantedDy = 0; break;
                    case Keys.Up: wantedDx = 0; wantedDy = -1; break;
                    case Keys.Down: wantedDx = 0; wantedDy = 1; break;
                }
                // Cheat aktivieren (CTRL+ALT+L)
                if (e.Control && e.Alt && e.KeyCode == Keys.L && !cheatActive)
                {
                    cheatActive = true;
                    cheatStart = DateTime.Now;
                    playerSpeed = 11;
                    Invalidate();
                }
            }
        }


        /// Repräsentiert einen Geist, der den Spieler jagt

        class Ghost
        {
            public Rectangle Rect;
            private int speed;
            public Ghost(int x, int y, int size, int speed = 2)
            {
                Rect = new Rectangle(x, y, size, size);
                this.speed = speed;
            }

            // Bewegt den Geist in Richtung Spieler, unter Berücksichtigung von Wänden und Kim Mode
            public void MoveTowards(Rectangle player, List<Rectangle> walls, bool kimMode)
            {
                int eff = speed * (kimMode ? 2 : 1);
                var dirs = new[] { new Point(-1, 0), new Point(1, 0), new Point(0, -1), new Point(0, 1) };
                double best = double.MaxValue; Point bestDir = new Point();
                foreach (var d in dirs)
                {
                    var next = Rect;
                    next.X += d.X * eff; next.Y += d.Y * eff;
                    if (!walls.Any(w => w.IntersectsWith(next)))
                    {
                        double dist = Math.Sqrt(Math.Pow(player.X - next.X, 2) + Math.Pow(player.Y - next.Y, 2));
                        if (dist < best) { best = dist; bestDir = d; }
                    }
                }
                Rect = new Rectangle(Rect.X + bestDir.X * eff, Rect.Y + bestDir.Y * eff, Rect.Width, Rect.Height);
            }
        }
    }
}