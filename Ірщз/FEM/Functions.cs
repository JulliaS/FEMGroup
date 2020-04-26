using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ірщз.FEMSolver
{
    class Functions
    {
        //public static double U(double x)
        //{
        //    double a = 0;
        //    double b = 1;
        //    var c2 = 1 / (Math.Exp(b / Math.Sqrt(8)) - Math.Exp(a / Math.Sqrt(2)));
        //    var c1 = -c2 * (Math.Exp(a / Math.Sqrt(8)) / Math.Exp(-a / Math.Sqrt(8)));
        //    // return c1 * Math.Exp(-x / Math.Sqrt(8)) + c2 * Math.Exp(x / Math.Sqrt(8));

        //    return (Math.Exp(-(x - 1) / (2 * Math.Sqrt(2))) * (Math.Exp(x / Math.Sqrt(2)) - 1)) / (Math.Exp(1 / Math.Sqrt(2)) - 1);
        //}

        //public static double DU(double x)
        //{
        //    double a = 0;
        //    double b = 1;
        //    var c2 = 1 / (Math.Exp(b / Math.Sqrt(8)) - Math.Exp(a / Math.Sqrt(2)));
        //    var c1 = -c2 * (Math.Exp(a / Math.Sqrt(8)) / Math.Exp(-a / Math.Sqrt(8)));
        //    // return (-1 / Math.Sqrt(8)) * c1 * Math.Exp(-x / Math.Sqrt(8)) + (1 / Math.Sqrt(8)) * c2 * Math.Exp(x / Math.Sqrt(8));

        //    return Math.Exp((1 - x) / (2 * Math.Sqrt(2)) + x / Math.Sqrt(2)) / (Math.Sqrt(2) * (Math.Exp(1 / Math.Sqrt(2)) - 1)) - (Math.Exp((1 - x) / (2 * Math.Sqrt(2))) * (Math.Exp(x / Math.Sqrt(2)) - 1)) / (2 * Math.Sqrt(2) * (Math.Exp(1 / Math.Sqrt(2)) - 1));
        //}

        public static double U(double x)
        {
            double a = 0;
            double b = 1;

            double firstPart = x / (b - a);
            double secondPart = a / (b - a);
            // return firstPart - secondPart;

            //Dina
            return (1 / 6) * (a + b) * x - (1 / 6) * b * a - (x * x) / 6;
            //Vika
            //return (3 * (b + a) * x) / 10 + (3 * a * a) / 10 - (3 * a * (b + a)) / 10 - (3 * x * x) / 10;
        }

        public static double DU(double x)
        {
            double a = 0;
            double b = 1;

            // return 1 / (b - a);

            //Dina
            return (1 / 6) * (a + b) - x / 3;

            //return (3 * (b + a)) / 10 - (6 * x) / 10; //viky
        }


    }
}
