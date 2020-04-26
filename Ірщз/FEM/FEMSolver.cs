using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using TriangleNet.Topology;
using Ірщз.FEM;

namespace Ірщз.FEMSolver
{

    public class FEMSolver
    {
        public FEMContext FEMContext { get; private set; }
        
        public FEMSolver(FEMContext femContext)
        {
            FEMContext = new FEMContext();
            
            FEMContext.Equation = femContext.Equation;
            FEMContext.Data = femContext.Data;
            FEMContext.Ke = Ke();
            FEMContext.Me = Me();
            FEMContext.Pe = Pe();
            FEMContext.Qe = Qe();
            FEMContext.Re = Re();
            FEMContext.X = SolutionSystem();

            femContext.Ke = FEMContext.Ke;
            femContext.Me = FEMContext.Me;
            femContext.Pe = FEMContext.Pe;
            femContext.Qe = FEMContext.Qe;
            femContext.Re = FEMContext.Re;
            femContext.X = FEMContext.X;
            femContext.A = FEMContext.A;
            femContext.B = FEMContext.B;
        }

        public Matrix[] Me()
        {
            Matrix[] Me = new Matrix[FEMContext.Data.NT.Length]; //create matrix for each finite element

            Matrix constMatrix = new Matrix(new double[3, 3] { { 2, 1, 1 }, { 1, 2, 1 }, { 1, 1, 2 } });
            for (int k = 0; k < Me.Length; k++)
            {
                double Square = FEMContext.Data.SquareTriangles[k];
                Me[k] = ((FEMContext.Equation.d * Square) / 12) * constMatrix;
            }

            return Me;
        }

        public Matrix[] Qe()
        {
            int N = FEMContext.Data.NT.Length; //count finite element

            Matrix[] Qe = new Matrix[N]; //create matrix for each finite element

            Matrix constMatrix = new Matrix(new double[3, 3] { { 2, 1, 1 }, { 1, 2, 1 }, { 1, 1, 2 } });
            for (int k = 0; k < N; k++)
            {
                Vertex i = FEMContext.Data.CT[FEMContext.Data.NT[k].GetVertexID(0)];
                Vertex j = FEMContext.Data.CT[FEMContext.Data.NT[k].GetVertexID(1)];
                Vertex m = FEMContext.Data.CT[FEMContext.Data.NT[k].GetVertexID(2)];

                double Square = FEMContext.Data.SquareTriangles[k];
                Matrix Fe = new Matrix(new double[3, 1] { { FEMContext.Equation.function }, { FEMContext.Equation.function }, { FEMContext.Equation.function } });
                Qe[k] = (Square / 12) * constMatrix * Fe;
                //MessageBox.Show(_Qe[k].ToString());
            }
            return Qe;
        }

        public Matrix[] Ke()
        {
            int N = FEMContext.Data.NT.Length; //count finite element

            Matrix[] Ke = new Matrix[N]; //create matrix for each finite element

            for (int k = 0; k < N; k++)
            {
                Vertex i = FEMContext.Data.CT[FEMContext.Data.NT[k].GetVertexID(0)];
                Vertex j = FEMContext.Data.CT[FEMContext.Data.NT[k].GetVertexID(1)];
                Vertex m = FEMContext.Data.CT[FEMContext.Data.NT[k].GetVertexID(2)];

                Matrix b = new Matrix(new double[1, 3] { { j.Y - m.Y, m.Y - i.Y, i.Y - j.Y } });

                Matrix c = new Matrix(new double[1, 3] { { m.X - j.X, i.X - m.X, j.X - i.X } });

                double Se = FEMContext.Data.SquareTriangles[k]; ;

                Matrix Fe = new Matrix(new double[3, 1] { { FEMContext.Equation.function }, { FEMContext.Equation.function }, { FEMContext.Equation.function } });
                Ke[k] = 1.0 / (4.0 * Se) * (FEMContext.Equation.a11 * b.Transpose() * b + FEMContext.Equation.a22 * c.Transpose() * c);
                //MessageBox.Show(_Ke[k].ToString());
            }
            return Ke;
        }

        public Matrix[] Re()
        {
            int N = FEMContext.Data.GT.GetLength(0);

            List<Matrix> Re = new List<Matrix>();

            Matrix constMatrix = new Matrix(new double[2, 2] { { 1.0 / 3.0, 1.0 / 6.0 }, { 1.0 / 6.0, 1.0 / 3.0 } });

            for (int k = 0; k < N; k++)
            {
                for (int s = 0; s < FEMContext.Data.GT[k].GetLength(0); s++)
                {
                    double sigma = FEMContext.Equation.Conditions[k].sigma;
                    double beta = FEMContext.Equation.Conditions[k].beta;
                    Re.Add((sigma / beta) * constMatrix * FEMContext.Data.LengthSegments[k][s]);
                }
            }
            return Re.ToArray();
        }

        public Matrix[] Pe()
        {
            int N = FEMContext.Data.GT.GetLength(0);

            List<Matrix> Pe = new List<Matrix>();

            Matrix constMatrix = new Matrix(new double[2, 1] { { 1.0 / 2.0 }, { 1.0 / 2.0 } });

            for (int k = 0; k < N; k++)
            {
                for (int s = 0; s < FEMContext.Data.GT[k].GetLength(0); s++)
                {
                    double sigma = FEMContext.Equation.Conditions[k].sigma;
                    double beta = FEMContext.Equation.Conditions[k].beta;
                    double uc = FEMContext.Equation.Conditions[k].uc;
                    Pe.Add((uc * sigma / beta) * constMatrix * FEMContext.Data.LengthSegments[k][s]);
                }
            }
            return Pe.ToArray();
        }

        private Matrix SolutionSystem()
        {
            int N = FEMContext.Data.CT.Length; //count finite element

            FEMContext.A = new Matrix(N);
            FEMContext.B = new Matrix(N, 1);

            int e = FEMContext.Data.NT.GetLength(0);
            for (int k = 0; k < e; k++)
            {
                for (int i = 0; i <= 2; i++)
                {
                    for (int j = 0; j <= 2; j++)
                    {
                        FEMContext.A[FEMContext.Data.NT[k].GetVertexID(i), FEMContext.Data.NT[k].GetVertexID(j)] += FEMContext.Ke[k][i, j] + FEMContext.Me[k][i, j];
                    }
                    FEMContext.B[FEMContext.Data.NT[k].GetVertexID(i)] += FEMContext.Qe[k][i, 0];
                }
            }

            N = FEMContext.Data.GT.GetLength(0);

            e = 0;
            for (int k = 0; k < N; k++)
            {
                for (int s = 0; s < FEMContext.Data.GT[k].GetLength(0); s++)
                {
                    for (int i = 0; i <= 1; i++)
                    {
                        for (int j = 0; j <= 1; j++)
                        {
                            FEMContext.A[FEMContext.Data.GT[k][s, i], FEMContext.Data.GT[k][s, j]] += FEMContext.Re[e][i, j];
                        }
                        FEMContext.B[FEMContext.Data.GT[k][s, i], 0] += FEMContext.Pe[e][i, 0];
                    }
                    e++;
                }
            }
            return new Matrix(DenseMatrix.OfArray(FEMContext.A.GetValues())
                .Solve(DenseVector.OfArray(FEMContext.B.GetColumn(0))).ToArray(), 1);
        }

    }
}
