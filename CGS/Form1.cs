using System.Windows.Forms.Design.Behavior;

namespace CGS
{
    public partial class Form1 : Form
    {
        Graphics g;
        Pen DrawPen = new Pen(Color.Black, 1);
        const int np = 20;
        Point[] ArPoints = new Point[np];
        int CountPoints = 0;
        int? SplineType=null;
        public Form1()
        {
            InitializeComponent();
            g = pictureBox1.CreateGraphics();
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            this.Resize += new EventHandler(Form1_Resize);
        }
        static double Factorial(int n)
        {
            double x = 1;
            for (int i = 1; i <= n; i++)
                x *= i;
            return x;
        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
            g = pictureBox1.CreateGraphics();
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        }
        public void DrawCubeSpline(Pen DrPen, Point[] P)
        {
            PointF[] L = new PointF[4]; // Матрица вещественных коэффициентов
            Point Pv1 = P[0];
            Point Pv2 = P[0];
            const double dt = 0.04;
            double t = 0;
            double xt, yt;
            Point Ppred = P[0], Pt = P[0];
            // Касательные векторы
            Pv1.X = 4 * (P[1].X - P[0].X);
            Pv1.Y = 4 * (P[1].Y - P[0].Y);
            Pv2.X = 4 * (P[3].X - P[2].X);
            Pv2.Y = 4 * (P[3].Y - P[2].Y);
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
        }
        public void DrawBezie(Pen DrPen,Point[] P,int n)
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
                    double J = Math.Pow(t, i) * Math.Pow(1 - t, n - i) * nFact / (Factorial(i) * Factorial(n - i));
                    xt += P[i].X * J;
                    yt += P[i].Y * J;
                    i++;
                }

                g.DrawLine(DrPen, xPred, yPred, (int)xt, (int)yt);
                t += dt;
                xPred = (int)xt;
                yPred = (int)yt;
            }
        }
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (CountPoints >= np) return;
            if (SplineType == null) return;
            ArPoints[CountPoints].X = e.X; ArPoints[CountPoints].Y = e.Y;
            g.DrawEllipse(DrawPen, e.X - 2, e.Y - 2, 5, 5);
            if (SplineType == 0) // Кубический сплайн
            {
                switch (CountPoints)
                {
                    case 1: // первый вектор
                        {
                            g.DrawLine(DrawPen, ArPoints[0], ArPoints[1]);
                            CountPoints++;
                        }
                        break;
                    case 3: // второй вектор
                        {
                            g.DrawLine(DrawPen, ArPoints[2], ArPoints[3]);
                            DrawCubeSpline(DrawPen, ArPoints);
                            CountPoints = 0;
                        }
                        break;
                    default:
                        CountPoints++; // иначе
                        break;
                }
            }
            else // Кривая Безье
            {
                if (e.Button == MouseButtons.Right) // Конец ввода
                {
                    DrawBezie(DrawPen, ArPoints, CountPoints);
                    CountPoints = 0;
                }
                else CountPoints++;
            }
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SplineType = comboBox1.SelectedIndex;
        }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
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
        private void button1_Click(object sender, EventArgs e)
        {
            g.Clear(Color.White);
            CountPoints = 0;
        }

    }
}
