using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;

namespace DAClipper4
{
    public partial class Form1 : Form
    {
        string clip;

        private class ClipResult
        {
            public string status;
            public string message;
            
            public int JLinksCount;
            public string JLinks;
            public string Title;

            public IEnumerable<ParsedLink> ParsedLinks;
            public IEnumerable<string> LinkDupes;
            public IEnumerable<int> SourceDupes;
            public IEnumerable<int> SourceMissing;
        }

        private class ParsedLink {
            public string Url;
            public string InnerText;
            public bool Valid;
            public bool JValid;
            public int Source;

            public ParsedLink(HtmlAgilityPack.HtmlNode node)
            {
                Url = node.GetAttributeValue("href","");
                InnerText = node.InnerText;

                JValid = Url.Contains("deviantart");

                //Valid = !(!JValid && !InnerText.Contains("ource") || node.Descendants("img").Count() > 0);
                Valid = (JValid && InnerText.Contains("ource")) || InnerText.Contains("ource");

                Match match = Regex.Match(InnerText, @"(\d+)");

                if (!int.TryParse(match.Groups[1].Value, out Source))
                {
                    Source = -1;
                }

            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            clip = Clipboard.GetText();
            backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            
            if (!clip.Contains("equestriadaily.com"))
            {
                backgroundWorker1.ReportProgress(100, new ClipResult() { status = "error", message = "No valid Equestria Daily link found on clipboard" });
                return;
            }

            backgroundWorker1.ReportProgress(10);

            HtmlWeb website = new HtmlWeb();

            HtmlAgilityPack.HtmlDocument document = website.Load(clip);

            backgroundWorker1.ReportProgress(25);

            //#(\d+)
            HtmlAgilityPack.HtmlNode titleNode = document.DocumentNode.SelectSingleNode("//*[@id=\"Blog1\"]/div[1]/ul/li/div[2]/div[1]/h3/a");
            
            HtmlAgilityPack.HtmlNode node = document.DocumentNode.SelectSingleNode("//*[@id=\"Blog1\"]/div[1]/ul/li/div[2]/div[2]");
            
            IEnumerable<HtmlNode> links = node.Descendants("a");

            IEnumerable<ParsedLink> parsedLinks = links.Select(l => new ParsedLink(l)).Where(v => v.Valid);

            backgroundWorker1.ReportProgress(50);

            IEnumerable<string> linkDupes = parsedLinks.GroupBy(l => l.Url).Where(g => g.Count() > 1).Select(ig => ig.Select(v => v.InnerText).Aggregate((a,b) => a + " and " + b));

            IEnumerable<int> numeratedSources = parsedLinks.Select(l => l.Source);

            IEnumerable<int> sourceDupes = numeratedSources.GroupBy(l => l).Where(g => g.Count() > 1).Select(d => d.Key).Where(n => n != -1);

            backgroundWorker1.ReportProgress(75);

            int max = numeratedSources.Max();
            IEnumerable<int> sourceMissing = Enumerable.Range(1, max).Except(numeratedSources);


            //string finalLinks = 

            backgroundWorker1.ReportProgress(100, new ClipResult() 
            { 
                status = "success", 
                ParsedLinks = parsedLinks, 
                LinkDupes = linkDupes, 
                SourceDupes = sourceDupes,  
                SourceMissing = sourceMissing,
                JLinksCount = parsedLinks.Where(l => l.JValid).Count(),
                JLinks = parsedLinks.Where(l => l.JValid).Select(j => j.Url).Aggregate((a,b) => string.Format("{0}{1}{2}", a, System.Environment.NewLine, b)),
                Title = titleNode.InnerText
            });



        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //MessageBox.Show((String)e.UserState);

            progressBar1.Value = e.ProgressPercentage;

            if (e.ProgressPercentage != 100) return;

            button1.Enabled = true;
            txtConsole.Text = "";

            ClipResult result = (ClipResult)e.UserState;

            if (result.status != "success")
            {
                
                txtConsole.AppendText(result.status + System.Environment.NewLine);
                txtConsole.AppendText(result.message);
                return;
            }

            foreach (ParsedLink link in result.ParsedLinks)
            {
                richTextBox1.SelectionColor = link.JValid ? Color.Green : Color.Red;
                richTextBox1.AppendText(link.InnerText + " " + link.Url + System.Environment.NewLine);
            }

            txtConsole.AppendText("Results for " + result.Title + System.Environment.NewLine + System.Environment.NewLine);
            
            txtConsole.AppendText("Links Analyzed: " + result.ParsedLinks.Count() + System.Environment.NewLine);
            txtConsole.AppendText("Valid JDownload Links found: " + result.JLinksCount + System.Environment.NewLine + System.Environment.NewLine);

            if(result.LinkDupes.Count() > 0){
                txtConsole.AppendText(string.Format("Duplicate links ({0}) found at:{1}", result.LinkDupes.Count(), System.Environment.NewLine));
                foreach (string dupe in result.LinkDupes)
                {
                    txtConsole.AppendText("    " + dupe + System.Environment.NewLine);
                }
            }

            if (result.SourceDupes.Count() > 0)
            {
                txtConsole.AppendText(string.Format("Source numbers duplicated ({0}):{1}", result.SourceDupes.Count(), System.Environment.NewLine));
                foreach (int dupe in result.SourceDupes)
                {
                    txtConsole.AppendText(string.Format("    Source[{0}]{1}", dupe, System.Environment.NewLine));
                }
            }

            if (result.SourceMissing.Count() > 0)
            {
                txtConsole.AppendText(string.Format("The following source numbers ({0}) could not be found:{1}", result.SourceMissing.Count(), System.Environment.NewLine));
                foreach (int missing in result.SourceMissing)
                {
                    txtConsole.AppendText(string.Format("    Source[{0}]{1}", missing, System.Environment.NewLine));
                }
            }

            textBox2.Text = result.JLinks;
            
            txtConsole.AppendText(System.Environment.NewLine + "Thank you for using EQD DA Clipper!");

            //IEnumerable<ParsedLink> ParsedLinks = (IEnumerable<ParsedLink>)e.UserState;

           // string outText = string.Join(System.Environment.NewLine, ParsedLinks.Select(l => l.InnerText + " " + l.Source + " " + l.Url));

            //txtConsole.Text = outText;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBox2.Text);
        }
    }
}
