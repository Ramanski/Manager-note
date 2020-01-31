using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Mannote
{
    /// <summary>
    /// Interaction logic for Statistic.xaml
    /// </summary>
    public partial class ModelStatistic : Page
    {
        StatisticModel sm;

        public ModelStatistic()
        {
            InitializeComponent();
            sm = new StatisticModel(0, new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1, 18, 0, 0).AddDays(-1) , DateTime.Now);
            SetUIElements();
            UpdateTextBlocks();
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void UpdateTextBlocks()
        {
            string departmentName;
            switch (sm.currentDepartment)
            {
                case 0:
                    departmentName = "всей дороге БЧ";
                    break;
                case 1:
                    departmentName = "Минскому отделению";
                    break;
                case 2:
                    departmentName = "Барановичскому отделению";
                    break;
                case 3:
                    departmentName = "Брестскому отделению";
                    break;
                case 4:
                    departmentName = "Гомельскому отделению";
                    break;
                case 5:
                    departmentName = "Могилевскому отделению";
                    break;
                case 6:
                    departmentName = "Витебскому отделению";
                    break;
                default:
                    departmentName = "???";
                    break;
            }
            tbDepartment.Text = "Показатели работы по "+departmentName;
            tbPeriod.Text = $"с {sm.dtFromTime.ToString("HH:mm d MMM yyyy", CultureInfo.GetCultureInfo("ru-RU"))} по {sm.dtToTime.ToString("HH:mm d MMM yyyy", CultureInfo.GetCultureInfo("ru-RU"))}";
        }

        private void SetUIElements()
        {
            rbNod0.IsChecked = true;
            dpStartPeriod.SelectedDate = sm.dtFromTime;
            dpEndPeriod.SelectedDate = sm.dtToTime;
        }

        private void bCalculate_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.AppStarting;
            try
            {
                dgValues.ItemsSource = sm.CalculateValues();
                UpdateTextBlocks();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }

        private void rbNod_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton selectedRb = (RadioButton)sender;

            switch(selectedRb.Name)
            {
                case "rbNod0":
                    sm.currentDepartment = 0;
                    break;
                case "rbNod1":
                    sm.currentDepartment = 1;
                    break;
                case "rbNod2":
                    sm.currentDepartment = 2;
                    break;
                case "rbNod3":
                    sm.currentDepartment = 3;
                    break;
                case "rbNod4":
                    sm.currentDepartment = 4;
                    break;
                case "rbNod5":
                    sm.currentDepartment = 5;
                    break;
                case "rbNod6":
                    sm.currentDepartment = 6;
                    break;
            }
        }

        private void dpStartPeriod_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void dpEndPeriod_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if(dpEndPeriod.SelectedDate <= dpStartPeriod.SelectedDate)
            {
                ToolTip "Конец периода не может превышать начало отсчета";
            }
            if(dpEndPeriod.SelectedDate > DateTime.Now)
            {
                dpEndPeriod.SelectedDate = DateTime.Now;
            }
        }
    }
}