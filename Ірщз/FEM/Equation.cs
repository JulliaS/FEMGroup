using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ірщз.FEMSolver
{
    public class Equation
    {
        public double a11 { get; }
        public double a22 { get; }
        public double d { get; }
        public double function { get; }
        public Condition[] Conditions { get; }

        public Equation(double a11, double a22, double d, double function, Condition[] Conditions)
        {
            this.a11 = a11;
            this.a22 = a22;
            this.d = d;
            this.function = function;
            this.Conditions = Conditions;
        }

    }
}
