﻿using System;
using System.Windows;
using System.Windows.Controls;

namespace Mannote
{
    /// <summary>
    /// Interaction logic for Info.xaml
    /// </summary>
    public partial class Info : Page
    {
        string selectedView;
        string viewName;
        RailwayDBConnector rwView;

        public Info(string viewObject)
        {
            InitializeComponent();
            switch (viewObject)
            {
                case "lokomotive": 
                    selectedView = "LokomotivesStatus";
                    viewName = "локомотивам";
                    break;
                case "train": 
                    selectedView = "TrainsStatus";
                    viewName = "поездам"; 
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            rwView = new RailwayDBConnector();
            bRefresh_Click(null, null);
        }

        private void bRefresh_Click(object sender, RoutedEventArgs e)
        {
            dgStatus.ItemsSource = rwView.GetViewTable(selectedView).DefaultView;
            tbViewName.Text = $"Справка по {viewName} на {DateTime.Now.ToString("HH: mm: ss")}";
        }
    }
}
