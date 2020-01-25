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

        public ModelEditor()
        {
            InitializeComponent();
            tbTime.Mask = "00:00:00";
            tbTime.ValueDataType = typeof(string);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LogicEditor.ConnectData();
            }
            catch(Exception)
            {
                MessageBox.Show("Произошла непредвиденная ситуация.", "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
                Mouse.OverrideCursor = Cursors.Arrow;
                return;
            }
            SetParameters();
            BindLokomotives();
            BindStations();
            BindCodes();
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void SetParameters()
        {
            LogicEditor.trainType = 2;
            rbCargoType.IsChecked = true;
            LogicEditor.powerKind = 1;
            rbElectro.IsChecked = true;
            lLokomotive.Content = "Модель электровоза";
            rbCargoType.Checked += (s, e) => rbTrainType_Checked(s, e);
            rbPassType.Checked += (s, e) => rbTrainType_Checked(s, e);
            rbElectro.Checked += (s, e) => rbPower_Checked(s, e);
            rbFuel.Checked += (s,e) => rbPower_Checked(s, e);
        }

        private void BindLokomotives()
        {
            List<Lokomotive> lok = LogicEditor.LoadFreeLokomotives();
            if (lok.Count == 0)
                MessageBox.Show("Нет свободных локомотивов выбранного типа!", "К сведению", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            cbLocomotive.ItemsSource = lok;
            cbLocomotive.SelectedIndex = 1;
        }

        private void BindStations()
        {
                var stations = LogicEditor.LoadStations();
                //Присоединение станций прибытия к combobox
                cbArrivalStation.ItemsSource = stations;
                //Присоединение станций отправления к combobox
                cbDepartureStation.ItemsSource = stations;
                cbArrivalStation.SelectedItem = 1;
                cbDepartureStation.SelectedItem = 2;
        } 
        
        private void BindCodes()
        {
            cbCodes.ItemsSource = LogicEditor.LoadCodes();
            cbCodes.SelectedIndex = 1;
        }

        private void bProcess_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.AppStarting;
                int trainId = LogicEditor.AddTrain(cbLocomotive.SelectedItem as Lokomotive, 
                                                   cbDepartureStation.SelectedItem as Station, 
                                                   cbArrivalStation.SelectedItem as Station);
                MessageBox.Show(String.Format("Поезд №{0} успешно сформирован!", trainId), "Ответ БД", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
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
                lvCargo.Items.Add(LogicEditor.AddCargo(tbCargoName.Text, weight, cost));
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
                LogicEditor.DelCargo(lvCargo.SelectedIndex);
                lvCargo.Items.RemoveAt(lvCargo.SelectedIndex);
            }
            catch(ArgumentNullException)
            {
                MessageBox.Show("Не выбран элемент для удаления", "Будьте внимательней :)", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void bClearCargo_Click(object sender, RoutedEventArgs e)
        {
            LogicEditor.ClearCargos();
            lvCargo.Items.Clear();
        }

        private void rbTrainType_Checked(object sender, RoutedEventArgs e)
        {
            if (rbCargoType.IsChecked == true)
            //Выбран грузовой поезд
            {
                LogicEditor.trainType = 2;
                gbCargos.IsEnabled = true;
            }
            else
            //Выбран пассажирский поезд
            {
                LogicEditor.trainType = 1;
                gbCargos.IsEnabled = false;
            }
            BindLokomotives();
        }

        private void rbPower_Checked(object sender, RoutedEventArgs e)
        {
            if (rbElectro.IsChecked == true)
            //Выбрана электрическая тяга
            {
                LogicEditor.powerKind = 1;
                lLokomotive.Content = "Модель электровоза";
            }
            else
            //Выбрана тепловозная тяга
            {
                LogicEditor.powerKind = 2;
                lLokomotive.Content = "Модель тепловоза";
            }
            BindLokomotives();
        }

        private void tbCargoName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbCargoName.Text == "")
                tbCargoName.Background = new SolidColorBrush(Colors.Transparent);
            else tbCargoName.Background = new SolidColorBrush(Colors.White);
        }

        private void bRefresh_Click(object sender, RoutedEventArgs e)
        {
            lvTrains.ItemsSource = LogicEditor.LoadActualTrains();
        }

        private void bSwitch_Click(object sender, RoutedEventArgs e)
        {
            int sta1 = cbDepartureStation.SelectedIndex;
            cbDepartureStation.SelectedIndex = cbArrivalStation.SelectedIndex;
            cbArrivalStation.SelectedIndex = sta1;
        }

        private void bDeleteTrain_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<int> trainIds = LogicEditor.DelTrains((IList<OperationsView>)lvTrains.SelectedItems);
                MessageBox.Show(String.Format("Поезд(а) №{0} успешно удалены!", String.Join(",", trainIds)), "Ответ БД", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void bCancelOperation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int[] opInfo = LogicEditor.CancelOperation((OperationsView)lvTrains.SelectedItem);
                MessageBox.Show(String.Format("Операция с кодом {0} для поезда №{1} успешно отменена!", opInfo[0], opInfo[1]), "Ответ БД", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch(ArgumentException)
            {
                var mbResult = MessageBox.Show("Вы собираетесь отменить единственную операцию для данного поезда, что приведет" +
                    " к его уданению... Продолжить?", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (mbResult == MessageBoxResult.Yes)
                    bDeleteTrain_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void bInfoOperation_Click(object sender, RoutedEventArgs e)
        {
            /*if (lvTrains.SelectedItems.Count == 0)
                MessageBox.Show("Выделите хотя бы один элемент для отображения информации", "Информация", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            else ...;*/
        }

        private void bAddOperation_Click(object sender, RoutedEventArgs e)
        {
            DateTime dateTime;
            try
            {
                dateTime = ParseDateTime();
            }
            catch (InvalidTimeZoneException)
            {
                return;
            }

            try
            {
                int[] opInfo = LogicEditor.AddOperation(lvTrains.SelectedItem as OperationsView, (Code)cbCodes.SelectedItem, dateTime);
                MessageBox.Show(String.Format("Операция с кодом {0} для поезда №{1} успешно добавлена!", opInfo[0], opInfo[1]), "Ответ БД", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void bUpdateOperation_Click(object sender, RoutedEventArgs e)
        {
            DateTime dateTime;
            try
            {
                dateTime = ParseDateTime();
            }
            catch (InvalidTimeZoneException)
            {
                return;
            }

            try
            {
                int trInfo = LogicEditor.UpdateOperation(lvTrains.SelectedItem as OperationsView, dateTime);
                MessageBox.Show(String.Format("Время последней операции для поезда №{0} успешно обновлено!", trInfo), "Ответ БД", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private DateTime ParseDateTime()
        {
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
                    throw new InvalidTimeZoneException();
                }
            }
            else dateTime = DateTime.Now;
            return dateTime;
        }
    }
}
