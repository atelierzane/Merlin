using MerlinPointOfSale.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using MerlinPointOfSale.Models;

namespace MerlinPointOfSale.Pages.ReleaseAppointmentsPages
{
    public partial class CalendarPage : Page
    {
        public CalendarPage()
        {
            InitializeComponent();

            subMenuBar.SubMenuItems = new ObservableCollection<string>
            {
                "Month",
                "Week",
                "Day"
            };

        }
      

    }
}