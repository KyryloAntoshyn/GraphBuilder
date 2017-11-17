using System.Windows;
using System.Windows.Documents;
using System.IO;
using System.Text;

namespace GraphBuilder
{
    /// <summary>
    /// Interaction logic for HelpWindow.xaml
    /// </summary>
    public partial class HelpWindow : Window
    {
        public HelpWindow()
        {
            InitializeComponent();
        }

        private void richTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            // Считывание с файла
            string help = "", temp = "";
            using (StreamReader fs = new StreamReader("Help.txt", Encoding.GetEncoding(1251)))
            {
                while (true)
                {
                    temp = fs.ReadLine();
                    if (temp == null) break;
                    help += temp + '\n';
                }
            }
            FlowDocument fd = new FlowDocument();
            Paragraph par = new Paragraph();
            par.Inlines.Add(new Bold(new Run(help)));
            fd.Blocks.Add(par);
            richTextBox.Document = fd;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            WindowOpened.isOpened = true;
            this.Close();
        }

        private void helpWnd_Closed(object sender, System.EventArgs e)
        {
            WindowOpened.isOpened = false;
        }
    }
}
