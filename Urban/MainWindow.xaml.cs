using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Urban.Model;

namespace Urban
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DBManager db;

        int currentUseAccountId = 0;
        int currentStaffNo = 0;

        List<OrderRecord> prepareOrder;
        OtherSaleRecord centralOsr;
        List<DiscountRecord> prepareDiscount;
        int currentBalance = 0;
        int finalBalance = 0;

        string currentBranchName;
        string filename;
        string filename25;
        string filenameDetail25;

        SerialPort portaserial;

        bool soldPanelToggle = false;

        CancelRecordParam getCancelParams;

        List<MassageTopic> globalListMassageTopic;
        //List<MassagePlan> globalListMassagePlan;


        public MainWindow()
        {
            InitializeComponent();

            //MessageBox.Show(Application.Current.MainWindow.Height + "//" + Application.Current.MainWindow.Width);
            //Check resolution
            if(Application.Current.MainWindow.Height != 1080)
            {
                MainScrollview.Height = 636;
                MainScrollview.Width = 1556;
            }


            this.db = new DBManager();
            prepareOrder = new List<OrderRecord>();
            prepareDiscount = new List<DiscountRecord>();

            CheckDataSetVersion();
            InitialInterface();
            ClockRunning();
            //StartCheckSystemVersion();
            CheckIn();
            ClearText();
            /*
            List<Account> listAccount = this.db.getAllAccount();
            MessageBox.Show(listAccount.Count().ToString());

            Account ac = new Account()
            {
                Date = "15-10-2016",
                Time = "1.47",
                StartMoney = "6000",
                StaffAmount = "20",
                Completed = "true",
                SendStatus = "false",
                UpdateStatus = "false"
            };

            this.db.checkIn(ac);

            listAccount = this.db.getAllAccount();
            MessageBox.Show(listAccount.Count().ToString());
            */
            
            
        }

        public void CheckDataSetVersion()
        {
            // if current version is less
            // update version no to new
            // delete all massagetopic,massageplan,massageset
            // Insert all new record

            // if current version is equal
            // do not thing

            try
            {
                if (GlobalValue.Instance.Json_MasterData.Equals("error"))
                {
                    GlobalValue.Instance.emailServer = this.db.getCurrentEmailServer().Value;
                    GlobalValue.Instance.senderEmail = this.db.getCurrentSenderEmail().Value;
                    GlobalValue.Instance.serverPort = Int32.Parse(this.db.getCurrentServerPort().Value);
                    GlobalValue.Instance.serverUsername = this.db.getCurrentServerUsername().Value;
                    GlobalValue.Instance.serverPassword = this.db.getCurrentServerPassword().Value;
                    GlobalValue.Instance.monitorComPort = this.db.getCurrentMonitorComPort().Value;
                    GlobalValue.Instance.monitorBaudRate = Int32.Parse(this.db.getCurrentMonitorBaudRate().Value);
                    GlobalValue.Instance.receiptPrinter = this.db.getCurrentReceiptPrinter().Value;
                    GlobalValue.Instance.commissionPrinter = this.db.getCurrentCommissionPrinter().Value;
                    GlobalValue.Instance.oilPrice = Int32.Parse(this.db.getCurrentOilPrice().Value);
                    GlobalValue.Instance.branchNameInMonitor = this.db.getCurrentBranchNameInMonitor().Value;
                    GlobalValue.Instance.report100 = this.db.getCurrentReport100Status().Value;
                    GlobalValue.Instance.report25 = this.db.getCurrentReport25Status().Value;
                    GlobalValue.Instance.reportDetail = this.db.getCurrentReportDetailStatus().Value;
                }
                else
                {


                    var parseJson = JObject.Parse(GlobalValue.Instance.Json_MasterData);
                    string versionNoFromServer = (string)parseJson["Version_Number"];

                    //string query = "SELECT * FROM Version WHERE BranchId = " + this.db.getBranch().Id;
                    //DataTable dt = new DataTable();
                    //dt = RunCommand(query, "Version").Tables["Version"];

                    if (Int32.Parse(versionNoFromServer) > this.db.getVersion().VersionNo)
                    {
                        //Delete MassageSet MassageTopic MassagePlan
                        this.db.clearData();


                        //string queryBranch = "SELECT * FROM Branch WHERE Id = " + this.db.getBranch().Id;
                        //DataTable dtBranch = new DataTable();
                        //dtBranch = RunCommand(queryBranch, "Branch").Tables["Branch"];

                        //Update MassageSetId and UpdateDateTime
                        string massageSetIdFromServer = (string)parseJson["MassageSet_Id"];
                        Branch curBranch = this.db.getBranch();
                        curBranch.MassageSetId = Int32.Parse(massageSetIdFromServer);
                        curBranch.UpdateDateTime = getCurDateTime();
                        this.db.updateBranch(curBranch);

                        //Insert New MassageTopic
                        var rootMassageTopic = parseJson["MassageTopic"];
                        for (int i = 0; i < rootMassageTopic.Count(); i++)
                        {
                            MassageTopic mTopic = new MassageTopic();
                            mTopic.Id = (int)rootMassageTopic[i]["Id"];
                            mTopic.Name = rootMassageTopic[i]["Name"].ToString();
                            mTopic.HeaderColor = rootMassageTopic[i]["HeaderColor"].ToString();
                            mTopic.ChildColor = rootMassageTopic[i]["ChildColor"].ToString();
                            mTopic.CreateDateTime = ConvertDateTime(rootMassageTopic[i]["CreateDateTime"].ToString());

                            this.db.InsertMassageTopic(mTopic);
                        }

                        //Insert New MassagePlan
                        var rootMassagePlan = parseJson["MassagePlan"];
                        for (int i = 0; i < rootMassagePlan.Count(); i++)
                        {
                            MassagePlan mPlan = new MassagePlan();
                            mPlan.Id = (int)rootMassagePlan[i]["Id"];
                            mPlan.Name = rootMassagePlan[i]["Name"].ToString();
                            mPlan.CreateDateTime = ConvertDateTime(rootMassagePlan[i]["CreateDateTime"].ToString());

                            this.db.InsertMassagePlan(mPlan);
                        }

                        //Insert New MassageSet
                        var rootMassageSet = parseJson["MassageSet"];
                        for (int i = 0; i < rootMassageSet.Count(); i++)
                        {
                            MassageSet mSet = new MassageSet();
                            mSet.Id = (int)rootMassageSet[i]["Id"];
                            mSet.MassageTopicId = (int)rootMassageSet[i]["MassageTopicId"];
                            mSet.MassagePlanId = (int)rootMassageSet[i]["MassagePlanId"];
                            mSet.Price = rootMassageSet[i]["Price"].ToString();
                            mSet.Commission = rootMassageSet[i]["Commission"].ToString();
                            mSet.CreateDateTime = ConvertDateTime(rootMassageSet[i]["CreateDateTime"].ToString());

                            this.db.InsertMassageSet(mSet);
                        }

                        //Update 03 October 2022
                        //Insert New OtherSale Master
                        var rootOtherSaleMaster = parseJson["OtherSale"];
                        for (int i = 0; i < rootOtherSaleMaster.Count(); i++)
                        {
                            OtherSale mOtherSale = new OtherSale();
                            mOtherSale.Id = (int)rootOtherSaleMaster[i]["Id"];
                            mOtherSale.Name = rootOtherSaleMaster[i]["Name"].ToString();
                            mOtherSale.Price = rootOtherSaleMaster[i]["Price"].ToString();
                            mOtherSale.Status = rootOtherSaleMaster[i]["Status"].ToString();
                            mOtherSale.CreateDateTime = ConvertDateTime(rootOtherSaleMaster[i]["CreateDateTime"].ToString());
                            //mDiscountMaster.CreateDateTime = ConvertDateTime(rootMassageSet[i]["CreateDateTime"].ToString());
                            //mDiscountMaster.UpdateDateTime = ConvertDateTime(rootMassageSet[i]["UpdateDateTime"].ToString());

                            this.db.InsertOtherSaleMaster(mOtherSale);
                        }

                        //Insert New DiscountMaster
                        var rootDiscountMaster = parseJson["DiscountMaster"];
                        for (int i = 0; i < rootDiscountMaster.Count(); i++)
                        {
                            DiscountMaster mDiscountMaster = new DiscountMaster();
                            mDiscountMaster.Id = (int)rootDiscountMaster[i]["Id"];
                            mDiscountMaster.Name = rootDiscountMaster[i]["Name"].ToString();
                            mDiscountMaster.Status = rootDiscountMaster[i]["Status"].ToString();
                            //mDiscountMaster.CreateDateTime = ConvertDateTime(rootMassageSet[i]["CreateDateTime"].ToString());
                            //mDiscountMaster.UpdateDateTime = ConvertDateTime(rootMassageSet[i]["UpdateDateTime"].ToString());

                            this.db.InsertDiscountMaster(mDiscountMaster);
                        }

                        //Insert New DiscountMasterDetail
                        var rootDiscountMasterDetail = parseJson["DiscountMasterDetail"];
                        for (int i = 0; i < rootDiscountMasterDetail.Count(); i++)
                        {
                            DiscountMasterDetail mDiscountMasterDetail = new DiscountMasterDetail();
                            mDiscountMasterDetail.Id = (int)rootDiscountMasterDetail[i]["Id"];
                            mDiscountMasterDetail.DiscountMasterId = (int)rootDiscountMasterDetail[i]["DiscountMasterId"];
                            mDiscountMasterDetail.Name = rootDiscountMasterDetail[i]["Name"].ToString();
                            mDiscountMasterDetail.Value = rootDiscountMasterDetail[i]["Value"].ToString();
                            mDiscountMasterDetail.Status = rootDiscountMasterDetail[i]["Status"].ToString();
                            //mDiscountMasterDetail.CreateDateTime = ConvertDateTime(rootMassageSet[i]["CreateDateTime"].ToString());
                            //mDiscountMasterDetail.UpdateDateTime = ConvertDateTime(rootMassageSet[i]["UpdateDateTime"].ToString());

                            this.db.InsertDiscountMasterDetail(mDiscountMasterDetail);
                        }

                        //Update VersionNo
                        ProgramVersion curVer = this.db.getVersion();
                        curVer.VersionNo = Int32.Parse((string)parseJson["Version_Number"]);
                        curVer.UpdateDateTime = getCurDateTime();
                        this.db.updateVersion(curVer);

                        ////Update Password
                        //Setting currentPasswordSetting = this.db.getCurrentPassword();
                        //currentPasswordSetting.Value = (string)parseJson["Password"];
                        //currentPasswordSetting.Version = (string)parseJson["Password_Version"];
                        //currentPasswordSetting.Status = (string)parseJson["Password_Status"];
                        //this.db.updatePassword(currentPasswordSetting);

                    }
                    else
                    {

                        ////Update Password
                        //Setting currentPasswordSetting = this.db.getCurrentPassword();
                        //currentPasswordSetting.Value = (string)parseJson["Password"];
                        //currentPasswordSetting.Version = (string)parseJson["Password_Version"];
                        //currentPasswordSetting.Status = (string)parseJson["Password_Status"];
                        //this.db.updatePassword(currentPasswordSetting);
                    }

                    //Insert and Update System Setting
                    this.db.clearAllSetting();

                    //Reset Seq
                    sqlite_sequence settingSeq = this.db.getSettingSeq();
                    if(settingSeq!=null)
                    {
                        settingSeq.seq = "0";
                        this.db.updateSettingSeq(settingSeq);
                    }

                    var rootSystemConfig = parseJson["SystemConfig"];
                    for (int i = 0; i < rootSystemConfig.Count(); i++)
                    {
                        Setting mSetting = new Setting();
                        mSetting.Id = (int)rootSystemConfig[i]["Id"];
                        mSetting.Name = rootSystemConfig[i]["Name"].ToString();
                        mSetting.Value = rootSystemConfig[i]["Value"].ToString();
                        mSetting.Version = (int)rootSystemConfig[i]["Version"];
                        mSetting.Status = rootSystemConfig[i]["Status"].ToString();
                        //mSetting.CreateDateTime = ConvertDateTime(rootMassageSet[i]["CreateDateTime"].ToString());
                        //mSetting.UpdateDateTime = ConvertDateTime(rootMassageSet[i]["UpdateDateTime"].ToString());

                        this.db.InsertSystemSetting(mSetting);
                    }

                    GlobalValue.Instance.emailServer = this.db.getCurrentEmailServer().Value;
                    GlobalValue.Instance.senderEmail = this.db.getCurrentSenderEmail().Value;
                    GlobalValue.Instance.serverPort = Int32.Parse(this.db.getCurrentServerPort().Value);
                    GlobalValue.Instance.serverUsername = this.db.getCurrentServerUsername().Value;
                    GlobalValue.Instance.serverPassword = this.db.getCurrentServerPassword().Value;
                    GlobalValue.Instance.monitorComPort = this.db.getCurrentMonitorComPort().Value;
                    GlobalValue.Instance.monitorBaudRate = Int32.Parse(this.db.getCurrentMonitorBaudRate().Value);
                    GlobalValue.Instance.receiptPrinter = this.db.getCurrentReceiptPrinter().Value;
                    GlobalValue.Instance.commissionPrinter = this.db.getCurrentCommissionPrinter().Value;
                    GlobalValue.Instance.oilPrice = Int32.Parse(this.db.getCurrentOilPrice().Value);
                    GlobalValue.Instance.branchNameInMonitor = this.db.getCurrentBranchNameInMonitor().Value;
                    GlobalValue.Instance.report100 = this.db.getCurrentReport100Status().Value;
                    GlobalValue.Instance.report25 = this.db.getCurrentReport25Status().Value;
                    GlobalValue.Instance.reportDetail = this.db.getCurrentReportDetailStatus().Value;
                }
            }
            catch (Exception o)
            {
                MessageBox.Show(o.ToString());
                GlobalValue.Instance.emailServer = this.db.getCurrentEmailServer().Value;
                GlobalValue.Instance.senderEmail = this.db.getCurrentSenderEmail().Value;
                GlobalValue.Instance.serverPort = Int32.Parse(this.db.getCurrentServerPort().Value);
                GlobalValue.Instance.serverUsername = this.db.getCurrentServerUsername().Value;
                GlobalValue.Instance.serverPassword = this.db.getCurrentServerPassword().Value;
                GlobalValue.Instance.monitorComPort = this.db.getCurrentMonitorComPort().Value;
                GlobalValue.Instance.monitorBaudRate = Int32.Parse(this.db.getCurrentMonitorBaudRate().Value);
                GlobalValue.Instance.receiptPrinter = this.db.getCurrentReceiptPrinter().Value;
                GlobalValue.Instance.commissionPrinter = this.db.getCurrentCommissionPrinter().Value;
                GlobalValue.Instance.oilPrice = Int32.Parse(this.db.getCurrentOilPrice().Value);
                GlobalValue.Instance.branchNameInMonitor = this.db.getCurrentBranchNameInMonitor().Value;
                GlobalValue.Instance.report100 = this.db.getCurrentReport100Status().Value;
                GlobalValue.Instance.report25 = this.db.getCurrentReport25Status().Value;
                GlobalValue.Instance.reportDetail = this.db.getCurrentReportDetailStatus().Value;

                MessageBox.Show("Check version fail, Please check your internet connection");
            }

        }

        public void InitialInterface()
        {
            //List<Branch> listBranch = this.db.getAllBranch();
            branchNameTxt.Text = this.db.getBranch().Name;
            reportBranchNameTxt.Text = this.db.getBranch().Name;
            currentBranchName = this.db.getBranch().Name;

            DateTime current = DateTime.Now;
            dateTxt.Text = current.ToString("dd MMMM yyyy");
            timeTxt.Text = current.ToString("HH:mm:ss");
            reportDateTxt.Text = current.ToString("dd MMMM yyyy");
            reportTimeTxt.Text = current.ToString("HH:mm:ss");

            List<int> getAllMassageTopicId = new List<int>();
            List<MassageSet> getAllMassageSets = this.db.getAllMassageSet();
            for(int i=0;i<getAllMassageSets.Count();i++)
            {
                getAllMassageTopicId.Add(getAllMassageSets[i].MassageTopicId);
                //System.Diagnostics.Debug.WriteLine(getAllMassageSets[i].MassageTopicId+"");
            }

            var set = new HashSet<int>(getAllMassageTopicId);
            List<int> finalTopicId = set.ToList<int>();

            globalListMassageTopic = this.db.getAllMassageTopic(finalTopicId);
            List<MassageTopic> listMassageTopic = this.db.getAllMassageTopic(finalTopicId);
            if(listMassageTopic.Count() < 1)
            {

            }
            else
            {
                for(int i=0;i< listMassageTopic.Count();i++)
                {

                    //Item StackPanel
                    StackPanel itemStackPanel = new StackPanel()
                    {
                        Width = 194,
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Top

                    };

                    //Topic
                    Grid topicGrid = new Grid()
                    {
                        Width = 190,
                        Height = 70,
                        Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(listMassageTopic[i].HeaderColor)),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Margin = new Thickness(0,0,0,12)
                    };

                    //Topic Text
                    TextBlock topicText = new TextBlock()
                    {
                        FontSize = 16,
                        Foreground = new SolidColorBrush(Colors.White),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Text = listMassageTopic[i].Name,
                        TextWrapping = TextWrapping.Wrap,
                        TextTrimming = TextTrimming.None,
                        TextAlignment = TextAlignment.Center,
                        Margin = new Thickness(4, 0, 4, 0)
                    };

                    topicGrid.Children.Add(topicText);
                    itemStackPanel.Children.Add(topicGrid);

                    List<MassageSet> msgPlanFromTopic = this.db.getMassagePlanFromTopic(listMassageTopic[i].Id);
                    if(msgPlanFromTopic.Count() < 1)
                    {

                    }
                    else
                    {
                        for(int j=0;j<msgPlanFromTopic.Count();j++)
                        {
                            string msgPlanName = this.db.getMassagePlanName(msgPlanFromTopic[j].MassagePlanId);
                            string msgPlanPrice = msgPlanFromTopic[j].Price;

                            //Massage Plan Border
                            Border borderPlan = new Border()
                            {
                                Width = 186,
                                Height = 100,
                                CornerRadius = new CornerRadius(25),
                                BorderThickness = new Thickness(0),
                                Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(listMassageTopic[i].ChildColor)),
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Stretch,
                                Margin = new Thickness(0, 3, 0, 3),
                                Tag = msgPlanFromTopic[j].MassageTopicId+"Y"+ msgPlanFromTopic[j].MassagePlanId+"Y"+ msgPlanFromTopic[j].Price+"Y"+ msgPlanFromTopic[j].Commission+"Y"+listMassageTopic[i].HeaderColor + "Y" + listMassageTopic[i].ChildColor + "Y"+"false"

                            };

                            borderPlan.MouseLeftButtonDown += BorderPlan_MouseLeftButtonDown;

                            //Massage Plan text
                            TextBlock planText = new TextBlock()
                            {
                                FontSize = 15,
                                Foreground = new SolidColorBrush(Colors.Black),
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Center,
                                Text = msgPlanName+"\n("+msgPlanPrice+" ฿)",
                                TextWrapping = TextWrapping.Wrap,
                                TextTrimming = TextTrimming.None,
                                TextAlignment = TextAlignment.Center,
                                Margin = new Thickness(4, 0, 4, 0)
                            };

                            borderPlan.Child = planText;
                            itemStackPanel.Children.Add(borderPlan);

                        }
                    }
                    
                    MassageSetContainer.Children.Add(itemStackPanel);
                    
                }
            }

            //Get all voucher list
            List<DiscountMasterDetail> listAllVOuchers = this.db.getAllVoucherList();
            for(int k = 0; k < listAllVOuchers.Count(); k++)
            {
                Grid voucherItem = new Grid()
                {
                    Width = 196,
                    Height = 70,
                    Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFD5E0EC")),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    //VerticalAlignment = VerticalAlignment.Stretch,
                    Margin = new Thickness(2, 0, 2, 2),
                    Tag = listAllVOuchers[k].Id + "Y" + listAllVOuchers[k].DiscountMasterId + "Y" + listAllVOuchers[k].Name + "Y" + listAllVOuchers[k].Value
                };

                voucherItem.MouseLeftButtonDown += VoucherItem_MouseLeftButtonDown;

                //Topic Text
                TextBlock voucherText = new TextBlock()
                {
                    FontSize = 18,
                    Foreground = new SolidColorBrush(Colors.Black),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Text = listAllVOuchers[k].Name,
                    TextWrapping = TextWrapping.Wrap,
                    TextTrimming = TextTrimming.None,
                    TextAlignment = TextAlignment.Center
                    //Margin = new Thickness(4, 0, 4, 0)
                };

                voucherItem.Children.Add(voucherText);
                voucherContainer.Children.Add(voucherItem);
            }

        }

        private void VoucherItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Grid voucherClicked = (Grid)sender;
            String[] voucherTagSplit = voucherClicked.Tag.ToString().Split('Y');
            int voucherId = Int32.Parse(voucherTagSplit[0]);
            //int voucherValue = Int32.Parse(voucherTagSplit[1]);
            DateTime current = DateTime.Now;
            string curDate = current.ToString("yyyy-MM-dd");
            string curTime = current.ToString("HH:mm:ss");
            string curDateTime = current.ToString("yyyy-MM-dd HH:mm:ss");

            //add discount row in summary page
            Grid itemInDiscountSummary = new Grid()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(24, 6, 24, 0)

            };

            TextBlock discountNameTxt = new TextBlock()
            {
                FontSize = 20,
                Foreground = new SolidColorBrush(Colors.Black),
                HorizontalAlignment = HorizontalAlignment.Left,
                Text = "- " + voucherTagSplit[2]
            };

            TextBlock discountValue = new TextBlock()
            {
                FontSize = 18,
                Foreground = new SolidColorBrush(Colors.Black),
                HorizontalAlignment = HorizontalAlignment.Right,
                Text = "- "+String.Format("{0:n}", voucherTagSplit[3]) + " ฿"
            };

            itemInDiscountSummary.Children.Add(discountNameTxt);
            itemInDiscountSummary.Children.Add(discountValue);

            summaryDiscountContainer.Children.Add(itemInDiscountSummary);

            //Convert summary amount to integer
            string cleanFloatingSummaryAmount = summaryAmountTxt.Text.Replace(".00", "");
            string cleanCommaSummaryAmount = cleanFloatingSummaryAmount.Replace(",", "");
            string finalCleanSummaryAmount = cleanCommaSummaryAmount.Replace(" ฿", "");
            int currentSummaryAmount = Int32.Parse(finalCleanSummaryAmount);
            int discountValueNumeric = Int32.Parse(voucherTagSplit[3]);
            int afterDiscount = currentSummaryAmount - discountValueNumeric;

            summaryAmountTxt.Text = String.Format("{0:n}", afterDiscount) + " ฿";
            finalBalance = afterDiscount;
            SendTextTotalWithDiscount();

            DiscountMasterDetail discountItemDetail = this.db.getDiscountMasterDetailFromId(voucherId);
            DiscountRecord discountRecordItemDetail = new DiscountRecord()
            {
                AccountId = currentUseAccountId,
                Date = curDate,
                Time = curTime,
                DiscountMasterId = discountItemDetail.DiscountMasterId,
                DiscountMasterDetailId = discountItemDetail.Id,
                Value = discountItemDetail.Value,
                SendStatus = "false",
                CancelStatus = "false",
                CreateDateTime = curDateTime,
                UpdateDateTime = curDateTime
            };

            prepareDiscount.Add(discountRecordItemDetail);
        }

        private void BorderPlan_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Border planClicked = (Border)sender;
            String[] planTagSplit = planClicked.Tag.ToString().Split('Y');

            DateTime current = DateTime.Now;
            string curDate = current.ToString("yyyy-MM-dd");
            string curTime = current.ToString("HH:mm:ss");
            string curDateTime = current.ToString("yyyy-MM-dd HH:mm:ss");

            if (planTagSplit[6].Equals("false"))
            {
                checkoutAndClearGrid.Visibility = Visibility.Visible;

                planClicked.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(planTagSplit[4]));
                planClicked.Tag = planClicked.Tag.ToString().Replace("false", "true");

                currentBalance = currentBalance + Int32.Parse(planTagSplit[2]);
                currentBalanceTxt.Text = String.Format("{0:n}", currentBalance);

                OrderRecord ordRecord = new OrderRecord()
                {
                    AccountId = currentUseAccountId,
                    Date = curDate,
                    Time = curTime,
                    MassageTopicId = Int32.Parse(planTagSplit[0]),
                    MassagePlanId = Int32.Parse(planTagSplit[1]),
                    Price = planTagSplit[2],
                    Commission = planTagSplit[3],
                    SendStatus = "false",
                    CancelStatus = "false",
                    CreateDateTime = curDateTime,
                    UpdateDateTime = curDateTime
                };

                prepareOrder.Add(ordRecord);
                SendTextToMonitor(planTagSplit);
            }
            else
            {
                planClicked.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(planTagSplit[5]));
                planClicked.Tag = planClicked.Tag.ToString().Replace("true", "false");

                currentBalance = currentBalance - Int32.Parse(planTagSplit[2]);
                if (currentBalance == 0)
                {
                    currentBalanceTxt.Text = "ราคาทั้งหมด / Total";
                    checkoutAndClearGrid.Visibility = Visibility.Collapsed;
                    ClearText();
                }
                else
                {
                    currentBalanceTxt.Text = String.Format("{0:n}", currentBalance);
                }
                
                for(int x=0;x<prepareOrder.Count();x++)
                {
                    if((prepareOrder[x].MassageTopicId == Int32.Parse(planTagSplit[0]))&&(prepareOrder[x].MassagePlanId == Int32.Parse(planTagSplit[1])))
                    {
                        prepareOrder.RemoveAt(x);
                    }
                }
                
            }

            //MessageBox.Show(planClicked.Tag.ToString());
            /*
            MessageBox.Show(prepareOrder.Count().ToString());
            for(int u=0;u<prepareOrder.Count();u++)
            {
                MessageBox.Show(prepareOrder[u].MassageTopicId + " " + prepareOrder[u].MassagePlanId + " " + prepareOrder[u].Price);
            }*/
            
        }

        public void CheckIn()
        {
            List<Account> listAccount = this.db.getAllAccount();
            if(listAccount.Count() > 0)
            {
                if(this.db.getLatestAcount().Completed.Equals("false"))
                {
                    currentUseAccountId = this.db.getLatestAcount().Id;
                    currentStaffNo = Int32.Parse(this.db.getStaffNumberFromAccountId(currentUseAccountId));
                    staffAmountTxt.Text = currentStaffNo.ToString();
                }
                else
                {
                    InitMoneyGrid.Visibility = Visibility.Visible;
                    if (CheckInternet() == true)
                    {
                        availableTbl.Visibility = Visibility.Visible;
                        unavailableTbl.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        availableTbl.Visibility = Visibility.Collapsed;
                        unavailableTbl.Visibility = Visibility.Visible;
                    }
                }
            }
            else
            {
                InitMoneyGrid.Visibility = Visibility.Visible;
                if (CheckInternet() == true)
                {
                    availableTbl.Visibility = Visibility.Visible;
                    unavailableTbl.Visibility = Visibility.Collapsed;
                }
                else
                {
                    availableTbl.Visibility = Visibility.Collapsed;
                    unavailableTbl.Visibility = Visibility.Visible;
                }
            }

        }

        private void ClockRunning()
        {
            DispatcherTimer timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                DateTime current = DateTime.Now;
                dateTxt.Text = current.ToString("dd MMMM yyyy");
                timeTxt.Text = current.ToString("HH:mm:ss");
                reportDateTxt.Text = current.ToString("dd MMMM yyyy");
                reportTimeTxt.Text = current.ToString("HH:mm:ss");
            }, this.Dispatcher);
        }

        
        private void Number_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            showInitMoneyTb.Text += btn.Content;
        }

        private void passwordNumber_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            if(showCodeTb.Password.Length < 4)
            {
                showCodeTb.Password += btn.Content;
            }
        }

        private void enterBtn_Click(object sender, RoutedEventArgs e)
        {
            if(showInitMoneyTb.Text.Length > 0)
            {
                DateTime current = DateTime.Now;
                string curDate = current.ToString("yyyy-MM-dd");
                string curTime = current.ToString("HH:mm:ss");
                string curDateTime = getCurDateTime();

                Account ac = new Account()
                {
                    Date = curDate,
                    Time = curTime,
                    StartMoney = showInitMoneyTb.Text,
                    StaffAmount = "0",
                    Completed = "false",
                    SendStatus = "false",
                    UpdateStatus = "false",
                    CreateDateTime = curDateTime,
                    UpdateDateTime = curDateTime
                };

                this.db.checkIn(ac);

                //List<Account> listAccount = this.db.getAllAccount();
                currentUseAccountId = this.db.getLatestAcount().Id;
                currentStaffNo = Int32.Parse(this.db.getStaffNumberFromAccountId(currentUseAccountId));
                staffAmountTxt.Text = currentStaffNo.ToString();

                InsertAccountToServer();
                InitMoneyGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                MessageBox.Show("กรุณาใส่เงินเริ่มต้น");
            }
        }

        private void enterCodeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (showCodeTb.Password.Length == 4)
            {
                //Check password function
                if (showCodeTb.Password.Equals(this.db.getCurrentPassword().Value))
                {
                    showCodeTb.Password = "";
                    PasswordLockGrid.Visibility = Visibility.Collapsed;
                    InitReportView();
                    reportPopupGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    MessageBox.Show("กรุณาใส่รหัสผ่านให้ถูกต้อง");
                    showCodeTb.Password = "";
                }
                
            }
            else
            {
                MessageBox.Show("กรุณาใส่รหัสผ่านให้ครบ 4 ตำแหน่ง");
            }
        }

        private void clearBtn_Click(object sender, RoutedEventArgs e)
        {
            showInitMoneyTb.Text = "";
        }

        private void deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (showInitMoneyTb.Text.Length >= 1)
            {
                showInitMoneyTb.Text = showInitMoneyTb.Text.Substring(0, showInitMoneyTb.Text.Length - 1);
            }
        }

        private void clearCodeBtn_Click(object sender, RoutedEventArgs e)
        {
            showCodeTb.Password = "";
        }

        private void deleteCodeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (showCodeTb.Password.Length >= 1)
            {
                showCodeTb.Password = showCodeTb.Password.Substring(0, showCodeTb.Password.Length - 1);
            }
        }

        private void Number_Other_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            showInitMoneyForOtherSaleTb.Text += btn.Content;
        }

        private void enterBtn_Other_Click(object sender, RoutedEventArgs e)
        {
            if (showInitMoneyForOtherSaleTb.Text.Length > 0)
            {
                prepareToPayForOtherSale("3","Others", showInitMoneyForOtherSaleTb.Text);
                InitMoneyForOtherSaleGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                MessageBox.Show("กรุณาใส่ราคา");
            }
        }

        private void clearBtn_Other_Click(object sender, RoutedEventArgs e)
        {
            showInitMoneyForOtherSaleTb.Text = "";
        }

        private void deleteBtn_Other_Click(object sender, RoutedEventArgs e)
        {
            if (showInitMoneyForOtherSaleTb.Text.Length >= 1)
            {
                showInitMoneyForOtherSaleTb.Text = showInitMoneyForOtherSaleTb.Text.Substring(0, showInitMoneyForOtherSaleTb.Text.Length - 1);
            }
        }

        private void clearSelectedBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            clearAllSelectedAndBalance();
            ClearText();
        }

        private void checkoutSummaryBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SendTextTotal();
            summaryContainer.Children.Clear();
            summaryDiscountContainer.Children.Clear(); //Clear discount stack panel
            finalBalance = currentBalance;

            for (int i=0;i<prepareOrder.Count();i++)
            {
                Grid itemInSummary = new Grid()
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(24,6,24,0)

                };

                TextBlock planNameTxt = new TextBlock()
                {
                    FontSize = 20,
                    Foreground = new SolidColorBrush(Colors.Black),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Text = "- "+this.db.getMassageTopicName(prepareOrder[i].MassageTopicId)+" ("+this.db.getMassagePlanName(prepareOrder[i].MassagePlanId)+")"
                };

                TextBlock planPrice = new TextBlock()
                {
                    FontSize = 18,
                    Foreground = new SolidColorBrush(Colors.Black),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Text = String.Format("{0:n}", Int32.Parse(prepareOrder[i].Price))+" ฿"
                };

                itemInSummary.Children.Add(planNameTxt);
                itemInSummary.Children.Add(planPrice);

                summaryContainer.Children.Add(itemInSummary);
            }

            summaryAmountTxt.Text = currentBalanceTxt.Text+" ฿";

            summaryPopupGrid.Visibility = Visibility.Visible;
        }

        private void cancelPayBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            summaryPopupGrid.Visibility = Visibility.Collapsed;
            ClearText();
            prepareDiscount.Clear();
            //clearAllSelectedAndBalance();
        }

        private async void payBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            transactionLoadingGrid.Visibility = Visibility.Visible;

            await Task.Delay(500);

            SaveOrderToDB("cash");
            SaveDiscountToDB("cash");
            PrintReceipt();
            PrintCommission();

            summaryPopupGrid.Visibility = Visibility.Collapsed;
            clearAllSelectedAndBalance();
            ClearText();
        }

        private async void payCreditBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            transactionLoadingGrid.Visibility = Visibility.Visible;

            await Task.Delay(500);

            SaveOrderToDB("credit");
            SaveDiscountToDB("credit");
            PrintReceipt();
            PrintCommission();

            summaryPopupGrid.Visibility = Visibility.Collapsed;
            clearAllSelectedAndBalance();
            ClearText();
        }

        public void InitReportView()
        {
            Account acct = this.db.getAccountFromId(currentUseAccountId);
            List<OrderRecord> recordNum = this.db.getAllOrderRecordExceptCancelled(currentUseAccountId);

            reportStartMoneyTxt.Text = String.Format("{0:n}", Int32.Parse(acct.StartMoney)) + " ฿";
            reportIncomeTxt.Text = String.Format("{0:n}", getTotalSale()) + " ฿";
            reportStaffAmountTxt.Text = currentStaffNo + " p.";
            reportPaxAmount.Text = recordNum.Count() + " p.";
            reportOilTxt.Text = String.Format("{0:n}", currentStaffNo * GlobalValue.Instance.oilPrice) + " ฿";
            reportCommissionTxt.Text = String.Format("{0:n}", getTotalCommission()) + " ฿";
            reportOtherSaleTxt.Text = String.Format("{0:n}", getGrandTotalOtherRecord()) + " ฿";

            reportTotalIncomeTxt.Text = String.Format("{0:n}", (currentStaffNo * GlobalValue.Instance.oilPrice) + getTotalSale() + getGrandTotalOtherRecord());
            reportTotalExpenseTxt.Text = String.Format("{0:n}", getTotalCommission());
            reportGrandTotalTxt.Text = String.Format("{0:n}", (currentStaffNo * GlobalValue.Instance.oilPrice) + getTotalSale() + getGrandTotalOtherRecord() - getTotalCommission());
        }

        private void reportBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(this.db.getCurrentPassword().Status.Equals("true"))
            {
                PasswordLockGrid.Visibility = Visibility.Visible;
            }
            else
            {
                InitReportView();
                reportPopupGrid.Visibility = Visibility.Visible;
            }
            
        }

        private void otherSaleBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            otherSaleListContainer.Children.Clear();
            List<OtherSale> listOtherSales = this.db.getAllOtherSaleList();

            for (int i = 0; i < listOtherSales.Count(); i++)
            {
                Grid itemInSummarys = new Grid();
                itemInSummarys.HorizontalAlignment = HorizontalAlignment.Stretch;
                itemInSummarys.VerticalAlignment = VerticalAlignment.Top;
                itemInSummarys.Margin = new Thickness(24, 6, 24, 0);
                itemInSummarys.Tag = listOtherSales[i].Id + "_" + listOtherSales[i].Name+"_"+listOtherSales[i].Price;
                itemInSummarys.MouseLeftButtonDown += ItemInSummarys_MouseLeftButtonDown;

                TextBlock planNameTxt = new TextBlock()
                {
                    FontSize = 20,
                    Foreground = new SolidColorBrush(Colors.Black),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Text = listOtherSales[i].Name
                };

                TextBlock planPrice = new TextBlock()
                {
                    FontSize = 18,
                    Foreground = new SolidColorBrush(Colors.Black),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Text = String.Format("{0:n}", Int32.Parse(listOtherSales[i].Price)) + " ฿"
                };

                itemInSummarys.Children.Add(planNameTxt);
                itemInSummarys.Children.Add(planPrice);

                otherSaleListContainer.Children.Add(itemInSummarys);
            }

            otherSaleListPopupGrid.Visibility = Visibility.Visible;
        }

        private void closeOtherListBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            otherSaleListPopupGrid.Visibility = Visibility.Collapsed;
        }

        private void ItemInSummarys_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var getObject = (Grid)sender;
            string getTag = getObject.Tag.ToString();
            string[] splitTag = getTag.Split('_');


            if(splitTag[1].Equals("Others"))
            {
                showInitMoneyForOtherSaleTb.Text = "";
                InitMoneyForOtherSaleGrid.Visibility = Visibility.Visible;
            }
            else
            {
                prepareToPayForOtherSale(splitTag[0], splitTag[1], splitTag[2]);
            }
        }

        public void prepareToPayForOtherSale(string otsId, string otsName, string otsPrice)
        {
            summaryOtherContainer.Children.Clear();

            Grid itemInSummary = new Grid()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(24, 6, 24, 0)

            };

            TextBlock planNameTxt = new TextBlock()
            {
                FontSize = 20,
                Foreground = new SolidColorBrush(Colors.Black),
                HorizontalAlignment = HorizontalAlignment.Left,
                Text = "- " + otsName
            };

            TextBlock planPrice = new TextBlock()
            {
                FontSize = 18,
                Foreground = new SolidColorBrush(Colors.Black),
                HorizontalAlignment = HorizontalAlignment.Right,
                Text = String.Format("{0:n}", Int32.Parse(otsPrice)) + " ฿"
            };

            itemInSummary.Children.Add(planNameTxt);
            itemInSummary.Children.Add(planPrice);

            summaryOtherContainer.Children.Add(itemInSummary);


            summaryOtherAmountTxt.Text = otsPrice + " ฿";

            InitMoneyForOtherSaleGrid.Visibility = Visibility.Collapsed;
            summaryOtherPopupGrid.Visibility = Visibility.Visible;

            DateTime current = DateTime.Now;
            string curDate = current.ToString("yyyy-MM-dd");
            string curTime = current.ToString("HH:mm:ss");

            centralOsr = new OtherSaleRecord()
            {
                AccountId = currentUseAccountId,
                Date = curDate,
                Time = curTime,
                OtherSaleId = Int32.Parse(otsId),
                Price = otsPrice,
                SendStatus = "false",
                CancelStatus = "false",
                CreateDateTime = getCurDateTime(),
                UpdateDateTime = getCurDateTime()
            };
        }

        private async void payOtherBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            transactionLoadingGrid.Visibility = Visibility.Visible;

            await Task.Delay(500);

            SaveOtherSaleOrderToDB("cash");
            PrintOtherSaleReceipt();

            summaryOtherPopupGrid.Visibility = Visibility.Collapsed;
            otherSaleListPopupGrid.Visibility = Visibility.Collapsed;
        }

        private async void payCreditOtherBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            transactionLoadingGrid.Visibility = Visibility.Visible;

            await Task.Delay(500);

            SaveOtherSaleOrderToDB("credit");
            PrintOtherSaleReceipt();

            summaryOtherPopupGrid.Visibility = Visibility.Collapsed;
            otherSaleListPopupGrid.Visibility = Visibility.Collapsed;
        }

        private void cancelPayOtherBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            summaryOtherPopupGrid.Visibility = Visibility.Collapsed;
        }


        private void reportCloseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (soldPanelToggle == true)
            {
                //Play Animation
                Storyboard soldIn = this.FindResource("SoldPanelOut") as Storyboard;
                soldIn.Begin();
            }

            reportPopupGrid.Visibility = Visibility.Collapsed;
        }

        private void staffAddBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Show staff confirm adding popup
            staffConfirmGrid.Visibility = Visibility.Visible;
        }

        private async void confirmAddBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            transactionLoadingGrid.Visibility = Visibility.Visible;

            await Task.Delay(500);

            currentStaffNo++;
            Account acctUpdate = this.db.getAccountFromId(currentUseAccountId);
            acctUpdate.StaffAmount = currentStaffNo.ToString();
            acctUpdate.UpdateDateTime = getCurDateTime();

            this.db.updateAcount(acctUpdate);
            staffAmountTxt.Text = currentStaffNo.ToString();

            UpdateAccountToServer();

            staffConfirmGrid.Visibility = Visibility.Collapsed;
        }

        private void cancelAddBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            staffConfirmGrid.Visibility = Visibility.Collapsed;
        }

        private void checkoutBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            exitPopupGrid.Visibility = Visibility.Visible;
        }

        private async void yesBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //PrintEndDay();
            //string zipPath = @"C:\SpaSystem\Release\spasystemdb0.zip";
            //using (FileStream zipFileToOpen = new FileStream(zipPath, FileMode.OpenOrCreate))
            //{
            //    using (ZipArchive archive = new ZipArchive(zipFileToOpen, ZipArchiveMode.Create))
            //    {
            //        //archive.CreateEntry(@"C:\SpaSystem\Release\aaa.pdf");
            //        archive.CreateEntryFromFile(@"C:\SpaSystem\Release\spasystemdb0.db", "spasystemdb0.db");
            //    }
            //}

            loadingGrid.Visibility = Visibility.Visible;
            //loadingTxt.Text = "Computer กำลังปิด โปรดรอสักครู่...";

            await Task.Delay(2000);
            exportPDF();
        }

        private void cancelExitBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            exitPopupGrid.Visibility = Visibility.Collapsed;
        }

        public void InitSoldList()
        {
            soldItemStack.Children.Clear();

            List<OrderRecord> ordRecList = this.db.getAllOrderRecord(currentUseAccountId);
            for (int u = 0; u < ordRecList.Count(); u++)
            {
                CancelRecordParam cancelParams = new CancelRecordParam()
                {
                    OrderRecordId = ordRecList[u].Id,
                    AccountId = ordRecList[u].AccountId,
                    ItemNo = u + 1,
                    TotalItems = ordRecList.Count(),
                    CancelStatus = ordRecList[u].CancelStatus
                };

                Grid itemGrid = new Grid()
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Top,
                    Tag = cancelParams
                };

                TextBlock massageNameItemTxt = new TextBlock()
                {
                    Text = this.db.getMassageTopicName(ordRecList[u].MassageTopicId) + " (" + this.db.getMassagePlanName(ordRecList[u].MassagePlanId) + ")" + "\nTime : " + ordRecList[u].Time + "   Commission : " + String.Format("{0:n}", Int32.Parse(ordRecList[u].Commission)) + " ฿",
                    FontSize = 15,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    TextWrapping = TextWrapping.Wrap,
                    TextTrimming = TextTrimming.None,
                    Width = 300,
                    Padding = new Thickness(8,8,0,8)
                };

                TextBlock massagePriceItemTxt = new TextBlock()
                {
                    Text = String.Format("{0:n}", Int32.Parse(ordRecList[u].Price)) + " ฿",
                    FontSize = 15,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    TextWrapping = TextWrapping.Wrap,
                    TextTrimming = TextTrimming.None,
                    Width = 100,
                    Padding = new Thickness(0, 8, 8, 8),
                    TextAlignment = TextAlignment.Right
                };

                if (ordRecList[u].CancelStatus.Equals("true"))
                {
                    itemGrid.Background = new SolidColorBrush(Colors.Red);
                    massageNameItemTxt.Foreground = new SolidColorBrush(Colors.White);
                    massagePriceItemTxt.Foreground = new SolidColorBrush(Colors.White);
                }
                else
                {
                    massageNameItemTxt.Foreground = new SolidColorBrush(Colors.Black);
                    massagePriceItemTxt.Foreground = new SolidColorBrush(Colors.Blue);
                }

                itemGrid.MouseLeftButtonDown += ItemGrid_MouseLeftButtonDown;

                itemGrid.Children.Add(massageNameItemTxt);
                itemGrid.Children.Add(massagePriceItemTxt);
                soldItemStack.Children.Add(itemGrid);
            }
        }

        private void reportListBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            soldPanelToggle = true;

            //Initial Sold List
            InitSoldList();

            //Play Animation
            Storyboard soldIn = this.FindResource("SoldPanelIn") as Storyboard;
            soldIn.Begin();
        }

        private void ItemGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Grid getCurItem = (Grid)sender;
            getCancelParams = (CancelRecordParam)getCurItem.Tag;

            if((getCancelParams.ItemNo==getCancelParams.TotalItems)&&getCancelParams.CancelStatus.Equals("false"))
            {
                deleteRecordConfirmGrid.Visibility = Visibility.Visible;
            }

            //MessageBox.Show("id=" + getCancelParams.OrderRecordId + "\n" +
            //                "accountid=" + getCancelParams.AccountId + "\n" +
            //                "itemno=" + getCancelParams.ItemNo + "\n" +
            //                "total=" + getCancelParams.TotalItems + "\n");
        }

        private void confirmDeleteRecordBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {

                OrderRecord getSelectedRecordForUpdate = this.db.getOrderRecordtFromIdAndAccountId(getCancelParams.OrderRecordId, getCancelParams.AccountId);
                getSelectedRecordForUpdate.CancelStatus = "true";
                getSelectedRecordForUpdate.UpdateDateTime = getCurDateTime();

                this.db.updateOrderRecord(getSelectedRecordForUpdate);
                UpdateOrderRecordToServer(getSelectedRecordForUpdate);
            }
            catch(Exception io)
            {
                MessageBox.Show("มีบางอย่างผิดพลาด ไม่สามารถยกเลิกได้ กรุณาติดต่อผู้ดูแลระบบ");
            }

            deleteRecordConfirmGrid.Visibility = Visibility.Collapsed;

            //Initial Sold List After Cancelled
            InitSoldList();
            InitReportView();
        }

        private void cancelDeleteRecordBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            deleteRecordConfirmGrid.Visibility = Visibility.Collapsed;
        }

        private void closeSoldListBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            soldPanelToggle = false;

            //Play Animation
            Storyboard soldIn = this.FindResource("SoldPanelOut") as Storyboard;
            soldIn.Begin();
        }

        public void SaveOrderToDB(string paymentType)
        {
            Account getUnSendAc = this.db.getLatestAcount();
            if(getUnSendAc.SendStatus.Equals("false"))
            {
                InsertAccountToServer();
            }

            if (this.db.getAllUnSendOrderRecord(currentUseAccountId).Count() != 0)
            {
                List<OrderRecord> listUnsendOrderRecord = this.db.getAllUnSendOrderRecord(currentUseAccountId);
                for (int k = 0; k < listUnsendOrderRecord.Count(); k++)
                {
                    OrderRecord myOrder = listUnsendOrderRecord[k];
                    InsertOrderRecordToServer(myOrder);
                }
            }
            for (int i=0;i<prepareOrder.Count();i++)
            {
                if(paymentType.Equals("credit"))
                {
                    prepareOrder[i].IsCreditCard = "true";
                }
                else
                {
                    prepareOrder[i].IsCreditCard = "false";
                }
                
                this.db.saveOrder(prepareOrder[i]);
                InsertOrderRecordToServer(prepareOrder[i]);
            }
        }

        public void SaveOtherSaleOrderToDB(string paymentType)
        {
            Account getUnSendAc = this.db.getLatestAcount();
            if (getUnSendAc.SendStatus.Equals("false"))
            {
                InsertAccountToServer();
            }

            if (this.db.getAllUnSendOtherSaleRecord(currentUseAccountId).Count() != 0)
            {
                List<OtherSaleRecord> listUnsendOtherSaleRecord = this.db.getAllUnSendOtherSaleRecord(currentUseAccountId);
                for (int k = 0; k < listUnsendOtherSaleRecord.Count(); k++)
                {
                    OtherSaleRecord myOrder = listUnsendOtherSaleRecord[k];
                    InsertOtherSaleOrderRecordToServer(myOrder);
                }
            }
            //for (int i = 0; i < prepareOrder.Count(); i++)
            //{
            //    this.db.saveOrder(prepareOrder[i]);
            //    InsertOrderRecordToServer(prepareOrder[i]);
            //}
            if (paymentType.Equals("credit"))
            {
                centralOsr.IsCreditCard = "true";
            }
            else
            {
                centralOsr.IsCreditCard = "false";
            }

            this.db.saveOtherSaleOrder(centralOsr);
            InsertOtherSaleOrderRecordToServer(centralOsr);

        }

        public void SaveDiscountToDB(string paymentType)
        {
            string curDateTime = getCurDateTime();
            List<OrderRecordWithDiscount> listOrderWithDiscount = new List<OrderRecordWithDiscount>();

            Account getUnSendAc = this.db.getLatestAcount();
            if (getUnSendAc.SendStatus.Equals("false"))
            {
                InsertAccountToServer();
            }

            //Check unsent discount then send to the server
            if (this.db.getAllUnSendDiscountRecord(currentUseAccountId).Count() != 0)
            {
                List<DiscountRecord> listUnsendDiscountRecord = this.db.getAllUnSendDiscountRecord(currentUseAccountId);
                for (int k = 0; k < listUnsendDiscountRecord.Count(); k++)
                {
                    DiscountRecord myDiscount = listUnsendDiscountRecord[k];
                    InsertDiscountRecordToServer(myDiscount);
                }
            }

            //Check unsent order record with dicount then send to the server
            if (this.db.getAllUnSendOrderRecordWithDiscount(currentUseAccountId).Count() != 0)
            {
                List<OrderRecordWithDiscount> listUnsendOrderRecordWithDiscount = this.db.getAllUnSendOrderRecordWithDiscount(currentUseAccountId);
                for (int k = 0; k < listUnsendOrderRecordWithDiscount.Count(); k++)
                {
                    OrderRecordWithDiscount myOrderWithDiscount = listUnsendOrderRecordWithDiscount[k];
                    InsertOrderRecordWithDiscountToServer(myOrderWithDiscount);
                }
            }

            //Save discount to local db then sent to server
            for (int i = 0; i < prepareDiscount.Count(); i++)
            {
                if (paymentType.Equals("credit"))
                {
                    prepareDiscount[i].IsCreditCard = "true";
                }
                else
                {
                    prepareDiscount[i].IsCreditCard = "false";
                }

                this.db.saveDiscountRecord(prepareDiscount[i]);
                InsertDiscountRecordToServer(prepareDiscount[i]);
            }

            //Save order record with discount to local db then sent to server
            for (int k=0;k<prepareOrder.Count();k++)
            {
                for(int m=0;m<prepareDiscount.Count();m++)
                {
                    OrderRecordWithDiscount orderWithDiscountItem = new OrderRecordWithDiscount()
                    {
                        OrderRecordId = prepareOrder[k].Id,
                        DiscountRecordId = prepareDiscount[m].Id,
                        AccountId = currentUseAccountId,
                        SendStatus = "false",
                        CreateDateTime = curDateTime,
                        UpdateDateTime = curDateTime
                    };

                    //listOrderWithDiscount.Add(orderWithDiscountItem);
                    this.db.saveOrderRecordWithDiscount(orderWithDiscountItem);
                    InsertOrderRecordWithDiscountToServer(orderWithDiscountItem);
                }
            }

        }

        public void PrintReceipt()
        {
            string getReceipt = getListItemInInvoice();
            //string thReplaceMin = TheSlip.getInvoice().Replace("นาที", "mins");
            //string thReplaceHr = thReplaceMin.Replace("ชั่วโมง", "hr");
            var sb = new StringBuilder();
            sb.AppendLine("       " + currentBranchName);
            sb.AppendLine("     " + DateTime.Now.ToString("dd MMMM yyyy    HH:mm"));
            sb.AppendLine("===============================");
            //sb.AppendLine("\n");
            sb.AppendLine("           < Receipt >");
            sb.AppendLine(getReceipt);
            sb.AppendLine("------------------------------");
            sb.AppendLine("       Total     " + String.Format("{0:n}", finalBalance) + " Baht");
            sb.AppendLine("\n");
            sb.AppendLine(" Thank you for using our service");
            sb.AppendLine("      Please come back again");
            sb.AppendLine("\n\n\n\n");
            sb.AppendLine("\x1b" + "\x69");
            //PrintDialog pd = new PrintDialog();
            //

            RawPrinterHelper.SendStringToPrinter(GlobalValue.Instance.receiptPrinter, sb.ToString());

            transactionLoadingGrid.Visibility = Visibility.Collapsed;
        }

        public void PrintCommission()
        {
            List<OrderRecord> listOrderRecords = this.db.getAllOrderRecord(currentUseAccountId);
            int paxBeforeCalculate = listOrderRecords.Count() - prepareOrder.Count();

            for (int s = 0; s < prepareOrder.Count; s++)
            {
                paxBeforeCalculate = paxBeforeCalculate + 1;
                var sb = new StringBuilder();
                sb.AppendLine("             " + currentBranchName);
                sb.AppendLine("           " + DateTime.Now.ToString("dd MMMM yyyy    HH:mm"));
                sb.AppendLine("             Commission No. : " + paxBeforeCalculate);
                sb.AppendLine("  =======================================");
                sb.AppendLine(" - " + this.db.getMassageTopicName(prepareOrder[s].MassageTopicId) + "(" + this.db.getMassagePlanName(prepareOrder[s].MassagePlanId) + ")");
                sb.AppendLine("\n");
                sb.AppendLine("              Total Commission");
                sb.AppendLine("                 " + String.Format("{0:n}", Int32.Parse(prepareOrder[s].Commission)) + " Baht");
                sb.AppendLine("\n");
                sb.AppendLine("          Don't loose this ticket");
                sb.AppendLine("\n");
                sb.AppendLine("  _______________________________________");
                sb.AppendLine("\n\n\n");
                sb.AppendLine("\x1b" + "\x69");
                //PrintDialog pd = new PrintDialog();
                //MessageBox.Show(paxBeforeCalculate+"");
                RawPrinterHelper.SendStringToPrinter(GlobalValue.Instance.commissionPrinter, sb.ToString());

            }

            transactionLoadingGrid.Visibility = Visibility.Collapsed;
        }

        public void PrintOtherSaleReceipt()
        {
            string getReceipt = getListItemInInvoice();
            //string thReplaceMin = TheSlip.getInvoice().Replace("นาที", "mins");
            //string thReplaceHr = thReplaceMin.Replace("ชั่วโมง", "hr");
            var sb = new StringBuilder();
            sb.AppendLine("       " + currentBranchName);
            sb.AppendLine("     " + DateTime.Now.ToString("dd MMMM yyyy    HH:mm"));
            sb.AppendLine("===============================");
            //sb.AppendLine("\n");
            sb.AppendLine("           < Receipt >");
            sb.AppendLine("- " + this.db.getOtherSaleNameFromId(centralOsr.OtherSaleId) + "\n  " + String.Format("{0:n}", Int32.Parse(centralOsr.Price)) + " Baht\n\n");
            sb.AppendLine("------------------------------");
            sb.AppendLine("       Total     " + String.Format("{0:n}", Int32.Parse(centralOsr.Price)) + " Baht");
            sb.AppendLine("\n");
            sb.AppendLine(" Thank you for using our service");
            sb.AppendLine("      Please come back again");
            sb.AppendLine("\n\n\n\n");
            sb.AppendLine("\x1b" + "\x69");
            //PrintDialog pd = new PrintDialog();
            //

            RawPrinterHelper.SendStringToPrinter(GlobalValue.Instance.receiptPrinter, sb.ToString());

            transactionLoadingGrid.Visibility = Visibility.Collapsed;
        }

        public void PrintEndDay()
        {
            Account acct = this.db.getAccountFromId(currentUseAccountId);
            List<OrderRecord> recordNumIncCancelled = this.db.getAllOrderRecord(currentUseAccountId);
            List<OrderRecord> recordNum = this.db.getAllOrderRecordExceptCancelled(currentUseAccountId);
            int totalCancelled = recordNumIncCancelled.Count() - recordNum.Count();

            int startMoney = Int32.Parse(acct.StartMoney);
            int totalSale = getTotalSale();
            int numStaff = currentStaffNo;
            int oil = currentStaffNo * GlobalValue.Instance.oilPrice;
            int commission = getTotalCommission();
            int numCustomer = recordNum.Count();
            int grandOthersales = getGrandTotalOtherRecord();

            int netBalance = (totalSale + oil + grandOthersales) - commission;

            var sb = new StringBuilder();
            sb.AppendLine("               " + currentBranchName);
            sb.AppendLine("            " + DateTime.Now.ToString("dd MMMM yyyy    HH:mm"));
            sb.AppendLine("                 <End Day Slip>");
            sb.AppendLine("    =========================================");
            sb.AppendLine("Initial Money");
            sb.AppendLine(String.Format("{0:n}", startMoney));
            sb.AppendLine("\n");
            sb.AppendLine("Total Sale");
            sb.AppendLine(String.Format("{0:n}", totalSale));
            sb.AppendLine("\n");
            sb.AppendLine("Total Staff");
            sb.AppendLine(numStaff.ToString());
            sb.AppendLine("\n");
            sb.AppendLine("Total Pax");
            sb.AppendLine(numCustomer.ToString());
            sb.AppendLine("\n");
            sb.AppendLine("Total Oil Income");
            sb.AppendLine(String.Format("{0:n}", oil));
            sb.AppendLine("\n");
            sb.AppendLine("Total Commission");
            sb.AppendLine(String.Format("{0:n}", commission));
            sb.AppendLine("\n");
            sb.AppendLine("Total Other Sale(Uniform, Tiger Balm, etc.)");
            sb.AppendLine(String.Format("{0:n}", grandOthersales));
            sb.AppendLine("\n");
            sb.AppendLine("                Balance Net");
            sb.AppendLine("              " + String.Format("{0:n}", netBalance) + " Baht");
            sb.AppendLine("\n");
            sb.AppendLine("---------------------------------------------");
            sb.AppendLine("Total Cancelled");
            sb.AppendLine(totalCancelled.ToString());
            sb.AppendLine("\n");
            sb.AppendLine("           Don't loose this ticket");
            sb.AppendLine("\n");
            sb.AppendLine("_____________________________________________");
            sb.AppendLine("\n\n\n");
            sb.AppendLine("\x1b" + "\x69");
            //PrintDialog pd = new PrintDialog();

            RawPrinterHelper.SendStringToPrinter(GlobalValue.Instance.commissionPrinter, sb.ToString());

            exportPDF();


        }

        public async void exportPDF()
        {
            List<Branch> listBranch = this.db.getAllBranch();

            // Create a new PDF document
            PdfDocument document = new PdfDocument();

            // Create an empty page
            PdfPage page = document.AddPage();
            page.Orientation = PageOrientation.Landscape;

            // Get an XGraphics object for drawing
            XGraphics gfx = XGraphics.FromPdfPage(page);

            //XRect rect = new XRect(0, 0, 250, 140);

            //XFont font = new XFont("Verdana", 10);
            //XBrush brush = XBrushes.Purple;


            XRect BigTitleRect = new XRect(0, 0, 800, 20);
            XRect TableHeaderRect = new XRect(10, 77, 770, 36);
            XRect TableFooterRect = new XRect(10, 517, 770, 12);

            XRect TableColumnRect_Date = new XRect(10, 77, 30, 452);
            XRect TableColumnRect_InitialMoney = new XRect(40, 77, 55, 452);
            XRect TableColumnRect_Total = new XRect(95, 77, 30, 452); 
            XRect TableColumnRect_Massage = new XRect(125, 77, 174, 452);
            XRect TableColumnRect_Massage_Cash = new XRect(125, 95, 62, 434);
            XRect TableColumnRect_Massage_Credit = new XRect(187, 95, 61, 434);
            XRect TableColumnRect_Massage_Voucher = new XRect(248, 95, 51, 434);
            XRect TableColumnRect_AveragePerPax = new XRect(299, 77, 47, 452);
            XRect TableColumnRect_TotalWorker = new XRect(346, 77, 34, 452);
            XRect TableColumnRect_OilIncome = new XRect(380, 77, 54, 452);
            XRect TableColumnRect_TotalIncome = new XRect(534, 77, 69, 452);//change position
            XRect TableColumnRect_PayWorker = new XRect(603, 77, 62, 452);//change position
            XRect TableColumnRect_TotalCancelled = new XRect(665, 77, 45, 452);//change position
            XRect TableColumnRect_TotalUniform = new XRect(434, 77, 46, 452);//change position
            XRect TableColumnRect_TotalTigerBalm = new XRect(480, 77, 54, 452);//change position
            XRect TableColumnRect_BalanceNet = new XRect(710, 77, 70, 452);

            XRect TableColumnRect_Date_Text_Header = new XRect(11, 90, 27, 520);
            XRect TableColumnRect_InitialMoney_Text_Header = new XRect(54, 90, 27, 520);
            XRect TableColumnRect_Total_Text_Header = new XRect(95, 90, 27, 520); 
            XRect TableColumnRect_Massage_Text_Header = new XRect(201, 80, 27, 520);
            XRect TableColumnRect_Massage_Cash_Text_Header = new XRect(147, 99, 14, 520);
            XRect TableColumnRect_Massage_Credit_Text_Header = new XRect(210, 99, 14, 520);
            XRect TableColumnRect_Massage_Voucher_Text_Header = new XRect(265, 99, 14, 520);
            XRect TableColumnRect_AveragePerPax_Text_Header = new XRect(308, 85, 27, 520);
            XRect TableColumnRect_AveragePerPax_2_Text_Header = new XRect(308, 95, 27, 520);
            XRect TableColumnRect_TotalWorker_Text_Header = new XRect(341, 85, 43, 520);
            XRect TableColumnRect_TotalWorker_2_Text_Header = new XRect(341, 95, 43, 520);
            XRect TableColumnRect_OilIncome_Text_Header = new XRect(390, 85, 33, 520);
            XRect TableColumnRect_OilIncome_2_Text_Header = new XRect(390, 95, 33, 520);
            XRect TableColumnRect_TotalIncome_Text_Header = new XRect(541, 90, 56, 520);//change position
            XRect TableColumnRect_PayWorker_Text_Header = new XRect(617, 90, 33, 520);//change position
            XRect TableColumnRect_TotalCancelled_Text_Header = new XRect(663, 90, 50, 520);//change position
            XRect TableColumnRect_TotalUniform_Text_Header = new XRect(433, 90, 50, 520);//change position
            XRect TableColumnRect_TotalTigerBalm_Text_Header = new XRect(483, 90, 50, 520);//change position
            XRect TableColumnRect_BalanceNet_Text_Header = new XRect(718, 90, 56, 520);

            XRect TableColumnRect_Date_Text = new XRect(11, 100, 27, 520);
            XRect TableColumnRect_InitialMoney_Text = new XRect(52, 100, 27, 520);
            XRect TableColumnRect_Total_Text = new XRect(90, 100, 27, 520); 
            XRect TableColumnRect_Massage_Text = new XRect(135, 100, 27, 520);
            XRect TableColumnRect_Massage_Credit_Text = new XRect(199, 100, 27, 520);
            XRect TableColumnRect_Massage_Voucher_Text = new XRect(259, 100, 27, 520);
            XRect TableColumnRect_AveragePerPax_Text = new XRect(306, 100, 27, 520);
            XRect TableColumnRect_TotalWorker_Text = new XRect(338, 100, 43, 520);
            XRect TableColumnRect_OilIncome_Text = new XRect(389, 100, 33, 520);
            XRect TableColumnRect_TotalIncome_Text = new XRect(535, 100, 56, 520);//change position
            XRect TableColumnRect_PayWorker_Text = new XRect(614, 100, 33, 520);//change position
            XRect TableColumnRect_TotalCancelled_Text = new XRect(664, 100, 50, 520);//change position
            XRect TableColumnRect_TotalUniform_Text = new XRect(438, 100, 50, 520);//change position
            XRect TableColumnRect_TotalTigerBalm_Text = new XRect(481, 100, 50, 520);//change position
            XRect TableColumnRect_BalanceNet_Text = new XRect(718, 100, 56, 520);

            XRect TableColumnRect_No = new XRect(10, 95, 27, 520);
            XRect TableColumnRect_Time = new XRect(10 + TableColumnRect_No.Width, 95, 50, 520);
            XRect TableColumnRect_Detail = new XRect(10 + TableColumnRect_No.Width + TableColumnRect_Time.Width, 95, 438, 520);
            XRect TableColumnRect_Price = new XRect(10 + TableColumnRect_No.Width + TableColumnRect_Time.Width + TableColumnRect_Detail.Width, 95, 75, 545);

            XRect TableColumnRect_No_Text = new XRect(10, 122, 27, 520);
            XRect TableColumnRect_Time_Text = new XRect(10 + TableColumnRect_No.Width, 122, 50, 520);
            XRect TableColumnRect_Detail_Text = new XRect(10 + TableColumnRect_No.Width + TableColumnRect_Time.Width, 122, 438, 520);
            XRect TableColumnRect_Price_Text = new XRect(10 + TableColumnRect_No.Width + TableColumnRect_Time.Width + TableColumnRect_Detail.Width, 122, 75, 545);

            //XRect TableFooterText_TotalSale = new XRect(10, 635, 590, 25);
            //XRect TableFooterText_TotalSaleValue = new XRect(10, 635, 590, 25);

            XFont BigTitleFont = new XFont("Verdana", 13);
            XFont HeaderContentFont = new XFont("Verdana", 10, XFontStyle.Underline);
            XFont ContentFont = new XFont("Verdana", 8);

            XBrush BlackBrush = XBrushes.Black;

            XStringFormat format = new XStringFormat();

            //gfx.DrawRectangle(XPens.YellowGreen, rect);
            gfx.DrawRectangle(XBrushes.LightGray, BigTitleRect);
            gfx.DrawRectangle(XPens.Black, BigTitleRect);
            //gfx.DrawLine(XPens.YellowGreen, rect.Width / 2, 0, rect.Width / 2, rect.Height);
            //gfx.DrawLine(XPens.YellowGreen, 0, rect.Height / 2, rect.Width, rect.Height / 2);
            Account curAcctx = this.db.getAccountFromId(currentUseAccountId);
            string acctxDate = curAcctx.Date;
            string[] splitAcctxDate = acctxDate.Split('-');
            MonthConvertor mc = new MonthConvertor();

            format.LineAlignment = XLineAlignment.Center;
            format.Alignment = XStringAlignment.Center;
            gfx.DrawString(currentBranchName, BigTitleFont, BlackBrush, BigTitleRect, format);
            gfx.DrawString("Daily Report", HeaderContentFont, BlackBrush, 20, 4 + HeaderContentFont.Height + BigTitleRect.Height);
            gfx.DrawString("Branch name : " + currentBranchName, ContentFont, BlackBrush, 20, 8 + HeaderContentFont.Height + BigTitleRect.Height + ContentFont.Height);
            gfx.DrawString("Month/Year : " + mc.calMonth(splitAcctxDate[1]) + " " + splitAcctxDate[0], ContentFont, BlackBrush, 20, 6 + HeaderContentFont.Height + BigTitleRect.Height + ContentFont.Height + ContentFont.Height);
            //gfx.DrawString("Branch name : " + currentBranchName, ContentFont, BlackBrush, 220, 8 + HeaderContentFont.Height + BigTitleRect.Height + ContentFont.Height);
            gfx.DrawString("Generate Date/Time : " + DateTime.Now.ToString("dd-MM-yyy HH:mm"), ContentFont, BlackBrush, 220, 6 + HeaderContentFont.Height + BigTitleRect.Height + ContentFont.Height + ContentFont.Height);

            gfx.DrawLine(XPens.Black, 0, 16 + HeaderContentFont.Height + BigTitleRect.Height + ContentFont.Height + ContentFont.Height, BigTitleRect.Width, 17 + HeaderContentFont.Height + BigTitleRect.Height + ContentFont.Height + ContentFont.Height);

            gfx.DrawRectangle(XBrushes.Yellow, TableHeaderRect);
            gfx.DrawRectangle(XPens.Black, TableHeaderRect);
            gfx.DrawRectangle(XBrushes.Yellow, TableFooterRect);

            //gfx.DrawRectangle(XBrushes.LightGray, TableFooterRect);
            //gfx.DrawRectangle(XPens.Black, TableFooterRect);

            gfx.DrawRectangle(XPens.Black, TableColumnRect_Date);
            gfx.DrawRectangle(XPens.Black, TableColumnRect_InitialMoney);
            gfx.DrawRectangle(XPens.Black, TableColumnRect_Total);
            gfx.DrawRectangle(XPens.Black, TableColumnRect_Massage);
            gfx.DrawRectangle(XPens.Black, TableColumnRect_Massage_Cash);
            gfx.DrawRectangle(XPens.Black, TableColumnRect_Massage_Credit);
            gfx.DrawRectangle(XPens.Black, TableColumnRect_Massage_Voucher);
            gfx.DrawRectangle(XPens.Black, TableColumnRect_AveragePerPax);
            gfx.DrawRectangle(XPens.Black, TableColumnRect_TotalWorker);
            gfx.DrawRectangle(XPens.Black, TableColumnRect_OilIncome);
            gfx.DrawRectangle(XPens.Black, TableColumnRect_TotalIncome);
            gfx.DrawRectangle(XPens.Black, TableColumnRect_PayWorker);
            gfx.DrawRectangle(XPens.Black, TableColumnRect_TotalCancelled);
            gfx.DrawRectangle(XPens.Black, TableColumnRect_TotalUniform);
            gfx.DrawRectangle(XPens.Black, TableColumnRect_TotalTigerBalm);
            gfx.DrawRectangle(XPens.Black, TableColumnRect_BalanceNet);

            format.LineAlignment = XLineAlignment.Near;
            format.Alignment = XStringAlignment.Center;

            gfx.DrawString("Date", ContentFont, BlackBrush, TableColumnRect_Date_Text_Header, format);
            gfx.DrawString("Start Money", ContentFont, BlackBrush, TableColumnRect_InitialMoney_Text_Header, format);
            gfx.DrawString("Pax", ContentFont, BlackBrush, TableColumnRect_Total_Text_Header, format);
            gfx.DrawString("Massage Amount", ContentFont, BlackBrush, TableColumnRect_Massage_Text_Header, format);
            gfx.DrawString("Cash", ContentFont, BlackBrush, TableColumnRect_Massage_Cash_Text_Header, format);
            gfx.DrawString("Credit", ContentFont, BlackBrush, TableColumnRect_Massage_Credit_Text_Header, format);
            gfx.DrawString("Voucher", ContentFont, BlackBrush, TableColumnRect_Massage_Voucher_Text_Header, format);
            gfx.DrawString("Average", ContentFont, BlackBrush, TableColumnRect_AveragePerPax_Text_Header, format);
            gfx.DrawString("/Pax", ContentFont, BlackBrush, TableColumnRect_AveragePerPax_2_Text_Header, format);
            gfx.DrawString("Total", ContentFont, BlackBrush, TableColumnRect_TotalWorker_Text_Header, format);
            gfx.DrawString("Worker", ContentFont, BlackBrush, TableColumnRect_TotalWorker_2_Text_Header, format);
            gfx.DrawString("Income", ContentFont, BlackBrush, TableColumnRect_OilIncome_Text_Header, format);
            gfx.DrawString(GlobalValue.Instance.oilPrice+"B/Staff", ContentFont, BlackBrush, TableColumnRect_OilIncome_2_Text_Header, format);
            gfx.DrawString("Total Incomes", ContentFont, BlackBrush, TableColumnRect_TotalIncome_Text_Header, format);
            gfx.DrawString("Pay Workers", ContentFont, BlackBrush, TableColumnRect_PayWorker_Text_Header, format);
            gfx.DrawString("Cancelled", ContentFont, BlackBrush, TableColumnRect_TotalCancelled_Text_Header, format);
            gfx.DrawString("Uniform", ContentFont, BlackBrush, TableColumnRect_TotalUniform_Text_Header, format);
            //gfx.DrawString("Tiger Balm", ContentFont, BlackBrush, TableColumnRect_TotalTigerBalm_Text_Header, format);
            gfx.DrawString("Other Sale", ContentFont, BlackBrush, TableColumnRect_TotalTigerBalm_Text_Header, format);
            gfx.DrawString("Balance Net", ContentFont, BlackBrush, TableColumnRect_BalanceNet_Text_Header, format);

            int y1 = 126;
            int y2 = 127;
            int plusYe = 23;

            for (int h = 0; h < 31; h++)
            {
                if (h == 0)
                {
                    gfx.DrawString(h + 1 + "", ContentFont, BlackBrush, TableColumnRect_Date_Text.X + 7, TableColumnRect_Date_Text.Y + plusYe);
                    gfx.DrawLine(XPens.Black, 10, y1, 780, y2);
                }
                else
                {
                    plusYe = plusYe + 13;
                    y1 = y1 + 13;
                    y2 = y2 + 13;
                    gfx.DrawString(h + 1 + "", ContentFont, BlackBrush, TableColumnRect_Date_Text.X + 7, TableColumnRect_Date_Text.Y + plusYe);
                    gfx.DrawLine(XPens.Black, 10, y1, 780, y2);
                }
            }

            List<Account> listAccount = this.db.getAccountLast40Records();
            List<DailyReportForm> allDailyForm = new List<DailyReportForm>();
            Account getLatestMonth = this.db.getLatestAcount();
            String[] sGetLatestMonth = getLatestMonth.Date.ToString().Split('-');
            //String[] s2GetLatestMonth = sGetLatestMonth[0].Split('/');
            DateTime getUseDateAddTmr = DateTime.Parse(getLatestMonth.Date).AddDays(1);
            string usingMonthAddTmr = getUseDateAddTmr.ToString("MM");

            if (listAccount.Count != 0)
            {
                //List<DailyReportForm> allDailyForm = new List<DailyReportForm>();
                for (int f = 0; f < listAccount.Count; f++)
                {
                    String[] s = listAccount[f].Date.ToString().Split('-');
                    //String[] s2 = s[0].Split('/');

                    //MessageBox.Show(s2[0] + "///" + curMonth + "===" + s2[2] + "///" + DateTime.Now.ToString("yyyy"));
                    if ((Int32.Parse(s[1]) == Int32.Parse(sGetLatestMonth[1])) && (Int32.Parse(s[0]) == Int32.Parse(sGetLatestMonth[0])))
                    {
                        int voucherCash = this.db.getAllDiscountWithCashFromAccountID(listAccount[f].Id);
                        int voucherCredit = this.db.getAllDiscountWithCreditFromAccountID(listAccount[f].Id);
                        int staff = Int32.Parse(listAccount[f].StaffAmount);
                        int oil = staff * GlobalValue.Instance.oilPrice;
                        int income = getTotalSaleFromId(listAccount[f].Id) - voucherCash;
                        int creditIncome = getTotalCreditSaleFromId(listAccount[f].Id) - voucherCredit;
                        int totalVoucher = voucherCash + voucherCredit;
                        int commis = getTotalCommissionFromId(listAccount[f].Id);
                        int pax = getTotalPaxFromId(listAccount[f].Id);
                        int grandIncome = income + creditIncome; //+ oil;
                        int averagePax = 0;
                        if (grandIncome != 0 && pax != 0)
                        {
                            averagePax = grandIncome / pax;
                        }
                        int uniform = getTotalUniformFromId(listAccount[f].Id);
                        //int tigerBalm = getTotalTigerBalmFromId(listAccount[f].Id);
                        int tigerBalm = getTotalTigerBalmWithOtherSaleFromId(listAccount[f].Id);
                        int finalIncome = grandIncome - commis + uniform + tigerBalm;
                        int totalCancelled = getTotalCancelledPaxFromId(listAccount[f].Id);
                        

                        DailyReportForm dailyForm = new DailyReportForm()
                        {
                            Date = s[2],
                            StartMoney = String.Format("{0:n}", Int32.Parse(listAccount[f].StartMoney)),
                            TotalPax = pax.ToString(),
                            MassageAmount = String.Format("{0:n}", income),
                            MassageCreditAmount = String.Format("{0:n}", creditIncome),
                            MassageVoucherAmount = String.Format("{0:n}", totalVoucher),
                            AveragePerPax = String.Format("{0:n}", averagePax),
                            TotalWorker = staff.ToString(),
                            OilAmount = String.Format("{0:n}", oil),
                            TotalIncome = String.Format("{0:n}", grandIncome),
                            PayWorkers = String.Format("{0:n}", commis),
                            TotalCancelled = totalCancelled.ToString(),
                            TotalUniform = String.Format("{0:n}", uniform),
                            TotalTigerBalm = String.Format("{0:n}", tigerBalm),
                            BalanceNet = String.Format("{0:n}", finalIncome)
                        };

                        allDailyForm.Add(dailyForm);
                    }

                }

                int plusY = 23;

                for (int h = 0; h < 31; h++)
                {
                    for (int j = 0; j < allDailyForm.Count; j++)
                    {
                        int a = h + 1;
                        if (a == Int32.Parse(allDailyForm[j].Date))
                        {
                            //gfx.DrawString(h + 1 + "", ContentFont, BlackBrush, TableColumnRect_Date_Text.X + 7, TableColumnRect_Date_Text.Y + plusY);
                            gfx.DrawString(allDailyForm[j].StartMoney, ContentFont, BlackBrush, TableColumnRect_InitialMoney_Text.X - 4, TableColumnRect_InitialMoney_Text.Y + plusY); //edit on 3 Nov 2019
                            gfx.DrawString(allDailyForm[j].TotalPax, ContentFont, BlackBrush, TableColumnRect_Total_Text.X + 8, TableColumnRect_Total_Text.Y + plusY);
                            gfx.DrawString(allDailyForm[j].MassageAmount, ContentFont, BlackBrush, TableColumnRect_Massage_Text.X - 4, TableColumnRect_Massage_Text.Y + plusY);
                            gfx.DrawString(allDailyForm[j].MassageCreditAmount, ContentFont, BlackBrush, TableColumnRect_Massage_Credit_Text.X - 4, TableColumnRect_Massage_Credit_Text.Y + plusY);
                            gfx.DrawString(allDailyForm[j].MassageVoucherAmount, ContentFont, BlackBrush, TableColumnRect_Massage_Voucher_Text.X - 4, TableColumnRect_Massage_Voucher_Text.Y + plusY);
                            gfx.DrawString(allDailyForm[j].AveragePerPax, ContentFont, BlackBrush, TableColumnRect_AveragePerPax_Text.X, TableColumnRect_AveragePerPax_Text.Y + plusY);
                            gfx.DrawString(allDailyForm[j].TotalWorker, ContentFont, BlackBrush, TableColumnRect_TotalWorker_Text.X + 14, TableColumnRect_TotalWorker_Text.Y + plusY);
                            gfx.DrawString(allDailyForm[j].OilAmount, ContentFont, BlackBrush, TableColumnRect_OilIncome_Text.X, TableColumnRect_OilIncome_Text.Y + plusY);
                            gfx.DrawString(allDailyForm[j].TotalIncome, ContentFont, BlackBrush, TableColumnRect_TotalIncome_Text.X + 8, TableColumnRect_TotalIncome_Text.Y + plusY);
                            gfx.DrawString(allDailyForm[j].PayWorkers, ContentFont, BlackBrush, TableColumnRect_PayWorker_Text.X - 3, TableColumnRect_PayWorker_Text.Y + plusY);
                            gfx.DrawString(allDailyForm[j].TotalCancelled, ContentFont, BlackBrush, TableColumnRect_TotalCancelled_Text.X + 15, TableColumnRect_TotalCancelled_Text.Y + plusY);
                            gfx.DrawString(allDailyForm[j].TotalUniform, ContentFont, BlackBrush, TableColumnRect_TotalUniform_Text.X + 5, TableColumnRect_TotalUniform_Text.Y + plusY);
                            gfx.DrawString(allDailyForm[j].TotalTigerBalm, ContentFont, BlackBrush, TableColumnRect_TotalTigerBalm_Text.X + 10, TableColumnRect_TotalTigerBalm_Text.Y + plusY);
                            gfx.DrawString(allDailyForm[j].BalanceNet, ContentFont, BlackBrush, TableColumnRect_BalanceNet_Text.X + 6, TableColumnRect_BalanceNet_Text.Y + plusY); //edit on 3 Nov 2019

                        }


                    }

                    plusY = plusY + 13;
                    //if (h == 0)
                    //{
                    //    gfx.DrawString(h + 1 + "", ContentFont, BlackBrush, TableColumnRect_Date_Text.X + 7, TableColumnRect_Date_Text.Y + plusY);
                    //    gfx.DrawString("6,000.00", ContentFont, BlackBrush, TableColumnRect_InitialMoney_Text.X, TableColumnRect_InitialMoney_Text.Y + plusY);
                    //    gfx.DrawString("125", ContentFont, BlackBrush, TableColumnRect_Total_Text.X + 4, TableColumnRect_Total_Text.Y + plusY);
                    //    gfx.DrawString("44,500.00", ContentFont, BlackBrush, TableColumnRect_Massage_Text.X - 4, TableColumnRect_Massage_Text.Y + plusY);
                    //    gfx.DrawString("300.00", ContentFont, BlackBrush, TableColumnRect_AveragePerPax_Text.X, TableColumnRect_AveragePerPax_Text.Y + plusY);
                    //    gfx.DrawString("30", ContentFont, BlackBrush, TableColumnRect_TotalWorker_Text.X + 10, TableColumnRect_TotalWorker_Text.Y + plusY);
                    //    gfx.DrawString("600.00", ContentFont, BlackBrush, TableColumnRect_OilIncome_Text.X, TableColumnRect_OilIncome_Text.Y + plusY);
                    //    gfx.DrawString("45,100.00", ContentFont, BlackBrush, TableColumnRect_TotalIncome_Text.X + 8, TableColumnRect_TotalIncome_Text.Y + plusY);
                    //    gfx.DrawString("20,000.00", ContentFont, BlackBrush, TableColumnRect_PayWorker_Text.X - 3, TableColumnRect_PayWorker_Text.Y + plusY);
                    //    gfx.DrawString("25,100.00", ContentFont, BlackBrush, TableColumnRect_BalanceNet_Text.X, TableColumnRect_BalanceNet_Text.Y + plusY);
                    //}
                    //else
                    //{
                    //    plusY = plusY + 13;

                    //    gfx.DrawString(h + 1 + "", ContentFont, BlackBrush, TableColumnRect_Date_Text.X + 7, TableColumnRect_Date_Text.Y + plusY);
                    //    gfx.DrawString("6,000.00", ContentFont, BlackBrush, TableColumnRect_InitialMoney_Text.X, TableColumnRect_InitialMoney_Text.Y + plusY);
                    //    gfx.DrawString("125", ContentFont, BlackBrush, TableColumnRect_Total_Text.X + 4, TableColumnRect_Total_Text.Y + plusY);
                    //    gfx.DrawString("44,500.00", ContentFont, BlackBrush, TableColumnRect_Massage_Text.X - 4, TableColumnRect_Massage_Text.Y + plusY);
                    //    gfx.DrawString("300.00", ContentFont, BlackBrush, TableColumnRect_AveragePerPax_Text.X, TableColumnRect_AveragePerPax_Text.Y + plusY);
                    //    gfx.DrawString("30", ContentFont, BlackBrush, TableColumnRect_TotalWorker_Text.X + 10, TableColumnRect_TotalWorker_Text.Y + plusY);
                    //    gfx.DrawString("600.00", ContentFont, BlackBrush, TableColumnRect_OilIncome_Text.X, TableColumnRect_OilIncome_Text.Y + plusY);
                    //    gfx.DrawString("45,100.00", ContentFont, BlackBrush, TableColumnRect_TotalIncome_Text.X + 8, TableColumnRect_TotalIncome_Text.Y + plusY);
                    //    gfx.DrawString("20,000.00", ContentFont, BlackBrush, TableColumnRect_PayWorker_Text.X - 3, TableColumnRect_PayWorker_Text.Y + plusY);
                    //    gfx.DrawString("25,100.00", ContentFont, BlackBrush, TableColumnRect_BalanceNet_Text.X, TableColumnRect_BalanceNet_Text.Y + plusY);
                    //}
                }
            }

            int netTotalPax = 0;
            int netMassageAmount = 0;
            int netMassageCreditAmount = 0;
            int netVoucherAmount = 0;
            int netAveragePerPax = 0;
            int netTotalWorker = 0;
            int netOil = 0;
            int netTotalIncome = 0;
            int netCommis = 0;
            int netCancelledPax = 0;
            int netUniform = 0;
            int netTigerBalm = 0;
            int netBalanceNet = 0;
            for (int k = 0; k < allDailyForm.Count; k++)
            {
                string convertTotalPax = allDailyForm[k].TotalPax.Replace(".00", "");
                string convertMassageAmount = allDailyForm[k].MassageAmount.Replace(".00", "");
                string convertMassageCreditAmount = allDailyForm[k].MassageCreditAmount.Replace(".00", "");
                string convertMassageVoucherAmount = allDailyForm[k].MassageVoucherAmount.Replace(".00", "");
                string convertAveragePerPax = allDailyForm[k].AveragePerPax.Replace(".00", "");
                string convertTotalWorker = allDailyForm[k].TotalWorker.Replace(".00", "");
                string convertOilAmount = allDailyForm[k].OilAmount.Replace(".00", "");
                string convertTotalIncome = allDailyForm[k].TotalIncome.Replace(".00", "");
                string convertCommis = allDailyForm[k].PayWorkers.Replace(".00", "");
                string convertCancelledPax = allDailyForm[k].TotalCancelled.Replace(".00", "");
                string convertUniform = allDailyForm[k].TotalUniform.Replace(".00", "");
                string convertTigerBalm = allDailyForm[k].TotalTigerBalm.Replace(".00", "");
                string convertBalance = allDailyForm[k].BalanceNet.Replace(".00", "");

                string convertTotalPaxs = convertTotalPax.Replace(",", "");
                string convertMassageAmounts = convertMassageAmount.Replace(",", "");
                string convertMassageCreditAmounts = convertMassageCreditAmount.Replace(",", "");
                string convertMassageVoucherAmounts = convertMassageVoucherAmount.Replace(",", "");
                string convertAveragePerPaxs = convertAveragePerPax.Replace(",", "");
                string convertTotalWorkers = convertTotalWorker.Replace(",", "");
                string convertOilAmounts = convertOilAmount.Replace(",", "");
                string convertTotalIncomes = convertTotalIncome.Replace(",", "");
                string convertCommiss = convertCommis.Replace(",", "");
                string convertCancelledPaxs = convertCancelledPax.Replace(",", "");
                string convertUniforms = convertUniform.Replace(",", "");
                string convertTigerBalms = convertTigerBalm.Replace(",", "");
                string convertBalances = convertBalance.Replace(",", "");

                netTotalPax += Int32.Parse(convertTotalPaxs);
                netMassageAmount += Int32.Parse(convertMassageAmounts);
                netMassageCreditAmount += Int32.Parse(convertMassageCreditAmounts);
                netVoucherAmount += Int32.Parse(convertMassageVoucherAmounts);
                //netAveragePerPax += Int32.Parse(convertAveragePerPaxs);
                netTotalWorker += Int32.Parse(convertTotalWorkers);
                netOil += Int32.Parse(convertOilAmounts);
                netTotalIncome += Int32.Parse(convertTotalIncomes);
                netCommis += Int32.Parse(convertCommiss);
                netCancelledPax += Int32.Parse(convertCancelledPaxs);
                netUniform += Int32.Parse(convertUniforms);
                netTigerBalm += Int32.Parse(convertTigerBalms);
                netBalanceNet += Int32.Parse(convertBalances);
            }

            //Updated on 04 October 2022
            netAveragePerPax = (netTotalIncome / netTotalPax);

            gfx.DrawString("Total", ContentFont, BlackBrush, TableColumnRect_Date_Text.X + 7, TableColumnRect_Date_Text.Y + 426);
            //gfx.DrawString("6,000.00", ContentFont, BlackBrush, TableColumnRect_InitialMoney_Text.X, TableColumnRect_InitialMoney_Text.Y + 426);
            gfx.DrawString(String.Format("{0:n0}", netTotalPax), ContentFont, BlackBrush, TableColumnRect_Total_Text.X + 8, TableColumnRect_Total_Text.Y + 426);
            gfx.DrawString(String.Format("{0:n}", netMassageAmount), ContentFont, BlackBrush, TableColumnRect_Massage_Text.X - 4, TableColumnRect_Massage_Text.Y + 426);
            gfx.DrawString(String.Format("{0:n}", netMassageCreditAmount), ContentFont, BlackBrush, TableColumnRect_Massage_Credit_Text.X - 4, TableColumnRect_Massage_Credit_Text.Y + 426);
            gfx.DrawString(String.Format("{0:n}", netVoucherAmount), ContentFont, BlackBrush, TableColumnRect_Massage_Voucher_Text.X - 4, TableColumnRect_Massage_Voucher_Text.Y + 426);
            gfx.DrawString(String.Format("{0:n}", netAveragePerPax), ContentFont, BlackBrush, TableColumnRect_AveragePerPax_Text.X - 3, TableColumnRect_AveragePerPax_Text.Y + 426);
            gfx.DrawString(netTotalWorker.ToString(), ContentFont, BlackBrush, TableColumnRect_TotalWorker_Text.X + 15, TableColumnRect_TotalWorker_Text.Y + 426);
            gfx.DrawString(String.Format("{0:n}", netOil), ContentFont, BlackBrush, TableColumnRect_OilIncome_Text.X, TableColumnRect_OilIncome_Text.Y + 426);
            gfx.DrawString(String.Format("{0:n}", netTotalIncome), ContentFont, BlackBrush, TableColumnRect_TotalIncome_Text.X + 8, TableColumnRect_TotalIncome_Text.Y + 426);
            gfx.DrawString(String.Format("{0:n}", netCommis), ContentFont, BlackBrush, TableColumnRect_PayWorker_Text.X - 5, TableColumnRect_PayWorker_Text.Y + 426);
            gfx.DrawString(netCancelledPax.ToString(), ContentFont, BlackBrush, TableColumnRect_TotalCancelled_Text.X + 20, TableColumnRect_TotalCancelled_Text.Y + 426);
            gfx.DrawString(String.Format("{0:n}", netUniform), ContentFont, BlackBrush, TableColumnRect_TotalUniform_Text.X + 5, TableColumnRect_TotalUniform_Text.Y + 426);
            gfx.DrawString(String.Format("{0:n}", netTigerBalm), ContentFont, BlackBrush, TableColumnRect_TotalTigerBalm_Text.X + 10, TableColumnRect_TotalTigerBalm_Text.Y + 426);
            gfx.DrawString(String.Format("{0:n}", netBalanceNet), ContentFont, BlackBrush, TableColumnRect_BalanceNet_Text.X + 5, TableColumnRect_BalanceNet_Text.Y + 426);
            gfx.DrawLine(XPens.Black, 10, 529, 780, 530);
            //MessageBox.Show(dateStamp.ToString()+"//"+dateStamp.ToLongDateString());


            string fullDate = new DateTime(Int32.Parse(sGetLatestMonth[0]), Int32.Parse(sGetLatestMonth[1]), Int32.Parse(sGetLatestMonth[2])).ToString("ddMMMMyyyy");
            //string[] longDate = dateStamp.ToLongDateString().Split(' ');
            //string preReal = longDate[2] + longDate[1] + longDate[3];
            //string realDate = preReal.Replace(",", "");
            filename = @"C:\SpaSystem\report" + fullDate + ".pdf";

            //test
            document.Save(filename);

            //Process.Start(filename);

            //**For test pdf do not forget to uncomment these**
            try
            {
                string curDateTime = getCurDateTime();

                Account getLastAc = this.db.getLatestAcount();
                Account newAc = new Account()
                {
                    Id = getLastAc.Id,
                    Date = getLastAc.Date,
                    Time = getLastAc.Time,
                    StartMoney = getLastAc.StartMoney,
                    StaffAmount = getLastAc.StaffAmount,
                    Completed = "true",
                    SendStatus = getLastAc.SendStatus,
                    UpdateStatus = getLastAc.UpdateStatus,
                    CreateDateTime = getLastAc.CreateDateTime,
                    UpdateDateTime = curDateTime
                };

                this.db.updateAcount(newAc);

                loadingGrid.Visibility = Visibility.Visible;
                //loadingTxt.Text = "Computer กำลังปิด โปรดรอสักครู่...";

                await Task.Delay(2000);
                string curDT = DateTime.Now.ToString("MM");
                int curMonth = Int32.Parse(curDT);
                //string nextDT = DateTime.Now.AddDays(1).ToString("MM");
                //int nextDayMonth = Int32.Parse(nextDT);
                int useMonth = Int32.Parse(sGetLatestMonth[1]);
                int useMonthPlus1Day = Int32.Parse(usingMonthAddTmr);

                if (useMonth != curMonth)
                {
                    exportPDF25Detail();
                }
                else
                {
                    if(useMonth != useMonthPlus1Day)
                    {
                        exportPDF25Detail();
                    }
                    else
                    {
                        if (GlobalValue.Instance.report100.Equals("false"))
                        {
                            Application.Current.Shutdown();

                            //test***********
                            var psi = new ProcessStartInfo("shutdown", "/s /t 0");
                            psi.CreateNoWindow = true;
                            psi.UseShellExecute = false;
                            Process.Start(psi);
                        }
                        else
                        {
                            MailMessage mail = new MailMessage();
                            SmtpClient SmtpServer = new SmtpClient(GlobalValue.Instance.emailServer);
                            mail.From = new MailAddress(GlobalValue.Instance.senderEmail);
                            String[] receiverSet = this.db.getCurrentReceiverEmail().Value.Split('/');
                            for (int i = 0; i < receiverSet.Length; i++)
                            {
                                mail.To.Add(receiverSet[i]);
                            }
                            //mail.To.Add("pascal_tober@hotmail.com");
                            //mail.To.Add("armaz@hotmail.fr");
                            //mail.To.Add("nit_sisuwan@hotmail.fr");
                            //mail.To.Add("panuwatdethfung@outlook.com");
                            //mail.To.Add("fa9fa9fa9@gmail.com");
                            //mail.To.Add("t.jaturong@outlook.com");
                            mail.Subject = currentBranchName + " - Daily Report(" + fullDate + ")";
                            mail.Body = "This daily report email is auto sent by Spa POS Program (" + currentBranchName + ")";

                            Attachment attachment;
                            attachment = new Attachment(filename);

                            mail.Attachments.Add(attachment);

                            SmtpServer.Port = GlobalValue.Instance.serverPort;
                            SmtpServer.Credentials = new NetworkCredential(GlobalValue.Instance.serverUsername, GlobalValue.Instance.serverPassword);
                            SmtpServer.EnableSsl = true;

                            SmtpServer.Send(mail);

                            ////////////////////////////////////////////////////////////////////////////////////////////////////////
                            //Private send DB to Jaturong
                            //MailMessage _mail = new MailMessage();
                            //SmtpClient _SmtpServer = new SmtpClient("smtp.sendgrid.net");
                            //_mail.From = new MailAddress("jaturong@24dvlop.com");
                            //_mail.To.Add("t.jaturong@outlook.com");
                            //_mail.Subject = currentBranchName + " - Master DB(" + fullDate + ")";
                            //_mail.Body = "This daily master DB by Spa POS program (" + currentBranchName + ")";

                            //Attachment _attachment;
                            //_attachment = new Attachment(filename);

                            //_mail.Attachments.Add(_attachment);

                            //_SmtpServer.Port = 587;
                            //_SmtpServer.Credentials = new NetworkCredential("apikey", "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");
                            ////_SmtpServer.EnableSsl = true;

                            //_SmtpServer.Send(_mail);
                            ////////////////////////////////////////////////////////////////////////////////////////////////////////

                            Application.Current.Shutdown();

                            //test***********
                            var psi = new ProcessStartInfo("shutdown", "/s /t 0");
                            psi.CreateNoWindow = true;
                            psi.UseShellExecute = false;
                            Process.Start(psi);
                        }
                    }
                    
                    
                }
                //test

                //MailMessage mail = new MailMessage();
                //SmtpClient SmtpServer = new SmtpClient(GlobalValue.Instance.emailServer);
                //mail.From = new MailAddress(GlobalValue.Instance.senderEmail);
                //mail.To.Add("pascal_tober@hotmail.com");
                //mail.To.Add("armaz@hotmail.fr");
                //mail.To.Add("nit_sisuwan@hotmail.fr");
                //mail.To.Add("siree941@gmail.com");
                //mail.To.Add("t.jaturong@outlook.com");
                //mail.Subject = currentBranchName + " - Daily Report(" + fullDate + ")";
                //mail.Body = "This daily report email is auto sent by SpaSystem program (" + currentBranchName + ")";

                //Attachment attachment;
                //attachment = new Attachment(filename);

                //mail.Attachments.Add(attachment);

                //SmtpServer.Port = GlobalValue.Instance.serverPort;
                //SmtpServer.Credentials = new NetworkCredential(GlobalValue.Instance.serverUsername, GlobalValue.Instance.serverPassword);
                //SmtpServer.EnableSsl = true;

                //SmtpServer.Send(mail);
                //MessageBox.Show("Email is sent\nEmail ถูกส่งเรียบร้อย");

                //Application.Current.Shutdown();

                ////test
                //var psi = new ProcessStartInfo("shutdown", "/s /t 0");
                //psi.CreateNoWindow = true;
                //psi.UseShellExecute = false;
                //Process.Start(psi);


            }
            catch (Exception pp)
            {
                //MessageBox.Show(pp.ToString());
                MessageBox.Show("ไม่สามารถส่ง Email ได้เนื่องจากไม่มี Internet กรุณาติดต่อผู้ดูแลระบบ");

                //exportPDF25Detail();
                Application.Current.Shutdown();

                //test
                var psi = new ProcessStartInfo("shutdown", "/s /t 0");
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                Process.Start(psi);


            }
            //Process.Start(filename);

        }

        public async void exportPDF25Detail()
        {
            List<Branch> listBranch = this.db.getAllBranch();

            // Create a new PDF document
            PdfDocument document = new PdfDocument();

            // Create an empty page
            PdfPage page = document.AddPage();
            page.Size = PageSize.A3;
            page.Orientation = PageOrientation.Landscape;

            // Get an XGraphics object for drawing
            XGraphics gfx = XGraphics.FromPdfPage(page);

            //XRect rect = new XRect(0, 0, 250, 140);

            //XFont font = new XFont("Verdana", 10);
            //XBrush brush = XBrushes.Purple;


            XRect BigTitleRect = new XRect(0, 0, 800, 34);
            XRect TableHeaderRect = new XRect(10, 95, 770, 18);
            XRect TableFooterRect = new XRect(10, 783, 770, 12);

            XRect TableColumnRect_Date = new XRect(10, 95, 460, 700);
            XRect TableColumnRect_InitialMoney = new XRect(470, 95, 155, 700);
            XRect TableColumnRect_Total = new XRect(625, 95, 155, 700);
            //XRect TableColumnRect_Massage = new XRect(400, 95, 70, 434);
            //XRect TableColumnRect_AveragePerPax = new XRect(470, 95, 70, 434);
            //XRect TableColumnRect_TotalWorker = new XRect(540, 95, 70, 434);
            //XRect TableColumnRect_OilIncome = new XRect(610, 95, 70, 434);
            //XRect TableColumnRect_TotalIncome = new XRect(680, 95, 100, 434);
            //XRect TableColumnRect_PayWorker = new XRect(517, 95, 65, 434);
            //XRect TableColumnRect_TotalCancelled = new XRect(582, 95, 82, 434);
            //XRect TableColumnRect_BalanceNet = new XRect(664, 95, 116, 434);

            XRect TableColumnRect_Date_Text = new XRect(11, 100, 460, 520);
            XRect TableColumnRect_InitialMoney_Text = new XRect(470, 100, 155, 520);
            XRect TableColumnRect_Total_Text = new XRect(625, 100, 155, 520);
            //XRect TableColumnRect_Date_Text = new XRect(11, 100, 250, 520);
            //XRect TableColumnRect_InitialMoney_Text = new XRect(260, 100, 70, 520);
            //XRect TableColumnRect_Total_Text = new XRect(330, 100, 70, 520);
            //XRect TableColumnRect_Massage_Text = new XRect(400, 100, 70, 520);
            //XRect TableColumnRect_AveragePerPax_Text = new XRect(470, 100, 70, 520);
            //XRect TableColumnRect_TotalWorker_Text = new XRect(540, 100, 70, 520);
            //XRect TableColumnRect_OilIncome_Text = new XRect(610, 100, 70, 520);
            //XRect TableColumnRect_TotalIncome_Text = new XRect(680, 100, 100, 520);
            //XRect TableColumnRect_PayWorker_Text = new XRect(534, 100, 33, 520);
            //XRect TableColumnRect_TotalCancelled_Text = new XRect(595, 100, 56, 520);
            //XRect TableColumnRect_BalanceNet_Text = new XRect(690, 100, 56, 520);

            XRect TableColumnRect_No = new XRect(10, 95, 27, 520);
            XRect TableColumnRect_Time = new XRect(10 + TableColumnRect_No.Width, 95, 50, 520);
            XRect TableColumnRect_Detail = new XRect(10 + TableColumnRect_No.Width + TableColumnRect_Time.Width, 95, 438, 520);
            XRect TableColumnRect_Price = new XRect(10 + TableColumnRect_No.Width + TableColumnRect_Time.Width + TableColumnRect_Detail.Width, 95, 75, 545);

            XRect TableColumnRect_No_Text = new XRect(10, 122, 27, 520);
            XRect TableColumnRect_Time_Text = new XRect(10 + TableColumnRect_No.Width, 122, 50, 520);
            XRect TableColumnRect_Detail_Text = new XRect(10 + TableColumnRect_No.Width + TableColumnRect_Time.Width, 122, 438, 520);
            XRect TableColumnRect_Price_Text = new XRect(10 + TableColumnRect_No.Width + TableColumnRect_Time.Width + TableColumnRect_Detail.Width, 122, 75, 545);

            //XRect TableFooterText_TotalSale = new XRect(10, 635, 590, 25);
            //XRect TableFooterText_TotalSaleValue = new XRect(10, 635, 590, 25);

            XFont BigTitleFont = new XFont("Verdana", 14);
            XFont HeaderContentFont = new XFont("Verdana", 10, XFontStyle.Underline);
            XFont ContentFont = new XFont("Verdana", 8);

            XBrush BlackBrush = XBrushes.Black;

            XStringFormat format = new XStringFormat();

            //gfx.DrawRectangle(XPens.YellowGreen, rect);
            gfx.DrawRectangle(XBrushes.LightGray, BigTitleRect);
            gfx.DrawRectangle(XPens.Black, BigTitleRect);
            //gfx.DrawLine(XPens.YellowGreen, rect.Width / 2, 0, rect.Width / 2, rect.Height);
            //gfx.DrawLine(XPens.YellowGreen, 0, rect.Height / 2, rect.Width, rect.Height / 2);
            Account curAcctx = this.db.getAccountFromId(currentUseAccountId);
            string acctxDate = curAcctx.Date;
            string[] splitAcctxDate = acctxDate.Split('-');
            MonthConvertor mc = new MonthConvertor();


            format.LineAlignment = XLineAlignment.Center;
            format.Alignment = XStringAlignment.Center;
            gfx.DrawString(currentBranchName, BigTitleFont, BlackBrush, BigTitleRect, format);
            gfx.DrawString("Daily Report (Detail)", HeaderContentFont, BlackBrush, 20, 4 + HeaderContentFont.Height + BigTitleRect.Height);
            gfx.DrawString("Branch name : " + currentBranchName, ContentFont, BlackBrush, 20, 8 + HeaderContentFont.Height + BigTitleRect.Height + ContentFont.Height);
            gfx.DrawString("Month/Year : " + mc.calMonth(splitAcctxDate[1]) + " " + splitAcctxDate[0], ContentFont, BlackBrush, 20, 6 + HeaderContentFont.Height + BigTitleRect.Height + ContentFont.Height + ContentFont.Height);
            //gfx.DrawString("Branch name : " + currentBranchName, ContentFont, BlackBrush, 220, 8 + HeaderContentFont.Height + BigTitleRect.Height + ContentFont.Height);
            gfx.DrawString("Generate Date/Time : " + DateTime.Now.ToString("dd-MM-yyy HH:mm"), ContentFont, BlackBrush, 220, 6 + HeaderContentFont.Height + BigTitleRect.Height + ContentFont.Height + ContentFont.Height);

            gfx.DrawLine(XPens.Black, 0, 16 + HeaderContentFont.Height + BigTitleRect.Height + ContentFont.Height + ContentFont.Height, BigTitleRect.Width, 17 + HeaderContentFont.Height + BigTitleRect.Height + ContentFont.Height + ContentFont.Height);

            gfx.DrawRectangle(XBrushes.Yellow, TableHeaderRect);
            gfx.DrawRectangle(XPens.Black, TableHeaderRect);
            gfx.DrawRectangle(XBrushes.Yellow, TableFooterRect);

            //gfx.DrawRectangle(XBrushes.LightGray, TableFooterRect);
            //gfx.DrawRectangle(XPens.Black, TableFooterRect);

            gfx.DrawRectangle(XPens.Black, TableColumnRect_Date);
            gfx.DrawRectangle(XPens.Black, TableColumnRect_InitialMoney);
            gfx.DrawRectangle(XPens.Black, TableColumnRect_Total);
            //gfx.DrawRectangle(XPens.Black, TableColumnRect_Massage);
            //gfx.DrawRectangle(XPens.Black, TableColumnRect_AveragePerPax);
            //gfx.DrawRectangle(XPens.Black, TableColumnRect_TotalWorker);
            //gfx.DrawRectangle(XPens.Black, TableColumnRect_OilIncome);
            //gfx.DrawRectangle(XPens.Black, TableColumnRect_TotalIncome);
            //gfx.DrawRectangle(XPens.Black, TableColumnRect_PayWorker);
            //gfx.DrawRectangle(XPens.Black, TableColumnRect_TotalCancelled);
            //gfx.DrawRectangle(XPens.Black, TableColumnRect_BalanceNet);

            format.LineAlignment = XLineAlignment.Near;
            format.Alignment = XStringAlignment.Center;

            gfx.DrawString("Massage Type", ContentFont, BlackBrush, TableColumnRect_Date_Text, format);
            gfx.DrawString("Total Pax", ContentFont, BlackBrush, TableColumnRect_InitialMoney_Text, format);
            gfx.DrawString("Total Sales", ContentFont, BlackBrush, TableColumnRect_Total_Text, format);
            //gfx.DrawString("1 h 30", ContentFont, BlackBrush, TableColumnRect_Massage_Text, format);
            //gfx.DrawString("2 h", ContentFont, BlackBrush, TableColumnRect_AveragePerPax_Text, format);
            //gfx.DrawString("2 h 30", ContentFont, BlackBrush, TableColumnRect_TotalWorker_Text, format);
            //gfx.DrawString("+30", ContentFont, BlackBrush, TableColumnRect_OilIncome_Text, format);
            //gfx.DrawString("Total", ContentFont, BlackBrush, TableColumnRect_TotalIncome_Text, format);
            //gfx.DrawString("Pay Workers", ContentFont, BlackBrush, TableColumnRect_PayWorker_Text, format);
            //gfx.DrawString("Total Cancelled Pax", ContentFont, BlackBrush, TableColumnRect_TotalCancelled_Text, format);
            //gfx.DrawString("Balance Net", ContentFont, BlackBrush, TableColumnRect_BalanceNet_Text, format);

            int y1 = 126;
            int y2 = 127;
            int plusYe = 23;

            //Backup code

            //for (int h = 0; h < globalListMassageTopic.Count(); h++)
            //{
            //    if (h == 0)
            //    {
            //        gfx.DrawString(globalListMassageTopic[h].Name, ContentFont, BlackBrush, TableColumnRect_Date_Text.X + 7, TableColumnRect_Date_Text.Y + plusYe);
            //        gfx.DrawLine(XPens.Black, 10, y1, 780, y2);
            //    }
            //    else
            //    {
            //        plusYe = plusYe + 13;
            //        y1 = y1 + 13;
            //        y2 = y2 + 13;
            //        gfx.DrawString(globalListMassageTopic[h].Name, ContentFont, BlackBrush, TableColumnRect_Date_Text.X + 7, TableColumnRect_Date_Text.Y + plusYe);
            //        gfx.DrawLine(XPens.Black, 10, y1, 780, y2);
            //    }
            //}

            //Update 07092022
            //for (int h = 0; h < globalListMassageTopic.Count(); h++)
            //{
            //    if(globalListMassageTopic[h].Name.Contains("Package")||globalListMassageTopic[h].Name.Contains("Promotion"))
            //    {
            //        List<MassageSet> msgPlanFromPackage = this.db.getMassagePlanFromTopic(globalListMassageTopic[h].Id);
            //        for (int j = 0; j < msgPlanFromPackage.Count(); j++)
            //        {
            //            string msgPlanNameFromPackage = this.db.getMassagePlanName(msgPlanFromPackage[j].MassagePlanId);

            //            plusYe = plusYe + 13;
            //            y1 = y1 + 13;
            //            y2 = y2 + 13;
            //            gfx.DrawString(globalListMassageTopic[h].Name +" - "+ msgPlanNameFromPackage, ContentFont, BlackBrush, TableColumnRect_Date_Text.X + 7, TableColumnRect_Date_Text.Y + plusYe);
            //            gfx.DrawLine(XPens.Black, 10, y1, 780, y2);
            //        }

            //    }
            //    else
            //    {
            //        if (h == 0)
            //        {
            //            gfx.DrawString(globalListMassageTopic[h].Name, ContentFont, BlackBrush, TableColumnRect_Date_Text.X + 7, TableColumnRect_Date_Text.Y + plusYe);
            //            gfx.DrawLine(XPens.Black, 10, y1, 780, y2);
            //        }
            //        else
            //        {
            //            plusYe = plusYe + 13;
            //            y1 = y1 + 13;
            //            y2 = y2 + 13;
            //            gfx.DrawString(globalListMassageTopic[h].Name, ContentFont, BlackBrush, TableColumnRect_Date_Text.X + 7, TableColumnRect_Date_Text.Y + plusYe);
            //            gfx.DrawLine(XPens.Black, 10, y1, 780, y2);
            //        }
            //    }
                
            //}

            List<Account> listAccount = this.db.getAccountLast40Records();
            List<DailyReportForm> allDailyForm = new List<DailyReportForm>();
            Account getLatestMonth = this.db.getLatestAcount();
            String[] sGetLatestMonth = getLatestMonth.Date.ToString().Split('-');
            List<OrderRecord> mainOrderRecord = new List<OrderRecord>();
            //String[] s2GetLatestMonth = sGetLatestMonth[0].Split('/');

            if (listAccount.Count != 0)
            {
                //List<DailyReportForm> allDailyForm = new List<DailyReportForm>();
                for (int f = 0; f < listAccount.Count; f++)
                {
                    String[] s = listAccount[f].Date.ToString().Split('-');
                    //String[] s2 = s[0].Split('/');

                    //MessageBox.Show(s2[0] + "///" + curMonth + "===" + s2[2] + "///" + DateTime.Now.ToString("yyyy"));
                    if ((Int32.Parse(s[1]) == Int32.Parse(sGetLatestMonth[1])) && (Int32.Parse(s[0]) == Int32.Parse(sGetLatestMonth[0])))
                    {
                        if (s[1].Equals("04") && s[2].Equals("12"))
                        {

                        }
                        else if (s[1].Equals("04") && s[2].Equals("13"))
                        {

                        }
                        else if (s[1].Equals("04") && s[2].Equals("14"))
                        {

                        }
                        else if (s[1].Equals("04") && s[2].Equals("15"))
                        {

                        }
                        else
                        {
                            mainOrderRecord.AddRange(this.db.getAllOrderRecordExceptCancelled(listAccount[f].Id));
                        }



                    }

                }

                //Update 07092022 Ph2
                var groupedSameTopicFromOrder = mainOrderRecord
                    .GroupBy(u => u.MassageTopicId)
                    .Select(grp => grp.ToList())
                    .ToList();

                List<int> groupMassageTopic = new List<int>();


                for(int v=0;v< groupedSameTopicFromOrder.Count();v++)
                {
                    groupMassageTopic.Add(groupedSameTopicFromOrder[v][0].MassageTopicId);
                }
                //int aaa = groupedSameTopicFromOrder[0][0].MassageTopicId;
                //int bbb = groupedSameTopicFromOrder[1][0].MassageTopicId;

                List<ShortMassageTopic> finalMassageTopicSet = this.db.getMassageTopicShortSet(groupMassageTopic);
                List<ShortMassageTopic> finalMassageTopicSetSorted = finalMassageTopicSet.OrderBy(o => o.Id).ToList();

                for (int h = 0; h < finalMassageTopicSetSorted.Count(); h++)
                {
                    if (finalMassageTopicSetSorted[h].Name.Contains("Package") || finalMassageTopicSetSorted[h].Name.Contains("Promotion"))
                    {
                        List<MassageSet> msgPlanFromPackage = this.db.getMassagePlanFromTopic(finalMassageTopicSetSorted[h].Id);
                        for (int j = 0; j < msgPlanFromPackage.Count(); j++)
                        {
                            string msgPlanNameFromPackage = this.db.getMassagePlanName(msgPlanFromPackage[j].MassagePlanId);

                            plusYe = plusYe + 13;
                            y1 = y1 + 13;
                            y2 = y2 + 13;
                            gfx.DrawString(finalMassageTopicSetSorted[h].Name + " - " + msgPlanNameFromPackage, ContentFont, BlackBrush, TableColumnRect_Date_Text.X + 7, TableColumnRect_Date_Text.Y + plusYe);
                            gfx.DrawLine(XPens.Black, 10, y1, 780, y2);
                        }

                    }
                    else
                    {
                        if (h == 0)
                        {
                            gfx.DrawString(finalMassageTopicSetSorted[h].Name, ContentFont, BlackBrush, TableColumnRect_Date_Text.X + 7, TableColumnRect_Date_Text.Y + plusYe);
                            gfx.DrawLine(XPens.Black, 10, y1, 780, y2);
                        }
                        else
                        {
                            plusYe = plusYe + 13;
                            y1 = y1 + 13;
                            y2 = y2 + 13;
                            gfx.DrawString(finalMassageTopicSetSorted[h].Name, ContentFont, BlackBrush, TableColumnRect_Date_Text.X + 7, TableColumnRect_Date_Text.Y + plusYe);
                            gfx.DrawLine(XPens.Black, 10, y1, 780, y2);
                        }
                    }

                }


                int plusY = 23;

                int totalPaxX = 0;
                int totalSaleX = 0;

                for (int p = 0; p < finalMassageTopicSetSorted.Count(); p++)
                {
                    if (finalMassageTopicSetSorted[p].Name.Contains("Package")|| finalMassageTopicSetSorted[p].Name.Contains("Promotion"))
                    {

                        List<MassageSet> msgPlanFromPackage = this.db.getMassagePlanFromTopic(finalMassageTopicSetSorted[p].Id);
                        for (int k = 0; k < msgPlanFromPackage.Count(); k++)
                        {
                            int numbers = 0;
                            int tPrice = 0;

                            for (int j = 0; j < mainOrderRecord.Count(); j++)
                            {
                                if (finalMassageTopicSetSorted[p].Id == mainOrderRecord[j].MassageTopicId && msgPlanFromPackage[k].MassagePlanId == mainOrderRecord[j].MassagePlanId)
                                {
                                    numbers = numbers + 1;
                                    tPrice = tPrice + Int32.Parse(mainOrderRecord[j].Price);
                                }
                            }

                            gfx.DrawString(String.Format("{0:n0}", numbers), ContentFont, BlackBrush, TableColumnRect_InitialMoney_Text.X + 65, TableColumnRect_InitialMoney_Text.Y + plusY);

                            gfx.DrawString(String.Format("{0:n}", tPrice), ContentFont, BlackBrush, TableColumnRect_Total_Text.X + 58, TableColumnRect_Total_Text.Y + plusY);

                            plusY = plusY + 13;

                            totalPaxX = totalPaxX + numbers;

                            totalSaleX = totalSaleX + tPrice;
                        }

                    }
                    else
                    {
                        int numbers = 0;
                        int tPrice = 0;
                        for (int j = 0; j < mainOrderRecord.Count(); j++)
                        {
                            if (finalMassageTopicSetSorted[p].Id == mainOrderRecord[j].MassageTopicId)
                            {
                                numbers = numbers + 1;
                                tPrice = tPrice + Int32.Parse(mainOrderRecord[j].Price);
                            }
                        }

                        //if (numbers > 3)
                        //{
                        //    gfx.DrawString(String.Format("{0:n0}", numbers / 4), ContentFont, BlackBrush, TableColumnRect_InitialMoney_Text.X + 65, TableColumnRect_InitialMoney_Text.Y + plusY);
                        //}
                        //else if (numbers == 0)
                        //{
                        //    gfx.DrawString(String.Format("{0:n0}", 0), ContentFont, BlackBrush, TableColumnRect_InitialMoney_Text.X + 65, TableColumnRect_InitialMoney_Text.Y + plusY);
                        //}
                        //else
                        //{
                        //    gfx.DrawString(String.Format("{0:n0}", 1), ContentFont, BlackBrush, TableColumnRect_InitialMoney_Text.X + 65, TableColumnRect_InitialMoney_Text.Y + plusY);
                        //}

                        gfx.DrawString(String.Format("{0:n0}", numbers), ContentFont, BlackBrush, TableColumnRect_InitialMoney_Text.X + 65, TableColumnRect_InitialMoney_Text.Y + plusY);

                        gfx.DrawString(String.Format("{0:n}", tPrice), ContentFont, BlackBrush, TableColumnRect_Total_Text.X + 58, TableColumnRect_Total_Text.Y + plusY);

                        plusY = plusY + 13;

                        //if (numbers > 3)
                        //{
                        //    //gfx.DrawString(String.Format("{0:n0}", numbers / 4), ContentFont, BlackBrush, TableColumnRect_InitialMoney_Text.X + 65, TableColumnRect_InitialMoney_Text.Y + plusY);
                        //    totalPaxX = totalPaxX + (numbers / 4);
                        //}
                        //else if (numbers == 0)
                        //{
                        //    //gfx.DrawString(String.Format("{0:n0}", 0), ContentFont, BlackBrush, TableColumnRect_InitialMoney_Text.X + 65, TableColumnRect_InitialMoney_Text.Y + plusY);
                        //    totalPaxX = totalPaxX + 0;
                        //}
                        //else
                        //{
                        //    //gfx.DrawString(String.Format("{0:n0}", 1), ContentFont, BlackBrush, TableColumnRect_InitialMoney_Text.X + 65, TableColumnRect_InitialMoney_Text.Y + plusY);
                        //    totalPaxX = totalPaxX + 1;
                        //}

                        totalPaxX = totalPaxX + numbers;

                        totalSaleX = totalSaleX + tPrice;
                    }
                    

                }


                gfx.DrawString("Total", ContentFont, BlackBrush, TableColumnRect_Date_Text.X + 7, TableColumnRect_Date_Text.Y + 692);
                gfx.DrawString(String.Format("{0:n0}", totalPaxX), ContentFont, BlackBrush, TableColumnRect_InitialMoney_Text.X + 65, TableColumnRect_InitialMoney_Text.Y + 692);
                gfx.DrawString(String.Format("{0:n}", totalSaleX), ContentFont, BlackBrush, TableColumnRect_Total_Text.X + 58, TableColumnRect_Total_Text.Y + 692);
            }

            //int netTotalPax = 0;
            //int netMassageAmount = 0;
            //int netAveragePerPax = 0;
            //int netTotalWorker = 0;
            //int netOil = 0;
            //int netTotalIncome = 0;
            //int netCommis = 0;
            //int netCancelledPax = 0;
            //int netBalanceNet = 0;
            //for (int k = 0; k < allDailyForm.Count; k++)
            //{
            //    string convertTotalPax = allDailyForm[k].TotalPax.Replace(".00", "");
            //    string convertMassageAmount = allDailyForm[k].MassageAmount.Replace(".00", "");
            //    string convertAveragePerPax = allDailyForm[k].AveragePerPax.Replace(".00", "");
            //    string convertTotalWorker = allDailyForm[k].TotalWorker.Replace(".00", "");
            //    string convertOilAmount = allDailyForm[k].OilAmount.Replace(".00", "");
            //    string convertTotalIncome = allDailyForm[k].TotalIncome.Replace(".00", "");
            //    string convertCommis = allDailyForm[k].PayWorkers.Replace(".00", "");
            //    string convertCancelledPax = allDailyForm[k].TotalCancelled.Replace(".00", "");
            //    string convertBalance = allDailyForm[k].BalanceNet.Replace(".00", "");

            //    string convertTotalPaxs = convertTotalPax.Replace(",", "");
            //    string convertMassageAmounts = convertMassageAmount.Replace(",", "");
            //    string convertAveragePerPaxs = convertAveragePerPax.Replace(",", "");
            //    string convertTotalWorkers = convertTotalWorker.Replace(",", "");
            //    string convertOilAmounts = convertOilAmount.Replace(",", "");
            //    string convertTotalIncomes = convertTotalIncome.Replace(",", "");
            //    string convertCommiss = convertCommis.Replace(",", "");
            //    string convertCancelledPaxs = convertCancelledPax.Replace(",", "");
            //    string convertBalances = convertBalance.Replace(",", "");

            //    netTotalPax += Int32.Parse(convertTotalPaxs);
            //    netMassageAmount += Int32.Parse(convertMassageAmounts);
            //    netAveragePerPax += Int32.Parse(convertAveragePerPaxs);
            //    netTotalWorker += Int32.Parse(convertTotalWorkers);
            //    netOil += Int32.Parse(convertOilAmounts);
            //    netTotalIncome += Int32.Parse(convertTotalIncomes);
            //    netCommis += Int32.Parse(convertCommiss);
            //    netCancelledPax += Int32.Parse(convertCancelledPaxs);
            //    netBalanceNet += Int32.Parse(convertBalances);
            //}

            //gfx.DrawString("Total", ContentFont, BlackBrush, TableColumnRect_Date_Text.X + 7, TableColumnRect_Date_Text.Y + 426);
            ////gfx.DrawString("6,000.00", ContentFont, BlackBrush, TableColumnRect_InitialMoney_Text.X, TableColumnRect_InitialMoney_Text.Y + 426);
            //gfx.DrawString(netTotalPax.ToString(), ContentFont, BlackBrush, TableColumnRect_Total_Text.X + 8, TableColumnRect_Total_Text.Y + 426);
            //gfx.DrawString(String.Format("{0:n}", netMassageAmount), ContentFont, BlackBrush, TableColumnRect_Massage_Text.X - 4, TableColumnRect_Massage_Text.Y + 426);
            //gfx.DrawString(String.Format("{0:n}", netAveragePerPax), ContentFont, BlackBrush, TableColumnRect_AveragePerPax_Text.X - 3, TableColumnRect_AveragePerPax_Text.Y + 426);
            //gfx.DrawString(netTotalWorker.ToString(), ContentFont, BlackBrush, TableColumnRect_TotalWorker_Text.X + 15, TableColumnRect_TotalWorker_Text.Y + 426);
            //gfx.DrawString(String.Format("{0:n}", netOil), ContentFont, BlackBrush, TableColumnRect_OilIncome_Text.X, TableColumnRect_OilIncome_Text.Y + 426);
            //gfx.DrawString(String.Format("{0:n}", netTotalIncome), ContentFont, BlackBrush, TableColumnRect_TotalIncome_Text.X + 8, TableColumnRect_TotalIncome_Text.Y + 426);
            //gfx.DrawString(String.Format("{0:n}", netCommis), ContentFont, BlackBrush, TableColumnRect_PayWorker_Text.X - 5, TableColumnRect_PayWorker_Text.Y + 426);
            //gfx.DrawString(netCancelledPax.ToString(), ContentFont, BlackBrush, TableColumnRect_TotalCancelled_Text.X + 20, TableColumnRect_TotalCancelled_Text.Y + 426);
            //gfx.DrawString(String.Format("{0:n}", netBalanceNet), ContentFont, BlackBrush, TableColumnRect_BalanceNet_Text.X + 5, TableColumnRect_BalanceNet_Text.Y + 426);
            //gfx.DrawLine(XPens.Black, 10, 529, 780, 530);

            string fullDate = new DateTime(Int32.Parse(sGetLatestMonth[0]), Int32.Parse(sGetLatestMonth[1]), Int32.Parse(sGetLatestMonth[2])).ToString("ddMMMMyyyy");
            //string[] longDate = dateStamp.ToLongDateString().Split(' ');
            //string preReal = longDate[2] + longDate[1] + longDate[3];
            //string realDate = preReal.Replace(",", "");
            filenameDetail25 = @"C:\SpaSystem\report" + fullDate + "_Detail.pdf";

            //test
            document.Save(filenameDetail25);
            exportPDF25();
            //Process.Start(rfilename);
            //try
            //{
            //    string curDateTime = getCurDateTime();

            //    Account getLastAc = this.db.getLatestAcount();
            //    Account newAc = new Account()
            //    {
            //        Id = getLastAc.Id,
            //        Date = getLastAc.Date,
            //        Time = getLastAc.Time,
            //        StartMoney = getLastAc.StartMoney,
            //        StaffAmount = getLastAc.StaffAmount,
            //        Completed = "true",
            //        SendStatus = getLastAc.SendStatus,
            //        UpdateStatus = getLastAc.UpdateStatus,
            //        CreateDateTime = getLastAc.CreateDateTime,
            //        UpdateDateTime = curDateTime
            //    };

            //    this.db.updateAcount(newAc);

            //    //loadingGrid.Visibility = Visibility.Visible;
            //    //loadingTxt.Text = "Computer กำลังปิด โปรดรอสักครู่...";

            //    await Task.Delay(2000);

            //    //test

            //    //MailMessage mail = new MailMessage();
            //    //SmtpClient SmtpServer = new SmtpClient(GlobalValue.Instance.emailServer);
            //    //mail.From = new MailAddress(GlobalValue.Instance.senderEmail);
            //    //mail.To.Add("pascal_tober@hotmail.com");
            //    //mail.To.Add("armaz@hotmail.fr");
            //    //mail.To.Add("nit_sisuwan@hotmail.fr");
            //    //mail.To.Add("siree941@gmail.com");
            //    //mail.To.Add("t.jaturong@outlook.com");
            //    //mail.Subject = currentBranchName + " - Daily Report(" + fullDate + ")";
            //    //mail.Body = "This daily report email is auto sent by SpaSystem program (" + currentBranchName + ")";

            //    //Attachment attachment;
            //    //attachment = new Attachment(filename);

            //    //mail.Attachments.Add(attachment);

            //    //SmtpServer.Port = GlobalValue.Instance.serverPort;
            //    //SmtpServer.Credentials = new NetworkCredential(GlobalValue.Instance.serverUsername, GlobalValue.Instance.serverPassword);
            //    //SmtpServer.EnableSsl = true;

            //    //SmtpServer.Send(mail);
            //    //MessageBox.Show("Email is sent\nEmail ถูกส่งเรียบร้อย");

            //    //Application.Current.Shutdown();

            //    ////test
            //    //var psi = new ProcessStartInfo("shutdown", "/s /t 0");
            //    //psi.CreateNoWindow = true;
            //    //psi.UseShellExecute = false;
            //    //Process.Start(psi);
            //    exportPDF25();


            //}
            //catch (Exception pp)
            //{
            //    //MessageBox.Show(pp.ToString());
            //    MessageBox.Show("ไม่สามารถส่ง Email ได้เนื่องจากไม่มี Internet กรุณาติดต่อผู้ดูแลระบบ");

            //    exportPDF25();
            //    //Application.Current.Shutdown();

            //    //test
            //    //var psi = new ProcessStartInfo("shutdown", "/s /t 0");
            //    //psi.CreateNoWindow = true;
            //    //psi.UseShellExecute = false;
            //    //Process.Start(psi);


            //}
            //Process.Start(filename);

            //try
            //{
            //    string curDateTime = getCurDateTime();

            //    //Account getLastAc = this.db.getLatestAcount();
            //    //Account newAc = new Account()
            //    //{
            //    //    Id = getLastAc.Id,
            //    //    Date = getLastAc.Date,
            //    //    Time = getLastAc.Time,
            //    //    StartMoney = getLastAc.StartMoney,
            //    //    StaffAmount = getLastAc.StaffAmount,
            //    //    Completed = "true",
            //    //    SendStatus = getLastAc.SendStatus,
            //    //    UpdateStatus = getLastAc.UpdateStatus,
            //    //    CreateDateTime = getLastAc.CreateDateTime,
            //    //    UpdateDateTime = curDateTime
            //    //};

            //    //this.db.updateAcount(newAc);

            //    //loadingGrid.Visibility = Visibility.Visible;
            //    //loadingTxt.Text = "Computer กำลังปิด โปรดรอสักครู่...";

            //    await Task.Delay(2000);

            //    //test

            //    MailMessage mail = new MailMessage();
            //    SmtpClient SmtpServer = new SmtpClient(GlobalValue.Instance.emailServer);
            //    mail.From = new MailAddress(GlobalValue.Instance.senderEmail);
            //    mail.To.Add("pascal_tober@hotmail.com");
            //    mail.To.Add("armaz@hotmail.fr");
            //    mail.To.Add("nit_sisuwan@hotmail.fr");
            //    mail.To.Add("panuwatdethfung@outlook.com");
            //    mail.To.Add("fa9fa9fa9@gmail.com");
            //    mail.To.Add("t.jaturong@outlook.com");
            //    mail.Subject = currentBranchName + " - Daily Report(" + fullDate + ")";
            //    mail.Body = "This daily report email is auto sent by SpaSystem program (" + currentBranchName + ")";

            //    Attachment attachment;
            //    attachment = new Attachment(filename);
            //    //Attachment attachment25;
            //    //attachment25 = new Attachment(filename25);
            //    Attachment attachment25Details;
            //    attachment25Details = new Attachment(filenameDetail25);

            //    mail.Attachments.Add(attachment);
            //    //mail.Attachments.Add(attachment25);
            //    mail.Attachments.Add(attachment25Details);

            //    SmtpServer.Port = GlobalValue.Instance.serverPort;
            //    SmtpServer.Credentials = new NetworkCredential(GlobalValue.Instance.serverUsername, GlobalValue.Instance.serverPassword);
            //    SmtpServer.EnableSsl = true;

            //    SmtpServer.Send(mail);
            //    //MessageBox.Show("Email is sent\nEmail ถูกส่งเรียบร้อย");

            //    Application.Current.Shutdown();

            //    //test
            //    var psi = new ProcessStartInfo("shutdown", "/s /t 0");
            //    psi.CreateNoWindow = true;
            //    psi.UseShellExecute = false;
            //    Process.Start(psi);


            //}
            //catch (Exception pp)
            //{
            //    //MessageBox.Show(pp.ToString());
            //    MessageBox.Show("ไม่สามารถส่ง Email ได้เนื่องจากไม่มี Internet กรุณาติดต่อผู้ดูแลระบบ");


            //    Application.Current.Shutdown();

            //    //test
            //    var psi = new ProcessStartInfo("shutdown", "/s /t 0");
            //    psi.CreateNoWindow = true;
            //    psi.UseShellExecute = false;
            //    Process.Start(psi);


            //}

        }

        public async void exportPDF25()
        {
            List<Branch> listBranch = this.db.getAllBranch();
            string fullDate = "";

            // Create a new PDF document
            PdfDocument documents = new PdfDocument();

            // Create an empty page
            PdfPage pages = documents.AddPage();
            pages.Orientation = PageOrientation.Landscape;


            // Get an XGraphics object for drawing
            XGraphics gfx = XGraphics.FromPdfPage(pages);

            //XRect rect = new XRect(0, 0, 250, 140);

            //XFont font = new XFont("Verdana", 10);
            //XBrush brush = XBrushes.Purple;


            XRect BigTitleRect = new XRect(0, 0, 800, 20);
            XRect TableHeaderRect = new XRect(10, 77, 770, 36);
            XRect TableFooterRect = new XRect(10, 517, 770, 12);

            XRect TableColumnRect_Date = new XRect(10, 77, 30, 452);
            XRect TableColumnRect_InitialMoney = new XRect(40, 77, 55, 452);
            XRect TableColumnRect_Total = new XRect(95, 77, 30, 452);
            XRect TableColumnRect_Massage = new XRect(125, 77, 174, 452);
            XRect TableColumnRect_Massage_Cash = new XRect(125, 95, 62, 434);
            XRect TableColumnRect_Massage_Credit = new XRect(187, 95, 61, 434);
            XRect TableColumnRect_Massage_Voucher = new XRect(248, 95, 51, 434);
            XRect TableColumnRect_AveragePerPax = new XRect(299, 77, 47, 452);
            XRect TableColumnRect_TotalWorker = new XRect(346, 77, 34, 452);
            XRect TableColumnRect_OilIncome = new XRect(380, 77, 54, 452);
            XRect TableColumnRect_TotalIncome = new XRect(434, 77, 65, 452);
            XRect TableColumnRect_PayWorker = new XRect(499, 77, 62, 452);
            XRect TableColumnRect_TotalCancelled = new XRect(561, 77, 45, 452);
            XRect TableColumnRect_TotalUniform = new XRect(606, 77, 50, 452);
            XRect TableColumnRect_TotalTigerBalm = new XRect(656, 77, 54, 452);
            XRect TableColumnRect_BalanceNet = new XRect(710, 77, 70, 452);

            XRect TableColumnRect_Date_Text_Header = new XRect(11, 90, 27, 520);
            XRect TableColumnRect_InitialMoney_Text_Header = new XRect(54, 90, 27, 520);
            XRect TableColumnRect_Total_Text_Header = new XRect(95, 90, 27, 520);
            XRect TableColumnRect_Massage_Text_Header = new XRect(201, 80, 27, 520);
            XRect TableColumnRect_Massage_Cash_Text_Header = new XRect(147, 99, 14, 520);
            XRect TableColumnRect_Massage_Credit_Text_Header = new XRect(210, 99, 14, 520);
            XRect TableColumnRect_Massage_Voucher_Text_Header = new XRect(265, 99, 14, 520);
            XRect TableColumnRect_AveragePerPax_Text_Header = new XRect(308, 85, 27, 520);
            XRect TableColumnRect_AveragePerPax_2_Text_Header = new XRect(308, 95, 27, 520);
            XRect TableColumnRect_TotalWorker_Text_Header = new XRect(341, 85, 43, 520);
            XRect TableColumnRect_TotalWorker_2_Text_Header = new XRect(341, 95, 43, 520);
            XRect TableColumnRect_OilIncome_Text_Header = new XRect(390, 85, 33, 520);
            XRect TableColumnRect_OilIncome_2_Text_Header = new XRect(390, 95, 33, 520);
            XRect TableColumnRect_TotalIncome_Text_Header = new XRect(440, 90, 56, 520);
            XRect TableColumnRect_PayWorker_Text_Header = new XRect(513, 90, 33, 520);
            XRect TableColumnRect_TotalCancelled_Text_Header = new XRect(557, 90, 50, 520);
            XRect TableColumnRect_TotalUniform_Text_Header = new XRect(605, 90, 50, 520);
            XRect TableColumnRect_TotalTigerBalm_Text_Header = new XRect(658, 90, 50, 520);
            XRect TableColumnRect_BalanceNet_Text_Header = new XRect(720, 90, 56, 520);

            XRect TableColumnRect_Date_Text = new XRect(11, 100, 27, 520);
            XRect TableColumnRect_InitialMoney_Text = new XRect(52, 100, 27, 520);
            XRect TableColumnRect_Total_Text = new XRect(90, 100, 27, 520);
            XRect TableColumnRect_Massage_Text = new XRect(135, 100, 27, 520);
            XRect TableColumnRect_Massage_Credit_Text = new XRect(203, 100, 27, 520);
            XRect TableColumnRect_Massage_Voucher_Text = new XRect(254, 100, 27, 520);
            XRect TableColumnRect_AveragePerPax_Text = new XRect(306, 100, 27, 520);
            XRect TableColumnRect_TotalWorker_Text = new XRect(337, 100, 43, 520);
            XRect TableColumnRect_OilIncome_Text = new XRect(393, 100, 33, 520);
            XRect TableColumnRect_TotalIncome_Text = new XRect(435, 100, 56, 520);
            XRect TableColumnRect_PayWorker_Text = new XRect(513, 100, 33, 520);
            XRect TableColumnRect_TotalCancelled_Text = new XRect(562, 100, 50, 520);
            XRect TableColumnRect_TotalUniform_Text = new XRect(610, 100, 50, 520);
            XRect TableColumnRect_TotalTigerBalm_Text = new XRect(657, 100, 50, 520);
            XRect TableColumnRect_BalanceNet_Text = new XRect(720, 100, 56, 520);

            XRect TableColumnRect_No = new XRect(10, 95, 27, 520);
            XRect TableColumnRect_Time = new XRect(10 + TableColumnRect_No.Width, 95, 50, 520);
            XRect TableColumnRect_Detail = new XRect(10 + TableColumnRect_No.Width + TableColumnRect_Time.Width, 95, 438, 520);
            XRect TableColumnRect_Price = new XRect(10 + TableColumnRect_No.Width + TableColumnRect_Time.Width + TableColumnRect_Detail.Width, 95, 75, 545);

            XRect TableColumnRect_No_Text = new XRect(10, 122, 27, 520);
            XRect TableColumnRect_Time_Text = new XRect(10 + TableColumnRect_No.Width, 122, 50, 520);
            XRect TableColumnRect_Detail_Text = new XRect(10 + TableColumnRect_No.Width + TableColumnRect_Time.Width, 122, 438, 520);
            XRect TableColumnRect_Price_Text = new XRect(10 + TableColumnRect_No.Width + TableColumnRect_Time.Width + TableColumnRect_Detail.Width, 122, 75, 545);

            //XRect TableFooterText_TotalSale = new XRect(10, 635, 590, 25);
            //XRect TableFooterText_TotalSaleValue = new XRect(10, 635, 590, 25);

            XFont BigTitleFont = new XFont("Verdana", 13);
            XFont HeaderContentFont = new XFont("Verdana", 10, XFontStyle.Underline);
            XFont ContentFont = new XFont("Verdana", 8);

            XBrush BlackBrush = XBrushes.Black;

            XStringFormat format = new XStringFormat();

            //gfx.DrawRectangle(XPens.YellowGreen, rect);
            gfx.DrawRectangle(XBrushes.LightGray, BigTitleRect);
            gfx.DrawRectangle(XPens.Black, BigTitleRect);
            //gfx.DrawLine(XPens.YellowGreen, rect.Width / 2, 0, rect.Width / 2, rect.Height);
            //gfx.DrawLine(XPens.YellowGreen, 0, rect.Height / 2, rect.Width, rect.Height / 2);
            Account curAcctx = this.db.getAccountFromId(currentUseAccountId);
            string acctxDate = curAcctx.Date;
            string[] splitAcctxDate = acctxDate.Split('-');
            MonthConvertor mc = new MonthConvertor();

            format.LineAlignment = XLineAlignment.Center;
            format.Alignment = XStringAlignment.Center;
            gfx.DrawString(currentBranchName, BigTitleFont, BlackBrush, BigTitleRect, format);
            gfx.DrawString("Daily Report", HeaderContentFont, BlackBrush, 20, 4 + HeaderContentFont.Height + BigTitleRect.Height);
            gfx.DrawString("Branch id : " + listBranch[0].Id, ContentFont, BlackBrush, 20, 8 + HeaderContentFont.Height + BigTitleRect.Height + ContentFont.Height);
            gfx.DrawString("Month/Year : " + mc.calMonth(splitAcctxDate[1]) + " " + splitAcctxDate[0], ContentFont, BlackBrush, 20, 6 + HeaderContentFont.Height + BigTitleRect.Height + ContentFont.Height + ContentFont.Height);
            gfx.DrawString("Branch name : " + currentBranchName, ContentFont, BlackBrush, 220, 8 + HeaderContentFont.Height + BigTitleRect.Height + ContentFont.Height);
            gfx.DrawString("Generate Date/Time : " + DateTime.Now.ToString("dd-MM-yyy HH:mm"), ContentFont, BlackBrush, 220, 6 + HeaderContentFont.Height + BigTitleRect.Height + ContentFont.Height + ContentFont.Height);

            gfx.DrawLine(XPens.Black, 0, 16 + HeaderContentFont.Height + BigTitleRect.Height + ContentFont.Height + ContentFont.Height, BigTitleRect.Width, 17 + HeaderContentFont.Height + BigTitleRect.Height + ContentFont.Height + ContentFont.Height);

            gfx.DrawRectangle(XBrushes.Yellow, TableHeaderRect);
            gfx.DrawRectangle(XPens.Black, TableHeaderRect);
            gfx.DrawRectangle(XBrushes.Yellow, TableFooterRect);

            //gfx.DrawRectangle(XBrushes.LightGray, TableFooterRect);
            //gfx.DrawRectangle(XPens.Black, TableFooterRect);

            gfx.DrawRectangle(XPens.Black, TableColumnRect_Date);
            gfx.DrawRectangle(XPens.Black, TableColumnRect_InitialMoney);
            gfx.DrawRectangle(XPens.Black, TableColumnRect_Total);
            gfx.DrawRectangle(XPens.Black, TableColumnRect_Massage);
            gfx.DrawRectangle(XPens.Black, TableColumnRect_Massage_Cash);
            gfx.DrawRectangle(XPens.Black, TableColumnRect_Massage_Credit);
            gfx.DrawRectangle(XPens.Black, TableColumnRect_Massage_Voucher);
            gfx.DrawRectangle(XPens.Black, TableColumnRect_AveragePerPax);
            gfx.DrawRectangle(XPens.Black, TableColumnRect_TotalWorker);
            gfx.DrawRectangle(XPens.Black, TableColumnRect_OilIncome);
            gfx.DrawRectangle(XPens.Black, TableColumnRect_TotalIncome);
            gfx.DrawRectangle(XPens.Black, TableColumnRect_PayWorker);
            gfx.DrawRectangle(XPens.Black, TableColumnRect_TotalCancelled);
            gfx.DrawRectangle(XPens.Black, TableColumnRect_TotalUniform);
            gfx.DrawRectangle(XPens.Black, TableColumnRect_TotalTigerBalm);
            gfx.DrawRectangle(XPens.Black, TableColumnRect_BalanceNet);

            format.LineAlignment = XLineAlignment.Near;
            format.Alignment = XStringAlignment.Center;

            gfx.DrawString("Date", ContentFont, BlackBrush, TableColumnRect_Date_Text_Header, format);
            gfx.DrawString("Start Money", ContentFont, BlackBrush, TableColumnRect_InitialMoney_Text_Header, format);
            gfx.DrawString("Pax", ContentFont, BlackBrush, TableColumnRect_Total_Text_Header, format);
            gfx.DrawString("Massage Amount", ContentFont, BlackBrush, TableColumnRect_Massage_Text_Header, format);
            gfx.DrawString("Cash", ContentFont, BlackBrush, TableColumnRect_Massage_Cash_Text_Header, format);
            gfx.DrawString("Credit", ContentFont, BlackBrush, TableColumnRect_Massage_Credit_Text_Header, format);
            gfx.DrawString("Voucher", ContentFont, BlackBrush, TableColumnRect_Massage_Voucher_Text_Header, format);
            gfx.DrawString("Average", ContentFont, BlackBrush, TableColumnRect_AveragePerPax_Text_Header, format);
            gfx.DrawString("/Pax", ContentFont, BlackBrush, TableColumnRect_AveragePerPax_2_Text_Header, format);
            gfx.DrawString("Total", ContentFont, BlackBrush, TableColumnRect_TotalWorker_Text_Header, format);
            gfx.DrawString("Worker", ContentFont, BlackBrush, TableColumnRect_TotalWorker_2_Text_Header, format);
            gfx.DrawString("Income", ContentFont, BlackBrush, TableColumnRect_OilIncome_Text_Header, format);
            gfx.DrawString(GlobalValue.Instance.oilPrice + "B/Staff", ContentFont, BlackBrush, TableColumnRect_OilIncome_2_Text_Header, format);
            gfx.DrawString("Total Incomes", ContentFont, BlackBrush, TableColumnRect_TotalIncome_Text_Header, format);
            gfx.DrawString("Pay Workers", ContentFont, BlackBrush, TableColumnRect_PayWorker_Text_Header, format);
            gfx.DrawString("Cancelled", ContentFont, BlackBrush, TableColumnRect_TotalCancelled_Text_Header, format);
            gfx.DrawString("Uniform", ContentFont, BlackBrush, TableColumnRect_TotalUniform_Text_Header, format);
            gfx.DrawString("Tiger Balm", ContentFont, BlackBrush, TableColumnRect_TotalTigerBalm_Text_Header, format);
            gfx.DrawString("Balance Net", ContentFont, BlackBrush, TableColumnRect_BalanceNet_Text_Header, format);

            int y1 = 126;
            int y2 = 127;
            int plusYe = 23;

            for (int h = 0; h < 31; h++)
            {
                if (h == 0)
                {
                    gfx.DrawString(h + 1 + "", ContentFont, BlackBrush, TableColumnRect_Date_Text.X + 7, TableColumnRect_Date_Text.Y + plusYe);
                    gfx.DrawLine(XPens.Black, 10, y1, 780, y2);
                }
                else
                {
                    plusYe = plusYe + 13;
                    y1 = y1 + 13;
                    y2 = y2 + 13;
                    gfx.DrawString(h + 1 + "", ContentFont, BlackBrush, TableColumnRect_Date_Text.X + 7, TableColumnRect_Date_Text.Y + plusYe);
                    gfx.DrawLine(XPens.Black, 10, y1, 780, y2);
                }
            }

            List<Account> listAccount = this.db.getAccountLast40Records();
            List<DailyReportForm> allDailyForm = new List<DailyReportForm>();
            Account getLatestMonth = this.db.getLatestAcount();
            String[] sGetLatestMonth = getLatestMonth.Date.ToString().Split('-');
            //String[] s2GetLatestMonth = sGetLatestMonth[0].Split('/');

            if (listAccount.Count != 0)
            {
                //List<DailyReportForm> allDailyForm = new List<DailyReportForm>();
                for (int f = 0; f < listAccount.Count; f++)
                {
                    String[] s = listAccount[f].Date.ToString().Split('-');
                    //String[] s2 = s[0].Split('/');

                    //MessageBox.Show(s2[0] + "///" + curMonth + "===" + s2[2] + "///" + DateTime.Now.ToString("yyyy"));
                    if ((Int32.Parse(s[1]) == Int32.Parse(sGetLatestMonth[1])) && (Int32.Parse(s[0]) == Int32.Parse(sGetLatestMonth[0])))
                    {
                        int voucherCash = this.db.getAllDiscountWithCashFromAccountID(listAccount[f].Id) / 4;
                        int voucherCredit = this.db.getAllDiscountWithCreditFromAccountID(listAccount[f].Id) / 4;
                        int voucherCashFull = this.db.getAllDiscountWithCashFromAccountID(listAccount[f].Id);
                        int voucherCreditFull = this.db.getAllDiscountWithCreditFromAccountID(listAccount[f].Id);
                        int income = (getTotalSaleFromId(listAccount[f].Id) - voucherCashFull) / 4;
                        int creditIncome = (getTotalCreditSaleFromId(listAccount[f].Id) - voucherCreditFull) / 4;
                        int commis = getTotalCommissionFromId(listAccount[f].Id) / 4;
                        int totalVoucher = voucherCash + voucherCredit;
                        int grandIncome = income + creditIncome; //+ oil;
                        int pax = 0;
                        if (grandIncome == 0)//new function 7 July 2020
                        {
                            pax = 0;
                        }
                        else if (grandIncome != 0 && getTotalPaxFromId(listAccount[f].Id) < 4)//new function 7 July 2020
                        {
                            pax = 1;
                        }
                        else
                        {
                            pax = getTotalPaxFromId(listAccount[f].Id) / 4; //new function 3 Nov 2019
                        }
                        int averagePax = 0;
                        if (grandIncome != 0 && pax != 0)
                        {
                            averagePax = (grandIncome / pax);
                        }
                        int pre_staff = Int32.Parse(listAccount[f].StaffAmount);
                        int staff = 0;
                        if (grandIncome != 0 && pre_staff < 4)//new function 7 July 2020
                        {
                            staff = 1;
                        }
                        else
                        {
                            staff = pre_staff / 4; //new function 3 Nov 2019
                        }
                        int oil = staff * GlobalValue.Instance.oilPrice;
                        int uniform = getTotalUniformFromId(listAccount[f].Id) / 4;
                        int tigerBalm = getTotalTigerBalmFromId(listAccount[f].Id) / 4;
                        int finalIncome = grandIncome - commis + uniform + tigerBalm;
                        int totalCancelled = getTotalCancelledPaxFromId(listAccount[f].Id);


                        DailyReportForm dailyForm = new DailyReportForm()
                        {
                            Date = s[2],
                            StartMoney = String.Format("{0:n}", Int32.Parse(listAccount[f].StartMoney)),
                            TotalPax = pax.ToString(),
                            MassageAmount = String.Format("{0:n}", income),
                            MassageCreditAmount = String.Format("{0:n}", creditIncome),
                            MassageVoucherAmount = String.Format("{0:n}", totalVoucher),
                            AveragePerPax = String.Format("{0:n}", averagePax),
                            TotalWorker = staff.ToString(),
                            OilAmount = String.Format("{0:n}", oil),
                            TotalIncome = String.Format("{0:n}", grandIncome),
                            PayWorkers = String.Format("{0:n}", commis),
                            TotalCancelled = totalCancelled.ToString(),
                            TotalUniform = String.Format("{0:n}", uniform),
                            TotalTigerBalm = String.Format("{0:n}", tigerBalm),
                            BalanceNet = String.Format("{0:n}", finalIncome)
                        };

                        allDailyForm.Add(dailyForm);
                    }

                }

                int plusY = 23;

                for (int h = 0; h < 31; h++)
                {
                    for (int j = 0; j < allDailyForm.Count; j++)
                    {
                        int a = h + 1;
                        if (a == Int32.Parse(allDailyForm[j].Date))
                        {
                            //gfx.DrawString(h + 1 + "", ContentFont, BlackBrush, TableColumnRect_Date_Text.X + 7, TableColumnRect_Date_Text.Y + plusY);
                            gfx.DrawString(allDailyForm[j].StartMoney, ContentFont, BlackBrush, TableColumnRect_InitialMoney_Text.X - 4, TableColumnRect_InitialMoney_Text.Y + plusY); //edit on 3 Nov 2019
                            gfx.DrawString(allDailyForm[j].TotalPax, ContentFont, BlackBrush, TableColumnRect_Total_Text.X + 8, TableColumnRect_Total_Text.Y + plusY);
                            gfx.DrawString(allDailyForm[j].MassageAmount, ContentFont, BlackBrush, TableColumnRect_Massage_Text.X - 4, TableColumnRect_Massage_Text.Y + plusY);
                            gfx.DrawString(allDailyForm[j].MassageCreditAmount, ContentFont, BlackBrush, TableColumnRect_Massage_Credit_Text.X - 4, TableColumnRect_Massage_Credit_Text.Y + plusY);
                            gfx.DrawString(allDailyForm[j].MassageVoucherAmount, ContentFont, BlackBrush, TableColumnRect_Massage_Voucher_Text.X - 4, TableColumnRect_Massage_Voucher_Text.Y + plusY);
                            gfx.DrawString(allDailyForm[j].AveragePerPax, ContentFont, BlackBrush, TableColumnRect_AveragePerPax_Text.X, TableColumnRect_AveragePerPax_Text.Y + plusY);
                            gfx.DrawString(allDailyForm[j].TotalWorker, ContentFont, BlackBrush, TableColumnRect_TotalWorker_Text.X + 14, TableColumnRect_TotalWorker_Text.Y + plusY);
                            gfx.DrawString(allDailyForm[j].OilAmount, ContentFont, BlackBrush, TableColumnRect_OilIncome_Text.X, TableColumnRect_OilIncome_Text.Y + plusY);
                            gfx.DrawString(allDailyForm[j].TotalIncome, ContentFont, BlackBrush, TableColumnRect_TotalIncome_Text.X + 8, TableColumnRect_TotalIncome_Text.Y + plusY);
                            gfx.DrawString(allDailyForm[j].PayWorkers, ContentFont, BlackBrush, TableColumnRect_PayWorker_Text.X - 3, TableColumnRect_PayWorker_Text.Y + plusY);
                            gfx.DrawString(allDailyForm[j].TotalCancelled, ContentFont, BlackBrush, TableColumnRect_TotalCancelled_Text.X + 15, TableColumnRect_TotalCancelled_Text.Y + plusY);
                            gfx.DrawString(allDailyForm[j].TotalUniform, ContentFont, BlackBrush, TableColumnRect_TotalUniform_Text.X + 5, TableColumnRect_TotalUniform_Text.Y + plusY);
                            gfx.DrawString(allDailyForm[j].TotalTigerBalm, ContentFont, BlackBrush, TableColumnRect_TotalTigerBalm_Text.X + 10, TableColumnRect_TotalTigerBalm_Text.Y + plusY);
                            gfx.DrawString(allDailyForm[j].BalanceNet, ContentFont, BlackBrush, TableColumnRect_BalanceNet_Text.X + 6, TableColumnRect_BalanceNet_Text.Y + plusY); //edit on 3 Nov 2019

                        }


                    }

                    plusY = plusY + 13;
                    //if (h == 0)
                    //{
                    //    gfx.DrawString(h + 1 + "", ContentFont, BlackBrush, TableColumnRect_Date_Text.X + 7, TableColumnRect_Date_Text.Y + plusY);
                    //    gfx.DrawString("6,000.00", ContentFont, BlackBrush, TableColumnRect_InitialMoney_Text.X, TableColumnRect_InitialMoney_Text.Y + plusY);
                    //    gfx.DrawString("125", ContentFont, BlackBrush, TableColumnRect_Total_Text.X + 4, TableColumnRect_Total_Text.Y + plusY);
                    //    gfx.DrawString("44,500.00", ContentFont, BlackBrush, TableColumnRect_Massage_Text.X - 4, TableColumnRect_Massage_Text.Y + plusY);
                    //    gfx.DrawString("300.00", ContentFont, BlackBrush, TableColumnRect_AveragePerPax_Text.X, TableColumnRect_AveragePerPax_Text.Y + plusY);
                    //    gfx.DrawString("30", ContentFont, BlackBrush, TableColumnRect_TotalWorker_Text.X + 10, TableColumnRect_TotalWorker_Text.Y + plusY);
                    //    gfx.DrawString("600.00", ContentFont, BlackBrush, TableColumnRect_OilIncome_Text.X, TableColumnRect_OilIncome_Text.Y + plusY);
                    //    gfx.DrawString("45,100.00", ContentFont, BlackBrush, TableColumnRect_TotalIncome_Text.X + 8, TableColumnRect_TotalIncome_Text.Y + plusY);
                    //    gfx.DrawString("20,000.00", ContentFont, BlackBrush, TableColumnRect_PayWorker_Text.X - 3, TableColumnRect_PayWorker_Text.Y + plusY);
                    //    gfx.DrawString("25,100.00", ContentFont, BlackBrush, TableColumnRect_BalanceNet_Text.X, TableColumnRect_BalanceNet_Text.Y + plusY);
                    //}
                    //else
                    //{
                    //    plusY = plusY + 13;

                    //    gfx.DrawString(h + 1 + "", ContentFont, BlackBrush, TableColumnRect_Date_Text.X + 7, TableColumnRect_Date_Text.Y + plusY);
                    //    gfx.DrawString("6,000.00", ContentFont, BlackBrush, TableColumnRect_InitialMoney_Text.X, TableColumnRect_InitialMoney_Text.Y + plusY);
                    //    gfx.DrawString("125", ContentFont, BlackBrush, TableColumnRect_Total_Text.X + 4, TableColumnRect_Total_Text.Y + plusY);
                    //    gfx.DrawString("44,500.00", ContentFont, BlackBrush, TableColumnRect_Massage_Text.X - 4, TableColumnRect_Massage_Text.Y + plusY);
                    //    gfx.DrawString("300.00", ContentFont, BlackBrush, TableColumnRect_AveragePerPax_Text.X, TableColumnRect_AveragePerPax_Text.Y + plusY);
                    //    gfx.DrawString("30", ContentFont, BlackBrush, TableColumnRect_TotalWorker_Text.X + 10, TableColumnRect_TotalWorker_Text.Y + plusY);
                    //    gfx.DrawString("600.00", ContentFont, BlackBrush, TableColumnRect_OilIncome_Text.X, TableColumnRect_OilIncome_Text.Y + plusY);
                    //    gfx.DrawString("45,100.00", ContentFont, BlackBrush, TableColumnRect_TotalIncome_Text.X + 8, TableColumnRect_TotalIncome_Text.Y + plusY);
                    //    gfx.DrawString("20,000.00", ContentFont, BlackBrush, TableColumnRect_PayWorker_Text.X - 3, TableColumnRect_PayWorker_Text.Y + plusY);
                    //    gfx.DrawString("25,100.00", ContentFont, BlackBrush, TableColumnRect_BalanceNet_Text.X, TableColumnRect_BalanceNet_Text.Y + plusY);
                    //}
                }
            }

            int netTotalPax = 0;
            int netMassageAmount = 0;
            int netMassageCreditAmount = 0;
            int netVoucherAmount = 0;
            int netAveragePerPax = 0;
            int netTotalWorker = 0;
            int netOil = 0;
            int netTotalIncome = 0;
            int netCommis = 0;
            int netCancelledPax = 0;
            int netUniform = 0;
            int netTigerBalm = 0;
            int netBalanceNet = 0;
            for (int k = 0; k < allDailyForm.Count; k++)
            {
                string convertTotalPax = allDailyForm[k].TotalPax.Replace(".00", "");
                string convertMassageAmount = allDailyForm[k].MassageAmount.Replace(".00", "");
                string convertMassageCreditAmount = allDailyForm[k].MassageCreditAmount.Replace(".00", "");
                string convertMassageVoucherAmount = allDailyForm[k].MassageVoucherAmount.Replace(".00", "");
                string convertAveragePerPax = allDailyForm[k].AveragePerPax.Replace(".00", "");
                string convertTotalWorker = allDailyForm[k].TotalWorker.Replace(".00", "");
                string convertOilAmount = allDailyForm[k].OilAmount.Replace(".00", "");
                string convertTotalIncome = allDailyForm[k].TotalIncome.Replace(".00", "");
                string convertCommis = allDailyForm[k].PayWorkers.Replace(".00", "");
                string convertCancelledPax = allDailyForm[k].TotalCancelled.Replace(".00", "");
                string convertUniform = allDailyForm[k].TotalUniform.Replace(".00", "");
                string convertTigerBalm = allDailyForm[k].TotalTigerBalm.Replace(".00", "");
                string convertBalance = allDailyForm[k].BalanceNet.Replace(".00", "");

                string convertTotalPaxs = convertTotalPax.Replace(",", "");
                string convertMassageAmounts = convertMassageAmount.Replace(",", "");
                string convertMassageCreditAmounts = convertMassageCreditAmount.Replace(",", "");
                string convertMassageVoucherAmounts = convertMassageVoucherAmount.Replace(",", "");
                string convertAveragePerPaxs = convertAveragePerPax.Replace(",", "");
                string convertTotalWorkers = convertTotalWorker.Replace(",", "");
                string convertOilAmounts = convertOilAmount.Replace(",", "");
                string convertTotalIncomes = convertTotalIncome.Replace(",", "");
                string convertCommiss = convertCommis.Replace(",", "");
                string convertCancelledPaxs = convertCancelledPax.Replace(",", "");
                string convertUniforms = convertUniform.Replace(",", "");
                string convertTigerBalms = convertTigerBalm.Replace(",", "");
                string convertBalances = convertBalance.Replace(",", "");

                netTotalPax += Int32.Parse(convertTotalPaxs);
                netMassageAmount += Int32.Parse(convertMassageAmounts);
                netMassageCreditAmount += Int32.Parse(convertMassageCreditAmounts);
                netVoucherAmount += Int32.Parse(convertMassageVoucherAmounts);
                netAveragePerPax += Int32.Parse(convertAveragePerPaxs);
                netTotalWorker += Int32.Parse(convertTotalWorkers);
                netOil += Int32.Parse(convertOilAmounts);
                netTotalIncome += Int32.Parse(convertTotalIncomes);
                netCommis += Int32.Parse(convertCommiss);
                netCancelledPax += Int32.Parse(convertCancelledPaxs);
                netUniform += Int32.Parse(convertUniforms);
                netTigerBalm += Int32.Parse(convertTigerBalms);
                netBalanceNet += Int32.Parse(convertBalances);
            }

            gfx.DrawString("Total", ContentFont, BlackBrush, TableColumnRect_Date_Text.X + 7, TableColumnRect_Date_Text.Y + 426);
            //gfx.DrawString("6,000.00", ContentFont, BlackBrush, TableColumnRect_InitialMoney_Text.X, TableColumnRect_InitialMoney_Text.Y + 426);
            gfx.DrawString(String.Format("{0:n0}", netTotalPax), ContentFont, BlackBrush, TableColumnRect_Total_Text.X + 8, TableColumnRect_Total_Text.Y + 426);
            gfx.DrawString(String.Format("{0:n}", netMassageAmount), ContentFont, BlackBrush, TableColumnRect_Massage_Text.X - 4, TableColumnRect_Massage_Text.Y + 426);
            gfx.DrawString(String.Format("{0:n}", netMassageCreditAmount), ContentFont, BlackBrush, TableColumnRect_Massage_Credit_Text.X - 4, TableColumnRect_Massage_Credit_Text.Y + 426);
            gfx.DrawString(String.Format("{0:n}", netVoucherAmount), ContentFont, BlackBrush, TableColumnRect_Massage_Voucher_Text.X - 4, TableColumnRect_Massage_Voucher_Text.Y + 426);
            gfx.DrawString(String.Format("{0:n}", netAveragePerPax), ContentFont, BlackBrush, TableColumnRect_AveragePerPax_Text.X - 3, TableColumnRect_AveragePerPax_Text.Y + 426);
            gfx.DrawString(netTotalWorker.ToString(), ContentFont, BlackBrush, TableColumnRect_TotalWorker_Text.X + 15, TableColumnRect_TotalWorker_Text.Y + 426);
            gfx.DrawString(String.Format("{0:n}", netOil), ContentFont, BlackBrush, TableColumnRect_OilIncome_Text.X, TableColumnRect_OilIncome_Text.Y + 426);
            gfx.DrawString(String.Format("{0:n}", netTotalIncome), ContentFont, BlackBrush, TableColumnRect_TotalIncome_Text.X + 8, TableColumnRect_TotalIncome_Text.Y + 426);
            gfx.DrawString(String.Format("{0:n}", netCommis), ContentFont, BlackBrush, TableColumnRect_PayWorker_Text.X - 5, TableColumnRect_PayWorker_Text.Y + 426);
            gfx.DrawString(netCancelledPax.ToString(), ContentFont, BlackBrush, TableColumnRect_TotalCancelled_Text.X + 20, TableColumnRect_TotalCancelled_Text.Y + 426);
            gfx.DrawString(String.Format("{0:n}", netUniform), ContentFont, BlackBrush, TableColumnRect_TotalUniform_Text.X + 5, TableColumnRect_TotalUniform_Text.Y + 426);
            gfx.DrawString(String.Format("{0:n}", netTigerBalm), ContentFont, BlackBrush, TableColumnRect_TotalTigerBalm_Text.X + 10, TableColumnRect_TotalTigerBalm_Text.Y + 426);
            gfx.DrawString(String.Format("{0:n}", netBalanceNet), ContentFont, BlackBrush, TableColumnRect_BalanceNet_Text.X + 5, TableColumnRect_BalanceNet_Text.Y + 426);
            gfx.DrawLine(XPens.Black, 10, 529, 780, 530);
            //MessageBox.Show(dateStamp.ToString()+"//"+dateStamp.ToLongDateString());

            fullDate = new DateTime(Int32.Parse(sGetLatestMonth[0]), Int32.Parse(sGetLatestMonth[1]), Int32.Parse(sGetLatestMonth[2])).ToString("ddMMMMyyyy");

            //string[] longDate = dateStamp.ToLongDateString().Split(' ');
            //string preReal = longDate[2] + longDate[1] + longDate[3];
            //string realDate = preReal.Replace(",", "");
            filename25 = @"C:\SpaSystem\report" + fullDate + "_S.pdf";

            //test
            documents.Save(filename25);
            //Application.Current.Shutdown();
            //Process.Start(filename);
            //for test ***********************
            try
            {
                string curDateTime = getCurDateTime();

                //Account getLastAc = this.db.getLatestAcount();
                //Account newAc = new Account()
                //{
                //    Id = getLastAc.Id,
                //    Date = getLastAc.Date,
                //    Time = getLastAc.Time,
                //    StartMoney = getLastAc.StartMoney,
                //    StaffAmount = getLastAc.StaffAmount,
                //    Completed = "true",
                //    SendStatus = getLastAc.SendStatus,
                //    UpdateStatus = getLastAc.UpdateStatus,
                //    CreateDateTime = getLastAc.CreateDateTime,
                //    UpdateDateTime = curDateTime
                //};

                //this.db.updateAcount(newAc);

                //loadingGrid.Visibility = Visibility.Visible;
                //loadingTxt.Text = "Computer กำลังปิด โปรดรอสักครู่...";

                await Task.Delay(2000);

                //test

                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient(GlobalValue.Instance.emailServer);
                mail.From = new MailAddress(GlobalValue.Instance.senderEmail);
                String[] receiverSet = this.db.getCurrentReceiverEmail().Value.Split('/');
                for (int i = 0; i < receiverSet.Length; i++)
                {
                    mail.To.Add(receiverSet[i]);
                }
                mail.Subject = currentBranchName + " - Daily Report(" + fullDate + ")";
                mail.Body = "This daily report email is auto sent by Spa POS Program (" + currentBranchName + ")";

                Attachment attachment;
                attachment = new Attachment(filename);
                Attachment attachment25;
                attachment25 = new Attachment(filename25);
                Attachment attachment25Details;
                attachment25Details = new Attachment(filenameDetail25);

                if (GlobalValue.Instance.report100.Equals("true"))
                {
                    mail.Attachments.Add(attachment);
                }
                if (GlobalValue.Instance.report25.Equals("true"))
                {
                    mail.Attachments.Add(attachment25);
                }
                if (GlobalValue.Instance.reportDetail.Equals("true"))
                {
                    mail.Attachments.Add(attachment25Details);
                }

                //mail.Attachments.Add(attachment);
                //mail.Attachments.Add(attachment25);
                //mail.Attachments.Add(attachment25Details);

                SmtpServer.Port = GlobalValue.Instance.serverPort;
                SmtpServer.Credentials = new NetworkCredential(GlobalValue.Instance.serverUsername, GlobalValue.Instance.serverPassword);
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);
                //MessageBox.Show("Email is sent\nEmail ถูกส่งเรียบร้อย");

                ////////////////////////////////////////////////////////////////////////////////////////////////////////
                //Private send DB to Jaturong
                //MailMessage _mail = new MailMessage();
                //SmtpClient _SmtpServer = new SmtpClient("smtp.sendgrid.net");
                //_mail.From = new MailAddress("jaturong@24dvlop.com");
                //_mail.To.Add("t.jaturong@outlook.com");
                //_mail.Subject = currentBranchName + " - Master DB(" + fullDate + ")";
                //_mail.Body = "This daily master DB by Spa POS program (" + currentBranchName + ")";

                //Attachment _attachment;
                //_attachment = new Attachment(filename);

                //_mail.Attachments.Add(_attachment);

                //_SmtpServer.Port = 587;
                //_SmtpServer.Credentials = new NetworkCredential("apikey", "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");
                ////_SmtpServer.EnableSsl = true;

                //_SmtpServer.Send(_mail);
                ////////////////////////////////////////////////////////////////////////////////////////////////////////
                
                Application.Current.Shutdown();

                //test**********************************
                var psi = new ProcessStartInfo("shutdown", "/s /t 0");
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                Process.Start(psi);


            }
            catch (Exception pp)
            {
                //MessageBox.Show(pp.ToString());
                MessageBox.Show("ไม่สามารถส่ง Email ได้เนื่องจากไม่มี Internet กรุณาติดต่อผู้ดูแลระบบ");


                Application.Current.Shutdown();

                //test************************
                var psi = new ProcessStartInfo("shutdown", "/s /t 0");
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                Process.Start(psi);


            }
            //Process.Start(filename);

        }

        public string getListItemInInvoice()
        {
            string text = "";
            int discountValue = 0;

            foreach (OrderRecord o in prepareOrder)
            {
                text += "- " + this.db.getMassageTopicName(o.MassageTopicId) +"("+this.db.getMassagePlanName(o.MassagePlanId)+")"+"\n  " + String.Format("{0:n}", Int32.Parse(o.Price)) + " Baht\n\n";
            }

            if(prepareDiscount.Count()>0)
            {
                foreach (DiscountRecord p in prepareDiscount)
                {
                    discountValue = Int32.Parse(this.db.getDiscountMasterDetailFromId(p.DiscountMasterDetailId).Value);
                    text += "- " + this.db.getDiscountMasterDetailFromId(p.DiscountMasterDetailId).Name + "\n  " + "-"+String.Format("{0:n}", discountValue) + " Baht\n\n";
                }
            }

            return text;
        }

        public int getTotalSale()
        {
            int sale = 0;
            List<OrderRecord> listOrders = this.db.getAllOrderRecordExceptCancelled(currentUseAccountId);
            foreach (OrderRecord o in listOrders)
            {
                sale += Int32.Parse(o.Price);
            }

            return sale;
        }

        public int getTotalSaleFromId(int AccountId)
        {
            int sale = 0;
            List<OrderRecord> listOrders = this.db.getAllOrderCashRecordExceptCancelled(AccountId);
            foreach (OrderRecord o in listOrders)
            {
                sale += Int32.Parse(o.Price);
            }

            return sale;
        }

        public int getTotalCreditSaleFromId(int AccountId)
        {
            int sale = 0;
            List<OrderRecord> listOrders = this.db.getAllOrderCreditRecordExceptCancelled(AccountId);
            foreach (OrderRecord o in listOrders)
            {
                sale += Int32.Parse(o.Price);
            }

            return sale;
        }

        public int getTotalSaleFromId25(int AccountId)
        {
            int sale = 0;
            List<OrderRecord> listOrders = this.db.getAllOrderRecordExceptCancelled25(AccountId);
            foreach (OrderRecord o in listOrders)
            {
                sale += Int32.Parse(o.Price);
            }

            return sale;
        }

        public int getTotalCommission()
        {
            int com = 0;
            List<OrderRecord> listOrders = this.db.getAllOrderRecordExceptCancelled(currentUseAccountId);
            foreach (OrderRecord o in listOrders)
            {
                com += Int32.Parse(o.Commission);
            }

            return com;
        }

        public int getGrandTotalOtherRecord()
        {
            int com = 0;
            List<OtherSaleRecord> listOrders = this.db.getGrandOtherSaleRecordExceptCancelled(currentUseAccountId);
            foreach (OtherSaleRecord o in listOrders)
            {
                com += Int32.Parse(o.Price);
            }

            return com;
        }

        public int getTotalCommissionFromId(int AccountId)
        {
            int com = 0;
            List<OrderRecord> listOrders = this.db.getAllOrderRecordExceptCancelled(AccountId);
            foreach (OrderRecord o in listOrders)
            {
                com += Int32.Parse(o.Commission);
            }

            return com;
        }

        public int getTotalCommissionFromId25(int AccountId)
        {
            int com = 0;
            List<OrderRecord> listOrders = this.db.getAllOrderRecordExceptCancelled25(AccountId);
            foreach (OrderRecord o in listOrders)
            {
                com += Int32.Parse(o.Commission);
            }

            return com;
        }

        public int getTotalPaxFromId(int AccountId)
        {
            List<OrderRecord> recordNum = this.db.getAllOrderRecordExceptCancelled(AccountId);

            return recordNum.Count();
        }

        public int getTotalPaxFromId25(int AccountId)
        {
            List<OrderRecord> recordNum = this.db.getAllOrderRecordExceptCancelled25(AccountId);

            return recordNum.Count();
        }

        public int getTotalCancelledPaxFromId(int AccountId)
        {
            List<OrderRecord> recordCancelledNum = this.db.getAllCancelledOrderRecord(AccountId);

            return recordCancelledNum.Count();
        }

        public int getTotalUniformFromId(int AccountId)
        {
            int com = 0;
            List<OtherSaleRecord> listOrders = this.db.getAllUniformRecordExceptCancelled(AccountId);
            foreach (OtherSaleRecord o in listOrders)
            {
                com += Int32.Parse(o.Price);
            }

            return com;
        }

        public int getTotalTigerBalmFromId(int AccountId) //edit on 3 Nov 2019
        {
            int comS = 0;
            int comB = 0;

            List<OtherSaleRecord> listOrders = this.db.getAllSmallTigerBalmRecordExceptCancelled(AccountId);
            foreach (OtherSaleRecord o in listOrders)
            {
                comS += Int32.Parse(o.Price);
            }

            List<OtherSaleRecord> listOrdersB = this.db.getAllBigTigerBalmRecordExceptCancelled(AccountId);
            foreach (OtherSaleRecord ob in listOrdersB)
            {
                comB += Int32.Parse(ob.Price);
            }

            return comS+comB;
        }

        public int getTotalTigerBalmWithOtherSaleFromId(int AccountId) //edit on 3 Nov 2019
        {
            int comS = 0;
            int comB = 0;
            int otherS = 0;

            List<OtherSaleRecord> listOrders = this.db.getAllSmallTigerBalmRecordExceptCancelled(AccountId);
            foreach (OtherSaleRecord o in listOrders)
            {
                comS += Int32.Parse(o.Price);
            }

            List<OtherSaleRecord> listOrdersB = this.db.getAllBigTigerBalmRecordExceptCancelled(AccountId);
            foreach (OtherSaleRecord ob in listOrdersB)
            {
                comB += Int32.Parse(ob.Price);
            }

            List<OtherSaleRecord> listOrdersOther = this.db.getAllOtherSaleRecordExceptCancelled(AccountId);
            foreach (OtherSaleRecord os in listOrdersOther)
            {
                otherS += Int32.Parse(os.Price);
            }

            return comS + comB + otherS;
        }

        public void clearAllSelectedAndBalance()
        {
            MassageSetContainer.Children.Clear();
            voucherContainer.Children.Clear();
            InitialInterface();
            currentBalance = 0;
            finalBalance = 0;
            currentBalanceTxt.Text = "ราคาทั้งหมด / Total";
            prepareOrder.Clear();
            prepareDiscount.Clear();
            checkoutAndClearGrid.Visibility = Visibility.Collapsed;
        }

        public static bool CheckInternet()
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (var stream = client.OpenRead("http://www.google.com"))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public void SendTextToMonitor(String[] tagArray)
        {
            int getBranchId = this.db.getBranch().Id;
            if (getBranchId == 7)
            {
                try
                {
                    Slip sps = new Slip();
                    string getFullName = this.db.getMassageTopicName(Int32.Parse(tagArray[0])) + "(" + this.db.getMassagePlanName(Int32.Parse(tagArray[1])) + ")";

                    portaserial = new SerialPort(GlobalValue.Instance.monitorComPort, GlobalValue.Instance.monitorBaudRate, Parity.None, 8, StopBits.One);
                    portaserial.Open();

                    byte[] bytestosend = { 0x0C };
                    portaserial.Write(bytestosend, 0, bytestosend.Length);

                    //byte[] bytestosendFirst = { 0x1F, 03 };
                    //portaserial.Write(bytestosendFirst, 0, bytestosendFirst.Length);

                    if (getFullName.Length > 18)
                    {
                        getFullName = getFullName.Remove(18, getFullName.Length - 18);
                        getFullName = getFullName + "..             ";
                    }
                    if (getFullName.Length == 11)
                    {
                        getFullName = getFullName + "                      ";
                    }
                    if (getFullName.Length == 12)
                    {
                        getFullName = getFullName + "                     ";
                    }
                    if (getFullName.Length == 13)
                    {
                        getFullName = getFullName + "                    ";
                    }
                    if (getFullName.Length == 14)
                    {
                        getFullName = getFullName + "                   ";
                    }
                    if (getFullName.Length == 15)
                    {
                        getFullName = getFullName + "                  ";
                    }
                    if (getFullName.Length == 16)
                    {
                        getFullName = getFullName + "                 ";
                    }
                    if (getFullName.Length == 17)
                    {
                        getFullName = getFullName + "                ";
                    }
                    if (getFullName.Length == 18)
                    {
                        getFullName = getFullName + "               ";
                    }
                    //await Task.Delay(500);
                    portaserial.WriteLine(getFullName);

                    byte[] bytestosendSecond = { 0x1F, 10 };//, 10 };
                    portaserial.Write(bytestosendSecond, 0, bytestosendSecond.Length);
                    //await Task.Delay(500);
                    portaserial.WriteLine(tagArray[2] + ".00");
                    portaserial.DiscardInBuffer();
                    portaserial.DiscardOutBuffer();
                    portaserial.Close();
                    portaserial.Dispose();
                    portaserial = null;
                }
                catch (Exception io)
                {

                }
            }
            else
            {
                try
                {
                    Slip sps = new Slip();
                    string getFullName = this.db.getMassageTopicName(Int32.Parse(tagArray[0])) + "(" + this.db.getMassagePlanName(Int32.Parse(tagArray[1])) + ")";

                    portaserial = new SerialPort(GlobalValue.Instance.monitorComPort, GlobalValue.Instance.monitorBaudRate, Parity.None, 8, StopBits.One);
                    portaserial.Open();

                    byte[] bytestosend = { 0x0C };
                    portaserial.Write(bytestosend, 0, bytestosend.Length);

                    //byte[] bytestosendFirst = { 0x1F, 03 };
                    //portaserial.Write(bytestosendFirst, 0, bytestosendFirst.Length);

                    if (getFullName.Length > 18)
                    {
                        getFullName = getFullName.Remove(18, getFullName.Length - 18);
                        getFullName = getFullName + "..";
                    }
                    //await Task.Delay(500);
                    portaserial.WriteLine(getFullName);

                    byte[] bytestosendSecond = { 0x1F, 0x24, 14, 2 };
                    portaserial.Write(bytestosendSecond, 0, bytestosendSecond.Length);
                    //await Task.Delay(500);
                    portaserial.WriteLine(tagArray[2] + ".00");
                    portaserial.DiscardInBuffer();
                    portaserial.DiscardOutBuffer();
                    portaserial.Close();
                    portaserial.Dispose();
                    portaserial = null;
                }
                catch (Exception io)
                {

                }
            }
            
        }

        public void SendTextTotal()
        {
            int getBranchId = this.db.getBranch().Id;
            if (getBranchId == 7)
            {
                try
                {
                    portaserial = new SerialPort(GlobalValue.Instance.monitorComPort, GlobalValue.Instance.monitorBaudRate, Parity.None, 8, StopBits.One);
                    portaserial.Open();

                    byte[] bytestosend = { 0x0C };
                    portaserial.Write(bytestosend, 0, bytestosend.Length);
                    //await Task.Delay(500);
                    portaserial.WriteLine("Total                          ");

                    //byte[] bytestosendSecond = { 0x1F, 0x24, 11, 2 };
                    byte[] bytestosendSecond = { 0x1F, 10 };
                    portaserial.Write(bytestosendSecond, 0, bytestosendSecond.Length);
                    //await Task.Delay(500);
                    portaserial.WriteLine(String.Format("{0:n}", currentBalance));
                    portaserial.DiscardInBuffer();
                    portaserial.DiscardOutBuffer();
                    portaserial.Close();
                    portaserial.Dispose();
                    portaserial = null;
                }
                catch (Exception io)
                {

                }
            }
            else
            {
                try
                {
                    portaserial = new SerialPort(GlobalValue.Instance.monitorComPort, GlobalValue.Instance.monitorBaudRate, Parity.None, 8, StopBits.One);
                    portaserial.Open();

                    byte[] bytestosend = { 0x0C };
                    portaserial.Write(bytestosend, 0, bytestosend.Length);
                    //await Task.Delay(500);
                    portaserial.WriteLine("Total");

                    byte[] bytestosendSecond = { 0x1F, 0x24, 13, 2 };
                    portaserial.Write(bytestosendSecond, 0, bytestosendSecond.Length);
                    //await Task.Delay(500);
                    portaserial.WriteLine(String.Format("{0:n}", currentBalance));
                    portaserial.DiscardInBuffer();
                    portaserial.DiscardOutBuffer();
                    portaserial.Close();
                    portaserial.Dispose();
                    portaserial = null;
                }
                catch (Exception io)
                {

                }
            }
            
        }

        public void SendTextTotalWithDiscount()
        {
            int getBranchId = this.db.getBranch().Id;
            if (getBranchId == 7)
            {
                try
                {
                    portaserial = new SerialPort(GlobalValue.Instance.monitorComPort, GlobalValue.Instance.monitorBaudRate, Parity.None, 8, StopBits.One);
                    portaserial.Open();

                    byte[] bytestosend = { 0x0C };
                    portaserial.Write(bytestosend, 0, bytestosend.Length);
                    //await Task.Delay(500);
                    portaserial.WriteLine("Total                          ");

                    //byte[] bytestosendSecond = { 0x1F, 0x24, 11, 2 };
                    byte[] bytestosendSecond = { 0x1F, 10 };
                    portaserial.Write(bytestosendSecond, 0, bytestosendSecond.Length);
                    //await Task.Delay(500);
                    portaserial.WriteLine(String.Format("{0:n}", finalBalance));
                    portaserial.DiscardInBuffer();
                    portaserial.DiscardOutBuffer();
                    portaserial.Close();
                    portaserial.Dispose();
                    portaserial = null;
                }
                catch (Exception io)
                {

                }
            }
            else
            {
                try
                {
                    portaserial = new SerialPort(GlobalValue.Instance.monitorComPort, GlobalValue.Instance.monitorBaudRate, Parity.None, 8, StopBits.One);
                    portaserial.Open();

                    byte[] bytestosend = { 0x0C };
                    portaserial.Write(bytestosend, 0, bytestosend.Length);
                    //await Task.Delay(500);
                    portaserial.WriteLine("Total");

                    byte[] bytestosendSecond = { 0x1F, 0x24, 13, 2 };
                    portaserial.Write(bytestosendSecond, 0, bytestosendSecond.Length);
                    //await Task.Delay(500);
                    portaserial.WriteLine(String.Format("{0:n}", finalBalance));
                    portaserial.DiscardInBuffer();
                    portaserial.DiscardOutBuffer();
                    portaserial.Close();
                    portaserial.Dispose();
                    portaserial = null;
                }
                catch (Exception io)
                {

                }
            }

        }

        public void ClearText()
        {
            int getBranchId = this.db.getBranch().Id;
            if (getBranchId == 7)
            {
                try
                {
                    portaserial = new SerialPort(GlobalValue.Instance.monitorComPort, GlobalValue.Instance.monitorBaudRate, Parity.None, 8, StopBits.One);
                    portaserial.Open();

                    byte[] bytestosend = { 0x0C };
                    portaserial.Write(bytestosend, 0, bytestosend.Length);
                    //await Task.Delay(500);
                    portaserial.WriteLine(GlobalValue.Instance.branchNameInMonitor);
                    portaserial.WriteLine("\n  **  WELCOME  **");

                    portaserial.DiscardInBuffer();
                    portaserial.DiscardOutBuffer();
                    portaserial.Close();
                    portaserial.Dispose();
                    portaserial = null;

                    transactionLoadingGrid.Visibility = Visibility.Collapsed;
                }
                catch (Exception io)
                {

                }
            }
            else
            {
                try
                {
                    portaserial = new SerialPort(GlobalValue.Instance.monitorComPort, GlobalValue.Instance.monitorBaudRate, Parity.None, 8, StopBits.One);
                    portaserial.Open();

                    byte[] bytestosend = { 0x0C };
                    portaserial.Write(bytestosend, 0, bytestosend.Length);
                    //await Task.Delay(500);
                    portaserial.WriteLine(GlobalValue.Instance.branchNameInMonitor);
                    portaserial.WriteLine("\n  **  WELCOME  **");

                    portaserial.DiscardInBuffer();
                    portaserial.DiscardOutBuffer();
                    portaserial.Close();
                    portaserial.Dispose();
                    portaserial = null;

                    transactionLoadingGrid.Visibility = Visibility.Collapsed;
                }
                catch (Exception io)
                {

                }
            }
            
        }

        public int RunCommandForInsert(string Query)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["MyConn"].ConnectionString);
            SqlCommand com = new SqlCommand(Query, con);
            con.Open();
            int res = com.ExecuteNonQuery();
            con.Close();
            return res;
        }

        public DataSet RunCommand(string QueryStr, string TableName)
        {
            string conn = ConfigurationManager.ConnectionStrings["MyConn"].ConnectionString;
            string query = QueryStr;
            SqlConnection connection = new SqlConnection(conn);
            SqlDataAdapter dadapter = new SqlDataAdapter(query, connection);

            DataSet ds = new DataSet();
            connection.Open();
            dadapter.Fill(ds, TableName);
            connection.Close();

            return ds;
            //SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["MyConn"].ConnectionString);
            //SqlCommand com = new SqlCommand(Query, con);
            //con.Open();
            //int res = com.ExecuteNonQuery();
            //con.Close();
            //return res;
        }

        public string getCurDateTime()
        {
            DateTime current = DateTime.Now;
            string curDateTime = current.ToString("yyyy-MM-dd HH:mm:ss");

            return curDateTime;
        }

        public async void InsertAccountToServer()
        {
            Account act = this.db.getLatestAcount();
            AccountSerialize acts = new AccountSerialize()
            {
                Id = act.Id,
                BranchId = this.db.getBranch().Id,
                Date = act.Date,
                Time = act.Time,
                StaffAmount = act.StaffAmount,
                StartMoney = act.StartMoney,
                CreateDateTime = act.CreateDateTime,
                UpdateDateTime = act.UpdateDateTime
            };

            try
            {
                var obj = new SerializeClass
                {
                    AccountData = acts
                };

                string serializeString = JsonConvert.SerializeObject(obj);

                string sendDataUrl = GlobalValue.Instance.Url_SendData;

                var client = new HttpClient();
                var values = new Dictionary<string, string>
                {
                    {"data",serializeString }
                };
                var content = new MyFormUrlEncodedContent(values);
                var response = await client.PostAsync(sendDataUrl, content);
                var resultAuthen = await response.Content.ReadAsStringAsync();

                var parseJson = JObject.Parse(resultAuthen);

                string checkStatus = (string)parseJson["Status"];
                if (checkStatus.Equals("true"))
                {
                    act.SendStatus = "true";
                    act.UpdateDateTime = getCurDateTime();
                    this.db.updateAcount(act);
                }
                else
                {
                    //MessageBox.Show("insert account fail"+"\nError : "+ (string)parseJson["Error_Message"]);
                }

            }
            catch (Exception ex)
            {
                //MessageBox.Show("insert account fail" + "\nError : " + ex.ToString());
            }
        }

        public async void UpdateAccountToServer()
        {
            Account act = this.db.getLatestAcount();

            if (act.SendStatus.Equals("true"))
            {
                //Do Update
                try
                {
                    AccountSerialize acts = new AccountSerialize()
                    {
                        Id = act.Id,
                        BranchId = this.db.getBranch().Id,
                        Date = act.Date,
                        Time = act.Time,
                        StaffAmount = act.StaffAmount,
                        StartMoney = act.StartMoney,
                        CreateDateTime = act.CreateDateTime,
                        UpdateDateTime = act.UpdateDateTime
                    };

                    var obj = new AccountUpdateSerialize
                    {
                        AccountData = acts
                    };

                    string serializeString = JsonConvert.SerializeObject(obj);

                    string updateAccountUrl = GlobalValue.Instance.Url_UpdateAccount;
                    var client = new HttpClient();
                    var values = new Dictionary<string, string>
                    {
                        {"accountData" ,serializeString}
                    };
                    var content = new FormUrlEncodedContent(values);
                    var response = await client.PostAsync(updateAccountUrl, content);
                    var resultAuthen = await response.Content.ReadAsStringAsync();

                    var parseJson = JObject.Parse(resultAuthen);

                    string checkStatus = (string)parseJson["Status"];
                    if (checkStatus.Equals("true"))
                    {
                        act.UpdateStatus = "true";
                        act.UpdateDateTime = getCurDateTime();
                        this.db.updateAcount(act);
                    }
                    else
                    {
                        act.UpdateStatus = "false";
                        act.UpdateDateTime = getCurDateTime();
                        this.db.updateAcount(act);
                        //MessageBox.Show("update account fail" + "\nError : " + (string)parseJson["Error_Message"]);
                    }

                }
                catch (Exception ex)
                {
                    //MessageBox.Show("update account fail \nError : " + ex.Message.ToString());
                }
            }
            else
            {
                //Do Save
                InsertAccountToServer();
            }

            transactionLoadingGrid.Visibility = Visibility.Collapsed;
        }

        public async void InsertOrderRecordToServer(OrderRecord getOrd)
        {
            List<OrderRecordSerialize> listOrders = new List<OrderRecordSerialize>();
            OrderRecordSerialize ords = new OrderRecordSerialize()
            {
                Id = getOrd.Id,
                BranchId = this.db.getBranch().Id,
                AccountId = getOrd.AccountId,
                Date = getOrd.Date,
                Time = getOrd.Time,
                MassageTopicId = getOrd.MassageTopicId,
                MassagePlanId = getOrd.MassagePlanId,
                Price = getOrd.Price,
                Commission = getOrd.Commission,
                IsCreditCard = getOrd.IsCreditCard,
                CancelStatus = getOrd.CancelStatus,
                CreateDateTime = getOrd.CreateDateTime,
                UpdateDateTime = getOrd.UpdateDateTime
            };

            listOrders.Add(ords);

            try
            {
                var obj = new SerializeClass
                {
                    OrderRecordList = listOrders
                };

                string serializeString = JsonConvert.SerializeObject(obj);

                string sendDataUrl = GlobalValue.Instance.Url_SendData;

                var client = new HttpClient();
                var values = new Dictionary<string, string>
                {
                    {"data",serializeString }
                };
                var content = new MyFormUrlEncodedContent(values);
                var response = await client.PostAsync(sendDataUrl, content);
                var resultAuthen = await response.Content.ReadAsStringAsync();

                var parseJson = JObject.Parse(resultAuthen);

                string checkStatus = (string)parseJson["Status"];
                if (checkStatus.Equals("true"))
                {
                    OrderRecord newForUpdate = getOrd;
                    newForUpdate.SendStatus = "true";
                    newForUpdate.UpdateDateTime = getCurDateTime();
                    this.db.updateOrderRecord(newForUpdate);
                }
                else
                {
                    //MessageBox.Show("insert order fail" + "\nError : " + (string)parseJson["Error_Message"]);
                }

            }
            catch (Exception ex)
            {
                //MessageBox.Show("insert order fail" + "\nError : " + ex.ToString());
            }

        }

        public async void UpdateOrderRecordToServer(OrderRecord getOrd)
        {
            try
            {

                OrderRecordSerialize ords = new OrderRecordSerialize()
                {
                    Id = getOrd.Id,
                    BranchId = this.db.getBranch().Id,
                    AccountId = getOrd.AccountId,
                    Date = getOrd.Date,
                    Time = getOrd.Time,
                    MassageTopicId = getOrd.MassageTopicId,
                    MassagePlanId = getOrd.MassagePlanId,
                    Price = getOrd.Price,
                    Commission = getOrd.Commission,
                    IsCreditCard = getOrd.IsCreditCard,
                    CancelStatus = getOrd.CancelStatus,
                    CreateDateTime = getOrd.CreateDateTime,
                    UpdateDateTime = getOrd.UpdateDateTime
                };

                var obj = new OrderUpdateSerializer
                {
                    OrderRecordData = ords
                };

                string serializeString = JsonConvert.SerializeObject(obj);

                string updateOrderUrl = GlobalValue.Instance.Url_UpdateOrderRecord;
                var client = new HttpClient();
                var values = new Dictionary<string, string>
                    {
                        {"orderRecordData" ,serializeString}
                    };
                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync(updateOrderUrl, content);
                var resultAuthen = await response.Content.ReadAsStringAsync();

                var parseJson = JObject.Parse(resultAuthen);

                string checkStatus = (string)parseJson["Status"];
                if (checkStatus.Equals("true"))
                {

                }
                else
                {
                    //MessageBox.Show("update order fail" + "\nError : " + (string)parseJson["Error_Message"]);
                }

            }
            catch (Exception ex)
            {
                //MessageBox.Show("update order fail \nError : " + ex.Message.ToString());
            }

        }

        public async void InsertOtherSaleOrderRecordToServer(OtherSaleRecord osrd)
        {
            OtherSaleRecordSerialize ords = new OtherSaleRecordSerialize()
            {
                Id = osrd.Id,
                BranchId = this.db.getBranch().Id,
                AccountId = osrd.AccountId,
                Date = osrd.Date,
                Time = osrd.Time,
                OtherSaleId = osrd.OtherSaleId,
                Price = osrd.Price,
                IsCreditCard = osrd.IsCreditCard,
                CancelStatus = osrd.CancelStatus,
                CreateDateTime = osrd.CreateDateTime,
                UpdateDateTime = osrd.UpdateDateTime
            };

            try
            {
                var obj = new SerializeClassForOtherSale
                {
                    OtherSaleRecordData = ords
                };

                string serializeString = JsonConvert.SerializeObject(obj);

                string sendDataUrl = GlobalValue.Instance.Url_SendOtherSaleData;

                var client = new HttpClient();
                var values = new Dictionary<string, string>
                {
                    {"data",serializeString }
                };
                var content = new MyFormUrlEncodedContent(values);
                var response = await client.PostAsync(sendDataUrl, content);
                var resultAuthen = await response.Content.ReadAsStringAsync();

                var parseJson = JObject.Parse(resultAuthen);

                string checkStatus = (string)parseJson["Status"];
                if (checkStatus.Equals("true"))
                {
                    OtherSaleRecord newForUpdate = osrd;
                    newForUpdate.SendStatus = "true";
                    newForUpdate.UpdateDateTime = getCurDateTime();
                    this.db.updateOtherSaleRecord(newForUpdate);
                }
                else
                {
                    //MessageBox.Show("insert order fail" + "\nError : " + (string)parseJson["Error_Message"]);
                }

            }
            catch (Exception ex)
            {
                //MessageBox.Show("insert order fail" + "\nError : " + ex.ToString());
            }

        }

        public async void InsertDiscountRecordToServer(DiscountRecord dcrd)
        {
            DiscountRecordSerialize dcsr = new DiscountRecordSerialize()
            {
                Id = dcrd.Id,
                BranchId = this.db.getBranch().Id,
                AccountId = dcrd.AccountId,
                Date = dcrd.Date,
                Time = dcrd.Time,
                DiscountMasterId = dcrd.DiscountMasterId,
                DiscountMasterDetailId = dcrd.DiscountMasterDetailId,
                Value = dcrd.Value,
                IsCreditCard = dcrd.IsCreditCard,
                CancelStatus = dcrd.CancelStatus,
                CreateDateTime = dcrd.CreateDateTime,
                UpdateDateTime = dcrd.UpdateDateTime
            };

            try
            {
                var obj = new SerializeClassForDiscount
                {
                    DiscountRecordData = dcsr
                };

                string serializeString = JsonConvert.SerializeObject(obj);

                string sendDataUrl = GlobalValue.Instance.Url_SendDiscountData;

                var client = new HttpClient();
                var values = new Dictionary<string, string>
                {
                    {"data",serializeString }
                };
                var content = new MyFormUrlEncodedContent(values);
                var response = await client.PostAsync(sendDataUrl, content);
                var resultAuthen = await response.Content.ReadAsStringAsync();

                var parseJson = JObject.Parse(resultAuthen);

                string checkStatus = (string)parseJson["Status"];
                if (checkStatus.Equals("true"))
                {
                    DiscountRecord newForUpdate = dcrd;
                    newForUpdate.SendStatus = "true";
                    newForUpdate.UpdateDateTime = getCurDateTime();
                    this.db.updateDiscountRecord(newForUpdate);
                }
                else
                {
                    //MessageBox.Show("insert order fail" + "\nError : " + (string)parseJson["Error_Message"]);
                }

            }
            catch (Exception ex)
            {
                //MessageBox.Show("insert order fail" + "\nError : " + ex.ToString());
            }

        }

        public async void InsertOrderRecordWithDiscountToServer(OrderRecordWithDiscount orderRecWithDisC)
        {
            OrderRecordWithDiscountSerialize orderRecWithDiscSerialize = new OrderRecordWithDiscountSerialize()
            {
                Id = orderRecWithDisC.Id,
                BranchId = this.db.getBranch().Id,
                AccountId = orderRecWithDisC.AccountId,
                OrderRecordId = orderRecWithDisC.OrderRecordId,
                DiscountRecordId = orderRecWithDisC.DiscountRecordId,
                CreateDateTime = orderRecWithDisC.CreateDateTime,
                UpdateDateTime = orderRecWithDisC.UpdateDateTime
            };

            try
            {
                var obj = new SerializeClassForOrderRecordWithDiscount
                {
                    OrderRecordWithDiscountData = orderRecWithDiscSerialize
                };

                string serializeString = JsonConvert.SerializeObject(obj);

                string sendDataUrl = GlobalValue.Instance.Url_SendOrderRecordWithDiscountData;

                var client = new HttpClient();
                var values = new Dictionary<string, string>
                {
                    {"data",serializeString }
                };
                var content = new MyFormUrlEncodedContent(values);
                var response = await client.PostAsync(sendDataUrl, content);
                var resultAuthen = await response.Content.ReadAsStringAsync();

                var parseJson = JObject.Parse(resultAuthen);

                string checkStatus = (string)parseJson["Status"];
                if (checkStatus.Equals("true"))
                {
                    OrderRecordWithDiscount newForUpdate = orderRecWithDisC;
                    newForUpdate.SendStatus = "true";
                    newForUpdate.UpdateDateTime = getCurDateTime();
                    this.db.updateOrderRecordWithDiscount(newForUpdate);
                }
                else
                {
                    //MessageBox.Show("insert order fail" + "\nError : " + (string)parseJson["Error_Message"]);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("insert order fail" + "\nError : " + ex.ToString());
            }

        }

        public string ConvertDateTime(string jsonDateTime)
        {
            string[] splitA = jsonDateTime.Split(' ');
            string[] splitB = splitA[0].Split('/');
            string convertedDateTime = splitB[2] + "-" + splitB[0] + "-" + splitB[1] + " " + splitA[1];

            return convertedDateTime;
        }
        private void cancelNumpadOtherBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            InitMoneyForOtherSaleGrid.Visibility = Visibility.Collapsed;
        }

        private void closeCode_Click(object sender, RoutedEventArgs e)
        {
            showCodeTb.Password = "";
            PasswordLockGrid.Visibility = Visibility.Collapsed;
        }


        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
