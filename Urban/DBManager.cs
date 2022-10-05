using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urban.Model;

namespace Urban
{
    public class DBManager
    {
        SQLiteConnection db;
        string dbname = @"C:\SpaSystem\Release\spasystemdb0.db";
        //string dateTimeFormat = "dd/MM/yyyy";
        public DBManager()
        {
            db = new SQLiteConnection(dbname);
        }

        public List<Account> getAllAccount()
        {
            List<Account> Accounts;
            using (var db = new SQLiteConnection(dbname))
            {
                Accounts = db.Table<Account>().ToList();
            }
            return Accounts;
        }

        public List<Account> getAccountLast40Records()
        {
            List<Account> Accounts;
            using (var db = new SQLiteConnection(dbname))
            {
                Accounts = db.Table<Account>().OrderByDescending(b => b.Id).Take(40).ToList();
            }
            return Accounts;
        }

        public List<Account> getAllUnSendAccount()
        {
            List<Account> Accounts;
            using (var db = new SQLiteConnection(dbname))
            {
                Accounts = db.Table<Account>().Where(b => b.SendStatus == "false").ToList();
            }
            return Accounts;
        }

        public void updateAcount(Account account)
        {
            using (var db = new SQLiteConnection(dbname))
            {
                db.Update(account);
            }
        }


        public Account getAccountFromId(int Id)
        {
            Account acct = new Account();
            using (var db = new SQLiteConnection(dbname))
            {
                acct = db.Table<Account>().Where(b => b.Id == Id).FirstOrDefault();
            }

            return acct;
        }

        public string getStaffNumberFromAccountId(int Id)
        {
            Account acStaffNo = new Account();
            using (var db = new SQLiteConnection(dbname))
            {
                acStaffNo = db.Table<Account>().Where(b => b.Id == Id).FirstOrDefault();
            }

            return acStaffNo.StaffAmount;
        }

        public Account getLatestAcount()
        {
            Account account = new Account();
            using (var db = new SQLiteConnection(dbname))
            {
                account = db.Table<Account>().Last();
            }
            return account;
        }

        public List<Branch> getAllBranch()
        {
            List<Branch> Branches;
            using (var db = new SQLiteConnection(dbname))
            {
                Branches = db.Table<Branch>().ToList();
            }
            return Branches;
        }

        public Branch getBranch()
        {
            Branch brch = new Branch();
            using (var db = new SQLiteConnection(dbname))
            {
                brch = db.Table<Branch>().Last();
            }
            return brch;
        }

        public void updateBranch(Branch branch)
        {
            using (var db = new SQLiteConnection(dbname))
            {
                db.Update(branch);
            }
        }

        public ProgramVersion getVersion()
        {
            ProgramVersion ver = new ProgramVersion();
            using (var db = new SQLiteConnection(dbname))
            {
                ver = db.Table<ProgramVersion>().Last();
            }
            return ver;
        }

        public void updateVersion(ProgramVersion versions)
        {
            using (var db = new SQLiteConnection(dbname))
            {
                db.Update(versions);
            }
        }

        public void checkIn(Account account)
        {
            using (var db = new SQLiteConnection(dbname))
            {
                db.Insert(account);

            }
        }

        public void InsertMassageTopic(MassageTopic msgTopic)
        {
            using (var db = new SQLiteConnection(dbname))
            {
                db.Insert(msgTopic);

            }
        }

        public void InsertMassagePlan(MassagePlan msgPlan)
        {
            using (var db = new SQLiteConnection(dbname))
            {
                db.Insert(msgPlan);

            }
        }

        public void InsertMassageSet(MassageSet msgSet)
        {
            using (var db = new SQLiteConnection(dbname))
            {
                db.Insert(msgSet);

            }
        }

        public void saveOrder(OrderRecord order)
        {
            using (var db = new SQLiteConnection(dbname))
            {
                db.Insert(order);

            }
        }

        public List<OrderRecord> getAllOrderRecord(int AccountId)
        {
            List<OrderRecord> OrderRecords;
            using (var db = new SQLiteConnection(dbname))
            {
                OrderRecords = db.Table<OrderRecord>().Where(b => b.AccountId == AccountId).ToList();
            }
            return OrderRecords;
        }

