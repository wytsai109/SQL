using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MicronM7Database;
using MicronM7_Windows.Database;
using ImageEnhance;

namespace MicronM7_Windows.OperationCase.PreOperative
{
    public partial class SurgeonPickerMainForm : Form
    {
        private EMUserAccountsList u = null;
        private SurgeonPickerMainForm() { InitializeComponent(); }
        public SurgeonPickerMainForm(Action<string> inSelectedAction)
        {
            InitializeComponent();
            u = new EMUserAccountsList(ListAccountType.ListAccountTypeIsOnlyPhysician, (string selecteduuuid) => {
                inSelectedAction(selecteduuuid);
            });
        }

        private Bitmap thisBmp = null;

        private void SurgeonPickerMainForm_Load(object sender, EventArgs e)
        {
            Bitmap myImage = new Bitmap(this.Width, this.Height);
            Graphics g = Graphics.FromImage(myImage);
            g.CopyFromScreen(new Point(this.Location.X, this.Location.Y), new Point(0, 0), new Size(this.Width, this.Height));
            IntPtr dc1 = g.GetHdc();
            g.ReleaseHdc(dc1);

            thisBmp = myImage;
            this.BackgroundImage = thisBmp.GaussianBlur(10)
                 .Brightness(-32);

            panel.Controls.Add(u);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
