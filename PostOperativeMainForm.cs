using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MicronM7Database;
using MicronM7_Windows.OperationCase.PreOperative;

namespace MicronM7_Windows.OperationCase.PostOperative
{
    public partial class PostOperativeMainForm : Form
    {
        private DBOperativeCase theOperativeCase = null;

        private List<PostOPTableDataRow> _dataRowsArray = null;

        public PostOperativeMainForm()
        {
            InitializeComponent();
            theOperativeCase = new DBOperativeCase();

        }
        public PostOperativeMainForm(string inCaseuuid)
        {
            InitializeComponent();
            theOperativeCase = new DBOperativeCase(inCaseuuid);

        }
        
        private void PostOperativeMainForm_Load(object sender, EventArgs e)
        {
            organizedButton(); //執行一次按鈕
            bindFormCaseInfo();
            renewDataRowsArray();
        }
        #region 表單填值
        private void bindFormCaseInfo()
        {
            DBPatientInfo patient = new DBPatientInfo(theOperativeCase.patuuid);
            lb_patientInfo.Text = String.Format("Name: {0}, Ages: {1}", patient.fullName, theOperativeCase.patientage);

            lb_bookedInfo.Text = String.Format("Surgeon: {0}, Operative Date: {1}", theOperativeCase.surgeonName, theOperativeCase.opdate);
            lb_caseStatusInfo.Text = theOperativeCase.status.ToString();
        }

        private void renewDataRowsArray()
        {
            if (_dataRowsArray == null)
            {
                _dataRowsArray = new List<PostOPTableDataRow>();
            }
            else
            {
                _dataRowsArray.Clear();
            }

            PostOPTableDataRow.fetchWithOriginalArray(_dataRowsArray, theOperativeCase.caseuuid, theOperativeCase.opdate,
                (bool success, List<PostOPTableDataRow> resultArray) =>
                {
                    foreach (PostOPTableDataRow p in resultArray)
                    {

                    }
                }
                );

            for (int row_i = 1; row_i < this.tableLayoutPanel1.RowCount; row_i++)
            {
                Control lbTitle = this.tableLayoutPanel1.GetControlFromPosition(0, row_i);
                if (lbTitle != null)
                {
                    PostOPTableDataRow p = _dataRowsArray[row_i - 1];
                    lbTitle.Text = p.title;
                }

                Control odbtn = this.tableLayoutPanel1.GetControlFromPosition(1, row_i);
                if (odbtn != null)
                {
                    PostOPTableDataRow p = _dataRowsArray[row_i - 1];

                    odbtn.Text = p.ODpostDateString == "" ? "ADD +" : p.ODpostDateString;
                    if (p.ODpostDateString != "" && p.ODchildCaseUUID != "")
                    {
                        odbtn.BackColor = Color.Red;                       
                    }
                }

                Control osbtn = this.tableLayoutPanel1.GetControlFromPosition(2, row_i);
                if (osbtn != null)
                {
                    PostOPTableDataRow p = _dataRowsArray[row_i - 1];
                    osbtn.Text = p.OSpostDateString == "" ? "ADD +" : p.OSpostDateString;
                    if (p.OSpostDateString != "" && p.OSchildCaseUUID != "")
                    {
                        osbtn.BackColor = Color.Red;
                    }
                }
            }


        }

        private void organizedButton()
        {
            int column_j = 0;
            int row_i = 0;
            //Console.WriteLine(this.tableLayoutPanel1.ColumnCount);
            //Console.WriteLine(this.tableLayoutPanel1.RowCount);

            for (column_j = 0; column_j <= this.tableLayoutPanel1.ColumnCount; column_j++)
            {
                for (row_i = 0; row_i <= this.tableLayoutPanel1.RowCount; row_i++)
                {
                    Control c = this.tableLayoutPanel1.GetControlFromPosition(column_j, row_i);
                    /*
                    if (c != null)
                    {
                        Console.WriteLine(c.ToString());
                    }*/

                    if (c is Button)
                    {
                        c.Click += new System.EventHandler(this.button_Click);
                        c.BackColor = column_j == 1 ? EMExtensions.ExcelsiusODBackGroundColor : EMExtensions.ExcelsiusOSBackGroundColor;
                    }
                }
            }
        }

