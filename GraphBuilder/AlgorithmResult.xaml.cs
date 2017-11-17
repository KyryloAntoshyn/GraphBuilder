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
    /// <summary>
    /// Interaction logic for AlgorithmResult.xaml
    /// </summary>
    public partial class AlgorithmResult : Window
    {
        public AlgorithmResult(string str)
        {
            InitializeComponent();
            FlowDocument fd = new FlowDocument();
            Paragraph par = new Paragraph();
            par.Inlines.Add(new Bold(new Run(str)));
            fd.Blocks.Add(par);
            richTextBoxResult.Document = fd;
        }

        private void Window_Closed(object sender, EventArgs e)
        { 
            WindowOpened.isOpened = false;
        }

        private void richTextBoxResult_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
