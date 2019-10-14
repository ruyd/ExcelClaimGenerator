using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
 
using OfficeOpenXml;
using OopFactory.X12.Parsing;
 

namespace ExcelClaimGenerator
{
    public partial class ViewModel : INotifyPropertyChanged
    {
 
        private bool _IsError;
        public bool IsError
        {
            get { return _IsError; }
            set { _IsError = value; OnPropertyChanged(); }
        }

        private string _ErrorMessage;
        public string ErrorMessage
        {
            get { return _ErrorMessage; }
            set { _ErrorMessage = value; OnPropertyChanged(); }
        }

        private string _CreateFilter = "A";
        public string CreateFilter
        {
            get { return _CreateFilter; }
            set
            {
                var prev = _CreateFilter;
                _CreateFilter = value;
                OnPropertyChanged();

 
            }
        }
         
  

        public void OnLoad()
        {
            Task.Run(LoadAsync);
        }

        public async Task LoadAsync()
        {
   
              
 
        }

  
        private ObservableCollection<ClaimLine> _ExcelList;
        public ObservableCollection<ClaimLine> ExcelList
        {
            get { if (_ExcelList == null) _ExcelList = new ObservableCollection<ClaimLine>(); return _ExcelList; }
            set
            {
                _ExcelList = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ExcelTotalFiles));
                OnPropertyChanged(nameof(ExcelTotalTariff));
                OnPropertyChanged(nameof(ExcelErrorList));
            }
        }

        public List<ClaimLine> ExcelErrorList => ExcelList.Where(a => a.MemberBirthDate == null).ToList();


        private string _Password;
        public string Password
        {
            get { return _Password; }
            set { _Password = value; OnPropertyChanged(); }
        }

       

        public System.Threading.CancellationTokenSource CancelToken;
 

        private bool _IsLoading;
        public bool IsLoading
        {
            get { return _IsLoading; }
            set
            {
                _IsLoading = value; OnPropertyChanged();
                if (!value)
                    LoadingMessage = null;
            }
        }

        private string _LoadingMessage;
        public string LoadingMessage
        {
            get { return _LoadingMessage; }
            set { _LoadingMessage = value; OnPropertyChanged(); }
        }

        private bool _ServerConnected;
        public bool ServerConnected
        {
            get { return _ServerConnected; }
            set { _ServerConnected = value; OnPropertyChanged(); }
        }


        private bool _ShowExcel = true;
        public bool ShowExcel
        {
            get { return _ShowExcel; }
            set { _ShowExcel = value; OnPropertyChanged(); }
        }


        private bool _IsExporting;
        public bool IsExporting
        {
            get { return _IsExporting; }
            set
            {
                _IsExporting = value;
                IsLoading = value;
                OnPropertyChanged();
            }
        }

 
        public decimal? ExcelTotalTariff => ExcelList.Sum(a => a.Tariff);
        public decimal ExcelTotalFiles => ExcelList.Count > 0 && FileRecordLimit > 0 ? ((ExcelList.Count / FileRecordLimit) > 0 ? (ExcelList.Count / FileRecordLimit) : 1) : 1;


        private int _FileRecordLimit = 1000;
        public int FileRecordLimit
        {
            get { return _FileRecordLimit; }
            set { _FileRecordLimit = value; OnPropertyChanged(); OnPropertyChanged(nameof(ExcelTotalFiles)); }
        }

        private string _FolderDestination;
        public string FolderDestination
        {
            get { return _FolderDestination; }
            set { _FolderDestination = value; OnPropertyChanged(); }
        }

        private string _GoText;
        public string GoText
        {
            get { return _GoText; }
            set { _GoText = value; OnPropertyChanged(); }
        }

        private System.Windows.Media.Color _GoColor;
        public System.Windows.Media.Color GoColor
        {
            get { return _GoColor; }
            set { _GoColor = value; OnPropertyChanged(); }
        }

        private DateTime? _DateFrom;
        public DateTime? DateFrom
        {
            get { return _DateFrom; }
            set { _DateFrom = value; OnPropertyChanged(); OnPropertyChanged(nameof(EnableGet)); }
        }

        private DateTime? _DateTo;
        public DateTime? DateTo
        {
            get { return _DateTo; }
            set { _DateTo = value; OnPropertyChanged(); }
        }

        private DateTime? _CreatedFrom;
        public DateTime? CreatedFrom
        {
            get { return _CreatedFrom; }
            set { _CreatedFrom = value; OnPropertyChanged(); OnPropertyChanged(nameof(EnableGet)); }
        }

        private DateTime? _CreatedTo;
        public DateTime? CreatedTo
        {
            get { return _CreatedTo; }
            set { _CreatedTo = value; OnPropertyChanged(); }
        }

        private int? _RecordFrom;
        public int? RecordFrom
        {
            get { return _RecordFrom; }
            set { _RecordFrom = value; OnPropertyChanged(); OnPropertyChanged(nameof(EnableGet)); }
        }

        private int? _RecordTo;
        public int? RecordTo
        {
            get { return _RecordTo; }
            set { _RecordTo = value; OnPropertyChanged(); }
        }

        public bool EnableGet => CreatedFrom != null || DateFrom != null || RecordFrom > 0  ;

        public RelayCommand ButtonCommand => new RelayCommand(OnButtonCommand);

        public bool CancellationRequested { get; set; }

        private void OnButtonCommand(object obj)
        {
            var s = obj as string;


            if (s == "browse")
            {
                var dlg = new WPFFolderBrowser.WPFFolderBrowserDialog();

                dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                var result = dlg.ShowDialog();
                if (result == true)
                {
                    FolderDestination = dlg.FileName;

                }
            }

 
            if (s == "export.excel")
            {
                Task.Run(() => ExportExcel());
            }

            if (s == "reset")
            {
                DateFrom = null;
                DateTo = null;
                CreatedFrom = null;
                CreatedTo = null;
                RecordFrom = null;
                RecordTo = null;
               // ItemsList.Clear();
                SelectedFile = null;
                Password = null;
                IsError = false;
                ErrorMessage = null;
                ExcelList.Clear();

            }

            if (s == "cancel")
            {
                CancelToken.Cancel();
                CancellationRequested = true;
            }

            if (s == "file")
            {
                IsError = false;
                ErrorMessage = null;

                var dlg = new OpenFileDialog();
                dlg.DefaultExt = ".xlsx";
                dlg.Filter = "Excel Files (.xls)|*.xlsx;*.xls;*.xlsm";
                SelectedFile = dlg.ShowDialog() == true ? dlg.FileName : null;

                if (System.IO.File.Exists(SelectedFile))
                {
                    var result = GetExcel(SelectedFile);
                    if (result.Item1)
                    {
                      
                        ExcelList = new ObservableCollection<ClaimLine>(result.Item2);
                    }

                }
            }
 
        }

        private string _SelectedFile;
        public string SelectedFile
        {
            get { return _SelectedFile; }
            set { _SelectedFile = value; OnPropertyChanged(); }
        }
        //COMMON    
        private void failOut(string s)
        {
            DoProgress(100, s);

            IsError = true;
            ErrorMessage = s;
            OnFinishedInstall();
        }

        public event EventHandler<ProgressEventArgs> ProgressMade;
        public void DoProgress(int i, string s, bool playSound = false)
        {
            //Progress = i;
            //NotifyMessage = s;
            ProgressMade?.Invoke(this, new ProgressEventArgs() { Progress = i, Message = s, PlaySound = playSound });
        }

        public event EventHandler FinishedInstall;
        public void OnFinishedInstall()
        {
            FinishedInstall?.Invoke(this, new EventArgs());
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public Tuple<bool, List<ClaimLine>> GetExcel(string file)
        {
            var results = new List<ClaimLine>();

            try
            {
                var fileInfo = new FileInfo(file);

                using (var xlPackage = new ExcelPackage(fileInfo, Password))
                {
                    // get the first worksheet in the workbook
                    var ws = xlPackage.Workbook.Worksheets.FirstOrDefault();
                    for (int i = 0; i < ws.Dimension.End.Row; i++)
                    {
                        if (i < 1) continue;

                        var row = ws.Row(i + 1);

                        var p = new ClaimLine();

                        p.ClaimNumber = ws.GetValue<string>(row.Row, 1);
                        p.ContractNumber = ws.GetValue<string>(row.Row, 2);
                        p.PatientLastName = ws.GetValue<string>(row.Row, 3);
                        p.PatientFirstName = ws.GetValue<string>(row.Row, 4);
                        p.PlaceOfService = ws.GetValue<string>(row.Row, 5);
                        p.Dx1 = ws.GetValue<string>(row.Row, 6);
                        p.Dx2 = ws.GetValue<string>(row.Row, 7);
                        p.Dx3 = ws.GetValue<string>(row.Row, 8);
                        p.Dx4 = ws.GetValue<string>(row.Row, 9);
                        p.Dx5 = ws.GetValue<string>(row.Row, 10);
                        p.Dx6 = ws.GetValue<string>(row.Row, 11);
                        p.Dx7 = ws.GetValue<string>(row.Row, 12);

                        p.RenderProvider = ws.GetValue<string>(row.Row, 13);
                        p.RenderNPI = ws.GetValue<string>(row.Row, 14);
                        p.CPT = ws.GetValue<string>(row.Row, 15);
                        p.ServiceFrom = ws.GetValue<DateTime?>(row.Row, 16);
                        p.ServiceTo = ws.GetValue<DateTime?>(row.Row, 17);
                        p.Tariff = ws.GetValue<decimal?>(row.Row, 18);

                        if (String.IsNullOrWhiteSpace(p.ClaimNumber)
                   && String.IsNullOrWhiteSpace(p.PatientFirstName)
                   && String.IsNullOrWhiteSpace(p.PatientLastName))
                        {
                            continue;
                        }

                        results.Add(p);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().Contains("password"))
                    ErrorMessage = "Excel file is encrypted. Please enter password before selecting it.";
                else
                    ErrorMessage = ex.Message;

                IsError = true;

                return new Tuple<bool, List<ClaimLine>>(false, null);
            }
            return new Tuple<bool, List<ClaimLine>>(true, results);
        }

 
    }

   

    public class ProgressEventArgs : EventArgs
    {
        public int Progress { get; set; }
        public string Message { get; set; }
        public bool PlaySound { get; set; }
    }

}
