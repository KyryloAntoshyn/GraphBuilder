using System.Collections.Generic;
using System.Windows.Media;
using System.IO;
using System;

namespace GraphBuilder
{
    public class LineColors
    {
        public Dictionary<string, Brush> colorsEdges { get; set; } // Для фокуса рёбер, где string - name линии
        public LineColors()
        {
            colorsEdges = new Dictionary<string, Brush>();
        }
        public LineColors(LineColors lc)
        {
            this.colorsEdges = lc.colorsEdges;
        }
        public void AddToFile(bool fl)
        {
            if (fl) // Сохранение
            {
                string str = "";
                foreach (KeyValuePair<string, Brush> kvp in colorsEdges)
                    str += kvp.Key + '|' + kvp.Value.ToString() + '\n';

                string directory = System.IO.Directory.GetCurrentDirectory() + "\\";
                File.WriteAllText(directory + "saved_colors_" + DateTime.Now.Hour + "_" + DateTime.Now.Minute + "_" + DateTime.Now.Second + ".txt", str); // Записываю канвас в файл
            }
            else
            {
                string directory = System.IO.Directory.GetCurrentDirectory() + "\\undo_colors.txt";
                if (File.Exists(directory))
                    File.Delete(directory);
                string data = "";
                foreach (KeyValuePair<string, Brush> kvp in colorsEdges)
                    data += kvp.Key + ' ' + kvp.Value + '\n';
                using (File.Create(directory)) { }
                File.WriteAllText(directory, data);
            }
        }
        public static LineColors DataFromFile()
        {
            Dictionary<string, Brush> dict = new Dictionary<string, Brush>();
            string[] data = File.ReadAllLines(System.IO.Directory.GetCurrentDirectory() + "\\undo_colors.txt");
            foreach (string s in data)
            {
                string[] tmpData = s.Split(' ');
                switch (tmpData[1])
                {
                    case "#FFFF8C00":
                        dict.Add(tmpData[0], Brushes.DarkOrange);
                        break;
                    case "#FF800080":
                        dict.Add(tmpData[0], Brushes.Purple);
                        break;
                    case "#FF87CEEB":
                        dict.Add(tmpData[0], Brushes.SkyBlue);
                        break;
                    case "#FFCD5C5C":
                        dict.Add(tmpData[0], Brushes.IndianRed);
                        break;
                    case "#FF7CFC00":
                        dict.Add(tmpData[0], Brushes.LawnGreen);
                        break;
                }
            }
            LineColors obj = new LineColors();
            obj.colorsEdges = dict;
            return obj;
        }
        public Dictionary<string, Brush> ReadFromFile(string path)
        {
            Dictionary<string, Brush> colorsEdgesNew = new Dictionary<string, Brush>();
            string[] p = path.Split('_');
            p[2] = "colors";
            string[] data = File.ReadAllLines("saved_" + p[2] + "_" + p[3] + "_" + p[4] + "_" + p[5]);
            foreach (string str in data)
            {
                string[] tmp = str.Split('|');
                switch (tmp[1])
                {
                    case "#FFFF8C00":
                        colorsEdgesNew.Add(tmp[0], Brushes.DarkOrange);
                        break;
                    case "#FF800080":
                        colorsEdgesNew.Add(tmp[0], Brushes.Purple);
                        break;
                    case "#FF87CEEB":
                        colorsEdgesNew.Add(tmp[0], Brushes.SkyBlue);
                        break;
                    case "#FFCD5C5C":
                        colorsEdgesNew.Add(tmp[0], Brushes.IndianRed);
                        break;
                    case "#FF7CFC00":
                        colorsEdgesNew.Add(tmp[0], Brushes.LawnGreen);
                        break;
                }
            }
            return colorsEdgesNew;
        }
    }
}