        #endregion

        private void button_Click(object sender, EventArgs e)
        {
            int row_i = 0, column_j = 0;
            bool foundControl = false;
            for (row_i = 0; row_i <= this.tableLayoutPanel1.RowCount; row_i++)
            {
                for (column_j = 1; column_j <= 2; column_j++)
                {
                    Control c = this.tableLayoutPanel1.GetControlFromPosition(column_j, row_i);
                    if (sender == c)
                    {
                        foundControl = true;
                        break;
                    }
                }
                if (foundControl)
                {
                    break;
                }
            }

            Console.WriteLine("找到按鈕:{0},{1}", row_i, column_j);
            toPostOpForm(row_i, column_j);
        }

        private void toPostOpForm(int pos_row, int pos_column)
        {
            PostOPTableDataRow p = _dataRowsArray[pos_row - 1];
            p.fetchODOSInfo();

            string click_part = (pos_column == 1) ? "OD" : "OS";
            string OPuuid = (pos_column == 1) ? p.ODpostOpUUID : p.OSpostOpUUID;

           

            PostOPForm f = new PostOPForm(OPuuid, theOperativeCase.caseuuid, click_part, p.startDateString, p.endDateString);
            f.finishAction = (PostOPForm.PostOPFinishedAction action, string postuuid) =>
            {

                if (action == PostOPForm.PostOPFinishedAction.IsRefresh)
                {
                    //f.Close();
                    return;
                }
                if (action == PostOPForm.PostOPFinishedAction.IsChildCase)
                {
                    //f.Hide();
                    string childCaseuuid = DBOperativeCase.caseuuidFromParentPostOPuuid(postuuid);
                    PreOperativeMainForm childPreOPForm = new PreOperativeMainForm(childCaseuuid, postuuid);
                    childPreOPForm.ShowDialog();
                    childPreOPForm.Dispose();
                }
                renewDataRowsArray();
            };

            PostOPForm.Temp = this;
            Update_TabPage("postOPForm", f, OPuuid, theOperativeCase.caseuuid, click_part, p.startDateString, p.endDateString);

            this.passtabControl += new PasstabControl(f.ReceivepasstabControl);
            this.passtabControl(tabControlOperation);  
            //f.ShowDialog();
            //f.Dispose();
            //f = null;

        }

        TabControl tabControlOperation;

        public void ReceivepasstabControl(object sender)
        {
            tabControlOperation = ((TabControl)sender);
        }

        public void Update_TabPage(string str, Form myForm, string selectedCaseuuid)
        {
            tabControlOperation.SelectTab(tabControlOperation.TabPages.Count - 1);
            myForm.FormBorderStyle = FormBorderStyle.None;
            myForm.TopLevel = false;     //令myform卡在tagpage裡 
            this.Hide();
            myForm.Show();
            myForm.Parent = tabControlOperation.SelectedTab;
        }
        public void Update_TabPage(string str, Form myForm, string OPuuid, string caseuuid, string click_part, string startdatestring, string datestring)
        {
            tabControlOperation.SelectTab(tabControlOperation.TabPages.Count - 1);
            myForm.FormBorderStyle = FormBorderStyle.None;
            myForm.TopLevel = false;     //令myform卡在tagpage裡 
            this.Hide();
            myForm.Show();
            myForm.Parent = tabControlOperation.SelectedTab;
        }
        public delegate void PasstabControl(object sender);
        public PasstabControl passtabControl;


        public static OperativeCaseMainForm Temp = null;
        private void btndataOperativeReturn_Click(object sender, EventArgs e)
        {
            this.Close();
            //OperativeCaseMainForm operativeCaseMainForm = new OperativeCaseMainForm(theOperativeCase.patuuid);
            //Update_TabPage("Operative", operativeCaseMainForm, theOperativeCase.patuuid);

            //this.passtabControl += new PasstabControl(operativeCaseMainForm.ReceivepasstabControl);
            //this.passtabControl(tabControlOperation);
            Temp.showform();
        }

        public void showForm()
        {
            this.Show();            
            
            renewDataRowsArray();
        }

    }
}
