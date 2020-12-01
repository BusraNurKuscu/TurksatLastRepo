using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging; 


namespace turksatdeneme_6
{
    public partial class ImageView : Control
    {
        public ImageView()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            refresh();
        }
       void refresh()
        {
            if(IsDraw==true)
            {
                Invalidate();
            }
        }
        bool IsDraw { get { return Width > 0 && Height > 0; } }

        Image img = null; 
       public Image Image { get { return img; } set { img = value;refresh(); } }
        protected override void OnResize(EventArgs e)
        {
             
            refresh(); 
            
             
            base.OnResize(e);
        }
    
  
      
         protected override void OnPaint(PaintEventArgs pe)
        {
 if(img!=null &&IsDraw==true)
            {
                pe.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
                pe.Graphics.InterpolationMode = InterpolationMode.Low;
                pe.Graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed; 
               
                pe.Graphics.DrawImage(img, pe.ClipRectangle);
            }
            base.OnPaint(pe);
        }
    }
}
