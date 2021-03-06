﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;
using MahApps.Metro.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Messaging;
using culture = UTH.Infrastructure.Resource.Culture;
using UTH.Infrastructure.Utility;
using UTH.Framework;
using UTH.Framework.Wpf;
using UTH.Domain;
using UTH.Plug;
using UTH.Meeting.Win.View;
using UTH.Meeting.Win.ViewModel;
using System.Diagnostics;

namespace UTH.Meeting.Win
{
    /// <summary>
    /// Startup.xaml 的交互逻辑
    /// </summary>
    public partial class Startup : UTHWindow
    {
        StartupViewModel viewModel;

        public Startup()
        {
            InitializeComponent();
            Initialize();
            this.Loaded += Startup_Loaded;
        }

        private void Initialize()
        {
            viewModel = DataContext as StartupViewModel;
            viewModel.CheckNull();
            viewModel.Configuration(this);
        }

        private void Startup_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel.Start();
        }

    }
}
