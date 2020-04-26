using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ірщз.FEMSolver;

namespace Ірщз.FEM
{
    public class FEMContext
    {
        public Equation Equation;
        public Data Data;
        public Matrix[] Me { get; set; }
        public Matrix[] Qe { get; set; }
        public Matrix[] Ke { get; set; }
        public Matrix[] Pe { get; set; }
        public Matrix[] Re { get; set; }
        public Matrix A { get; set; }
        public Matrix B { get; set; }
        public Matrix X { get; set; }
    }
}
