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
    public partial class Form4 : Form
    {
        public Form4()
        {
            
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1 main = this.Owner as Form1;
            if (main != null)
            {
                int numberOfTrackLocal = 0;
                string id = "";
                string descr = "";

                XmlDocument myXmlDocument = new XmlDocument();
                myXmlDocument.Load(main.DataXMLPath);

                foreach (XmlNode xv in myXmlDocument.GetElementsByTagName("Items"))
                {
                    foreach (XmlNode xn in xv)
                    {
                        if (xn.Name == "Track")
                        {
                            foreach (XmlAttribute attr in xn.Attributes)
                            {
                                if (attr.Name == "id") { id = attr.Value; }
                                if ((attr.Name == "Descr") & (attr.Value == main.postage) & (id == main.s)) 
                                {
                                    attr.Value = textBox1.Text;
                                }
                            }
                        }
                    }
                }
                myXmlDocument.Save(main.DataXMLPath);
                main.listBox1.Items.Clear();
                int i = 0; 
                main.numberOftrack = 0;
               

                foreach (XmlNode xn in myXmlDocument.GetElementsByTagName("Track"))
                {

                    foreach (XmlAttribute attr in xn.Attributes)
                    {
                        if (attr.Name == "id") 
                        {
                            id = attr.Value;
                        }
                        if (attr.Name == "Descr")
                        {
                            descr = attr.Value;
                        }
                        if (attr.Name == "Arhive")
                        {
                            if (attr.Value == "0")
                            {
                                main.listBox1.Items.Add(descr);
                                main.Tracks[i] = id + "/" + descr;
                                i++;
                                main.numberOftrack++;
                                numberOfTrackLocal = main.numberOftrack;
                                main.StatusLabel5.Text = numberOfTrackLocal.ToString();
                            }
                            if ((attr.Value == "1") && (main.ViewArchiveItems == true))
                            {
                                main.listBox1.Items.Add(descr);
                                main.Tracks[i] = id + "/" + descr;
                                i++;
                                main.numberOftrack++;
                                numberOfTrackLocal = main.numberOftrack;
                                main.StatusLabel5.Text = numberOfTrackLocal.ToString();
                            }
                        }
                    }
                }
            }
            this.Close();
        }

        private void Form4_Shown(object sender, EventArgs e)
        {
            Form1 main = this.Owner as Form1;
            if (main != null)
            {
                textBox1.Text = main.postage;
                textBox2.Text = main.s;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form4_Load(object sender, EventArgs e)
        {

        }
    }
}
