using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HIddenScreenshot
{
    public partial class HiddenHotKeyListenerForm : Form
    {
        private readonly Config _config;

        private enum Hotkeys
        {
            Quit = 1,
            MakeScreenshot = 2
        }

        private static class KeyModifier
        {
            public const int None = 0;
            public const int Alt = 1;
            public const int Control = 2;
            public const int Shift = 4;
            public const int WinKey = 8;
        }

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);



        public HiddenHotKeyListenerForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            Load += Form_Load;
            InitializeComponent();

            _config = GetConfig();
            if (_config == null)
            {
                MessageBox.Show("Unable to load config. Exiting...");
                Environment.Exit(1);
            }

            if (!RegisterHotkeys())
            {
                MessageBox.Show("Sorry unable to start. Exiting...");
                Environment.Exit(1);
            }
        }

        private Config GetConfig()
        {
            try
            {
                var config = File.ReadAllText("appconfig.json");
                return JsonSerializer.Deserialize<Config>(config);
            }
            catch
            {
                return null;
            }
        }

        private bool RegisterHotkeys()
        {
            var quitHotKeyCode = (int)Keys.F12;
            var quitModifiers = KeyModifier.Control | KeyModifier.Shift;
            var quitHotKeyRegistered = RegisterHotKey(
                Handle, (int)Hotkeys.Quit, quitModifiers, quitHotKeyCode
            );

            if (!quitHotKeyRegistered)
            {
                return false;
            }

            var makeScreenshotHotKeyCode = (int)Keys.F11;
            var makeScreenshotModifiers = KeyModifier.Shift;
            var makeScreenshotHotKeyRegistered = RegisterHotKey(
                Handle, (int)Hotkeys.MakeScreenshot, makeScreenshotModifiers, makeScreenshotHotKeyCode
            );

            if (!makeScreenshotHotKeyRegistered)
            {
                return false;
            }

            return true;
        }

        private void Form_Load(object sender, EventArgs e)
        {
            Size = new Size(0, 0);
        }

        protected override void WndProc(ref Message m)
        {
            const int wmHotkeyMsgId = 0x0312;

            if (m.Msg == wmHotkeyMsgId)
            {
                var id = m.WParam.ToInt32();

                if (id == (int)Hotkeys.Quit)
                {
                    Environment.Exit(1);
                }

                if (id == (int)Hotkeys.MakeScreenshot)
                {
                    MakeAndSendScreenshot();
                }
            }

            base.WndProc(ref m);
        }

        private void MakeAndSendScreenshot()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png");

            var bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size);
                bmp.Save(path);
            }

            // Send
            Task.Run(async () =>
            {
                var client = new SmtpClient();
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                await client.ConnectAsync(_config.SmtpHost, _config.SmtpPort);
                await client.AuthenticateAsync(_config.UserName, _config.Password);

                var message = new MimeMessage();
                message.From.Add(MailboxAddress.Parse(_config.UserName));
                message.To.Add(MailboxAddress.Parse(_config.ToEmail));
                message.Subject = "New hidden screenshot";

                var builder = new BodyBuilder { TextBody = @"Here is new hidden screenshot" };
                builder.Attachments.Add(path);

                message.Body = builder.ToMessageBody();

                await client.SendAsync(message);
            });
        }
    }
}
