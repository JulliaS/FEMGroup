using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Ірщз.model
{
    public class Triangle : Shape
    {
        public List<Point> Points
        {
            get { return (List<Point>)this.GetValue(PointProperty); }
            set { this.SetValue(PointProperty, value); }
        }
        
        public int SegmentId
        {
            get {
                return (int)this.GetValue(SegmentIdProperty);
            }
            set {
                this.SetValue(SegmentIdProperty, value);
            }
        }

        public static readonly DependencyProperty PointProperty = DependencyProperty.Register("Points", typeof(List<Point>), typeof(Triangle));
        public static readonly DependencyProperty SegmentIdProperty = DependencyProperty.Register("SegmentId", typeof(int), typeof(Triangle));
        public Triangle()
        {
        }

        protected override Geometry DefiningGeometry
        {
            get
            {

                System.Windows.Point p1 = new System.Windows.Point(Points[0].X, Points[0].Y);
                System.Windows.Point p2 = new System.Windows.Point(Points[1].X, Points[1].Y);
                System.Windows.Point p3 = new System.Windows.Point(Points[2].X, Points[2].Y);

                List<PathSegment> segments = new List<PathSegment>(3);
                segments.Add(new LineSegment(p1, true));
                segments.Add(new LineSegment(p2, true));
                segments.Add(new LineSegment(p3, true));

                List<PathFigure> figures = new List<PathFigure>(1);
                PathFigure pf = new PathFigure(p1, segments, true);
                figures.Add(pf);

                Geometry g = new PathGeometry(figures, FillRule.EvenOdd, null);

                return g;
            }
        }

    }
}
