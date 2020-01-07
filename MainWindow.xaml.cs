using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Configuration;
using System.Windows.Controls;
using Mannote.Pages;

namespace Mannote
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Page InfoPage;
        Page StatisticPage;
        Page MainPage;
        Page DocumentsPage;

        public MainWindow()
        {
            InitializeComponent();
            MainPage = new Main();
            CurrentPage.Content = MainPage;
        }

        private void Info_Click(object sender, RoutedEventArgs e)
        {
            if (InfoPage == null) InfoPage = new Info();
            CurrentPage.Content = InfoPage;
        }

        private void Statistic_Click(object sender, RoutedEventArgs e)
        {
            if (StatisticPage == null) StatisticPage = new Statistic();
            CurrentPage.Content = StatisticPage;
        }

        private void Document_Click(object sender, RoutedEventArgs e)
        {
            if (DocumentsPage == null) DocumentsPage = new Document();
            CurrentPage.Content = DocumentsPage;
        }

        private void Main_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage.Content = MainPage;
        }
    }
}