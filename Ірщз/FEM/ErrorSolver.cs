using System;
using System.Collections.Generic;
using System.Windows;
using Ірщз.FEMSolver;
using Ірщз.Service.Impl.CoR;
using Point = TriangleNet.Geometry.Point;
using CustomPoint = Ірщз.model.Point;
using System.Windows.Controls;
using TriangleNet.Geometry;
using TriangleNet.Topology;

namespace Ірщз.FEM
{
    class ErrorSolver
    {
        public delegate double Function(double x);

        private FEMContext CurrentFEMContext;

        private TriangleNetHandler TriangleNetHandler;

        private List<CustomPoint> Points;

        private Equation Equation;

        private double[] AreaTriangles;

        private double[][] LengthSegments;

        public ErrorSolver(FEMContext fEMContext, TriangleNetHandler triangleNetHandler, List<CustomPoint> points, Equation equation)
        {
            Points = points;
            Equation = equation;
            TriangleNetHandler = triangleNetHandler;
            CurrentFEMContext = fEMContext;
            CalculateAreas(CurrentFEMContext);
            CalculateLength(CurrentFEMContext);
        }

        public double PL2()
        {
            var currentMaxSquare = TriangleNetHandler.MaxSquare;
            TriangleNetHandler.MaxSquare = currentMaxSquare / 2;

            var dataInitializer = new DataInitializer(TriangleNetHandler, Points, Equation);

            var newFemContext = dataInitializer.GetInitialData();
            CalculateAreas(newFemContext);
            CalculateLength(newFemContext);
            var fem = new FEMSolver.FEMSolver(newFemContext);

           var pL2 = Math.Abs(Math.Log(NormaErrorL2(Functions.U, CurrentFEMContext)) - Math.Log(NormaErrorL2(Functions.U, newFemContext))) / Math.Log(2);

            TriangleNetHandler.MaxSquare = currentMaxSquare;

            return pL2;
        }

        public double PW2()
        {
            var currentMaxSquare = TriangleNetHandler.MaxSquare;
            TriangleNetHandler.MaxSquare = currentMaxSquare / 2;

            var dataInitializer = new DataInitializer(TriangleNetHandler, Points, Equation);

            var newFemContext = dataInitializer.GetInitialData();
            CalculateAreas(newFemContext);
            CalculateLength(newFemContext);
            var fem = new FEMSolver.FEMSolver(newFemContext);

            var pW2 = Math.Abs(Math.Log(NormaErrorW2(Functions.U, Functions.DU, CurrentFEMContext)) - 
                Math.Log(NormaErrorW2(Functions.U, Functions.DU, newFemContext))) / Math.Log(2);

            TriangleNetHandler.MaxSquare = currentMaxSquare;

            return pW2;
        }

        public double PAitkenL2()
        {
            var currentMaxSquare = TriangleNetHandler.MaxSquare;

            TriangleNetHandler.MaxSquare = currentMaxSquare / 2;

            var dataInitializer = new DataInitializer(TriangleNetHandler, Points, Equation);

            var firstNewFemContext = dataInitializer.GetInitialData();
            CalculateAreas(firstNewFemContext);
            CalculateLength(firstNewFemContext);
            var fem = new FEMSolver.FEMSolver(firstNewFemContext);

            TriangleNetHandler.MaxSquare = currentMaxSquare / 4;

            dataInitializer = new DataInitializer(TriangleNetHandler, Points, Equation);

            var secondNewFemContext = dataInitializer.GetInitialData();
            CalculateAreas(secondNewFemContext);
            CalculateLength(secondNewFemContext);
            fem = new FEMSolver.FEMSolver(secondNewFemContext);

            var pL2 = Math.Abs(Math.Log(Math.Abs(NormaL2(firstNewFemContext) - NormaL2(CurrentFEMContext))) -
                    Math.Log(Math.Abs(NormaL2(secondNewFemContext) - NormaL2(firstNewFemContext)))) / Math.Log(2);

            TriangleNetHandler.MaxSquare = currentMaxSquare;

            return pL2;
        }

