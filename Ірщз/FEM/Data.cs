using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriangleNet.Geometry;
using TriangleNet.Topology;

namespace Ірщз.FEMSolver
{
    public class Data
    {

        public Vertex[] CT { get; } //points

        public Triangle[] NT { get; } //triangle

        public int[][,] GT { get; } //segments

        public double[] SquareTriangles { get; }

        public double[][] LengthSegments { get; }

        public Data(Vertex[] points, Triangle[] triangles, int[][,] segments, double[] squareTriangles, double[][] lengthSegments)
        {
            this.CT = points;
            this.NT = triangles;
            this.GT = segments;
            this.SquareTriangles = squareTriangles;
            this.LengthSegments = lengthSegments;
        }
    }
}
