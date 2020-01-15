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
using CodeFirst;
using System.Data.Entity;

namespace Mannote.Pages
{
    /// <summary>
    /// Interaction logic for ModelEditor.xaml
    /// </summary>
    public partial class ModelEditor : Page
    {
        public ModelEditor()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<SampleContext>());

                      
            PowerKind powerKind = new PowerKind();
            powerKind.Name = "На Божьем слове";

            TrainType trainType = new TrainType();
            trainType.Name = "Бомжевоз";

            Code code = new Code();
            code.Name = "Послан";

            Operation operation = new Operation();
            operation.Code = code;
            operation.Date = DateTime.Now.AddDays(-10);

            Station Stot = new Station();
            Stot.Name = "ОтВерблюда";
            Stot.Department = 0;

            Station Stnz = new Station();
            Stnz.Name = "Караганда";
            Stnz.Department = 6;

            CodeFirst.Path path = new CodeFirst.Path();
            path.PathId = 3;
            path.DepartureStation = Stot;
            path.ArriveStation = Stnz;
            path.Distance = 999;

            Lokomotive lokomotive = new Lokomotive();
            lokomotive.Model = "Тапок";
            lokomotive.PowerKind = powerKind;
            lokomotive.TrainType = trainType;

            Train train = new Train();
            train.Lokomotive = lokomotive;
            train.Path = path;
            train.Operations = new List<Operation>();
            train.Operations.Add(operation);

            Cargo cargo = new Cargo();
            cargo.Name = "Чай з малинавым варэннем";
            cargo.Weight = 0.5f;
            cargo.CostToTransport = 50 * path.Distance;
            train.Cargos = new List<Cargo>();
            train.Cargos.Add(cargo);
            

            // Создать объект контекста
            SampleContext context = new SampleContext();

            // Вставить данные в таблицу Customers с помощью LINQ
            context.PowerKinds.Add(powerKind);
            context.TrainTypes.Add(trainType);
            context.Codes.Add(code);
            context.Operations.Add(operation);
            context.Stations.Add(Stot);
            context.Stations.Add(Stnz);
            context.Paths.Add(path);
            context.Lokomotives.Add(lokomotive);
            context.Cargos.Add(cargo);
            context.Trains.Add(train);

            // Сохранить изменения в БД
            context.SaveChanges();
        }
    }
}
