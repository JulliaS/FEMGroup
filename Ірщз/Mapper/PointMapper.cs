using TriangleNet.Geometry;
using Ірщз.model;
using Point = Ірщз.model.Point;

namespace Ірщз.Mapper
{
    class PointMapper
    {
        public static Point fromSystemPointToMyPoint(System.Windows.Point point)
        {
            return new Point(point.X, point.Y);
        }

        public static System.Windows.Point fromMyPointToSystemPoint(Point point)
        {
            return new System.Windows.Point(point.X, point.Y);
        }

        public static Point FromTriangleNetVartexToMyPoint(Vertex vertex)
        {
            return new Point(vertex.X, vertex.Y);
        }

        public static System.Windows.Point FromVertexToSystemPoint(Vertex vertex)
        {
            return new System.Windows.Point(vertex.X, vertex.Y);
        }
    }
}
