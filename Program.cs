using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GraphDrawingOptimized
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }

    public class Form1 : Form
    {
        // Поля даних
        private double[] _xValues;
        private double[] _yValues;

        // Тип відображення
        private bool _drawLineGraph = true; // true - лінія, false - точки

        public Form1()
        {
            this.Text = "Графік y = (x^3 - 2)/(3 ln x)";
            this.Size = new Size(800, 600);

            // Включаємо DoubleBuffer для зменшення мерехтіння
            this.SetStyle(ControlStyles.UserPaint |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer, true);

            GenerateData();

            // UI: перемикач лінія/точки
            var checkBox = new CheckBox
            {
                Text = "Line Graph",
                Checked = true,
                Location = new Point(10, 10),
                AutoSize = true
            };
            checkBox.CheckedChanged += (s, e) =>
            {
                _drawLineGraph = checkBox.Checked;
                this.Invalidate();
            };
            this.Controls.Add(checkBox);
        }

        private void GenerateData()
        {
            double startX = 4.5;
            double endX = 16.4;
            double dx = 2.2;

            int n = (int)Math.Ceiling((endX - startX) / dx) + 1;
            _xValues = new double[n];
            _yValues = new double[n];

            for (int i = 0; i < n; i++)
            {
                double x = startX + i * dx;
                _xValues[i] = x;
                _yValues[i] = (x * x * x - 2) / (3 * Math.Log(x));
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawGraph(e.Graphics);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.Invalidate(); // Перемалювати графік при зміні розміру
        }

        private void DrawGraph(Graphics g)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(Color.White);

            int w = this.ClientSize.Width;
            int h = this.ClientSize.Height;
            int margin = 50;

            if (_xValues.Length == 0) return;

            // Обчислюємо мін/макс
            double xMin = _xValues.Min();
            double xMax = _xValues.Max();
            double yMin = _yValues.Min();
            double yMax = _yValues.Max();

            // Перевірка на ділення на нуль
            double eps = 1e-6;
            if (Math.Abs(xMax - xMin) < eps || Math.Abs(yMax - yMin) < eps) return;

            // Функції перетворення в пікселі
            Func<double, int> XPixel = x => (int)((x - xMin) / (xMax - xMin) * (w - 2 * margin) + margin);
            Func<double, int> YPixel = y => (int)((yMax - y) / (yMax - yMin) * (h - 2 * margin) + margin);

            DrawAxes(g, w, h, margin);

            // Малюємо графік
            using (var pen = new Pen(Color.Red, 2))
            {
                if (_drawLineGraph)
                {
                    for (int i = 0; i < _xValues.Length - 1; i++)
                    {
                        g.DrawLine(pen,
                            XPixel(_xValues[i]), YPixel(_yValues[i]),
                            XPixel(_xValues[i + 1]), YPixel(_yValues[i + 1]));
                    }
                }
                else
                {
                    foreach (var (x, y) in _xValues.Zip(_yValues, (a, b) => (a, b)))
                    {
                        g.FillEllipse(Brushes.Blue, XPixel(x) - 3, YPixel(y) - 3, 6, 6);
                    }
                }
            }
        }

        private void DrawAxes(Graphics g, int w, int h, int margin)
        {
            using (var axisPen = new Pen(Color.Black, 1))
            {
                // X
                g.DrawLine(axisPen, margin, h - margin, w - margin, h - margin);
                // Y
                g.DrawLine(axisPen, margin, margin, margin, h - margin);
            }
        }
    }
}
