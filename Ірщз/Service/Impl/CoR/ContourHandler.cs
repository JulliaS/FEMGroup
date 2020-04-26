using BezierCurveSample.View.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Ірщз.Mapper;
using Ірщз.model;
using Ірщз.Service.Abstraction;

namespace Ірщз.Service.Impl.CoR
{
    public class ContourHandler : PointHandler
    {
        public ContourHandler(PointHandler nextHandler) : base(nextHandler) { }
        public ContourHandler() { }
        protected override void ProcessContext(Canvas canvas, List<Point> points)
        {
            DrawMainPolygon(canvas, points);
        }

        private void DrawMainPolygon(Canvas canvas, List<Point> points)
        {
            if (points.Count < 3) { return; }
            var path = new Path();
            var pathSegmentCollection = new PathSegmentCollection();
            var pathFigure = new PathFigure();
            List<Point> processedPoints = JarvisAlgorithm.ConvexHull(points);
            for (int i = 0; i < processedPoints.Count; i++)
            {
                processedPoints[i] = new Point(processedPoints[i].X + 100, processedPoints[i].Y + 100);
            }
            pathFigure.StartPoint = PointMapper.fromMyPointToSystemPoint(processedPoints.First());

            var beizerSegments = InterpolationUtils.InterpolatePointWithBeizerCurves(processedPoints, true);

            foreach (var beizerCurveSegment in beizerSegments)
            {
                var segment = new BezierSegment
                {
                    Point1 = PointMapper.fromMyPointToSystemPoint(beizerCurveSegment.FirstControlPoint),
                    Point2 = PointMapper.fromMyPointToSystemPoint(beizerCurveSegment.SecondControlPoint),
                    Point3 = PointMapper.fromMyPointToSystemPoint(beizerCurveSegment.EndPoint)
                };
                pathSegmentCollection.Add(segment);
            }

            pathFigure.Segments = pathSegmentCollection;
            var pathFigureCollection = new PathFigureCollection { pathFigure };

            var pathGeometry = new PathGeometry { Figures = pathFigureCollection };

            path.Data = pathGeometry;
            path.Stroke = Brushes.Black;
            path.StrokeThickness = 1;
            canvas.Children.Add(path);
        }
    }
}
