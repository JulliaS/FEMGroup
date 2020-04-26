using System;

namespace Ірщз.model
{
    public class Vector : Point
    {
        public Vector(double x, double y) : base(x, y)
        {
        }
        public Vector(Point p1, Point p2)
        {
            this.X = p2.X - p1.X;
            this.Y = p2.Y - p1.Y;
        }

        public double getModule()
        {
            return Math.Sqrt(Math.Pow(this.X, 2) + Math.Pow(this.Y, 2));
        }
        public Vector Normalize()
        {
            double module = this.getModule();
            this.X = this.X / module;
            this.Y = this.Y / module;
            return this;
        }

        public static Vector operator *(double number, Vector point)
        {
            return new Vector(point.X * number, point.Y * number);
        }
        public static Vector operator +(Vector v1, Vector v2)
        {
            return new Vector(v1.X + v2.X, v1.Y + v2.Y);
        }
        public static Vector operator /(Vector v1, double divider)
        {
            return new Vector(v1.X / divider, v1.Y / divider);
        }
        public Vector FindPerpedicularVector()
        {
            return new Vector(-this.Y, this.X);
        }
        
    }
}
