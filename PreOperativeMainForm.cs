using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MicronM7Database;
using ImageEnhance;
using ViewerX;

namespace MicronM7_Windows.OperationCase.PreOperative
{
    public partial class PreOperativeMainForm : Form
    {

        private FormDataType formDataType = FormDataType.FormDataTypeIsAdd;
#pragma warning disable 414
        private bool formIsModified = false;
        private bool firstLaunch = true;

        private DBOperativeCase theOperativeCase = null;
        private DBPreOperativeData theODPreOperativeData = null;
        private DBPreOperativeData theOSPreOperativeData = null;

        private DBOperativeCase tmpopCase = null;
        private DBPreOperativeData tmpODInfo = null;
        private DBPreOperativeData tmpOSInfo = null;

        private string selectedPatientuuid = "";
        private string selectedBookedsurgeonuuid = "";

        PatientPickerMainForm patientPicker = null;
        SurgeonPickerMainForm surgeonPicker = null;

        private Bitmap thisBmp = null;

        public PreOperativeMainForm()
        {
            InitializeComponent();
            theOperativeCase = new DBOperativeCase();
            theODPreOperativeData = new DBPreOperativeData();
            theOSPreOperativeData = new DBPreOperativeData();

            tmpopCase = new DBOperativeCase();
            tmpODInfo = new DBPreOperativeData();
            tmpOSInfo = new DBPreOperativeData();

            formDataType = FormDataType.FormDataTypeIsAdd;

        }
        /// <summary>
        /// 二次手術呼叫表單
        /// </summary>
        /// <param name="inCaseuuid">病例識別碼</param>
        /// <param name="inParentPostUuid">父項術後識別碼</param>
        public PreOperativeMainForm(string inCaseuuid, string inParentPostUuid)
        {
            //從 PreOperativeMainForm(string inCaseuuid) 複製
            InitializeComponent();
            firstLaunch = false;

            DBPostOperativeData parentPost = new DBPostOperativeData(inParentPostUuid);

            theOperativeCase = new DBOperativeCase(inCaseuuid);
            tmpopCase = new DBOperativeCase(inCaseuuid);
            if (theOperativeCase.caseuuid != "")
            {
                formDataType = FormDataType.FormDataTypeIsEdit;
                selectedPatientuuid = theOperativeCase.patuuid;
                selectedBookedsurgeonuuid = theOperativeCase.bookedsurgeonuuid;
            }
            else
            {
                //從父項術後找出原病人術後
                formDataType = FormDataType.FormDataTypeIsAdd;
                DBOperativeCase parentCase = new DBOperativeCase(parentPost.caseuuid);
                selectedPatientuuid = parentCase.patuuid;
                doSelectedPatient(selectedPatientuuid);

                theOperativeCase.parentpostopuuid = inParentPostUuid;
                tmpopCase.parentpostopuuid = inParentPostUuid;

            }
            theODPreOperativeData = new DBPreOperativeData(inCaseuuid, "OD");
            theOSPreOperativeData = new DBPreOperativeData(inCaseuuid, "OS");
            tmpODInfo = new DBPreOperativeData(inCaseuuid, "OD");
            tmpOSInfo = new DBPreOperativeData(inCaseuuid, "OS");

            //從父項綁定
            bindPreOperativeInfoFromParentPostUuid(inParentPostUuid);
        }
        public PreOperativeMainForm(string inCaseuuid)
        {
            InitializeComponent();
            theOperativeCase = new DBOperativeCase(inCaseuuid);
            tmpopCase = new DBOperativeCase(inCaseuuid);
            if (theOperativeCase.caseuuid != "")
            {
                formDataType = FormDataType.FormDataTypeIsEdit;
                selectedPatientuuid = theOperativeCase.patuuid;
                selectedBookedsurgeonuuid = theOperativeCase.bookedsurgeonuuid;
            }
            theODPreOperativeData = new DBPreOperativeData(inCaseuuid, "OD");
            theOSPreOperativeData = new DBPreOperativeData(inCaseuuid, "OS");
            tmpODInfo = new DBPreOperativeData(inCaseuuid, "OD");
            tmpOSInfo = new DBPreOperativeData(inCaseuuid, "OS");
        }

