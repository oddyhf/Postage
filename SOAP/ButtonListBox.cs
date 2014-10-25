using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace ListBoxButtons
{
    sealed class ButtonListBox : ListBox
    {
        public string[] timesListItem = new string[50];
        int x, y, itemWidth, itemHeight;

        public ButtonListBox()
        {
            
            DrawMode = DrawMode.OwnerDrawVariable;
        }
        #region Прорисовка

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

            //создаем обычную кисть с заданным цветом
            Brush textBrush = new SolidBrush(Color.Black);
            Brush textBrush2 = new SolidBrush(Color.SteelBlue);
            Brush bgBrush = new SolidBrush(Color.LightGray);
            Brush bgBrush1 = new SolidBrush(Color.White);
            Brush bgBrush2 = new SolidBrush(Color.SkyBlue);
            Brush bgBrush3 = new SolidBrush(Color.RoyalBlue);
            Pen bgPen = new Pen(Color.SkyBlue, 1);
            //определяем координаты элемента в списке
            //т.к. для каждого элемента они разные
            x = e.Bounds.X;
            y = e.Bounds.Y;

            //также определяем его ширину и высоту 
            itemWidth = e.Bounds.Width;
            itemHeight = e.Bounds.Height;

            if ((e.State & DrawItemState.Focus) == DrawItemState.Focus)//если активный
            {
            
                e.Graphics.FillRectangle(new SolidBrush(SystemColors.Info), x, y, itemWidth, itemHeight);
                e.Graphics.DrawString(s, Font, textBrush, new Rectangle(5, y + 10, itemWidth, 16), sf);
                e.Graphics.FillRectangle(textBrush2, itemWidth - 150, y, 150, itemHeight);
                e.Graphics.DrawRectangle(bgPen, itemWidth - 150, y, itemWidth, itemHeight);
                e.Graphics.DrawString(timesListItem[e.Index + 1], Font, bgBrush1, new Rectangle(itemWidth - 145, y + 10, 150, itemHeight), sf);
            }
            else // если не активный
            {
        
                if (e.Index % 2 != 0)
                {
                    e.Graphics.FillRectangle(bgBrush, x, y, itemWidth, itemHeight);
                    e.Graphics.DrawString(s, Font, textBrush, new Rectangle(5, y + 10, itemWidth, 16), sf);
                    e.Graphics.FillRectangle(textBrush2, itemWidth - 150, y, 150, itemHeight);
                    e.Graphics.DrawRectangle(bgPen, itemWidth - 150, y, itemWidth, itemHeight);
                    e.Graphics.DrawString(timesListItem[e.Index + 1], Font, bgBrush1, new Rectangle(itemWidth - 145, y + 10, 150, itemHeight), sf);
                }
                else
                {
                    e.Graphics.FillRectangle(bgBrush1, x, y, itemWidth, itemHeight);
                    e.Graphics.DrawString(s, Font, textBrush, new Rectangle(5, y + 10, itemWidth, 16), sf);
                    e.Graphics.FillRectangle(textBrush2, itemWidth - 150, y, 150, itemHeight);
                    e.Graphics.DrawRectangle(bgPen, itemWidth - 150, y, itemWidth, itemHeight);
                    e.Graphics.DrawString(timesListItem[e.Index + 1], Font, bgBrush1, new Rectangle(itemWidth - 145, y + 10, 150, itemHeight), sf);
                }
                if (e.Index == Items.Count-1)
                {
                    e.Graphics.FillRectangle(bgBrush2, x, y, itemWidth, itemHeight);
                    e.Graphics.DrawString(s, Font, textBrush, new Rectangle(5, y + 10, itemWidth, 16), sf);
                    e.Graphics.FillRectangle(textBrush2, itemWidth - 150, y, 150, itemHeight);
                    e.Graphics.DrawRectangle(bgPen, itemWidth - 150, y, itemWidth, itemHeight);
                    e.Graphics.DrawString(timesListItem[e.Index + 1], Font, bgBrush1, new Rectangle(itemWidth - 145, y + 10, 150, itemHeight), sf);
                }
            }

            textBrush.Dispose();
            bgBrush.Dispose();
            bgBrush1.Dispose();
            bgBrush2.Dispose();
            bgBrush3.Dispose();
            bgPen.Dispose();
        }

        #endregion Прорисовка

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
