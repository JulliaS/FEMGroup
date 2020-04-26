using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ірщз.FEM
{
    class ErrorContext
    {
        public int TrianglesNumber { get; set; }
        public double ErrorL2 { get; set; }
        public double ErrorW2 { get; set; }
        public double pL2 { get; set; }
        public double pW2 { get; set; }
        public double NormL2 { get; set; }
        public double NormW2 { get; set; }
        public double pAitkenL2 { get; set; }
        public double pAitkenW2 { get; set; }
    }
}
