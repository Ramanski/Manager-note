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
using System.Windows.Navigation;
using System.Windows.Shapes;
using RailwayConnectedLayer;
using System.Configuration;

namespace Mannote.Pages
{
    /// <summary>
    /// Interaction logic for ModelEditor.xaml
    /// </summary>
    public partial class ModelEditor : Page
    {
        List<int> stations;

        public ModelEditor()
        {
            InitializeComponent();
            RailwayDAL railwayDAL = new RailwayDAL();
            railwayDAL.OpenConnection(ConfigurationManager.AppSettings["conStr"]);
            stations = railwayDAL.GetStations();
            railwayDAL.CloseConnection();
            cbDepartureStation.ItemsSource = stations;
            cbArrivalStation.ItemsSource = stations;
        }

        private void bProcess_Click(object sender, RoutedEventArgs e)
        {
            RailwayDAL railwayDal = new RailwayDAL();
            railwayDal.OpenConnection(ConfigurationManager.AppSettings["conStr"]);
            railwayDal.InsertTrain(130007, 130100, 1, 1);
            railwayDal.CloseConnection();
        }

        private void bEdit_Click(object sender, RoutedEventArgs e)
        {
            RailwayDAL railwayDal = new RailwayDAL();
            railwayDal.OpenConnection(ConfigurationManager.AppSettings["conStr"]);
            railwayDal.UpdateOperation(201, 201);
            railwayDal.CloseConnection();
        }

        private void bOperate_Click(object sender, RoutedEventArgs e)
        {
            RailwayDAL railwayDal = new RailwayDAL();
            railwayDal.OpenConnection(ConfigurationManager.AppSettings["conStr"]);
            railwayDal.InsertOperation(101, 200);
            railwayDal.CloseConnection();
        }
    }
}
