using System.Windows;
using System.IO;
using System.Windows.Markup;
using System.Xml;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Shapes;

namespace GraphBuilder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public static class WindowOpened
    {
        public static bool isOpened;
        static WindowOpened()
        {
            isOpened = false;
        }
    }
    public partial class MainWindow : Window
    {
        public static Graph graph;

        public MainWindow()
        {
            InitializeComponent();
            graph = new Graph(this.canvas);
        }

        // Обработчики нажатий------------------------------------------------------------------------------------------------------------
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            graph.AddEllipse();
        }        

        private void btnAddEdge_Click(object sender, RoutedEventArgs e)
        {
            graph.united = true;
        }

        private void btnDeleteNode_Click(object sender, RoutedEventArgs e)
        {
           graph.isDeleteVertex = true;
        }

        private void btnDeleteEdge_Click(object sender, RoutedEventArgs e)
        {
            graph.DeleteEdges();
        }        

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            graph.SaveGraph();
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "Choose saved canvas you need"; // Default file name
            dlg.DefaultExt = ".txt"; // Default file extension
            dlg.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension
            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                string fileName = dlg.FileName;
                graph = graph.OpenGraph(fileName);
                if (graph.isDirected)
                {
                    radioButtonDirected.IsChecked = true;
                    graph.MakeUndirected();
                    graph.MakeDirected();
                }
                else
                    radioButtonUndirected.IsChecked = true;
            }
        }

        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            if (graph.conditionsUndo.Count > 1)
            {
                graph.conditionsRedo.Push(graph.conditionsUndo.Pop());
                StringReader strReader = new StringReader(graph.conditionsUndo.Peek());
                XmlReader xmlReader = XmlReader.Create(strReader);
                Canvas canvasTmp = (Canvas)XamlReader.Load(xmlReader);

                // Формирую список объектов
                List<object> listObjects = new List<object>();
                foreach (var v in canvasTmp.Children)
                    listObjects.Add(v);

                canvasTmp.Children.Clear();
                this.canvas.Children.Clear();

                // ПОЛЯ ГРАФА
                Graph oldGraph = new Graph(this.canvas); // tmp graph
                oldGraph.conditionsUndo = graph.conditionsUndo;
                oldGraph.conditionsRedo = graph.conditionsRedo;
                oldGraph.adjacencyUndo = graph.adjacencyUndo;
                oldGraph.adjacencyRedo = graph.adjacencyRedo;
                oldGraph.colorsUndo = graph.colorsUndo;
                oldGraph.colorsRedo = graph.colorsRedo;
                oldGraph.isDirectedUndo = graph.isDirectedUndo;
                oldGraph.isDirectedRedo = graph.isDirectedRedo;
                oldGraph.newTagCounterUndo = graph.newTagCounterUndo;
                oldGraph.newTagCounterRedo = graph.newTagCounterRedo;

                graph = new Graph(this.canvas);
                graph.conditionsUndo = oldGraph.conditionsUndo;
                graph.conditionsRedo = oldGraph.conditionsRedo;
                graph.adjacencyUndo = oldGraph.adjacencyUndo;
                graph.adjacencyRedo = oldGraph.adjacencyRedo;
                graph.colorsUndo = oldGraph.colorsUndo;
                graph.colorsRedo = oldGraph.colorsRedo;
                graph.isDirectedUndo = oldGraph.isDirectedUndo;
                graph.isDirectedRedo = oldGraph.isDirectedRedo;
                graph.newTagCounterUndo = oldGraph.newTagCounterUndo;
                graph.newTagCounterRedo = oldGraph.newTagCounterRedo;

                // Меняю список смежности

                graph.adjacencyRedo.Push(graph.adjacencyUndo.Pop());
                graph.graphAlgorithms.adjacencyList = graph.adjacencyUndo.Peek();


                graph.newTagCounterRedo.Push(graph.newTagCounterUndo.Pop());
                graph.newTagCouter = graph.newTagCounterUndo.Peek();


                // Меняю цвета
                graph.colorsRedo.Push(graph.colorsUndo.Pop());
                graph.lineColors = graph.colorsUndo.Peek();

                graph.isDirectedRedo.Push(graph.isDirectedUndo.Pop());
                graph.isDirected = graph.isDirectedUndo.Peek();

                if (graph.isDirected)
                {
                    radioButtonDirected.IsChecked = true;
                }
                else
                {
                    radioButtonUndirected.IsChecked = true;
                }

                // Меняю список смежности для интерфейса
                foreach (object obj in listObjects)
                {
                    if (obj is Ellipse)
                    {
                        graph.AddEllipseUndoRedo(obj as Ellipse);
                    }
                }
                foreach (object obj in listObjects) // Все вершины уже созданы и занесены в список
                {
                    if (obj is Line && (obj as Line).Tag.ToString()[0] != 'A') // Линия и не линия-стрелка
                    {
                        graph.AddLineUndoRedo(obj as Line);
                    }
                }
            }
            else
            {
                if (!WindowOpened.isOpened)
                {
                    Window helpWindow = new AlgoHelper("There are no actions to undo.");
                    helpWindow.Show();
                    WindowOpened.isOpened = true;
                }
            }
        }

        private void btnRedo_Click(object sender, RoutedEventArgs e)
        {
            if (graph.conditionsRedo.Count > 1)
            {
                StringReader strReader = new StringReader(graph.conditionsRedo.Peek());
                XmlReader xmlReader = XmlReader.Create(strReader);
                Canvas canvasTmp = (Canvas)XamlReader.Load(xmlReader);

                // Формирую список объектов
                List<object> listObjects = new List<object>();
                foreach (var v in canvasTmp.Children)
                    listObjects.Add(v);

                canvasTmp.Children.Clear();
                this.canvas.Children.Clear();

                // ПОЛЯ ГРАФА
                Graph oldGraph = new Graph(this.canvas); // tmp graph
                oldGraph.conditionsUndo = graph.conditionsUndo;
                oldGraph.conditionsRedo = graph.conditionsRedo;
                oldGraph.adjacencyUndo = graph.adjacencyUndo;
                oldGraph.adjacencyRedo = graph.adjacencyRedo;
                oldGraph.colorsUndo = graph.colorsUndo;
                oldGraph.colorsRedo = graph.colorsRedo;
                oldGraph.isDirectedUndo = graph.isDirectedUndo;
                oldGraph.isDirectedRedo = graph.isDirectedRedo;
                oldGraph.newTagCounterUndo = graph.newTagCounterUndo;
                oldGraph.newTagCounterRedo = graph.newTagCounterRedo;

                graph = new Graph(this.canvas);
                graph.conditionsUndo = oldGraph.conditionsUndo;
                graph.conditionsRedo = oldGraph.conditionsRedo;
                graph.adjacencyUndo = oldGraph.adjacencyUndo;
                graph.adjacencyRedo = oldGraph.adjacencyRedo;
                graph.colorsUndo = oldGraph.colorsUndo;
                graph.colorsRedo = oldGraph.colorsRedo;
                graph.isDirectedUndo = oldGraph.isDirectedUndo;
                graph.isDirectedRedo = oldGraph.isDirectedRedo;
                graph.newTagCounterUndo = oldGraph.newTagCounterUndo;
                graph.newTagCounterRedo = oldGraph.newTagCounterRedo;

                // Меняю список смежности

                graph.graphAlgorithms.adjacencyList = graph.adjacencyRedo.Peek();
                graph.adjacencyUndo.Push(graph.adjacencyRedo.Pop());

                graph.newTagCouter = graph.newTagCounterRedo.Peek();
                graph.newTagCounterUndo.Push(graph.newTagCounterRedo.Pop());

                // Меняю цвета
                graph.lineColors = graph.colorsRedo.Peek();
                graph.colorsUndo.Push(graph.colorsRedo.Pop());


                graph.isDirected = graph.isDirectedRedo.Peek();
                graph.isDirectedUndo.Push(graph.isDirectedRedo.Pop());

                if (graph.isDirected)
                    radioButtonDirected.IsChecked = true;
                else
                    radioButtonUndirected.IsChecked = true;

                // Меняю список смежности для интерфейса
                foreach (object obj in listObjects)
                {
                    if (obj is Ellipse)
                    {
                        graph.AddEllipseUndoRedo(obj as Ellipse);
                    }
                }
                foreach (object obj in listObjects) // Все вершины уже созданы и занесены в список
                {
                    if (obj is Line && (obj as Line).Tag.ToString()[0] != 'A') // Линия и не линия-стрелка
                    {
                        graph.AddLineUndoRedo(obj as Line);
                    }
                }
                graph.conditionsUndo.Push(graph.conditionsRedo.Pop());
            }
            else
            {
                if (!WindowOpened.isOpened)
                {
                    Window helpWindow = new AlgoHelper("There are no actions to redo.");
                    helpWindow.Show();
                    WindowOpened.isOpened = true;
                }
            }
        }

        private void btnGetHelp_Click(object sender, RoutedEventArgs e)
        {
            if (!WindowOpened.isOpened)
            {
                Window helpWindow = new HelpWindow();
                helpWindow.Show();
                WindowOpened.isOpened = true;
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            graph.isEdit = true;
        }
               
        private void radioButtonDirected_Checked(object sender, RoutedEventArgs e)
        {
            graph.isDirected = true;
            graph.graphAlgorithms.ChangeToDirected();
            graph.MakeDirected();
        }

        private void radioButtonUndirected_Checked(object sender, RoutedEventArgs e)
        {
            graph.isDirected = false;
            graph.MakeUndirected();
        }

        private void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            radioButtonUndirected.IsChecked = true;
        }

        // Algorithms------------------------------------------------------------------------------------------------------------
        private void Prim_Click(object sender, RoutedEventArgs e)
        {
            graph.isDeleteVertex = false;
            graph.isEdit = false;
            if ((!graph.isDirected && graph.graphAlgorithms.adjacencyList.Count != 0) && graph.canvas.Children.Count != 0)
            {
                graph.ReturnColors();
                graph.MinimalOstov(graph.graphAlgorithms.Prim());
                Window helper = new AlgoHelper("Click on node to return colors");
                helper.Show();
                WindowOpened.isOpened = true;
            }
            else if (!WindowOpened.isOpened && graph.isDirected)
            {
                Window helper = new AlgoHelper("Graph must be undirected!");
                helper.Show();
                WindowOpened.isOpened = true;
            }
            else if (graph.graphAlgorithms.adjacencyList.Count == 0 || graph.canvas.Children.Count == 0)
            {
                Window helper = new AlgoHelper("Graph doesn't contain elements!");
                helper.Show();
                WindowOpened.isOpened = true;
            }
        }

        private void btnBFS_Click(object sender, RoutedEventArgs e)
        {
            graph.isDeleteVertex = false;
            graph.isEdit = false;
            if (graph.graphAlgorithms.adjacencyList.Count == 0 || graph.canvas.Children.Count == 0)
            {
                Window helper = new AlgoHelper("Graph doesn't contain elements!");
                helper.Show();
                WindowOpened.isOpened = true;
            }
            else if (!WindowOpened.isOpened && graph.isDirected)
            {
                Window helper = new AlgoHelper("Graph must be undirected!");
                helper.Show();
                WindowOpened.isOpened = true;
            }
            else
            {
                graph.isBFS = true;
                Window helper = new AlgoHelper("Select one node");
                helper.Show();
                WindowOpened.isOpened = true;
            }
        }

        private void btnDijkstra_Click(object sender, RoutedEventArgs e)
        {
            graph.isDeleteVertex = false;
            graph.isEdit = false;
            if (graph.graphAlgorithms.adjacencyList.Count == 0 || graph.canvas.Children.Count == 0)
            {
                Window helper = new AlgoHelper("Graph doesn't contain elements!");
                helper.Show();
                WindowOpened.isOpened = true;
            }
            else
            {
                if (graph.isDirected)
                    graph.graphAlgorithms.ChangeToDirected();
                graph.isDijkstra = true;
                Window helper = new AlgoHelper("Select one node");
                helper.Show();
                WindowOpened.isOpened = true;
            }
        }
    }
}