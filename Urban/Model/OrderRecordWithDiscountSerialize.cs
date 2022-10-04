using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Urban.Model
{
    public class OrderRecordWithDiscountSerialize
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public int AccountId { get; set; }
        public int OrderRecordId { get; set; }
        public int DiscountRecordId { get; set; }
        public string CreateDateTime { get; set; }
        public string UpdateDateTime { get; set; }
    }
}
