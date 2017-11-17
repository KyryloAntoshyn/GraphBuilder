using System.Collections.Generic;
using System.Linq;

namespace GraphBuilder
{
    class DijkstraAlgorithm
    {
        private const int INF = 1000000000; // Максимально большой вес ребра
        private int []shortestDistance;
        private int[] parents;
        private bool[] used;
        public DijkstraAlgorithm(int countVertexes)
        {
            shortestDistance = new int[countVertexes];
            parents = new int[countVertexes];
            used = new bool[countVertexes];
            for (int i = 0; i < countVertexes; i++)
            {
                shortestDistance[i] = INF;
                used[i] = false;
            }
        }
        public string Process(List<List<Edge>> adjacencyList, int startVertex)
        {
            shortestDistance[startVertex] = 0;

            for (int i = 0; i < adjacencyList.Count; i++)
            {
                int v = -1;
                for (int j = 0; j < adjacencyList.Count; j++)
                    if (!used[j])
                    {
                        if (v == -1)
                            v = j;
                        else if (shortestDistance[j] < shortestDistance[v])
                            v = j;
                    } 
           
                if (shortestDistance[v] == INF)
                    break;
                used[v] = true;

                for (int j = 0; j < adjacencyList[v].Count(); j++) // Просматриваем все ребра из данной вершины и делаем релаксацию
                {
                    int vertexTo = adjacencyList[v][j].numberVertexTo;
                    int weight = adjacencyList[v][j].cost;
                    if (shortestDistance[v] + weight < shortestDistance[vertexTo]) //  Релаксация
                    {
                        shortestDistance[vertexTo] = shortestDistance[v] + weight;
                        parents[vertexTo] = v;
                    }
                }
            }
            string result = "Shortest path from vertex " + startVertex.ToString() + " is:\n";
            for (int i = 0; i < parents.Length; i++)
            {
                if (i == startVertex || shortestDistance[i] == INF)
                    continue;
                else
                    result += "Vertex " + i.ToString() + "-->" + shortestDistance[i] + '\n';
            }
            return result;
        }
    }
}