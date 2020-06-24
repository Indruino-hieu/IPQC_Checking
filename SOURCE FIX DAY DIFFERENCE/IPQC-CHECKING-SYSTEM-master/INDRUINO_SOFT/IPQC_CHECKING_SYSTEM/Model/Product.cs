using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPQC_CHECKING_SYSTEM.Model
{
    class Product
    {
        public string Partnumber;
        public string Type;
        public string Time_in;
        public string SubmitPIC;
        public string IPQC;
        public string TimeSubmit;
        public string TimeRecive;
        public string ReleaseTime;
        public string CheckingTime;
        public string Result;
        public string Status;

        public Product()
        {

        }

        public Product(string partnumber, string time_in, string type, string result, string status)
        {
            this.Partnumber = partnumber;
            this.Time_in = time_in;
            this.Type = type;
            this.Result = result;
            this.Status = status;
        }
    }
}
