using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using IPQC_CHECKING_SYSTEM;
using IPQC_CHECKING_SYSTEM.Common;
using IPQC_CHECKING_SYSTEM.Model;
using Microsoft.Office.Interop.Excel;
using Application = Microsoft.Office.Interop.Excel.Application;
using Range = Microsoft.Office.Interop.Excel.Range;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace IPQC_CHECKING_SYSTEM
{
    public partial class MainWindow : Form
    {
        
        List<IPQC> lstSource = new List<IPQC>();
        List<IPQC> lstView = new List<IPQC>();
        List<IPQC> lstExport= new List<IPQC>();

        int index = 0;
        int rowCount = 15;
        string returnData;
        string ip = "169.254.5.220";
        string time_get_data;
        Int16 port = 8080;
        Int16 Send = 0;

        UdpClient udpClient = new UdpClient(8080);

        private void ChangeDisplayData()
        {
            for(int j=0;j<lstSource.Count;j++)
            {
                if (lstSource[j].isNOTShown != null && lstSource[j].isNOTShown.Trim().Length > 0)
                {
                    lstExport.Add(lstSource[j]);
                    lstSource.RemoveAt(j);
                    j--;
                }
            }

            if (lstSource.Count <= rowCount)
            {
                lstView = lstSource;
                index = 0;
                return;
            }
            lstView.Clear();
            int i = index;
            while (lstView.Count< lstSource.Count && lstView.Count<=rowCount)
            {
                if (i >= lstSource.Count - 1)
                    i = 0;
                lstView.Add(lstSource[i]);
                i++;                
            }
            index++;
            if (index >= lstSource.Count - 1)
                index = 0;
            dgvData.Refresh();
      
        }

        public MainWindow()
        {
            InitializeComponent();
            readExcelFile();
            Console.WriteLine(DateTime.Now);
            lbl_CurrentDate.Text = "NGÀY: " + DateTime.Now.Day + "/" + DateTime.Now.Month + "/" +DateTime.Now.Year;

          

//            DateTime date1 = DateTime.Parse("24-12-19 18:26 PM");
//            DateTime date2 = DateTime.Parse("24-12-19 19:25 PM");

//            TimeSpan q = date2 - date1;
//            Console.WriteLine(q);
//            Console.WriteLine(q.Days*24 + q.Hours*60 + q.Minutes);
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {

            Thread thdUDPServer = new Thread(new ThreadStart(serverThread));
            thdUDPServer.Start();

            //Danh sach data chua cac POINT trong driver
            List<DataSending> lstData = new List<DataSending>();
            for (int z = 0; z < Driver.lstDataSending.Count; z++)
            {
                lstData.Add(Driver.lstDataSending[z]);
            }
            Driver.lstDataSending.Clear();
            DesDisplay();
            //Neu lstData co gia tri (Trong khoang thoi gian nay nguoi ta co check)
            if (lstData.Count > 0)
            {
                for (int r = 0; r < lstData.Count; r++)
                {
                    DataSending dataSend = lstData[r];
                    if (lstSource.Count == 0)
                    {
                        IPQC ipItem = new IPQC();
                        ipItem.PartNumber = dataSend.partNumber.ToUpper();
                        ipItem.SubmitPIC = dataSend.employee;
                        if (dataSend.value.Trim().Equals(BTN.PRESS_1) || dataSend.value.Trim().Equals(BTN.PRESS_2))
                            ipItem.Type = TYPE.BUY_OFF_SAMPLE;
                        ipItem.Date_TimeSubmit = DateTime.Now.ToString();
                        ipItem.TimeSubmit = DateTime.Now.Hour + ":" + DateTime.Now.Minute;
                        ipItem.Status = Common.STATUS.WAITING;
                        // update online time list source
                        lstSource.Add(ipItem);
                    }
                    else
                    {
                        if (searchData(lstData[r].partNumber) != -1)
                        {
                            int z = searchData(lstData[r].partNumber);
                            // update online time list source
                            switch (dataSend.value)
                            {
                                //case 1 and 2: 
                                case BTN.PRESS_1:
                                case BTN.PRESS_2:
                                    //is repaired
                                    if (STATUS.REPAIR.Equals(lstSource[z].Status) == true)
                                    {
                                        lstSource[z].isRepaired = "T";
                                        IPQC ipItem = new IPQC(); 
                                        ipItem.SubmitPIC = dataSend.employee;
                                        ipItem.PartNumber = dataSend.partNumber;
                                        ipItem.Type = TYPE.REPAIR;
                                        ipItem.Date_TimeSubmit = DateTime.Now.ToString();
                                        ipItem.TimeSubmit = DateTime.Now.Hour + ":" + DateTime.Now.Minute;
                                        ipItem.Status = Common.STATUS.WAITING;
                                        lstSource.Add(ipItem);
                                    }
                                    else
                                    {
                                        MessageBox.Show("Thông tin sản phẩm tồn tại");
                                    }
                                    break;
                                case BTN.PRESS_3:
                                    if (lstSource[z].PartNumber!=null && lstSource[z].PartNumber.Trim().Length>0)
                                    {
                                        lstSource[z].Date_TimeRecive = DateTime.Now.ToString();
                                        lstSource[z].TimeRecive = DateTime.Now.Hour + ":" + DateTime.Now.Minute;
                                        lstSource[z].Status = STATUS.CHECKING;
                                        lstSource[z].IPQC1 = dataSend.employee;
                                    }
                                    else
                                    {
                                        MessageBox.Show("Thao tác sai");
                                    }
                                    break;
                                case BTN.PRESS_OK:
                                    {
                                        if (lstSource[z].TimeRecive != null && lstSource[z].TimeRecive.Trim().Length > 0)
                                        {
                                            if (lstSource[z].Result.Equals(RESULT.OK))
                                            {
                                                MessageBox.Show("Sản phẩm đã được kiểm tra trước đó");
                                            }
                                            else
                                            {
                                                lstSource[z].Date_ReleaseTime = DateTime.Now.ToString();
                                                lstSource[z].ReleaseTime = DateTime.Now.ToString();
                                                lstSource[z].Result = RESULT.OK;
                                                DateTime date1 = DateTime.Parse(lstSource[z].Date_ReleaseTime);
                                                DateTime date2 = DateTime.Parse(lstSource[z].Date_TimeSubmit);
                                                DateTime date3 = DateTime.Parse(lstSource[z].Date_TimeRecive);

                                                TimeSpan compete = date1 - date3;
                                                lstSource[z].CheckingTime = (compete.Days * 24 + compete.Hours * 60 + compete.Minutes).ToString();
                                                compete = date1 - date2;
                                                lstSource[z].TotalTime = (compete.Days * 24 + compete.Hours * 60 + compete.Minutes).ToString();

                                                if (lstSource[z].Type.Equals(TYPE.REPAIR))
                                                {
                                                    for (int j = 0; j < lstSource.Count; j++)
                                                    {
                                                        if (lstSource[j].PartNumber.Equals(lstSource[z].PartNumber) && lstSource[j].Result.Equals(RESULT.NG))
                                                        {
                                                            lstSource[j].isNOTShown = "True";
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        

                                    }
                                    break;
                                case BTN.PRESS_NG:
                                    {
                                        if (lstSource[z].TimeRecive != null && lstSource[z].TimeRecive.Trim().Length > 0)
                                        {
                                            lstSource[z].Date_ReleaseTime = DateTime.Now.ToString();
                                            lstSource[z].ReleaseTime = DateTime.Now.ToString();
                                            lstSource[z].Result = RESULT.NG;
                                            DateTime date1 = DateTime.Parse(lstSource[z].Date_ReleaseTime);
                                            DateTime date2 = DateTime.Parse(lstSource[z].Date_TimeSubmit);
                                            DateTime date3 = DateTime.Parse(lstSource[z].Date_TimeRecive);

                                            TimeSpan compete = date1 - date3;
                                            lstSource[z].CheckingTime = (compete.Days * 24 + compete.Hours * 60 + compete.Minutes).ToString();
                                            compete = date1 - date2;
                                            lstSource[z].TotalTime = (compete.Days * 24 + compete.Hours * 60 + compete.Minutes).ToString();
                                        }
                                    }
                                    break;
                                //case 4 and 5
                                case "4":
                                case "5":
                                    if (lstSource[z].Result == RESULT.NG)
                                    {
                                        lstSource[z].Status = STATUS.REPAIR;
                                        lstSource[z].TimeRepair = DateTime.Now.ToString();
                                        lstSource[z].Date_TimeRepair = DateTime.Now.ToString();
                                    }
                                    break;
                                default:
                                    MessageBox.Show("Value code Error! Code value" + dataSend.value);
                                    break;
                            }
                        }

                        else
                        {
                            if (dataSend.value.Trim().Equals(BTN.PRESS_3) ||
                               dataSend.value.Trim().Equals(BTN.PRESS_OK) ||
                               dataSend.value.Trim().Equals(BTN.PRESS_NG) ||
                               dataSend.value.Trim().Equals(BTN.PRESS_4) ||
                               dataSend.value.Trim().Equals(BTN.PRESS_5))
                            {
                                MessageBox.Show("Thao tác lỗi");
                            }
                            else
                            {
                                IPQC ipItem = new IPQC();
                                ipItem.PartNumber = dataSend.partNumber.ToUpper();
                                ipItem.SubmitPIC = dataSend.employee;
                                if (dataSend.value.Trim().Equals(BTN.PRESS_1) || dataSend.value.Trim().Equals(BTN.PRESS_2))
                                    ipItem.Type = TYPE.BUY_OFF_SAMPLE;
                                ipItem.Date_TimeSubmit = DateTime.Now.ToString();
                                ipItem.TimeSubmit = DateTime.Now.Hour + ":" + DateTime.Now.Minute;
                                ipItem.Status = Common.STATUS.WAITING;
                                // update online time list source
                                lstSource.Add(ipItem);
                            }
                        }
                    }
                }
               
                WriteExcelFile();
                readExcelFile();
            }
          

            ChangeDisplayData();
           

            dgvData.Refresh();

        }

        public void serverThread()
        {
            //UdpClient udpClient = new UdpClient(8080);
            while (true)
            {
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 8080);
                Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
                returnData = Encoding.ASCII.GetString(receiveBytes);
                String str = returnData.ToString();
                string[] arrListStr = str.Split(new char[] { ',' });
                this.Invoke(new MethodInvoker(delegate ()
                {

                    //listBox_received.Items.Add(RemoteIpEndPoint.Address.ToString() + ":" + returnData.ToString());
                    //listBox_received.Items.Add(returnData.ToString());
                    //listBox_received.SelectedIndex = listBox_received.Items.Count - 1;
                    //listBox_received.SelectedIndex = -1;
                    Send = 1;

                }));
                if (Send == 1)
                {
                    bool checking = false;
                    //UdpClient udpClient = new UdpClient();
                    DataSending dataSend = new DataSending();
                    dataSend.partNumber = arrListStr[0].ToUpper();
                    dataSend.value = arrListStr[1].ToUpper();
                    if (arrListStr[1] == "3"|| arrListStr[1] == "2"|| arrListStr[1] == "1") checking = true;
                    if (checking)
                        dataSend.employee = arrListStr[2].ToUpper();

                    Driver.lstDataSending.Add(dataSend);

                    udpClient.Connect(ip, port);
                    time_get_data = getCurrentHourAndMinute();
                    Byte[] senddata = Encoding.ASCII.GetBytes(returnData.ToString());
                    udpClient.Send(senddata, senddata.Length);
                    Send = 0;
                }
            }
        }

        public bool DesDisplay()
        {
            bool flat = false;
            try
            {
                for (int i = 0; i < lstSource.Count; i++)
                {
                    if (lstSource[i].Result != null && lstSource[i].Result.Equals("OK"))
                    {
                        DateTime date1 = DateTime.Parse(lstSource[i].ReleaseTime);
                        string dateTime = DateTime.Now.Hour + ":" + DateTime.Now.Minute;
                        DateTime date2 = DateTime.Parse(dateTime);

                        DateTime compete = DateTime.Parse(date2.Subtract(date1).ToString());
                        int total = compete.Hour * 60 + compete.Minute;
                        if (total >= 30)
                        {
                            lstSource[i].isNOTShown = "F";
                            flat = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return flat;
        }

        public int searchData(string partNumber)
        {
            int z = 0;
            for (z = 0; z < lstSource.Count; z++)
            {
                try
                {
                    if (lstSource[z].PartNumber.Equals(partNumber) && lstSource[z].isRepaired.Trim().Length == 0)
                    {
                        return z;
                    }
                }
                catch
                {

                }
            }

            return -1;
        }

        public string getCurrentHourAndMinute()
        {
            DateTime date =  DateTime.Now;
            return date.Hour + " : " + date.Minute;
        }

        private void MainWindow_Shown(object sender, EventArgs e)
        {
            dgvData.AutoGenerateColumns = false;                              
            lbl_CurrentDate.Focus();
            dgvData.EnableHeadersVisualStyles = false;
            dgvData.DataSource = lstView;
           
            t_Reload_Data.Start();
            t_Reload_View.Start();
        }

        private void dgvData_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.Border);
            using (Pen p = new Pen(Color.Black, 2))
            {
                System.Drawing.Rectangle rect = e.CellBounds;
                rect.Width -= 2;
                rect.Height -= 2;
                e.Graphics.DrawRectangle(p, rect);
            }
            e.Handled = true;
        }

        private void dgvData_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            e.CellStyle.Font = new System.Drawing.Font("Arial", 25);
            if (e.Value != null)
            {
                e.Value = e.Value.ToString().TrimStart().TrimEnd();
                if (("RESULT").Equals(dgvData.Columns[e.ColumnIndex].Name.ToUpper()))
                {
                    string result = e.Value as string;
                    if (result.Contains("OK"))
                        e.CellStyle.BackColor = Color.Green;
                    if (result.Contains("NG"))
                        e.CellStyle.BackColor = Color.Red;
                }
            }
        }

        private void t_Reload_Data_Tick(object sender, EventArgs e)
        {
            MainWindow_Load(this, null);
        }

        private void t_Reload_View_Tick(object sender, EventArgs e)
        {
            dgvData.DataSource = lstView;
            dgvData.Refresh();
            
        }

        private void SendData_Click(object sender, EventArgs e)
        {
            /*if (txtValue.Text.Trim().Length > 0 && txtPartNumber.Text.Trim().Length > 0 && txtEmployee.Text.Trim().Length >0)
            {
                DataSending dataSend = new DataSending();
                dataSend.partNumber = txtPartNumber.Text.ToString();
                dataSend.value = txtValue.Text.ToString();
                dataSend.employee = txtEmployee.Text.ToString();
                Driver.lstDataSending.Add(dataSend);
            }*/
        }

        public void readExcelFile()
        {
            List<IPQC> lstIPQC = new List<IPQC>();
            try
            {
                Workbook MyBook = null;
                Application MyApp = null;
                Worksheet MySheet = null;
                MyApp = new Application();
                MyApp.Visible = false;
                string urlRead = "D:\\Database\\DATABASE.xlsx";
                MyBook = MyApp.Workbooks.Open(urlRead);
                MySheet = (Worksheet)MyBook.Sheets[1];
                int lastRow = MySheet.Cells.SpecialCells(XlCellType.xlCellTypeLastCell).Row;
                int lastColumn = MySheet.Cells.SpecialCells(XlCellType.xlCellTypeLastCell).Column;
                //MessageBox.Show("LastRow= " + lastRow + " LastCol= " + lastColumn);
                int step = 2;
                for (int i = step; i <= lastRow; i++)
                {
                    Microsoft.Office.Interop.Excel.Range rowContent_cellLeft = MySheet.Cells[i, 1];
                    Range rowContent_cellRight = MySheet.Cells[i, 13];
                    System.Array rowContent = (System.Array)MySheet.get_Range(rowContent_cellLeft, rowContent_cellRight).Cells.Value;
                    if (rowContent.GetValue(1, 1) == null)
                        break;
                    string partCode     = rowContent.GetValue(1, 1) + "";
                    string type         = rowContent.GetValue(1, 2) + "";
                    string submitPIC    = rowContent.GetValue(1, 3) + "";
                    string IPQC         = rowContent.GetValue(1, 4) + "";
                    string timeSubmit   = rowContent.GetValue(1, 5) + "";
                    string timeStart    = rowContent.GetValue(1, 6) + "";
                    string releaseTime  = rowContent.GetValue(1, 7) + "";
                    string repairTime   = rowContent.GetValue(1, 8) + "";
                    string checkingTime = rowContent.GetValue(1, 9) + "";
                    string totalTime    = rowContent.GetValue(1, 10) + "";
                    string result       = rowContent.GetValue(1, 11) + "";
                    string isRepaired   = rowContent.GetValue(1, 12) + "";
                    string isNotShown   = rowContent.GetValue(1, 13) + "";
                    
                    
                    IPQC ipqc = new IPQC();
                    try
                    {
                        ipqc.PartNumber = partCode;
                        ipqc.Type = type;
                        ipqc.SubmitPIC = submitPIC;
                        ipqc.IPQC1 = IPQC;

                        ipqc.isRepaired = isRepaired;
                        ipqc.isNOTShown = isNotShown;

                        ipqc.TimeRepair = repairTime;
                        ipqc.CheckingTime = checkingTime;
                        ipqc.TotalTime = totalTime;
                        ipqc.Result = result;
                        ipqc.isRepaired = isRepaired;
                        ipqc.isNOTShown = isNotShown;

                        ipqc.Date_TimeSubmit = timeSubmit;
                        DateTime timeConvert = DateTime.Parse(timeSubmit);
                        ipqc.TimeSubmit = timeConvert.Hour + ":" + timeConvert.Minute;

                        ipqc.Date_TimeRecive = timeStart;
                        timeConvert = DateTime.Parse(timeStart);
                        ipqc.TimeRecive = timeConvert.Hour + ":" + timeConvert.Minute;

                        ipqc.Date_ReleaseTime = releaseTime;
                        timeConvert = DateTime.Parse(releaseTime);
                        ipqc.ReleaseTime = timeConvert.Hour + ":" + timeConvert.Minute;

                        ipqc.Date_TimeRepair = repairTime;
                        timeConvert = DateTime.Parse(repairTime);
                        ipqc.TimeRepair = timeConvert.Hour + ":" + timeConvert.Minute;






                    }
                    catch
                    {
                       
                    }

                    if (ipqc.Result != null && ipqc.Result.Trim().Length > 0)
                    {
                        if ((ipqc.TimeRepair != null && ipqc.TimeRepair.Trim().Length > 0))
                        {
                            ipqc.Status = STATUS.REPAIR;
                        }
                        else
                        {
                            ipqc.Status = STATUS.CHECKED;
                        }
                    }
                    else
                    {
                        if (ipqc.TimeRecive != null && ipqc.TimeRecive.Trim().Length > 0)
                        {
                            ipqc.Status = STATUS.CHECKING;
                        }
                        else
                        {
                            ipqc.Status = STATUS.WAITING;
                        }
                    }


                    lstIPQC.Add(ipqc);

                }

                MyBook.Close(true);
                MyApp.Quit();
                releaseObject(MySheet);
                releaseObject(MyBook);
                releaseObject(MyApp);
            }
            catch (Exception ex)
            {
                //"Không thể load dữ liệu"
               MessageBox.Show(ex.ToString());
            }
            lstSource = lstIPQC;
        }

        private static void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                obj = null;
            }
            finally
            { GC.Collect(); }

        }

        public void WriteExcelFile()
        {
            try
            {
                Workbook MyBook = null;
                Application MyApp = null;
                Worksheet MySheet = null;

                MyApp = new Application();
                MyApp.Visible = false;
                string urlRead = "D:\\Database\\DATABASE.xlsx";

                MyBook = MyApp.Workbooks.Open(urlRead, 0, false, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", true, false, 0, true, 1, 0);
                MySheet = (Worksheet)MyBook.Sheets[1];

                for (int i = 0; i < lstSource.Count; i++)
                {
                    try
                    {
                        
                        int currentIndex = i + 2;
                        Range currentRange = MySheet.get_Range("A" + currentIndex, "M1" + currentIndex);
                        currentRange.Clear();
                        IPQC ipqc = lstSource[i];
                        MySheet.Cells[currentIndex, 1] = ipqc.PartNumber;
                        MySheet.Cells[currentIndex, 2] = ipqc.Type;
                        MySheet.Cells[currentIndex, 3] = ipqc.SubmitPIC;
                        MySheet.Cells[currentIndex, 4] = ipqc.IPQC1;
                        MySheet.Cells[currentIndex, 5] = ipqc.Date_TimeSubmit;
                        MySheet.Cells[currentIndex, 6] = ipqc.Date_TimeRecive;
                        MySheet.Cells[currentIndex, 7] = ipqc.Date_ReleaseTime;
                        MySheet.Cells[currentIndex, 8] = ipqc.Date_TimeRepair;
                        MySheet.Cells[currentIndex, 9] = ipqc.CheckingTime.Replace(":", "H"); 
                        MySheet.Cells[currentIndex, 10] = ipqc.TotalTime.Replace(":", "H"); ;
                        MySheet.Cells[currentIndex, 11] = ipqc.Result;
                        MySheet.Cells[currentIndex, 12] = ipqc.isRepaired;
                        MySheet.Cells[currentIndex, 13] = ipqc.isNOTShown;
                    }
                    catch
                    {
                        
                    }
                   
                }
                MyApp.DisplayAlerts = false;
                MyBook.SaveAs(urlRead, Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookDefault );
                MyBook.Close(true);
                MyApp.Quit();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể lưu dữ liệu");
            }
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            StoredExcelFile(lstSource);

            System.Environment.Exit(1);
        }

        public void StoredExcelFile(List<IPQC> lstIPQC)
        {
            try
            {
                Workbook MyBook = null;
                Application MyApp = null;
                Worksheet MySheet = null;
                Range RangeCell = null;
                bool exist_file = false;

                MyApp = new Application();
                MyApp.Visible = false;
                DateTime current = DateTime.Now;
                string FileName = current.Year + "_" + current.Month + "_" + current.Day + "_IPQC_CHECKING.xlsx";
                string url = "D:\\ExcelData\\" + FileName;
                if (System.IO.File.Exists(url) == false)
                {
                    MyBook = MyApp.Workbooks.Add(System.Reflection.Missing.Value);
                    MySheet = MyBook.ActiveSheet;
                }
                else
                {
                    MyBook = MyApp.Workbooks.Open(url, 0, false, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", true, false, 0, true, 1, 0);
                    MySheet = (Worksheet)MyBook.Sheets[1];
                    exist_file = true;
                }

                MySheet.Cells[1, 1] = "MÃ HÀNG";
                MySheet.Cells[1, 2] = "LOẠI MẨU";
                MySheet.Cells[1, 3] = "NGƯỜI GIAO MẨU";
                MySheet.Cells[1, 4] = "NGƯỜI KIỂM";
                MySheet.Cells[1, 5] = "THỜI GIAN VÀO";
                MySheet.Cells[1, 6] = "THỜI GIAN NHẬN";
                MySheet.Cells[1, 7] = "THỜI GIAN HOÀN THÀNH";
                MySheet.Cells[1, 8] = "THỜI GIAN SỬA CHỮA";
                MySheet.Cells[1, 9] = "THỜI GIAN KIỂM TRA";
                MySheet.Cells[1, 10] = "TỔNG THỜI GIAN XỬ LÝ";
                MySheet.Cells[1, 11] = "KẾT QUẢ";

                MySheet.get_Range("A1", "K1").Font.Bold = true;
                MySheet.get_Range("A1", "K1").VerticalAlignment = Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;
                MySheet.get_Range("A1", "K1").EntireColumn.AutoFit();

                RangeCell = MySheet.get_Range("A1", "K1");
                RangeCell.Interior.ColorIndex = 36;

                for(int j=0;j<lstExport.Count;j++)
                {
                    lstSource.Add(lstExport[j]);
                }

                int lastRow = 2;
                if (exist_file)
                {
                    lastRow = MySheet.Cells.SpecialCells(XlCellType.xlCellTypeLastCell).Row + 1;
                    //MessageBox.Show(lastRow.ToString());
                }

                for (int i = 0; i < lstSource.Count; i++)
                {
                    try
                    {
                        int currentIndex = i + lastRow;
                        IPQC ipqc = lstSource[i];
                        MySheet.Cells[currentIndex, 1] = ipqc.PartNumber;
                        MySheet.Cells[currentIndex, 2] = ipqc.Type;
                        MySheet.Cells[currentIndex, 3] = ipqc.SubmitPIC;
                        MySheet.Cells[currentIndex, 4] = ipqc.IPQC1;
                        if(ipqc.TimeSubmit!=null)
                        {
                            MySheet.Cells[currentIndex, 5] = ipqc.TimeSubmit.Replace(":", "H");
                        }
                        if(ipqc.TimeRecive!=null)
                        {
                            MySheet.Cells[currentIndex, 6] = ipqc.TimeRecive.Replace(":", "H");
                        }

                        if (ipqc.ReleaseTime != null)
                        {
                            MySheet.Cells[currentIndex, 7] = ipqc.ReleaseTime.Replace(":", "H");
                        }

                        MySheet.Cells[currentIndex, 8] = ipqc.TimeRepair.Replace(":", "H");
                        MySheet.Cells[currentIndex, 9] = ipqc.CheckingTime.Replace(":", "H");
                        MySheet.Cells[currentIndex, 9].Interior.Color = 16758272;
                        MySheet.Cells[currentIndex, 10] = ipqc.TotalTime.Replace(":", "H");
                        MySheet.Cells[currentIndex, 10].Interior.Color = 16758272;
                        MySheet.Cells[currentIndex, 11] = ipqc.Result;
                        if(ipqc.Result.Trim().Equals(RESULT.NG))
                        {
                            MySheet.Cells[currentIndex, 11].Characters[1, 3].Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Red);
                        }
                        Range itemRang = MySheet.get_Range("A" + currentIndex, "K" + currentIndex);
                        itemRang.Borders.Weight = Microsoft.Office.Interop.Excel.XlBorderWeight.xlThin;

                    }
                    catch
                    {

                    }

                }
               
                MySheet.Columns.ColumnWidth = 20;
                RangeCell.Font.Size = 14;
                RangeCell.VerticalAlignment = Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;
                RangeCell.Borders.Weight = Microsoft.Office.Interop.Excel.XlBorderWeight.xlThin;
                RangeCell.Interior.ColorIndex = 36;

                MySheet.UsedRange.Select();
                MySheet.Sort.SortFields.Clear();
                MySheet.Sort.SortFields.Add(MySheet.UsedRange.Columns["E"], Microsoft.Office.Interop.Excel.XlSortOn.xlSortOnValues, Microsoft.Office.Interop.Excel.XlSortOrder.xlAscending, System.Type.Missing, Microsoft.Office.Interop.Excel.XlSortDataOption.xlSortNormal);
                var sort = MySheet.Sort;
                sort.SetRange(MySheet.UsedRange);
                sort.Header = Microsoft.Office.Interop.Excel.XlYesNoGuess.xlYes;
                sort.MatchCase = false;
                sort.Orientation = Microsoft.Office.Interop.Excel.XlSortOrientation.xlSortColumns;
                sort.SortMethod = Microsoft.Office.Interop.Excel.XlSortMethod.xlPinYin;
                sort.Apply();


                MyApp.DisplayAlerts = false;
                MyBook.SaveAs(url, Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookDefault);
                MyBook.Close(true);
                MyApp.Quit();

                ClearExcelDataBase();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể Export data dữ liệu");
            }
        }
        public void ClearExcelDataBase()
        {
            for (int i = 0; i < lstSource.Count; i++)
            {
                if (lstSource[i].Result.Equals(RESULT.OK) || lstSource[i].isNOTShown.Trim().Length > 0)
                {
                    lstSource.RemoveAt(i);
                    i--;
                }
            }
            WriteExcelFile();
        }

        private void dgvData_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
           

        }
    }

}
   