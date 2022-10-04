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
        }

        public async void CountDown()
        {
            await Task.Delay(10000);

            var newForm = new MainWindow(); //create your new form.
            newForm.Show(); //show the new form.
            this.Close(); //only if you want to close the current form.
        }

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
                MessageBox.Show("Internet ของท่านมีปัญหา ระบบไม่สามารถตรวจสอบ Version ข้อมูลล่าสุดได้");
                DeserializeJson("error");
            }
        }

        public async void DeserializeJson(string jsonStr)
        {
            GlobalValue.Instance.Json_MasterData = jsonStr;
            await Task.Delay(3000);

            var newForm = new MainWindow(); //create your new form.
            newForm.Show(); //show the new form.
            this.Close(); //only if you want to close the current form.
        }
    }
}
