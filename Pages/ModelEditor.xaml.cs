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
using System.Globalization;

namespace Mannote.Pages
{
    /// <summary>
    /// Interaction logic for ModelEditor.xaml
    /// </summary>
    public partial class ModelEditor : Page
    {
        SampleContext context;
        List<Cargo> cargos; 
        int powerKind = 1;
        int trainType = 2;

        public ModelEditor()
        {
            InitializeComponent();
            // Создать объект контекста
            context = new SampleContext();
            tbTime.Mask = "00:00:00";
            tbTime.ValueDataType = typeof(string);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // !!Построение БД заново!!
           Database.SetInitializer(new DropCreateDatabaseIfModelChanges<SampleContext>());
            //Логгирование запросов к БД
            context.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s));
            SetParameters();
            LoadFreeLokomotives(trainType, powerKind);
            LoadStations();
            LoadCodes();
            cargos = new List<Cargo>();
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void SetParameters()
        {
            rbCargoType.IsChecked = true;
            rbElectro.IsChecked = true;
            lLokomotive.Content = "Модель электровоза";
            rbCargoType.Checked += (s, e) => rbTrainType_Checked(s, e);
            rbPassType.Checked += (s, e) => rbTrainType_Checked(s, e);
            rbElectro.Checked += (s, e) => rbPower_Checked(s, e);
            rbFuel.Checked += (s,e) => rbPower_Checked(s, e);
        }

        private void LoadStations()
        {
            if (context != null)
            {
                //Получение списка станций
                List<Station> stations = context.Stations.OrderBy(s => s.Name).ToList();
                //Присоединение 
                cbArrivalStation.ItemsSource = stations;
                cbDepartureStation.ItemsSource = stations;
                cbArrivalStation.SelectedItem = 1;
                cbDepartureStation.SelectedItem = 2;
            }
        } 
        
