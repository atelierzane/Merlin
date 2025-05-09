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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using MerlinPointOfSale.Pages;
using MerlinPointOfSale.Controls;

namespace MerlinPointOfSale.Controls
{
    public partial class LoadingAnimation : UserControl
    {
        public LoadingAnimation()
        {
            InitializeComponent();
            Loaded += LoadingAnimation_Loaded;
        }

        private void LoadingAnimation_Loaded(object sender, RoutedEventArgs e)
        {
            var storyboard = (Storyboard)FindResource("BounceAnimation");
            storyboard.Begin(this, true); // Start the storyboard
        }
    }
}
