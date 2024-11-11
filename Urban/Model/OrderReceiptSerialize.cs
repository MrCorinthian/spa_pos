using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Urban.Model
{
    public class OrderReceiptSerialize
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public int AccountId { get; set; }
        public string ReceiptNo { get; set; }
        public string CancelStatus { get; set; }
        public string CreateDateTime { get; set; }
        public string UpdateDateTime { get; set; }
        public int SellItemTypeId { get; set; }

    }
}
