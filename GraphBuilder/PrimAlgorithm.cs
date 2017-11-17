using System.Collections.Generic;

namespace GraphBuilder
{
    class PrimAlgorithm
    {
        private const int INF = 1000000000;
        private int[] min_e, sel_e;
        private bool[] used;
        List<List<int>> newAdjacency;
        public PrimAlgorithm(List<List<Edge>> adjacencyList)
        {
            int countVertexes = adjacencyList.Count;
            min_e = new int[countVertexes];
            sel_e = new int[countVertexes];
            used = new bool[countVertexes];
            for (int i = 0; i < countVertexes; i++)
            {
                min_e[i] = INF;
                sel_e[i] = -1;
            }
            for (int c = 0; c < countVertexes; c++)
                if (adjacencyList[c].Count != 0)
                {
                    min_e[c] = 0;
                    break;
                }

            newAdjacency = new List<List<int>>();
            for (int i = 0; i < adjacencyList.Count; i++)
            {
                List<int> tmp = new List<int>();
                for (int j = 0; j < adjacencyList.Count; j++)
                    tmp.Add(INF);
                newAdjacency.Add(tmp);
            }

            for (int k = 0; k < adjacencyList.Count; k++)
            {
                foreach (Edge e in adjacencyList[k])
                {
                    newAdjacency[k][e.numberVertexTo] = e.cost;
                    newAdjacency[e.numberVertexTo][k] = e.cost;
                }
            }
        }
        public List<string> Process()
        {
            List<string> path = new List<string>();
            for (int i = 0; i < newAdjacency.Count; i++)
            {
                int v = -1;
                for (int j = 0; j < newAdjacency.Count; j++)
                    if (!used[j] && (v == -1 || min_e[j] < min_e[v]))
                        v = j;
                if (min_e[v] == INF)
                    return path;

                used[v] = true;
                if (sel_e[v] != -1)
                    path.Add(sel_e[v] + "_" + v);

                for (int to = 0; to < newAdjacency.Count; to++)
                    if (newAdjacency[v][to] < min_e[to])
                    {
                        min_e[to] = newAdjacency[v][to];
                        sel_e[to] = v;
                    }                 
            }
            return path;
        }
    }
}