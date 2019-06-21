using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MicronM7_Windows.Database;
using ImageEnhance;
using System.Threading;

namespace MicronM7_Windows.OperationCase.PreOperative
{
    public partial class PatientPickerMainForm : Form
    {
        private EMPatientsList p = null;
        private PatientPickerMainForm()
        {
            InitializeComponent();
           
          
        }

        public PatientPickerMainForm(Action<string> inSelectAction)
        {
            InitializeComponent();
            p = new EMPatientsList((string patientuuid) =>
            {
                inSelectAction(patientuuid);
            });
        }
        private Bitmap thisBmp = null;
        private void PatientPickerMainForm_Load(object sender, EventArgs e)
        {
            
            Bitmap myImage = new Bitmap(this.Width, this.Height);
            Graphics g = Graphics.FromImage(myImage);
            g.CopyFromScreen(new Point(this.Location.X, this.Location.Y), new Point(0, 0), new Size(this.Width, this.Height));
            IntPtr dc1 = g.GetHdc();
            g.ReleaseHdc(dc1);

            thisBmp = myImage;
            this.BackgroundImage = thisBmp.GaussianBlur(10)
                 .Brightness(-32);

            panel.Controls.Add(p);
           
            
        }
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        
      
        
    }
}
