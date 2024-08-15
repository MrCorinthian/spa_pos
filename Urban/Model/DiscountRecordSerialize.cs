using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Urban.Model
{
    public class DiscountRecordSerialize
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public int AccountId { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public int DiscountMasterId { get; set; }
        public int DiscountMasterDetailId { get; set; }
        public string Value { get; set; }
        public string IsCreditCard { get; set; }
        public string CancelStatus { get; set; }
        public string CreateDateTime { get; set; }
        public string UpdateDateTime { get; set; }
        public int OrderReceiptId { get; set; }
    }
}
