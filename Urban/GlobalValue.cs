using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Urban
{
    public class GlobalValue
    {
        private static GlobalValue _mInstance;

        public static GlobalValue Instance
        {
            get { return _mInstance ?? (_mInstance = new GlobalValue()); }
        }

        //public string _comport { get; set; }

        //Master Data
        public string Json_MasterData { get; set; }
        public string branchNameInMonitor { get; set; }
        public int CurrentSystemVersion { get; set; }
        public string downloadUpdateURL { get; set; }

        //Url
        public string Url_GetBranchData = "http://spabackoffice2019.azurewebsites.net/API/GetBranchData";
        public string Url_VerifyMember = "http://spabackoffice2019.azurewebsites.net/API/VerifyMember";
        public string Url_SendData = "http://spabackoffice2019.azurewebsites.net/API/SendData";
        public string Url_SendOtherSaleData = "http://spabackoffice2019.azurewebsites.net/API/SendOtherSaleData";
        public string Url_SendDiscountData = "http://spabackoffice2019.azurewebsites.net/API/SendDiscountData";
        public string Url_SendOrderRecordWithDiscountData = "http://spabackoffice2019.azurewebsites.net/API/SendOrderRecordWithDiscountData";
        public string Url_UpdateAccount = "http://spabackoffice2019.azurewebsites.net/API/UpdateAccount";
        public string Url_UpdateOrderRecord = "http://spabackoffice2019.azurewebsites.net/API/UpdateOrderRecord";
        public string Url_SendReeipt = "http://spabackoffice2019.azurewebsites.net/API/SendReceiptData";
        public string Url_SendOrderReceipt = "http://spabackoffice2019.azurewebsites.net/API/SendOrderReceiptData";
        public string Url_UpdateOrderReceipt = "http://spabackoffice2019.azurewebsites.net/API/UpdateOrderReceipt";

        //Test Url
        //public string Url_GetBranchData = "http://localhost:49393/API/GetBranchData";
        //public string Url_SendData = "http://localhost:49393/API/SendData";
        //public string Url_UpdateAccount = "http://localhost:49393/API/UpdateAccount";
        //public string Url_UpdateOrderRecord = "http://localhost:49393/API/UpdateOrderRecord";

        //Email Setup
        public string emailServer { get; set; }
        public string senderEmail { get; set; }
        public int serverPort { get; set; }
        public string serverUsername { get; set; }
        public string serverPassword { get; set; }
        //public string emailServer = "smtp.sendgrid.net";
        //public string senderEmail = "spasystem.khaosan.report@gmail.com";
        //public int serverPort = 587;
        ////public string serverUsername = "azure_2edff7cedcf6d7c5ca829dc6a2f1ab0a@azure.com"; -- From jaturong@24dvlop.com Azure
        //public string serverUsername = "azure_fcc0e507ac20d187cfc9066391d706a9@azure.com";
        //public string serverPassword = "abcd1234!";

        //Monitor Setup
        public string monitorComPort { get; set; }
        public int monitorBaudRate { get; set; }
        public string receiptPrinter { get; set; }
        public string commissionPrinter { get; set; }
        //public string monitorComPort = "COM5";
        //public string receiptPrinter = "RCPT";
        //public string commissionPrinter = "CMS";

        //Oil setting
        public int oilPrice { get; set; }
        //public int oilPrice = 20;

        //Report setting
        public string report100 { get; set; }
        public string report25 { get; set; }
        public string reportDetail { get; set; }
        public Model.MemberDetail usingMember { get; set; }
        public Model.Receipt latestReceipt { get; set; }

        //System config
        public string MobileQrEnable { get; set; }
        public string VIPCardEnable { get; set; }

        //Etc
        public int TargetOrderReceiptId { get; set; }
    }
}
