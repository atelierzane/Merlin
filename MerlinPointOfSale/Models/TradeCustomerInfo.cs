using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinPointOfSale.Models
{
    public class TradeCustomerInfo : INotifyPropertyChanged
    {
        private string firstName;
        private string lastName;
        private string emailAddress;
        private string phoneNumber;
        private string streetAddress;
        private string city;
        private string state;
        private string zip;
        private string idType;
        private string issuingAuthority;
        private string documentNumber;
        private DateTime documentIssueDate;
        private DateTime documentExpirationDate;
        private DateTime birthDate;
        private string sex;
        private string ethnicity;
        private string hairColor;
        private string eyeColor;
        private int heightFeet;
        private int heightInches;
        private int weight;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string FirstName
        {
            get => firstName;
            set { firstName = value; OnPropertyChanged(nameof(FirstName)); }
        }

        public string LastName
        {
            get => lastName;
            set { lastName = value; OnPropertyChanged(nameof(LastName)); }
        }

        public string EmailAddress
        {
            get => emailAddress;
            set { emailAddress = value; OnPropertyChanged(nameof(EmailAddress)); }
        }

        public string PhoneNumber
        {
            get => phoneNumber;
            set { phoneNumber = value; OnPropertyChanged(nameof(PhoneNumber)); }
        }

        public string StreetAddress
        {
            get => streetAddress;
            set { streetAddress = value; OnPropertyChanged(nameof(StreetAddress)); }
        }

        public string City
        {
            get => city;
            set { city = value; OnPropertyChanged(nameof(City)); }
        }

        public string State
        {
            get => state;
            set { state = value; OnPropertyChanged(nameof(State)); }
        }

        public string Zip
        {
            get => zip;
            set { zip = value; OnPropertyChanged(nameof(Zip)); }
        }

        public string IDType
        {
            get => idType;
            set { idType = value; OnPropertyChanged(nameof(IDType)); }
        }

        public string IssuingAuthority
        {
            get => issuingAuthority;
            set { issuingAuthority = value; OnPropertyChanged(nameof(IssuingAuthority)); }
        }

        public string DocumentNumber
        {
            get => documentNumber;
            set { documentNumber = value; OnPropertyChanged(nameof(DocumentNumber)); }
        }

        public DateTime DocumentIssueDate
        {
            get => documentIssueDate;
            set { documentIssueDate = value; OnPropertyChanged(nameof(DocumentIssueDate)); }
        }

        public DateTime DocumentExpirationDate
        {
            get => documentExpirationDate;
            set { documentExpirationDate = value; OnPropertyChanged(nameof(DocumentExpirationDate)); }
        }

        public DateTime BirthDate
        {
            get => birthDate;
            set { birthDate = value; OnPropertyChanged(nameof(BirthDate)); }
        }

        public string Sex
        {
            get => sex;
            set { sex = value; OnPropertyChanged(nameof(Sex)); }
        }

        public string Ethnicity
        {
            get => ethnicity;
            set { ethnicity = value; OnPropertyChanged(nameof(Ethnicity)); }
        }

        public string HairColor
        {
            get => hairColor;
            set { hairColor = value; OnPropertyChanged(nameof(HairColor)); }
        }

        public string EyeColor
        {
            get => eyeColor;
            set { eyeColor = value; OnPropertyChanged(nameof(EyeColor)); }
        }

        public int HeightFeet
        {
            get => heightFeet;
            set { heightFeet = value; OnPropertyChanged(nameof(HeightFeet)); }
        }

        public int HeightInches
        {
            get => heightInches;
            set { heightInches = value; OnPropertyChanged(nameof(HeightInches)); }
        }

        public int Weight
        {
            get => weight;
            set { weight = value; OnPropertyChanged(nameof(Weight)); }
        }
    }
}
