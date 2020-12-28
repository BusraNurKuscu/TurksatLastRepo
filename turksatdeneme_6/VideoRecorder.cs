
using AForge.Video;
using AForge.Video.DirectShow;
using AviFile;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace turksatdeneme_6
{
   public class VideoRecorder :Component
    {
        public VideoRecorder()
        {
            webcam = new
                        FilterInfoCollection(FilterCategory.VideoInputDevice);
        }
       public FilterInfoCollection webcam;
      public VideoCaptureDevice cam = null;
  public      AviManager mana;
       public VideoStream avistream;
        public ComboBox comboBox;
        bool init = false;
        Bitmap video; 

        Bitmap first;
        string path = "test.avi"; 
        public string FileName { get { return path; } set { value = path; } }
        double framerate = 10;
 public double Rate
        {
            get { return framerate; }
            set { framerate = value; }
        }
        void set()
        {
          
            mana = new AviManager(path, false);  
            avistream = mana.AddVideoStream(false, framerate, first);
            init = true; 
            
        }
      void clear()
        {
            GC.GetTotalMemory(true);
            GC.Collect(GC.MaxGeneration);
            GC.WaitForPendingFinalizers();
            GC.GetTotalMemory(true);
        }
        void Cam_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if(init==false)
            {
                first= eventArgs.Frame;
                set();
            }
            video = (Bitmap)eventArgs.Frame.Clone(); //kısaca bu eventta kameradan alınan görüntüyü picturebox a atıyoruz.
            sendImage(video);


            avistream.AddFrame(eventArgs.Frame);

            clear();


        }
        public void Start()
        {
            start();
        }
        bool _clear = false; 
        public bool FileRefreshOnStart { get { return _clear; } set { _clear = value; } }
        bool deleteFile(string path)
        {
            try
            {
               
                File.Delete(path);
                return true;
            }
            catch
            {
                return false;
            }
            }
        public void Stop()
        {
            stop();
        }
       
        void stop()
        {
            cam.Stop();
            mana.Close(); 
        }
        void start()
        {
          cam=  new
            VideoCaptureDevice(webcam[comboBox.SelectedIndex].MonikerString);
            cam.NewFrame += new NewFrameEventHandler(Cam_NewFrame);
            cam.Start();
        }
      public  void list(ComboBox combox)
        {
        for(int i=0;i<webcam.Count;i++)
            {
                comboBox.Items.Add(webcam[i].Name);
            }
         
        }
        void sendImage(Bitmap bmp)
        {
            try
            {
                OnNewPictureCaptured(bmp);
            }
            catch
            {

            }
        }
            
        public delegate void NewPictureCaptured(Bitmap bmp);
        public event NewPictureCaptured OnNewPictureCaptured; 

        public ComboBox ComboBox
        {
            get { return comboBox; }
            set
            {
                comboBox = value;
               
                if(comboBox.Items.Count>0)
                {
                    comboBox.Items.Clear();
                }
                list(comboBox);
             
            } 
        }
        
    }
}
