using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;

namespace Northkorea_Run
{
    public partial class Form1 : Form
    {
        private System.Windows.Forms.Timer gameTimer;
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

        public Form1()
        {
            // Set up form properties
            this.Size = new Size(cellSize * mazeSize, cellSize * mazeSize);
            this.DoubleBuffered = true;
            this.Paint += new PaintEventHandler(DrawGameWindow);
            this.KeyDown += new KeyEventHandler(HandleKeyDown);
            this.PreviewKeyDown += (s, e) => { e.IsInputKey = true; };

            List<string> maze = new List<string>
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
            };
            walls = new List<Rectangle>();
            for (int y = 0; y < maze.Count; y++)
            {
                for (int x = 0; x < maze[y].Length; x++)
                {
                    if (maze[y][x] == '1')
                    {
                        walls.Add(new Rectangle(x * cellSize, y * cellSize, cellSize, cellSize));
                    }
                }
            }
            // Determine empty cells and spawn point
            List<Point> emptyCells = new List<Point>();
            for (int y = 0; y < maze.Count; y++)
            {
                for (int x = 0; x < maze[y].Length; x++)
                {
                    if (maze[y][x] == '0')
                    {
                        emptyCells.Add(new Point(x, y));
                    }
                }
            }
            Point spawnCell = emptyCells[0];
            player = new Rectangle(spawnCell.X * cellSize + 5, spawnCell.Y * cellSize + 5, cellSize - 10, cellSize - 10);
            playerDx = 0;
            playerDy = 0;
            wantedDx = 0;
            wantedDy = 0;
            // Place collectible points
            Random rnd = new Random();
            var freeForPoints = emptyCells.Where(p => !(p.X == spawnCell.X && p.Y == spawnCell.Y)).ToList();
            var chosenPoints = new HashSet<int>();
            while (chosenPoints.Count < 5 && chosenPoints.Count < freeForPoints.Count)
            {
                chosenPoints.Add(rnd.Next(freeForPoints.Count));
            }
            points = chosenPoints.Select(i => new Point(freeForPoints[i].X * cellSize, freeForPoints[i].Y * cellSize)).ToList();
            exitDoor = new Rectangle(13 * cellSize, 13 * cellSize, cellSize, cellSize);
            doorOpen = false;

            // Initialize ghost(s)
            ghosts = new List<Ghost>();
            var forGhosts = emptyCells.Where(p => !(p.X == spawnCell.X && p.Y == spawnCell.Y) && !points.Any(pt => pt.X / cellSize == p.X && pt.Y / cellSize == p.Y)).ToList();
            int ghostSize = cellSize - 10;
            if (forGhosts.Count > 0)
            {
                Point ghostCell = forGhosts[forGhosts.Count - 1];
                ghosts.Add(new Ghost(ghostCell.X * cellSize + 5, ghostCell.Y * cellSize + 5, ghostSize, 3));
            }

            // Initialize game timer
            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 50;
            gameTimer.Tick += new EventHandler(GameTimer_Tick);
            gameTimer.Start();
        }

        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    wantedDx = -1;
                    wantedDy = 0;
                    break;
                case Keys.Right:
                    wantedDx = 1;
                    wantedDy = 0;
                    break;
                case Keys.Up:
                    wantedDx = 0;
                    wantedDy = -1;
                    break;
                case Keys.Down:
                    wantedDx = 0;
                    wantedDy = 1;
                    break;
            }
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            // Calculate wanted movement
            Rectangle wantedPosition = player;
            wantedPosition.X += wantedDx * playerSpeed;
            wantedPosition.Y += wantedDy * playerSpeed;
            if (!walls.Any(w => w.IntersectsWith(wantedPosition)))
            {
                playerDx = wantedDx;
                playerDy = wantedDy;
            }
            // Calculate next position for actual movement
            Rectangle nextPosition = player;
            nextPosition.X += playerDx * playerSpeed;
            nextPosition.Y += playerDy * playerSpeed;
            if (!walls.Any(w => w.IntersectsWith(nextPosition)))
            {
                player = nextPosition;
            }
            // Remove collected points
            int keySize = 32;
            int offset = (cellSize - keySize) / 2;
            points.RemoveAll(p => new Rectangle(p.X + offset, p.Y + offset, keySize, keySize).IntersectsWith(player));
            if (!doorOpen && points.Count == 0)
            {
                doorOpen = true;
            }
            if (doorOpen && exitDoor.IntersectsWith(player))
            {
                gameTimer.Stop();
                MessageBox.Show("Level completed!");
            }
            if (ghosts != null)
            {
                foreach (var ghost in ghosts)
                {
                    ghost.MoveTowards(player, walls);
                }
            }
            this.Invalidate();
        }

        private void DrawGameWindow(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            // Clear background
            g.Clear(Color.Black);
            // Draw maze walls
            foreach (Rectangle wall in walls)
            {
                g.FillRectangle(Brushes.DarkRed, wall);
            }

            // Draw collectible points
            int keySize = 32;
            int offset = (cellSize - keySize) / 2;
            foreach (var point in points)
            {
                g.FillEllipse(Brushes.Gold, new Rectangle(point.X + offset, point.Y + offset, keySize, keySize));
                g.DrawEllipse(new Pen(Color.Gray, 2), new Rectangle(point.X + offset, point.Y + offset, keySize, keySize));
            }

            // Draw exit door if open
            if (doorOpen)
            {
                g.FillRectangle(Brushes.SaddleBrown, exitDoor);
                g.DrawRectangle(new Pen(Color.Yellow, 4), exitDoor);
                int knobR = 6;
                g.FillEllipse(Brushes.Gold, exitDoor.X + exitDoor.Width - 14, exitDoor.Y + exitDoor.Height / 2 - knobR / 2, knobR, knobR);
            }

            // Draw player as a gold circle
            g.FillEllipse(Brushes.Gold, player);
            // Draw ghost(s)
            if (ghosts != null)
            {
                foreach (var ghost in ghosts)
                {
                    g.FillEllipse(Brushes.Red, ghost.Rect);
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
            public void MoveTowards(Rectangle player, List<Rectangle> walls)
            {
                int effectiveSpeed = speed;
                List<Point> directions = new List<Point> { new Point(-1, 0), new Point(1, 0), new Point(0, -1), new Point(0, 1) };
                double bestDist = double.MaxValue;
                Point bestDir = new Point(0, 0);
                foreach (Point dir in directions)
                {
                    Rectangle next = Rect;
                    next.X += dir.X * effectiveSpeed;
                    next.Y += dir.Y * effectiveSpeed;
                    if (!walls.Any(w => w.IntersectsWith(next)))
                    {
                        double dist = Math.Sqrt(Math.Pow(player.X - next.X, 2) + Math.Pow(player.Y - next.Y, 2));
                        if (dist < bestDist)
                        {
                            bestDist = dist;
                            bestDir = dir;
                        }
                    }
                }
                Rect = new Rectangle(Rect.X + bestDir.X * effectiveSpeed, Rect.Y + bestDir.Y * effectiveSpeed, Rect.Width, Rect.Height);
            }
        }
    }
}