#region Using
using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.IO.Compression;
using System.Diagnostics;
using System.ComponentModel;
using AutoUpdaterDotNET;
#endregion

namespace Launcher2._0
{
    public partial class Form1 : Form
    {

        const string url_updater = "http://109.248.11.225/launcher/AutoUpdateData.xml";
        const string url_game = "http://109.248.11.225/download/main.zip";
        const string url_vers = "http://109.248.11.225/download/v.txt";
        const string url_conf = "http://109.248.11.225/download/Config.zip";
        string config_path = (System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + @"\..\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\Config");
        string path = @"C:\Program Files";
        string version = "none";
        string server_version = "none";
        readonly string exe_name = "RimWorldWin64.exe";
        string game_path = "none";

        private bool dd;

        public Form1()
        {
            InitializeComponent();
        }    

        private void Form1_Load(object sender, EventArgs e)
        {
            lblVersion.Text = Application.ProductVersion;

            AutoUpdater.Start(url_updater);

            Config_download();
            label1.Hide();

            //Кнопки ВК и Дискорд
            button2.Text = "VK";
            button2.Click += vk;
            button3.Text = "Discord";
            button3.Click += discrod;

            //Инициализация регистра и создание дефолтного пути установки
            dd = (Registry.CurrentUser.CreateSubKey("RimworldOnline").GetValue("path") == null) | ((string)Registry.CurrentUser.CreateSubKey("RimworldOnline").GetValue("path") == "");
            if (dd)
            {
                game_path = path;
                Registry.CurrentUser.CreateSubKey("RimworldOnline").SetValue("path", game_path);
            }
            else
            {
                game_path = (string)Registry.CurrentUser.CreateSubKey("RimworldOnline").GetValue("path");
            }
            version_controller(game_path);

            //Инициализация кнопок и лейблов

            if (File.Exists(game_path + @"/main.zip"))
            {
                File.Delete(game_path + @"/main.zip");
            }            
        }

        private void version_controller(string path_1)
        {
            version = (string)Registry.CurrentUser.CreateSubKey("RimworldOnline").GetValue("version");

            WebClient client_ver = new WebClient();
            client_ver.DownloadFileAsync(new Uri(url_vers), path_1 + @"\" + "v.txt");
            client_ver.DownloadFileCompleted += Client_ver_DownloadFileCompleted;
        }

        private void Client_ver_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            server_version = File.ReadAllText(game_path + @"\" + "v.txt");
            if (version == null)
            {
                label2.Text = "Игра не установлена";
            }
            else
            {
                label2.Text = ("Установленная Версия Игры: " + version + @". Последняя версия: " + server_version);
            }


            if (version != server_version)
            {
                button1.Text = "Обновить";
                button1.Click += GameUpdate;
            }
            else
            {
                button1.Text = "Играть";
                button1.Click += Play;
            }
        }

        private void GameUpdate(object sender, EventArgs e)
        {
            if ((!(File.Exists(game_path + @"\main"))))
            {

                WebClient client = new WebClient();
                client.DownloadProgressChanged += Client_DownloadProgressChanged;
                client.DownloadFileCompleted += Client_DownloadFileCompleted;
                client.DownloadFileAsync(new Uri(url_game), (game_path + @"\" + "main.zip"));
                button1.Enabled = false;
                button1.BackColor = Color.Green;

            }
            else
            {
                button1.Text = "Играть";
                button1.Click += Play;
            }
            Registry.CurrentUser.CreateSubKey("RimworldOnline").SetValue("version", server_version);
        }
        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar1.Maximum = (int)e.TotalBytesToReceive;
            progressBar1.Value = (int)e.BytesReceived;
            label1.Show();
            label1.Text = (e.ProgressPercentage.ToString() + "%/100%");
        }
        private void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            button1.Enabled = true;
            button1.BackColor = Color.DarkGoldenrod;
            ZipFile.ExtractToDirectory((game_path + @"\main.zip"), game_path);
            File.Delete(game_path + @"\main.zip");
            button1.Text = "Играть";
            button1.Click += Play;
            Application.Restart();
        }




        private void Play(object sender, EventArgs e)
        {
            try
            {
                Process.Start(game_path + @"\main\" + exe_name);
                File.Delete(game_path + @"\v.txt");
                if (File.Exists(game_path + @"\main.zip")) { File.Delete(game_path + @"\main.zip"); }
            }

            catch
            {
                MessageBox.Show("Ошибка запуска клиента. Перезапустите программу");
                if (!((string)Registry.CurrentUser.OpenSubKey("RimworldOnline").GetValue("path") == ""))
                {
                    RegistryKey key = Registry.CurrentUser.CreateSubKey("RimworldOnline");
                    key.SetValue("path", "");
                    key.SetValue("version", "");
                    key.Close();
                }
            }

            finally
            {

                Registry.CurrentUser.Close();
                this.Close();
            }
        }

        private void vk(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://vk.com/rimworldline");
        }

        private void discrod(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://discord.gg/GCQsZKKkTx");
        }



        private void pictureBox1_Click(object sender, EventArgs e) //Выбор локации установки
        {
            folderBrowserDialog1.ShowDialog();
            game_path = folderBrowserDialog1.SelectedPath;
            Registry.CurrentUser.CreateSubKey("RimworldOnline").SetValue("path", game_path);
            Application.Restart();
        }

        private void Config_download()
        {
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(config_path);
                foreach (FileInfo file in dirInfo.GetFiles())
                {
                    file.Delete();
                }

                WebClient client_conf = new WebClient();
                client_conf.DownloadFileAsync(new Uri(url_conf), (config_path + @"\Config.zip"));
                client_conf.DownloadFileCompleted += Client_conf_DownloadFileCompleted;
            }
            catch
            {
                DirectoryInfo dirInfo = new DirectoryInfo((System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + @"\..\LocalLow\"));
                dirInfo.CreateSubdirectory(@"Ludeon Studios\RimWorld by Ludeon Studios\Config");
                WebClient client_conf = new WebClient();
                client_conf.DownloadFileAsync(new Uri(url_conf), (config_path + @"\Config.zip"));
                client_conf.DownloadFileCompleted += Client_conf_DownloadFileCompleted;
            }

        }

        private void Client_conf_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            ZipFile.ExtractToDirectory((config_path + @"\Config.zip"), config_path);
            File.Delete(config_path + @"\Config.zip");
        }

        

    }
}
