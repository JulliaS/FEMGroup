using System;
using System.Collections.Generic;
using System.Linq;
using Ірщз.model;

namespace Ірщз.Service
{
    class JarvisAlgorithm
    {
        public static List<Point> ConvexHull(List<Point> points)
        {
            if (points.Count < 3)
            {
                return points;
            }

            List<Point> hull = new List<Point>();

            Point vPointOnHull = points.Where(p => p.X == points.Min(min => min.X)).First();

            Point vEndpoint;
            do
            {
                hull.Add(vPointOnHull);
                vEndpoint = points[0];

                for (int i = 1; i < points.Count; i++)
                {
                    if ((vPointOnHull == vEndpoint)
                        || (Orientation(vPointOnHull, vEndpoint, points[i]) == -1))
                    {
                        vEndpoint = points[i];
                    }
                }

                vPointOnHull = vEndpoint;

            }
            while (vEndpoint != hull[0]);

            return hull;
        }

        private static int Orientation(Point p1, Point p2, Point p)
        {
            double Orin = (p2.X - p1.X) * (p.Y - p1.Y) - (p.X - p1.X) * (p2.Y - p1.Y);

            if (Orin > 0)
                return -1;
            if (Orin < 0)
                return 1;

            return 0;
        }
    }
}
