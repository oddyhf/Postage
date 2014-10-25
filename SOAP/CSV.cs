using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Postage
{
    class CSV
    {
        public string ID { get; set; }
        public string Descr { get; set; }



        //Метод для получения частей из строки
        public void piece(string line)
        {
            string[] parts = line.Split(',');  //Разделитель в CVS файле.
            ID = parts[0];
            Descr = parts[1];
        }



        public static List<CSV> ReadFile(string filename)
        {
            List<CSV> res = new List<CSV>();
            if (File.Exists(filename))
            {
                using (StreamReader sr = new StreamReader(filename))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        CSV p = new CSV();
                        p.piece(line);
                        res.Add(p);
                    }
                }
            }
            return res;
        }
    }
}
