using CodeFirst;
using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace Mannote
{
    /// <summary>
    /// Interaction logic for OperationsInfo.xaml
    /// </summary>
    public partial class OperationsInfo : Window
    {
        public OperationsInfo(int TrainId, List<Operation> operations)
        {
            InitializeComponent();
            wOper.Title += TrainId;
            tbOperations.Text = formText(operations);
        }

        private string formText(List<Operation> operations)
        {
            StringBuilder sb = new StringBuilder();
            foreach(Operation op in operations)
            {
                DateTime dateTime = op.Date;
                sb.Append($"{op.Code} {dateTime.ToString("dd MMM", CultureInfo.GetCultureInfo("ru-RU"))} в {dateTime.ToString("HH:mm:ss")}");
                sb.Append('\n');
            }
            return sb.ToString();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
