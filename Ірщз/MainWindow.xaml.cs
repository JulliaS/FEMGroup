using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Point = Ірщз.model.Point;
using System.Windows.Input;
using Ірщз.Service;
using System.IO;
using Ірщз.Service.Abstraction;
using Ірщз.Service.Impl.CoR;
using TriangleNet.Meshing;
using TriangleNet.Geometry;
using TriangleNet.Topology;
using System.Windows.Controls;
using System.Windows.Data;
using Ірщз.FEMSolver;
using Condition = Ірщз.FEMSolver.Condition;
using Ірщз.FEM;

namespace Ірщз
{
    public partial class MainWindow : Window
    {
        private PointHandler _pointHandler;
        private List<Point> points;
        private TriangleNetHandler TriangleNetHandler;
        private List<int> Coefs = new List<int>();
        private List<ErrorContext> errorContextForGrid = new List<ErrorContext>();
        private Equation equation;
        private FEMSolver.FEMSolver fem;
        private String pathToCoefs;

        private Data data;

        public MainWindow()
        {
            InitializeComponent();
            TriangleNetHandler = new TriangleNetHandler();
            points = new List<Point>();
            _pointHandler = TriangleNetHandler;
        }


        private void DrawFigure(List<Point> points)
        {
            MainCanvas.Children.Clear();
            TriangleNetHandler.MaxSquare = Convert.ToDouble(MaxSquare.Text);
            TriangleNetHandler.MinAngel = Convert.ToInt32(MinAngel.Text);
            _pointHandler.Process(MainCanvas, points);

            
            var mesh = TriangleNetHandler.mesh;
            ReadCoefFromFile(pathToCoefs, GetBoundarySegments(mesh));

            FormationMatrix(TriangleNetHandler, points, MainCanvas, equation);

            FillData(mesh);
            // FormationMatrix(mesh);
            
        }

        private void FormationMatrix(TriangleNetHandler triangleNetHandler, List<Point> points, Canvas canvas, Equation equation)
        {
            ///////////FEM
            
            var dataInitializer = new DataInitializer(TriangleNetHandler, points, equation);

            var femContext = dataInitializer.GetInitialData();

            fem = new FEMSolver.FEMSolver(femContext);

            var errorSolver = new ErrorSolver(femContext, TriangleNetHandler, points, equation);

            var errorsContext = new ErrorContext
            {
                ErrorL2 = errorSolver.NormaErrorL2(Functions.U, femContext),
                ErrorW2 = errorSolver.NormaErrorW2(Functions.U, Functions.DU, femContext),
                NormL2 = errorSolver.NormaL2(femContext),
                NormW2 = errorSolver.NormaW2(femContext),
                pL2 = errorSolver.PL2(),
                pW2 = errorSolver.PW2(),
                pAitkenL2 = errorSolver.PAitkenL2(),
                pAitkenW2 = errorSolver.PAitkenW2(),
                TrianglesNumber = femContext.Data.NT.Length
            };

            errorContextForGrid.Add(errorsContext);

            ErrorGrid.ItemsSource = from v in errorContextForGrid
                                    select
            new
            {
                v.TrianglesNumber,
                v.NormL2,
                v.NormW2,
                v.ErrorL2,
                v.ErrorW2,
                v.pL2,
                v.pW2,
                v.pAitkenL2,
                v.pAitkenW2
            };

            // DrawInMatlab(x1, x2, z);

        }

        private Matrix AddValuesToFilesAndGetResult(FEMSolver.FEMSolver fem, double[] squareTriangles, double[][] lengthSegments, Triangle[] triangles, int[][,] segments)
        {
            String InfoMatrices = MatricesAreaToString(fem.Me(), "M", squareTriangles, triangles);
            InfoMatrices += MatricesAreaToString(fem.Qe(), "Q", squareTriangles, triangles);
            InfoMatrices += MatricesAreaToString(fem.Ke(), "K", squareTriangles, triangles);
            InfoMatrices += MatricesBoundaryToString(fem.Re(), "R", lengthSegments, segments);
            InfoMatrices += MatricesBoundaryToString(fem.Pe(), "P", lengthSegments, segments);

            var A = fem.FEMContext.A;
            var b = fem.FEMContext.B;

            var result = fem.FEMContext.X;

            InfoMatrices += MatrixToString(A, "Matrix A");
            InfoMatrices += MatrixToString(b, "Vector B");

            

            InfoMatrices += MatrixToString(result, "Vector U");

            File.WriteAllText("Result.txt", InfoMatrices);

            return result;
        }