        public List<OrderRecord> getAllOrderRecordExceptCancelled(int AccountId)
        {
            List<OrderRecord> OrderRecords;
            using (var db = new SQLiteConnection(dbname))
            {
                OrderRecords = db.Table<OrderRecord>().Where(b => b.AccountId == AccountId && b.CancelStatus == "false").ToList();
            }
            return OrderRecords;
        }

        public List<OrderRecord> getAllOrderCashRecordExceptCancelled(int AccountId)
        {
            List<OrderRecord> OrderRecords;
            using (var db = new SQLiteConnection(dbname))
            {
                OrderRecords = db.Table<OrderRecord>().Where(b => b.AccountId == AccountId && b.IsCreditCard != "true" && b.CancelStatus == "false").ToList();
            }
            return OrderRecords;
        }

        public List<OrderRecord> getAllOrderCreditRecordExceptCancelled(int AccountId)
        {
            List<OrderRecord> OrderRecords;
            using (var db = new SQLiteConnection(dbname))
            {
                OrderRecords = db.Table<OrderRecord>().Where(b => b.AccountId == AccountId && b.IsCreditCard == "true" && b.CancelStatus == "false").ToList();
            }
            return OrderRecords;
        }

        public List<OrderRecord> getAllOrderRecordExceptCancelled25(int AccountId)
        {
            List<OrderRecord> OrderRecords = this.getAllOrderRecordExceptCancelled(AccountId);
            List<OrderRecord> OrderRecords25;
            int finalNum = OrderRecords.Count() * 25 / 100;
            using (var db = new SQLiteConnection(dbname))
            {
                OrderRecords25 = db.Table<OrderRecord>().Where(b => b.AccountId == AccountId && b.CancelStatus == "false").Take(finalNum).ToList();

                //db.Table<Account>().OrderByDescending(b => b.Id).Take(40).ToList();
            }
            return OrderRecords25;
        }

        public List<OrderRecord> getAllCancelledOrderRecord(int AccountId)
        {
            List<OrderRecord> OrderRecords;
            using (var db = new SQLiteConnection(dbname))
            {
                OrderRecords = db.Table<OrderRecord>().Where(b => b.AccountId == AccountId && b.CancelStatus == "true").ToList();
            }
            return OrderRecords;
        }

        public List<OrderRecord> getAllUnSendOrderRecord(int AccountId)
        {
            List<OrderRecord> OrderRecords;
            using (var db = new SQLiteConnection(dbname))
            {
                OrderRecords = db.Table<OrderRecord>().Where(b => b.AccountId == AccountId && b.SendStatus == "false").ToList();
            }
            return OrderRecords;
        }

        public OrderRecord getOrderRecordtFromIdAndAccountId(int Id, int AccountId)
        {
            OrderRecord ordrc = new OrderRecord();
            using (var db = new SQLiteConnection(dbname))
            {
                ordrc = db.Table<OrderRecord>().Where(b => b.Id == Id && b.AccountId == AccountId).FirstOrDefault();
            }

            return ordrc;
        }

        public void updateOrderRecord(OrderRecord orderRecord)
        {
            using (var db = new SQLiteConnection(dbname))
            {
                db.Update(orderRecord);
            }
        }

        public List<MassageSet> getAllMassageSet()
        {
            List<MassageSet> MassageSets;
            using (var db = new SQLiteConnection(dbname))
            {
                MassageSets = db.Table<MassageSet>().ToList();
            }
            return MassageSets;
        }

        public List<MassageTopic> getAllMassageTopic(List<int> topicId)
        {
            List<MassageTopic> listMsgTopics = new List<MassageTopic>();
            MassageTopic MassageTopics;
            using (var db = new SQLiteConnection(dbname))
            {
                for(int i=0;i<topicId.Count();i++)
                {
                    int a = topicId[i];
                    MassageTopics = db.Table<MassageTopic>().Where(b => b.Id == a).FirstOrDefault();
                    //MassageTopics = db.Table<MassageTopic>().Where(b => b.Id == topicId[i]).FirstOrDefault();
                    listMsgTopics.Add(MassageTopics);
                }
                
            }
            return listMsgTopics;
        }