        public double PAitkenW2()
        {
            var currentMaxSquare = TriangleNetHandler.MaxSquare;
            TriangleNetHandler.MaxSquare = currentMaxSquare / 2;

            var dataInitializer = new DataInitializer(TriangleNetHandler, Points, Equation);

            var firstNewFemContext = dataInitializer.GetInitialData();
            CalculateAreas(firstNewFemContext);
            CalculateLength(firstNewFemContext);
            var fem = new FEMSolver.FEMSolver(firstNewFemContext);

            TriangleNetHandler.MaxSquare = currentMaxSquare / 4;

            dataInitializer = new DataInitializer(TriangleNetHandler, Points, Equation);

            var secondNewFemContext = dataInitializer.GetInitialData();
            CalculateAreas(secondNewFemContext);
            CalculateLength(secondNewFemContext);
            fem = new FEMSolver.FEMSolver(secondNewFemContext);

            var pW2 = Math.Abs(Math.Log(Math.Abs(NormaW2(firstNewFemContext) - NormaW2(CurrentFEMContext))) -
                    Math.Log(Math.Abs(NormaW2(secondNewFemContext) - NormaW2(firstNewFemContext)))) / Math.Log(2);

            TriangleNetHandler.MaxSquare = currentMaxSquare;

            return pW2;
        }

        public double NormaL2(FEMContext FEMContext)
        {
            double norma = 0;
            for (int e = 0; e < FEMContext.Data.NT.GetLength(0); e++)
            {
                Point i = FEMContext.Data.CT[FEMContext.Data.NT[e].GetVertexID(0)];
                Point j = FEMContext.Data.CT[FEMContext.Data.NT[e].GetVertexID(1)];
                Point m = FEMContext.Data.CT[FEMContext.Data.NT[e].GetVertexID(2)];
                Point point = new Point((i.X + j.X + m.X) / 3, (i.Y + j.Y + m.Y) / 3);
                if (TriangleOnPoint(point, i, j, m))
                {
                    double fi = CalculateArea(point, j, m) * FEMContext.X[FEMContext.Data.NT[e].GetVertexID(0)];
                    double fj = CalculateArea(point, m, i) * FEMContext.X[FEMContext.Data.NT[e].GetVertexID(1)];
                    double fm = CalculateArea(point, i, j) * FEMContext.X[FEMContext.Data.NT[e].GetVertexID(2)];
                    norma += Math.Pow((fi + fj + fm) / AreaTriangles[e], 2) * AreaTriangles[e];
                }
                else
                {
                    MessageBox.Show("Exception - (NormaL2)");
                }
            }
            return Math.Sqrt(norma);
        }

        public double NormaW2(FEMContext FEMContext)
        {
            double norma = 0;
            for (int e = 0; e < FEMContext.Data.NT.GetLength(0); e++)
            {
                Point i = FEMContext.Data.CT[FEMContext.Data.NT[e].GetVertexID(0)];
                Point j = FEMContext.Data.CT[FEMContext.Data.NT[e].GetVertexID(1)];
                Point m = FEMContext.Data.CT[FEMContext.Data.NT[e].GetVertexID(2)];
                Point point = new Point((i.X + j.X + m.X) / 3, (i.Y + j.Y + m.Y) / 3);
                if (TriangleOnPoint(point, i, j, m))
                {
                    double fi = CalculateArea(point, j, m) * FEMContext.X[FEMContext.Data.NT[e].GetVertexID(0)];
                    double fj = CalculateArea(i, point, m) * FEMContext.X[FEMContext.Data.NT[e].GetVertexID(1)];
                    double fm = CalculateArea(i, j, point) * FEMContext.X[FEMContext.Data.NT[e].GetVertexID(2)];
                    double Uh = (fi + fj + fm) / AreaTriangles[e];
                    double dUh = UhDerivative(e, FEMContext);

                    norma += (Math.Abs(Math.Pow(Uh, 2)) + Math.Abs(Math.Pow(dUh, 2))) * AreaTriangles[e];
                }
                else
                {
                    MessageBox.Show("Exception - (NormaW2)");
                }
            }
            return Math.Sqrt(norma);
        }

