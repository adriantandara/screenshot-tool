using System.Runtime.InteropServices;

namespace ScreenCaptureApp
{
    class Program
    {
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vlc);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        const int HOTKEY_ID = 1;
        const uint MOD_NONE = 0x0000;
        const uint VK_F11 = 0x7A;

        public Program()
        {
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Exit", null, Exit!);

            trayIcon = new NotifyIcon();
            trayIcon.Icon = SystemIcons.Application;
            trayIcon.ContextMenuStrip = trayMenu;
            trayIcon.Visible = true;
            
            ScreenCaptureSelectionForm.ShowToastNotification("Screen Capture App", "Welcome to Screen Capture App! Press F11 to capture a screenshot.");

            RegisterHotKey(IntPtr.Zero, HOTKEY_ID, MOD_NONE, VK_F11);
            Application.AddMessageFilter(new KeyMessageFilter(this));
        }

        public void Run()
        {
            Application.Run();
        }

        public void OnHotKeyPressed()
        {
            StartScreenCapture();
        }

        private void StartScreenCapture()
        {
            using (var selectionForm = new ScreenCaptureSelectionForm())
            {
                selectionForm.ShowDialog();
            }
        }

        private void Exit(object sender, EventArgs e)
        {
            trayIcon.Visible = false;
            UnregisterHotKey(IntPtr.Zero, HOTKEY_ID);
            Application.Exit();
        }

        private class ScreenCaptureSelectionForm : Form
        {
            private Point _start;
            private Point _end;
            private bool _isSelecting = false;
            private BufferedGraphics _bufferedGraphics;

            public ScreenCaptureSelectionForm()
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.BackColor = Color.Black;
                this.Opacity = 0.5;
                this.WindowState = FormWindowState.Maximized;
                this.TopMost = true;

                this.MouseDown += OnMouseDown!;
                this.MouseMove += OnMouseMove!;
                this.MouseUp += OnMouseUp!;

                var bufferedContext = BufferedGraphicsManager.Current;
                bufferedContext.MaximumBuffer = new Size(this.Width + 1, this.Height + 1);
                _bufferedGraphics = bufferedContext.Allocate(this.CreateGraphics(), this.DisplayRectangle);
            }

            private void OnMouseDown(object sender, MouseEventArgs e)
            {
                _start = e.Location;
                _isSelecting = true;
            }

            private void OnMouseMove(object sender, MouseEventArgs e)
            {
                if (_isSelecting)
                {
                    _end = e.Location;
                    DrawSelection();
                }
            }

            private void OnMouseUp(object sender, MouseEventArgs e)
            {
                _end = e.Location;
                _isSelecting = false;

                var rect = GetRectangle(_start, _end);
                if (rect.Width > 0 && rect.Height > 0)
                {
                    CaptureScreen(rect);
                }

                this.Close();
            }

            private void DrawSelection()
            {
                _bufferedGraphics.Graphics.Clear(Color.Black);
                var rect = GetRectangle(_start, _end);

                using (var greenPen = new Pen(Color.LimeGreen, 2))
                {
                    _bufferedGraphics.Graphics.DrawRectangle(greenPen, rect);
                    _bufferedGraphics.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(50, Color.LimeGreen)), rect);
                }

                _bufferedGraphics.Render();
            }

            private Rectangle GetRectangle(Point p1, Point p2)
            {
                return new Rectangle(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y), Math.Abs(p2.X - p1.X), Math.Abs(p2.Y - p1.Y));
            }

            private void CaptureScreen(Rectangle rect)
            {
                this.Hide();
                Thread.Sleep(100);

                var screenshot = new Bitmap(rect.Width, rect.Height);
                using (var graphics = Graphics.FromImage(screenshot))
                {
                    graphics.CopyFromScreen(rect.Location, Point.Empty, rect.Size);
                }

                Clipboard.SetImage(screenshot);

                ShowToastNotification("Screenshot Saved", "Your screenshot has been saved to your clipboard.");

                this.Show();
            }

            public static void ShowToastNotification(string title, string text)
            {
                NotifyIcon notifyIcon = new NotifyIcon()
                {
                    Icon = SystemIcons.WinLogo,
                    Visible = true,
                    BalloonTipTitle = title,
                    BalloonTipText = text,
                    BalloonTipIcon = ToolTipIcon.Info
                };

                notifyIcon.ShowBalloonTip(3000);

                System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer { Interval = 4000 };
                timer.Tick += (s, e) =>
                {
                    notifyIcon.Dispose();
                    timer.Stop();
                };
                timer.Start();
            }
        }

        public class KeyMessageFilter : IMessageFilter
        {
            private readonly Program _program;

            public KeyMessageFilter(Program program)
            {
                _program = program;
            }

            public bool PreFilterMessage(ref Message m)
            {
                const int WM_HOTKEY = 0x0312;
                if (m.Msg == WM_HOTKEY)
                {
                    _program.OnHotKeyPressed();
                }
                return false;
            }
        }

        [STAThread]
        static void Main()
        {
            var app = new Program();
            app.Run();
        }
    }
}