        public List<MassageSet> getMassagePlanFromTopic(int id)
        {
            List<MassageSet> msgSets;
            using (var db = new SQLiteConnection(dbname))
            {
                msgSets = db.Table<MassageSet>().Where(b => b.MassageTopicId == id).ToList();
            }
            if (msgSets == null)
            {
                return null;
            }
            else
            {
                return msgSets;
            }
        }


        public string getMassageTopicName(int id)
        {
            MassageTopic msgTopic = new MassageTopic();
            using (var db = new SQLiteConnection(dbname))
            {
                msgTopic = db.Table<MassageTopic>().Where(b => b.Id == id).FirstOrDefault();
            }
            if (msgTopic == null)
            {
                return null;
            }
            else
            {
                return msgTopic.Name;
            }
        }

        public List<ShortMassageTopic> getMassageTopicShortSet(List<int> listDupTopic)
        {
            List<ShortMassageTopic> dupMassageTopicSet = new List<ShortMassageTopic>();
            for(int a=0;a<listDupTopic.Count();a++)
            {
                dupMassageTopicSet.Add(new ShortMassageTopic { Id = listDupTopic[a], Name = getMassageTopicName(listDupTopic[a]) });
            }

            return dupMassageTopicSet;
        }

        public string getMassagePlanName(int id)
        {
            MassagePlan msgPlan = new MassagePlan();
            using (var db = new SQLiteConnection(dbname))
            {
                msgPlan = db.Table<MassagePlan>().Where(b => b.Id == id).FirstOrDefault();
            }
            if (msgPlan == null)
            {
                return null;
            }
            else
            {
                return msgPlan.Name;
            }
        }


        public void clearData()
        {
            using (var db = new SQLiteConnection(dbname))
            {
                db.DeleteAll<MassagePlan>();
                db.DeleteAll<MassageTopic>();
                db.DeleteAll<MassageSet>();
                db.DeleteAll<OtherSale>();
                db.DeleteAll<DiscountMaster>();
                db.DeleteAll<DiscountMasterDetail>();
            }
        }

        public void clearSettingSeq()
        {
            using (var db = new SQLiteConnection(dbname))
            {
                //db.re<sqlite_sequence>().Where(b => b.name == "Setting");
            }
        }

        public List<OtherSale> getAllOtherSaleList()
        {
            List<OtherSale> OtherSales;
            using (var db = new SQLiteConnection(dbname))
            {
                OtherSales = db.Table<OtherSale>().Where(b => b.Status == "true").ToList();
            }
            return OtherSales;
        }

        public void saveOtherSaleOrder(OtherSaleRecord otherSaleOrder)
        {
            using (var db = new SQLiteConnection(dbname))
            {
                db.Insert(otherSaleOrder);

            }
        }

        public string getOtherSaleNameFromId(int Id)
        {
            OtherSale ots = new OtherSale();
            using (var db = new SQLiteConnection(dbname))
            {
                ots = db.Table<OtherSale>().Where(b => b.Id == Id).FirstOrDefault();
            }

            return ots.Name;
        }
        public List<OtherSaleRecord> getGrandOtherSaleRecordExceptCancelled(int AccountId)
        {
            List<OtherSaleRecord> grandRecords;
            using (var db = new SQLiteConnection(dbname))
            {
                grandRecords = db.Table<OtherSaleRecord>().Where(b => b.AccountId == AccountId && b.CancelStatus == "false").ToList();
            }
            return grandRecords;
        }


        public List<OtherSaleRecord> getAllUniformRecordExceptCancelled(int AccountId)
        {
            List<OtherSaleRecord> UniformRecords;
            using (var db = new SQLiteConnection(dbname))
            {
                UniformRecords = db.Table<OtherSaleRecord>().Where(b => b.AccountId == AccountId && b.OtherSaleId == 1 && b.CancelStatus == "false").ToList();
            }
            return UniformRecords;
        }

