﻿using BiliDuang.UI;
using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BiliDuang
{
    public partial class MainForm : MaterialForm
    {
        private bool resultSeeing = false;
        private Video v = null;

        public MainForm()
        {
            InitializeComponent();
            MaterialSkinManager materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            Other.RefreshColorSceme();
            Initialize();
        }

        private void Initialize()
        {
            Directory.CreateDirectory(Environment.CurrentDirectory + "/config");
            Directory.CreateDirectory(Environment.CurrentDirectory + "/temp");
            RefreshUserData();
            Settings.ReadSettings();
            materialSingleLineTextField2.Text = Settings.maxMission.ToString();
            LowCache.Checked = Settings.lowcache;
            materialLabel2.BackColor = Other.GetBackGroundColor();
            Tabs.Size = new Size(Tabs.Width, Tabs.Height + 30);
            materialCheckBox1.Checked = Settings.usearia2c;
            materialCheckBox2.Checked = Settings.downloaddanmaku;
            materialCheckBox4.Checked = Settings.downloadcc;
            aria2cargu.Visible = materialCheckBox1.Checked;
            aria2cargu.Text = Settings.aria2cargument;
            materialFlatButton7.Visible = materialCheckBox1.Checked;
            if (string.IsNullOrEmpty(Settings.apilink))
            {
                Settings.apilink = "https://api.bilibili.com/x/player/playurl?fnval=0";
            }
            APILink.Text = Settings.apilink;
            AreaSelector.SelectedIndex = Settings.area switch
            {
                "cn" => 0,
                "hk" => 1,
                "tw" => 2,
                "th" => 3,
                _ => 0
            };
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                Tabs.Region = new Region(new RectangleF(Tabs.Left, Tabs.Top, Tabs.Width, Tabs.Height));
                TabSelector.Location = new System.Drawing.Point(0, 64);
                materialLabel2.Location = new Point(411, 80);
                materialLabel2.Size = new Size(691, 25);
                videoList1.Size = new Size(1200, 650);
                videoList1.Location = new Point(-5, 70);
                panel3.Location = new Point(0, 5);
            }

            checkUpdate();
        }

        private async void checkUpdate()
        {
            await Task.Run(() =>
            {
                try
                {
                    WebClient web = new WebClient();
                    web.Credentials = CredentialCache.DefaultCredentials;
                    System.Net.ServicePointManager.SecurityProtocol |=
                        SecurityProtocolType.Tls12; //适配某些老旧的HTTPS
                    string bak = Encoding.UTF8.GetString(
                        web.DownloadData("https://gitee.com/api/v5/repos/kengwang/BiliDuang/releases/latest"));
                    JSONCallback.Update.Root upjson =
                        Newtonsoft.Json.JsonConvert.DeserializeObject<JSONCallback.Update.Root>(bak);
                    if (upjson.tag_name != Settings.versionCode)
                    {
                        MessageBox.Show(
                            "版本号:" + upjson.tag_name + "\r\n当前版本:" + Settings.versionCode + "\r\n更新日志:\r\n" +
                            upjson.body + "\r\n\r\n点击确认后跳转到下载页面", "发现新版本!");
                        System.Diagnostics.Process.Start("explorer.exe",
                            "https://gitee.com/kengwang/BiliDuang/releases");
                    }
                }
                catch (Exception)
                {
                }
            });
        }

        private async void ResultShowReady()
        {
            await Task.Run(() =>
            {
                //159 - > 0
                //26,164,917
                materialLabel1.Text = "勇者大人请传令↓";
                materialFlatButton1.Text = "Logout";
                resultSeeing = true;
                if (Environment.OSVersion.Platform != PlatformID.Unix)
                {
                    while (panel1.Location.Y != 125)
                    {
                        panel1.Location = new Point(panel1.Location.X, panel1.Location.Y - 1);
                    }
                }
                else
                {
                    while (panel1.Location.Y != 140)
                    {
                        panel1.Location = new Point(panel1.Location.X, panel1.Location.Y - 1);
                    }
                }
            });
        }

        private void CloseCase()
        {
            materialLabel1.Text = "勇者大人请传令->";
            materialFlatButton1.Text = "Link Start!";
            resultSeeing = false;
            while (panel1.Location.Y != 318)
            {
                panel1.Location = new Point(panel1.Location.X, panel1.Location.Y + 1);
            }
        }

        private void materialFlatButton1_Click(object sender, EventArgs e)
        {
            if (!resultSeeing)
            {
                SearchStart();
            }
            else
            {
                Tabs.SelectTab(0);
                SeasonSelectBox.Visible = false;
                CloseCase();
                videoList1.DisableAllCards();
                materialLabel2.Text = "";
            }
        }

        public void RefreshUserData()
        {
            LoginButton.Text = "正在加载用户信息...";
            Task.Run(() =>
            {
                User.RefreshUserInfo();
                if (User.islogin)
                {
                    LoginButton.Icon = Image.FromFile(User.face);
                    LoginButton.Text = User.name;
                }
                else
                {
                    LoginButton.Icon = null;
                    LoginButton.Text = "登录bilibili,开启新世界";
                }

                LoginButton.BackColor = Other.GetBackGroundColor();
            });
        }


        private void SelectAll_Click(object sender, EventArgs e)
        {
            foreach (Control c in videoList1.panel2.Controls)
            {
                if (c is UI.AVCard)
                {
                    UI.AVCard card = (UI.AVCard) c;
                    card.check = true;
                }
            }
        }

        private void DownloadSelected_Click(object sender, EventArgs e)
        {
            foreach (Control c in videoList1.panel2.Controls)
            {
                if (c is UI.AVCard)
                {
                    UI.AVCard card = (UI.AVCard) c;
                    if (string.IsNullOrEmpty(card.DPath))
                    {
                        MessageBox.Show("请选择下载路径!");
                        return;
                    }

                    if (card.check)
                    {
                        card.StartDownload();
                    }
                }
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Settings.SaveSettings();
            Environment.Exit(0);
        }

        private void QualityBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (Control c in videoList1.panel2.Controls)
            {
                if (c is UI.AVCard)
                {
                    UI.AVCard card = (UI.AVCard) c;
                    card.QualityBox.SelectedIndex = QualityBox.SelectedIndex;
                }
            }
        }

        private void materialFlatButton2_Click(object sender, EventArgs e)
        {
            foreach (Control c in videoList1.panel2.Controls)
            {
                if (c is UI.AVCard)
                {
                    UI.AVCard card = (UI.AVCard) c;

                    card.check = !card.check;
                }
            }
        }

        private void materialFlatButton3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                foreach (Control c in videoList1.panel2.Controls)
                {
                    if (c is UI.AVCard)
                    {
                        UI.AVCard card = (UI.AVCard) c;

                        card.DPath = dialog.SelectedPath;
                    }
                }

                materialSingleLineTextField1.Text = dialog.SelectedPath;
            }
        }

        public void StartAllButton_Click(object sender, EventArgs e)
        {
            DownloadQueue.StartAll(false);
        }

        private void PauseAll_Click(object sender, EventArgs e)
        {
            DownloadQueue.PauseAll();
        }

        private void DeleteAll_Click(object sender, EventArgs e)
        {
            DownloadQueue.DeleteAll();
        }

        private void SearchBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SearchStart();
            }
        }

        public void SearchStart()
        {
            videoList1.DisableAllCards();
            ResultShowReady();
            videoList1.SetTipMessage("正在加载......");
            RealSearchStart();
            panel3.BringToFront();
        }

        private void RealSearchStart()
        {
            v = new Video(SearchBox.Text);
            materialLabel2.Visible = true;
            SeasonSelectBox.Visible = false;
            switch (v.Type)
            {
                case 1:
                    //av
                    VideoClass.AV av = v.av[0];
                    if (av.status)
                    {
                        videoList1.InitCards(v.av[0].episodes);
                        materialLabel2.Text = v.av[0].name;
                        Tabs.SelectTab(1);
                    }

                    break;
                case 3:
                    //SS                    
                    materialLabel2.Visible = false;
                    SeasonSelectBox.Visible = true;
                    SeasonSelectBox.Items.Clear();
                    foreach (VideoClass.SeasonSection ss in v.ss.ss)
                    {
                        SeasonSelectBox.Items.Add(ss.name);
                    }

                    SeasonSelectBox.SelectedIndex = 0;
                    //videoList1.InitCards(v.ss.ss[0].episodes);
                    Tabs.SelectTab(1);
                    break;
                case 5:
                    //Cheese
                    materialLabel2.Text = v.cs.name;
                    videoList1.InitCards(v.cs.episodes);
                    Tabs.SelectTab(1);
                    break;
            }

            videoList1.SetTipMessage("加载完成", false);
        }

        private void Tabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Tabs.SelectedIndex >= 2)
            {
                panel1.Visible = false;
            }
            else
            {
                panel1.Visible = true;
            }
            /*
            if (Tabs.SelectedIndex == 1 && !resultSeeing)
            {
                panel3.Visible = false;
            }
            */
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DownloadQueue.SaveMissons();
            Environment.Exit(0);
        }

        private void materialFlatButton4_Click(object sender, EventArgs e)
        {
            using (Process process = new System.Diagnostics.Process())
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    process.StartInfo.FileName = Environment.CurrentDirectory + "/tools/mp4box.exe";
                }
                else
                {
                    process.StartInfo.FileName = "mp4box";
                }

                process.StartInfo.Arguments = "-version";
                // 禁用操作系统外壳程序 
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true; //输出开启
                process.StartInfo.RedirectStandardError = true;
                string output = "";
                process.OutputDataReceived += new DataReceivedEventHandler((s, e) => { output += e.Data + "\r\n"; });
                process.ErrorDataReceived += new DataReceivedEventHandler((s, e) => { output += e.Data + "\r\n"; });
                process.Start(); //启动进程
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit(2000);
                MessageBox.Show(output);
            }
        }

        private void materialSingleLineTextField1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                foreach (Control c in videoList1.panel2.Controls)
                {
                    if (c is UI.AVCard)
                    {
                        UI.AVCard card = (UI.AVCard) c;

                        card.DPath = materialSingleLineTextField1.Text;
                    }
                }

                MessageBox.Show(materialSingleLineTextField1.Text);
            }
        }

        private void LowCache_CheckedChanged(object sender, EventArgs e)
        {
            Settings.lowcache = LowCache.Checked;
            Settings.SaveSettings();
        }

        private void materialFlatButton5_Click(object sender, EventArgs e)
        {
            Settings.maxMission = int.Parse(materialSingleLineTextField2.Text);
            Settings.SaveSettings();
        }

        private void materialFlatButton6_Click(object sender, EventArgs e)
        {
            About aboutdlg = new About();
            aboutdlg.ShowDialog();
        }

        private void materialComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (materialComboBox1.SelectedIndex == 0)
            {
                Settings.autodark = true;
                Other.RefreshColorSceme();
            }
            else
            {
                Settings.autodark = false;
                Settings.darkmode = materialComboBox1.SelectedIndex == 2 ? false : true;
                Other.RefreshColorSceme();
            }
        }

        private void LoginButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                EditSession form = new EditSession();
                form.ShowDialog();
                RefreshUserData();
            }
            else if (e.Button == MouseButtons.Left)
            {
                if (!User.islogin)
                {
                    UI.BLoginForm loginForm = new UI.BLoginForm
                    {
                        StartPosition = FormStartPosition.CenterScreen
                    };
                    loginForm.Show();
                    loginForm.Login();
                }
                else if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    UserInfoForm uf = new UserInfoForm();
                    uf.ShowDialog();
                }
                else
                {
                    QRLogin form = new QRLogin();
                    form.ShowDialog();
                    RefreshUserData();
                }
            }
        }

        private void APISelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (APISelector.SelectedIndex)
            {
                case 0:
                    APILink.Text = "https://api.bilibili.com/x/player/playurl?fnval=0";
                    Settings.apilink = "https://api.bilibili.com/x/player/playurl?fnval=0";
                    break;
                case 1:
                    APILink.Text = "https://api.bilibili.com/x/player/playurl?fnval=16";
                    Settings.apilink = "https://api.bilibili.com/x/player/playurl?fnval=16";
                    break;
                case 2:
                    APILink.Text = "https://www.biliplus.com/BPplayurl.php?otype=json&module=bangumi";
                    Settings.apilink = "https://www.biliplus.com/BPplayurl.php?otype=json&module=bangumi";
                    break;
                case 3:
                    APILink.Text =
                        "https://api.bilibili.com/x/tv/ugc/playurl?type=&otype=json&device=android&platform=android&mobi_app=android_tv_yst&build=102801&fnver=0&fnval=80";
                    Settings.apilink =
                        "https://api.bilibili.com/x/tv/ugc/playurl?type=&otype=json&device=android&platform=android&mobi_app=android_tv_yst&build=102801&fnver=0&fnval=80";
                    break;
                case 4:
                    APILink.Text = "https://bili.tuturu.top/pgc/player/web/playurl?fnval=16";
                    Settings.apilink = "https://bili.tuturu.top/pgc/player/web/playurl?fnval=16";
                    break;
            }
            Settings.SaveSettings();
        }

        private void materialCheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            Settings.usearia2c = materialCheckBox1.Checked;
            aria2cargu.Visible = materialCheckBox1.Checked;
            materialFlatButton7.Visible = materialCheckBox1.Checked;
            Settings.SaveSettings();
        }

        private void materialFlatButton7_Click(object sender, EventArgs e)
        {
            Settings.aria2cargument = aria2cargu.Text;
            Settings.SaveSettings();
        }

        private void materialCheckBox2_CheckedChanged(object sender, EventArgs e)
        {
            Settings.downloaddanmaku = materialCheckBox2.Checked;
            Settings.SaveSettings();
        }

        private void SeasonSelectBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (v == null) return;
            videoList1.DisableAllCards();
            videoList1.InitCards(v.ss.ss[SeasonSelectBox.SelectedIndex].episodes);
        }

        private void materialFlatButton8_Click(object sender, EventArgs e)
        {
            if (!APILink.Text.StartsWith("http")) APILink.Text = "https://" + APILink.Text;
            if (!APILink.Text.Contains("playurl"))
                APILink.Text += Settings.area == "th"
                    ? "/intl/gateway/v2/ogv/playurl"
                    : "/pgc/player/api/playurl";

            Settings.apilink = APILink.Text;
            Settings.SaveSettings();
        }

        private void materialCheckBox3_CheckedChanged(object sender, EventArgs e)
        {
            Settings.area = AreaSelector.SelectedIndex switch
            {
                0 => "cn",
                1 => "hk",
                2 => "tw",
                3 => "th",
                _ => "cn"
            };
        }

        private void materialCheckBox4_CheckedChanged(object sender, EventArgs e)
        {
            Settings.downloadcc = materialCheckBox4.Checked;
            Settings.SaveSettings();
        }
    }
}