        private string MatricesAreaToString(Matrix[] matrices, string title, double[] squareTriangles, Triangle[] data)
        {
            string info = "\nMatrix " + title + "e:\n";
            for (int i = 0; i < matrices.Length; i++)
            {
                info += title + i + "-triangle" + i + ";area=" + squareTriangles[i] + ":\n";
                info += matrices[i].ToString();
            }
            return info;
        }

        private string MatrixToString(Matrix matrix, string title)
        {
            string info = title + "\n";
            info += matrix.ToString();

            return info;
        }


        //private void DrawInMatlab(Array x1, Array x2, Array result)
        //{
        //    object output = null;

        //    MLApp.MLApp matlab = new MLApp.MLApp();
        //    matlab.Execute(@"cd C:\Users\Yulia\Desktop\love\draw.m");
        //    matlab.Feval("draw", 0, out output, x1, x2, result);
        //}

        private string MatricesBoundaryToString(Matrix[] matrices, string title, double[][] lengthSegments, int[][,] segments)
        {
            string info = "Matrix " + title + "e:\n";
            int k = 0;
            for (int i = 0; i < segments.GetLength(0); i++)
            {
                for (int s = 0; s < segments[i].GetLength(0); s++)
                {
                    info += title + k + "{" + segments[i][s, 0] + "," + segments[i][s, 1] + "} - size = " + lengthSegments[i][s] + ":\n";
                    info += matrices[k].ToString();
                    k++;
                }
            }
            return info;
        }

        private void FillData(IMesh mesh)
        {
            PointGrid.ItemsSource = from v in mesh.Vertices
                                    select
            new
            {
                ID = v.ID,
                v.X,
                v.Y
            };

            TriangleGrid.ItemsSource = from t in mesh.Triangles
                                       select
              new
              {
                  TriangleID = t.ID,
                  P1 = FormatPointInfo(t.GetVertex(0)),
                  P2 = FormatPointInfo(t.GetVertex(1)),
                  P3 = FormatPointInfo(t.GetVertex(2)),
                  Square = CalculateSquareOfTriangle(t)
              };

            SortedDictionary<int, List<int[]>> pairs = GetSegments(mesh);

            foreach (var key in pairs.Keys)
            {
                for (int j = 0; j < pairs[key].Count; j++)
                {
                    EdgeGrid.Items.Add(new { Number = key, ID_1 = pairs[key][j][0], ID_2 = pairs[key][j][1] });
                }

            }
            EdgeGrid.Columns.Add(new DataGridTextColumn { Header = "Number", Binding = new Binding("Number") });
            EdgeGrid.Columns.Add(new DataGridTextColumn { Header = "ID_1", Binding = new Binding("ID_1") });
            EdgeGrid.Columns.Add(new DataGridTextColumn { Header = "ID_2", Binding = new Binding("ID_2") });

            MaxSquareTriangleInfo.Content = FindTriangleWithMaxSquare(mesh.Triangles);
        }

        private SortedDictionary<int, List<int[]>> GetSegments(IMesh mesh)
        {
            SortedDictionary<int, List<int[]>> segments = new SortedDictionary<int, List<int[]>>();
            foreach (SubSegment segment in mesh.Segments)
            {
                int key;
                if (Math.Min(segment.GetVertex(2).ID, segment.GetVertex(3).ID) == 0 && Math.Max(segment.GetVertex(2).ID, segment.GetVertex(3).ID) > 2)
                {
                    key = Math.Max(segment.GetVertex(2).ID, segment.GetVertex(3).ID);
                }
                else
                {
                    key = Math.Min(segment.GetVertex(2).ID, segment.GetVertex(3).ID);
                }

                if (segments.TryGetValue(key, out List<int[]> listSegments))
                {
                    listSegments.Add(new int[2] { segment.GetVertex(0).ID, segment.GetVertex(1).ID });
                }
                else
                {
                    List<int[]> listInitialSegments = new List<int[]>();
                    listInitialSegments.Add(new int[2] { segment.GetVertex(0).ID, segment.GetVertex(1).ID });
                    segments.Add(key, listInitialSegments);
                }
            }
            return segments;
        }

