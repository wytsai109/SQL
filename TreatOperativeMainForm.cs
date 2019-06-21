using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MicronM7Database;
using ViewerX;
using ImageEnhance;
using MicronM7_Windows.OperationCase.TreatOperative.Parameter;
using AddTable;

namespace MicronM7_Windows.OperationCase.TreatOperative
{
    public partial class TreatOperativeMainForm : Form
    {
        private FormDataType formDataType = FormDataType.FormDataTypeIsAdd;
#pragma warning disable 414
        private bool formIsModified = false;

        private DBOperativeCase thisOperativeCase = null;

        private PreOPInfoControl ODPreInfoControl = null;
        private PreOPInfoControl OSPreInfoControl = null;       

        private DBTreatOperativeData thisODTreatOperativeData = null;
        private DBTreatOperativeData thisOSTreatOperativeData = null;

        public static double ODsphericalcorr = 0;
        public static double OSsphericalcorr = 0;
        public static double ODcylindericlcorr = 0;
        public static double OScylindericlcorr = 0;
        public static double ODcylaxis = 0;
        public static double OScylaxis = 0;
        public double ODCCT = 0;
        public double OSCCT = 0;
        public double ODFlapthickness = 0;
        public double OSFlapthickness = 0;
        
        public bool IsODResidual = false;
        public bool IsOSResidual = false;

        private TreatOperativeParametersControl ODTreatmentOPParaControl = null;
        private TreatOperativeParametersControl OSTreatmentOPParaControl = null;

        public static string currentSelectedStringODOS = "OD";

        public static bool IsODPreParameter = false;
        public static bool IsOSPreParameter = false;

        public TreatOperativeMainForm()
        {
            InitializeComponent();
        }
        public TreatOperativeMainForm(string inCaseuuid)
        {
            InitializeComponent();
            thisOperativeCase = new DBOperativeCase(inCaseuuid);
            if (thisOperativeCase.caseuuid == "")
            {
                new CustomMessageBox().Show("Error Operative Case!!", true);
                this.Close();
                this.Dispose();
                return;
            }

            ODPreInfoControl = new PreOPInfoControl(inCaseuuid, "OD");
            OSPreInfoControl = new PreOPInfoControl(inCaseuuid, "OS");

            //處理 DBTreatOperativeData: OD/OS
            thisODTreatOperativeData = new DBTreatOperativeData(inCaseuuid, "OD");
            thisOSTreatOperativeData = new DBTreatOperativeData(inCaseuuid, "OS");
            if (thisODTreatOperativeData.treatopuuid == "")
            {
                thisODTreatOperativeData.caseuuid = inCaseuuid;
                thisODTreatOperativeData.part = "OD";
            }
            if (thisOSTreatOperativeData.treatopuuid == "")
            {
                thisOSTreatOperativeData.caseuuid = inCaseuuid;
                thisOSTreatOperativeData.part = "OS";
            }

            //this.btnOD.Click += new System.EventHandler(this.btnODOS_Click);
            //this.btnOS.Click += new System.EventHandler(this.btnODOS_Click);
            
            //
            DBPreOperativeData ODtmpObj = new DBPreOperativeData(inCaseuuid, "OD");
            DBPreOperativeData OStmpObj = new DBPreOperativeData(inCaseuuid, "OS");

            ODsphericalcorr = ODtmpObj.spherical;
            OSsphericalcorr = OStmpObj.spherical;
            ODcylindericlcorr = ODtmpObj.cylinder;
            OScylindericlcorr = OStmpObj.cylinder;
            ODcylaxis = ODtmpObj.preaxis;
            OScylaxis = OStmpObj.preaxis;
            ODCCT = ODtmpObj.cct;
            OSCCT = OStmpObj.cct;
            ODFlapthickness = ODtmpObj.flapthickness;
            OSFlapthickness = OStmpObj.flapthickness;

            if (thisODTreatOperativeData.treatopuuid != "" || thisOSTreatOperativeData.treatopuuid != "")
            {
                formDataType = FormDataType.FormDataTypeIsEdit;
            }

        }


