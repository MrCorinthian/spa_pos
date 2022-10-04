using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Urban.Model
{
    public class SerializeClass
    {
        public AccountSerialize AccountData { get; set; }
        public List<OrderRecordSerialize> OrderRecordList { get; set; }
    }
}
