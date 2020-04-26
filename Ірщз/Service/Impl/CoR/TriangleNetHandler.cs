using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using TriangleNet;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using TriangleNet.Meshing.Algorithm;
using Ірщз.Mapper;
using Ірщз.model;
using Ірщз.Service.Abstraction;
using Point = Ірщз.model.Point;
using Triangle = Ірщз.model.Triangle;

namespace Ірщз.Service.Impl.CoR
{
    public class TriangleNetHandler : PointHandler
    {
        private List<Ellipse> tempFigures = new List<Ellipse>();
        public double MaxSquare { get; set; }
        public int MinAngel { get; set; }
        public IMesh mesh;

        public TriangleNetHandler(PointHandler nextHandler) : base(nextHandler)
        {
            MaxSquare = 100;
            MinAngel = 20;
        }
        public TriangleNetHandler()
        {
            MaxSquare = 100;
            MinAngel = 20;
        }
        protected override void ProcessContext(Canvas canvas, List<Point> points)
        {
            if (points.Count < 3) { return; }

            var options = new ConstraintOptions();
            options.ConformingDelaunay = true;

            var quality = new QualityOptions();
            quality.MinimumAngle = MinAngel;
            quality.MaximumArea = MaxSquare;

            var polygon = new TriangleNet.Geometry.Polygon();

            points.ForEach(p => polygon.Add(new Vertex(p.X, p.Y)));
            for (int i = 0; i < points.Count-1; i++)
            {
                polygon.Add(new Segment(new Vertex(points[i].X, points[i].Y), new Vertex(points[i + 1].X, points[i + 1].Y)));
            }
            Point last = points.LastOrDefault();
            Point first = points.FirstOrDefault();
            polygon.Add(new Segment(new Vertex(last.X, last.Y), new Vertex(first.X, first.Y)));
            var mesh = polygon.Triangulate(options, quality);
            this.mesh = mesh;
            if (mesh != null)
            {
                mesh.Refine(quality, true);
            }
            this.mesh.Renumber();
            foreach (ITriangle triangle in mesh.Triangles)
            {

                Point p1 = new Point
                {
                    X = (triangle.GetVertex(0).X + 1) * 100,
                    Y = (triangle.GetVertex(0).Y + 1)* 100,
                    ID = triangle.GetVertex(0).ID
                };
                Point p2 = new Point
                {
                    X = (triangle.GetVertex(1).X + 1)*100,
                    Y = (triangle.GetVertex(1).Y + 1)*100,
                    ID = triangle.GetVertex(1).ID
                };
                Point p3 = new Point
                {
                    X = (triangle.GetVertex(2).X + 1)*100,
                    Y = (triangle.GetVertex(2).Y + 1)*100,
                    ID = triangle.GetVertex(2).ID
                };
                Triangle myTriangle = new Triangle
                {
                    Points = new List<Point> { p1, p2, p3 },
                    SegmentId = triangle.ID
                };

                myTriangle.Stroke = Brushes.Black;
                myTriangle.MouseEnter += (sender, e) => { MouseEnterAction(sender, e); };
                myTriangle.MouseLeave += (sender, e) => { MouseLeaveAction(sender, e); };
                myTriangle.MouseDown += (sender, e) => { MouseUp(sender, e); };
                if (canvas != null)
                    canvas.Children.Add(myTriangle);
            }
        }

        private void MouseEnterAction(object sender, EventArgs e)
        {
            if (sender == null) { return; }
            var polygon = (Triangle)sender;
            polygon.Stroke = Brushes.Red;
            polygon.Fill = Brushes.Maroon;

            Canvas parent = (Canvas)polygon.Parent;
            foreach (Point p in polygon.Points)
            {
                Ellipse ellipse = new Ellipse
                {
                    Width = 7,
                    Height = 7,
                    Fill = Brushes.Black
                };
                parent.Children.Add(ellipse);
                tempFigures.Add(ellipse);
                ellipse.SetValue(Canvas.LeftProperty, p.X - 1);
                ellipse.SetValue(Canvas.TopProperty, p.Y - 1);
            }
        }

        public void MouseUp(object sender, EventArgs e)
        {
            var triangle = (Triangle)sender;
            TriangleInfoView triangleInfoView = new TriangleInfoView();
            triangleInfoView.ElemID.Content = triangleInfoView.ElemID.Content
                + triangle.SegmentId.ToString();
            triangleInfoView.P1X.Content = triangle.Points[0].X;
            triangleInfoView.P1Y.Content = triangle.Points[0].Y;
            triangleInfoView.P2X.Content = triangle.Points[1].X;
            triangleInfoView.P2Y.Content = triangle.Points[1].Y;
            triangleInfoView.P3X.Content = triangle.Points[2].X;
            triangleInfoView.P3Y.Content = triangle.Points[2].Y;
            triangleInfoView.P1ID.Content = triangle.Points[0].ID;
            triangleInfoView.P2ID.Content = triangle.Points[1].ID;
            triangleInfoView.P3ID.Content = triangle.Points[2].ID;
            triangleInfoView.Show();
        }
        private void MouseLeaveAction(object sender, EventArgs e)
        {
            if (sender == null) { return; }
            var polygon = (Triangle)sender;
            polygon.Stroke = Brushes.Black;
            polygon.Fill = Brushes.White;
            Canvas parent = (Canvas)polygon.Parent;
            if (parent != null)
            {
                tempFigures.ForEach(p => parent.Children.Remove(p));

            }
        }
    }
}
