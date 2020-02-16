using CodeFirst;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;

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