        private Bitmap thisBmp = null;
        private void TreatOperativeMainForm_Load(object sender, EventArgs e)
        {
            Bitmap myImage = new Bitmap(this.Width, this.Height);
            Graphics g = Graphics.FromImage(myImage);
            g.CopyFromScreen(new Point(this.Location.X, this.Location.Y), new Point(0, 0), new Size(this.Width, this.Height));
            IntPtr dc1 = g.GetHdc();
            g.ReleaseHdc(dc1);

            thisBmp = myImage;
            this.panel1.BackgroundImage = thisBmp.GaussianBlur(10)
                 .Brightness(-32);

            /*
            if (currentSelectedStringODOS == "OD")
            {
                btnOD.Select();
                btnOD.PerformClick();

            }
            else
            {
                btnOS.Select();
                btnOS.PerformClick();
            }*/


            bindCaseInfo();
            if (ODTreatmentOPParaControl != null)
            {
                ODTreatmentOPParaControl.removeFromParentControl(true);
            }
            ODTreatmentOPParaControl = new TreatOperativeParametersControl(thisODTreatOperativeData, true);
            ODTreatmentOPParaControl.Size = panel_TreatmentOP.Size;

            if (OSTreatmentOPParaControl != null)
            {
                OSTreatmentOPParaControl.removeFromParentControl(true);
            }
            OSTreatmentOPParaControl = new TreatOperativeParametersControl(thisOSTreatOperativeData, true);
            OSTreatmentOPParaControl.Size = panel_TreatmentOP.Size;

            btnOD_Click(sender, e);
        }
        //private void TreatOperativeMainForm_Activated(object sender, EventArgs e)
        //{
        //    bindCaseInfo();
        //    if (ODTreatmentOPParaControl != null)
        //    {
        //        ODTreatmentOPParaControl.removeFromParentControl(true);
        //    }
        //    ODTreatmentOPParaControl = new TreatOperativeParametersControl(thisODTreatOperativeData, true);
        //    ODTreatmentOPParaControl.Size = panel_TreatmentOP.Size;

        //    if (OSTreatmentOPParaControl != null)
        //    {
        //        OSTreatmentOPParaControl.removeFromParentControl(true);
        //    }
        //    OSTreatmentOPParaControl = new TreatOperativeParametersControl(thisOSTreatOperativeData, true);
        //    OSTreatmentOPParaControl.Size = panel_TreatmentOP.Size;

        //    if (currentSelectedStringODOS == "OD")
        //    {
        //        btnOD.Select();
        //        btnOD.PerformClick();
        //    }
        //    else
        //    {
        //        btnOS.Select();
        //        btnOS.PerformClick();
        //    }
        //}

        private void bindCaseInfo()
        {

            lb_PatientInfo.Text = String.Format("{0}, Ages:{1}", thisOperativeCase.patientName, thisOperativeCase.patientage);
            lb_PreOPDate.Text = thisOperativeCase.preopdate;
            lb_OperativeStatus.Text = thisOperativeCase.status.ToString();

        }

        
        #region 按鈕事件
        //private void btnODOS_Click(object sender, EventArgs e)
        //{
        //    System.Windows.Forms.Control btn = (System.Windows.Forms.Control)sender;
        //    currentSelectedStringODOS = btn.Text;// "OD";
        //    if (currentSelectedStringODOS == "OD")
        //    {
        //        btnOD.BackColor = EMExtensions.ExcelsiusODBackGroundColor;
        //        btnOD.ForeColor = EMExtensions.ExcelsuisWhiteColor;
        //        btnOS.BackColor = EMExtensions.ExcelsiusOSBackGroundColor;
        //        btnOS.ForeColor = Color.Black;
        //        lblResidual.BackColor = Color.FromArgb(62, 111, 143);

