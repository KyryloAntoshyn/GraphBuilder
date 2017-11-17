using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Markup;
using System.Xml;

namespace GraphBuilder
{
    public class Graph
    {
        public Canvas canvas { get; set; }
        private bool isCaptured;
        private string nameNodeToEdit;
        private int counterChoosenVertexes;
        private int tagVertex1;
        private int tagVertex2;
        public Point p1, p2;
        private int focusedEdges;
        public LineColors lineColors { get; set; } // Цвета линий

        public GraphAlgorithms graphAlgorithms; // Алгоритмы с графом
        public bool isDijkstra { get; set; }
        public bool isBFS { get; set; }
        public Arrow arrow { get; set; }
        public List<Vertex> vertices { get; set; }
        public bool united { get; set; }
        public bool isDeleteVertex { get; set; }
        public bool isEdit { get; set; }
        public int newTagCouter { get; set; }
        public bool isDirected { get; set; }
        public int startVertex { get; set; }
        public static bool isReturnColors { get; set; }

        //// UNDO/REDO
        public Stack<string> conditionsUndo { get; set; }
        public Stack<string> conditionsRedo { get; set; }

        public Stack<List<List<Edge>>> adjacencyUndo { get; set; }
        public Stack<List<List<Edge>>> adjacencyRedo { get; set; }

        public Stack<LineColors> colorsUndo { get; set; }
        public Stack<LineColors> colorsRedo { get; set; }

        public Stack<bool> isDirectedUndo { get; set; }
        public Stack<bool> isDirectedRedo { get; set; }

        public Stack<int> newTagCounterUndo { get; set; }
        public Stack<int> newTagCounterRedo { get; set; }

        public Graph(Canvas c)
        {
            isDijkstra = false;
            isBFS = false;

            arrow = new Arrow();
            canvas = c;
            canvas.Background = Brushes.White;

            vertices = new List<Vertex>();

            united = false;
            isDeleteVertex = false;
            isEdit = false;

            newTagCouter = 0;
            isCaptured = false;
            nameNodeToEdit = "";
            counterChoosenVertexes = 0;

            tagVertex1 = 0;
            tagVertex2 = 0;

            p1 = new Point();
            p2 = new Point();

            lineColors = new LineColors();

            isDirected = false; // При запуске граф неориентирован по дефолту

            focusedEdges = 0;

            graphAlgorithms = new GraphAlgorithms();

            isReturnColors = false;

            conditionsUndo = new Stack<string>();
            conditionsUndo.Push(XamlWriter.Save(canvas)); // Сохраняю пустой канвас
            conditionsRedo = new Stack<string>();
            conditionsRedo.Push(XamlWriter.Save(canvas)); // Сохраняю пустой канвас

            adjacencyUndo = new Stack<List<List<Edge>>>();
            adjacencyUndo.Push(new List<List<Edge>>(DeepClone<List<List<Edge>>>(graphAlgorithms.adjacencyList)));
            adjacencyRedo = new Stack<List<List<Edge>>>();
            adjacencyRedo.Push(new List<List<Edge>>(DeepClone<List<List<Edge>>>(graphAlgorithms.adjacencyList)));

            colorsUndo = new Stack<LineColors>();
            lineColors.AddToFile(false);
            colorsUndo.Push(new LineColors (LineColors.DataFromFile()));
            colorsRedo = new Stack<LineColors>();
            colorsRedo.Push(new LineColors(LineColors.DataFromFile()));

            isDirectedUndo = new Stack<bool>();
            isDirectedUndo.Push(false); // Сначала граф не ориентирован
            isDirectedRedo = new Stack<bool>();
            isDirectedRedo.Push(false);

            newTagCounterUndo = new Stack<int>();
            newTagCounterUndo.Push(0);
            newTagCounterRedo = new Stack<int>();
            newTagCounterRedo.Push(0);
        }

