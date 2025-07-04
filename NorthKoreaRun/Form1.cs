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
        private int cellSize = 40, mazeSize = 15;
        private Rectangle player;
        private int playerSpeed = 8;
        private int playerDx, playerDy;

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
            // Initialize game timer
            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 50;
            gameTimer.Tick += new EventHandler(GameTimer_Tick);

            // Initialize player
            player = new Rectangle(50, 50, 30, 30);
            playerDx = 0;
            playerDy = 0;

            // Start the game timer
            gameTimer.Start();
        }

        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    playerDx = -1;
                    playerDy = 0;
                    break;
                case Keys.Right:
                    playerDx = 1;
                    playerDy = 0;
                    break;
                case Keys.Up:
                    playerDx = 0;
                    playerDy = -1;
                    break;
                case Keys.Down:
                    playerDx = 0;
                    playerDy = 1;
                    break;
            }
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            // Move player based on current direction
            // Calculate next position
            Rectangle nextPosition = player;
            nextPosition.X += playerDx * playerSpeed;
            nextPosition.Y += playerDy * playerSpeed;
            // Move if no wall collision
            if (!walls.Any(w => w.IntersectsWith(nextPosition)))
            {
                player = nextPosition;
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

            // Draw player as a gold circle
            g.FillEllipse(Brushes.Gold, player);
        }
    }
}