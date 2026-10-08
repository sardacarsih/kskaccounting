namespace Accounting.Updater
{
    /// <summary>
    /// Small always-on-top window shown while the update installs and the database migrates,
    /// so users don't relaunch Accounting in the middle of it.
    /// </summary>
    internal sealed class StatusWindow : Form
    {
        private readonly Label _status = new()
        {
            AutoSize = false,
            Dock = DockStyle.Top,
            Height = 40,
            TextAlign = ContentAlignment.MiddleLeft
        };

        private StatusWindow(string version)
        {
            Text = string.IsNullOrWhiteSpace(version) ? "Update Accounting" : $"Update Accounting {version}";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            ControlBox = false;
            ShowInTaskbar = true;
            TopMost = true;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(420, 90);
            Padding = new Padding(16, 10, 16, 10);

            var progress = new ProgressBar
            {
                Style = ProgressBarStyle.Marquee,
                Dock = DockStyle.Top,
                Height = 18
            };

            Controls.Add(progress);
            Controls.Add(_status);
        }

        public void SetStatus(string text)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => _status.Text = text);
                return;
            }

            _status.Text = text;
        }

        /// <summary>Runs <paramref name="work"/> on a worker thread while the window is shown.</summary>
        public static void Run(string version, Action<Action<string>> work)
        {
            Application.EnableVisualStyles();
            using var window = new StatusWindow(version);
            Exception? failure = null;

            window.Shown += (_, _) =>
            {
                Task.Run(() =>
                {
                    try
                    {
                        work(window.SetStatus);
                    }
                    catch (Exception ex)
                    {
                        failure = ex;
                    }
                }).ContinueWith(_ => window.BeginInvoke(window.Close));
            };

            Application.Run(window);

            if (failure is not null)
            {
                throw new InvalidOperationException(failure.Message, failure);
            }
        }
    }
}