        //        OSPreInfoControl.removeFromParentControl(false);
        //        if (OSTreatmentOPParaControl != null)
        //        {
        //            OSTreatmentOPParaControl.removeFromParentControl(false);
        //        }
        //        if (ODTreatmentOPParaControl == null)
        //        {
        //            ODTreatmentOPParaControl = new TreatOperativeParametersControl(thisODTreatOperativeData, true);
        //        }
        //        if (thisODTreatOperativeData.treattypename == "")
        //        {
        //            lblhint.Visible = true;
        //            lblhint.BackColor = Color.FromArgb(62, 111, 143);
        //        }
        //        else
        //        {
        //            lblhint.Visible = false;
        //        }
        //        panel_PreOP.Controls.Add(ODPreInfoControl);
        //        panel_TreatmentOP.Controls.Add(ODTreatmentOPParaControl);
        //    }
        //    else if (currentSelectedStringODOS == "OS")
        //    {
        //        btnOS.BackColor = EMExtensions.ExcelsiusOSBackGroundColor;
        //        btnOS.ForeColor = EMExtensions.ExcelsuisWhiteColor;
        //        btnOD.BackColor = EMExtensions.ExcelsiusODBackGroundColor;
        //        btnOD.ForeColor = Color.Black;
        //        lblResidual.BackColor = Color.FromArgb(24, 104, 0);

        //        ODPreInfoControl.removeFromParentControl(false);
        //        if (ODTreatmentOPParaControl != null)
        //        {
        //            ODTreatmentOPParaControl.removeFromParentControl(false);
        //        }
        //        if (OSTreatmentOPParaControl == null)
        //        {
        //            OSTreatmentOPParaControl = new TreatOperativeParametersControl(thisOSTreatOperativeData, true);
        //        }
        //        if (thisOSTreatOperativeData.treattypename == "")
        //        {
        //            lblhint.Visible = true;
        //            lblhint.BackColor = Color.FromArgb(24, 104, 0);
        //        }
        //        else
        //        {
        //            lblhint.Visible = false;
        //        }
        //        panel_PreOP.Controls.Add(OSPreInfoControl);
        //        panel_TreatmentOP.Controls.Add(OSTreatmentOPParaControl);
        //    }

            
        //}
        public void btnOD_Click(object sender, EventArgs e)
        {
            currentSelectedStringODOS = "OD";
            btnOD.BackColor = EMExtensions.ExcelsiusODBackGroundColor;
            btnOD.ForeColor = EMExtensions.ExcelsuisWhiteColor;
            btnOS.BackColor = EMExtensions.ExcelsiusOSBackGroundColor;
            btnOS.ForeColor = Color.Black;
            //lblResidual.BackColor = Color.FromArgb(62, 111, 143);
            //lblablationdepth.BackColor = Color.FromArgb(62, 111, 143);            
            
            OSPreInfoControl.removeFromParentControl(false);
            if (OSTreatmentOPParaControl != null)
            {
                OSTreatmentOPParaControl.removeFromParentControl(false);
            }
            if (ODTreatmentOPParaControl == null)
            {
                ODTreatmentOPParaControl = new TreatOperativeParametersControl(thisODTreatOperativeData, true);
            }
            if (thisODTreatOperativeData.treattypename == "")
            {
                lblhint.Visible = true;
                lblhint.BackColor = Color.FromArgb(62, 111, 143);
            }
            else
            {
                lblhint.Visible = false;
            }

            ODTreatmentOPParaControl.Size = panel_TreatmentOP.Size;
            panel_PreOP.Controls.Add(ODPreInfoControl);
            panel_TreatmentOP.Controls.Add(ODTreatmentOPParaControl);
        }

