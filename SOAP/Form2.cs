using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Postage
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string str="";
            int numberOfTrackLocal = 0;
            Form1 main = this.Owner as Form1;
            if (main.numberOftrack >= 50) { this.Close(); }
            if(main != null)
        {
            XmlDocument my = new XmlDocument();
            my.Load(main.DataXMLPath);

            int status = 0;
            str = textBox1.Text;
            if (str == "") { MessageBox.Show("Пустой запрос!"); status = 1; return; }

            foreach (XmlNode xn in my.GetElementsByTagName("Track"))
            {

                foreach (XmlAttribute attr in xn.Attributes)
                {
                    if (attr.Value == str) { MessageBox.Show("Такой элемент уже есть в базе!"); status = 1; }
                }
            }

            if (status == 0)
            {
                    XmlNode xn = my.GetElementsByTagName("Items").Item(0);
                    XmlElement newitem = my.CreateElement("Track");
                    XmlAttribute idattr;

                    idattr = my.CreateAttribute("id");
                    idattr.Value = str;
                    newitem.SetAttributeNode(idattr);

                    idattr = my.CreateAttribute("Descr");
                    idattr.Value = textBox2.Text;
                    newitem.SetAttributeNode(idattr);
                    
                    idattr = my.CreateAttribute("State");
                    idattr.Value = "0";
                    newitem.SetAttributeNode(idattr);
                    
                    idattr = my.CreateAttribute("Arhive");
                    idattr.Value = "0";
                    newitem.SetAttributeNode(idattr);
                    
                    xn.InsertAfter(newitem, xn.LastChild);

                    main.listBox1.Items.Add(textBox2.Text);
                    main.Tracks[main.numberOftrack] = str + "/" + textBox2.Text;
                    main.numberOftrack++;
                    numberOfTrackLocal = main.numberOftrack;
                    main.StatusLabel5.Text = numberOfTrackLocal.ToString();

                    my.Save(main.DataXMLPath);
            }
         }
            XmlDocument myX = new XmlDocument();
            myX.Load(main.DataXMLPath);
            long is_time = 0;
            string descr = "";
            foreach (XmlNode xn in myX.GetElementsByTagName("Track"))
            {

                foreach (XmlAttribute attr in xn.Attributes)
                {
                    if (attr.Name == "Descr") { descr = attr.Value; }
                    if (attr.Name == "State")
                    {

                        is_time = DateTime.Now.Ticks / 10000000 / 60 / 60 / 24 - Convert.ToInt32(attr.Value);
                        if (attr.Value != "0")
                        {
                            main.listBox1.daysListItem[main.listBox1.Items.IndexOf(descr) + 1] = is_time.ToString();
                        }
                        else
                        {
                            main.listBox1.daysListItem[main.listBox1.Items.IndexOf(descr) + 1] = " ";
                        }

                    }
                    if (attr.Name == "Arhive")
                    {
                        if (attr.Value == "1")
                        {

                            main.listBox1.archiveListItem[main.listBox1.Items.IndexOf(descr) + 1] = true;
                        }
                        else
                        {
                            main.listBox1.archiveListItem[main.listBox1.Items.IndexOf(descr) + 1] = false;
                        }
                    }
                }
            }
            main.listBox1.Refresh();
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
