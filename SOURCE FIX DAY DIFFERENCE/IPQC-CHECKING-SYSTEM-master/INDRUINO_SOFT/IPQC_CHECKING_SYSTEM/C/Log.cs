using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Aspose.Cells;
using System.Drawing;

namespace Schedule
{
	class Log : IJob
	{
        /// <summary>
        /// Protect sheet setting
        /// </summary>
        /// <param name="sheet"></param>
        private void ProtectSheet(ref Worksheet sheet)
        {       
            sheet.Protection.Password = "Allied Co.,Ltd";
            sheet.Protection.AllowDeletingColumn = false;
            sheet.Protection.AllowDeletingRow = false;
            sheet.Protection.AllowEditingContent = false;
            sheet.Protection.AllowEditingObject = false;
            sheet.Protection.AllowEditingScenario = false;
            sheet.Protection.AllowFiltering = false;
            sheet.Protection.AllowFormattingCell = false;
            sheet.Protection.AllowFormattingColumn = false;
            sheet.Protection.AllowFormattingRow = false;
            sheet.Protection.AllowInsertingColumn = false;
            sheet.Protection.AllowInsertingHyperlink = false;
            sheet.Protection.AllowInsertingRow = false;
            sheet.Protection.AllowSelectingLockedCell = true;
            sheet.Protection.AllowSelectingUnlockedCell = true;
            sheet.Protection.AllowSorting = true;
            sheet.Protection.AllowUsingPivotTable = true;
            sheet.Protect(Aspose.Cells.ProtectionType.All);
            sheet.AutoFitColumns();
            sheet.AutoFitRows();
        }
        
        /// <summary>
        /// Get cell header style
        /// </summary>
        /// <returns></returns>
        private Aspose.Cells.Style GetHeaderStyle()
        {
            Aspose.Cells.Style style = new Aspose.Cells.Style();
            style.SetBorder(Aspose.Cells.BorderType.TopBorder, CellBorderType.Thin, Color.Black);
            style.SetBorder(Aspose.Cells.BorderType.BottomBorder, CellBorderType.Thin, Color.Black);
            style.SetBorder(Aspose.Cells.BorderType.LeftBorder, CellBorderType.Thin, Color.Black);
            style.SetBorder(Aspose.Cells.BorderType.RightBorder, CellBorderType.Thin, Color.Black);            
            style.Pattern = BackgroundType.Solid;
            style.ForegroundColor = Color.Yellow;
            style.Font.Color = Color.Black;
            style.Font.IsBold = true;
            style.Font.Name = "Arial";
            style.Font.Size = 15;
            style.HorizontalAlignment = TextAlignmentType.Center;            
            return style;
        }
        /// <summary>
        /// Get cell style
        /// </summary>
        /// <returns></returns>
        private Aspose.Cells.Style GetCellStyle()
        {
            Aspose.Cells.Style style = new Aspose.Cells.Style();            
            style.SetBorder(Aspose.Cells.BorderType.TopBorder, CellBorderType.Thin, Color.Black);
            style.SetBorder(Aspose.Cells.BorderType.BottomBorder, CellBorderType.Thin, Color.Black);
            style.SetBorder(Aspose.Cells.BorderType.LeftBorder, CellBorderType.Thin, Color.Black);
            style.SetBorder(Aspose.Cells.BorderType.RightBorder, CellBorderType.Thin, Color.Black);
            style.Font.Color = Color.Black;
            style.Font.IsBold = false;
            style.Font.Name = "Arial";
            style.Font.Size = 13;
            style.HorizontalAlignment = TextAlignmentType.Center;            
            return style;
        }

        /// <summary>
        /// initialize a sheet with default values
        /// </summary>
        /// <param name="sheet"></param>
        private void InitSheet(ref Worksheet sheet)
        {
            var style = GetHeaderStyle();
            sheet.AutoFitColumns();
            sheet.Cells[$"A1"].Value = "Part code";
            sheet.Cells[$"A1"].SetStyle(style);
            sheet.Cells[$"B1"].Value = "Type";
            sheet.Cells[$"B1"].SetStyle(style);
            sheet.Cells[$"C1"].Value = "Submit P.I.C";
            sheet.Cells[$"C1"].SetStyle(style);
            sheet.Cells[$"D1"].Value = "IPQC";
            sheet.Cells[$"D1"].SetStyle(style);
            sheet.Cells[$"E1"].Value = "Time submit";
            sheet.Cells[$"E1"].SetStyle(style);
            sheet.Cells[$"F1"].Value = "Time receive";
            sheet.Cells[$"F1"].SetStyle(style);
            sheet.Cells[$"G1"].Value = "Release time";
            sheet.Cells[$"G1"].SetStyle(style);
            sheet.Cells[$"H1"].Value = "Checking time";
            sheet.Cells[$"H1"].SetStyle(style);
            sheet.Cells[$"I1"].Value = "Total time";
            sheet.Cells[$"I1"].SetStyle(style);
            sheet.Cells[$"J1"].Value = "Result";
            sheet.Cells[$"J1"].SetStyle(style);
        }