        public List<OtherSaleRecord> getAllSmallTigerBalmRecordExceptCancelled(int AccountId)
        {
            List<OtherSaleRecord> TigerBalmRecords;
            using (var db = new SQLiteConnection(dbname))
            {
                TigerBalmRecords = db.Table<OtherSaleRecord>().Where(b => b.AccountId == AccountId && b.OtherSaleId == 4 && b.CancelStatus == "false").ToList(); //edit on 3 Nov 2019
            }
            return TigerBalmRecords;
        }

        public List<OtherSaleRecord> getAllBigTigerBalmRecordExceptCancelled(int AccountId) //edit on 3 Nov 2019
        {
            List<OtherSaleRecord> TigerBalmRecords;
            using (var db = new SQLiteConnection(dbname))
            {
                TigerBalmRecords = db.Table<OtherSaleRecord>().Where(b => b.AccountId == AccountId && b.OtherSaleId == 5 && b.CancelStatus == "false").ToList();
            }
            return TigerBalmRecords;
        }

        public List<OtherSaleRecord> getAllOtherSaleRecordExceptCancelled(int AccountId)
        {
            List<OtherSaleRecord> OtherSaleRecords;
            using (var db = new SQLiteConnection(dbname))
            {
                OtherSaleRecords = db.Table<OtherSaleRecord>().Where(b => b.AccountId == AccountId && b.OtherSaleId == 3 && b.CancelStatus == "false").ToList();
            }
            return OtherSaleRecords;
        }

        public void updateOtherSaleRecord(OtherSaleRecord otherSaleRecords)
        {
            using (var db = new SQLiteConnection(dbname))
            {
                db.Update(otherSaleRecords);
            }
        }

        public List<OtherSaleRecord> getAllUnSendOtherSaleRecord(int AccountId)
        {
            List<OtherSaleRecord> OtherSaleRecords;
            using (var db = new SQLiteConnection(dbname))
            {
                OtherSaleRecords = db.Table<OtherSaleRecord>().Where(b => b.AccountId == AccountId && b.SendStatus == "false").ToList();
            }
            return OtherSaleRecords;
        }

        //Voucher
        public List<DiscountMasterDetail> getAllVoucherList()
        {
            List<DiscountMasterDetail> voucherList;
            using (var db = new SQLiteConnection(dbname))
            {
                voucherList = db.Table<DiscountMasterDetail>().Where(b => b.DiscountMasterId == 1 && b.Status == "true").ToList();
            }
            return voucherList;
        }

        public void saveDiscountRecord(DiscountRecord discountRecord)
        {
            using (var db = new SQLiteConnection(dbname))
            {
                db.Insert(discountRecord);

            }
        }

        public void updateDiscountRecord(DiscountRecord discountRecords)
        {
            using (var db = new SQLiteConnection(dbname))
            {
                db.Update(discountRecords);
            }
        }

        public List<DiscountRecord> getAllUnSendDiscountRecord(int AccountId)
        {
            List<DiscountRecord> discountRecords;
            using (var db = new SQLiteConnection(dbname))
            {
                discountRecords = db.Table<DiscountRecord>().Where(b => b.AccountId == AccountId && b.SendStatus == "false").ToList();
            }
            return discountRecords;
        }

        public DiscountMasterDetail getDiscountMasterDetailFromId(int Id)
        {
            DiscountMasterDetail dmd = new DiscountMasterDetail();
            using (var db = new SQLiteConnection(dbname))
            {
                dmd = db.Table<DiscountMasterDetail>().Where(b => b.Id == Id).FirstOrDefault();
            }

            return dmd;
        }

        public int getAllDiscountWithCashFromAccountID(int AccountId)
        {
            int discountWithCash = 0;
            List<DiscountRecord> discountRec;

            using (var db = new SQLiteConnection(dbname))
            {
                discountRec = db.Table<DiscountRecord>().Where(b => b.AccountId == AccountId && b.IsCreditCard == "false" && b.CancelStatus == "false").ToList();
            }

            foreach(DiscountRecord dr in discountRec)
            {
                discountWithCash += Int32.Parse(dr.Value);
            }

            return discountWithCash;
        }

