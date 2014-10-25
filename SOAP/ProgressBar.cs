using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ProgressBarNew
{
    [Description("Продвинутый индикатор процесса")]
    public sealed class ProgressBarNew : Control
    {
        #region Переменные
        #region Приватные

        private int currentvalue = 0, minimum = 0, maximum = 100, step = 10;
        //Значения по умолчанию для всех цветов
        private Color bordercolor = Color.FromArgb(146, 137, 128);
        private Color backgroundcolor = Color.FromArgb(219, 211, 203);
        private Color barcolor1 = Color.FromArgb(255, 187, 115);
        private Color barcolor2 = Color.FromArgb(250, 163, 70);
        private Color barBorder = Color.FromArgb(206, 134, 58);
        private Color shadow1 = Color.FromArgb(198, 190, 182);
        private Color shadow2 = Color.FromArgb(207, 199, 191);
        private Color shadow3 = Color.FromArgb(215, 207, 199);

        private GraphicsPath gfxPath;
        private int cornerRadius = 3; //Радиус углов
        #endregion

        #region Публичные поля
        [Description("Текущее значение")]
        [Category("Behavior")]
        public int Value
        {
            get { return currentvalue; }
            set
            {
                currentvalue = value;
                if (currentvalue > maximum)
                    currentvalue = maximum;
                if (currentvalue < minimum)
                    currentvalue = minimum;
                this.Invalidate();
                this.Update();
            }
        }

        [Description("Минимальное значение")]
        [Category("Behavior")]
        public int Minimum
        {
            get { return minimum; }
            set
            {
                minimum = value;

                if (minimum > maximum)
                    maximum = minimum;
                if (minimum > currentvalue)
                    currentvalue = minimum;
                this.Invalidate();
                this.Update();
            }
        }

        [Description("Максимальное значение")]
        [Category("Behavior")]
        public int Maximum
        {
            get { return maximum; }
            set
            {
                maximum = value;
                if (maximum < minimum)
                    minimum = maximum;
                if (maximum < currentvalue)
                    currentvalue = maximum;
                this.Invalidate();
                this.Update();
            }
        }

        [Description("Шаг")]
        [Category("Behavior")]
        public int Step
        {
            get { return step; }
            set { step = value; }
        }

        [Description("Цвет обрамления")]
        [Category("Colors")]
        public Color BorderСolor
        {
            get { return bordercolor; }
            set
            {
                bordercolor = value;
                this.Invalidate();
                this.Update();
            }
        }

        [Description("Цвет заднего фона")]
        [Category("Colors")]
        public Color BackgroundСolor
        {
            get { return backgroundcolor; }
            set
            {
                backgroundcolor = value;
                this.Invalidate();
                this.Update();
            }
        }

        [Description("Первичный цвет бегунка")]
        [Category("Colors")]
        public Color BarColorFirst
        {
            get { return barcolor1; }
            set
            {
                barcolor1 = value;
                this.Invalidate();
                this.Update();
            }
        }

        [Description("Вторичный цвет бегунка")]
        [Category("Colors")]
        public Color BarColorSecond
        {
            get { return barcolor2; }
            set
            {
                barcolor2 = value;
                this.Invalidate();
                this.Update();
            }
        }
        #endregion

        public ProgressBarNew()
        {
            base.Size = new Size(150, 15);
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.DoubleBuffer,
                true
                );
        }
        #endregion

        #region Отрисовка
        /// <summary>
        /// Главный метод отрисовки компонента
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            //Создаем фигуру
            gfxPath = new GraphicsPath();

            int x = 0, y = 0; //начальные координаты

            //С учетом следующих значений определяется область отрисовки
            int width = ClientRectangle.Width - 1; //фактическая ширина
            int height = ClientRectangle.Height - 1; //и высота

            //Задаем форму и область
            gfxPath.AddLine(x + cornerRadius, y, x + width - (cornerRadius * 2), y);
            gfxPath.AddArc(x + width - (cornerRadius * 2), y, cornerRadius * 2, cornerRadius * 2, 270, 90);
            gfxPath.AddLine(x + width, y + cornerRadius, x + width, y + height - (cornerRadius * 2));
            gfxPath.AddArc(x + width - (cornerRadius * 2), y + height - (cornerRadius * 2), cornerRadius * 2, cornerRadius * 2, 0, 90);
            gfxPath.AddLine(x + width - (cornerRadius * 2), y + height, x + cornerRadius, y + height);
            gfxPath.AddArc(x, y + height - (cornerRadius * 2), cornerRadius * 2, cornerRadius * 2, 90, 90);
            gfxPath.AddLine(x, y + height - (cornerRadius * 2), x, y + cornerRadius);
            gfxPath.AddArc(x, y, cornerRadius * 2, cornerRadius * 2, 180, 90);
            gfxPath.CloseFigure();

            //Закрашиваем область
            SolidBrush bgBrush = new SolidBrush(backgroundcolor); //кисть с заданным цветом
            g.FillPath(bgBrush, gfxPath); //закрашиваем область
            bgBrush.Dispose(); //удаляем из памяти
            //Тени задаются для радиуса закруглений от 2 до 4
            //Рисуем тень линиями
            g.DrawLine(new Pen(shadow1), new Point(x + cornerRadius - 1, y + 1), new Point(x + width - (cornerRadius - 1), y + 1));
            g.DrawLine(new Pen(shadow2), new Point(x + cornerRadius - 2, y + 2), new Point(x + width - (cornerRadius - 2), y + 2));
            g.DrawLine(new Pen(shadow3), new Point(x + cornerRadius - 3, y + 3), new Point(x + width - (cornerRadius - 3), y + 3));

            //Если текущее значение минимально, ничего не отрисовываем и выходим из метода
            if (maximum == minimum || currentvalue == minimum)
            {
                //Рисуем только бордюр
                drawBorder(e.Graphics);
                return;
            }
            //Задаем отступ
            x += 1;

            //Определяем, какую ширину должен иметь бегунок процесса в пикселях
            //Без учета по одному пикселю слева и справа
            int fillWidth = ((width - x) * currentvalue) / (maximum - minimum);

            //Если размер индикатора процесса, который необходимо нарисовать, больше 0
            if (fillWidth > 0)
            {
                //Определяем область отрисовки индикатора процесса
                GraphicsPath gfxPathBar = new GraphicsPath();
                //В любом случае рисуем одну вертикальную левую линию снизу вверх
                gfxPathBar.AddLine(x, y + height - (cornerRadius), x, y + cornerRadius);

                //Только если ширина индикатора процесса больше 1 рисуем все остальное
                if (fillWidth > 1)
                {
                    gfxPathBar.AddArc(x, y, cornerRadius, cornerRadius, 180, 90);
                    gfxPathBar.AddLine(x + cornerRadius, y, x + fillWidth - cornerRadius, y);
                    gfxPathBar.AddArc(x + fillWidth - cornerRadius, y, cornerRadius, cornerRadius, 270, 90);
                    gfxPathBar.AddLine(x + fillWidth, y + cornerRadius, x + fillWidth, y + height - (cornerRadius * 2));
                    gfxPathBar.AddArc(x + fillWidth - (cornerRadius * 2), y + height - (cornerRadius * 2), cornerRadius * 2, cornerRadius * 2, 0, 90);
                    gfxPathBar.AddLine(x + fillWidth - (cornerRadius * 2), y + height, x + cornerRadius, y + height);
                    gfxPathBar.AddArc(x, y + height - (cornerRadius), cornerRadius, cornerRadius, 90, 90);
                }
                //Определяем область закрашивания
                //В данном случае координаты значения не имеют
                //Так как здесь создается размер кисти
                Rectangle mainProgressRect = new Rectangle(0, 0, fillWidth, height - 1);
                //Создаем градиентную кисть для закрашивания
                Brush gradientBrush = new LinearGradientBrush(mainProgressRect, barcolor1, barcolor2, LinearGradientMode.Vertical);
                g.FillPath(gradientBrush, gfxPathBar); //закрашиваем
                gradientBrush.Dispose(); //уничтожаем

                //Если не достигнут край, рисуем бордюр с правой стороны индикатора
                //Не рисуем, если значение максимально
                if (fillWidth != width - x && currentvalue != maximum)
                    g.DrawLine(new Pen(barBorder), x + fillWidth - 1, y + 1, x + fillWidth - 1, height - 1);

            }

            //Рисуем бордюр
            drawBorder(g);
            //Пишем текст
           // drawText(g);
        }

        /// <summary>
        /// Показывать текстовую информацию
        /// </summary>
        /// <param name="g"></param>
        private void drawText(Graphics g)
        {
            Font f = new System.Drawing.Font("Arial", 9, FontStyle.Bold);
            StringFormat sf = new StringFormat();
            //Формат выставляем по центру горизонтали и вертикали
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;
            //Отображаем текст на экране заданным цветом
            g.DrawString(string.Format("{0}%", currentvalue), f, new SolidBrush(Color.FromArgb(73, 73, 73)), ClientRectangle, sf);
            f.Dispose();
            sf.Dispose();
        }

        /// <summary>
        /// Рисуем бордюр с закругленными углами
        /// </summary>
        /// <param name="g">На чем будем рисовать</param>
        private void drawBorder(Graphics g)
        {
            //Задаем режим сглаживания
            g.SmoothingMode = SmoothingMode.AntiAlias;
            //Рисуем бордюр уже поверх всего остального, что было нарисовано ранее
            g.DrawPath(new Pen(bordercolor), gfxPath);
            //Уничтожаем область
            gfxPath.Dispose();
        }
        #endregion

        #region Публичные методы
        /// <summary>
        /// Увеличение текущего значения на заданную величину
        /// </summary>
        /// <param name="value">Целое значение</param>
        public void Increment(int value)
        {
            currentvalue += value;
            if (currentvalue > maximum)
                currentvalue = maximum;
            if (currentvalue < minimum)
                currentvalue = minimum;
            Invalidate();
            Update();
        }

        /// <summary>
        /// Уменьшение текущего значения на заданную величину
        /// </summary>
        /// <param name="value">Целое значение</param>
        public void Decrement(int value)
        {
            currentvalue -= value;
            if (currentvalue < minimum)
                currentvalue = minimum;
            if (currentvalue > maximum)
                currentvalue = maximum;
            Invalidate();
            Update();
        }

        /// <summary>
        /// Увеличение текущего значения на заданный шаг
        /// </summary>
        public void PerformStep()
        {
            Increment(step);
        }

        /// <summary>
        /// Уменьшение текущего значения на заданный шаг
        /// </summary>
        public void PerformStepBack()
        {
            Decrement(step);
        }
        #endregion
    }
}
