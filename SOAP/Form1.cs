using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Diagnostics;



namespace Postage
{

    public partial class Form1 : Form
    {

        #region Varables
        public static string my_version = "1.2.6.1";
        public bool ViewArchiveItems = true;
        public string DataXMLPath = "";
        public string s = "", postage = "";
        int status_post = 0;
        public long timerinterval = DateTime.Now.Ticks;
        public int timereload = 60;
        public string ProxyName = "", ProxyPassword = "", ProxyAdress = "", ProxyPort = "";
        public bool UseProxy = false;
        public string[] Tracks = new string[100];
        public byte numberOftrack = 0;
        public long timerinit = DateTime.Now.Ticks / 10000000 / 60 / 60 / 24;
        int printed = 0;
        string startdate = "";
        DateTime dtStart;
        public long timerinitend = 0;
        int OperDateCount;
        string soapRequestResult = "NONE";
        public bool refresh_icon = true;
        #endregion Varables

        public Form1()
        {
            InitializeComponent();

        }

        public void SoapRequest()
        {
            string
       soapEnv = @"<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:SOAP-ENC=""http://schemas.xmlsoap.org/soap/encoding/"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:m0=""http://schemas.xmlsoap.org/soap/envelope/"">" + "\n" +
                           "<SOAP-ENV:Body>" + "\n" +
                           "<m:OperationHistoryRequest xmlns:m=\"http://russianpost.org/operationhistory/data\">" + "\n" +
                           "<m:Barcode>" + s + "</m:Barcode>" + "\n" +
                           "<m:MessageType>0</m:MessageType>" + "\n" +
                           "</m:OperationHistoryRequest>" + "\n" +
                           "</SOAP-ENV:Body>" + "\n" +
                            "</SOAP-ENV:Envelope>";


            try
            {
                WebRequest request = HttpWebRequest.Create("http://voh.russianpost.ru:8080/niips-operationhistory-web/OperationHistory");
                if (UseProxy)
                {
                    WebProxy myProxy = new WebProxy();
                    myProxy.Address = new Uri("http://" + ProxyAdress + ":" + ProxyPort, UriKind.Absolute);
                    myProxy.Credentials = new NetworkCredential(ProxyName, ProxyPassword);
                    request.Proxy = myProxy;
                    request.PreAuthenticate = true;
                }
                request.Timeout = 30000;
                request.Credentials = CredentialCache.DefaultCredentials;
                request.Method = "POST";
                request.ContentType = "text/xml; charset=utf-8";
                request.ContentLength = soapEnv.Length;
                request.Headers.Add("SOAPAction", "http://voh.russianpost.ru:8080/niips-operationhistory-web/OperationHistory");

                StreamWriter streamWriter = new StreamWriter(request.GetRequestStream());
                streamWriter.Write(soapEnv);
                streamWriter.Close();

                WebResponse response = request.GetResponse();
                StreamReader streamReader = new StreamReader(response.GetResponseStream());

                soapRequestResult = streamReader.ReadToEnd();
                streamReader.Close();
                response.Close();

            }

            catch
            {
                soapRequestResult = "NONE";
            }
        }

