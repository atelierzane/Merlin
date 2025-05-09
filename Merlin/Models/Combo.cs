using System.ComponentModel;

namespace MerlinAdministrator.Models
{
    public class Combo : INotifyPropertyChanged
    {
        private bool isSelected;

        public string ComboSKU { get; set; }
        public string ComboName { get; set; }
        public decimal ComboPrice { get; set; }

        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    OnPropertyChanged(nameof(IsSelected)); // Notify UI of change
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