        public int getAllDiscountWithCreditFromAccountID(int AccountId)
        {
            int discountWithCredit = 0;
            List<DiscountRecord> discountRec;

            using (var db = new SQLiteConnection(dbname))
            {
                discountRec = db.Table<DiscountRecord>().Where(b => b.AccountId == AccountId && b.IsCreditCard == "true" && b.CancelStatus == "false").ToList();
            }

            foreach (DiscountRecord dr in discountRec)
            {
                discountWithCredit += Int32.Parse(dr.Value);
            }

            return discountWithCredit;
        }

        //OrderRecord x Voucher
        public void saveOrderRecordWithDiscount(OrderRecordWithDiscount orderWithDiscount)
        {
            using (var db = new SQLiteConnection(dbname))
            {
                db.Insert(orderWithDiscount);

            }
        }

        public void updateOrderRecordWithDiscount(OrderRecordWithDiscount orderRecordWithDiscounts)
        {
            using (var db = new SQLiteConnection(dbname))
            {
                db.Update(orderRecordWithDiscounts);
            }
        }

        public List<OrderRecordWithDiscount> getAllUnSendOrderRecordWithDiscount(int AccountId)
        {
            List<OrderRecordWithDiscount> orderRecordWithDiscounts;
            using (var db = new SQLiteConnection(dbname))
            {
                orderRecordWithDiscounts = db.Table<OrderRecordWithDiscount>().Where(b => b.AccountId == AccountId && b.SendStatus == "false").ToList();
            }
            return orderRecordWithDiscounts;
        }

        public sqlite_sequence getSettingSeq()
        {
            sqlite_sequence seq = new sqlite_sequence();
            using (var db = new SQLiteConnection(dbname))
            {
                seq = db.Table<sqlite_sequence>().Where(b => b.name == "Setting").FirstOrDefault();
            }
            return seq;
        }

        public void updateSettingSeq(sqlite_sequence seq)
        {
            using (var db = new SQLiteConnection(dbname))
            {
                db.Update(seq);
            }
        }

        public Setting getCurrentPassword()
        {
            Setting pw = new Setting();
            using (var db = new SQLiteConnection(dbname))
            {
                pw = db.Table<Setting>().Where(b => b.Name == "Password").FirstOrDefault();
            }
            return pw;
        }

        public Setting getCurrentEmailServer()
        {
            Setting conFig = new Setting();
            using (var db = new SQLiteConnection(dbname))
            {
                conFig = db.Table<Setting>().Where(b => b.Name == "EmailServer").FirstOrDefault();
            }
            return conFig;
        }

        public Setting getCurrentSenderEmail()
        {
            Setting conFig = new Setting();
            using (var db = new SQLiteConnection(dbname))
            {
                conFig = db.Table<Setting>().Where(b => b.Name == "SenderEmail").FirstOrDefault();
            }
            return conFig;
        }

        public Setting getCurrentReceiverEmail()
        {
            Setting conFig = new Setting();
            using (var db = new SQLiteConnection(dbname))
            {
                conFig = db.Table<Setting>().Where(b => b.Name == "ReceiverEmail").FirstOrDefault();
            }
            return conFig;
        }

        public Setting getCurrentServerUsername()
        {
            Setting conFig = new Setting();
            using (var db = new SQLiteConnection(dbname))
            {
                conFig = db.Table<Setting>().Where(b => b.Name == "ServerUsername").FirstOrDefault();
            }
            return conFig;
        }

        public Setting getCurrentServerPassword()
        {
            Setting conFig = new Setting();
            using (var db = new SQLiteConnection(dbname))
            {
                conFig = db.Table<Setting>().Where(b => b.Name == "ServerPassword").FirstOrDefault();
            }
            return conFig;
        }

        public Setting getCurrentMonitorComPort()
        {
            Setting conFig = new Setting();
            using (var db = new SQLiteConnection(dbname))
            {
                conFig = db.Table<Setting>().Where(b => b.Name == "MonitorComPort").FirstOrDefault();
            }
            return conFig;
        }

