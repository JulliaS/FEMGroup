
namespace Ірщз.model
{
    public class Point
    {

        public Point()
        {
        }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Point(double x, double y, int id)
        {
            X = x;
            Y = y;
            ID = id;
        }

        public int ID { get; set; }

        public double X { get; set; }
        public double Y { get; set; }

        public static Point operator*(double number, Point point)
        {
            return new Point(point.X * number, point.Y * number);
        }
        public static Point operator +(Vector v, Point p)
        {
            return new Vector(v.X + p.X, v.Y + p.Y);
        }

        public override bool Equals(object obj)
        {
            var point = obj as Point;
            return point != null &&
                   X == point.X &&
                   Y == point.Y;
        }
    }
}
