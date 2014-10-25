using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
namespace Postage
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]

        [DllImport("user32.dll")]
        static extern bool ShowWindow(int hWnd, int n);


        [DllImport("User32.dll")]
        private static extern int FindWindow(String ClassName, String WindowName);
        [STAThread]
        static void Main()
        {
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

           
                int hWnd = FindWindow(null, "Отслеживание почтовых отправлений через Почту России");
                if (hWnd > 0) //нашли
                {
                    ShowWindow(hWnd,1);
                    return;
      
                }
            else
            {
                Application.Run(new Form1());
            }
        }
    }
}
