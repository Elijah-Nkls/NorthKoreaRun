using System;
using System.Drawing;
using System.Windows.Forms;

namespace Northkorea_Run
{
    public partial class Form1 : Form
    {
        private System.Windows.Forms.Timer gameTimer;
        private Rectangle player;
        private int playerSpeed = 8;
        private int playerDx, playerDy;

        public Form1()
        {
            // Set up form properties
            this.Size = new Size(600, 600);
            this.DoubleBuffered = true;
            this.Paint += new PaintEventHandler(DrawGameWindow);
            this.KeyDown += new KeyEventHandler(HandleKeyDown);
            this.PreviewKeyDown += (s, e) => { e.IsInputKey = true; };

            // Initialize game timer
            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 50;
            gameTimer.Tick += new EventHandler(GameTimer_Tick);
            gameTimer.Start();

            // Initialize player
            player = new Rectangle(50, 50, 30, 30);
            playerDx = 0;
            playerDy = 0;
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
            player.X += playerDx * playerSpeed;
            player.Y += playerDy * playerSpeed;
            this.Invalidate();
        }

        private void DrawGameWindow(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            // Clear background
            g.Clear(Color.Black);
            // Draw player as a gold circle
            g.FillEllipse(Brushes.Gold, player);
        }
    }
}