        public Setting getCurrentMonitorBaudRate()
        {
            Setting conFig = new Setting();
            using (var db = new SQLiteConnection(dbname))
            {
                conFig = db.Table<Setting>().Where(b => b.Name == "MonitorBaudRate").FirstOrDefault();
            }
            return conFig;
        }

        public Setting getCurrentReceiptPrinter()
        {
            Setting conFig = new Setting();
            using (var db = new SQLiteConnection(dbname))
            {
                conFig = db.Table<Setting>().Where(b => b.Name == "ReceiptPrinter").FirstOrDefault();
            }
            return conFig;
        }

        public Setting getCurrentCommissionPrinter()
        {
            Setting conFig = new Setting();
            using (var db = new SQLiteConnection(dbname))
            {
                conFig = db.Table<Setting>().Where(b => b.Name == "CommissionPrinter").FirstOrDefault();
            }
            return conFig;
        }

        public Setting getCurrentOilPrice()
        {
            Setting conFig = new Setting();
            using (var db = new SQLiteConnection(dbname))
            {
                conFig = db.Table<Setting>().Where(b => b.Name == "OilPrice").FirstOrDefault();
            }
            return conFig;
        }

        public Setting getCurrentServerPort()
        {
            Setting conFig = new Setting();
            using (var db = new SQLiteConnection(dbname))
            {
                conFig = db.Table<Setting>().Where(b => b.Name == "ServerPort").FirstOrDefault();
            }
            return conFig;
        }

        public Setting getCurrentBranchNameInMonitor()
        {
            Setting conFig = new Setting();
            using (var db = new SQLiteConnection(dbname))
            {
                conFig = db.Table<Setting>().Where(b => b.Name == "BranchNameInMonitor").FirstOrDefault();
            }
            return conFig;
        }

        public Setting getCurrentReport100Status()
        {
            Setting conFig = new Setting();
            using (var db = new SQLiteConnection(dbname))
            {
                conFig = db.Table<Setting>().Where(b => b.Name == "Report100").FirstOrDefault();
            }
            return conFig;
        }

        public Setting getCurrentReport25Status()
        {
            Setting conFig = new Setting();
            using (var db = new SQLiteConnection(dbname))
            {
                conFig = db.Table<Setting>().Where(b => b.Name == "Report25").FirstOrDefault();
            }
            return conFig;
        }

        public Setting getCurrentReportDetailStatus()
        {
            Setting conFig = new Setting();
            using (var db = new SQLiteConnection(dbname))
            {
                conFig = db.Table<Setting>().Where(b => b.Name == "ReportDetail").FirstOrDefault();
            }
            return conFig;
        }

        //public Setting getCurrentSystemVersion()
        //{
        //    Setting conFig = new Setting();
        //    using (var db = new SQLiteConnection(dbname))
        //    {
        //        conFig = db.Table<Setting>().Where(b => b.Name == "SystemVersion").FirstOrDefault();
        //    }
        //    return conFig;
        //}

        public void clearAllSetting()
        {
            using (var db = new SQLiteConnection(dbname))
            {
                db.DeleteAll<Setting>();
            }
        }

        public void InsertSystemSetting(Setting systemSetting)
        {
            using (var db = new SQLiteConnection(dbname))
            {
                db.Insert(systemSetting);

            }
        }

        public void InsertOtherSaleMaster(OtherSale otherSaleMaster)
        {
            using (var db = new SQLiteConnection(dbname))
            {
                db.Insert(otherSaleMaster);

            }
        }

        public void InsertDiscountMaster(DiscountMaster discountMaster)
        {
            using (var db = new SQLiteConnection(dbname))
            {
                db.Insert(discountMaster);

            }
        }

        public void InsertDiscountMasterDetail(DiscountMasterDetail discountMasterDetail)
        {
            using (var db = new SQLiteConnection(dbname))
            {
                db.Insert(discountMasterDetail);

            }
        }

        //public string getCurrentPasswordVal()
        //{
        //    Setting pw = new Setting();
        //    using (var db = new SQLiteConnection(dbname))
        //    {
        //        pw = db.Table<Setting>().Where(b => b.Name == "Password").FirstOrDefault();
        //    }
        //    return pw.Value;
        //}


    }
}