        private void PreOperativeMainForm_Load(object sender, EventArgs e)
        {
            
            if (theOperativeCase.caseuuid != "")
            {
                bindFormCaseInfo();
                bindFormPatientAndSurgeonUUID();
                bindFormOSOD();
            }

            Bitmap myImage = new Bitmap(this.Width, this.Height);
            Graphics g = Graphics.FromImage(myImage);
            g.CopyFromScreen(new Point(this.Location.X, this.Location.Y), new Point(0, 0), new Size(this.Width, this.Height));
            IntPtr dc1 = g.GetHdc();
            g.ReleaseHdc(dc1);
            thisBmp = myImage;
            panel1.BackgroundImage = thisBmp.GaussianBlur(10)
                .Brightness(-32);
            //panel2.BackgroundImage = thisBmp.GaussianBlur(10)
            //    .Brightness(-32);
            //panel3.BackgroundImage = thisBmp.GaussianBlur(10)
            //    .Brightness(-32);
            //panel4.BackgroundImage = thisBmp.GaussianBlur(10)
            //    .Brightness(-32);
            //panel5.BackgroundImage = thisBmp.GaussianBlur(10)
            //    .Brightness(-32);
            this.BackgroundImage = thisBmp.GaussianBlur(10)
                 .Brightness(-32);

        }

        private void PreOperativeMainForm_Activated(object sender, EventArgs e)
        {
            if (theOperativeCase.caseuuid == "" && firstLaunch)
            {
                btnChoosePatient_Click(null, null);
            }
            firstLaunch = false;
        }

        public static OperativeMainForm Temp = null;
        #region 表單填值
        private void bindFormCaseInfo()
        {
            if (tb_PatientAges.Text == "")
                tb_PatientAges.Text = theOperativeCase.patientage.ToString();

            if (tb_BookedOPDate.Text == "")
                tb_BookedOPDate.Text = theOperativeCase.bookedopdate;

            if (tb_PreOPDate.Text == "")
                tb_PreOPDate.Text = theOperativeCase.preopdate;

            tb_Comment.Text = theOperativeCase.comment;
        }

        private void bindFormPatientAndSurgeonUUID()
        {

            DBPatientInfo patient = new DBPatientInfo(selectedPatientuuid);
            lb_PatientInfo.Text = String.Format("Name: {0}\r\rBirth: {1}", patient.fullName, patient.birthdayDBString);

            DBUser surgeon = new DBUser(selectedBookedsurgeonuuid);
            lb_BookedInfo.Text = String.Format("Surgeon: {0}", surgeon.displayName);

        }

        private void bindFormOSOD()
        {
            //OD
            tbOD_Cylinder.Text = theODPreOperativeData.cylinder.EMDoubleToString();
            tbOD_Spherical.Text = theODPreOperativeData.spherical.EMDoubleToString();
            tbOD_PreAxis.Text = theODPreOperativeData.preaxis.EMDoubleToString();
            tbOD_UCVA.Text = theODPreOperativeData.ucva.EMDoubleToString();
            tbOD_BSCVA.Text = theODPreOperativeData.bscva.EMDoubleToString();
            tbOD_CCT.Text = theODPreOperativeData.cct.EMDoubleToString();
            tbOD_Flapthickness.Text = theODPreOperativeData.flapthickness.EMDoubleToString();
            tbOD_Pupildiameter_S.Text = theODPreOperativeData.pupildiameter_s.EMDoubleToString();
            tbOD_Pupildiameter_M.Text = theODPreOperativeData.pupildiameter_m.EMDoubleToString();
            tbOD_Pupildiameter_P.Text = theODPreOperativeData.pupildiameter_m.EMDoubleToString();
            tbOD_KValueSteep.Text = theODPreOperativeData.kvaluesteep.EMDoubleToString();
            tbOD_Axis_KS.Text = theODPreOperativeData.axis_ks.EMDoubleToString();
            tbOD_KValueFlat.Text = theODPreOperativeData.kvalueflat.EMDoubleToString();
            tbOD_Axis_KF.Text = theODPreOperativeData.axis_kf.EMDoubleToString();
            tbOD_QValue.Text = theODPreOperativeData.qvalue.EMDoubleToString();
            tbOD_ContactlensPeriod.Text = theODPreOperativeData.contactlensperiod.EMDoubleToString();

            //OS
            tbOS_Cylinder.Text = theOSPreOperativeData.cylinder.EMDoubleToString();
            tbOS_Spherical.Text = theOSPreOperativeData.spherical.EMDoubleToString();
            tbOS_PreAxis.Text = theOSPreOperativeData.preaxis.EMDoubleToString();
            tbOS_UCVA.Text = theOSPreOperativeData.ucva.EMDoubleToString();
            tbOS_BSCVA.Text = theOSPreOperativeData.bscva.EMDoubleToString();
            tbOS_CCT.Text = theOSPreOperativeData.cct.EMDoubleToString();
            tbOS_Flapthickness.Text = theOSPreOperativeData.flapthickness.EMDoubleToString();
            tbOS_Pupildiameter_S.Text = theOSPreOperativeData.pupildiameter_s.EMDoubleToString();
            tbOS_Pupildiameter_M.Text = theOSPreOperativeData.pupildiameter_m.EMDoubleToString();
            tbOS_Pupildiameter_P.Text = theOSPreOperativeData.pupildiameter_m.EMDoubleToString();
            tbOS_KValueSteep.Text = theOSPreOperativeData.kvaluesteep.EMDoubleToString();
            tbOS_Axis_KS.Text = theOSPreOperativeData.axis_ks.EMDoubleToString();
            tbOS_KValueFlat.Text = theOSPreOperativeData.kvalueflat.EMDoubleToString();
            tbOS_Axis_KF.Text = theOSPreOperativeData.axis_kf.EMDoubleToString();
            tbOS_QValue.Text = theOSPreOperativeData.qvalue.EMDoubleToString();
            tbOS_ContactlensPeriod.Text = theOSPreOperativeData.contactlensperiod.EMDoubleToString();
        }
        #endregion

