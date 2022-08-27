using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace VttConvertSrt
{
    public partial class Form1 : Form
    {
        private string[] fileNames;//所有选定文件文件名(包含文件路径)
        private string vttPrefix = "WEBVTT" + System.Environment.NewLine;// vtt前缀有多种格式，youtube需要自行修改

        public Form1()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void btn_Choose_VTT(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "请选择.vtt文件";
            // 能够多选
            openFileDialog.Multiselect = true;
            // 格式过滤器
            openFileDialog.Filter = "vtt字幕文件(*.vtt)|*.vtt";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                fileNames = openFileDialog.FileNames;// 得到所有选定文件文件名(包含文件路径)
            }
        }


        private void btn_Transfer_VTT(object sender, EventArgs e)
        {
            foreach (string fileName in fileNames)
            {
                vtt2srt(fileName);
            }
            MessageBox.Show("Convert Success!");
        }


        private void btn_Choose_SRT(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "请选择.srt文件";
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "srt字幕文件(*.srt)|*.srt";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                fileNames = openFileDialog.FileNames;
            }
        }

        private void btn_Transfer_SRT(object sender, EventArgs e)
        {
            foreach (string fileName in fileNames)
            {
                srt2vtt(fileName);
            }
            MessageBox.Show("Convert Success!");
        }


        private void vtt2srt(string name)
        {
            string[] oldTexts = File.ReadAllLines(name, Encoding.UTF8);
            List<string> resultTextsList = new List<string>();

            // 匹配两种时间格式          vtt                                  srt
            // 分:秒.毫秒      00:03.490 --> 00:03.950           00:00:03.490 --> 00:00:03.950
            // 时:分:秒.毫秒   01:00:03.490 --> 01:00:03.950     01:00:03.490 --> 01:00:03.950
            //string timePattern = @"(\d{2}:)?(\d{2}:)(\d{2}).(\d{3})\s-->\s(\d{2}:)?(\d{2}:)(\d{2}).(\d{3})";
            string timePattern1 = @"(\d{2}:)(\d{2}).(\d{3})\s-->\s(\d{2}:)(\d{2}).(\d{3})";
            string timePattern2 = @"(\d{2}:)(\d{2}:)(\d{2}).(\d{3})\s-->\s(\d{2}:)(\d{2}:)(\d{2}).(\d{3})";
            int lineNumber = 1;
            bool isFirstMatchTimeLine = false;// 是否第一次匹配到时间轴

            foreach (string oldLine in oldTexts)
            {
                if (Regex.IsMatch(oldLine, timePattern1) || Regex.IsMatch(oldLine, timePattern2))
                {
                    // vtt2srt 加行号
                    string lineNumberText = lineNumber.ToString();
                    resultTextsList.Add(lineNumberText);
                    lineNumber++;
                    // vtt2srt '.'改为','
                    if (Regex.IsMatch(oldLine, timePattern1))
                    {
                        resultTextsList.Add(Regex.Replace(oldLine, timePattern1, "00:$1$2,$3 --> 00:$4$5,$6"));
                    }
                    else if (Regex.IsMatch(oldLine, timePattern2))
                    {
                        resultTextsList.Add(Regex.Replace(oldLine, timePattern2, "$1$2$3,$4 --> $5$6$7,$8"));
                    }
                    isFirstMatchTimeLine = true;
                }
                else
                {
                    // 忽略第一个时间轴之前所有的内容
                    // 未匹配到第一个行号，isFirstMatchTimeLine始终为false，不添加到List
                    if (isFirstMatchTimeLine == true)
                    {
                        resultTextsList.Add(oldLine);
                    }
                }
            }

            string newName = name.Replace(".vtt", ".srt");
            FileInfo fileInfo = new FileInfo(newName);
            if (fileInfo.Exists)
            {
                newName = newName.Replace(".srt", " - 副本.srt");
            }
            File.WriteAllLines(newName, resultTextsList, Encoding.UTF8);
        }


        private void srt2vtt(string name)
        {
            string[] oldTexts = File.ReadAllLines(name, Encoding.UTF8);
            List<string> resultTextsList = new List<string>();
            string timePattern = @"(\d{2}:)(\d{2}:)(\d{2}),(\d{3})\s-->\s(\d{2}:)(\d{2}:)(\d{2}),(\d{3})";
            string lineNumberPattern = @"^\d+$";// 匹配行号(一到多位纯数字)
            bool isFirstMatchTimeLine = false;// 是否第一次匹配到时间轴

            foreach (string oldLine in oldTexts)
            {
                if (Regex.IsMatch(oldLine, timePattern))
                {
                    if (isFirstMatchTimeLine == false)
                    {
                        resultTextsList.Add(vttPrefix);// 在第一个时间轴前面添加"WEBVTT\n"前缀
                    }
                    resultTextsList.Add(Regex.Replace(oldLine, timePattern, "$1$2$3.$4 --> $5$6$7.$8"));
                    isFirstMatchTimeLine = true;
                }
                else if (!Regex.IsMatch(oldLine, lineNumberPattern))// 忽略行号和时间轴，剩下字幕文本
                {
                    resultTextsList.Add(oldLine);
                }
            }

            string newName = name.Replace(".srt", ".vtt");
            FileInfo fileInfo = new FileInfo(newName);
            if (fileInfo.Exists)
            {
                newName = newName.Replace(".vtt", " - 副本.vtt");// 更改文件名字符串
            }
            File.WriteAllLines(newName, resultTextsList, Encoding.UTF8);
        }
    }
}