        private void ImportColumnWithParameter(List<string> data, string column,ref Worksheet sheet)
        {
            if (data == null)
                return;
            for (int i = 0; i < data.Count; i++)
                sheet.Cells[$"{column}{i+2}"].Value = data[i];
        }

        private void FormatCellStyle(ref Worksheet sheet, int row)
        {
            var cellstyle = GetCellStyle();
            for (int i = 0; i < row; i++)
            {
                sheet.Cells[$"A{i + 2}"].SetStyle(cellstyle);
                sheet.Cells[$"B{i + 2}"].SetStyle(cellstyle);
                sheet.Cells[$"C{i + 2}"].SetStyle(cellstyle);
                sheet.Cells[$"D{i + 2}"].SetStyle(cellstyle);
                sheet.Cells[$"E{i + 2}"].SetStyle(cellstyle);
                sheet.Cells[$"F{i + 2}"].SetStyle(cellstyle);
                sheet.Cells[$"G{i + 2}"].SetStyle(cellstyle);
                sheet.Cells[$"H{i + 2}"].SetStyle(cellstyle);
                sheet.Cells[$"I{i + 2}"].SetStyle(cellstyle);
                sheet.Cells[$"J{i + 2}"].SetStyle(cellstyle);
            }
        }
        /// <summary>
        /// execute export
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
		public Task Execute(IJobExecutionContext context)
		{			
            return Task.Run(()=>
            {
                var rowCount = 0;
                JobDataMap dataMap = context.JobDetail.JobDataMap;
                string FileName = "";
                List<string> PartCode;
                List<string> Type;
                List<string> SubmitPIC;
                List<string> IPQC;
                List<string> TimeSubmit;
                List<string> TimeReceive;
                List<string> ReleaseTime;
                List<string> CheckingTime;
                List<string> TotalTime;
                List<string> Result;

                FileName = context.JobDetail.JobDataMap.GetString("FileName");

                PartCode = context.JobDetail.JobDataMap.Get("PartNumber") as List<string>;
                Type = context.JobDetail.JobDataMap.Get("Type") as List<string>;
                SubmitPIC = context.JobDetail.JobDataMap.Get("SubmitPIC") as List<string>;
                IPQC = context.JobDetail.JobDataMap.Get("IPQC") as List<string>;
                TimeSubmit = context.JobDetail.JobDataMap.Get("TimeSubmit") as List<string>;
                TimeReceive = context.JobDetail.JobDataMap.Get("TimeReceive") as List<string>;
                ReleaseTime = context.JobDetail.JobDataMap.Get("ReleaseTime") as List<string>;
                CheckingTime = context.JobDetail.JobDataMap.Get("CheckingTime") as List<string>;
                TotalTime = context.JobDetail.JobDataMap.Get("TotalTime") as List<string>;
                Result = context.JobDetail.JobDataMap.Get("Result") as List<string>;

                if (PartCode != null)
                    rowCount = PartCode.Count;
                if (Type != null && Type.Count > rowCount)
                    rowCount = Type.Count;
                if (SubmitPIC != null && SubmitPIC.Count > rowCount)
                    rowCount = SubmitPIC.Count;
                if (IPQC != null && IPQC.Count > rowCount)
                    rowCount = IPQC.Count;
                if (TimeSubmit != null && TimeSubmit.Count > rowCount)
                    rowCount = TimeSubmit.Count;
                if (TimeReceive != null && TimeReceive.Count > rowCount)
                    rowCount = TimeReceive.Count;
                if (ReleaseTime != null && ReleaseTime.Count > rowCount)
                    rowCount = ReleaseTime.Count;
                if (CheckingTime != null && CheckingTime.Count > rowCount)
                    rowCount = CheckingTime.Count;
                if (TotalTime != null && TotalTime.Count > rowCount)
                    rowCount = TotalTime.Count;
                if (Result != null && Result.Count > rowCount)
                    rowCount = Result.Count;

                var workbooks = new Workbook();                
                Worksheet sheet = workbooks.Worksheets[0];
                //----------------------------------------------------------------------------
                InitSheet(ref sheet);
                ImportColumnWithParameter(PartCode, "A", ref sheet);
                ImportColumnWithParameter(Type, "B", ref sheet);
                ImportColumnWithParameter(SubmitPIC, "C", ref sheet);
                ImportColumnWithParameter(IPQC, "D", ref sheet);
                ImportColumnWithParameter(TimeSubmit, "E", ref sheet);
                ImportColumnWithParameter(TimeReceive, "F", ref sheet);
                ImportColumnWithParameter(ReleaseTime, "G", ref sheet);
                ImportColumnWithParameter(CheckingTime, "H", ref sheet);
                ImportColumnWithParameter(TotalTime, "I", ref sheet);
                ImportColumnWithParameter(Result, "J", ref sheet);
                FormatCellStyle(ref sheet, rowCount);
                sheet.AutoFitColumns();
                ProtectSheet(ref sheet);    
                
                workbooks.Save(FileName);
            }
            );        
		}
	}
}
