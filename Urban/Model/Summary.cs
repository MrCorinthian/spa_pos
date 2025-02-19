using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Urban.Model
{
    public class IncomeSummary
    {
        public int AccountId { get; set; }
        public int CashM { get; set; }
        public int CashB { get; set; }
        public int CreditM { get; set; }
        public int CreditB { get; set; }
    }

    public class DiscountSummary
    {
        public int AccountId { get; set; }
        public int VoucherCashM { get; set; }
        public int VoucherCreditM { get; set; }
        public int VoucherCashB { get; set; }
        public int VoucherCreditB { get; set; }
    }

    public class CommissionSummary
    {
        public int AccountId { get; set; }
        public int CommisM { get; set; }
        public int CommisB { get; set; }
    }

    public class PaxSummary
    {
        public int AccountId { get; set; }
        public int PaxM { get; set; }
        public int PaxB { get; set; }
    }

}
