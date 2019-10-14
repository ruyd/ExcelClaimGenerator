using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ExcelClaimGenerator
{
    public class ClaimLine : INotifyPropertyChanged 
    {
        public string ClaimNumber { get; set; }
        public string ContractNumber { get; set; }
        public string PatientLastName { get; set; }
        public string PatientFirstName { get; set; }
        public string PlaceOfService { get; set; }
        public string Dx1 { get; set; }
        public string Dx2 { get; set; }
        public string Dx3 { get; set; }
        public string Dx4 { get; set; }
        public string Dx5 { get; set; }
        public string Dx6 { get; set; }
        public string Dx7 { get; set; }
        public string RenderProvider { get; set; }
        public string RenderNPI { get; set; }
        public string CPT { get; set; }
        public DateTime? ServiceFrom { get; set; }
        public DateTime? ServiceTo { get; set; }
        public decimal? Tariff { get; set; }

 
        private string _Message;
        public string Message
    {
            get { return _Message; }
            set { _Message = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsError)); }
        }

        private DateTime? _MemberBirthDate; 
        public DateTime? MemberBirthDate
        {
            get { return _MemberBirthDate; }
            set { _MemberBirthDate = value; OnPropertyChanged(); }
        }

        public bool IsError => Message?.ToLower().Contains("error") == true 
            || Message?.ToLower().Contains("missing") == true;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