        public void btnOS_Click(object sender, EventArgs e)
        {
            currentSelectedStringODOS = "OS";
            btnOS.BackColor = EMExtensions.ExcelsiusOSBackGroundColor;
            btnOS.ForeColor = EMExtensions.ExcelsuisWhiteColor;
            btnOD.BackColor = EMExtensions.ExcelsiusODBackGroundColor;
            btnOD.ForeColor = Color.Black;
            //lblResidual.BackColor = Color.FromArgb(24, 104, 0);
            //lblablationdepth.BackColor = Color.FromArgb(24, 104, 0);            

            ODPreInfoControl.removeFromParentControl(false);
            if (ODTreatmentOPParaControl != null)
            {
                ODTreatmentOPParaControl.removeFromParentControl(false);
            }
            if (OSTreatmentOPParaControl == null)
            {
                OSTreatmentOPParaControl = new TreatOperativeParametersControl(thisOSTreatOperativeData, true);
            }
            if (thisOSTreatOperativeData.treattypename == "")
            {
                lblhint.Visible = true;
                lblhint.BackColor = Color.FromArgb(24, 104, 0);
            }
            else
            {
                lblhint.Visible = false;
            }

            OSTreatmentOPParaControl.Size = panel_TreatmentOP.Size;
            panel_PreOP.Controls.Add(OSPreInfoControl);
            panel_TreatmentOP.Controls.Add(OSTreatmentOPParaControl);
        }
        
        private void btnSelectTreatmentType_Click(object sender, EventArgs e)
        {
            TreatmentTypePicker.Temp = this;
            IsODPreParameter = true;
            IsOSPreParameter = true;
            string _toTreatmentTypeuuid = (currentSelectedStringODOS == "OD") ? thisODTreatOperativeData.treattypeuuid : thisOSTreatOperativeData.treattypeuuid;
            bool _toTrapsepi = (currentSelectedStringODOS == "OD") ? thisODTreatOperativeData.isTransepi : thisOSTreatOperativeData.isTransepi;
            TreatmentTypePicker p = new TreatmentTypePicker(_toTreatmentTypeuuid, currentSelectedStringODOS, _toTrapsepi, (bool isChoosed, string selectedTreatmentTypeUUID, string ODOSString, bool selectedTransepi) =>
            {
                Console.WriteLine("{0},{1},{2}", isChoosed.ToString(), selectedTreatmentTypeUUID, ODOSString);

                if (!isChoosed)
                {
                    return;
                }
                bindSelectTreatmentTypeUUID(selectedTreatmentTypeUUID, ODOSString, selectedTransepi);
               
            });
            p.ShowDialog();
            p.Dispose();

            //lblResidual.Visible = false;
        }