        #region 選擇病患/醫師
        private void btnChoosePatient_Click(object sender, EventArgs e)
        {
            if (patientPicker != null) return;
            patientPicker = new PatientPickerMainForm((string selectedPuuid) =>
            {
                doSelectedPatient(selectedPuuid);
                patientPicker.Close();

            });
            patientPicker.ShowDialog();
            patientPicker.Dispose();
            patientPicker = null;
            if (tb_PreOPDate.Text == "")
            {
                tb_PreOPDate.Text = appHelper.dateToString(DateTime.Now);
            }

        }
        private void doSelectedPatient(string patientuuid)
        {
            selectedPatientuuid = patientuuid;
            bindFormPatientAndSurgeonUUID();
            //重新計算年齡
            DBPatientInfo patient = new DBPatientInfo(selectedPatientuuid);
            TimeSpan ts = appHelper.DateDiffTimeSpan(patient.birthdayDBString.EMStringToDate(), DateTime.Now);
            tb_PatientAges.Text = Math.Floor(Convert.ToDouble(ts.Days) / 365).ToString();

        }
        private void btnChooseSurgeon_Click(object sender, EventArgs e)
        {
            if (surgeonPicker != null) return;
            surgeonPicker = new SurgeonPickerMainForm((string selectedUuuid) =>
            {
                selectedBookedsurgeonuuid = selectedUuuid;
                bindFormPatientAndSurgeonUUID();

                surgeonPicker.Close();

            });
            surgeonPicker.ShowDialog();
            surgeonPicker.Dispose();
            surgeonPicker = null;

            if (tb_BookedOPDate.Text == "")
            {
                tb_BookedOPDate.Text = appHelper.dateToString(DateTime.Now);
            }
        }
        #endregion

        #region 存檔
        private void btnSave_Click(object sender, EventArgs e)
        {
            PupilDiameterWarning();

            if(tbOD_ContactlensPeriod.Text.Trim() == "Years")
            {
                tbOD_ContactlensPeriod.Text = "";
            }

            if (tbOS_ContactlensPeriod.Text.Trim() == "Years")
            {
                tbOS_ContactlensPeriod.Text = "";
            }
            bindObjValueFromForm();
            tmpODInfo.writeToDB();
            if (tmpopCase.writeToDB() && tmpODInfo.writeToDB() && tmpOSInfo.writeToDB())
            {                
                if (selectedBookedsurgeonuuid != "")
                {
                    tmpopCase.status = OperativeStatus.ReadyForOp;
                }
                else
                {
                    tmpopCase.status = OperativeStatus.PreOpFinished;
                }
                tmpopCase.writeToDB();
                formDataType = FormDataType.FormDataTypeIsEdit;

                theOperativeCase.bindValuefromObj(tmpopCase);
                theODPreOperativeData.bindValuefromObj(tmpODInfo);
                theOSPreOperativeData.bindValuefromObj(tmpOSInfo);
                Temp.emCasePicker_renewData();
                new CustomMessageBox().Show("Pre-Operative Data saved!", true);
            }
            else
            {
                new CustomMessageBox().Show("Pre-Operative Data saved Error!", true);

            }
            
        }

