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
using System.Drawing.Imaging;
using AviFile;
using GMap.NET.WindowsForms;

namespace turksatdeneme_6
{
    public partial class Form1 : Form
    {
        public static List<Bitmap> bitmaps = new List<Bitmap>();
        public static Bitmap _latestFrame;
        private int x;
        private int y;
        private int z;
        string base64Text;
        private static List<Telemetri> dataset;
        private static string _data;
        private static string _oldData;
        private FilterInfoCollection webcam; //webcam isminde tanımladığımız değişken bilgisayara kaç kamera bağlıysa onları tutan bir dizi.
        public bool IsClosed { get; private set; }
        public ChromiumWebBrowser chromeBrowser;
        public Form1()
        {

            InitializeComponent();
          
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

        
        private void Cek_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            SaveFileDialog swf = saveFileDialog;
            swf.Filter = "(*.jpg)|*.jpg|Bitma*p(*.bmp)|*.bmp";
            DialogResult dialog = swf.ShowDialog();  //resmi çekiyoruz ve aşağıda da kaydediyoruz.

            if (dialog == DialogResult.OK)
            {
               
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
            videoRecorder1.Start();
            
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            videoRecorder1.Stop(); 

        }
        private static void MakeAvi(List<Bitmap> maps)
        {
            AviManager mana = new AviManager("deneme1.avi", false);
            VideoStream avistream = mana.AddVideoStream(false, 21, maps[0]);

            for (int i = 1; i < maps.Count; i++)
            {
                avistream.AddFrame(maps[i]);
            }
            mana.Close();
        }




        private void tmrRefresh_Tick(object sender, EventArgs e)// timer ile gelen verileri saniyede bir yenilemeyi sağlayan fonksiyonumuz.
        {
            if (_data != _oldData)
            {
                _oldData = _data;
                string[] pots = _data.Split(',');

                var tele = new Telemetri
                {
                    Takim_No = int.Parse(pots[0]),
                    Paket_No = int.Parse(pots[1]),
                    Gonderme_Zamani = DateTime.Now,//DateTime.Parse(pots[2]),
                    Basinc = float.Parse(pots[3]) / 100.0f,
                    Yukseklik = float.Parse(pots[4]) / 100.0f,
                    Inis_Hizi = float.Parse(pots[5]) / 100.0f,
                    Sicaklik = float.Parse(pots[6]) / 100.0f,
                    Pil_Gerilimi = float.Parse(pots[7]) / 100.0f,
                    Pil_Gerilimi2 = float.Parse(pots[8]) / 100.0f,
                    GPS_Lat =  float.Parse(pots[9]) / 1000000.0f,
                    GPS_Long =float.Parse(pots[10]) / 1000000.0f,
                    GPS_Alt = float.Parse(pots[11]) / 100.0f,
                    Uydu_Statusu = Convert.ToString(pots[12]),
                    Pitch = float.Parse(pots[13]) / 100f,
                    Roll = float.Parse(pots[14]) / 100.0f,
                    Yaw = float.Parse(pots[15]) / 100f,
                    Donus_Sayisi = float.Parse(pots[16]) / 100.0f,
                    Video_Aktarım_Bilgisi = float.Parse(pots[17]) / 100.0f,
                    Manyetik_Alan = float.Parse(pots[18]) / 100.0f,
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
                try
                {
                    StringBuilder adress = new StringBuilder();
                    adress.Append($"https://www.google.com.tr/maps/#{tele.GPS_Lat},{tele.GPS_Long},{tele.GPS_Alt}");
                }
                catch
                {
                    MessageBox.Show("Beklenmedik bir hata oluştu.");
                }
            }




        }



        private void dataGridView1_DataSourceChanged(object sender, EventArgs e)
        {
            txtRPM.Text = dataset[0].Manyetik_Alan.ToString();
            txtGPS_Alt.Text = dataset[0].GPS_Alt.ToString();
            txtStatu.Text = dataset[0].Uydu_Statusu.ToString();
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
            textBox3.Text = dataset[0].GPS_Long.ToString();
            txtCadde.Text = dataset[0].GPS_Lat.ToString();
            textBox4.Text = dataset[0].GPS_Alt.ToString();


        }
       
        private void btnVdGnd_Click(object sender, EventArgs e)
        {
            string path = @"D:\sample\base64.txt";
            using (StreamWriter stream = File.CreateText(txt_path.Text))
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
            serialPort1.Write("motoron");
        }

        private void button2_Click(object sender, EventArgs e)
        {
          
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                this.txt_path.Text = dialog.FileName;
                rhcVdGnd.Text = txt_path.Text;
                byte[] imageArray = System.IO.File.ReadAllBytes(dialog.FileName);
                base64Text = Convert.ToBase64String(imageArray); // base64Text global olmalı ama richtextbox kullanacağım
                richTextBox1.Text = base64Text;

               
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.Visible = false;
        }
       

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            webBrowser1.ScrollBarsEnabled = true;
        }
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            butStop.Visible = true;
        }


        private void button1_Click_1(object sender, EventArgs e)
        {
            serialPort1.Write("motoroff");
        }

        private void txt_path_TextChanged(object sender, EventArgs e)
        {
            txt_path.Visible = false;
        }

        private void videoRecorder1_OnNewPictureCaptured(Bitmap bmp)
        {
            ımageView1.Image = bmp;
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {

        }

        private void btnbul_Click(object sender, EventArgs e)
        {
            map.DragButton = MouseButtons.Left;
            map.MapProvider = GMapProviders.GoogleMap;
            double lat = Convert.ToDouble(txtCadde.Text);
            double longt = Convert.ToDouble(textBox3.Text);
            map.Position = new GMap.NET.PointLatLng(lat,longt);
            map.MinZoom = 10;
            map.MaxZoom = 10000;
            map.Zoom = 10;

            GMap.NET.PointLatLng point = new GMap.NET.PointLatLng(lat, longt);
           
        }

        private void button4_Click(object sender, EventArgs e)
        {
            serialPort1.Write("birles");
        }
    }
}
