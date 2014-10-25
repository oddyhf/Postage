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
    public partial class Form5 : Form
    {
        public Form5()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1 main = this.Owner as Form1;
            if (main != null)
            {
                main.timerinterval = DateTime.Now.Ticks;
                main.timereload = (int)numericUpDown1.Value;
                main.timer1.Interval = main.timereload*60000;
                Microsoft.Win32.RegistryKey key;
                key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("ProxySettings");
                key.SetValue("TimeInterval", (int)numericUpDown1.Value);
                this.Close();
            }
        }
    }
}
