using Ірщз.model;

namespace Ірщз
{
    class NormalizationUtils
    {
       public static Vector GetBisectorVectorCoordinates(Point point1, Point point2, Point point3)
        {
            Vector vector1 = new Vector(point2.X - point1.X, point2.Y - point1.Y);
            Vector vector2 = new Vector(point3.X - point2.X, point3.Y - point2.Y);
             
            return vector2.getModule() * vector1 + vector1.getModule() * vector2;
        }

    }
}
