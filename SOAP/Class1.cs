using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace ListBoxButtons
{
    sealed public class ButtonListBoxMy : ListBox
    {
        //вcпомогательные переменные для отрисовки
        public int[] numListItem = new int[50];
        public string[] daysListItem = new string[50];
        public bool[] archiveListItem = new bool[50];

        int x, y, itemWidth, itemHeight;

        public ButtonListBoxMy()
        {
            DrawMode = DrawMode.OwnerDrawVariable;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            //e - элемент, с которым мы дальше и работаем
            //если текущего элемента нет или в списке нет вообще элементов, выходим из метода
            if (e.Index <= -1 || this.Items.Count == 0)
                return;

            //получаем текст элемента
            string s = Items[e.Index].ToString();


            //формат строки для рисования текста
            StringFormat sf = new StringFormat();
            //формат выставляем по центру
            sf.Alignment = StringAlignment.Near;
            sf.FormatFlags = StringFormatFlags.NoWrap;
            StringFormat sf1 = new StringFormat();
            sf1.Alignment = StringAlignment.Center;
            sf1.FormatFlags = StringFormatFlags.NoWrap;

            //создаем обычную кисть с заданным цветом
            Brush textBrush = new SolidBrush(Color.Black);
            Brush textBrush1 = new SolidBrush(Color.White);
            Brush bgBrush = new SolidBrush(Color.LightGray);
            Brush bgBrush1 = new SolidBrush(Color.White);
            Brush bgBrush2 = new SolidBrush(Color.Yellow);
            Pen bgPen = new Pen(Color.SkyBlue, 1);
            //Brush bgBrush3 = new SolidBrush(Color.SkyBlue);
            //определяем координаты элемента в списке
            //т.к. для каждого элемента они разные
            x = e.Bounds.X;
            y = e.Bounds.Y;

            //также определяем его ширину и высоту 
            itemWidth = e.Bounds.Width;
            itemHeight = e.Bounds.Height;

            if ((e.State & DrawItemState.Focus) == DrawItemState.Focus)//если активный
            {
                //рисуем выбранный элемент
                e.Graphics.FillRectangle(new SolidBrush(Color.SkyBlue), x, y, itemWidth-22, itemHeight);
           
                 //рисуем текст элемента
                e.Graphics.DrawString(s, Font, textBrush, new Rectangle(5, y + 10, itemWidth, 16), sf);
                e.Graphics.FillRectangle(new SolidBrush(Color.RoyalBlue), itemWidth - 22, y, 22, itemHeight);

                if (archiveListItem[e.Index + 1] == false)
                {
                    e.Graphics.DrawString(daysListItem[e.Index + 1], Font, textBrush1, new Rectangle(itemWidth - 22, y + 10, 22, itemHeight), sf1);
                }
                else
                {
                    e.Graphics.DrawString("A", Font, textBrush1, new Rectangle(itemWidth - 22, y + 10, 22, itemHeight), sf1);
                }
                //e.Graphics.DrawString(, Font, textBrush, new Rectangle(5, y + 10, itemWidth, 16), sf);
            
            }
            else // если не активный
            {
                //заполняем прямоугольник выбранным цветом
              //  if (e.Index % 2 != 0)

                for (int i = 1; i < 50; i++)
                {

                    if (numListItem[e.Index + 1] == 1)
                    {
                        e.Graphics.FillRectangle(bgBrush, x, y, itemWidth, itemHeight);
                    }
                    else
                    {
                        e.Graphics.FillRectangle(bgBrush1, x, y, itemWidth, itemHeight);
                       // e.Graphics.DrawLine(bgPen, x, y, itemWidth - 22, itemHeight);
                    }
                }

  
                e.Graphics.FillRectangle(new SolidBrush(Color.RoyalBlue), itemWidth - 22, y, 22, itemHeight);

                e.Graphics.DrawRectangle(bgPen, x, y, itemWidth, itemHeight);
                //пишем текст

                e.Graphics.DrawString(s, Font, textBrush, new Rectangle(5, y + 10, itemWidth, 16), sf);

                if (archiveListItem[e.Index + 1] == false)
                {
                    e.Graphics.DrawString(daysListItem[e.Index + 1], Font, textBrush1, new Rectangle(itemWidth - 22, y + 10, 22, itemHeight), sf1);
                }
                else
                {
                    e.Graphics.DrawString("A", Font, textBrush1, new Rectangle(itemWidth - 22, y + 10, 22, itemHeight), sf1);
                }
            }
            textBrush.Dispose();
            textBrush1.Dispose();
            bgBrush.Dispose();
            bgBrush1.Dispose();
            bgBrush2.Dispose();
        }
        //после изменения размера
        protected override void OnSizeChanged(EventArgs e)
        {
            //вызываем обновление компонента
            Refresh();
            base.OnSizeChanged(e);
        }

        //во время задания размеров элемента
        protected override void OnMeasureItem(MeasureItemEventArgs e)
        {
            //если это элемент
            if (e.Index > -1)
            {
                //задаем высоту
                e.ItemHeight = 30;
                //ширину
                e.ItemWidth = Width;
            }
        }
    }
}
