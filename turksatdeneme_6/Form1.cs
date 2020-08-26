using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Windows.Forms;
using System.IO.Ports;
using System.Diagnostics;
using System.Threading;
using GMap.NET.CacheProviders;
using System.IO;
using GMap.NET.MapProviders;
using CefSharp;
using CefSharp.WinForms;
using System.Security.Cryptography;
using Accord.Video.FFMPEG;
using Accord.Video.VFW;

namespace turksatdeneme_6
{
    public partial class Form1 : Form
    {
        private VideoCaptureDeviceForm VideoCaptureDevices;

        private VideoFileWriter FileWriter = new VideoFileWriter();
        private SaveFileDialog saveAvi;
        public static Bitmap _latestFrame;
        private int x;
        private int y;
        private int z;
        Bitmap image;
        Bitmap video;
        string base64Text;
        private static List<Telemetri> dataset;
        private static string _data;
        private static string _oldData;
        private FilterInfoCollection webcam; //webcam isminde tanımladığımız değişken bilgisayara kaç kamera bağlıysa onları tutan bir dizi.
        private VideoCaptureDevice cam= null; //cam ise bizim kullanacağımız aygıt.
        public bool IsClosed { get; private set; }
        public ChromiumWebBrowser chromeBrowser;
        public Form1()
        {

            InitializeComponent();
            InitializeChromium();
            //  Javascript'te CefCustomObject sınıfının işleviyle "cefCustomObject" adlı bir nesneyi kaydediyoruz: :3
           //  chromeBrowser.RegisterJsObject("cefCustomObject", new CefCustomObject (chromeBrowser, this));
            //com3 usb bağlıntısını kontrol ediyoruz ve bağlantının açılıp açılmadığını denetliyoruz
            while (true)
                try
                {
                    if (serialPort1.IsOpen == false)
                    {
                        serialPort1.Open();
                        break;
                    }
                }
               catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
               
        }

        void Cam_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap video = (Bitmap)eventArgs.Frame.Clone(); //kısaca bu eventta kameradan alınan görüntüyü picturebox a atıyoruz.
            pcbVideo.Image = video;
            if (butStop.Text == "Kayıt durdu.")
            {
                video = (Bitmap)eventArgs.Frame.Clone();
                pcbVideo.Image = (Bitmap)eventArgs.Frame.Clone();
                FileWriter.WriteVideoFrame(video);
            }
            else
            {
                video = (Bitmap)eventArgs.Frame.Clone();
                pcbVideo.Image = (Bitmap)eventArgs.Frame.Clone();
            }
        }
        private void Cek_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            SaveFileDialog swf = saveFileDialog;
            swf.Filter = "(*.jpg)|*.jpg|Bitma*p(*.bmp)|*.bmp";
            DialogResult dialog = swf.ShowDialog();  //resmi çekiyoruz ve aşağıda da kaydediyoruz.

            if (dialog == DialogResult.OK)
            {
                pcbVideo.Image.Save(swf.FileName);
            }

        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
        webBrowser1.ScriptErrorsSuppressed = true;
            webBrowser1.Navigate($"file:///{Environment.CurrentDirectory}/simulator/index.html#{x++},{y--},{z++}");
            webBrowser1.Refresh();


            var ports = SerialPort.GetPortNames();
            cmbPort.DataSource = ports;

