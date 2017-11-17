using System.Collections.Generic;
using System.Linq;

namespace GraphBuilder
{
    class BFS
    {
        List<List<int>> newAdjacency;
        Queue<int> queue;
        int[] marks;
        int[] steps;
        int step = 0;
        public BFS(List<List<Edge>> adjacencyList)
        {
            newAdjacency = new List<List<int>>();
            for (int i = 0; i < adjacencyList.Count; i++)
            {
                List<int> tmp = new List<int>();
                for (int j = 0; j < adjacencyList.Count; j++)
                    tmp.Add(-1111);
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
            for (int i = 0; i < newAdjacency.Count; i++)
            {
                for (int j = 0; j < newAdjacency.Count; j++)
                {
                    if (newAdjacency[i][j] != -1111)
                        newAdjacency[i][j] = 1;
                    else
                        newAdjacency[i][j] = 0;
                }
            }
            queue = new Queue<int>();
            marks = new int[newAdjacency.Count];
            steps = new int[newAdjacency.Count];
        }
        public string Process(int numberVertex)
        {
            List<int> dequeued = new List<int>();
            queue.Enqueue(numberVertex);
            steps[numberVertex] = step; step++;
            // 1 - зеленый цвет, -1 - черный
            marks[numberVertex] = 1; // Пометили вершину зеленым цветом
            
            while (queue.Count != 0)
            {
                bool f = false;

                dequeued.Add(queue.Dequeue());
                int dequeuedNum = dequeued.Last();
                marks[dequeuedNum] = -1; // При извлечении метим чёрным цветом

                for (int j = 0; j < newAdjacency.Count; j++)
                 {
                    if (newAdjacency[dequeuedNum][j] != 0 && marks[j] != -1 && marks[j] != 1)
                    {
                        f = true;
                        queue.Enqueue(j);
                        steps[j] = step;
                        marks[j] = 1;
                    }
                }
                if (f)
                    step++;
            }
            string res = "Wave from vertex  " + numberVertex.ToString() + ":\n";
            for (int i = 0; i < steps.Length; i++)
            {
                if (MainWindow.graph.graphAlgorithms.adjacencyList[i].Count > 0)
                    res += i.ToString() + "-->" + steps[i] + '\n';
            }
            return res;
        }
    }
}