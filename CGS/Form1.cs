using System.Windows.Forms.Design.Behavior;

namespace CGS
{
    public partial class Form1 : Form
    {
        Bitmap bitmap; //Объявление переменной типа bitmap
        Graphics g; //Объявление поверхности рисования Graphics
        Pen DrawPen = new Pen(Color.Black, 1); //Инициализация объекта для рисования линий
        const int np = 20; //Инициализация переменной для ограничения размера массива точек
        Point[] ArPoints = new Point[np]; //Объявление массива точек
        int CountPoints = 0; //Инициализация счётчика точек
        int? SplineType = null;
        bool GuideCheck = false;
        public Form1()
        {
            InitializeComponent();
            bitmap = new Bitmap(2560, 1440);// Создание новго объекта Bitmap с разрешением 2560x1440
            pictureBox1.Image = bitmap; // Установка созданного Bitmap в качестве изображения для pictureBox1
            g = Graphics.FromImage(bitmap);// Создаём объект Graphics из bitmap
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        }
        static double Factorial(int n) //Метод для нахождения факториала
        {
            double x = 1;
            for (int i = 1; i <= n; i++)
                x *= i;
            return x;
        }
        public void SetBoxesStatus(bool Status) //Метод для блокировки/разблокировки списка comboBox
        {
            comboBox1.Enabled = Status;
            comboBox2.Enabled = Status;
            comboBox3.Enabled = Status;
        }
        public void DrawCubeSpline(Pen DrPen, Point[] P) //Метод для рисования кубических сплайнов
        {
            PointF[] L = new PointF[4]; // Матрица вещественных коэффициентов
            Point Pv1 = P[0]; // Начальная точка для первого касательного вектора
            Point Pv2 = P[0]; // Начальная точка для второго касательного вектора
            const double dt = 0.04;
            double t = 0;
            double xt, yt; // Переменные для хранения координат
            Point Ppred = P[0], Pt = P[0]; // Предыдущая и текущая точки
            // Касательные векторы
            Pv1.X = 4 * (P[1].X - P[0].X); // X-координата первого касательного вектора
            Pv1.Y = 4 * (P[1].Y - P[0].Y); // Y-координата первого касательного вектора
            Pv2.X = 4 * (P[3].X - P[2].X); // X-координата второго касательного вектора
            Pv2.Y = 4 * (P[3].Y - P[2].Y); // Y-координата второго касательного вектора
            // Коэффициенты полинома
            L[0].X = 2 * P[0].X - 2 * P[2].X + Pv1.X + Pv2.X; // Ax
            L[0].Y = 2 * P[0].Y - 2 * P[2].Y + Pv1.Y + Pv2.Y; // Ay
            L[1].X = -3 * P[0].X + 3 * P[2].X - 2 * Pv1.X - Pv2.X; // Bx
            L[1].Y = -3 * P[0].Y + 3 * P[2].Y - 2 * Pv1.Y - Pv2.Y; // By
            L[2].X = Pv1.X; // Cx
            L[2].Y = Pv1.Y; // Cy
            L[3].X = P[0].X; // Dx
            L[3].Y = P[0].Y; // Dy
            while (t < 1 + dt / 2)
            {
                xt = ((L[0].X * t + L[1].X) * t + L[2].X) * t + L[3].X;
                yt = ((L[0].Y * t + L[1].Y) * t + L[2].Y) * t + L[3].Y;
                Pt.X = (int)Math.Round(xt);
                Pt.Y = (int)Math.Round(yt);
                g.DrawLine(DrPen, Ppred, Pt);
                Ppred = Pt;
                t = t + dt;
            }
            pictureBox1.Image = bitmap; // Обновление изображения в pictureBox
        }
        public void DrawBezie(Pen DrPen, Point[] P, int n) //Метод для рисования кривых Безье
        {
            double nFact = Factorial(n);
            const double dt = 0.04;
            double t = dt;
            int xPred = P[0].X;
            int yPred = P[0].Y;
            while (t < 1 + dt / 2)
            {
                double xt = 0;
                double yt = 0;
                int i = 0;
                while (i <= n)
                {
                    double J = Math.Pow(t, i) * Math.Pow(1 - t, n - i) * nFact / (Factorial(i) * Factorial(n - i)); // Вычисление коэффициента для текущего i
                    xt += P[i].X * J;
                    yt += P[i].Y * J;
                    i++;
                }

                g.DrawLine(DrPen, xPred, yPred, (int)xt, (int)yt);
                t += dt;
                xPred = (int)xt;
                yPred = (int)yt;
            }
            pictureBox1.Image = bitmap; // Обновление изображения в pictureBox
        }
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e) //Обработчик события нажатия на кнопку мыши
        {
            if (CountPoints >= np) return; // Проверка на превышение количества точек в массиве
            if (SplineType == null) return;
            if (e.Button == MouseButtons.Left) //Условие для создания точек только при нажатии на левую кнопку мыши
            {
                if (comboBox1.Enabled == true)
                { // Блокировка кнопок comboBox
                    SetBoxesStatus(false);
                }
                ArPoints[CountPoints].X = e.X; ArPoints[CountPoints].Y = e.Y; //Запись координат X и Y в объект Point в массиве
                g.DrawEllipse(DrawPen, e.X - 2, e.Y - 2, 5, 5); // Отрисовка точек при нажатии
            }
            if (SplineType == 0) // Если выбран Кубический сплайн
            {
                switch (CountPoints)
                {
                    case 1: // первый вектор    
                        {
                            if (GuideCheck == true)
                            {
                                g.DrawLine(DrawPen, ArPoints[0], ArPoints[1]);
                            }
                            CountPoints++;
                        }
                        break;
                    case 3: // второй вектор
                        {
                            if (GuideCheck == true)
                            {
                                g.DrawLine(DrawPen, ArPoints[2], ArPoints[3]);
                            }
                            DrawCubeSpline(DrawPen, ArPoints); //Отрисовка сплайна
                            SetBoxesStatus(true); // Разблокировка ComboBox'ов
                            CountPoints = 0; //сброс счётчика
                        }
                        break;
                    default: // иначе
                        CountPoints++;
                        break;
                }
            }
            else // Кривая Безье
            {
                if (e.Button == MouseButtons.Right) // Условие для проверки нажатия правой кнопки мыши
                {
                    DrawBezie(DrawPen, ArPoints, CountPoints - 1); //Отрисовка кривой Безье
                    SetBoxesStatus(true); // Разблокировка ComboBox'ов
                    CountPoints = 0; //сброс счётчика
                }
                else
                {
                    if (CountPoints == 0)
                    {
                        CountPoints++;
                    }
                    else
                    {
                        if (GuideCheck == true)
                        {
                            g.DrawLine(DrawPen, ArPoints[CountPoints - 1], ArPoints[CountPoints]); //Отрисовка начальных отрезков
                        }
                        CountPoints++;
                    }
                }
            }
            pictureBox1.Image = bitmap;  // Обновление изображения в pictureBox
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) //Обработчик события изменения индекса comboBox1
        {
            SplineType = comboBox1.SelectedIndex;
        }
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e) //Обработчик события изменения индекса comboBox3
        {
            if (comboBox3.SelectedIndex == 0)
            {
                GuideCheck = false;
            }
            else
            {
                GuideCheck = true;
            }
        }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e) //Обработчик события изменения индекса comboBox2
        {
            switch (comboBox2.SelectedIndex) // выбор цвета
            {
                case 0:
                    DrawPen.Color = Color.Black;
                    break;
                case 1:
                    DrawPen.Color = Color.Red;
                    break;
                case 2:
                    DrawPen.Color = Color.Green;
                    break;
                case 3:
                    DrawPen.Color = Color.Blue;
                    break;
            }
        }
        private void button1_Click(object sender, EventArgs e) //Обработчик события нажатия кнопки очищения
        {
            g.Clear(Color.White); //Очистка поверхности для рисования
            CountPoints = 0; //Сброс счётчика точек
            pictureBox1.Image = bitmap; // Обновление изображения в pictureBox
            if (comboBox1.Enabled == false) //Разблокировка comboBox'ов
            {
                SetBoxesStatus(true);
            }
        }
    }
}
