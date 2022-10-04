using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Urban.Model
{
    public class AccountSerialize
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string StartMoney { get; set; }
        public string StaffAmount { get; set; }
        public string CreateDateTime { get; set; }
        public string UpdateDateTime { get; set; }
    }
}
