using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using WMPLib;  // Requires adding a COM reference to Windows Media Player

namespace SpinningCatApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Capture the entire virtual screen (all monitors) before showing the form.
            Bitmap desktopImage = CaptureDesktop();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(desktopImage));
        }

        private static Bitmap CaptureDesktop()
        {
            // Capture the full virtual screen.
            Rectangle bounds = SystemInformation.VirtualScreen;
            Bitmap screenshot = new Bitmap(bounds.Width, bounds.Height);
            using (Graphics g = Graphics.FromImage(screenshot))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
            }
            return screenshot;
        }
    }

    public class Form1 : Form
    {
        private Timer spawnTimer;
        private Timer flashTimer;
        private Timer rotationTimer;
        private Panel overlayPanel;
        private Random random;
        private WindowsMediaPlayer player;
        private Bitmap _backgroundImage;
        private float rotationAngle = 0;

        public Form1(Bitmap backgroundImage)
        {
            _backgroundImage = backgroundImage;
            InitializeComponent();

            // Set full-screen form with no border.
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;

            // Instead of setting the BackgroundImage property, we override OnPaintBackground to draw our rotating image.
            random = new Random();

            // Initialize a transparent overlay panel for the neon green flash.
            overlayPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(0, 0, 255, 0)
            };
            this.Controls.Add(overlayPanel);
            overlayPanel.BringToFront();

            // Timer to spawn cat GIFs every 500ms.
            spawnTimer = new Timer { Interval = 500 };
            spawnTimer.Tick += SpawnTimer_Tick;
            spawnTimer.Start();

            // Timer to rotate the background.
            rotationTimer = new Timer { Interval = 30 }; // about 33 FPS
            rotationTimer.Tick += (s, e) =>
            {
                rotationAngle += 1;
                if (rotationAngle >= 360)
                    rotationAngle = 0;
                this.Invalidate();
            };
            rotationTimer.Start();

            // Start playing the looping cat MP3 using Windows Media Player.
            player = new WindowsMediaPlayer();
            player.URL = "https://github.com/orlyjamie/spinningcat/raw/refs/heads/main/cat.mp3";
            player.settings.setMode("loop", true);
            player.controls.play();
        }

        private void SpawnTimer_Tick(object sender, EventArgs e)
        {
            SpawnCatGif();
        }

        private void SpawnCatGif()
        {
            // Create a PictureBox to host the cat GIF.
            PictureBox catPic = new PictureBox
            {
                Size = new Size(150, 150),
                SizeMode = PictureBoxSizeMode.StretchImage,
                ImageLocation = "https://raw.githubusercontent.com/orlyjamie/spinningcat/refs/heads/main/cat.gif"
            };

            // Position the cat GIF at a random location within the client area.
            int x = random.Next(0, Math.Max(1, this.ClientSize.Width - catPic.Width));
            int y = random.Next(0, Math.Max(1, this.ClientSize.Height - catPic.Height));
            catPic.Location = new Point(x, y);

            this.Controls.Add(catPic);
            catPic.BringToFront();

            // Remove the PictureBox after 15 seconds to prevent overflow.
            Timer removeTimer = new Timer { Interval = 15000 };
            removeTimer.Tick += (s, ev) =>
            {
                removeTimer.Stop();
                this.Controls.Remove(catPic);
                catPic.Dispose();
                removeTimer.Dispose();
            };
            removeTimer.Start();
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (_backgroundImage != null)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                // Move the origin to the center of the client area.
                g.TranslateTransform(this.ClientSize.Width / 2, this.ClientSize.Height / 2);
                g.RotateTransform(rotationAngle);
                float scale = Math.Max((float)this.ClientSize.Width / _backgroundImage.Width,(float)this.ClientSize.Height / _backgroundImage.Height);
                g.ScaleTransform(scale, scale);
                // Draw the image centered.
                g.DrawImage(_backgroundImage, -_backgroundImage.Width / 2, -_backgroundImage.Height / 2);
                // Reset transform.
                g.ResetTransform();
            }
            else
            {
                base.OnPaintBackground(e);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // Form1
            // 
            this.ClientSize = new Size(800, 600);
            this.Name = "Form1";
            this.Text = "Spinning Cat";
            this.ResumeLayout(false);
        }
    }
}
