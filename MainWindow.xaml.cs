using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Configuration;
using System.Windows.Controls;
using Mannote.Pages;
using System.Windows.Input;
using System.Windows.Media;
using static CodeFirst.Administration;

namespace Mannote
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Page InfoLokomotivesPage;
        Page InfoTrainsPage;
        Page StatisticPage;
        Page MainPage;
        Page EditorPage;

        public MainWindow()
        {
            InitializeComponent();
            Authorization authorization = new Authorization();
            authorization.ShowDialog();
            if (!App.priveleges.FormTrains && !App.priveleges.EditOperations) 
                bEditor.Visibility = Visibility.Collapsed;
            MainPage = new Main();
            CurrentPage.Content = MainPage;
        }

        private void Info_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Name)
            {
                case "bLokInfo":
                {
                    if (InfoLokomotivesPage == null) InfoLokomotivesPage = new Info("lokomotive");
                    CurrentPage.Content = InfoLokomotivesPage;
                }
                break;
                case "bTrainInfo":
                {
                    if (InfoTrainsPage == null) InfoTrainsPage = new Info("train");
                    CurrentPage.Content = InfoTrainsPage;
                }
                break;
            }
        }

        private void Statistic_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            if (StatisticPage == null) StatisticPage = new ModelStatistic();
            CurrentPage.Content = StatisticPage;
        }

        private void Main_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage.Content = MainPage;
        }

        private void ModelEditor_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            if (EditorPage == null) EditorPage = new ModelEditor();
            CurrentPage.Content = EditorPage;
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            string about = "Блокнот руководителя - инструмент поддержки принятия решений для руководства дороги и должностных лиц предприятий железнодорожного транспорта.\n" +
                           "Вопросы и замечания просьба отправлять на почту 3romanski@gmail.com\n\n"+
                           "\tВерсия 1.0 (февраль 2020)\n" +
                           "\tРазработчик - Роман Гривусевич\n";
            MessageBox.Show(about, "О программе", MessageBoxButton.OK);
        }

        private void Expander_MouseEnter(object sender, MouseEventArgs e)
        {
            (sender as Expander).Foreground = Brushes.Red;
        }

        private void Expander_MouseLeave(object sender, MouseEventArgs e)
        {
            (sender as Expander).Foreground = Brushes.White;
        }
    }
}