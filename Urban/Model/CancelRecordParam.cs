using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Urban.Model
{
    public class CancelRecordParam
    {
        public int OrderRecordId { get; set; }
        public int AccountId { get; set; }
        public int ItemNo { get; set; }
        public int TotalItems { get; set; }
        public string CancelStatus { get; set; }
    }
}
