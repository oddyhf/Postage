using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Postage
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1 main = this.Owner as Form1;
            if (main != null)
            {

                Microsoft.Win32.RegistryKey key;
                key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("ProxySettings");
                
                key.SetValue("ProxyName", this.textBox1.Text);
                key.SetValue("ProxyPassword", this.textBox2.Text);
                key.SetValue("ProxyAdress", this.textBox3.Text);
                key.SetValue("ProxyPort", this.textBox4.Text);
                main.ProxyName = textBox1.Text;
                main.ProxyPassword = textBox2.Text;
                main.ProxyAdress = textBox3.Text;
                main.ProxyPort = textBox4.Text;
  
                
                if (this.checkBox1.Checked)
                {
                    main.UseProxy = true;
                    key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("ProxySettings");
                    key.SetValue("UseProxy","true");
                    key.Close();
                } 
                else 
                {
                    main.UseProxy = false;
                    key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("ProxySettings");
                    key.SetValue("UseProxy", "false");
                };
                key.Close();
                this.Close();
            }
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            Form1 main = this.Owner as Form1;
            if (main != null)
            {
                textBox1.Text = main.ProxyName;
                textBox2.Text = main.ProxyPassword;
                textBox3.Text = main.ProxyAdress;
                textBox4.Text = main.ProxyPort;
                if ( main.UseProxy== true) { this.checkBox1.Checked = true; } else { this.checkBox1.Checked = false; };
                }; 
            }
        
    }
}
