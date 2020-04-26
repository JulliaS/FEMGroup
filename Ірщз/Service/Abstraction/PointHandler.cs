using System.Collections.Generic;
using System.Windows.Controls;
using TriangleNet.Meshing;
using Ірщз.model;

namespace Ірщз.Service.Abstraction
{
    public abstract class PointHandler
    {
        public PointHandler(PointHandler nextHandler)
        {
            _nextHandler = nextHandler;
        }

        public PointHandler() { }

        private PointHandler _nextHandler;
        public void Process(Canvas canvas, List<Point> points)
        {
            ProcessContext(canvas, points);
            ExecuteNext(canvas, points);
        }

        protected abstract void ProcessContext(Canvas canvas, List<Point> points);

        protected void ExecuteNext(Canvas canvas, List<Point> points)
        {
            if (_nextHandler != null)
            {
                _nextHandler.Process(canvas, points);
            }
        }
    }
}
