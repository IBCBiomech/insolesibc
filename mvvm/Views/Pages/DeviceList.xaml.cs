﻿using System;
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
using Wpf.Ui.Common.Interfaces;

namespace mvvm.Views.Pages
{
    /// <summary>
    /// Lógica de interacción para DeviceList.xaml
    /// </summary>
    public partial class DeviceListPage : INavigableView<ViewModels.DeviceListViewModel>
    {
        public ViewModels.DeviceListViewModel ViewModel
        {
            get;
        }
        public DeviceListPage(ViewModels.DeviceListViewModel viewModel)
        {
            ViewModel = viewModel;

            InitializeComponent();
        }
    }
}