using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriangleNet.Topology;

namespace Ірщз.FEMSolver
{
    public class Condition
    {
        public double beta { get; }
        public double sigma { get; }
        public double uc { get; }
        public int SegmentNumber { get; set; }
        public List<SubSegment> segments { get; set; }

        public Condition(double beta, double sigma, double uc)
        {
            this.beta = beta;
            this.sigma = sigma;
            this.uc = uc;
        }

        public Condition(double beta, double sigma, double uc, int segmentNumber, List<SubSegment> segments)
        {
            this.beta = beta;
            this.sigma = sigma;
            this.uc = uc;
            this.SegmentNumber = SegmentNumber;
            this.segments = segments;
        }

    }
}
