using HtmlAgilityPack;
using Knyaz.Optimus;
using Knyaz.Optimus.TestingTools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Console = System.Console;

namespace DAClipper4
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void btn_go_click(object sender, EventArgs e)
        {
            Console.WriteLine("starting engine");
            /*var engine = new Engine();
            engine.OpenUrl("https://www.deviantart.com/kriss-studios/art/COMM-robert-800288859");
            engine.WaitSelector(".dev-page-download");

            Console.WriteLine("Found Button" + engine.Document.QuerySelectorAll(".dev-page-download").First().GetAttribute("href"));*/

            HtmlWeb website = new HtmlWeb();

            HtmlAgilityPack.HtmlDocument document = website.Load("http://www.deviantart.com/kriss-studios/art/COMM-robert-800288859");

            var download = document.DocumentNode.SelectSingleNode("//*[@id=\"root\"]/div[2]/div[1]/div[3]/div/div[3]/div[1]/div[4]/span/span/a");

            Console.WriteLine("Found button" + download.GetAttributeValue("href", "nothing!"));
        }
    }
}