            new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        string data = serialPort1.ReadLine();
                        _data = data;
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Port okuma işlemi sonlandırıldı.");
                    }

                }

            }).Start();
            webcam = new
            FilterInfoCollection(FilterCategory.VideoInputDevice); //webcam dizisine mevcut kameraları dolduruyoruz.
            foreach (FilterInfo item in webcam)
            {
                comboBox1.Items.Add(item.Name); //kameraları combobox a dolduruyoruz.
            }
            comboBox1.SelectedIndex = 0;
            //VideoCaptureDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            //foreach (FilterInfo VideoCaptureDevice in VideoCaptureDevices)
            //{
            //    comboBox1.Items.Add(VideoCaptureDevice.Name);
            //}
            comboBox1.SelectedIndex = 0;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            


        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            cam = new
            VideoCaptureDevice(webcam[comboBox1.SelectedIndex].MonikerString); //başlaya basıldığıdnda yukarda tanımladığımız cam değişkenine comboboxta seçilmş olan kamerayı atıyoruz.
            cam.NewFrame += new NewFrameEventHandler(Cam_NewFrame);
            cam.Start(); //kamerayı başlatıyoruz.
            //cam = new 
            //VideoCaptureDevice(VideoCaptureDevices[comboBox1.SelectedIndex].MonikerString);
            //cam.NewFrame += new NewFrameEventHandler(FinalVideo_NewFrame);
            //cam.Start();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            butStop.Text = "Kayıt durdu.";
            if (cam.IsRunning)
            {
                cam.Stop(); // kamerayı durduruyoruz.
            }
        }



        private void tmrRefresh_Tick(object sender, EventArgs e)// timer ile gelen verileri saniyede bir yenilemeyi sağlayan fonksiyonumuz.
        {
            if (_data != _oldData)
            {
                _oldData = _data;
                string[] pots = _data.Split(',');

                var tele = new Telemetri
                {
                    Takim_No = 55502,// int.Parse(pots[0]),
                    Paket_No = 1,//int.Parse(pots[1]),
                    Gonderme_Zamani =DateTime.Now,//DateTime.Parse(pots[2]),
                    Basinc = 1,//float.Parse(pots[3]) / 100.0f,
                    Yukseklik = 255,// float.Parse(pots[4]) / 100.0f,
                    Inis_Hizi = 0,//float.Parse(pots[5]) / 100.0f,
                    Sicaklik = 29,// float.Parse(pots[6]) / 100.0f,
                    Pil_Gerilimi = 0,//float.Parse(pots[7]) / 100.0f,
                    Pil_Gerilimi2 = 4,//float.Parse(pots[8]) / 100.0f,
                    GPS_Lat = 0,//float.Parse(pots[9]) / 100.0f,
                    GPS_Long = 0,//float.Parse(pots[10]) / 100.0f,
                    GPS_Alt = 0,//float.Parse(pots[11]) / 100.0f,
                    Uydu_Statusu = "Beklemede",//int.Parse(pots[12]) / 100.0f,
                    Pitch = float.Parse(pots[13]) / 100f,
                    Roll = float.Parse(pots[14]) / 100.0f,
                    Yaw = float.Parse(pots[15]) / 100f,
                    Donus_Sayisi = 0,//float.Parse(pots[16]) / 100.0f,
                    Video_Aktarım_Bilgisi = 1,//float.Parse(pots[17]) / 100.0f
                    Manyetik_Alan = 1//float.Parse(pots[18]) / 100.0f,

                };

                Telemetri.Add(tele);
                dataGridView1.DataSource = dataset = Telemetri.GetAll();

                this.chtBsn.Series["Basınç hPa"].Points.AddXY(tele.Gonderme_Zamani.ToString(), tele.Basinc);
                this.chtDns.Series["Dönüş Sayısı"].Points.AddXY(tele.Gonderme_Zamani.ToString(), tele.Donus_Sayisi);
                this.chtGPSLg.Series["GPS Long"].Points.AddXY(tele.Gonderme_Zamani.ToString(), tele.GPS_Long);
                this.chtGPSLt.Series["GPS Lat"].Points.AddXY(tele.Gonderme_Zamani.ToString(), tele.GPS_Lat);
                this.chtHiz.Series["İniş Hızı m/s"].Points.AddXY(tele.Gonderme_Zamani.ToString(), tele.Inis_Hizi);
                this.chtPil.Series["Pil Gerilimi V"].Points.AddXY(tele.Gonderme_Zamani.ToString(), tele.Pil_Gerilimi);
                this.chtPtc.Series["Pitch"].Points.AddXY(tele.Gonderme_Zamani.ToString(), tele.Pitch);
                this.chtRoll.Series["Roll"].Points.AddXY(tele.Gonderme_Zamani.ToString(), tele.Roll);
                this.chtSck.Series["Sıcaklık C"].Points.AddXY(tele.Gonderme_Zamani.ToString(), tele.Sicaklik);
                this.chtYaw.Series["Yaw"].Points.AddXY(tele.Gonderme_Zamani.ToString(), tele.Yaw);
                this.chtYks.Series["Yükseklik m"].Points.AddXY(tele.Gonderme_Zamani.ToString(), tele.Yukseklik);
                webBrowser1.Navigate($"file:///{Environment.CurrentDirectory}/simulator/index.html#{tele.Pitch},{tele.Roll},{tele.Yaw}");
                webBrowser1.Refresh();
                if (tele.Manyetik_Alan == 1)
                {
                    txtOtoAyr.Text = ("Otonom ayrılma gerçekleşmedi.");
                }
                else
                {
                    txtOtoAyr.Text = ("Otonom ayrılma gerçekleşti.");
                }
               // label2.Text = "Device running..." + cam.FramesReceived.ToString() + " FPS";
            }

           


        }



        private void dataGridView1_DataSourceChanged(object sender, EventArgs e)
        {
            txtRPM.Text = dataset[0].Manyetik_Alan.ToString();
            txtGPS_Alt.Text = dataset[0].GPS_Alt.ToString();
            txtStatu.Text = dataset[0].Uydu_Statusu;
            txtBsn.Text = dataset[0].Basinc.ToString();
            txtDns.Text = dataset[0].Donus_Sayisi.ToString();
            txtGnd.Text = dataset[0].Gonderme_Zamani.ToString();
            txtGPSlg.Text = dataset[0].GPS_Long.ToString();
            txtGPSlt.Text = dataset[0].GPS_Lat.ToString();
            txtPil.Text = dataset[0].Pil_Gerilimi.ToString();
            txtPitch.Text = dataset[0].Pitch.ToString();
            txtPkt.Text = dataset[0].Paket_No.ToString();
            txtRoll.Text = dataset[0].Roll.ToString();
            txtSck.Text = dataset[0].Sicaklik.ToString();
            txtTkm.Text = dataset[0].Takim_No.ToString();
            txtYaw.Text = dataset[0].Yaw.ToString();
            txtHiz.Text = dataset[0].Inis_Hizi.ToString();
            txtYks.Text = dataset[0].Yukseklik.ToString();


            map.DragButton = MouseButtons.Right;
            map.MapProvider = GMapProviders.GoogleMap;
            map.Position = new GMap.NET.PointLatLng(dataset[0].GPS_Long, dataset[0].GPS_Lat);
            map.MaxZoom = 1000;
            map.MinZoom = 1;
            map.Zoom = 10;
        }
       
        private void btnVdGnd_Click(object sender, EventArgs e)
        {
            string path = @"D:\sample\base64.txt";
            using (StreamWriter stream = File.CreateText(path))
            {
                // stream.Write(richTextBox1.Text);
                stream.Write(base64Text);
            }
            try
            {
                
                serialPort1.Write(richTextBox1.Text);

                txtVdGndDnt.Text = ("Gönderme başarılı.");

            }
            catch (Exception)
            {
                txtVdGndDnt.Text = ("Gönderme başarısız.");

            }
            if (dataset[0].Video_Aktarım_Bilgisi == 1)
            {
                txtVdKytKnt.Text = ("Video SD karta kaydedildi.");
            }
            else
            {
                txtVdKytKnt.Text = ("Video SD karta kaydedilemedi.");
            }

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //saveAvi = new SaveFileDialog();
            //saveAvi.Filter = "Avi Files (*.avi)|*.avi";
            // FileWriter.WriteVideoFrame(video);
            //if (saveAvi.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //{
            //    int h = VideoCaptureDevices.VideoDevice.VideoResolution.FrameSize.Height;
            //    int w = VideoCaptureDevices.VideoDevice.VideoResolution.FrameSize.Width;
            //    FileWriter.Open(saveAvi.FileName, w, h, 25, VideoCodec.Default, 5000000);
            //    FileWriter.WriteVideoFrame(video);
            //    butStop.Text = "Kayıt durdu.";
            //}
            var Tele = new List<Telemetri>(dataset);
            ExportCsv(Tele, "Telemetriess");
            if (cam.IsRunning == true) cam.Stop();
            Environment.Exit(0);
            Cef.Shutdown();
        }

        public static void ExportCsv<T>(List<T> genericList, string fileName)
        {
            var sb = new StringBuilder();
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var finalPath = Path.Combine(basePath, fileName + ".csv");
            var header = "";
            var info = typeof(T).GetProperties();
            if (!File.Exists(finalPath))
            {
                var file = File.Create(finalPath);
                file.Close();
                foreach (var prop in typeof(T).GetProperties())
                {
                    header += prop.Name + "; ";
                }
                header = header.Substring(0, header.Length - 2);
                sb.AppendLine(header);
                TextWriter sw = new StreamWriter(finalPath, true);
                sw.Write(sb.ToString());
                sw.Close();
            }
            foreach (var obj in genericList)
            {
                sb = new StringBuilder();
                var line = "";
                foreach (var prop in info)
                {
                    line += prop.GetValue(obj, null) + "; ";
                }
                line = line.Substring(0, line.Length - 2);
                sb.AppendLine(line);
                TextWriter sw = new StreamWriter(finalPath, true);
                sw.Write(sb.ToString());
                sw.Close();
            }
        }

        private void btnAyrıl_Click(object sender, EventArgs e)
        {
            serialPort1.Write("ayril");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            serialPort1.Write("julide");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Custom Description";

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                string sSelectedPath = fbd.SelectedPath;
                axWindowsMediaPlayer1.URL = sSelectedPath;
            }

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image Files(*.BMP;*.JPG;*.PNG)|*.BMP;*.JPG;*.PNG" +
            "|All files(*.*)|*.*" + "|Video files(*.AVI; *.MP4)|*.AVI;*.MP4";
            dialog.CheckFileExists = true;
            dialog.Multiselect = false;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                image = new Bitmap(dialog.FileName);
                
                byte[] imageArray = System.IO.File.ReadAllBytes(dialog.FileName);
                base64Text = Convert.ToBase64String(imageArray); // base64Text global olmalı ama richtextbox kullanacağım
                richTextBox1.Text = base64Text;
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.Visible = true;
        }
        public void InitializeChromium()
        {
            CefSettings settings = new CefSettings();
            // cef'i başlatıyoruz
            Cef.Initialize(settings);
            // Bir tarayıcı bileşeni oluşturuyoruz
            chromeBrowser = new ChromiumWebBrowser("https://samsununi.almscloud.com/Account/LoginBefore");
            //   Bunu forma ekleyip ve form penceresine dolduruyoruz.
            this.Controls.Add(chromeBrowser);
            chromeBrowser.Dock = webBrowser1.Dock;
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            webBrowser1.ScrollBarsEnabled = true;
        }
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            butStop.Visible = true;
        }
    }
}
