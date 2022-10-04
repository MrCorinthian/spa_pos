using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace Urban.Model
{/*
    public class Massage
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [NotNull]
        public int Type { get; set; }
        [NotNull]
        public int Price { get; set; }
        [NotNull]
        public int Commission { get; set; }
        public string Duration { get; set; }
        public int PackageId { get; set; }
    }

    public class Type
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [NotNull]
        public string Name { get; set; }
    }

    public class Package
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [NotNull]
        public string Name { get; set; }
    }

    public class PackageMassage
    {
        [PrimaryKey,NotNull]
        public int PakageId { get; set; }
        [NotNull]
        public int MassageId { get; set; }
    }

    public class Order
    {
        [PrimaryKey]
        public int Id { get; set; }
        [NotNull]
        public DateTime DateStamp { get; set; }
        public DateTime TimeStamp { get; set; }
        [NotNull]
        public bool Status { get; set; }
        [NotNull]
        public int BranchId { get; set; }
        [NotNull]
        public int AccountId { get; set; }
    }

    public class OrderMassage
    {
        //[PrimaryKey]
        //public int Id { get; set; }
        [PrimaryKey,NotNull,AutoIncrement]
        public int Id { get; set; }
        [NotNull]
        public int OrderId { get; set; }
        [NotNull]
        public int MassageId { get; set; }
        [NotNull]
        public DateTime CreateDateTime { get; set; }
        public DateTime UpdateDateTime { get; set; }
        [NotNull]
        public bool Status { get; set; }
        [NotNull]
        public int BranchId { get; set; }
    }
    */
    public class Account
    {
        [PrimaryKey, NotNull, AutoIncrement]
        public int Id { get; set; }
        [NotNull]
        public string Date { get; set; }
        [NotNull]
        public string Time { get; set; }
        [NotNull]
        public string StartMoney { get; set; }
        [NotNull]
        public string StaffAmount { get; set; }
        [NotNull]
        public string Completed { get; set; }
        [NotNull]
        public string SendStatus { get; set; }
        [NotNull]
        public string UpdateStatus { get; set; }
        public string CreateDateTime { get; set; }
        public string UpdateDateTime { get; set; }
    }

    public class Branch
    {
        [PrimaryKey, NotNull]
        public int Id { get; set; }
        [NotNull]
        public string Name { get; set; }
        [NotNull]
        public int MassageSetId { get; set; }
        public string CreateDateTime { get; set; }
        public string UpdateDateTime { get; set; }
    }

    public class MassageTopic
    {
        [PrimaryKey, NotNull]
        public int Id { get; set; }
        [NotNull]
        public string Name { get; set; }
        public string HeaderColor { get; set; }
        public string ChildColor { get; set; }
        public string CreateDateTime { get; set; }
    }

    public class MassagePlan
    {
        [PrimaryKey, NotNull]
        public int Id { get; set; }
        [NotNull]
        public string Name { get; set; }
        public string CreateDateTime { get; set; }
    }

    public class MassageSet
    {
        [NotNull]
        public int Id { get; set; }
        [NotNull]
        public int MassageTopicId { get; set; }
        [NotNull]
        public int MassagePlanId { get; set; }
        [NotNull]
        public string Price { get; set; }
        [NotNull]
        public string Commission { get; set; }
        public string CreateDateTime { get; set; }
    }

    public class OrderRecord
    {
        [PrimaryKey, NotNull, AutoIncrement]
        public int Id { get; set; }
        [NotNull]
        public int AccountId { get; set; }
        [NotNull]
        public string Date { get; set; }
        [NotNull]
        public string Time { get; set; }
        [NotNull]
        public int MassageTopicId { get; set; }
        [NotNull]
        public int MassagePlanId { get; set; }
        [NotNull]
        public string Price { get; set; }
        [NotNull]
        public string Commission { get; set; }
        public string IsCreditCard { get; set; }
        [NotNull]
        public string SendStatus { get; set; }
        [NotNull]
        public string CancelStatus { get; set; }
        public string CreateDateTime { get; set; }
        public string UpdateDateTime { get; set; }
    }

    public class OtherSale
    {
        [PrimaryKey, NotNull]
        public int Id { get; set; }
        [NotNull]
        public string Name { get; set; }
        [NotNull]
        public string Price { get; set; }
        [NotNull]
        public string Status { get; set; }
        public string CreateDateTime { get; set; }
    }

    public class OtherSaleRecord
    {
        [PrimaryKey, NotNull, AutoIncrement]
        public int Id { get; set; }
        [NotNull]
        public int AccountId { get; set; }
        [NotNull]
        public string Date { get; set; }
        [NotNull]
        public string Time { get; set; }
        [NotNull]
        public int OtherSaleId { get; set; }
        [NotNull]
        public string Price { get; set; }
        public string IsCreditCard { get; set; }
        [NotNull]
        public string SendStatus { get; set; }
        [NotNull]
        public string CancelStatus { get; set; }
        public string CreateDateTime { get; set; }
        public string UpdateDateTime { get; set; }
    }

    public class DiscountMaster
    {
        [PrimaryKey, NotNull]
        public int Id { get; set; }
        [NotNull]
        public string Name { get; set; }
        [NotNull]
        public string Status { get; set; }
        public string CreateDateTime { get; set; }
        public string UpdateDateTime { get; set; }
    }

    public class DiscountMasterDetail
    {
        [PrimaryKey, NotNull]
        public int Id { get; set; }
        [NotNull]
        public int DiscountMasterId { get; set; }
        [NotNull]
        public string Name { get; set; }
        [NotNull]
        public string Value { get; set; }
        [NotNull]
        public string Status { get; set; }
        public string CreateDateTime { get; set; }
        public string UpdateDateTime { get; set; }
    }

    public class DiscountRecord
    {
        [PrimaryKey, NotNull, AutoIncrement]
        public int Id { get; set; }
        [NotNull]
        public int AccountId { get; set; }
        [NotNull]
        public string Date { get; set; }
        [NotNull]
        public string Time { get; set; }
        [NotNull]
        public int DiscountMasterId { get; set; }
        [NotNull]
        public int DiscountMasterDetailId { get; set; }
        [NotNull]
        public string Value { get; set; }
        [NotNull]
        public string IsCreditCard { get; set; }
        [NotNull]
        public string SendStatus { get; set; }
        [NotNull]
        public string CancelStatus { get; set; }
        public string CreateDateTime { get; set; }
        public string UpdateDateTime { get; set; }
    }

    public class OrderRecordWithDiscount
    {
        [PrimaryKey, NotNull, AutoIncrement]
        public int Id { get; set; }
        [NotNull]
        public int AccountId { get; set; }
        [NotNull]
        public int OrderRecordId { get; set; }
        [NotNull]
        public int DiscountRecordId { get; set; }
        [NotNull]
        public string SendStatus { get; set; }
        public string CreateDateTime { get; set; }
        public string UpdateDateTime { get; set; }
    }

    public class Setting
    {
        [PrimaryKey, NotNull, AutoIncrement]
        public int Id { get; set; }
        [NotNull]
        public string Name { get; set; }
        public string Value { get; set; }
        public int Version { get; set; }
        public string Status { get; set; }
        public string CreateDateTime { get; set; }
        public string UpdateDateTime { get; set; }
    }

    public class ProgramVersion
    {
        [PrimaryKey, NotNull]
        public int Id { get; set; }
        [NotNull]
        public int VersionNo { get; set; }
        public string CreateDateTime { get; set; }
        public string UpdateDateTime { get; set; }
    }

    public class sqlite_sequence
    {
        [PrimaryKey, NotNull]
        public string name { get; set; }
        public string seq { get; set; }
    }

}
