using AutoGameSystem.Core;
using AutoGameSystem.Models;
using AutoGameSystem.Utilities;
using Emgu.CV.Ocl;
using System;
using System.Windows.Forms;
namespace AutoGameSystem
{
    public partial class MainForm : Form
    {
        private AutomationEngine _engine;
        private AppConfig _config;
        public MainForm()
        {
            //InitializeComponent();
            LoadConfig();
            InitializeEngine();
            InitializeComponentManual();
        }
        private void LoadConfig()
        {
            _config = ConfigManager.LoadAppConfig();
        }
        private void InitializeEngine()
        {
            _engine = new AutomationEngine(_config);
            _engine.OnLogMessage += message =>
                Invoke(new Action(() => LogMessage(message)));
            _engine.OnTaskCompleted += (message, success) =>
                Invoke(new Action(() => LogMessage($"{message} - {(success ? "SUCCESS" : "FAILED")}")));
        }
        private void LogMessage(string message)
        {
            if (logTextBox.TextLength > 10000)
                logTextBox.Clear();

            logTextBox.AppendText($"{DateTime.Now:HH:mm:ss} - {message}{Environment.NewLine}");
            logTextBox.ScrollToCaret();
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            _engine.LoadData();
            _engine.Start();
            startButton.Enabled = false;
            stopButton.Enabled = true;
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            _engine.Stop();
            _engine.SaveData();
            startButton.Enabled = true;
            stopButton.Enabled = false;
        }
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _engine?.Stop();
            ConfigManager.SaveAppConfig(_config);
        }

        private void settingsButton_Click(object sender, EventArgs e)
        {
            // Tạo form settings đơn giản
            using (var settingsForm = new Form())
            {
                settingsForm.Text = "Settings";
                settingsForm.Size = new System.Drawing.Size(400, 300);
                settingsForm.StartPosition = FormStartPosition.CenterParent;

                var btnOk = new Button() { Text = "OK", DialogResult = DialogResult.OK };
                btnOk.Location = new System.Drawing.Point(250, 220);
                settingsForm.Controls.Add(btnOk);

                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    ConfigManager.SaveAppConfig(_config);
                }

            }    
                
        }
        private void InitializeComponentManual()
        {
            this.SuspendLayout();
            this.Text = "Auto Game System";
            this.Size = new System.Drawing.Size(800, 600);
            this.FormClosing += new FormClosingEventHandler(this.MainForm_FormClosing);

            this.logTextBox = new TextBox();
            this.logTextBox.Multiline = true;
            this.logTextBox.ScrollBars = ScrollBars.Vertical;
            this.logTextBox.Location = new System.Drawing.Point(12, 12);
            this.logTextBox.Size = new System.Drawing.Size(760, 450);
            this.logTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.Controls.Add(this.logTextBox);

            this.startButton = new Button();
            this.startButton.Text = "Start";
            this.startButton.Location = new System.Drawing.Point(12, 480);
            this.startButton.Size = new System.Drawing.Size(100, 30);
            this.startButton.Click += new EventHandler(this.startButton_Click);
            this.Controls.Add(this.startButton);

            this.stopButton = new Button();
            this.stopButton.Text = "Stop";
            this.stopButton.Location = new System.Drawing.Point(120, 480);
            this.stopButton.Size = new System.Drawing.Size(100, 30);
            this.stopButton.Enabled = false;
            this.stopButton.Click += new EventHandler(this.stopButton_Click);
            this.Controls.Add(this.stopButton);

            // Tạo settingsButton
            this.settingsButton = new Button();
            this.settingsButton.Text = "Settings";
            this.settingsButton.Location = new System.Drawing.Point(228, 480);
            this.settingsButton.Size = new System.Drawing.Size(100, 30);
            this.settingsButton.Click += new EventHandler(this.settingsButton_Click);
            this.Controls.Add(this.settingsButton);

            this.ResumeLayout(false);
            this.PerformLayout();

        }
        private TextBox logTextBox;
        private Button startButton;
        private Button stopButton;
        private Button settingsButton;
        
    }
}
