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
    /// Interaction logic for AlgoHelper.xaml
    /// </summary>
    public partial class AlgoHelper : Window
    { 
        public AlgoHelper(string help)
        {
            InitializeComponent();
            textBoxHelp.Text = help;
        }

        private void wndHelper_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WindowOpened.isOpened = false;
            Graph.isReturnColors = true;
        }
    }
}
