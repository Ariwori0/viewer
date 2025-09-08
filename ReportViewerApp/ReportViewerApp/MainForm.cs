using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace ReportViewerApp
{
    // メインフォーム
    public partial class MainForm : Form
    {
        private const string REPORTS_BASE_PATH = @"C:\Users\youli\source\repos\ReportViewerApp\Reports";
        private const string USER_CONFIG_FILE = "user_config.json";

        private string currentUserId = "";
        private Panel loginPanel;
        private Panel mainPanel;
        private ComboBox userIdComboBox;
        private TextBox passwordTextBox;
        private ListBox reportListBox;
        private Button loginButton;
        private Button logoutButton;
        private Label statusLabel;
        private WebBrowser pdfViewer;
        private PictureBox imageViewer;

        private Dictionary<string, string> userCredentials;

        public MainForm()
        {
            InitializeComponent();
            LoadUserCredentials();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1000, 700);
            this.Text = "レポート閲覧システム";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;

            // ログインパネルの作成
            CreateLoginPanel();

            // メインパネルの作成
            CreateMainPanel();

            // 初期状態はログイン画面
            ShowLoginPanel();
        }

        private void CreateLoginPanel()
        {
            loginPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.LightBlue
            };

            // ログインフォームをセンタリング
            var loginFormPanel = new Panel
            {
                Size = new Size(400, 300),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            loginFormPanel.Location = new Point(
                (loginPanel.Width - loginFormPanel.Width) / 2,
                (loginPanel.Height - loginFormPanel.Height) / 2
            );

            var titleLabel = new Label
            {
                Text = "レポート閲覧システム",
                Font = new Font("MS Gothic", 16, FontStyle.Bold),
                Location = new Point(80, 30),
                Size = new Size(240, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var userIdLabel = new Label
            {
                Text = "ユーザーID:",
                Location = new Point(50, 80),
                Size = new Size(80, 23)
            };

            userIdComboBox = new ComboBox
            {
                Location = new Point(140, 80),
                Size = new Size(200, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            var passwordLabel = new Label
            {
                Text = "パスワード:",
                Location = new Point(50, 120),
                Size = new Size(80, 23)
            };

            passwordTextBox = new TextBox
            {
                Location = new Point(140, 120),
                Size = new Size(200, 23),
                PasswordChar = '*'
            };

            loginButton = new Button
            {
                Text = "ログイン",
                Location = new Point(160, 180),
                Size = new Size(80, 30),
                BackColor = Color.LightGreen
            };
            loginButton.Click += LoginButton_Click;

            statusLabel = new Label
            {
                Text = "",
                Location = new Point(50, 220),
                Size = new Size(300, 23),
                ForeColor = Color.Red
            };

            loginFormPanel.Controls.AddRange(new Control[] {
                titleLabel, userIdLabel, userIdComboBox, passwordLabel,
                passwordTextBox, loginButton, statusLabel
            });

            // パネルリサイズ時の処理
            loginPanel.Resize += (s, e) => {
                loginFormPanel.Location = new Point(
                    (loginPanel.Width - loginFormPanel.Width) / 2,
                    (loginPanel.Height - loginFormPanel.Height) / 2
                );
            };

            loginPanel.Controls.Add(loginFormPanel);
            this.Controls.Add(loginPanel);
        }

        private void CreateMainPanel()
        {
            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Visible = false
            };

            // ヘッダーパネル
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.LightGray
            };

            var welcomeLabel = new Label
            {
                Text = "ようこそ",
                Location = new Point(20, 20),
                Size = new Size(200, 23),
                Font = new Font("MS Gothic", 12)
            };

            logoutButton = new Button
            {
                Text = "ログアウト",
                Location = new Point(headerPanel.Width - 120, 15),
                Size = new Size(100, 30),
                BackColor = Color.LightCoral,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            logoutButton.Click += LogoutButton_Click;

            headerPanel.Controls.AddRange(new Control[] { welcomeLabel, logoutButton });

            // 左パネル（レポート一覧）
            var leftPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 300,
                BorderStyle = BorderStyle.FixedSingle
            };

            var reportListLabel = new Label
            {
                Text = "レポート一覧",
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.LightYellow,
                Font = new Font("MS Gothic", 10, FontStyle.Bold)
            };

            reportListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("MS Gothic", 10)
            };
            reportListBox.SelectedIndexChanged += ReportListBox_SelectedIndexChanged;

            leftPanel.Controls.AddRange(new Control[] { reportListLabel, reportListBox });

            // 右パネル（PDF表示）
            //var rightPanel = new Panel
            //{
            //    Dock = DockStyle.Fill,
            //    BorderStyle = BorderStyle.FixedSingle
            //};

            //var pdfViewerLabel = new Label
            //{
            //    Text = "レポート表示",
            //    Dock = DockStyle.Top,
            //    Height = 30,
            //    TextAlign = ContentAlignment.MiddleCenter,
            //    BackColor = Color.LightYellow,
            //    Font = new Font("MS Gothic", 10, FontStyle.Bold)
            //};

            //pdfViewer = new WebBrowser
            //{
            //    Dock = DockStyle.Fill
            //};

            //rightPanel.Controls.AddRange(new Control[] { pdfViewerLabel, pdfViewer });
            // 右パネル（画像表示）
            var rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle
            };

            var imageViewerLabel = new Label
            {
                Text = "レポート表示",
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.LightYellow,
                Font = new Font("MS Gothic", 10, FontStyle.Bold)
            };

            imageViewer = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Black
            };

            rightPanel.Controls.AddRange(new Control[] { imageViewerLabel, imageViewer });

            // メインパネルに各パネルを追加
            mainPanel.Controls.AddRange(new Control[] { rightPanel, leftPanel, headerPanel });
            this.Controls.Add(mainPanel);
        }

        private void LoadUserCredentials()
        {
            try
            {
                if (File.Exists(USER_CONFIG_FILE))
                {
                    string json = File.ReadAllText(USER_CONFIG_FILE);
                    userCredentials = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                }
                else
                {
                    // デフォルトユーザーを作成
                    userCredentials = new Dictionary<string, string>
                    {
                        {"ID001", "pass001"},
                        {"ID002", "pass002"},
                        {"ID003", "pass003"},
                        {"TEST", "test123"}
                    };
                    SaveUserCredentials();
                }

                // コンボボックスにユーザーIDを追加
                userIdComboBox.Items.Clear();
                foreach (var userId in userCredentials.Keys)
                {
                    userIdComboBox.Items.Add(userId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ユーザー設定の読み込みに失敗しました: {ex.Message}", "エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                userCredentials = new Dictionary<string, string>();
            }
        }

        private void SaveUserCredentials()
        {
            try
            {
          
                string json = JsonConvert.SerializeObject(userCredentials, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(USER_CONFIG_FILE, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ユーザー設定の保存に失敗しました: {ex.Message}", "エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            string userId = userIdComboBox.Text.Trim();
            string password = passwordTextBox.Text;

            if (string.IsNullOrEmpty(userId))
            {
                statusLabel.Text = "ユーザーIDを選択してください。";
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                statusLabel.Text = "パスワードを入力してください。";
                return;
            }

            // 認証チェック
            if (userCredentials.ContainsKey(userId) && userCredentials[userId] == password)
            {
                currentUserId = userId;
                ShowMainPanel();
                LoadReportList();
            }
            else
            {
                statusLabel.Text = "ユーザーIDまたはパスワードが間違っています。";
                passwordTextBox.Clear();
            }
        }

        private void LogoutButton_Click(object sender, EventArgs e)
        {
            currentUserId = "";
            passwordTextBox.Clear();
            statusLabel.Text = "";
            ShowLoginPanel();
        }

        private void ShowLoginPanel()
        {
            loginPanel.Visible = true;
            mainPanel.Visible = false;
        }

        private void ShowMainPanel()
        {
            loginPanel.Visible = false;
            mainPanel.Visible = true;

            // ウェルカムメッセージを更新
            var welcomeLabel = mainPanel.Controls.OfType<Panel>().First().Controls.OfType<Label>().First();
            welcomeLabel.Text = $"ようこそ {currentUserId} さん";
        }

        private void LoadReportList()
        {
            reportListBox.Items.Clear();
            //pdfViewer.Navigate("about:blank");

            string userReportPath = Path.Combine(REPORTS_BASE_PATH, currentUserId);

            try
            {
                if (Directory.Exists(userReportPath))
                {
                    //string[] pdfFiles = Directory.GetFiles(userReportPath, "*.pdf");
                    string[] imageFiles = Directory.GetFiles(userReportPath, "*.*")
                    .Where(f => f.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                             || f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                             || f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                             || f.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
                    .ToArray();


                    if (imageFiles.Length == 0)
                    {
                        MessageBox.Show($"フォルダは見つかったが 画像 が0件です: {userReportPath}");
                        reportListBox.Items.Add("レポートがありません");
                    }
                    else
                    {
                        foreach (string filePath in imageFiles.OrderByDescending(f => File.GetCreationTime(f)))
                        {
                            MessageBox.Show($"見つかった画像: {filePath}");
                            FileInfo fileInfo = new FileInfo(filePath);
                            string displayName = $"{fileInfo.Name} ({fileInfo.CreationTime:yyyy/MM/dd HH:mm})";
                            reportListBox.Items.Add(new ReportItem { DisplayName = displayName, FilePath = filePath });
                        }
                    }
                }
                else
                {
                    reportListBox.Items.Add("ユーザーフォルダが見つかりません");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"レポートの読み込みに失敗しました: {ex.Message}", "エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                reportListBox.Items.Add("レポートの読み込みエラー");
            }
        }

        private void ReportListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (reportListBox.SelectedItem is ReportItem reportItem)
            {
                try
                {
                    if (File.Exists(reportItem.FilePath))
                    {
                        // 既存の画像があればリソースを解放
                        if (imageViewer.Image != null)
                        {
                            imageViewer.Image.Dispose();
                        }

                        // 画像ファイルを読み込んで表示
                        using (var fileStream = new FileStream(reportItem.FilePath, FileMode.Open, FileAccess.Read))
                        {
                            imageViewer.Image = Image.FromStream(fileStream);
                        }

                    }
                    else
                    {
                        MessageBox.Show("選択されたファイルが見つかりません。", "エラー",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (OutOfMemoryException)
                {
                    MessageBox.Show("画像ファイルが破損しているか、サポートされていない形式です。", "エラー",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    imageViewer.Image = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"画像の表示に失敗しました: {ex.Message}\n\n" +
                        "外部アプリケーションで開きますか？", "エラー",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (MessageBox.Show("外部アプリケーションで開きますか？", "確認",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo(reportItem.FilePath) { UseShellExecute = true });
                        }
                        catch (Exception startEx)
                        {
                            MessageBox.Show($"外部アプリケーションの起動に失敗しました: {startEx.Message}", "エラー",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    imageViewer.Image = null;
                }
            }
        }


        // レポートアイテムクラス
        public class ReportItem
        {
            public string DisplayName { get; set; }
            public string FilePath { get; set; }

            public override string ToString()
            {
                return DisplayName;
            }
        }
    }

}
