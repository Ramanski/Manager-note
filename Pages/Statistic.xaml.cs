using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Mannote
{
    /// <summary>
    /// Interaction logic for Statistic.xaml
    /// </summary>
    public partial class Statistic : Page
    {
        SqlCommand myCommand;
        ArrayList arrayList;

        public Statistic()
        {
            InitializeComponent();
        }

        void connectDB()
        {
            // Получение строки подключения и поставщика из *.config
            string provider = ConfigurationManager.AppSettings["provider"];
            string connectionStr = ConfigurationManager.AppSettings["conStr"];
            // Создание открытого подключения
            using (SqlConnection cn = new SqlConnection())
            {
                cn.ConnectionString = connectionStr;
                try
                {
                    //Открыть подключение
                    cn.Open();
                    // Создание объекта команды с помощью конструктора
                    string strSQL = "Select * From [Stations]";
                    myCommand = new SqlCommand(strSQL, cn);
                    //Выполнение запроса
                    SqlDataReader dr = myCommand.ExecuteReader();
                    while (dr.Read())
                        arrayList.Add(dr.GetString(1));
                }
                catch (SqlException ex)
                {
                    // Протоколировать исключение
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    // Гарантировать освобождение подключения
                    cn.Close();
                }
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            arrayList = new ArrayList();
            connectDB();
            comboBox.ItemsSource = arrayList;
        }
    }
}