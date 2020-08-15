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
using System.Security.Cryptography;

namespace turksatdeneme_6
{
    public partial class Form1 : Form
    {
        Bitmap image;
        string base64Text;
        private static List<Telemetri> dataset;
        private static string _data;
        private static string _oldData;
        private FilterInfoCollection webcam; //webcam isminde tanımladığımız değişken bilgisayara kaç kamera bağlıysa onları tutan bir dizi.
        private VideoCaptureDevice cam; //cam ise bizim kullanacağımız aygıt.
        public bool IsClosed { get; private set; }

        public Form1()
        {
            InitializeComponent();
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
            Bitmap bmp = (Bitmap)eventArgs.Frame.Clone(); //kısaca bu eventta kameradan alınan görüntüyü picturebox a atıyoruz.
            pcbVideo.Image = bmp;
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
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;


        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            cam = new
            VideoCaptureDevice(webcam[comboBox1.SelectedIndex].MonikerString); //başlaya basıldığıdnda yukarda tanımladığımız cam değişkenine comboboxta seçilmş olan kamerayı atıyoruz.
            cam.NewFrame += new NewFrameEventHandler(Cam_NewFrame);
            cam.Start(); //kamerayı başlatıyoruz.
        }

        private void btnStop_Click(object sender, EventArgs e)
        {

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
                    Uydu_Statusu = "Beklemede",
                    Basinc = float.Parse(pots[7]) / 100.0f,
                    Donus_Sayisi = 0,
                    Roll = float.Parse(pots[4]) / 100.0f,
                    GPS_Long = 0,
                    Gonderme_Zamani = DateTime.Now,
                    Takim_No = int.Parse(pots[9]),
                    GPS_Lat = 0,
                    GPS_Alt = 0,
                    Inis_Hizi = 0,
                    Paket_No = int.Parse(pots[10]),
                    Pil_Gerilimi = 0,
                    Pitch = float.Parse(pots[1]) / 100f,
                    Yaw = float.Parse(pots[5]) / 100f,
                    Yukseklik = float.Parse(pots[8]) / 100.0f,
                    Sicaklik = float.Parse(pots[6]) / 100.0f,
                    Manyetik_Alan = 1,
                    Pil_Gerilimi2 = 4,
                    Video_Aktarım_Bilgisi = 1
                };

                Telemetri.Add(tele);
                dataGridView1.DataSource = dataset = Telemetri.GetAll();

                this.chtBsn.Series["Basınç hPa"].Points.AddXY(tele.Gonderme_Zamani.ToString(), tele.Basinc);
                this.chtDns.Series["Dönüş Sayısı"].Points.AddXY(tele.Gonderme_Zamani.ToString(), tele.Donus_Sayisi);
                this.chtGPSLg.Series["GPS Long"].Points.AddXY(tele.Gonderme_Zamani.ToString(), tele.GPS_Long);
                this.chtGPSLt.Series["GPS Lat"].Points.AddXY(tele.Gonderme_Zamani.ToString(), tele.GPS_Lat);
                this.chtHiz.Series["İniş Hızı"].Points.AddXY(tele.Gonderme_Zamani.ToString(), tele.Inis_Hizi);
                this.chtPil.Series["Pil Gerilimi"].Points.AddXY(tele.Gonderme_Zamani.ToString(), tele.Pil_Gerilimi);
                this.chtPtc.Series["Pitch"].Points.AddXY(tele.Gonderme_Zamani.ToString(), tele.Pitch);
                this.chtRoll.Series["Roll"].Points.AddXY(tele.Gonderme_Zamani.ToString(), tele.Roll);
                this.chtSck.Series["Sıcaklık"].Points.AddXY(tele.Gonderme_Zamani.ToString(), tele.Sicaklik);
                this.chtYaw.Series["Yaw"].Points.AddXY(tele.Gonderme_Zamani.ToString(), tele.Yaw);
                this.chtYks.Series["Yükseklik m"].Points.AddXY(tele.Gonderme_Zamani.ToString(), tele.Yukseklik);

                if (tele.Manyetik_Alan == 1)
                {
                    txtOtoAyr.Text = ("Otonom ayrılma gerçekleşmedi.");
                }
                else
                {
                    txtOtoAyr.Text = ("Otonom ayrılma gerçekleşti.");
                }
                
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
            var Tele = new List<Telemetri>(dataset);
            ExportCsv(Tele, "Telemetriess");

            Environment.Exit(0);
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
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image Files(*.BMP;*.JPG;*.PNG)|*.BMP;*.JPG;*.PNG" +
            "|All files(*.*)|*.*";
            dialog.CheckFileExists = true;
            dialog.Multiselect = false;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                image = new Bitmap(dialog.FileName);
                pictureBox1.Image = (Image)image;

                byte[] imageArray = System.IO.File.ReadAllBytes(dialog.FileName);
                base64Text = Convert.ToBase64String(imageArray); //base64Text must be global but I'll use  richtext
                richTextBox1.Text = base64Text;
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.Visible = false;
        }
    }
}