        void GetUpdateProgram()
        {
            try
            {
                WebClient client = new WebClient();
                if (UseProxy)
                {
                    WebProxy myProxy = new WebProxy();
                    myProxy.Address = new Uri("http://" + ProxyAdress + ":" + ProxyPort, UriKind.Absolute);
                    myProxy.Credentials = new NetworkCredential(ProxyName, ProxyPassword);
                    client.Proxy = myProxy;
                    client.UseDefaultCredentials = true;
                }
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                string downloadVersion = client.DownloadString(new Uri("http://oddy.gixx.ru/update/version.txt")).Trim();
                if (downloadVersion != my_version)
                {
                    DialogResult dialog = MessageBox.Show("Доступна новая версия \nпрограммы, обновить?", "Автоматическое обновление", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    if (dialog == DialogResult.Yes)
                    {
                        string md = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                        md += "\\Postage\\setup.exe";
                        client.DownloadFileAsync(new Uri("http://oddy.gixx.ru/update/setup.exe"), md);


                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            string md = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            md += "\\Postage\\setup.exe";
            System.Diagnostics.Process.Start(md);
            Environment.Exit(0);
        }


        void Get()
        {

            toolStripButton2.Enabled = false;
            toolStripButton1.Enabled = false;

            Thread myThread = new Thread(SoapRequest);
            myThread.Name = "soapRequestThread";
            myThread.Priority = ThreadPriority.Lowest;
            myThread.IsBackground = true;
            myThread.Start();
            while (myThread.IsAlive) { Application.DoEvents(); }
            string result = soapRequestResult;

            if (result[0] == '<')
            {


                var pattern = @"<ns\d+\:?";
                var replacement = "<";
                Regex rgx = new Regex(pattern);
                result = rgx.Replace(result, replacement);

                pattern = @"</ns\d+\:?";
                replacement = "</";
                rgx = new Regex(pattern);
                result = rgx.Replace(result, replacement);



                System.Xml.XmlDocument doc = new XmlDocument();
                doc.LoadXml(result);
                doc.PreserveWhitespace = true;


                StringWriter stringwriter = new StringWriter();
                XmlTextWriter xmlwriter = new XmlTextWriter(stringwriter);
                xmlwriter.Formatting = Formatting.Indented;
                doc.WriteTo(xmlwriter);

                XmlNodeList hystoryRecords = doc.GetElementsByTagName("historyRecord");
                string stroka = "";
                string Format = "";
                string[] formatPost = new string[10];
                string numberofattr = "";
                string state = "";
                int amount = 0; int count = 0;

                XmlDocument myXmlDocument = new XmlDocument();
                myXmlDocument.Load(DataXMLPath);

                buttonListBox1.Items.Clear();


                foreach (XmlNode track in myXmlDocument.GetElementsByTagName("Track"))
                {

                    foreach (XmlAttribute attr in track.Attributes)
                    {
                        if (attr.Value == s)
                        {
                            foreach (XmlNode x in track)
                            {
                                if (x.Name == "Item")
                                {
                                    foreach (XmlAttribute attribute in x.Attributes)
                                    {
                                        if (attribute.Name == "id")
                                        {
                                            numberofattr = attribute.Value;
                                            count = Convert.ToInt32(numberofattr);
                                        }
                                    }

                                    foreach (XmlNode xc in x.ChildNodes) state = xc.InnerText;
                                    buttonListBox1.Items.Add(state);
                                }
                            }
                        }

                    }
                }

                for (int i = 1; i < 9; i++) { formatPost[i] = ""; }
                Format = "";
                OperDateCount = 0;


                foreach (XmlNode a in hystoryRecords)
                {
                    foreach (XmlNode b in a)
                    {
                        foreach (XmlNode c in b)
                        {
                            if (c.Name == "OperDate")
                            {
                                stroka = c.InnerXml;
                                if (OperDateCount == 0)
                                {
                                    startdate = stroka;
                                }
                                OperDateCount++;
                                formatPost[4] = DateTime.Parse(stroka).ToString();
                            }
                            if (b.Name == "UserParameters")
                            {
                                if (c.Name == "Rcpn") { formatPost[7] = c.InnerXml; }
                            }
                            foreach (XmlNode d in c)
                            {
                                if (d.Name == "Description") { formatPost[3] = d.InnerXml; }
                                if (d.Name == "NameRU") { formatPost[8] = d.InnerXml; }
                                if (d.Name == "Index") { formatPost[2] = d.InnerXml; }
                                if (b.Name == "OperationParameters")
                                {
                                    if (d.Name == "Name")
                                    {
                                        formatPost[5] = d.InnerXml;
                                        Format = Format + "  " + d.InnerXml + ", ";
                                        formatPost[1] = Format;
                                    }
                                }
                            }
                        }
                    }


                    Format = "";
                    Format = formatPost[1] + "  " + formatPost[2] + ",  " + formatPost[3] + ",  " + formatPost[8] + ",  " + formatPost[7];
                   
                    amount++;


                    if ((amount > count) )
                    {
                        status_post = 1;
                        XmlElement newitem = myXmlDocument.CreateElement("Item");
                        XmlAttribute idattr;

                        idattr = myXmlDocument.CreateAttribute("id");
                        idattr.Value = amount.ToString();
                        newitem.SetAttributeNode(idattr);

                        idattr = myXmlDocument.CreateAttribute("time");
                        idattr.Value = formatPost[4];
                        newitem.SetAttributeNode(idattr);


                        XmlElement nameel = myXmlDocument.CreateElement("Name");
                        nameel.InnerText = Format;

                        newitem.AppendChild(nameel);



                        foreach (XmlNode xn in myXmlDocument.GetElementsByTagName("Track"))
                        {

                            foreach (XmlAttribute attr in xn.Attributes)
                            {
                                if (attr.Value == s)
                                {
                                    xn.InsertAfter(newitem, xn.LastChild);
                                }

                            }
                            bool initstate = false;
                            string descr = "", id = "";

                            foreach (XmlNode xv in myXmlDocument.GetElementsByTagName("Items"))
                            {
                                foreach (XmlNode xnv in xv)
                                {
                                    if (xnv.Name == "Track")
                                    {
                                        foreach (XmlAttribute attr in xn.Attributes)
                                        {
                                            if ((attr.Name == "id") & (attr.Value == s)) { id = attr.Value; initstate = true; }
                                            if (attr.Name == "Descr") { descr = attr.Value; }
                                            if ((attr.Name == "State") & (initstate == true))
                                            {

                                                dtStart = DateTime.Parse(startdate);
                                                timerinitend = dtStart.Ticks / 10000000 / 60 / 60 / 24;
                                                attr.Value = timerinitend.ToString();
                                                initstate = false;
                                            }
                                            if ((attr.Name == "Arhive") & (id == s))
                                            {
                                                if (formatPost[5] == "Вручение адресату")
                                                {
                                                    attr.Value = "1";
                                                    listBox1.archiveListItem[listBox1.Items.IndexOf(descr) + 1] = true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        buttonListBox1.Items.Add(Format);
                    }

                    Format = "";
                }
                myXmlDocument.Save(DataXMLPath);
            }
            toolStripButton2.Enabled = true;
            toolStripButton1.Enabled = true;

        }





        void Notify()
        {
            notifyIcon1.Visible = true;
            notifyIcon1.ShowBalloonTip(3600, "Почта", "Обновлен статус посылки " + postage, ToolTipIcon.Info);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

           

            for (var ii = 0; ii < 50; ii++)
            {
                listBox1.archiveListItem[ii] = false;
            }

            this.Width = Properties.Settings.Default.myX;
            this.Height = Properties.Settings.Default.myY;
            this.splitContainer1.SplitterDistance = Properties.Settings.Default.splitter;
            this.ViewArchiveItems = Properties.Settings.Default.Archive;

            if (ViewArchiveItems == true)
            {
                this.toolStripButton7.Text = "Скрыть архивные";
            }
            else
            {
                this.toolStripButton7.Text = "Показать архивные";
            }


            string md = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            DataXMLPath = md + "\\Postage";
            if (Directory.Exists(DataXMLPath) == false)
            {
                Directory.CreateDirectory(DataXMLPath);
            }
            DataXMLPath += "\\" + "data.xml";
            FileInfo file = new FileInfo(DataXMLPath);
            if (file.Exists == false)
            {
                StreamWriter write_text;
                write_text = file.AppendText();
                write_text.WriteLine(@"<?xml version=""1.0"" encoding=""utf-8""?>" + "\n" + "<Items>" + "\n" + "</Items>");
                write_text.Close();
            }



            listBox1.Sorted = false;
            buttonListBox1.Items.Clear();
            string id = "";
            string descr = "";
            string key_reg;
            int i = 0;
            long is_time = 0;


            try
            {
                Microsoft.Win32.RegistryKey key;
                key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("ProxySettings");
                key_reg = (string)key.GetValue("UseProxy");
                if (key_reg == "true") { UseProxy = true; } else { UseProxy = false; };
                ProxyName = (string)key.GetValue("ProxyName");
                ProxyPassword = (string)key.GetValue("ProxyPassword");
                ProxyAdress = (string)key.GetValue("ProxyAdress");
                ProxyPort = (string)key.GetValue("ProxyPort");
                timereload = (int)key.GetValue("TimeInterval");
                if ((int)key.GetValue("listBoxSorted") == 0) { listBox1.Sorted = false; this.сортировкаToolStripMenuItem.Text = "Сортировка по имени"; }
                else { listBox1.Sorted = true; this.сортировкаToolStripMenuItem.Text = "Сортировка по добавлению"; };
                key.Close();
            }
            catch
            {
                Microsoft.Win32.RegistryKey key;
                key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("ProxySettings");
                key.SetValue("UseProxy", "false");
                key.SetValue("ProxyName", "oddy");
                key.SetValue("ProxyPassword", "giperfish");
                key.SetValue("ProxyAdress", "10.10.0.1");
                key.SetValue("ProxyPort", "3128");
                key.SetValue("TimeInterval", 60);
                key.SetValue("listBoxSorted", 0);
                UseProxy = false;
                key.Close();
            };


            GetUpdateProgram();

            try
            {
                XmlDocument my = new XmlDocument();
                my.Load(DataXMLPath);


                foreach (XmlNode xn in my.GetElementsByTagName("Track"))
                {

                    foreach (XmlAttribute attr in xn.Attributes)
                    {
                        if (attr.Name == "id") { id = attr.Value; }
                        if (attr.Name == "Descr")
                        {
                            descr = attr.Value;
                        }
                        if (attr.Name == "Arhive")
                        {
                            if (attr.Value == "0")
                            {
                                listBox1.Items.Add(descr);
                                Tracks[i] = id + "/" + descr;
                                i++;
                                numberOftrack++;
                                StatusLabel5.Text = numberOftrack.ToString();
                            }
                            if ((attr.Value == "1") && (ViewArchiveItems == true))
                            {
                                listBox1.Items.Add(descr);
                                Tracks[i] = id + "/" + descr;
                                i++;
                                numberOftrack++;
                                StatusLabel5.Text = numberOftrack.ToString();
                            }
                        }
                    }
                }


                foreach (XmlNode xn in my.GetElementsByTagName("Track"))
                {

                    foreach (XmlAttribute attr in xn.Attributes)
                    {
                        if (attr.Name == "Descr") { descr = attr.Value; }
                        if ((attr.Name == "State"))
                        {

                            is_time = DateTime.Now.Ticks / 10000000 / 60 / 60 / 24 - Convert.ToInt32(attr.Value);
                            if ((attr.Value != "0") && (Convert.ToInt32(is_time) < 100))
                            {
                                listBox1.daysListItem[listBox1.Items.IndexOf(descr) + 1] = is_time.ToString();
                            }
                            else
                            {
                                listBox1.daysListItem[listBox1.Items.IndexOf(descr) + 1] = " ";
                            }
                        }
                        if (attr.Name == "Arhive")
                        {
                            if (attr.Value == "1")
                            {

                                listBox1.archiveListItem[listBox1.Items.IndexOf(descr) + 1] = true;
                            }
                            else
                            {
                                listBox1.archiveListItem[listBox1.Items.IndexOf(descr) + 1] = false;
                            }
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("Error : " + ex.Message);
            }

            timer1.Interval = timereload * 60000;
            timerinterval = DateTime.Now.Ticks;
            toolStripStatusLabel6.Text = (timerinterval / 10000000 / 60 + timereload - DateTime.Now.Ticks / 10000000 / 60).ToString();

        }



        private void timer1_Tick(object sender, EventArgs e)
        {
            Refresh_post();
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.notifyIcon1.Visible = false;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                this.notifyIcon1.Visible = true;
                this.notifyIcon1.ShowBalloonTip(3600, "Почта", "Проверка почтового отправления", ToolTipIcon.Info);
            }
        }


        void SelectTrack()
        {
            try
            {
                string numberofattr = "";
                string attrvalue = "";
                int count = 0;
                string str, str1;
                buttonListBox1.Items.Clear();
                if (listBox1.SelectedItems.Count != 0)
                {
                    StatusLabel3.Text = (listBox1.SelectedIndex + 1).ToString();
                    listBox1.numListItem[listBox1.Items.IndexOf(listBox1.SelectedItem.ToString()) + 1] = 0;
                    StatusLabel3.Text = (listBox1.SelectedIndex + 1).ToString();
                    str1 = listBox1.SelectedItem.ToString();
                    for (var i = 0; i < numberOftrack; i++)
                    {
                        str = Tracks[i];
                        string[] words = str.Split('/');
                        if (words[1] == str1)
                        {
                            s = words[0];
                            postage = words[1];
                        }
                    }


                    XmlDocument myXmlDocument = new XmlDocument();
                    myXmlDocument.Load(DataXMLPath);

                    foreach (XmlNode track in myXmlDocument.GetElementsByTagName("Track"))
                    {

                        foreach (XmlAttribute attr in track.Attributes)
                        {
                            if (attr.Value == s)
                            {
                                foreach (XmlNode x in track)
                                {
                                    if (x.Name == "Item")
                                    {
                                        foreach (XmlAttribute attribute in x.Attributes)
                                        {
                                            if (attribute.Name == "id")
                                            {
                                                numberofattr = attribute.Value;
                                                count = Convert.ToInt32(numberofattr);
                                            }
                                            if (attribute.Name == "time")
                                            {
                                                numberofattr = attribute.Value;
                                                buttonListBox1.timesListItem[count] = numberofattr;
                                            }
                                            else
                                            {
                                                buttonListBox1.timesListItem[count] = "";
                                            }
                                        }

                                        foreach (XmlNode xc in x.ChildNodes) attrvalue = xc.InnerText;
                                        buttonListBox1.Items.Add(attrvalue);
                                    }
                                }
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }


        private void listBox1_Click(object sender, EventArgs e)
        {
            SelectTrack();
        }



        void Refresh_post()
        {
            XmlDocument myXml = new XmlDocument();
            myXml.Load(DataXMLPath);
            string temp_string = "", descr = " ";
            postage = "";
            status_post = 0;
            int count_of_postage = 0;
            int count_of_postage_recived = 0;
            progressBar1.Maximum = numberOftrack;
            progressBar1.Value = 0;
            progressBar1.Step = 1;
            timerinterval = DateTime.Now.Ticks;
            long is_time;
            listBox1.Enabled = false;
            var res = Properties.Resources.ResourceManager.GetObject("refresh_90");
            notifyIcon1.Icon = (System.Drawing.Icon)res;
            timer3.Enabled = true;


            foreach (XmlNode track in myXml.GetElementsByTagName("Track"))
            {

                foreach (XmlAttribute attrib in track.Attributes)
                {
                    if (attrib.Name == "id")
                    {
                        s = attrib.Value;
                    }
                    if (attrib.Name == "Descr")
                    {
                        temp_string = attrib.Value;
                    }
                    if (attrib.Name == "Arhive")
                    {
                        if (attrib.Value == "0")
                        {
                            count_of_postage_recived = listBox1.Items.IndexOf(temp_string);
                            Get();
                            buttonListBox1.Items.Clear();
                            count_of_postage++;
                            StatusLabel1.Text = count_of_postage.ToString();
                            progressBar1.PerformStep();
                            progressBar1.Refresh();
                        }
                        else
                            if (ViewArchiveItems == true)
                            {
                                progressBar1.Maximum -= 1;
                            }
                    }

                    if (status_post == 1)
                    {
                        listBox1.numListItem[count_of_postage_recived + 1] = 1;
                        postage = postage + ",  " + temp_string;
                        status_post = 0;
                    }

                }

            }

            if (postage != "") { Notify(); };
            postage = "";

            XmlDocument my = new XmlDocument();
            my.Load(DataXMLPath);
            foreach (XmlNode xn in my.GetElementsByTagName("Track"))
            {

                foreach (XmlAttribute attr in xn.Attributes)
                {
                    if (attr.Name == "Descr") { descr = attr.Value; }
                    if (attr.Name == "State")
                    {

                        is_time = DateTime.Now.Ticks / 10000000 / 60 / 60 / 24 - Convert.ToInt32(attr.Value);
                        if ((attr.Value != "0") & (Convert.ToInt32(is_time) < 100))
                        {
                            listBox1.daysListItem[listBox1.Items.IndexOf(descr) + 1] = is_time.ToString();
                        }
                        else
                        {
                            listBox1.daysListItem[listBox1.Items.IndexOf(descr) + 1] = " ";
                        }

                    }
                }
            }
            listBox1.Refresh();
            progressBar1.Maximum = numberOftrack;
            progressBar1.Value = numberOftrack;
            timer3.Enabled = false;
            res = Properties.Resources.ResourceManager.GetObject("email");
            notifyIcon1.Icon = (System.Drawing.Icon)res;
            listBox1.Enabled = true;


        }

        void DeleteTrack()
        {
            XmlDocument myXmlDocument = new XmlDocument();
            myXmlDocument.Load(DataXMLPath);
            string id = "";
            string descr = "";


            if (listBox1.SelectedItems.Count != 0)
            {
                XmlElement root = myXmlDocument.DocumentElement;

                foreach (XmlNode xv in myXmlDocument.GetElementsByTagName("Items"))
                {
                    foreach (XmlNode xn in xv)
                    {
                        if (xn.Name == "Track")
                        {
                            foreach (XmlAttribute attr in xn.Attributes)
                            {
                                if (attr.Value == s)
                                {
                                    root.RemoveChild(xn);
                                    myXmlDocument.Save(DataXMLPath);

                                }
                            }
                        }
                    }
                }
                listBox1.Items.Clear(); int i = 0; numberOftrack = 0;

                foreach (XmlNode xn in myXmlDocument.GetElementsByTagName("Track"))
                {

                    foreach (XmlAttribute attr in xn.Attributes)
                    {
                        if (attr.Name == "id") { id = attr.Value; }
                        if (attr.Name == "Descr")
                        {
                            descr = attr.Value;
                        }
                        if (attr.Name == "Arhive")
                        {
                            if (attr.Value == "0")
                            {
                                listBox1.Items.Add(descr);
                                Tracks[i] = id + "/" + descr;
                                i++;
                                numberOftrack++;
                                StatusLabel5.Text = numberOftrack.ToString();
                            }
                            if ((attr.Value == "1") && (ViewArchiveItems == true))
                            {
                                listBox1.Items.Add(descr);
                                Tracks[i] = id + "/" + descr;
                                i++;
                                numberOftrack++;
                                StatusLabel5.Text = numberOftrack.ToString();
                            }
                        }
                    }
                }

                XmlDocument myX = new XmlDocument();
                myX.Load(DataXMLPath);
                long is_time = 0;
                foreach (XmlNode xn in myX.GetElementsByTagName("Track"))
                {

                    foreach (XmlAttribute attr in xn.Attributes)
                    {
                        if (attr.Name == "Descr") { descr = attr.Value; }
                        if (attr.Name == "State")
                        {

                            is_time = DateTime.Now.Ticks / 10000000 / 60 / 60 / 24 - Convert.ToInt32(attr.Value);
                            if ((attr.Value != "0") & (Convert.ToInt32(is_time) < 100))
                            {
                                listBox1.daysListItem[listBox1.Items.IndexOf(descr) + 1] = is_time.ToString();
                            }
                            else
                            {
                                listBox1.daysListItem[listBox1.Items.IndexOf(descr) + 1] = " ";
                            }

                        }
                        if (attr.Name == "Arhive")
                        {
                            if (attr.Value == "1")
                            {

                                listBox1.archiveListItem[listBox1.Items.IndexOf(descr) + 1] = true;
                            }
                            else
                            {
                                listBox1.archiveListItem[listBox1.Items.IndexOf(descr) + 1] = false;
                            }
                        }
                    }
                }
                listBox1.Refresh();
            }
        }


        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Form2 secondForm = new Form2();
            secondForm.Owner = this;
            secondForm.ShowDialog();
        }

        private void удалитьТрекToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteTrack();
        }



        private void измениитьТрекToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form4 secondForm = new Form4();
            secondForm.Owner = this;
            secondForm.ShowDialog();
        }


        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Get();
            status_post = 0;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            Refresh_post();

        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            Form3 secondForm = new Form3();
            secondForm.Owner = this;
            secondForm.ShowDialog();
        }


        private void listBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Menu1.Show(MousePosition, ToolStripDropDownDirection.Right);
            }
        }


        private void timer2_Tick(object sender, EventArgs e)
        {
            toolStripStatusLabel6.Text = (timerinterval / 10000000 / 60 + timereload - DateTime.Now.Ticks / 10000000 / 60).ToString();
        }

        private void toolStripButton4_Click_1(object sender, EventArgs e)
        {
            AboutBox1 About = new AboutBox1();
            About.Owner = this;
            About.ShowDialog();
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            Form5 secondForm = new Form5();
            secondForm.Owner = this;
            secondForm.ShowDialog();

        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            pageSetupDialog1.PageSettings =
            new System.Drawing.Printing.PageSettings();


            pageSetupDialog1.PrinterSettings =
                new System.Drawing.Printing.PrinterSettings();

            pageSetupDialog1.ShowNetwork = false;
            DialogResult result = pageSetupDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                printDocument1.DefaultPageSettings = pageSetupDialog1.PrinterSettings.DefaultPageSettings;
                printDocument1.DefaultPageSettings.Margins = pageSetupDialog1.PageSettings.Margins;
                printDocument1.PrinterSettings.PrinterName = pageSetupDialog1.PrinterSettings.PrinterName;
                printDocument1.Print();
            }
        }

        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            var g = e.Graphics;
            string str_line = "-----------------------------------------------------------------------------";

            int max_height = e.PageBounds.Height;
            int height = 0;
            int temp;

            temp = TextRenderer.MeasureText(s, this.Font).Height;
            height += temp + 5;
            g.DrawString(postage + " " + s, this.Font, Brushes.Black, new Rectangle(e.PageBounds.X + 10, height, e.PageBounds.Width, temp));
            height += temp + 5;
            g.DrawString(str_line, this.Font, Brushes.Black, new Rectangle(e.PageBounds.X + 10, height, e.PageBounds.Width, temp));
            height += temp + 5;

            for (; printed < buttonListBox1.Items.Count; ++printed)
            {
                temp = TextRenderer.MeasureText((string)buttonListBox1.Items[printed], this.Font).Height;

                if (height + temp + 5 > max_height)
                    break;

                g.DrawString((string)buttonListBox1.Items[printed], this.Font, Brushes.Black, new Rectangle(e.PageBounds.X, height, e.PageBounds.Width, temp));

                height += temp + 5;
            }

            e.HasMorePages = printed != buttonListBox1.Items.Count;
        }

        private void printDocument1_BeginPrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            if (printed != 0)
                e.Cancel = true;
        }

        private void printDocument1_EndPrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            printed = 0;
        }

        private void сортировкаToolStripMenuItem_Click(object sender, EventArgs e)
        {

            string id = "";
            string descr = "";
            long is_time = 0;
            if (listBox1.Sorted == false)
            {
                listBox1.Sorted = true; this.сортировкаToolStripMenuItem.Text = "Сортировка по добавлению";

                XmlDocument my = new XmlDocument();
                my.Load(DataXMLPath);

                foreach (XmlNode xn in my.GetElementsByTagName("Track"))
                {

                    foreach (XmlAttribute attr in xn.Attributes)
                    {
                        if (attr.Name == "Descr") { descr = attr.Value; }
                        if (attr.Name == "State")
                        {

                            is_time = DateTime.Now.Ticks / 10000000 / 60 / 60 / 24 - Convert.ToInt32(attr.Value);
                            if ((attr.Value != "0") & (Convert.ToInt32(is_time) < 100))
                            {
                                listBox1.daysListItem[listBox1.Items.IndexOf(descr) + 1] = is_time.ToString();
                            }
                            else
                            {
                                listBox1.daysListItem[listBox1.Items.IndexOf(descr) + 1] = " ";
                            }

                        }
                        if (attr.Name == "Arhive")
                        {
                            if (attr.Value == "1")
                            {

                                listBox1.archiveListItem[listBox1.Items.IndexOf(descr) + 1] = true;
                            }
                            else
                            {
                                listBox1.archiveListItem[listBox1.Items.IndexOf(descr) + 1] = false;
                            }
                        }
                    }
                }


            }
            else
            {
                this.listBox1.Sorted = false;
                this.сортировкаToolStripMenuItem.Text = "Сортировка по имени";
                this.listBox1.Items.Clear();
                try
                {
                    XmlDocument my = new XmlDocument();
                    my.Load(DataXMLPath);


                    foreach (XmlNode xn in my.GetElementsByTagName("Track"))
                    {

                        foreach (XmlAttribute attr in xn.Attributes)
                        {
                            if (attr.Name == "id") { id = attr.Value; }
                            if (attr.Name == "Descr") { descr = attr.Value; }
                            if (attr.Name == "Arhive")
                            {
                                if (attr.Value == "0")
                                {
                                    listBox1.Items.Add(descr);
                                }
                                if ((attr.Value == "1") && (ViewArchiveItems == true))
                                {
                                    listBox1.Items.Add(descr);
                                }
                            }
                        }
                    }

                    foreach (XmlNode xn in my.GetElementsByTagName("Track"))
                    {

                        foreach (XmlAttribute attr in xn.Attributes)
                        {
                            if (attr.Name == "Descr") { descr = attr.Value; }
                            if (attr.Name == "State")
                            {

                                is_time = DateTime.Now.Ticks / 10000000 / 60 / 60 / 24 - Convert.ToInt32(attr.Value);
                                if ((attr.Value != "0") & (Convert.ToInt32(is_time) < 100))
                                {
                                    listBox1.daysListItem[listBox1.Items.IndexOf(descr) + 1] = is_time.ToString();
                                }
                                else
                                {
                                    listBox1.daysListItem[listBox1.Items.IndexOf(descr) + 1] = " ";
                                }

                            }
                            if (attr.Name == "Arhive")
                            {
                                if (attr.Value == "1")
                                {

                                    listBox1.archiveListItem[listBox1.Items.IndexOf(descr) + 1] = true;
                                }
                                else
                                {
                                    listBox1.archiveListItem[listBox1.Items.IndexOf(descr) + 1] = false;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error : " + ex.Message);
                }
            }

            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("ProxySettings");
            if (listBox1.Sorted == true)
            {
                key.SetValue("listBoxSorted", 1);
            }
            else
            {
                key.SetValue("listBoxSorted", 0);
            }
            key.Close();
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.myX = this.Width;
            Properties.Settings.Default.myY = this.Height;
            Properties.Settings.Default.splitter = this.splitContainer1.SplitterDistance;
            Properties.Settings.Default.Archive = ViewArchiveItems;
            Properties.Settings.Default.Save();

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Environment.Exit(1);
        }

        private void сделатьАрхивнымToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var Descr = " ";

            XmlDocument myXmlDocument = new XmlDocument();
            myXmlDocument.Load(DataXMLPath);

            foreach (XmlNode xv in myXmlDocument.GetElementsByTagName("Items"))
            {
                foreach (XmlNode xn in xv)
                {
                    if (xn.Name == "Track")
                    {
                        foreach (XmlAttribute attr in xn.Attributes)
                        {
                            if (attr.Name == "Descr")
                            {
                                Descr = attr.Value;
                            }
                            if ((attr.Name == "Arhive") & (Descr == postage))
                            {
                                if (attr.Value == "1")
                                {
                                    attr.Value = "0";
                                    listBox1.archiveListItem[listBox1.Items.IndexOf(Descr) + 1] = false;
                                }
                                else
                                {
                                    attr.Value = "1";
                                    listBox1.archiveListItem[listBox1.Items.IndexOf(Descr) + 1] = true;
                                }
                            }
                        }
                    }
                }
            }
            myXmlDocument.Save(DataXMLPath);
            listBox1.Refresh();
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            XmlDocument my = new XmlDocument();
            var id = " ";
            var descr = " ";
            var i = 0;
            long is_time = 0;
            numberOftrack = 0;
            listBox1.Items.Clear();

            if (ViewArchiveItems == true)
            {
                ViewArchiveItems = false;
                this.toolStripButton7.Text = "Показать архивные";

                try
                {

                    my.Load(DataXMLPath);


                    foreach (XmlNode xn in my.GetElementsByTagName("Track"))
                    {

                        foreach (XmlAttribute attr in xn.Attributes)
                        {
                            if (attr.Name == "id") { id = attr.Value; }
                            if (attr.Name == "Descr")
                            {
                                descr = attr.Value;
                            }
                            if (attr.Name == "Arhive")
                            {
                                if (attr.Value == "0")
                                {
                                    listBox1.Items.Add(descr);
                                    Tracks[i] = id + "/" + descr;
                                    i++;
                                    numberOftrack++;
                                    StatusLabel5.Text = numberOftrack.ToString();
                                }
                                if ((attr.Value == "1") && (ViewArchiveItems == true))
                                {
                                    listBox1.Items.Add(descr);
                                    Tracks[i] = id + "/" + descr;
                                    i++;
                                    numberOftrack++;
                                    StatusLabel5.Text = numberOftrack.ToString();
                                }
                            }
                        }
                    }


                    foreach (XmlNode xn in my.GetElementsByTagName("Track"))
                    {

                        foreach (XmlAttribute attr in xn.Attributes)
                        {
                            if (attr.Name == "Descr") { descr = attr.Value; }
                            if ((attr.Name == "State"))
                            {

                                is_time = DateTime.Now.Ticks / 10000000 / 60 / 60 / 24 - Convert.ToInt32(attr.Value);
                                if ((attr.Value != "0") & (Convert.ToInt32(is_time) < 100))
                                {
                                    listBox1.daysListItem[listBox1.Items.IndexOf(descr) + 1] = is_time.ToString();
                                }
                                else
                                {
                                    listBox1.daysListItem[listBox1.Items.IndexOf(descr) + 1] = " ";
                                }
                            }
                            if (attr.Name == "Arhive")
                            {
                                if (attr.Value == "1")
                                {

                                    listBox1.archiveListItem[listBox1.Items.IndexOf(descr) + 1] = true;
                                }
                                else
                                {
                                    listBox1.archiveListItem[listBox1.Items.IndexOf(descr) + 1] = false;
                                }
                            }
                        }
                    }


                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error : " + ex.Message);
                }
            }
            else
            {
                my.Load(DataXMLPath);
                ViewArchiveItems = true;
                this.toolStripButton7.Text = "Скрыть архивные";


                try
                {

                    my.Load(DataXMLPath);


                    foreach (XmlNode xn in my.GetElementsByTagName("Track"))
                    {

                        foreach (XmlAttribute attr in xn.Attributes)
                        {
                            if (attr.Name == "id") { id = attr.Value; }
                            if (attr.Name == "Descr")
                            {
                                descr = attr.Value;
                            }
                            if (attr.Name == "Arhive")
                            {
                                if (attr.Value == "0")
                                {
                                    listBox1.Items.Add(descr);
                                    Tracks[i] = id + "/" + descr;
                                    i++;
                                    numberOftrack++;
                                    StatusLabel5.Text = numberOftrack.ToString();
                                }
                                if ((attr.Value == "1") && (ViewArchiveItems == true))
                                {
                                    listBox1.Items.Add(descr);
                                    Tracks[i] = id + "/" + descr;
                                    i++;
                                    numberOftrack++;
                                    StatusLabel5.Text = numberOftrack.ToString();
                                }
                            }
                        }
                    }


                    foreach (XmlNode xn in my.GetElementsByTagName("Track"))
                    {

                        foreach (XmlAttribute attr in xn.Attributes)
                        {
                            if (attr.Name == "Descr") { descr = attr.Value; }
                            if ((attr.Name == "State"))
                            {

                                is_time = DateTime.Now.Ticks / 10000000 / 60 / 60 / 24 - Convert.ToInt32(attr.Value);
                                if ((attr.Value != "0") & (Convert.ToInt32(is_time) < 100))
                                {
                                    listBox1.daysListItem[listBox1.Items.IndexOf(descr) + 1] = is_time.ToString();
                                }
                                else
                                {
                                    listBox1.daysListItem[listBox1.Items.IndexOf(descr) + 1] = " ";
                                }
                            }
                            if (attr.Name == "Arhive")
                            {
                                if (attr.Value == "1")
                                {

                                    listBox1.archiveListItem[listBox1.Items.IndexOf(descr) + 1] = true;
                                }
                                else
                                {
                                    listBox1.archiveListItem[listBox1.Items.IndexOf(descr) + 1] = false;
                                }
                            }
                        }
                    }


                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error : " + ex.Message);
                }
            }

        }

        private void progressBarNew1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            List<CSV> CSV_Struct = new List<CSV>();

            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            openFileDialog1.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                CSV_Struct = CSV.ReadFile(openFileDialog1.FileName);
                XmlDocument my = new XmlDocument();
                my.Load(DataXMLPath);

                for (int i = 0; i <= CSV_Struct.Count - 1; i++)
                {

                    if (numberOftrack <= 50)
                    {
                        int status = 0;

                        if (CSV_Struct[i].ID == "") { MessageBox.Show("Пустой запрос!"); status = 1; return; }

                        foreach (XmlNode xn in my.GetElementsByTagName("Track"))
                        {

                            foreach (XmlAttribute attr in xn.Attributes)
                            {
                                if (attr.Value == CSV_Struct[i].ID) { MessageBox.Show("Такой элемент уже есть в базе!"); status = 1; }
                            }
                        }

                        if (status == 0)
                        {
                            XmlNode xn = my.GetElementsByTagName("Items").Item(0);
                            XmlElement newitem = my.CreateElement("Track");
                            XmlAttribute idattr;

                            idattr = my.CreateAttribute("id");
                            idattr.Value = CSV_Struct[i].ID;
                            newitem.SetAttributeNode(idattr);

                            idattr = my.CreateAttribute("Descr");
                            idattr.Value = CSV_Struct[i].Descr;
                            newitem.SetAttributeNode(idattr);

                            idattr = my.CreateAttribute("State");
                            idattr.Value = "0";
                            newitem.SetAttributeNode(idattr);

                            idattr = my.CreateAttribute("Arhive");
                            idattr.Value = "0";
                            newitem.SetAttributeNode(idattr);

                            xn.InsertAfter(newitem, xn.LastChild);

                            listBox1.Items.Add(CSV_Struct[i].Descr);
                            Tracks[numberOftrack] = CSV_Struct[i].ID + "/" + CSV_Struct[i].Descr;
                            numberOftrack++;

                        }
                    }
                }
                StatusLabel5.Text = numberOftrack.ToString();
                my.Save(DataXMLPath);
                XmlDocument myX = new XmlDocument();
                myX.Load(DataXMLPath);
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
                            if ((attr.Value != "0") & (Convert.ToInt32(is_time) < 100))
                            {
                                listBox1.daysListItem[listBox1.Items.IndexOf(descr) + 1] = is_time.ToString();
                            }
                            else
                            {
                                listBox1.daysListItem[listBox1.Items.IndexOf(descr) + 1] = " ";
                            }

                        }
                        if (attr.Name == "Arhive")
                        {
                            if (attr.Value == "1")
                            {

                                listBox1.archiveListItem[listBox1.Items.IndexOf(descr) + 1] = true;
                            }
                            else
                            {
                                listBox1.archiveListItem[listBox1.Items.IndexOf(descr) + 1] = false;
                            }
                        }
                    }
                }
                listBox1.Refresh();
            }
        }

        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            saveFileDialog1.Filter = "*.csv|*.csv";
            saveFileDialog1.RestoreDirectory = true;

          if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                FileStream f = new FileStream(saveFileDialog1.FileName, FileMode.Create, FileAccess.Write);
                StreamWriter stm = new StreamWriter(f, System.Text.Encoding.Unicode);
                for (var i = 0; i <numberOftrack; i++)
                {
                    string str = Tracks[i];
                    string[] words = str.Split('/'); 
                    
                    stm.WriteLine(words[0] + "," + words[1]);
                }
                stm.Close();
            }

        }

        private void toolStripButton10_Click(object sender, EventArgs e)
        {
            XmlDocument myXmlDocument = new XmlDocument();
            myXmlDocument.Load(DataXMLPath);
            XmlElement root = myXmlDocument.DocumentElement;

            XmlNode xv = myXmlDocument.GetElementsByTagName("Items")[0];
                
                    xv.RemoveAll();
                   
            myXmlDocument.Save(DataXMLPath); 
            listBox1.Items.Clear(); 
            numberOftrack = 0;
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void toolStripButton11_Click(object sender, EventArgs e)
        {
            Form2 secondForm = new Form2();
            secondForm.Owner = this;
            secondForm.ShowDialog();
           // toolStripButton4.DisplayStyle = ToolStripItemDisplayStyle.Text; 

        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            if (refresh_icon == true)
            {
                var res = Properties.Resources.ResourceManager.GetObject("refresh_90");
                notifyIcon1.Icon = (System.Drawing.Icon)res;
                refresh_icon = false;
            }
            else
            {
                var res = Properties.Resources.ResourceManager.GetObject("refresh");
                notifyIcon1.Icon = (System.Drawing.Icon)res;
                refresh_icon = true;
            }

        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }
    }
}
