using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PI.Core
{
    public class Box
    {
        private readonly double _containerWidth;
        private readonly SolidColorBrush _background;
        private double _v, _m;
        private readonly Pen _pen;

        public double X { get; set; }
        public double Y { get; }
        public double Width { get; }
        public double M
        {
            get { return _m; }
            set { _m = value; }
        }
        public double V
        {
            get { return _v; }
            set { _v = value; }
        }

        public double Bounce(Box other)
        {
            var sumM = this.M + other.M;
            var newV = (this.M - other.M) / sumM * this.V;
            return newV + (2 * other.M / sumM) * other.V;
        }

        public bool Move(int times)
        {
            if(X>_containerWidth)
                return false;
            X += V / times;
            return true;
        }

        public bool HitWall()
        {
            if (X < 0)
            {
                V *= -1;
                return true;
            }

            return false;
        }

        public Box(double x, double width, double containerWidth, double containerHeight, SolidColorBrush background, double v = -1)
        {
            X = x;
            Y = containerHeight-width;
            Width = width;
            V = v;
            M = 1;
            _containerWidth = containerWidth;
            _background = background;
            _pen = new Pen(Brushes.Black, 0.5);
        }

        public void Draw(DrawingContext dc)
        {
            dc.DrawRectangle(_background, _pen, new Rect(X, Y, Width, Width));
        }

        public void Reset(double x , double v = -1)
        {
            M = 1;
            V = v;
            X = x;
        }
    }
}
