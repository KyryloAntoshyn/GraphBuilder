using System.Collections.Generic;
using System.Windows.Shapes;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace GraphBuilder
{
    public class GraphAlgorithms
    {
        public List<List<Edge>> adjacencyList { get; set; } // Список смежности будем формировать динамически

        private DijkstraAlgorithm dijkstra;
        private PrimAlgorithm prim;
        private BFS bfs;
        public GraphAlgorithms()
        {
            adjacencyList = new List<List<Edge>>();
        }
        public void AddEdgeToAdjacencyList(int vertex1, int vertex2, int weight, bool directed)
        {
            for (int i = 0; i < adjacencyList.Count; i++)
            {
                if (vertex1 == i)
                    adjacencyList[i].Add(new Edge(vertex2, weight));
                else if (vertex2 == i)
                    adjacencyList[i].Add(new Edge(vertex1, weight));
            }
        }
        public void ChangeToDirected()
        {
            List<string> tags = new List<string>();
            foreach (Vertex v in MainWindow.graph.vertices)
            {
                foreach (Line l in v.linesBordered)
                {
                    if (!tags.Contains(l.Tag.ToString()))
                        tags.Add(l.Tag.ToString());
                }
            }
            List<string> used = new List<string>();
            foreach (string tag in tags)
            {
                for (int i = 0; i < adjacencyList.Count; i++)
                {
                    List<Edge> listEdges = adjacencyList[i];
                    foreach (Edge edge in listEdges)
                    {
                        if (adjacencyList.IndexOf(listEdges).ToString() + "_" + edge.numberVertexTo.ToString() == tag && !used.Contains(tag))
                        {
                            List<Edge> edges = adjacencyList[edge.numberVertexTo];
                            foreach (Edge e in edges)
                            {
                                if (e.numberVertexTo == adjacencyList.IndexOf(listEdges))
                                {
                                    adjacencyList[edge.numberVertexTo].Remove(e);
                                    break;
                                }
                            }
                            used.Add(tag);
                            i--;
                            break;
                        }
                    }
                }
            }
        }
        public void ChangeToUndirected()
        {
            List<string> tags = new List<string>();
            foreach (Vertex v in MainWindow.graph.vertices)
            {
                foreach (Line l in v.linesBordered)
                {
                    if (!tags.Contains(l.Tag.ToString()))
                        tags.Add(l.Tag.ToString());
                }
            }
            foreach (string tag in tags)
            {
                int vertex1 = int.Parse(tag[0].ToString()), vertex2 = int.Parse(tag[2].ToString());
                int weight = 0;
                foreach (Edge e in adjacencyList[vertex1])
                {
                    if (e.numberVertexTo == vertex2)
                    {
                        weight = e.cost;
                        break;
                    }
                }
                adjacencyList[vertex2].Add(new Edge(vertex1, weight));
            }
        }
        public string Dijkstra(int startVertex)
        {
            dijkstra = new DijkstraAlgorithm(adjacencyList.Count);
            return dijkstra.Process(adjacencyList, startVertex);
        }
        public List<string> Prim()
        {
            prim = new PrimAlgorithm(adjacencyList);
            return prim.Process();
        }
        public string BFS(int startVertex)
        {
            bfs = new BFS(adjacencyList);
            return bfs.Process(startVertex);
        }
        // Для SAVE
        public void SerializeAdjacencyList()
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (FileStream fs = new FileStream("saved_adjacency_" + DateTime.Now.Hour + "_" + DateTime.Now.Minute + "_" + DateTime.Now.Second + ".dat", FileMode.Create))
            {
                bf.Serialize(fs, this.adjacencyList);
            }
        }
        public List<List<Edge>> DeserializeAdjacencyList(string path)
        {
            BinaryFormatter bf = new BinaryFormatter();
            List<List<Edge>> adjacencyListDeserialized = new List<List<Edge>>();

            string[] tmpStr = path.Split('\\');
            string file = tmpStr[tmpStr.Length - 1];
            string[] fileName = file.Split('_');
            fileName[1] = "adjacency";
            string tmp = fileName[4].Split('.')[0];
            fileName[4] = tmp + ".dat";
            file = fileName[0] + "_" + fileName[1] + "_" + fileName[2] + "_" + fileName[3] + "_" + fileName[4];
            using (FileStream fs = new FileStream(file, FileMode.Open))
            {
                adjacencyListDeserialized = bf.Deserialize(fs) as List<List<Edge>>;
            }
            return adjacencyListDeserialized;
        }
    }
}