        //-----------------------------------------------------SAVING---------------------------------------------------------------------------------------------
        public void SaveGraph() // ВОЗМОЖНОСТЬ СЕРИАЛИЗАЦИИ ГРАФА
        {
            if (!isDeleteVertex && !isDeleteVertex && !isEdit && !isBFS && !isReturnColors && !united && !isCaptured)
            {
                string saved = XamlWriter.Save(this.canvas); // Сохраняю канвас
                string path = System.IO.Directory.GetCurrentDirectory() + "\\" + "saved_canvas_" + DateTime.Now.Hour + "_" + DateTime.Now.Minute + "_" + DateTime.Now.Second + ".txt";

                using (System.IO.File.Create(path)) { }
                System.IO.File.WriteAllText(path, saved); // Записываю канвас в файл
                graphAlgorithms.SerializeAdjacencyList(); // Сериализую список смежности для алгоритмов
                this.lineColors.AddToFile(true); // Сохраняю цвета, чтобы потом восстановить
                // Сохранение свойств
                string path2 = System.IO.Directory.GetCurrentDirectory() + "\\" + "saved_properties_" + DateTime.Now.Hour + "_" + DateTime.Now.Minute + "_" + DateTime.Now.Second + ".txt";
                using (System.IO.File.Create(path2)) { }
                string properties = isDirected.ToString() + ' ' + newTagCouter.ToString();
                System.IO.File.WriteAllText(path2, properties);
            }
            else
            {
                if (!WindowOpened.isOpened)
                {
                    Window helper = new AlgoHelper("Finish your actions.");
                    helper.Show();
                    WindowOpened.isOpened = true;
                }
            }
        }
        public Graph OpenGraph(string savedCanvasPath) // Возращение к сохраненному состоянию
        {
            if (!isDeleteVertex && !isDeleteVertex && !isEdit && !isBFS && !isReturnColors && !united && !isCaptured)
            {
                string obj = System.IO.File.ReadAllText(savedCanvasPath);
                System.IO.StringReader strReader = new System.IO.StringReader(obj);
                XmlReader xmlReader = XmlReader.Create(strReader);
                Canvas canvasTmp = (Canvas)XamlReader.Load(xmlReader);

                // Формирую список объектов
                List<object> listObjects = new List<object>();
                foreach (var v in canvasTmp.Children)
                    listObjects.Add(v);

                canvasTmp.Children.Clear(); // Чтобы не конфликтовала со связями
                this.canvas.Children.Clear(); // Очищаю канвас

                Graph newGraph = new Graph(this.canvas); // Создаю новый класс граф, в котором буду заполнять поляnum
                string[] tmp = savedCanvasPath.Split('\\');
                string[] tmp1 = tmp[9].Split('_');
                tmp1[1] = "properties";
                tmp[9] = tmp1[0] + '_' + tmp1[1] + '_' + tmp1[2] + '_' + tmp1[3] + '_' + tmp1[4];
                string tmpStr = tmp[0] + "\\" + tmp[1] + "\\" + tmp[2] + "\\" + tmp[3] + "\\" + tmp[4] +
                    "\\" + tmp[5] + "\\" + tmp[6] + "\\" + tmp[7] + "\\" + tmp[8] + "\\" + tmp[9];

                string [] properties= System.IO.File.ReadAllLines(tmpStr);
                properties = properties[0].Split(' ');
                // Заполняю свойства
                newGraph.isDirected = Convert.ToBoolean(properties[0]);
                newGraph.newTagCouter = Convert.ToInt32(properties[1]);

                newGraph.graphAlgorithms.adjacencyList = graphAlgorithms.DeserializeAdjacencyList(savedCanvasPath);
                newGraph.lineColors.colorsEdges = this.lineColors.ReadFromFile(savedCanvasPath); // Считываю сохраненные цвета с файла
                
                foreach (object o in listObjects)
                {
                    if (o is Ellipse) // Добавляю вершину с номером на ней
                    {
                        newGraph.AddEllipseOpen(o as Ellipse);
                    }
                    else if (o is TextBox && (o as TextBox).Tag != null) // Textbox линии
                    {
                        (o as TextBox).MouseDoubleClick += newGraph.OnTextBoxEdgeMouseDoubleClick;
                        newGraph.canvas.Children.Add(o as TextBox);
                    }
                    else if (o is TextBox) // Textbox вершины
                    {
                        (o as TextBox).MouseDoubleClick += newGraph.OnTextBoxNodeMouseDoubleClick;
                        newGraph.canvas.Children.Add(o as TextBox);
                    }
                    else if (o is Line && (o as Line).Tag.ToString().First() == 'A') // Часть стрелки
                    {
                        newGraph.canvas.Children.Add(o as Line);
                    }
                }
                foreach (object o in listObjects) // Гарантия, что все вершины сформированы
                {
                    if (o is Line && (o as Line).Tag.ToString().First() != 'A') // Линия - не стрелка
                    {
                        newGraph.AddLineOpen(o as Line);
                    }
                }
                return newGraph;
            }
            else
            {
                if (!WindowOpened.isOpened)
                {
                    Window helper = new AlgoHelper("Finish your actions.");
                    helper.Show();
                    WindowOpened.isOpened = true;
                }
                return null;
            }
        }
        public void AddEllipseOpen(Ellipse ell)
        {
            ell.MouseLeftButtonDown += new MouseButtonEventHandler(OnNodeClicked);
            ell.MouseLeftButtonUp += new MouseButtonEventHandler(OnEllipseUp);
            ell.MouseMove += new MouseEventHandler(OnShapeMouseMove);

            Canvas.SetZIndex(ell, 2);
            this.canvas.Children.Add(ell);

            vertices.Add(new Vertex(new Point(p1.X, p1.Y), new List<Line>(), Convert.ToInt32(ell.Tag)));
        }
        public void AddLineOpen(Line line)
        {
            line.MouseLeftButtonUp += OnEdgeClicked;
            Canvas.SetZIndex(line, 1);
            this.canvas.Children.Add(line);
            // Формирую список линий для вершин
            foreach (Vertex v in vertices)
            {
                if (v.tag == Convert.ToInt32(line.Tag.ToString().Split('_')[0]) || v.tag == Convert.ToInt32(line.Tag.ToString().Split('_')[1]))
                    v.linesBordered.Add(line);
            }
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------

        // UNDO/REDO------------------------------------------------------------------------------------------------------------------
        public void AddEllipseUndoRedo(Ellipse ellipse)
        {
            ellipse.MouseLeftButtonDown += new MouseButtonEventHandler(OnNodeClicked);
            ellipse.MouseLeftButtonUp += new MouseButtonEventHandler(OnEllipseUp);
            ellipse.MouseMove += new MouseEventHandler(OnShapeMouseMove);
            Canvas.SetZIndex(ellipse, 2);
            canvas.Children.Add(ellipse);

            ConnectTextBoxWithVertex(ellipse);

            vertices.Add(new Vertex(new Point(Canvas.GetLeft(ellipse), Canvas.GetTop(ellipse)), new List<Line>(), Convert.ToInt32(ellipse.Tag)));
        }
        public void AddLineUndoRedo(Line line)
        {
            line.MouseLeftButtonUp += OnEdgeClicked;
            Canvas.SetZIndex(line, 1);

            if (isDirected)
                arrow.DrawArrow(line, canvas);

            // Доделать цвета линии
            canvas.Children.Add(line);

            foreach (Vertex v in vertices) // Добавляю смежные линии
                if (v.tag.ToString() == line.Tag.ToString()[0].ToString() || v.tag.ToString() == line.Tag.ToString()[2].ToString())
                    v.linesBordered.Add(line);

            string weight = null;
            int i = 0;
            foreach (List<Edge> listEdges in graphAlgorithms.adjacencyList)
            {
                foreach (Edge e in listEdges)
                    if (line.Tag.ToString() == i.ToString() + '_' + e.numberVertexTo.ToString() || line.Tag.ToString() == e.numberVertexTo.ToString() + '_' + i.ToString())
                    {
                        weight = e.cost.ToString();
                        break;
                    }
                if (weight != null)
                    break;
                i++;
            }
            ConnectTextBoxWithEdge(line, weight);
        }
        public static T DeepClone<T>(T obj) // Deep copy
        {
            using (var ms = new System.IO.MemoryStream())
            {
                var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddEllipse()
        {
            Ellipse ell = new Ellipse();
            ell.MouseLeftButtonDown += new MouseButtonEventHandler(OnNodeClicked);
            ell.MouseLeftButtonUp += new MouseButtonEventHandler(OnEllipseUp);
            ell.MouseMove += new MouseEventHandler(OnShapeMouseMove);
            foreach (UIElement ui in canvas.Children) // Если номер вершины был изменен на ту, которую хотим создать
            {
                if (ui is Ellipse && Convert.ToInt32((ui as Ellipse).Tag) == newTagCouter)
                    newTagCouter++;
            }
            ell.Tag = newTagCouter;
            ell.Height = 47;
            ell.Width = 47;
            ell.Fill = Brushes.White;
            ell.Stroke = Brushes.Blue;
            ell.StrokeThickness = 2;
            p1.X = 500;
            p1.Y = 100;
            Canvas.SetLeft(ell, p1.X);
            Canvas.SetTop(ell, p1.Y);
            Canvas.SetZIndex(ell, 2);
            canvas.Children.Add(ell);

            ConnectTextBoxWithVertex(ell);

            vertices.Add(new Vertex(new Point(p1.X, p1.Y), new List<Line>(), newTagCouter));
            newTagCouter++;               

            graphAlgorithms.adjacencyList.Add(new List<Edge>()); // Добавляем вершину в список смежности

            // Сохраняю состояние canvas'a
            conditionsUndo.Push(XamlWriter.Save(canvas));
            // Добавить новый список смежности с ребром
            adjacencyUndo.Push(new List<List<Edge>>(DeepClone<List<List<Edge>>>(graphAlgorithms.adjacencyList)));
            // Добавить в цвета
            lineColors.AddToFile(false);
            colorsUndo.Push(new LineColors(LineColors.DataFromFile()));
            isDirectedUndo.Push(isDirected);
            newTagCounterUndo.Push(newTagCouter);
        }

        public void OnNodeClicked(object sender, MouseButtonEventArgs e)
        {
            Ellipse ell = sender as Ellipse;
            if (united != true)
            {
                if (isReturnColors)
                {
                    ReturnColors();
                    isReturnColors = false;
                }
                else if (!isDeleteVertex && !isEdit && !isDijkstra && !isBFS)
                {
                    ell.Stroke = Brushes.Red; // Выделение красным
                    isCaptured = true;
                    ell.CaptureMouse();
                    ell.Cursor = Cursors.Cross;
                }
                else if (!isDeleteVertex && isEdit) // Редактирование
                {
                    canvas.Background = Brushes.Gray;
                    foreach (UIElement ui in canvas.Children)
                    {
                        if (ui is TextBox && (ui as TextBox).Text == ell.Tag.ToString())
                        {
                            (ui as TextBox).IsEnabled = true;
                            nameNodeToEdit = (ui as TextBox).Text.ToString();
                            isEdit = false;
                            return;
                        }
                    }
                }
                else if (isDeleteVertex) // Выбрана функция Delete vertex                   
                {
                    DeleteVertex(ell);
                }
                else if (isDijkstra)
                {
                    if (!WindowOpened.isOpened)
                    {
                        Window algoRes = new AlgorithmResult(graphAlgorithms.Dijkstra(Convert.ToInt32(ell.Tag)));
                        algoRes.Show();
                        WindowOpened.isOpened = true;
                    }
                    isDijkstra = false;
                }
                else if (isBFS)
                {
                    if (!WindowOpened.isOpened)
                    {
                        Window algoRes = new AlgorithmResult(graphAlgorithms.BFS(Convert.ToInt32(ell.Tag)));
                        algoRes.Show();
                        WindowOpened.isOpened = true;
                    }
                    isBFS = false;
                }
            }
            else // Будем добавлять ребро
            {
                ell.Stroke = Brushes.Red;

                counterChoosenVertexes++;
                if (counterChoosenVertexes == 1)
                {
                    tagVertex1 = Convert.ToInt32(ell.Tag);
                    p1.X = Canvas.GetLeft(ell) + ell.ActualWidth / 2;
                    p1.Y = Canvas.GetTop(ell) + ell.ActualHeight / 2;

                    foreach (Vertex v in vertices) // Меняю координаты первой захваченной точки
                    {
                        if (v.tag == tagVertex1)
                            v.vertex = new Point(p1.X, p1.Y);
                    }
                }
                else if (counterChoosenVertexes == 2)
                {
                    tagVertex2 = Convert.ToInt32(ell.Tag); // Вторая точка (конец в линии)

                    if (tagVertex1 == tagVertex2) // Ребро из точки в точку
                    {
                        united = false;
                        counterChoosenVertexes = 0;
                        tagVertex1 = 0;
                        tagVertex2 = 0;
                        return;
                    }
                    // Если ребро между точками уже существует
                    if (IsEdgeExist(tagVertex1.ToString(), tagVertex2.ToString()))
                    {
                        united = false;
                        counterChoosenVertexes = 0;
                        tagVertex1 = 0;
                        tagVertex2 = 0;
                        return;
                    }

                    p2.X = Canvas.GetLeft(ell) + ell.ActualWidth / 2;
                    p2.Y = Canvas.GetTop(ell) + ell.ActualHeight / 2;

                    foreach (Vertex v in vertices) // Меняю координаты второй захваченной точки
                    {
                        if (v.tag == tagVertex2)
                            v.vertex = new Point(p2.X, p2.Y);
                    }

                    UniteVertexes(tagVertex1, tagVertex2);
                    united = false;
                    counterChoosenVertexes = 0;
                    tagVertex1 = 0;
                    tagVertex2 = 0;

                    conditionsUndo.Pop();
                    adjacencyUndo.Pop();
                    colorsUndo.Pop();
                    isDirectedUndo.Pop();
                    newTagCounterUndo.Pop();
                }
            }
        }

        public void OnEdgeClicked(object sender, MouseButtonEventArgs e)
        {
            if (isEdit) // Изменяю вес ребра
            {
                foreach (UIElement ui in canvas.Children) // Нельзя трогать другие элементы до завершения действия
                {
                    if (ui is Line && (ui as Line).Tag == (sender as Line).Tag)
                        continue;
                    else
                        ui.IsEnabled = false;
                }

                canvas.Background = Brushes.Gray;
                foreach (UIElement ui in canvas.Children)
                {
                    if (ui is TextBox && (ui as TextBox).Tag != null && (ui as TextBox).Tag.ToString() == (sender as Line).Tag.ToString())
                    {
                        (ui as TextBox).IsEnabled = true;
                        isEdit = false;
                        return;
                    }
                }
            }
            else
            {
                foreach (UIElement ui in canvas.Children)
                {
                    if (ui is Line && (ui as Line).Stroke == Brushes.Red)
                    {
                        if ((ui as Line).Tag == (sender as Line).Tag) //  Нажали еще раз на линию
                        {
                            foreach (KeyValuePair<string, Brush> kvp in lineColors.colorsEdges)
                            {
                                if (kvp.Key == (ui as Line).Tag.ToString())
                                {
                                    (ui as Line).Stroke = kvp.Value;
                                    break;
                                }
                            }
                            focusedEdges--;
                            return;
                        }
                    }
                }
                Line l = sender as Line;
                l.Stroke = Brushes.Red;
                focusedEdges++;
            }
            // Сохраняю состояние canvas'a
            conditionsUndo.Push(XamlWriter.Save(canvas));
            // Добавить новый список смежности с ребром
            adjacencyUndo.Push(new List<List<Edge>>(DeepClone<List<List<Edge>>>(graphAlgorithms.adjacencyList)));
            // Добавить в цвета
            lineColors.AddToFile(false);
            colorsUndo.Push(new LineColors(LineColors.DataFromFile()));
            isDirectedUndo.Push(isDirected);
            newTagCounterUndo.Push(newTagCouter);
        }

        public void OnEllipseUp(object sender, MouseButtonEventArgs e)
        {
            isCaptured = false;
            (sender as Ellipse).Cursor = Cursors.Arrow;
            (sender as Ellipse).ReleaseMouseCapture();
            (sender as Ellipse).Stroke = Brushes.Blue;
            foreach (Vertex v in vertices)
            {
                if (v.tag.ToString() == (sender as Ellipse).Tag.ToString())
                {
                    v.vertex = new Point(Canvas.GetLeft(sender as Ellipse), Canvas.GetTop(sender as Ellipse));
                    break;
                }
            }
            if (!united) // Не добавляем ребро
            {
                // Сохраняем состояние canvas'а
                conditionsUndo.Push(XamlWriter.Save(canvas));
                // Добавить новый список смежности с ребром
                adjacencyUndo.Push(new List<List<Edge>>(DeepClone<List<List<Edge>>>(graphAlgorithms.adjacencyList)));
                // Добавить в цвета
                lineColors.AddToFile(false);
                colorsUndo.Push(new LineColors(LineColors.DataFromFile()));
                isDirectedUndo.Push(isDirected);
                newTagCounterUndo.Push(newTagCouter);
            }
        }

        private void OnTextBoxNodeMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            canvas.Background = Brushes.White;
            int res = 0, indexOfEdge = 0;
            foreach (UIElement ui in canvas.Children)
            {
                if (ui is Ellipse && (ui as Ellipse).Tag.ToString() == nameNodeToEdit)
                {
                    break;
                }
                indexOfEdge++;
            }
            foreach (UIElement ch in canvas.Children)
            {
                if (ch is Line)
                {
                    if ((ch as Line).Tag.ToString().Split('_')[0].ToString() == nameNodeToEdit)
                    {
                        foreach (KeyValuePair<string, Brush> kvp in lineColors.colorsEdges)
                        {
                            if (kvp.Key.Split('_')[0].ToString() == nameNodeToEdit)
                            {
                                Brush br = kvp.Value;
                                lineColors.colorsEdges.Remove(kvp.Key);
                                lineColors.colorsEdges.Add((sender as TextBox).Text + "_" + kvp.Key.Split('_')[1].ToString(), br);
                                break;
                            }
                        }
                    }
                    else if ((ch as Line).Tag.ToString()[2].ToString() == nameNodeToEdit)
                    {
                        foreach (KeyValuePair<string, Brush> kvp in lineColors.colorsEdges)
                        {
                            if (kvp.Key.Split('_')[1].ToString() == nameNodeToEdit)
                            {
                                Brush br = kvp.Value;
                                lineColors.colorsEdges.Remove(kvp.Key);
                                lineColors.colorsEdges.Add(kvp.Key.Split('_')[0].ToString() + "_" + (sender as TextBox).Text, br);
                                break;
                            }
                        }
                    }
                }
            }

            bool isInt = Int32.TryParse((sender as TextBox).Text, out res); // Проверка на ввод цифры
            if (!isInt || res < 0)
            {
                // Сохраняю состояние canvas'a
                conditionsUndo.Push(XamlWriter.Save(canvas));
                // Добавить новый список смежности с ребром
                adjacencyUndo.Push(new List<List<Edge>>(DeepClone<List<List<Edge>>>(graphAlgorithms.adjacencyList)));
                // Добавить в цвета
                lineColors.AddToFile(false);
                colorsUndo.Push(new LineColors(LineColors.DataFromFile()));
                isDirectedUndo.Push(isDirected);
                newTagCounterUndo.Push(newTagCouter);
                return;
            }

            for (int i = 0; i < vertices.Count; i++) // Проверка на переименование вершины, которая уже существует
            {
                if (vertices[i].tag == res)
                {
                    Window helper = new AlgoHelper("This edge exists!");
                    helper.Show();
                    canvas.Background = Brushes.Gray;
                    WindowOpened.isOpened = true;

                    // Сохраняю состояние canvas'a
                    conditionsUndo.Push(XamlWriter.Save(canvas));
                    // Добавить новый список смежности с ребром
                    adjacencyUndo.Push(new List<List<Edge>>(DeepClone<List<List<Edge>>>(graphAlgorithms.adjacencyList)));
                    // Добавить в цвета
                    lineColors.AddToFile(false);
                    colorsUndo.Push(new LineColors(LineColors.DataFromFile()));
                    isDirectedUndo.Push(isDirected);
                    newTagCounterUndo.Push(newTagCouter);
                    return;
                }
            }

            (canvas.Children[indexOfEdge] as Ellipse).Tag = res; // Меняем вершине её номер в канвас

            // Меняем теги всех линий в lines bordered
            foreach (Vertex v in vertices)
            {
                if (v.tag.ToString() == nameNodeToEdit)
                {
                    foreach (Line l in v.linesBordered)
                    {
                        string vertex1 = l.Tag.ToString().Split('_')[0],
                        vertex2 = l.Tag.ToString().Split('_')[1];

                        if (vertex1 == nameNodeToEdit)
                        {
                            l.Tag = res + "_" + vertex2;
                        }
                        else if (vertex2 == nameNodeToEdit)
                        {
                            l.Tag = vertex1 + "_" + res;
                        }
                    }
                    break;
                }
            }
            // Меняем вершине её номер в списке вершин
            foreach (Vertex v in vertices)
            {
                if (v.tag.ToString() == nameNodeToEdit)
                {
                    v.tag = res;
                    break;
                }
            }
            // Поменять tag textbox'a на линиях, идущих к вершине
            foreach (UIElement ui in canvas.Children)
            {
                if (ui is TextBox && (ui as TextBox).Tag != null) // Гарантия, что TextBox принадлежит линии 
                {
                    if ((ui as TextBox).Tag.ToString()[0].ToString() == nameNodeToEdit)
                    {
                        (ui as TextBox).Tag = (sender as TextBox).Text + "_" + (ui as TextBox).Tag.ToString()[2];
                    }
                    else if ((ui as TextBox).Tag.ToString()[2].ToString() == nameNodeToEdit)
                    {
                        (ui as TextBox).Tag = (ui as TextBox).Tag.ToString()[0] + "_" + (sender as TextBox).Text;
                    }
                }
            }
            // Меняем теги всех линий, идущих к вершине на канвасе
            foreach (UIElement ui in canvas.Children)
            {
                if (ui is Line)
                {
                    string vertex1 = (ui as Line).Tag.ToString().Split('_')[0],
                        vertex2 = (ui as Line).Tag.ToString().Split('_')[1];
                    if (vertex1 == nameNodeToEdit)
                    {
                        (ui as Line).Tag = res + "_" + vertex2;
                    }
                    else if (vertex2 == nameNodeToEdit)
                    {
                        (ui as Line).Tag = vertex1 + "_" + res;
                    }
                }
            }
            (sender as TextBox).IsEnabled = false;

            // МЕНЯЮ В СПИСКЕ СМЕЖНОСТИ
            foreach (List<Edge> listEdges in graphAlgorithms.adjacencyList)
            {
                foreach (Edge edge in listEdges)
                {
                    if (edge.numberVertexTo == int.Parse(nameNodeToEdit))
                    {
                        edge.numberVertexTo = int.Parse((sender as TextBox).Text);
                        break;
                    }
                }
            }

            while (int.Parse((sender as TextBox).Text) + 1 > graphAlgorithms.adjacencyList.Count)
                graphAlgorithms.adjacencyList.Add(new List<Edge>());

            graphAlgorithms.adjacencyList[int.Parse((sender as TextBox).Text)] = new List<Edge>(graphAlgorithms.adjacencyList[int.Parse(nameNodeToEdit)]);
            graphAlgorithms.adjacencyList[int.Parse(nameNodeToEdit)] = new List<Edge>(); // Освобождаю для вершины с прошлым номером

            // Сохраняю состояние canvas'a
            conditionsUndo.Push(XamlWriter.Save(canvas));
            // Добавить новый список смежности с ребром
            adjacencyUndo.Push(new List<List<Edge>>(DeepClone<List<List<Edge>>>(graphAlgorithms.adjacencyList)));
            // Добавить в цвета
            lineColors.AddToFile(false);
            colorsUndo.Push(new LineColors(LineColors.DataFromFile()));
            isDirectedUndo.Push(isDirected);
            newTagCounterUndo.Push(newTagCouter);
        }

        private void OnTextBoxEdgeMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            foreach (UIElement ui in canvas.Children)
                if ((ui is TextBox) == false)
                    ui.IsEnabled = true;

            canvas.Background = Brushes.White;
            (sender as TextBox).IsEnabled = false;
            // Меняю в списке смежности
            bool fl;
            List<int> used = new List<int>();
            for (int i = 0; i < 2; i++)
            {
                fl = false;
                for (int j = 0; j < graphAlgorithms.adjacencyList.Count; j++)
                {
                    foreach (Edge edges in graphAlgorithms.adjacencyList[j])
                        if (((j.ToString() + "_" + edges.numberVertexTo.ToString() == (sender as TextBox).Tag.ToString()) && !used.Contains(j)) || ((edges.numberVertexTo.ToString() + "_" + j.ToString() == (sender as TextBox).Tag.ToString()) && !used.Contains(j)))
                        {
                            edges.cost = int.Parse((sender as TextBox).Text);
                            fl = true;
                            used.Add(j);
                            break;
                        }
                    if (fl)
                        break;
                }
            }
            // Сохраняю состояние canvas'a
            conditionsUndo.Push(XamlWriter.Save(canvas));
            // Добавить новый список смежности с ребром
            adjacencyUndo.Push(new List<List<Edge>>(DeepClone<List<List<Edge>>>(graphAlgorithms.adjacencyList)));
            // Добавить в цвета
            lineColors.AddToFile(false);
            colorsUndo.Push(new LineColors(LineColors.DataFromFile()));
            isDirectedUndo.Push(isDirected);
            newTagCounterUndo.Push(newTagCouter);
        }

        public void ConnectTextBoxWithVertex(Ellipse ell)
        {
            TextBox textBox = new TextBox();
            textBox.FontFamily = new FontFamily("Bulka");
            textBox.FontSize = 25;
            textBox.Background = ell.Fill;
            textBox.BorderThickness = new Thickness(0);
            textBox.MouseDoubleClick += OnTextBoxNodeMouseDoubleClick;
            textBox.IsEnabled = false;
            textBox.Text = ell.Tag.ToString();
            Canvas.GetLeft(ell);
            Canvas.SetLeft(textBox, Canvas.GetLeft(ell) + 15);
            Canvas.SetTop(textBox, Canvas.GetTop(ell) + 4);
            Canvas.SetZIndex(textBox, 2);
            canvas.Children.Add(textBox);
        }

        private void DeleteVertex(Ellipse e)
        {
            DeleteFromAdjacencyListVertex(e);

            int counterChild = 0;
            foreach (UIElement ui in canvas.Children)
            {
                if (ui is Ellipse && (ui as Ellipse).Tag == e.Tag)
                {
                    canvas.Children.RemoveAt(counterChild); // Удалили выбранную вершину

                    int indexVertex = 0;
                    foreach (Vertex v in vertices) // Получаем индекс точки 
                    {
                        if (v.tag == Convert.ToInt32((ui as Ellipse).Tag))
                        {
                            foreach (UIElement child in canvas.Children)
                            {
                                if (child is TextBox && (child as TextBox).Text == v.tag.ToString())
                                {
                                    canvas.Children.Remove(child as TextBox);
                                    break;
                                }
                            }
                            break;
                        }
                        indexVertex++;
                    }
                    foreach (Line l in vertices[indexVertex].linesBordered) // Находим по тегу
                    {
                        string[] tag = Convert.ToString(l.Tag).Split('_'); // Пару точек, с которой линия граничит получил

                        if (tag[0] == Convert.ToString(vertices[indexVertex].tag) || tag[1] == Convert.ToString(vertices[indexVertex].tag))
                        {
                            if (isDirected) // Для ориентированного графа удаляем вместе с линией стрелки на ней
                            {
                                List<int> counter = new List<int>();
                                int i = 0;
                                int j = 0;
                                int c = 0;
                                bool fl = false;
                                while (true) // Для удаления первой и второй части линии
                                {
                                    i++;
                                    j = 0;
                                    c = 0;
                                    foreach (UIElement child in canvas.Children)
                                    {
                                        if (child is Line && (child as Line).Tag.ToString().Contains('|'))
                                        {
                                            if ((child as Line).Tag.ToString().Split('|')[1] == (l as Line).Tag.ToString() && !counter.Contains(j)) // Часть стрелки лежит на линии
                                            {
                                                counter.Add(j - i);
                                                canvas.Children.Remove(child as Line);
                                                break;
                                            }
                                        }
                                        j++;
                                        c++;
                                        if (c == canvas.Children.Count)
                                        {
                                            fl = true;
                                            break;
                                        }
                                    }
                                        if (fl)
                                            break;
                                }
                            }
                            if (lineColors.colorsEdges.ContainsKey((l as Line).Tag.ToString())) // Удаление цвета
                                lineColors.colorsEdges.Remove((l as Line).Tag.ToString());

                            foreach (UIElement uiEl in canvas.Children) // Удаление textbox с canvas'a
                            {
                                if (uiEl is TextBox && (uiEl as TextBox).Tag != null && (uiEl as TextBox).Tag.ToString() == l.Tag.ToString())
                                {
                                    canvas.Children.Remove(uiEl as TextBox);
                                    break;
                                }
                            }
                            canvas.Children.Remove((l as Line));
                        }
                    }
                    isDeleteVertex = false;

                    // Удаление lines bordered
                    foreach (Vertex v in vertices)
                    {
                        foreach (Line l in v.linesBordered)
                        {
                            if (l.Tag.ToString()[0].ToString() == (ui as Ellipse).Tag.ToString() || l.Tag.ToString()[2].ToString() == (ui as Ellipse).Tag.ToString())
                            {
                                v.linesBordered.Remove(l);
                                break;
                            }
                        }
                    }

                    // Удаление этой вершины со списка
                    foreach (Vertex v in vertices)
                    {
                        if (v.tag.ToString() == (ui as Ellipse).Tag.ToString())
                        {
                            vertices.Remove(v);
                            // Сохраняю состояние canvas'a
                            conditionsUndo.Push(XamlWriter.Save(canvas));
                            // Добавить новый список смежности с ребром
                            adjacencyUndo.Push(new List<List<Edge>>(DeepClone<List<List<Edge>>>(graphAlgorithms.adjacencyList)));
                            // Добавить в цвета
                            lineColors.AddToFile(false);
                            colorsUndo.Push(new LineColors(LineColors.DataFromFile()));
                            isDirectedUndo.Push(isDirected);
                            newTagCounterUndo.Push(newTagCouter);
                            return;
                        }
                    }
                }
                counterChild++;
            }
        }

        private void UniteVertexes(int tagVertex1, int tagVertex2)
        {
            Line line = new Line();
            line.MouseLeftButtonUp += OnEdgeClicked;
            line.Tag = tagVertex1.ToString() + '_' + tagVertex2.ToString();
            GeometryGroup gr = new GeometryGroup();
            Canvas.SetZIndex(line, 1);
            line.X1 = p1.X;
            line.Y1 = p1.Y;
            line.X2 = p2.X;
            line.Y2 = p2.Y;
            line.StrokeThickness = 3;

            if (isDirected)
                arrow.DrawArrow(line, canvas);

            // Один из 5 возможных цветов кисти
            Brush br;
            Random rnd = new Random();
            switch (rnd.Next(0, 5))
            {
                case 0:
                    br = Brushes.DarkOrange;
                    line.Stroke = br;
                    break;
                case 1:
                    br = Brushes.Purple;
                    line.Stroke = br;
                    break;
                case 2:
                    br = Brushes.SkyBlue;
                    line.Stroke = br;
                    break;
                case 3:
                    br = Brushes.IndianRed;
                    line.Stroke = br;
                    break;
                case 4:
                    br = Brushes.LawnGreen;
                    line.Stroke = br;
                    break;
            }
            canvas.Children.Add(line);

            // Добавляем ребро в список смежности
            graphAlgorithms.AddEdgeToAdjacencyList(tagVertex1, tagVertex2, 0, false);

            if (lineColors.colorsEdges.ContainsKey(line.Tag.ToString())) // Если у линии был уже цвет, но иной ==> удаляем значение по данному ключу в словаре
                lineColors.colorsEdges.Remove(line.Tag.ToString());

            lineColors = new LineColors(lineColors);
            lineColors.colorsEdges.Add(line.Tag.ToString(), line.Stroke); // Сохраняем цвет данной линии, чтобы при расфокусировке сохранять цвет


            foreach (Vertex v in vertices)
            {
                if (v.tag == tagVertex1 || v.tag == tagVertex2)
                    v.linesBordered.Add(line);
            }
            ConnectTextBoxWithEdge(line, "0");

            // Сохранить состояние canvas'а
            conditionsUndo.Push(XamlWriter.Save(canvas));
            // Добавить новый список смежности с ребром
            adjacencyUndo.Push(new List<List<Edge>>(DeepClone<List<List<Edge>>>(graphAlgorithms.adjacencyList)));
            // Добавить в цвета
            lineColors.AddToFile(false);
            colorsUndo.Push(new LineColors(LineColors.DataFromFile()));
            isDirectedUndo.Push(isDirected);
            newTagCounterUndo.Push(newTagCouter);
        }

        private void ConnectTextBoxWithEdge(Line line, string weight)
        {
            foreach (UIElement child in canvas.Children) // Для перерисовки textbox над линией при перемещении
            {
                if (child is TextBox && (child as TextBox).Tag != null && (child as TextBox).Tag.ToString() == line.Tag.ToString())
                {
                    canvas.Children.Remove(child as TextBox);
                    break;
                }
            }

            double X3 = (line.X1 + line.X2) / 2;
            double Y3 = (line.Y1 + line.Y2) / 2;

            double d = Math.Sqrt(Math.Pow(line.X2 - line.X1, 2) + Math.Pow(line.Y2 - line.Y1, 2));

            double X = line.X2 - line.X1;
            double Y = line.Y2 - line.Y1;

            double X4 = X3 - (X / d);
            double Y4 = Y3 - (Y / d);

            double Xp = line.Y2 - line.Y1;
            double Yp = line.X1 - line.X2;

            double X5 = X4 + (Xp / d) * 15;
            double Y5 = Y4 + (Yp / d) * 15;

            TextBox textBox = new TextBox();
            textBox.MouseDoubleClick += OnTextBoxEdgeMouseDoubleClick;
            textBox.FontFamily = new FontFamily("Bulka");
            textBox.Tag = line.Tag;
            textBox.IsEnabled = false;
            textBox.BorderThickness = new Thickness(0);
            textBox.Background = Brushes.White;
            textBox.FontSize = 25;
            textBox.Text = weight;
            Canvas.SetLeft(textBox, X5);
            Canvas.SetTop(textBox, Y5);
            Canvas.SetZIndex(canvas, 2);
            canvas.Children.Add(textBox);         
        }

        private bool IsEdgeExist(string tag1, string tag2)
        {
            foreach (UIElement ui in canvas.Children)
            {
                if (ui is Line && ((ui as Line).Tag.ToString() == tag1 + "_" + tag2 || (ui as Line).Tag.ToString() == tag2 + "_" + tag1))
                    return true;
            }
            return false;
        }

        private void DrawEdges(double X2, double Y2, Ellipse e)
        {
            // Находим точку, для которой меняем координаты линий, идущих к ней
            string oldWeight = "";
            if (vertices.Count > 1)
            {
                foreach (Vertex v in vertices)
                {
                    if (v.tag == Convert.ToInt32(e.Tag))
                    {
                        for (int i = 0; i < v.linesBordered.Count; i++)
                        {
                            int counterChild = 0;

                            string[] lineTag = v.linesBordered[i].Tag.ToString().Split('_');

                            if (lineTag[0] == v.tag.ToString()) // Двигаем Начало
                            {
                                foreach (UIElement ui in canvas.Children)
                                {
                                    if (ui is Line)
                                    {
                                        if ((ui as Line).Tag.ToString() == v.linesBordered[i].Tag.ToString())
                                        {
                                            (ui as Line).X1 = X2;
                                            (ui as Line).Y1 = Y2;
                                            canvas.Children[counterChild] = ui as Line;

                                            foreach (UIElement tb in canvas.Children) // Если вес вершины изменён
                                            {
                                                if (tb is TextBox && (tb as TextBox).Tag != null && (tb as TextBox).Tag.ToString() == (ui as Line).Tag.ToString())
                                                {
                                                    oldWeight = ((tb as TextBox).Text);
                                                    break;
                                                }
                                            }

                                            ConnectTextBoxWithEdge(ui as Line, oldWeight); // Перетягивание веса

                                            if (isDirected)
                                                arrow.DrawArrow(ui as Line, true, canvas);
                                            break;
                                        }
                                    }
                                    counterChild++;
                                }
                            }
                            else if (lineTag[1] == v.tag.ToString()) // Двигаем конец
                            {
                                foreach (UIElement ui in canvas.Children)
                                {
                                    if (ui is Line)
                                    {
                                        if ((ui as Line).Tag.ToString() == v.linesBordered[i].Tag.ToString())
                                        {
                                            (ui as Line).X2 = X2;
                                            (ui as Line).Y2 = Y2;
                                            canvas.Children[counterChild] = ui as Line;

                                            foreach (UIElement tb in canvas.Children) // Если вес вершины изменён
                                            {
                                                if (tb is TextBox && (tb as TextBox).Tag != null && (tb as TextBox).Tag.ToString() == (ui as Line).Tag.ToString())
                                                {
                                                    oldWeight = ((tb as TextBox).Text);
                                                    break;
                                                }
                                            }

                                            ConnectTextBoxWithEdge(ui as Line, oldWeight); // Перетягивание веса

                                            if (isDirected)
                                                arrow.DrawArrow(ui as Line, true, canvas);
                                            break;
                                        }
                                    }
                                    counterChild++;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void DeleteEdges()
        {
            DeleteFromAdjacencyListEdges();
            int counter = 0;
            while (counter < focusedEdges)
            {
                foreach (UIElement ui in canvas.Children)
                {
                    if (ui is Line && (ui as Line).Stroke == Brushes.Red)
                    {
                        foreach (Vertex v in vertices)
                        {
                            for (int i = 0; i < v.linesBordered.Count; i++)
                            {
                                if (v.linesBordered[i].Tag == (ui as Line).Tag)
                                {
                                    v.linesBordered.RemoveAt(i);
                                    break;
                                }
                            }
                        }
                        if (isDirected) // Удаление стрелок, если граф ориентирован
                        {
                            int c = 0;
                            while (c != 2)
                            {
                                foreach (UIElement child in canvas.Children)
                                {
                                    if (child is Line && (child as Line).Tag.ToString().Contains('|')) // Гарантия того, что линия принадлежит стрелке
                                    {
                                        if ((child as Line).Tag.ToString().Split('|')[1] == (ui as Line).Tag.ToString())
                                        {
                                            canvas.Children.Remove(child as Line);
                                            c++;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        // Удаление текстбокса, привязанного к линии
                        foreach (UIElement child in canvas.Children)
                        {
                            if (child is TextBox && (child as TextBox).Tag != null && (child as TextBox).Tag.ToString() == (ui as Line).Tag.ToString())
                            {
                                canvas.Children.Remove(child as TextBox);
                                break;
                            }
                        }

                        if (lineColors.colorsEdges.ContainsKey((ui as Line).Tag.ToString()))
                            lineColors.colorsEdges.Remove((ui as Line).Tag.ToString());

                        canvas.Children.Remove(ui as Line);
                        break;
                    }
                }
                counter++;
            }
            focusedEdges = 0;

            // Сохраняю состояние canvas'a
            conditionsUndo.Push(XamlWriter.Save(canvas));
            // Добавить новый список смежности с ребром
            adjacencyUndo.Push(new List<List<Edge>>(DeepClone<List<List<Edge>>>(graphAlgorithms.adjacencyList)));
            // Добавить в цвета
            lineColors.AddToFile(false);
            colorsUndo.Push(new LineColors(LineColors.DataFromFile()));
            isDirectedUndo.Push(isDirected);
            newTagCounterUndo.Push(newTagCouter);
        }

        private void OnShapeMouseMove(object sender, MouseEventArgs e)
        {
            if (isCaptured)
            {
                var mousePos = e.GetPosition(canvas);
                double left = mousePos.X - ((sender as Ellipse).ActualWidth / 2);
                double top = mousePos.Y - ((sender as Ellipse).ActualHeight / 2);
                Canvas.SetLeft((sender as Ellipse), left);
                Canvas.SetTop((sender as Ellipse), top);

                foreach (UIElement ui in canvas.Children) // Перемещение по канвасу № вершины
                {
                    if (ui is TextBox && (ui as TextBox).Tag == null && (ui as TextBox).Text == (sender as Ellipse).Tag.ToString())
                    {
                        Canvas.SetLeft(ui as TextBox, left + 15);
                        Canvas.SetTop(ui as TextBox, top + 4);
                        break;
                    }
                }
                DrawEdges(mousePos.X, mousePos.Y, sender as Ellipse);
            }
        }

        public void MakeUndirected()
        {
            int countArrows = arrow.CountArrows(canvas);

            while (countArrows > 0)
            {
                foreach (UIElement ui in canvas.Children)
                {
                    if (ui is Line)
                    {
                        if ((ui as Line).Tag.ToString().First() == 'A') // Гарантия того, что это линия, принадлежащая стрелке
                        {
                            canvas.Children.Remove(ui as Line); // Удаляем стрелки на линиях
                            break;
                        }
                    }
                }
                countArrows = arrow.CountArrows(canvas);
            }
            // Сохраняю состояние canvas'a
            conditionsUndo.Push(XamlWriter.Save(canvas));
            // Добавить новый список смежности с ребром
            adjacencyUndo.Push(new List<List<Edge>>(DeepClone<List<List<Edge>>>(graphAlgorithms.adjacencyList)));
            // Добавить в цвета
            lineColors.AddToFile(false);
            colorsUndo.Push(new LineColors(LineColors.DataFromFile()));
            isDirectedUndo.Push(isDirected);
            newTagCounterUndo.Push(newTagCouter);
        } // Убираем все стрелки на ребрах

        public void MakeDirected()
        {
            List<string> linesWithArrowTags = new List<string>();
            int countChildren = 0;

            while (countChildren != canvas.Children.Count)
            {
                countChildren = 0;
                foreach (UIElement ui in canvas.Children)
                {
                    if (ui is Line && (ui as Line).Tag.ToString().First() != 'A' && !linesWithArrowTags.Contains((ui as Line).Tag)) // Рисуем стрелку для этой линии
                    {
                        //foreach (UIElement uiEl in canvas.Children)
                        //{
                        //    if (uiEl is Line && (uiEl as Line).Tag.ToString() == (ui as Line).Tag.ToString())
                        //        return;
                        //}
                        arrow.DrawArrow(ui as Line, canvas);
                        linesWithArrowTags.Add((ui as Line).Tag.ToString());
                        break;
                    }
                    countChildren++;
                }
            }
            // Сохраняю состояние canvas'a
            conditionsUndo.Push(XamlWriter.Save(canvas));
            // Добавить новый список смежности с ребром
            adjacencyUndo.Push(new List<List<Edge>>(DeepClone<List<List<Edge>>>(graphAlgorithms.adjacencyList)));
            // Добавить в цвета
            lineColors.AddToFile(false);
            colorsUndo.Push(new LineColors(LineColors.DataFromFile()));
            isDirectedUndo.Push(isDirected);
            newTagCounterUndo.Push(newTagCouter);
        } // Рисуем стрелки на ребрах

        public void MinimalOstov(List<string> minOstov)
        {
            for (int i = 0; i < minOstov.Count; i++)
            {
                string[] tags = minOstov[i].Split('_');
                foreach (UIElement ui in canvas.Children)
                {
                    if ((ui is Line && (ui as Line).Tag.ToString() == tags[0] + "_" + tags[1]) || (ui is Line && (ui as Line).Tag.ToString() == tags[1] + "_" + tags[0]))
                    {
                        (ui as Line).Stroke = Brushes.Red;
                        break;
                    }
                }
            }
            // Сохраняю состояние canvas'a
            conditionsUndo.Push(XamlWriter.Save(canvas));
            // Добавить новый список смежности с ребром
            adjacencyUndo.Push(new List<List<Edge>>(DeepClone<List<List<Edge>>>(graphAlgorithms.adjacencyList)));
            // Добавить в цвета
            lineColors.AddToFile(false);
            colorsUndo.Push(new LineColors(LineColors.DataFromFile()));
            isDirectedUndo.Push(isDirected);
            newTagCounterUndo.Push(newTagCouter);
        }

        public void ReturnColors()
        {
            foreach (UIElement ui in canvas.Children)
            {
                if (ui is Line)
                    foreach (KeyValuePair<string, Brush> kvp in lineColors.colorsEdges)
                    {
                        if (kvp.Key == (ui as Line).Tag.ToString())
                            (ui as Line).Stroke = kvp.Value;
                    }
            }
            // Сохраняю состояние canvas'a
            conditionsUndo.Push(XamlWriter.Save(canvas));
            // Добавить новый список смежности с ребром
            adjacencyUndo.Push(new List<List<Edge>>(DeepClone<List<List<Edge>>>(graphAlgorithms.adjacencyList)));
            // Добавить в цвета
            lineColors.AddToFile(false);
            colorsUndo.Push(new LineColors(LineColors.DataFromFile()));
            isDirectedUndo.Push(isDirected);
            newTagCounterUndo.Push(newTagCouter);
        }

        private void DeleteFromAdjacencyListVertex(Ellipse ell)
        {
            for (int i = 0; i < graphAlgorithms.adjacencyList.Count; i++)
            {
                if (i == Convert.ToInt32(ell.Tag))
                    graphAlgorithms.adjacencyList[i] = new List<Edge>();
                while (true)
                {
                    foreach (Edge edge in graphAlgorithms.adjacencyList[i])
                    {
                        if (edge.numberVertexTo == Convert.ToInt32(ell.Tag))
                        {
                            graphAlgorithms.adjacencyList[i].Remove(edge);
                            break;
                        }
                    }
                    break;
                }
            }
        }

        private void DeleteFromAdjacencyListEdges()
        {
            List<string> numVertexes = new List<string>();
            foreach (UIElement ui in canvas.Children)
                if (ui is Line && (ui as Line).Stroke == Brushes.Red)
                    numVertexes.Add((ui as Line).Tag.ToString());          

            for (int i = 0; i < graphAlgorithms.adjacencyList.Count; i++)
            {
                List<Edge> listEdges = graphAlgorithms.adjacencyList[i];
                int counter = 0, count = listEdges.Count;
                while (true)
                {
                    if (counter == count)
                        break;
                    foreach (Edge edge in listEdges)
                    {
                        if (numVertexes.Contains(i.ToString() + "_" + edge.numberVertexTo.ToString()) || numVertexes.Contains(edge.numberVertexTo.ToString() + "_" + i.ToString()))
                        {
                            listEdges.Remove(edge);
                            break;
                        }
                    }
                    counter++;
                }
            }
        }
    }
}