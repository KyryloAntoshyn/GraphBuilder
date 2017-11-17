using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
namespace GraphBuilder
{
    public class Arrow // Постройка стрелки к линии (Контейнер)
    {
        public double X3 { get; set; }
        public double Y3 { get; set; }
        public double X5 { get; set; }
        public double Y5 { get; set; }
        public double X6 { get; set; }
        public double Y6 { get; set; }

        public Arrow()
        {
            X3 = 0;
            Y3 = 0;
            X5 = 0;
            Y5 = 0;
            X6 = 0;
            Y6 = 0;
        }

        public void DrawArrow(Line line, bool f, Canvas canvas) // Двигаем стрелку
        {
            int countArrowLines = 0;
            int indexArrowLine = -1111;
            while (countArrowLines != 2)
            {
                foreach (UIElement child in canvas.Children)
                {
                    if (child is Line && (child as Line).Tag.ToString() == "A|" + line.Tag && canvas.Children.IndexOf(child as Line) != indexArrowLine) // Гарантия того, что эта линия принадлежит стрелке на ребре, которое двигаем
                    {
                        indexArrowLine = canvas.Children.IndexOf(child as Line);

                        //Изменение координат и замещение старой линии-стрелки
                        double X3 = (line.X1 + line.X2) / 2; // Середина вектора
                        double Y3 = (line.Y1 + line.Y2) / 2;

                        double d = Math.Sqrt(Math.Pow(line.X2 - line.X1, 2) + Math.Pow(line.Y2 - line.Y1, 2)); // Длина отрезка

                        double X = line.X2 - line.X1; // Длина отрезка
                        double Y = line.Y2 - line.Y1;

                        double X4 = X3 - (X / d) * 10; // Точка, удаленная на 10 пикс
                        double Y4 = Y3 - (Y / d) * 10;

                        double Xp = line.Y2 - line.Y1; // Координаты перпендикуляра
                        double Yp = line.X1 - line.X2;

                        double X5 = X4 + (Xp / d) * 5;
                        double Y5 = Y4 + (Yp / d) * 5;
                        double X6 = X4 - (Xp / d) * 5;
                        double Y6 = Y4 - (Yp / d) * 5;

                        switch (countArrowLines)
                        {
                            case 0: // Прорисовка левого конца стрелки
                                (child as Line).X1 = X3;
                                (child as Line).Y1 = Y3;
                                (child as Line).X2 = X5;
                                (child as Line).Y2 = Y5;
                                break;
                            case 1: // Прорисовка правого конца стрелки
                                (child as Line).X1 = X3;
                                (child as Line).Y1 = Y3;
                                (child as Line).X2 = X6;
                                (child as Line).Y2 = Y6;
                                break;
                        }

                        countArrowLines++;
                        break;
                    }
                }
            }

        } // Без изминения старых стрелок

        public void DrawArrow(Line line, Canvas canvas)
        {
            //Рисую стрелку
            double X3 = (line.X1 + line.X2) / 2; // Середина вектора
            double Y3 = (line.Y1 + line.Y2) / 2;

            double d = Math.Sqrt(Math.Pow(line.X2 - line.X1, 2) + Math.Pow(line.Y2 - line.Y1, 2)); // Длина отрезка

            double X = line.X2 - line.X1; // Длина отрезка
            double Y = line.Y2 - line.Y1;

            double X4 = X3 - (X / d) * 10; // Точка, удаленная на 10 пикс
            double Y4 = Y3 - (Y / d) * 10;

            double Xp = line.Y2 - line.Y1; // Координаты перпендикуляра
            double Yp = line.X1 - line.X2;

            double X5 = X4 + (Xp / d) * 5;
            double Y5 = Y4 + (Yp / d) * 5;
            double X6 = X4 - (Xp / d) * 5;
            double Y6 = Y4 - (Yp / d) * 5;

            Line lineArr1 = new Line();
            lineArr1.Tag = "A" + '|' + line.Tag;
            lineArr1.X1 = X3;
            lineArr1.Y1 = Y3;
            lineArr1.X2 = X5;
            lineArr1.Y2 = Y5;
            lineArr1.Stroke = System.Windows.Media.Brushes.Black;
            lineArr1.StrokeThickness = 3;
            canvas.Children.Add(lineArr1);

            Line lineArr2 = new Line();
            lineArr2.Tag = "A" + '|' + line.Tag;
            lineArr2.X1 = X3;
            lineArr2.Y1 = Y3;
            lineArr2.X2 = X6;
            lineArr2.Y2 = Y6;
            lineArr2.Stroke = System.Windows.Media.Brushes.Black;
            lineArr2.StrokeThickness = 3;
            canvas.Children.Add(lineArr2);
        }

        public int CountArrows(Canvas canvas)
        {
            int c = 0;
            foreach (UIElement ui in canvas.Children)
            {
                if (ui.GetType() == typeof(Line))
                {
                    if ((ui as Line).Tag.ToString().First() == 'A')
                        c++;
                }
            }
            return c;
        }     
    }
}
