using System;
using System.Drawing;
using System.Windows.Forms;

namespace GraphDrawing
{
    public partial class Form1 : Form
    {
        // Масив значень
        private double[] xValues;
        private double[] yValues;

        public Form1()
        {
            InitializeComponent();

            // Заповнення даних
            GenerateData();

            // Підписка на подію перерисовки
            this.Paint += Form1_Paint;

            // Перерисовка при зміні розміру
            this.Resize += Form1_Resize;
        }

        private void GenerateData()
        {
            double startX = 4.5;
            double endX = 16.4;
            double dx = 2.2;

            int n = (int)Math.Ceiling((endX - startX) / dx) + 1;
            xValues = new double[n];
            yValues = new double[n];

            for (int i = 0; i < n; i++)
            {
                double x = startX + i * dx;
                xValues[i] = x;
                yValues[i] = (x * x * x - 2) / (3 * Math.Log(x)); // y = (x^3 - 2)/(3ln(x))
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int w = this.ClientSize.Width;
            int h = this.ClientSize.Height;

            g.Clear(Color.White);

            if (xValues.Length == 0) return;

            // Знаходимо мін/макс для масштабування
            double xMin = xValues[0];
            double xMax = xValues[xValues.Length - 1];
            double yMin = double.MaxValue;
            double yMax = double.MinValue;

            foreach (double y in yValues)
            {
                if (y < yMin) yMin = y;
                if (y > yMax) yMax = y;
            }

            // Поля від країв
            int margin = 40;

            // Перетворення координат (математичні -> пікселі)
            Func<double, int> XPixel = x => (int)((x - xMin) / (xMax - xMin) * (w - 2 * margin) + margin);
            Func<double, int> YPixel = y => (int)((yMax - y) / (yMax - yMin) * (h - 2 * margin) + margin);

            // Малюємо осі
            Pen axisPen = new Pen(Color.Black, 1);
            g.DrawLine(axisPen, margin, h - margin, w - margin, h - margin); // X
            g.DrawLine(axisPen, margin, margin, margin, h - margin); // Y

            // Малюємо графік
            Pen graphPen = new Pen(Color.Red, 2);
            for (int i = 0; i < xValues.Length - 1; i++)
            {
                g.DrawLine(graphPen, XPixel(xValues[i]), YPixel(yValues[i]), XPixel(xValues[i + 1]), YPixel(yValues[i + 1]));
            }

            // Малюємо точки
            Brush pointBrush = Brushes.Blue;
            foreach (var (x, y) in Zip(xValues, yValues))
            {
                int px = XPixel(x);
                int py = YPixel(y);
                g.FillEllipse(pointBrush, px - 3, py - 3, 6, 6);
            }
        }

        // Допоміжний метод для zip
        private static System.Collections.Generic.IEnumerable<(T1, T2)> Zip<T1, T2>(T1[] a, T2[] b)
        {
            for (int i = 0; i < Math.Min(a.Length, b.Length); i++)
                yield return (a[i], b[i]);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            this.Invalidate(); // Перемалювати при зміні розміру
        }
    }
}
