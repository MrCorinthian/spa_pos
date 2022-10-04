using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Urban
{
    /// <summary>
    /// Interaction logic for PlanMassage.xaml
    /// </summary>
    public partial class PlanMassage : UserControl
    {
        public int Price { get; set; }
        public int Value { get; set; }
        public int Type { get; set; }
        public string Text { get; set; }
        public int Commission { get; set; }

        private SolidColorBrush planPurple = new SolidColorBrush(Color.FromRgb(0xCC, 0xB3, 0xD3)); //ccb3d3
        private SolidColorBrush planPurpleClick = new SolidColorBrush(Color.FromRgb(0x88, 0x16, 0xa9)); //8816a9

        private SolidColorBrush planCyan = new SolidColorBrush(Color.FromRgb(0xA6, 0xE2, 0xE3)); //a6e2e3
        private SolidColorBrush planCyanClick = new SolidColorBrush(Color.FromRgb(0x0A, 0xB5, 0xBC)); // 0ab5bc

        private SolidColorBrush planBlue = new SolidColorBrush(Color.FromRgb(0xAA, 0xCD, 0xF3)); //aacdf3
        private SolidColorBrush planBlueClick = new SolidColorBrush(Color.FromRgb(0x0E, 0x64, 0xBB)); //0e64bb

        private SolidColorBrush planGreen = new SolidColorBrush(Color.FromRgb(0x8F, 0xDD, 0xAD)); //8fddad
        private SolidColorBrush planGreenClick = new SolidColorBrush(Color.FromRgb(0x0F, 0x94, 0x41)); //0f9441

        private SolidColorBrush planYellow = new SolidColorBrush(Color.FromRgb(0xFE, 0xEB, 0x9C)); //feeb9c
        private SolidColorBrush planYellowClick = new SolidColorBrush(Color.FromRgb(0xE7, 0xBC, 0x0B)); // e7bc0b

        private SolidColorBrush planOrange = new SolidColorBrush(Color.FromRgb(0xF8, 0xCA, 0xB0)); // f8cab0
        private SolidColorBrush planOrangeClick = new SolidColorBrush(Color.FromRgb(0xE8, 0x64, 0x1A)); // e8641a

        private SolidColorBrush planPink = new SolidColorBrush(Color.FromRgb(0xFF, 0xD4, 0xE5)); //ffd4e5
        private SolidColorBrush planPinkClick = new SolidColorBrush(Color.FromRgb(0xE2, 0x00, 0x52)); //e20052

        private SolidColorBrush planDarkGreen = new SolidColorBrush(Color.FromRgb(0xC0, 0xCE, 0xAD)); //c0cead
        private SolidColorBrush planDarkGreenClick = new SolidColorBrush(Color.FromRgb(0x7B, 0xAd, 0x3E)); //7bad3e

        private SolidColorBrush[] solids;
        private SolidColorBrush[] solidsClick;

        public bool state = false;

        SerialPort portaserial;

        public PlanMassage(int price, int value, int type, string text, int commission)
        {
            this.Price = price;
            this.Value = value;
            this.Type = type;
            this.Text = text;
            this.Commission = commission;
            this.solids = new SolidColorBrush[] { null, planPurple, planCyan, planBlue, planGreen, planYellow, planOrange, planPink, planDarkGreen };
            this.solidsClick = new SolidColorBrush[] { null, planPurpleClick, planCyanClick, planBlueClick, planGreenClick, planYellowClick, planOrangeClick, planPinkClick, planDarkGreenClick };
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            string text = "";
            if(this.Type == 7)
            {
                text = this.Text + "(" + this.Price.ToString() + " ฿)";
            }
            else if(this.Type == 8)
            {
                text = "+" + this.Text + "\n(" + this.Price.ToString() + " ฿)";
            }
            else
            {
                text = this.Text + "\n(" + this.Price.ToString() + " ฿)";
            }
            this.planBorder.Background = solids[this.Type];
            //if(this.Type == 0)
            //{
            //    this.planBorder.Background = planGreen;
            //    //this.planBorder.BorderThickness = new Thickness(0, 0, 2, 0);
            //    text = this.Text + "\n(" + this.Price.ToString() + " บาท)";
            //}
            //else if(this.Type == 1)
            //{
            //    this.planBorder.Background = planBlue;
            //    text = this.Text + "(" + this.Price.ToString() + " บาท)";
            //}
            //else if(this.Type == 2)
            //{
            //    this.planBorder.Background = planPink;
            //    //this.planBorder.BorderThickness = new Thickness(0, 0, 2, 0);
            //    text = "เพิ่ม " + this.Text + "\n(" + this.Price.ToString() + " บาท)";
            //}
            this.planTextBlock.Text = text;
        }

        public void ChangeState()
        {
            if (state)
            {
                this.planBorder.Background = solids[this.Type];
                this.state = false;
            }
            else
            {
                this.planBorder.Background = solidsClick[this.Type];
                this.state = true;

            }
        }
    }
}
