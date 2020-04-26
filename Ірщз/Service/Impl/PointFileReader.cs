using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ірщз.model;

namespace Ірщз.Service
{
    class PointFileReader
    {
        public List<Point> ReadPoints(FileStream fileStream)
        {
            List<Point> points = new List<Point>();

            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
            {
                String line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    string[] stringCoordinates = line.Split(' ');
                    double x = Convert.ToDouble(stringCoordinates[0]);
                    double y = Convert.ToDouble(stringCoordinates[1]);
                    points.Add(new Point() { X = x, Y = y });
                }
            }
            return points;
        }

        public List<int> ReadCoefs(FileStream fileStream)
        {
            List<int> coefs = new List<int>();
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
            {
                string line = streamReader.ReadLine();
                string[] splitedLine = line.Split(' ');
                foreach(string i in splitedLine)
                {
                    coefs.Add(Convert.ToInt32(i));
                }
            }
            return coefs;
        }
    }
}