        private void bindSelectTreatmentTypeUUID(string inSelectedUUID, string inODOS, bool inTransepi)
        {
            if (inODOS == "OD")
            {
                if (ODTreatmentOPParaControl != null)
                {
                    ODTreatmentOPParaControl.removeFromParentControl(true);
                    ODTreatmentOPParaControl = null;
                }
                thisODTreatOperativeData.isTransepi = inTransepi;
                thisODTreatOperativeData.createParameters(inSelectedUUID);
                ODTreatmentOPParaControl = new TreatOperativeParametersControl(thisODTreatOperativeData, true);
                btnOD.PerformClick();

            }
            else if (inODOS == "OS")
            {
                if (OSTreatmentOPParaControl != null)
                {
                    OSTreatmentOPParaControl.removeFromParentControl(true);
                    OSTreatmentOPParaControl = null;
                }
                thisOSTreatOperativeData.isTransepi = inTransepi;
                thisOSTreatOperativeData.createParameters(inSelectedUUID);
                OSTreatmentOPParaControl = new TreatOperativeParametersControl(thisOSTreatOperativeData, true);
                btnOS.PerformClick();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (thisODTreatOperativeData.treatopuuid == "")
            {
                thisODTreatOperativeData.treatopuuid = appHelper.newUUID();
            }
            if (thisOSTreatOperativeData.treatopuuid == "")
            {
                thisOSTreatOperativeData.treatopuuid = appHelper.newUUID();
            }
            //存檔
            bool ODsavesuccess = true;
            bool OSsavesuccess = true;
            if (thisODTreatOperativeData.writePreTreatDataToDB())
            {
                ODsavesuccess = thisODTreatOperativeData.writeAllParameters();


                if (thisODTreatOperativeData.treattypename == "LASIK" || thisODTreatOperativeData.treattypename == "PRK" || thisODTreatOperativeData.treattypename == "Tepi-PRK")
                {
                    
                    Operation.LASIK_display(thisODTreatOperativeData.Parameter_Values[0], thisODTreatOperativeData.Parameter_Values[1], thisODTreatOperativeData.Parameter_Values[2],
                        thisODTreatOperativeData.Parameter_Values[5], thisODTreatOperativeData.Parameter_Values[6],
                       thisODTreatOperativeData.Parameter_Values[7], thisODTreatOperativeData.Parameter_Values[8]);
                }
                else if (thisODTreatOperativeData.treattypename == "Presbyopia" || thisODTreatOperativeData.treattypename == "Tepi-Presbyopia")
                {
                   
                    Operation.Presbyopia_display(thisODTreatOperativeData.Parameter_Values[0], thisODTreatOperativeData.Parameter_Values[1], thisODTreatOperativeData.Parameter_Values[2],
                        thisODTreatOperativeData.Parameter_Values[5], thisODTreatOperativeData.Parameter_Values[6], thisODTreatOperativeData.Parameter_Values[7],
                        thisODTreatOperativeData.Parameter_Values[8], thisODTreatOperativeData.Parameter_Values[9], thisODTreatOperativeData.Parameter_Values[10]);
                }
                if (Operation.disp_sph_surgery_depth > Operation.disp_cyl_surgery_depth)
                {
                    Operation.ODResidualStromalBedThickness = (ODCCT - ODFlapthickness - (double)Operation.disp_sph_surgery_depth );
                }
                else
                {
                    Operation.ODResidualStromalBedThickness = (ODCCT - ODFlapthickness - (double)Operation.disp_cyl_surgery_depth);
                }
                
            }
            else
            {
                ODsavesuccess = false;
            }

            if (thisOSTreatOperativeData.writePreTreatDataToDB())
            {
                OSsavesuccess = thisOSTreatOperativeData.writeAllParameters();


                if (thisOSTreatOperativeData.treattypename == "LASIK" || thisOSTreatOperativeData.treattypename == "PRK" || thisOSTreatOperativeData.treattypename == "Tepi-PRK")
                {                   
                    Operation.LASIK_display(thisOSTreatOperativeData.Parameter_Values[0], thisOSTreatOperativeData.Parameter_Values[1], thisOSTreatOperativeData.Parameter_Values[2],
                        thisOSTreatOperativeData.Parameter_Values[5], thisOSTreatOperativeData.Parameter_Values[6],
                       thisOSTreatOperativeData.Parameter_Values[7], thisOSTreatOperativeData.Parameter_Values[8]);
                }
                else if (thisOSTreatOperativeData.treattypename == "Presbyopia" || thisOSTreatOperativeData.treattypename == "Tepi-Presbyopia")
                {                    
                    Operation.Presbyopia_display(thisOSTreatOperativeData.Parameter_Values[0], thisOSTreatOperativeData.Parameter_Values[1], thisOSTreatOperativeData.Parameter_Values[2],
                        thisOSTreatOperativeData.Parameter_Values[5], thisOSTreatOperativeData.Parameter_Values[6], thisOSTreatOperativeData.Parameter_Values[7],
                        thisOSTreatOperativeData.Parameter_Values[8], thisOSTreatOperativeData.Parameter_Values[9], thisOSTreatOperativeData.Parameter_Values[10]);
                }


                if (Operation.disp_sph_surgery_depth > Operation.disp_cyl_surgery_depth)
                {
                    Operation.OSResidualStromalBedThickness = (OSCCT - OSFlapthickness - (double)Operation.disp_sph_surgery_depth);
                }
                else
                {
                    Operation.OSResidualStromalBedThickness = (OSCCT - OSFlapthickness - (double)Operation.disp_cyl_surgery_depth);
                }
                
            }
            else
            {
                OSsavesuccess = false;
            }


            if (currentSelectedStringODOS == "OD")
            {
                lblablationdepth.Text = "Ablation depth : " + Operation.disp_sph_surgery_depth.ToString() + "μm";
                lblResidual.Text = "Residual stromal bed : " + Operation.ODResidualStromalBedThickness.ToString() + "μm";
                if (Operation.ODResidualStromalBedThickness < ResidualStromalBedThicknessLimit)
                {
                    lblResidual.ForeColor = Color.Red;
                }
                else
                {
                    lblResidual.ForeColor = Color.White;
                }
                
            }
            else if (currentSelectedStringODOS == "OS")
            {
                lblablationdepth.Text = "Ablation depth : " + Operation.disp_sph_surgery_depth.ToString() + "μm";
                lblResidual.Text = "Residual stromal bed : " + Operation.OSResidualStromalBedThickness.ToString() + "μm";
                if (Operation.OSResidualStromalBedThickness < ResidualStromalBedThicknessLimit)
                {
                    lblResidual.ForeColor = Color.Red;
                }
                else
                {
                    lblResidual.ForeColor = Color.White;
                }
              
            }


            if (ODsavesuccess && OSsavesuccess)
            {
                new CustomMessageBox().Show("Treatment Parameters Saved!", true);
                formIsModified = true;
            }
            else
            {
                new CustomMessageBox().Show("Save Treatment Parameters Error!", true);
            }

            
            
        }
        #endregion

        #region 頁面定義，切換頁面
        TabControl tabControlOperation;
        public static MicronM7Win Temp = null;
        public void ReceivepasstabControl(object sender)
        {
            tabControlOperation = ((TabControl)sender);
        }

        public void Update_TabPage(string str, Form myForm, string caseuuid)
        {
            tabControlOperation.SelectTab(tabControlOperation.TabPages.Count - 1);
            myForm.FormBorderStyle = FormBorderStyle.None;
            myForm.TopLevel = false;     //令myform卡在tagpage裡       
            //this.Hide();
            myForm.Show();
            myForm.Parent = tabControlOperation.SelectedTab;
        }
        
        private void btnTreatMentProcess_Click(object sender, EventArgs e)
        {
            if (formIsModified)
            {
                if (thisODTreatOperativeData.treattypename == "Tepi-Presbyopia" || thisODTreatOperativeData.treattypename == "Tepi-PRK" ||
                    thisOSTreatOperativeData.treattypename == "Tepi-Presbyopia" || thisOSTreatOperativeData.treattypename == "Tepi-PRK")
                {
                    
                    thisOperativeCase.surgeonuuid = UserHelper.currentLoginUser.uuuid;
                    thisOperativeCase.status = OperativeStatus.Operation;
                    this.thisOperativeCase.modidatetime = appHelper.dateToString(DateTime.Now);
                    if (!thisOperativeCase.writeToDB())
                    {
                        new CustomMessageBox().Show("To Treatment Operative Unknow Error.", true);
                        return;
                    }
                    this.Close();
                    residualstromaltimer.Enabled = false;
                    //TreatProcessMainForm treatProcessMainForm = new TreatProcessMainForm(thisOperativeCase, thisODTreatOperativeData, thisOSTreatOperativeData);
                    //Update_TabPage("treatProcessMainForm", treatProcessMainForm, thisOperativeCase, thisODTreatOperativeData, thisOSTreatOperativeData);
                    ChangePage(Page.DBepiConfirm);
                    Temp.epiSetting(thisOperativeCase, thisODTreatOperativeData, thisOSTreatOperativeData);
                    //Temp = null;
                }
                else
                {
                    thisOperativeCase.surgeonuuid = UserHelper.currentLoginUser.uuuid;
                    thisOperativeCase.status = OperativeStatus.Operation;
                    this.thisOperativeCase.modidatetime = appHelper.dateToString(DateTime.Now);
                    if (!thisOperativeCase.writeToDB())
                    {
                        new CustomMessageBox().Show("To Treatment Operative Unknow Error.", true);
                        return;
                    }
                    this.Close();
                    residualstromaltimer.Enabled = false;
                    //TreatProcessMainForm treatProcessMainForm = new TreatProcessMainForm(thisOperativeCase, thisODTreatOperativeData, thisOSTreatOperativeData);
                    //Update_TabPage("treatProcessMainForm", treatProcessMainForm, thisOperativeCase, thisODTreatOperativeData, thisOSTreatOperativeData);
                    ChangePage(Page.Surgery);
                    Temp.ReceiveData(thisOperativeCase, thisODTreatOperativeData, thisOSTreatOperativeData);
                    //Temp = null;
                }
                
            }
            else
            {
                new CustomMessageBox().Show("Save first.", true);
                return;
            }
                    
        }
        
        /// <summary>
        /// 頁面
        /// </summary>
        public enum Page : int
        {
            Main = 0, Surgery = 2, DBepiConfirm = 28
        };

        /// <summary>
        /// 換頁面用方法
        /// </summary>
        /// <param name="p"></param>
        public void ChangePage(Page p)
        {
            nowSelectPage = p;
            tabControlOperation.SelectedIndex = (int)p;

        }

        //---------------
        /// <summary>
        /// 目前選擇的頁面
        /// </summary>
        Page nowSelectPage = Page.Main;
        
        public delegate void PasstabControl(object sender);
        public PasstabControl passtabControl;
        #endregion

        private void btnTreatOperativeReturn_Click(object sender, EventArgs e)
        {
            this.Close();
            OperativeCaseMainForm operativeCaseMainForm = new OperativeCaseMainForm(thisOperativeCase.caseuuid);
            Update_TabPage("OperativeCase", operativeCaseMainForm, thisOperativeCase.caseuuid);
            this.passtabControl += new PasstabControl(operativeCaseMainForm.ReceivepasstabControl);
            this.passtabControl(tabControlOperation);
        }

        public int ResidualStromalBedThicknessLimit = 310;
        private void residualstromaltimer_Tick(object sender, EventArgs e)
        {
            if (currentSelectedStringODOS == "OD")
            {
                if (thisODTreatOperativeData.treattypename == "LASIK" || thisODTreatOperativeData.treattypename == "PRK" || thisODTreatOperativeData.treattypename == "Tepi-PRK")
                {
                    lblResidual.Visible = true;
                    lblablationdepth.Visible = true;
                    panel_depthbed.Visible = true;
                    Operation.LASIK_display(thisODTreatOperativeData.Parameter_Values[0], thisODTreatOperativeData.Parameter_Values[1], thisODTreatOperativeData.Parameter_Values[2],
                        thisODTreatOperativeData.Parameter_Values[5], thisODTreatOperativeData.Parameter_Values[6],
                       thisODTreatOperativeData.Parameter_Values[7], thisODTreatOperativeData.Parameter_Values[8]);
                }
                else if (thisODTreatOperativeData.treattypename == "Presbyopia" || thisODTreatOperativeData.treattypename == "Tepi-Presbyopia")
                {
                    lblResidual.Visible = true;
                    lblablationdepth.Visible = true;
                    panel_depthbed.Visible = true;
                    Operation.Presbyopia_display(thisODTreatOperativeData.Parameter_Values[0], thisODTreatOperativeData.Parameter_Values[1], thisODTreatOperativeData.Parameter_Values[2],
                        thisODTreatOperativeData.Parameter_Values[5], thisODTreatOperativeData.Parameter_Values[6], thisODTreatOperativeData.Parameter_Values[7],
                        thisODTreatOperativeData.Parameter_Values[8], thisODTreatOperativeData.Parameter_Values[9], thisODTreatOperativeData.Parameter_Values[10]);
                }
                else
                {
                    lblResidual.Visible = false;
                    lblablationdepth.Visible = false;
                    panel_depthbed.Visible = false;
                }

                if (Operation.disp_sph_surgery_depth > Operation.disp_cyl_surgery_depth)
                {
                    Operation.ODResidualStromalBedThickness = (ODCCT - ODFlapthickness - (double)Operation.disp_sph_surgery_depth);
                }
                else
                {
                    Operation.ODResidualStromalBedThickness = (ODCCT - ODFlapthickness - (double)Operation.disp_cyl_surgery_depth);
                }
                lblResidual.Text = "Residual stromal bed : " + Operation.ODResidualStromalBedThickness.ToString() + "μm";
                lblablationdepth.Text = "Ablation Depth : " + Operation.disp_sph_surgery_depth + "μm";
                if (Operation.ODResidualStromalBedThickness < ResidualStromalBedThicknessLimit)
                {
                    lblResidual.ForeColor = Color.Red;
                }
                else
                {
                    lblResidual.ForeColor = Color.White;
                }
                
            }
            else
            {
                if (thisOSTreatOperativeData.treattypename == "LASIK" || thisOSTreatOperativeData.treattypename == "PRK" || thisOSTreatOperativeData.treattypename == "Tepi-PRK")
                {
                    lblResidual.Visible = true;
                    lblablationdepth.Visible = true;
                    panel_depthbed.Visible = true;
                    Operation.LASIK_display(thisOSTreatOperativeData.Parameter_Values[0], thisOSTreatOperativeData.Parameter_Values[1], thisOSTreatOperativeData.Parameter_Values[2],
                        thisOSTreatOperativeData.Parameter_Values[5], thisOSTreatOperativeData.Parameter_Values[6],
                       thisOSTreatOperativeData.Parameter_Values[7], thisOSTreatOperativeData.Parameter_Values[8]);
                }
                else if (thisOSTreatOperativeData.treattypename == "Presbyopia" || thisOSTreatOperativeData.treattypename == "Tepi-Presbyopia")
                {
                    lblResidual.Visible = true;
                    lblablationdepth.Visible = true;
                    panel_depthbed.Visible = true;
                    Operation.Presbyopia_display(thisOSTreatOperativeData.Parameter_Values[0], thisOSTreatOperativeData.Parameter_Values[1], thisOSTreatOperativeData.Parameter_Values[2],
                        thisOSTreatOperativeData.Parameter_Values[5], thisOSTreatOperativeData.Parameter_Values[6], thisOSTreatOperativeData.Parameter_Values[7],
                        thisOSTreatOperativeData.Parameter_Values[8], thisOSTreatOperativeData.Parameter_Values[9], thisOSTreatOperativeData.Parameter_Values[10]);
                }
                else
                {
                    lblResidual.Visible = false;
                    lblablationdepth.Visible = false;
                    panel_depthbed.Visible = false;
                }


                if (Operation.disp_sph_surgery_depth > Operation.disp_cyl_surgery_depth)
                {
                    Operation.OSResidualStromalBedThickness = (OSCCT - OSFlapthickness - (double)Operation.disp_sph_surgery_depth);
                }
                else
                {
                    Operation.OSResidualStromalBedThickness = (OSCCT - OSFlapthickness - (double)Operation.disp_cyl_surgery_depth);
                }

                lblResidual.Text = "Residual stromal bed : " + Operation.OSResidualStromalBedThickness.ToString() + "μm";
                lblablationdepth.Text = "Ablation Depth : " + Operation.disp_sph_surgery_depth + "μm";
                if (Operation.OSResidualStromalBedThickness < ResidualStromalBedThicknessLimit)
                {
                    lblResidual.ForeColor = Color.Red;
                }
                else
                {
                    lblResidual.ForeColor = Color.White;
                }
                
            }
        }



        

    }
}