        private void PupilDiameterWarning()
        {
            if (tbOD_Pupildiameter_P.Text.EMStringToDecimal() > tbOD_Pupildiameter_M.Text.EMStringToDecimal())
            {
                new CustomMessageBox().Show("OD: Pupil diameter(Photopic) should not exceed Pupil diameter(Mesopic)!", true);
            }
            else if (tbOD_Pupildiameter_M.Text.EMStringToDecimal() > tbOD_Pupildiameter_S.Text.EMStringToDecimal())                
            {
                new CustomMessageBox().Show("OD: Pupil diameter(Mesopic) should not exceed Pupil diameter(Scotopic)!", true);
            }
            if (tbOS_Pupildiameter_P.Text.EMStringToDecimal() > tbOS_Pupildiameter_M.Text.EMStringToDecimal())
            {
                new CustomMessageBox().Show("OS: Pupil diameter(Photopic) should not exceed Pupil diameter(Mesopic)!", true);
            }
            else if (tbOS_Pupildiameter_M.Text.EMStringToDecimal() > tbOS_Pupildiameter_S.Text.EMStringToDecimal())                
            {
                new CustomMessageBox().Show("OS: Pupil diameter(Mesopic) should not exceed Pupil diameter(Scotopic)!", true);
            }
            
        }

        private void CCTWarning()
        {
            if (tbOD_CCT.Text.EMStringToDecimal() < 450)
            {
                new CustomMessageBox().Show("OD: CCT should not be under 500 um for LASIK and 450 for PRK", true);
            }
           
            if (tbOS_CCT.Text.EMStringToDecimal() < 450)
            {
                new CustomMessageBox().Show("OS: CCT should not be under 500 um for LASIK and 450 for PRK!", true);
            }
                        
        }
        

        private void bindObjValueFromForm()
        {
            if (tmpopCase.caseuuid == "")
                tmpopCase.caseuuid = appHelper.newUUID();

            tmpopCase.preopdate = tb_PreOPDate.Text;
            tmpopCase.comment = tb_Comment.Text;
            tmpopCase.patuuid = selectedPatientuuid;
            tmpopCase.patientage = Convert.ToInt32(tb_PatientAges.Text);

            tmpopCase.bookedsurgeonuuid = selectedBookedsurgeonuuid;
            tmpopCase.bookedopdate = tb_BookedOPDate.Text;
            tmpopCase.uuuid = UserHelper.currentLoginUser.uuuid;
            tmpopCase.modidatetime = DateTime.Now.EMDateToString();

            //OD
            if (tmpODInfo.preopuuid == "")
                tmpODInfo.preopuuid = appHelper.newUUID();
            tmpODInfo.caseuuid = tmpopCase.caseuuid;
            tmpODInfo.part = "OD";
            bindViewValueToPreOPObj(tmpODInfo);
            //OS
            if (tmpOSInfo.preopuuid == "")
                tmpOSInfo.preopuuid = appHelper.newUUID();
            tmpOSInfo.caseuuid = tmpopCase.caseuuid;
            tmpOSInfo.part = "OS";
            bindViewValueToPreOPObj(tmpOSInfo);

        }

