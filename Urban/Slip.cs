using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Urban
{
    public class Slip
    {
        public string DateStamp;
        public List<PlanMassage> Plans;

        public Slip()
        {
            this.DateStamp = DateTime.Now.ToString("dd MMMM yyyy    HH:mm");
            this.Plans = new List<PlanMassage>();
        }

        public void addPlan(PlanMassage plan)
        {
            this.Plans.Add(plan);
        }

        private string getPlanName(PlanMassage plan)
        {
            string text = "-";
            if (plan.Type == 8)
            {
                text += "[Add] ";
                if(plan.Value == 29)
                {
                    text += "Foot Massage (OutSide)";
                }
                else if (plan.Value == 30)
                {
                    text += "Foot Massage (Inside)";
                }
                else if (plan.Value == 31)
                {
                    text += "Thai Massage";
                }
                else if (plan.Value == 32)
                {
                    text += "Back, Head & Shoulder Massage";
                }
                else if (plan.Value == 33)
                {
                    text += "Oil Massage";
                }
                else if (plan.Value == 34)
                {
                    text += "Aroma Oil Massage";
                }
            }
            else if (plan.Type == 1)
            {
                text += "Foot Massage (Outside)";
            }
            else if (plan.Type == 2)
            {
                text += "Foot Massage (Inside)";
            }
            else if (plan.Type == 3)
            {
                text += "Thai Massage";
            }
            else if (plan.Type == 4)
            {
                text += "Back, Head & Shoulder Massage";
            }
            else if (plan.Type == 5)
            {
                text += "Oil Massage";
            }
            else if(plan.Type == 6)
            {
                text += "Aroma Massage";
            }
            else
            {
                text += "[Package]\n";
            }
            //if (plan.Value >= 29 && plan.Value <= 34)
            //{
            //    text += "[เพิ่ม]";
            //}
            //if (plan.Value % 7 == 1)
            //    text += "Foot Massage (Outside) ";
            //else if (plan.Value % 7 == 2)
            //    text += "Foot Massage (Inside) ";
            //else if (plan.Value % 7 == 3)
            //    text += "Thai Massage ";
            //else if (plan.Value % 7 == 4)
            //    text += "Back, Head & Shoulder Massage ";
            //else if (plan.Value % 7 == 5)
            //    text += "Oil Massage ";
            //else if (plan.Value % 7 == 6)
            //    text += "Aroma Massage ";
            //else
            //    text += "[Package]\n ";
            text += " "+plan.Text;
            return text;
        }

        public string getPlanNameForDisplay(PlanMassage plan)
        {
            string text = "-";
            if (plan.Type == 8)
            {
                text += "[Add] ";
                if (plan.Value == 29)
                {
                    text += "Foot Massage (OutSide)";
                }
                else if (plan.Value == 30)
                {
                    text += "Foot Massage (Inside)";
                }
                else if (plan.Value == 31)
                {
                    text += "Thai Massage";
                }
                else if (plan.Value == 32)
                {
                    text += "Back, Head & Shoulder Massage";
                }
                else if (plan.Value == 33)
                {
                    text += "Oil Massage";
                }
                else if (plan.Value == 34)
                {
                    text += "Aroma Oil Massage";
                }
            }
            else if (plan.Type == 1)
            {
                text += "Foot Massage (Outside)";
            }
            else if (plan.Type == 2)
            {
                text += "Foot Massage (Inside)";
            }
            else if (plan.Type == 3)
            {
                text += "Thai Massage";
            }
            else if (plan.Type == 4)
            {
                text += "Back, Head & Shoulder Massage";
            }
            else if (plan.Type == 5)
            {
                text += "Oil Massage";
            }
            else if (plan.Type == 6)
            {
                text += "Aroma Massage";
            }
            else
            {
                text += "[Package]\n";
            }
            //if (plan.Value >= 29 && plan.Value <= 34)
            //{
            //    text += "[เพิ่ม]";
            //}
            //if (plan.Value % 7 == 1)
            //    text += "Foot Massage (Outside) ";
            //else if (plan.Value % 7 == 2)
            //    text += "Foot Massage (Inside) ";
            //else if (plan.Value % 7 == 3)
            //    text += "Thai Massage ";
            //else if (plan.Value % 7 == 4)
            //    text += "Back, Head & Shoulder Massage ";
            //else if (plan.Value % 7 == 5)
            //    text += "Oil Massage ";
            //else if (plan.Value % 7 == 6)
            //    text += "Aroma Massage ";
            //else
            //    text += "[Package]\n ";
            text += " " + plan.Text;
            return text;
        }

        public string getCommissionSlip()
        {
            string text = "";
            //List<string> slipList = new List<string>();

            foreach(PlanMassage p in this.Plans)
            {
                text += this.getPlanName(p) + "\n";
                //slipList.Add(this.getPlanName(p));
            }

            return text;
        }

        public List<string> getCommissionListSlip()
        {
            List<string> slipList = new List<string>();

            foreach (PlanMassage p in this.Plans)
            {
                slipList.Add(this.getPlanName(p));
            }

            return slipList;
        }

        public int getCommission()
        {
            int commission = 0;
            foreach (PlanMassage p in this.Plans)
            {
                commission += p.Commission;
            }
            return commission;
        }

        public List<int> getCommissionAmountList()
        {
            List<int> comAmountList = new List<int>();
            foreach (PlanMassage p in this.Plans)
            {
                comAmountList.Add(p.Commission);
                //commission += p.Commission;
            }
            return comAmountList;
        }

        public string getInvoice()
        {
            string text = "";
            foreach (PlanMassage p in this.Plans)
            {
                text += this.getPlanName(p) + "\n  " + p.Price + " Baht\n";
            }
            return text;
        }

        public string getInvoiceForBigPrinter()
        {
            string text = "";
            foreach (PlanMassage p in this.Plans)
            {
                text += "    " + this.getPlanName(p) + "\n     " + p.Price + " Baht\n\n";
            }
            return text;
        }

        public int getTotalInvoice()
        {
            int total = 0;
            foreach (PlanMassage p in this.Plans)
            {
                total += p.Price;
            }
            return total;
        }

        public string getDateStamp()
        {
            return this.DateStamp;
        }

        public void debugSlip()
        {
            Debug.WriteLine("-------Slip-------");
            foreach(PlanMassage p in Plans)
            {
                Debug.WriteLine(this.getPlanName(p));
            }
            Debug.WriteLine("-------------------");
        }
    }
}
