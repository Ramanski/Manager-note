﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CodeFirst;
using System.Globalization;
using System.Threading.Tasks;

namespace Mannote.Pages
{
    public partial class ModelEditor : Page
    {
        LogicEditor logicEditor;

        public float weight { get; set; }
        public float cost { get; set; }
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        public ModelEditor()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            InitializeComponent();         
            SetUIElements();
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        // Первоначальная установка элементов
        private void SetUIElements()
        {
            if (!App.priveleges.EditOperations)
            {
                tiEditOperation.Visibility = Visibility.Collapsed;
                tiEditOperation.Content = null;
                tiFormTrain.IsSelected = true;
                logger.Info("Скрыта вкладка редактирования операций");
            }
            if (!App.priveleges.FormTrains)
            {
                tiFormTrain.Visibility = Visibility.Collapsed;
                tiFormTrain.Content = null;
                tiEditOperation.IsSelected = true;
                logger.Info("Скрыта вкладка формирования поездов");
            }
            tbTime.Mask = "00:00:00";
            tbWeight.Text = ".0";
            tbTariff.Text = ".0";
            tbTime.ValueDataType = typeof(string);
            gridCargoDetails.DataContext = this;
        }

        // Подсветка кнопки обновления списка
        private void LightenRefreshButton()
        {
            bRefresh.Background = Brushes.LightCoral;
            bRefresh.BorderBrush = Brushes.OrangeRed;
            ToolTip toolTip = new ToolTip();
            toolTip.Content = "Для актуализации списка требуется его обновить";
            bRefresh.ToolTip = toolTip;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.AppStarting;
            try
            {
                logicEditor = new LogicEditor();
            }
            catch(Exception)
            {
                logger.Error("Ошибка при создании объекта логической модели");
                MessageBox.Show("Произошла непредвиденная ситуация.", "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
                Mouse.OverrideCursor = Cursors.Arrow;
                return;
            }
            SetParameters();
            BindLokomotives();
            BindStations();
            BindCodes();
            Mouse.OverrideCursor = Cursors.Arrow;
            LightenRefreshButton();
        }

        // Установка первоначальных параметров элементов
        private void SetParameters()
        {
            logicEditor.trainType = 2;
            rbCargoType.IsChecked = true;
            logicEditor.powerKind = 1;
            rbElectro.IsChecked = true;
            lLokomotive.Content = "Модель электровоза";
            rbCargoType.Checked += (s, e) => rbTrainType_Checked(s, e);
            rbPassType.Checked += (s, e) => rbTrainType_Checked(s, e);
            rbElectro.Checked += (s, e) => rbPower_Checked(s, e);
            rbFuel.Checked += (s,e) => rbPower_Checked(s, e);
        }

        // Привязка локомотивов к ComboBox
        private async void BindLokomotives()
        {
                List<Lokomotive> lok = await logicEditor.LoadFreeLokomotives();
                if (lok.Count == 0)
                    MessageBox.Show("Нет свободных локомотивов выбранного типа!", "К сведению", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                cbLocomotive.ItemsSource = lok;
                cbLocomotive.SelectedIndex = 0;

        }

        // Привязка станций к ComboBox
        private async void BindStations()
        {
                var stations = await logicEditor.LoadStations();
                //Присоединение станций прибытия к combobox
                cbArrivalStation.ItemsSource = stations;
                //Присоединение станций отправления к combobox
                cbDepartureStation.ItemsSource = stations;
                cbArrivalStation.SelectedIndex = 0;
                cbDepartureStation.SelectedIndex = 1;
        }

        // Привязка кодов операций к ComboBox
        private async void BindCodes()
        {
            cbCodes.ItemsSource = await logicEditor.LoadCodesForOperations();
            cbCodes.SelectedIndex = 0;
        }

        private void bProcess_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.AppStarting;
            try
            {
                int trainId = logicEditor.AddTrain(cbLocomotive.SelectedItem as Lokomotive, 
                                                   cbDepartureStation.SelectedItem as Station, 
                                                   cbArrivalStation.SelectedItem as Station);
                MessageBox.Show(String.Format("Поезд №{0} успешно сформирован!", trainId), "Ответ БД", MessageBoxButton.OK, MessageBoxImage.Information);
                bClearCargo_Click(null, null);
                Task.Run (() => BindLokomotives());
            }
            catch (Exception ex)
            {
                logger.Warn("Не удалось сформировать поезд");
                MessageBox.Show(ex.Message, "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }

        }

        // Добавление груза в список
        private void bAddCargo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                float weight = float.Parse(tbWeight.Text);
                decimal cost = decimal.Parse(tbTariff.Text);
                if (tbCargoName.Text == "")
                    throw new Exception("Введите название груза.");
                lvCargo.Items.Add(logicEditor.AddCargo(tbCargoName.Text, weight, cost));
                tbWeight.Text = "0.0";
                tbTariff.Text = "0.0";
                tbCargoName.Clear();
            }
            catch (FormatException)
            {
                MessageBox.Show("Введены некорректные значения","Ошибка", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        // Удаление груза из списка
        private void bDelCargo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                logicEditor.DelCargo(lvCargo.SelectedIndex);
                lvCargo.Items.RemoveAt(lvCargo.SelectedIndex);
            }
            catch(ArgumentOutOfRangeException)
            {
                MessageBox.Show("Не выбран элемент для удаления", "Будьте внимательней :)", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        // Очистка списка от грузов
        private void bClearCargo_Click(object sender, RoutedEventArgs e)
        {
            logicEditor.ClearCargos();
            lvCargo.Items.Clear();
        }

        private void rbTrainType_Checked(object sender, RoutedEventArgs e)
        {
            if (rbCargoType.IsChecked == true)
            //Выбран грузовой поезд
            {
                logicEditor.trainType = 2;
                gbCargos.IsEnabled = true;
            }
            else
            //Выбран пассажирский поезд
            {
                logicEditor.trainType = 1;
                gbCargos.IsEnabled = false;
            }
            BindLokomotives();
        }

        private void rbPower_Checked(object sender, RoutedEventArgs e)
        {
            if (rbElectro.IsChecked == true)
            //Выбрана электрическая тяга
            {
                logicEditor.powerKind = 1;
                lLokomotive.Content = "Модель электровоза";
            }
            else
            //Выбрана тепловозная тяга
            {
                logicEditor.powerKind = 2;
                lLokomotive.Content = "Модель тепловоза";
            }
            BindLokomotives();
        }

        // Видимость маски "Введите наименование груза..."
        private void tbCargoName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbCargoName.Text == "")
                tbCargoName.Background = new SolidColorBrush(Colors.Transparent);
            else tbCargoName.Background = new SolidColorBrush(Colors.White);
        }

        // Обновление списка поездов
        private void bRefresh_Click(object sender, RoutedEventArgs e)
        {
            lvTrains.ItemsSource = logicEditor.LoadActualTrains();
            bRefresh.Background = Brushes.Transparent;
            bRefresh.BorderBrush = Brushes.LightGray;
            ToolTip toolTip = new ToolTip();
            toolTip.Content = "Обновить список";
            bRefresh.ToolTip = toolTip;
        }

        // Смена направления (станций отправления и назначения)
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
                List<int> trainIds = new List<int>();
                foreach (object selectedTrain in lvTrains.SelectedItems)
                    trainIds.Add(logicEditor.DelTrain((OperationsView)selectedTrain));
                MessageBox.Show(String.Format("Поезд(а) №{0} успешно удалены!", String.Join(",", trainIds)), "Ответ БД", MessageBoxButton.OK, MessageBoxImage.Information);
                lvTrains.SelectedItems.Clear();
                LightenRefreshButton();
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
                int[] opInfo = logicEditor.CancelOperation((OperationsView)lvTrains.SelectedItem);
                MessageBox.Show(String.Format("Операция с кодом {0} для поезда №{1} успешно отменена!", opInfo[0], opInfo[1]), "Ответ БД", MessageBoxButton.OK, MessageBoxImage.Information);
                lvTrains.SelectedItem = null;
                LightenRefreshButton();
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
            try
            {
                int trainId = logicEditor.getTrainId((OperationsView)lvTrains.SelectedItem);
                OperationsInfo op = new OperationsInfo(trainId, logicEditor.getOperations(trainId));
                op.ShowDialog();
                lvTrains.SelectedItem = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                int[] opInfo = logicEditor.AddOperation(lvTrains.SelectedItem as OperationsView, (Code)cbCodes.SelectedItem, dateTime);
                MessageBox.Show(String.Format("Операция с кодом {0} для поезда №{1} успешно добавлена!", opInfo[0], opInfo[1]), "Ответ БД", MessageBoxButton.OK, MessageBoxImage.Information);
                lvTrains.SelectedItem = null;
                LightenRefreshButton();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        // 
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
                int trInfo = logicEditor.UpdateOperation(lvTrains.SelectedItem as OperationsView, dateTime);
                MessageBox.Show(String.Format("Время последней операции для поезда №{0} успешно обновлено!", trInfo), "Ответ БД", MessageBoxButton.OK, MessageBoxImage.Information);
                lvTrains.SelectedItem = null;
                LightenRefreshButton();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработка блока Заменить время
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