        private String FormatPointInfo(Vertex vertex)
        {
            return String.Format("{0} ({1}, {2})", vertex.ID, vertex.X, vertex.Y);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            points = new List<Point>();
            MainCanvas.Children.Clear();
            DrawFigure(points);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DrawFigure(points);
        }

        private void ChooseFileOfPoints_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();

            Nullable<bool> result = openFileDlg.ShowDialog();

            if (result == true)
            {
                PointFileReader pointFileReader = new PointFileReader();
                PointsFile.Content = result.Value;
                points.AddRange(pointFileReader.ReadPoints(new FileStream(openFileDlg.FileName, FileMode.Open, FileAccess.Read)));
            }

            ((Button)sender).IsEnabled = false;
        }

        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Point position = Mouse.GetPosition(MainCanvas);
            MousePosition.Content = "Current Mouse Position: " + position.X + "   " + position.Y;
        }

        private double CalculateSquareOfTriangle(ITriangle triangle)
        {
            Vertex A = triangle.GetVertex(0);
            Vertex B = triangle.GetVertex(1);
            Vertex C = triangle.GetVertex(2);
            return Math.Abs((A.X * (B.Y - C.Y) + B.X * (C.Y - A.Y) + C.X * (A.Y - B.Y)) / 2);
        }

        private String FindTriangleWithMaxSquare(ICollection<TriangleNet.Topology.Triangle> triangles)
        {
            ITriangle triangleWithMaxSquare = triangles.ToList()[0];
            double maxSquare = CalculateSquareOfTriangle(triangles.ToList()[0]);
            foreach (ITriangle triangle in triangles)
            {
                double calculatedSquare = CalculateSquareOfTriangle(triangle);
                if (calculatedSquare > maxSquare)
                {
                    triangleWithMaxSquare = triangle;
                    maxSquare = calculatedSquare;
                }
            }

            return string.Format("Triangle with max square - ID: {0}, Square: {1}", triangleWithMaxSquare.ID, maxSquare);
        }

        private void ReadCoefs_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();

            Nullable<bool> result = openFileDlg.ShowDialog();

            if (result == true)
            {
                pathToCoefs = openFileDlg.FileName;

            }

            ReadCoefs.IsEnabled = false;
        }

        private void ReadCoefFromFile(String path, SortedDictionary<int, List<SubSegment>> boundarySegments)
        {
            using (var streamReader = new StreamReader(new FileStream(path, FileMode.Open)))
            {
                Condition[] conditions = new Condition[4];
                string[] lines = streamReader.ReadToEnd().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < 4; i++)
                {
                    List<SubSegment> subSegments;
                    boundarySegments.TryGetValue(i, out subSegments);
                    string[] condition = lines[i].Split(' ');
                    conditions[i] = new Condition(Convert.ToDouble(condition[0]), Convert.ToDouble(condition[1]), Convert.ToDouble(condition[2]), i, subSegments);
                }

                string[] equationCoefs = lines[4].Split(' ');
                this.equation = new Equation(Convert.ToDouble(equationCoefs[0]), Convert.ToDouble(equationCoefs[2]),
                    Convert.ToDouble(equationCoefs[3]), Convert.ToDouble(equationCoefs[4]), conditions);
            }
           

        }

        public void ClearAll() {

        }

        private SortedDictionary<int, List<SubSegment>> GetBoundarySegments(IMesh mesh)
        {
            SortedDictionary<int, List<SubSegment>> segments = new SortedDictionary<int, List<SubSegment>>();
            foreach (SubSegment segment in mesh.Segments)
            {
                int key;
                if (Math.Min(segment.GetVertex(2).ID, segment.GetVertex(3).ID) == 0 && Math.Max(segment.GetVertex(2).ID, segment.GetVertex(3).ID) > 2)
                {
                    key = Math.Max(segment.GetVertex(2).ID, segment.GetVertex(3).ID);
                }
                else
                {
                    key = Math.Min(segment.GetVertex(2).ID, segment.GetVertex(3).ID);
                }

                if (segments.TryGetValue(key, out List<SubSegment> listSegments))
                {
                    listSegments.Add(segment);
                }
                else
                {
                    List<SubSegment> listInitialSegments = new List<SubSegment>();
                    listInitialSegments.Add(segment);
                    segments.Add(key, listInitialSegments);
                }
            }
            return segments;
        }

        
    }
}
