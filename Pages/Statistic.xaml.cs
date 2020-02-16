using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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
            Mouse.OverrideCursor = Cursors.Wait;
            InitializeComponent();
            dgPlan.IsReadOnly = !App.priveleges.EditPlans;
            sm = new StatisticModel();
            SetUIElements();
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
            DateTime[] period = sm.GetPeriod();
            tbPeriod.Text = $"с {period[0].ToString("HH:mm d MMM yyyy", CultureInfo.GetCultureInfo("ru-RU"))} по {period[1].ToString("HH:mm d MMM yyyy", CultureInfo.GetCultureInfo("ru-RU"))}";
        }

        private void SetUIElements()
        {
            rbNod0.IsChecked = true;
            dpStartPeriod.DisplayDateStart = dpEndPeriod.DisplayDateStart = new DateTime(2019, 1, 1);
            dpEndPeriod.DisplayDateEnd = DateTime.Today;
            dpStartPeriod.DisplayDateEnd = DateTime.Today.AddDays(-1);
            dpStartPeriod.SelectedDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddDays(-1);
            dpEndPeriod.SelectedDate = DateTime.Today;
            dpStartPeriod.SelectedDateChanged += (s, e) => dpPeriod_SelectedDateChanged();
            dpEndPeriod.SelectedDateChanged += (s, e) => dpPeriod_SelectedDateChanged();
        }

        private void bCalculate_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                DateTime dtToTime = (dpEndPeriod.SelectedDate.Value == DateTime.Today? DateTime.Now: dpEndPeriod.SelectedDate.Value.AddHours(18));
                DateTime dtFromTime = dpStartPeriod.SelectedDate.Value.AddHours(18);
                sm.SetNewPeriod(dtFromTime, dtToTime);
            }
            catch(ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Будьте внимательней :)", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            Mouse.OverrideCursor = Cursors.AppStarting;
            try
            {
                sm.CalculateValues();
                try
                {
                    if (sm.SetPlanValues())
                    {
                        //Период входит в рамки одного отчетного месяца
                        dgPlan.Visibility = Visibility.Visible;
                        dgPercentage.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        //Период выходит за рамки отчетного месяца
                        dgPlan.Visibility = Visibility.Collapsed;
                        dgPercentage.Visibility = Visibility.Collapsed;
                    }
                }
                catch(ArgumentException ex)
                {
                    MessageBox.Show(ex.Message, "Плановые показатели", MessageBoxButton.OK, MessageBoxImage.Information);
                    dgPlan.Visibility = Visibility.Visible;
                    dgPercentage.Visibility = Visibility.Collapsed;
                }
                dgValues.ItemsSource = sm.GetStatisticValues();
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

        private void dpPeriod_SelectedDateChanged()
        {
            if (dpStartPeriod.SelectedDate < dpEndPeriod.SelectedDate)
            {
                dpEndPeriod.Background = dpStartPeriod.Background = Brushes.White;
            }
            else
            {
                dpEndPeriod.Background = dpStartPeriod.Background = Brushes.LightCoral;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void dgValues_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            StatisticValue sv = (StatisticValue)e.Row.DataContext;
            if (sv.percentage < 1 && sv.percentage > 0)
                e.Row.Background = Brushes.Gold;
            else e.Row.Background = Brushes.White;
        }

        private void bPlanSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                sm.SavePlanValues();
                MessageBox.Show($"Плановые показатели за {sm.GetPeriod()[1].ToString("MMMM yyyy года", CultureInfo.GetCultureInfo("ru-RU"))} успешно установлены.");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgValues_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            float val;
            var editedTextBox = (TextBox) e.EditingElement;
            if (editedTextBox != null)
            {
                float.TryParse(editedTextBox.Text, out val);
                if (val < 0)
                    e.Cancel = true;
                else
                {
                    bPlanSave.IsEnabled = true;
                }
            }
        }
    }
}