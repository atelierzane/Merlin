using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MerlinPointOfSale.Models
{
    public class CalendarDate : INotifyPropertyChanged
    {
        public DateTime Date { get; set; }
        public int Day => Date.Day;

        private ObservableCollection<Appointment> appointments;
        public ObservableCollection<Appointment> Appointments
        {
            get => appointments;
            set
            {
                appointments = value;
                OnPropertyChanged(nameof(Appointments));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
