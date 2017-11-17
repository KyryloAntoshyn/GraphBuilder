using System;
namespace GraphBuilder
{
    [Serializable]
    public class Edge
    {
        public int numberVertexTo { get; set; }
        public int cost { get; set; }
        public Edge(int v, int c)
        {
            numberVertexTo = v;
            cost = c;
        }
    }
}

