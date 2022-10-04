using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Urban.Model
{
    public class MonthConvertor
    {
        public string calMonth(string monthNum)
        {
            string monthName = "";
            if (monthNum.Equals("1") || monthNum.Equals("01"))
            {
                monthName = "January";
            }
            else if (monthNum.Equals("2") || monthNum.Equals("02"))
            {
                monthName = "February";
            }
            else if (monthNum.Equals("3") || monthNum.Equals("03"))
            {
                monthName = "March";
            }
            else if (monthNum.Equals("4") || monthNum.Equals("04"))
            {
                monthName = "April";
            }
            else if (monthNum.Equals("5") || monthNum.Equals("05"))
            {
                monthName = "May";
            }
            else if (monthNum.Equals("6") || monthNum.Equals("06"))
            {
                monthName = "June";
            }
            else if (monthNum.Equals("7") || monthNum.Equals("07"))
            {
                monthName = "July";
            }
            else if (monthNum.Equals("8") || monthNum.Equals("08"))
            {
                monthName = "August";
            }
            else if (monthNum.Equals("9") || monthNum.Equals("09"))
            {
                monthName = "September";
            }
            else if (monthNum.Equals("10") || monthNum.Equals("010"))
            {
                monthName = "October";
            }
            else if (monthNum.Equals("11") || monthNum.Equals("011"))
            {
                monthName = "November";
            }
            else if (monthNum.Equals("12") || monthNum.Equals("012"))
            {
                monthName = "December";
            }

            return monthName;
        }
    }
}