        private void LoadFreeLokomotives(int trainType, int powerKind)
        {
            if (context != null)
            {
                //Запрос "свободных" локомотивов со статусом "брошен" или "прибыл"
                var freeLokomotives = context.Lokomotives
                                         .Where(l => (l.Code.CodeId == 204 || l.Code.CodeId == 200) &&
                                                l.PowerKind.PowerKindId == powerKind &&
                                                l.TrainType.TrainTypeId == trainType);
                var lokomotivesList = freeLokomotives.ToList();
                if (lokomotivesList.Count == 0)
                    MessageBox.Show("Нет свободных локомотивов выбранного типа!", "К сведению", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                cbLocomotive.ItemsSource = lokomotivesList;
                
                cbLocomotive.SelectedIndex = 1;
            }
        }

        private void LoadCodes()
        {
            cbCodes.ItemsSource = context.Codes.ToList();
            cbCodes.SelectedIndex = 1;
        }

        private void bProcess_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.AppStarting;
            string MessageText = "Произошла непредвиденная ситуация";

            Train train = new Train
            {
                Lokomotive = cbLocomotive.SelectedItem as Lokomotive,
                Cargos = cargos,
            };

            train.Lokomotive.Code = context.Codes.Where(c => c.CodeId == 9).SingleOrDefault();
            train.Lokomotive.Train = train;

            Operation operation = new Operation
            {
                Code = context.Codes.Where(c => c.CodeId == 9).SingleOrDefault(),
                Date = DateTime.Now
            };

            train.Operations.Add(operation);
            train.LastOperation = operation.Code.Name;

            try 
             {
                int stot = (cbDepartureStation.SelectedItem as Station).StationId;
                int stnz = (cbArrivalStation.SelectedItem as Station).StationId;
                train.Path = context.Paths.Where(p => p.DepartureStation.StationId == stot
                                                 && p.ArriveStation.StationId == stnz
                                                 ).Single();
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
                Mouse.OverrideCursor = Cursors.Arrow;
                return;
            }

            float Weight = 0;
            foreach (Cargo cargo in cargos)
            {
                cargo.Train = train;
                cargo.CostToTransport *= train.Path.Distance;
                Weight += cargo.Weight;
            }

            train.Weight = Weight;

             // Вставить данные в таблицу  с помощью LINQ
             context.Operations.Add(operation);
             context.Cargos.AddRange(cargos);
             context.Trains.Add(train);

            // Сохранить изменения в БД
            try
            {
            context.SaveChanges();
                MessageBox.Show(String.Format("Поезд №{0} успешно сформирован!", train.TrainId), "Ответ БД", MessageBoxButton.OK, MessageBoxImage.Information); 
            }
            catch(System.Data.Entity.Infrastructure.DbUpdateException)
            {
                MessageText = "Произошла ошибка при обновлении записей.";
            }
            catch(System.Data.Entity.Validation.DbEntityValidationException)
            {
                MessageText = "Произошла ошибка при валидации полученных данных.";
            }
            catch(InvalidOperationException)
            {
                MessageText = "Произошла ошибка записи в базу данных.";
            }
            catch(Exception)
            {
                MessageBox.Show(MessageText, "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            finally
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }

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
                cargos.RemoveAt(lvCargo.SelectedIndex);
                lvCargo.Items.RemoveAt(lvCargo.SelectedIndex);
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
            LoadFreeLokomotives(trainType, powerKind);
        }

        private void rbPower_Checked(object sender, RoutedEventArgs e)
        {
            if (rbElectro.IsChecked == true)
            //Выбрана электрическая тяга
            {
                powerKind = 1;
                lLokomotive.Content = "Модель электровоза";
            }
            else
            //Выбрана тепловозная тяга
            {
                powerKind = 2;
                lLokomotive.Content = "Модель тепловоза";
            }
            LoadFreeLokomotives(trainType, powerKind);
        }

        private void tbCargoName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbCargoName.Text == "")
                tbCargoName.Background = new SolidColorBrush(Colors.Transparent);
            else tbCargoName.Background = new SolidColorBrush(Colors.White);
        }

        private void bRefresh_Click(object sender, RoutedEventArgs e)
        {
            //Выборка сведений по всем поездам, кроме расформированных
            var actualTrains = context.Trains
                .Where(t => t.Operations
                            .OrderByDescending(o => o.OperationId)
                            .FirstOrDefault()
                            .Code.CodeId != 205)
                .Select(t => new
                {
                    num = t.TrainId,
                    stot = t.Path.DepartureStation.Name,
                    stnz = t.Path.ArriveStation.Name,
                    oper = t.Operations.FirstOrDefault().Code.Name,
                    time = t.Operations.FirstOrDefault().Date,
                    train = t
                })
                .AsEnumerable()
                .Select(an => new OperationsView
                {
                    stot = an.stot,
                    stnz = an.stnz,
                    oper = an.oper,
                    time = an.time,
                    train = an.train
                }).ToList();
            lvTrains.ItemsSource = actualTrains;
        }

        private void bSwitch_Click(object sender, RoutedEventArgs e)
        {
            int sta1 = cbDepartureStation.SelectedIndex;
            cbDepartureStation.SelectedIndex = cbArrivalStation.SelectedIndex;
            cbArrivalStation.SelectedIndex = sta1;
        }

        private void bDeleteTrain_Click(object sender, RoutedEventArgs e)
        {
            List<Train> deleteTrains = new List<Train>();
            List<Cargo> linkedCargos = new List<Cargo>();
            foreach(OperationsView viewTrain in lvTrains.SelectedItems)
            {
                Train train = viewTrain.train;
                //Удалить связанные грузы и локомотивы
                deleteTrains.Add(train);
                linkedCargos = context.Cargos.Where(c => c.Train == train).ToList();
                if (linkedCargos != null)
                    context.Cargos.RemoveRange(linkedCargos);
                train.Lokomotive.Train = null;
            }
            context.Trains.RemoveRange(deleteTrains);
            context.SaveChanges();
            bRefresh_Click(null, null);
        }

        private void bCancelOperation_Click(object sender, RoutedEventArgs e)
        {
            Train selectedTrain = (lvTrains.SelectedItem as OperationsView).train;
            List<Operation> operations = context.Trains.Where(t => t.TrainId == selectedTrain.TrainId).Select(t => t.Operations).SingleOrDefault();
            if (operations == null)
            {
                MessageBox.Show("Не найдено ни одной операции с поездом", "Ошибка логики БД", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (operations.Count == 1)
                {
                    var mbResult = MessageBox.Show("Вы собираетесь отменить единственную операцию для данного поезда, что приведет" +
                        " к его уданению... Продолжить?", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (mbResult == MessageBoxResult.Yes)
                        bDeleteTrain_Click(null, null);
                    return;
                }
            context.Operations.Remove(operations.Last());
            operations.Remove(operations.Last());
            selectedTrain.LastOperation = operations.Last().Code.Name;
            context.SaveChanges();
            bRefresh_Click(null, null);
        }

        private void TabControl_LostFocus(object sender, RoutedEventArgs e)
        {
            /*if (context.ChangeTracker.HasChanges())
            {
                var result = MessageBox.Show("Сохранить внесенные изменения?", "Подтвердите изменения", MessageBoxButton.YesNoCancel);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        context.SaveChanges();
                    case MessageBoxResult.Cancel:

                    case MessageBoxResult.No:
                }
            }*/
        }

        private void bInfoOperation_Click(object sender, RoutedEventArgs e)
        {
            /*if (lvTrains.SelectedItems.Count == 0)
                MessageBox.Show("Выделите хотя бы один элемент для отображения информации", "Информация", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            else ...;*/
        }

        private void bAddOperation_Click(object sender, RoutedEventArgs e)
        {
            Code code = (Code)cbCodes.SelectedItem;
            Train selectedTrain = (lvTrains.SelectedItem as OperationsView).train;
            DateTime dateTime = new DateTime();

            if (cbDateNow.IsChecked ?? false)
            {
                TimeSpan ts = new TimeSpan();
                if (TimeSpan.TryParseExact(tbTime.Text, "hh\\:mm\\:ss", CultureInfo.CurrentCulture, out ts))
                {
                    dateTime = dpDate.SelectedDate.Value.Add(ts);
                    tbTime.BorderBrush = Brushes.Black;
                    tbTime.BorderThickness = new Thickness(1);
                    tbTime.Background = Brushes.White;
                }
                else
                {
                    tbTime.BorderBrush = Brushes.Red;
                    tbTime.BorderThickness = new Thickness(2);
                    tbTime.Background = Brushes.LightPink;
                    return;
                }
            }
            else dateTime = DateTime.Now;
            selectedTrain.Operations.Add(new Operation { Code = code, Date = dateTime });
            selectedTrain.LastOperation = code.Name;
            context.SaveChanges();
        }

        private void bUpdateOperation_Click(object sender, RoutedEventArgs e)
        {
            Code code = (Code)cbCodes.SelectedItem;
            Train selectedTrain = (lvTrains.SelectedItem as OperationsView).train;
            selectedTrain.Operations.Last().Code = code;
            context.SaveChanges();
        }
    }
}
