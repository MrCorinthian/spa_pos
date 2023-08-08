using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Urban.Model;

namespace Urban
{
    /// <summary>
    /// Interaction logic for LandingPage.xaml
    /// </summary>
    public partial class LandingPage : Window
    {
        DBManager db;

        public LandingPage()
        {
            InitializeComponent();
            this.db = new DBManager();
            //CountDown();

            GlobalValue.Instance.CurrentSystemVersion = Int32.Parse(systemVersion.Text);
            GetBranchMasterData();
            //testTask();
        }

        public async void CountDown()
        {
            await Task.Delay(10000);

            var newForm = new MainWindow(); //create your new form.
            newForm.Show(); //show the new form.
            this.Close(); //only if you want to close the current form.
        }

        //public async void testTask()
        //{
        //    await Task.Run(() => GetBranchMasterData());
        //}

        public async void GetBranchMasterData()
        {
            try
            {
                var client = new HttpClient();
                var values = new Dictionary<string, string>
                    {
                        {"branchId", this.db.getBranch().Id.ToString()}
                    };
                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync(GlobalValue.Instance.Url_GetBranchData, content);
                var resultAuthen = await response.Content.ReadAsStringAsync();
                DeserializeJson(resultAuthen);
            }
            catch(Exception io)
            {
                //MessageBox.Show("Internet ของท่านมีปัญหา ระบบไม่สามารถตรวจสอบ Version ข้อมูลล่าสุดได้");
                DeserializeJson("error");
            }
        }

        public async void DeserializeJson(string jsonStr)
        {
            if(jsonStr.Equals("error"))
            {
                displayNameTxt.Text = "POS System";
                ImageBrush imgBrush = new ImageBrush();

                statusTxt.Text = "No internet connection";
                statusTxt.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));

                // Point to the path of the image inside the project
                imgBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/defaultBG.jpg"));
                splashGrid.Background = imgBrush;
            }
            else
            {
                //Display name and BG setup
                var parseJson = JObject.Parse(jsonStr);
                var rootSystemConfig = parseJson["SystemConfig"];
                for (int i = 0; i < rootSystemConfig.Count(); i++)
                {
                    if (rootSystemConfig[i]["Name"].ToString().Equals("SystemDisplayName"))
                    {
                        displayNameTxt.Text = rootSystemConfig[i]["Value"].ToString();
                    }
                    if (rootSystemConfig[i]["Name"].ToString().Equals("SystemDisplayNameTxtColor"))
                    {
                        String[] displayNameSplit = rootSystemConfig[i]["Value"].ToString().Split('/');
                        int _r = Int32.Parse(displayNameSplit[0]);
                        int _g = Int32.Parse(displayNameSplit[1]);
                        int _b = Int32.Parse(displayNameSplit[2]);
                        displayNameTxt.Foreground = new SolidColorBrush(Color.FromArgb(255, (byte)_r, (byte)_g, (byte)_b));
                        progressBar.Foreground = new SolidColorBrush(Color.FromArgb(255, (byte)_r, (byte)_g, (byte)_b));
                    }
                    if (rootSystemConfig[i]["Name"].ToString().Equals("LandingPageBgImage"))
                    {
                        ImageBrush imgBrush = new ImageBrush();

                        // Point to the path of the image inside the project
                        imgBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/" + rootSystemConfig[i]["Value"]));
                        splashGrid.Background = imgBrush;
                    }
                }
            }

            GlobalValue.Instance.Json_MasterData = jsonStr;
            await Task.Delay(3000);

            var newForm = new MainWindow(); //create your new form.
            newForm.Show(); //show the new form.
            this.Close(); //only if you want to close the current form.
        }
    }
}
