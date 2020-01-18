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
        SampleContext context;
        IQueryable<Lokomotive> freeLokomotives;
        List<Station> stations;
        List<Cargo> cargos;
        int trainType = 0;
        int powerKind = 0;

        public ModelEditor()
        {
            InitializeComponent();
            // Создать объект контекста
//            context = new SampleContext();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // !!Построение БД заново!!
            //Database.SetInitializer(new DropCreateDatabaseIfModelChanges<SampleContext>());        
            
            //Логгирование запросов к БД
//            context.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s));
            SetParameters();
            LoadStations();
            cargos = new List<Cargo>();
        }

        private void SetParameters()
        {
            rbCargoType.IsChecked = true;
            rbElectro.IsChecked = true;
        }

        private void LoadStations()
        {
            if (context != null)
            {
                //Получение списка станций
                stations = context.Stations.OrderBy(s => s.Name).ToList();
                //Присоединение 
                cbArrivalStation.ItemsSource = stations;
                cbDepartureStation.ItemsSource = stations;
                cbArrivalStation.SelectedItem = 10;
                cbDepartureStation.SelectedItem = 14;
            }
        } 
        
        private void LoadFreeLokomotives()
        {
            if (context != null)
            {
                //Запрос "свободных" локомотивов со статусом "брошен" или "прибыл"
                freeLokomotives = context.Lokomotives
                                         .Where(l => (l.Code.CodeId == 204 || l.Code.CodeId == 200) &&
                                                l.PowerKind.PowerKindId == powerKind &&
                                                l.TrainType.TrainTypeId == trainType);
                cbLocomotive.ItemsSource = freeLokomotives.ToList();
                cbLocomotive.SelectedIndex = 1;
            }
        }

        private void bProcess_Click(object sender, RoutedEventArgs e)
        {
             string MessageText = "Произошла непредвиденная ситуация";
             Operation operation = new Operation();
             operation.Code = context.Codes.Where(c=> c.CodeId == 9).SingleOrDefault();
             operation.Date = DateTime.Now;

             Train train = new Train();
             train.Lokomotive = cbLocomotive.SelectedItem as Lokomotive;
             try 
             {
                train.Path = context.Paths.Where(p => p.DepartureStation == (cbDepartureStation.SelectedItem as Station) 
                                                    && p.ArriveStation == (cbArrivalStation.SelectedItem as Station)).Single();
             }
            catch (ArgumentNullException)
            {
                MessageText = "Не удалось найти маршрут по выбанным станциям";
            }
            catch (InvalidOperationException)
            {
                MessageText = "Найдено несколько вариантов маршрута по выбанным станциям";
            }
            catch(Exception)
            {
                MessageBox.Show(MessageText, "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);  
            }

             train.Operations = new List<Operation>();
             train.Operations.Add(operation);

             train.Cargos = cargos;
            train.Lokomotive.Code.CodeId = 9;

             // Вставить данные в таблицу  с помощью LINQ
             context.Operations.Add(operation);
             context.Cargos.AddRange(cargos);
             context.Trains.Add(train);

             // Сохранить изменения в БД
             context.SaveChanges();
        }

        private void bAddCargo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                float weight = float.Parse(tbWeight.Text);
                decimal cost = decimal.Parse(tbTariff.Text);
                Cargo cargo = new Cargo(tbCargoName.Text, weight, cost);
                cargos.Add(cargo);
                lvCargo.Items.Add(cargo);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                tbWeight.Clear();
                tbTariff.Clear();
                tbCargoName.Clear();
            }
        }

        private void bDelCargo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                lvCargo.Items.RemoveAt(lvCargo.SelectedIndex);
                cargos.RemoveAt(lvCargo.SelectedIndex);
            }
            catch(ArgumentOutOfRangeException)
            {
                MessageBox.Show("Не выбран элемент для удаления", "Будьте внимательней :)", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void bClearCargo_Click(object sender, RoutedEventArgs e)
        {
            cargos.Clear();
            lvCargo.Items.Clear();
        }

        private void rbTrainType_Checked(object sender, RoutedEventArgs e)
        {
            if (rbCargoType.IsChecked == true)
            //Выбран грузовой поезд
            {
                trainType = 2;
                gbCargos.IsEnabled = true;
            }
            else
            //Выбран пассажирский поезд
            {
                trainType = 1;
                gbCargos.IsEnabled = false;
            }
            LoadFreeLokomotives();
        }

        private void rbPower_Checked(object sender, RoutedEventArgs e)
        {
            if (rbElectro.IsChecked == true)
            //Выбрана электрическая тяга
            {
                powerKind = 1;
                lLokomotive.Content = "Модель электровоза";
            }
            //Выбрана тепловозная тяга
            else
            {
                powerKind = 2;
                lLokomotive.Content = "Модель тепловоза";
            }
            LoadFreeLokomotives();
        }

        private void tbCargoName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbCargoName.Text == "")
                tbCargoName.Background = new SolidColorBrush(Colors.Transparent);
            else tbCargoName.Background = new SolidColorBrush(Colors.White);
        }
    }
}
