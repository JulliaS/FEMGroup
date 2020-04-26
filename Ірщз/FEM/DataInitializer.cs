using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using TriangleNet.Topology;
using Ірщз.FEMSolver;
using Ірщз.Service.Impl.CoR;
using CustomPoint = Ірщз.model.Point;

namespace Ірщз.FEM
{
    public class DataInitializer
    {
        public Vertex[] vertices { get; set; }
        public Triangle[] triangles { get; set; }
        public int[][,] segments { get; set; }
        public double[] squareTriangles { get; set; }
        public double[][] lengthSegments { get; set; }

        private TriangleNetHandler TriangleNetHandler;

        private List<CustomPoint> Points;

        private Equation Equation;

        public DataInitializer(TriangleNetHandler triangleNetHandler, List<CustomPoint> points, Equation equation)
        {
            Points = points;
            Equation = equation;
            TriangleNetHandler = triangleNetHandler;
        }

        public FEMContext GetInitialData()
        {
            var femContex = new FEMContext();

            TriangleNetHandler.Process(null, Points);

            var mesh = TriangleNetHandler.mesh;

            vertices = GetPointsFromMesh(mesh);//points
            triangles = GetTrianglesFromMesh(mesh);
            segments = GetArraySegmentsFromMesh(mesh);
            squareTriangles = GetSquareTriangles(triangles);
            lengthSegments = GetLengthSegments(segments, vertices);

            femContex.Data = new Data(vertices, triangles, segments, squareTriangles, lengthSegments);
            femContex.Equation = Equation;

            return femContex;
        }

        private Vertex[] GetPointsFromMesh(IMesh mesh)
        {
            int N = mesh.Vertices.Max(x => x.ID);
            Vertex[] vertices = new Vertex[N + 1];
            foreach (Vertex vertex in mesh.Vertices)
            {
                vertices[vertex.ID] = vertex;
            }
            return vertices;
        }

        private Triangle[] GetTrianglesFromMesh(IMesh mesh)
        {
            return mesh.Triangles.ToArray();
        }

        private int[][,] GetArraySegmentsFromMesh(IMesh mesh)
        {
            SortedDictionary<int, List<int[]>> SegmentsDictionary = GetSegments(mesh);
            int[][,] SegmentsArray = new int[SegmentsDictionary.Count][,];
            int k = 0;
            foreach (List<int[]> subSegments in SegmentsDictionary.Values)
            {
                SegmentsArray[k] = new int[subSegments.Count, 2];
                for (int i = 0; i < subSegments.Count; i++)
                {
                    SegmentsArray[k][i, 0] = subSegments[i][0];
                    SegmentsArray[k][i, 1] = subSegments[i][1];
                }
                k++;
            }
            return SegmentsArray;
        }

        private double[] GetSquareTriangles(Triangle[] triangles)
        {
            double[] area = new double[triangles.Length];
            for (int i = 0; i < triangles.Length; i++)
            {
                area[i] = CalculateSquareOfTriangle(triangles[i]);
            }
            return area;
        }

        private double[][] GetLengthSegments(int[][,] segments, Vertex[] vertices)
        {
            double[][] LengthSegments = new double[segments.Length][];
            for (int border = 0; border < segments.Length; border++)
            {
                LengthSegments[border] = new double[segments[border].GetLength(0)];
                for (int k = 0; k < segments[border].GetLength(0); k++)
                {
                    Vertex v1 = vertices[segments[border][k, 0]];
                    Vertex v2 = vertices[segments[border][k, 1]];
                    LengthSegments[border][k] = Length(v1, v2);
                }
            }
            return LengthSegments;
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

        private double Length(Vertex v1, Vertex v2)
        {
            return Math.Sqrt(Math.Pow(v1.X - v2.X, 2) + Math.Pow(v1.Y - v2.Y, 2));
        }

        private double CalculateSquareOfTriangle(ITriangle triangle)
        {
            Vertex A = triangle.GetVertex(0);
            Vertex B = triangle.GetVertex(1);
            Vertex C = triangle.GetVertex(2);
            return Math.Abs((A.X * (B.Y - C.Y) + B.X * (C.Y - A.Y) + C.X * (A.Y - B.Y)) / 2);
        }
    }
}
