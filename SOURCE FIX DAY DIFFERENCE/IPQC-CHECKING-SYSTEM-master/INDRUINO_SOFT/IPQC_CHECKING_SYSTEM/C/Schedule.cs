using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using IPQC_CHECKING_SYSTEM;
using IPQC_CHECKING_SYSTEM.Model;
using System.Data;

namespace Schedule
{
    public class Schedule
    {

        public static async void Start(int hh, int mm, int ss, string filename)
                                      
        {
            /*------------------------------------------------------------------
            DELARE
            ------------------------------------------------------------------*/
            JobDataMap _partNumber = new JobDataMap();
            JobDataMap _Type = new JobDataMap();
            JobDataMap _submitPIC = new JobDataMap();
            JobDataMap _iPQC = new JobDataMap();
            JobDataMap _TimeSubmit = new JobDataMap();
            JobDataMap _timeReceive = new JobDataMap();
            JobDataMap _releaseTime = new JobDataMap();
            JobDataMap _checkingTime = new JobDataMap();
            JobDataMap _totalTime = new JobDataMap();
            JobDataMap _result = new JobDataMap();
            DB_Entities db = new DB_Entities();
            List<IPQC> lstdata = null;          
            List<string> PartNumber = new List<string>();
            List<string> Type = new List<string>();
            List<string> SubmitPIC = new List<string>();
            List<string> IPQC = new List<string>();
            List<string> TimeSubmit = new List<string>();
            List<string> TimeReceive = new List<string>();
            List<string> ReleaseTime = new List<string>();
            List<string> CheckingTime = new List<string>();
            List<string> TotalTime = new List<string>();
            List<string> Result = new List<string>();            
            IScheduler scheduler =
                await StdSchedulerFactory.GetDefaultScheduler().ConfigureAwait(false);
            /*------------------------------------------------------------------
            initialize
            ------------------------------------------------------------------*/
            lstdata = db.IPQCs.ToList();           
            foreach (var item in lstdata)
            {
                PartNumber.Add(item.PartNumber);
                Type.Add(item.Type);
                SubmitPIC.Add(item.SubmitPIC);
                IPQC.Add(item.IPQC1);
                TimeSubmit.Add(item.TimeSubmit);
                TimeReceive.Add(item.TimeRecive);
                CheckingTime.Add(item.CheckingTime);
                TotalTime.Add(item.TotalTime);
                Result.Add(item.Result);
                ReleaseTime.Add(item.ReleaseTime);
            }
            _partNumber.Add("PartNumber", PartNumber);
            _Type.Add("Type", Type);
            _submitPIC.Add("SubmitPIC", SubmitPIC);
            _iPQC.Add("IPQC", IPQC);
            _TimeSubmit.Add("TimeSubmit", TimeSubmit);
            _timeReceive.Add("TimeReceive", TimeReceive);
            _releaseTime.Add("ReleaseTime", ReleaseTime);
            _checkingTime.Add("CheckingTime", CheckingTime);
            _totalTime.Add("TotalTime", TotalTime);
            _result.Add("Result", Result);

            await scheduler.Start();
            /*------------------------------------------------------------------
            build report
            ------------------------------------------------------------------*/
            var job = JobBuilder.Create<Log>()
                .UsingJobData("FileName",filename)
                .UsingJobData(_partNumber)
                .UsingJobData(_Type)
                .UsingJobData(_submitPIC)
                .UsingJobData(_iPQC)
                .UsingJobData(_TimeSubmit)
                .UsingJobData(_timeReceive)
                .UsingJobData(_releaseTime)
                .UsingJobData(_checkingTime)
                .UsingJobData(_totalTime)
                .UsingJobData(_result)
                .Build();
            /*------------------------------------------------------------------
            build report trigger 
            ------------------------------------------------------------------*/
            var trigger = TriggerBuilder.Create()
				.WithDailyTimeIntervalSchedule
                ( s=>s
				    .WithIntervalInHours(24)
				    .OnEveryDay()
				    .StartingDailyAt(TimeOfDay.HourMinuteAndSecondOfDay(hh,mm,ss))
				)
				.Build();
            /*------------------------------------------------------------------
            run schedule
            ------------------------------------------------------------------*/
            await scheduler.ScheduleJob(job, trigger);
		}	
	}
}