        private void bindViewValueToPreOPObj(DBPreOperativeData obj)
        {
            if (obj.part == "OD")
            {
                obj.spherical = tbOD_Spherical.Text.EMStringToDouble();                
                obj.cylinder = tbOD_Cylinder.Text.EMStringToDouble();                
                obj.preaxis = tbOD_PreAxis.Text.EMStringToDouble();
                obj.ucva = tbOD_UCVA.Text.EMStringToDouble();
                obj.bscva = tbOD_BSCVA.Text.EMStringToDouble();
                obj.cct = tbOD_CCT.Text.EMStringToDouble();
                obj.flapthickness = tbOD_Flapthickness.Text.EMStringToDouble();
                obj.pupildiameter_s = tbOD_Pupildiameter_S.Text.EMStringToDouble();
                obj.pupildiameter_m = tbOD_Pupildiameter_M.Text.EMStringToDouble();
                obj.pupildiameter_p = tbOD_Pupildiameter_P.Text.EMStringToDouble();
                obj.kvaluesteep = tbOD_KValueSteep.Text.EMStringToDouble();
                obj.axis_ks = tbOD_Axis_KS.Text.EMStringToDouble();
                obj.kvalueflat = tbOD_KValueFlat.Text.EMStringToDouble();
                obj.axis_kf = tbOD_Axis_KF.Text.EMStringToDouble();
                obj.qvalue = tbOD_QValue.Text.EMStringToDouble();
                obj.contactlensperiod = tbOD_ContactlensPeriod.Text.EMStringToDouble();
            }
            else if (obj.part == "OS")
            {
                obj.spherical = tbOS_Spherical.Text.EMStringToDouble();                
                obj.cylinder = tbOS_Cylinder.Text.EMStringToDouble();                
                obj.preaxis = tbOS_PreAxis.Text.EMStringToDouble();
                obj.ucva = tbOS_UCVA.Text.EMStringToDouble();
                obj.bscva = tbOS_BSCVA.Text.EMStringToDouble();
                obj.cct = tbOS_CCT.Text.EMStringToDouble();
                obj.flapthickness = tbOS_Flapthickness.Text.EMStringToDouble();
                obj.pupildiameter_s = tbOS_Pupildiameter_S.Text.EMStringToDouble();
                obj.pupildiameter_m = tbOS_Pupildiameter_M.Text.EMStringToDouble();
                obj.pupildiameter_p = tbOS_Pupildiameter_P.Text.EMStringToDouble();
                obj.kvaluesteep = tbOS_KValueSteep.Text.EMStringToDouble();
                obj.axis_ks = tbOS_Axis_KS.Text.EMStringToDouble();
                obj.kvalueflat = tbOS_KValueFlat.Text.EMStringToDouble();
                obj.axis_kf = tbOS_Axis_KF.Text.EMStringToDouble();
                obj.qvalue = tbOS_QValue.Text.EMStringToDouble();
                obj.contactlensperiod = tbOS_ContactlensPeriod.Text.EMStringToDouble();
            }

        }

        #endregion

        #region 二次手術綁定初始值
        private void bindPreOperativeInfoFromParentPostUuid(string inParentpostopuuid)
        {
            if (formDataType != FormDataType.FormDataTypeIsAdd)
            {
                return;
            }
            DBPostOperativeData parentInfo = new DBPostOperativeData(inParentpostopuuid);
            tb_PreOPDate.Text = parentInfo.postdate;
            string anotherPart = (parentInfo.part == "OD") ? "OS" : "OD";
            DBPostOperativeData anotherPostOpInfo = new DBPostOperativeData(parentInfo.caseuuid, anotherPart, parentInfo.postdate);
            bindPreOperativeInfoFromParentPostObj(parentInfo);
            bindPreOperativeInfoFromParentPostObj(anotherPostOpInfo);
        }

        private void bindPreOperativeInfoFromParentPostObj(DBPostOperativeData obj)
        {
            if (obj.part == "OD")
            {
                tbOD_Spherical.Text = obj.sphere.EMDoubleToString();
                tbOD_Cylinder.Text = obj.cylinder.EMDoubleToString();
                tbOD_PreAxis.Text = obj.axis.EMDoubleToString();
                tbOD_UCVA.Text = obj.ucva.EMDoubleToString();
                tbOD_BSCVA.Text = obj.bscva.EMDoubleToString();
                tbOD_QValue.Text = obj.qvalue.EMDoubleToString();
            }
            else if (obj.part == "OS")
            {
                tbOS_Spherical.Text = obj.sphere.EMDoubleToString();
                tbOS_Cylinder.Text = obj.cylinder.EMDoubleToString();
                tbOS_PreAxis.Text = obj.axis.EMDoubleToString();
                tbOS_UCVA.Text = obj.ucva.EMDoubleToString();
                tbOS_BSCVA.Text = obj.bscva.EMDoubleToString();
                tbOS_QValue.Text = obj.qvalue.EMDoubleToString();
            }
        }


        #endregion