        public double NormaL2Exact(Function f, FEMContext FEMContext)
        {
            double norma = 0;
            for (int e = 0; e < FEMContext.Data.NT.GetLength(0); e++)
            {
                Point i = FEMContext.Data.CT[FEMContext.Data.NT[e].GetVertexID(0)];
                Point j = FEMContext.Data.CT[FEMContext.Data.NT[e].GetVertexID(1)];
                Point m = FEMContext.Data.CT[FEMContext.Data.NT[e].GetVertexID(2)];
                Point point = new Point((i.X + j.X + m.X) / 3, (i.Y + j.Y + m.Y) / 3);
                if (TriangleOnPoint(point, i, j, m))
                {
                    norma += (Math.Pow(f(point.X), 2)) * AreaTriangles[e];
                }
                else
                {
                    MessageBox.Show("Exception - (NormaL2Exact)");
                }
            }
            return Math.Sqrt(norma);
        }

        public double NormaW2Exact(Function u, Function du, FEMContext FEMContext)
        {
            double norma = 0;
            for (int e = 0; e < FEMContext.Data.NT.GetLength(0); e++)
            {
                Point i = FEMContext.Data.CT[FEMContext.Data.NT[e].GetVertexID(0)];
                Point j = FEMContext.Data.CT[FEMContext.Data.NT[e].GetVertexID(1)];
                Point m = FEMContext.Data.CT[FEMContext.Data.NT[e].GetVertexID(2)];
                Point point = new Point((i.X + j.X + m.X) / 3, (i.Y + j.Y + m.Y) / 3);
                if (TriangleOnPoint(point, i, j, m))
                {
                    norma += (Math.Pow(u(point.X), 2) + Math.Pow(du(point.X), 2)) * AreaTriangles[e];
                }
                else
                {
                    MessageBox.Show("Exception - (NormaW2_Exact)");
                }
            }
            return Math.Sqrt(norma);
        }

        public double NormaErrorL2(Function u, FEMContext FEMContext)
        {
            double norma = 0;
            for (int e = 0; e < FEMContext.Data.NT.GetLength(0); e++)
            {
                Point i = FEMContext.Data.CT[FEMContext.Data.NT[e].GetVertexID(0)];
                Point j = FEMContext.Data.CT[FEMContext.Data.NT[e].GetVertexID(1)];
                Point m = FEMContext.Data.CT[FEMContext.Data.NT[e].GetVertexID(2)];
                Point point = new Point((i.X + j.X + m.X) / 3, (i.Y + j.Y + m.Y) / 3);
                if (TriangleOnPoint(point, i, j, m))
                {
                    double fi = CalculateArea(point, j, m) * FEMContext.X[FEMContext.Data.NT[e].GetVertexID(0)];
                    double fj = CalculateArea(i, point, m) * FEMContext.X[FEMContext.Data.NT[e].GetVertexID(1)];
                    double fm = CalculateArea(i, j, point) * FEMContext.X[FEMContext.Data.NT[e].GetVertexID(2)];
                    var funcValue = u(point.X);
                    norma += Math.Abs((Math.Pow(u(point.X) - (fi + fj + fm) / AreaTriangles[e], 2))) * AreaTriangles[e];
                }
                else
                {
                    MessageBox.Show("Exception - (NormaErrorL2)");
                }
            }
            return norma;
        }

        public double NormaErrorW2(Function u, Function du, FEMContext FEMContext)
        {
            double norma = 0;
            for (int e = 0; e < FEMContext.Data.NT.GetLength(0); e++)
            {
                Point i = FEMContext.Data.CT[FEMContext.Data.NT[e].GetVertexID(0)];
                Point j = FEMContext.Data.CT[FEMContext.Data.NT[e].GetVertexID(1)];
                Point m = FEMContext.Data.CT[FEMContext.Data.NT[e].GetVertexID(2)];
                Point point = new Point((i.X + j.X + m.X) / 3, (i.Y + j.Y + m.Y) / 3);
                if (TriangleOnPoint(point, i, j, m))
                {
                    double fi = CalculateArea(point, j, m) * FEMContext.X[FEMContext.Data.NT[e].GetVertexID(0)];
                    double fj = CalculateArea(i, point, m) * FEMContext.X[FEMContext.Data.NT[e].GetVertexID(1)];
                    double fm = CalculateArea(i, j, point) * FEMContext.X[FEMContext.Data.NT[e].GetVertexID(2)];
                    double Uh = (fi + fj + fm) / AreaTriangles[e];
                    double dUh = UhDerivative(e, FEMContext);

                    norma += (Math.Abs(Math.Pow(Uh - u(point.X), 2)) + Math.Abs(Math.Pow(dUh - du(point.X), 2))) * AreaTriangles[e];
                }
                else
                {
                    MessageBox.Show("Exception - (NormaErrorW2)");
                }
            }
            return norma;
        }

