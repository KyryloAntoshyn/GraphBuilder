using System.Collections.Generic;
using System.Windows;
using System.Windows.Shapes;

namespace GraphBuilder
{
    public class Vertex
    {
        public Point vertex { get; set; }
        public List<Line> linesBordered { get; set; } // Для прорисовки интерфейса
        public int tag { get; set; } // По-дугому, номер
        public Vertex(Point vertex, List<Line> linesBordered, int tag)
        {
            this.vertex = vertex;
            this.linesBordered = linesBordered;
            this.tag = tag;
        }
    }
}