        private void btndataOperationReturn_Click(object sender, EventArgs e)
        {
            this.Close();

        }

        #region Check 勾選狀態
        /// <summary>
        /// Check 勾選旗標
        /// </summary>
        bool Ischk_ODContactlensY = false;
        bool Ischk_ODContactlensN = false;
        bool Ischk_OSContactlensY = false;
        bool Ischk_OSContactlensN = false;

        private void chk_ODContactlensY_Click(object sender, EventArgs e)
        {
            Ischk_ODContactlensY = true;
            Ischk_ODContactlensN = false;
            Show_ODContactlensY_Status();
            Show_ODContactlensN_Status();            
        }

        private void chk_ODContactlensN_Click(object sender, EventArgs e)
        {
            Ischk_ODContactlensY = false;
            Ischk_ODContactlensN = true;
            Show_ODContactlensY_Status();
            Show_ODContactlensN_Status();
        }

        private void chk_OSContactlensY_Click(object sender, EventArgs e)
        {
            Ischk_OSContactlensY = true;
            Ischk_OSContactlensN = false;
            Show_OSContactlensY_Status();
            Show_OSContactlensN_Status();
        }

        private void chk_OSContactlensN_Click(object sender, EventArgs e)
        {
            Ischk_OSContactlensY = false;
            Ischk_OSContactlensN = true;
            Show_OSContactlensY_Status();
            Show_OSContactlensN_Status();
        }

        public void Show_ODContactlensY_Status()
        {
            if (Ischk_ODContactlensY)
            {
                chk_ODContactlensY.Image = Properties.Resources.checkY;
                tbOD_ContactlensPeriod.Visible = true;
            }
            else
            {
                chk_ODContactlensY.Image = Properties.Resources.checkN;
                tbOD_ContactlensPeriod.Visible = false;
            }
        }

        public void Show_ODContactlensN_Status()
        {
            if (Ischk_ODContactlensN)
            {
                chk_ODContactlensN.Image = Properties.Resources.checkY;
            }
            else
            {
                chk_ODContactlensN.Image = Properties.Resources.checkN;
            }
        }
        public void Show_OSContactlensY_Status()
        {
            if (Ischk_OSContactlensY)
            {
                chk_OSContactlensY.Image = Properties.Resources.checkY;
                tbOS_ContactlensPeriod.Visible = true;
            }
            else
            {
                chk_OSContactlensY.Image = Properties.Resources.checkN;
                tbOS_ContactlensPeriod.Visible = false;
            }
        }
        public void Show_OSContactlensN_Status()
        {
            if (Ischk_OSContactlensN)
            {
                chk_OSContactlensN.Image = Properties.Resources.checkY;
            }
            else
            {
                chk_OSContactlensN.Image = Properties.Resources.checkN;
            }
        }

        #endregion

        
        private void tbOD_ContactlensPeriod_MouseDown(object sender, MouseEventArgs e)
        {
            if (tbOD_ContactlensPeriod.Text.Trim() == "Years")
            {
                tbOD_ContactlensPeriod.Text = "";
            }
        }

        private void tbOS_ContactlensPeriod_MouseDown(object sender, MouseEventArgs e)
        {
            if (tbOS_ContactlensPeriod.Text.Trim() == "Years")
            {
                tbOS_ContactlensPeriod.Text = "";
            }
        }

        private void DiopterTimer_Tick(object sender, EventArgs e)
        {
            if (tbOD_Spherical.Text.EMStringToDouble() > 0)
            {
                tbOD_Spherical.ForeColor = Color.Tomato;
            }
            else
                tbOD_Spherical.ForeColor = Color.White;
            if (tbOD_Cylinder.Text.EMStringToDouble() > 0)
            {
                tbOD_Cylinder.ForeColor = Color.Tomato;
            }
            else
                tbOD_Cylinder.ForeColor = Color.White;
            if (tbOS_Spherical.Text.EMStringToDouble() > 0)
            {
                tbOS_Spherical.ForeColor = Color.Tomato;
            }
            else
                tbOS_Spherical.ForeColor = Color.White;
            if (tbOS_Cylinder.Text.EMStringToDouble() > 0)
            {
                tbOS_Cylinder.ForeColor = Color.Tomato;
            }
            else
                tbOS_Cylinder.ForeColor = Color.White;
        }

        

        

        
        
    }
}