        private double Length(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        private double CalculateArea(Point i, Point j, Point m)
        {
            var matrix = new Matrix(new double[3, 3] { { 1, i.X, i.Y }, { 1, j.X, j.Y }, { 1, m.X, m.Y } });
            var result = matrix.Determinant3x3();
            return result / 2;
        }

        private void CalculateAreas(FEMContext FEMContext)
        {
            this.AreaTriangles = new double[FEMContext.Data.NT.GetLength(0)];
            for (int k = 0; k < FEMContext.Data.NT.GetLength(0); k++)
            {
                Point i = FEMContext.Data.CT[FEMContext.Data.NT[k].GetVertexID(0)];
                Point j = FEMContext.Data.CT[FEMContext.Data.NT[k].GetVertexID(1)];
                Point m = FEMContext.Data.CT[FEMContext.Data.NT[k].GetVertexID(2)];
                AreaTriangles[k] = CalculateArea(i, j, m);
            }
        }

        private void CalculateLength(FEMContext FEMContext)
        {
            this.LengthSegments = new double[FEMContext.Data.GT.Length][];
            for (int border = 0; border < FEMContext.Data.GT.Length; border++)
            {
                LengthSegments[border] = new double[FEMContext.Data.GT[border].GetLength(0)];
                for (int k = 0; k < FEMContext.Data.GT[border].GetLength(0); k++)
                {
                    Point p1 = FEMContext.Data.CT[FEMContext.Data.GT[border][k, 0]];
                    Point p2 = FEMContext.Data.CT[FEMContext.Data.GT[border][k, 1]];
                    LengthSegments[border][k] = Length(p1, p2);
                }
            }
        }

        private double UhDerivative(int numberTriangle, FEMContext FEMContext)
        {
            double UhDer;
            Point i = FEMContext.Data.CT[FEMContext.Data.NT[numberTriangle].GetVertexID(0)];
            Point j = FEMContext.Data.CT[FEMContext.Data.NT[numberTriangle].GetVertexID(1)];
            Point m = FEMContext.Data.CT[FEMContext.Data.NT[numberTriangle].GetVertexID(2)];
            double bI = j.Y - m.Y;
            double bJ = m.Y - i.Y;
            double bM = i.Y - j.Y;

            double Ui = FEMContext.X[FEMContext.Data.NT[numberTriangle].GetVertexID(0)];
            double Uj = FEMContext.X[FEMContext.Data.NT[numberTriangle].GetVertexID(1)];
            double Um = FEMContext.X[FEMContext.Data.NT[numberTriangle].GetVertexID(2)];
            double sigma = 2 * AreaTriangles[numberTriangle];


            UhDer = (Ui * bI + Uj * bJ + Um * bM) / sigma;

            return UhDer;
        }

        private bool TriangleOnPoint(Point point, Point i, Point j, Point m)
        {
            double a = Math.Round((i.X - point.X) * (j.Y - i.Y) - (j.X - i.X) * (i.Y - point.Y), 13);

            double b = Math.Round((j.X - point.X) * (m.Y - j.Y) - (m.X - j.X) * (j.Y - point.Y), 13);

            double c = Math.Round((m.X - point.X) * (i.Y - m.Y) - (i.X - m.X) * (m.Y - point.Y), 13);

            if ((a >= 0 && b >= 0 && c >= 0) || (a <= 0 && b <= 0 && c <= 0))
            {
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}
