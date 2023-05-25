using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Urban.Model
{
    public class ReceiptSerialize
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string UsedStatus { get; set; }
        public string Created { get; set; }
        public string CreatedBy { get; set; }
        public string Updated { get; set; }
        public string UpdatedBy { get; set; }
    }
}
