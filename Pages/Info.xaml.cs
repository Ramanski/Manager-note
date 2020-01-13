using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using RailwayConnectedLayer;

namespace Mannote
{
    /// <summary>
    /// Interaction logic for Info.xaml
    /// </summary>
    public partial class Info : Page
    {
        DataTable uchTable;

        public Info()
        {
            InitializeComponent();
        }

        public void RefreshTrainStatus()
        {
            RailwayDAL railwayDAL = new RailwayDAL();
            railwayDAL.OpenConnection(ConfigurationManager.AppSettings["conStr"]);
            dgTrainStatus.ItemsSource = railwayDAL.GetAllTrainsAsDataTable().DefaultView;
            railwayDAL.CloseConnection();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshTrainStatus();
        }
    }
}
