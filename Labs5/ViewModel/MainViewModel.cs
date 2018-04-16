using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;

namespace Labs5.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase currentViewModel;
        private CompressViewModel compressViewModel = new CompressViewModel();
        private DeCompressViewModel deCompressViewModel = new DeCompressViewModel();
        private string textInputFile;
        private string textInputFileDe;
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            CurrentViewModel = compressViewModel;
            Compress = new RelayCommand(() => CompressExecute());
            OpenDeCompress = new RelayCommand(() => OpenDeCompressExecute());
            OpenCompress = new RelayCommand(() => OpenCompressExecute());
            DeCompress = new RelayCommand(() => DeCompressExecute());
            TextInputFile = "C:\\1.txt";
            textInputFileDe = "C:\\1compress.txt";
        }

        #region Инкапсулированные поля
        public ViewModelBase CurrentViewModel
        {
            get => currentViewModel;
            set
            {
                currentViewModel = value;
                RaisePropertyChanged("CurrentViewModel");
            }
        }

        public string TextInputFile
        {
            get => textInputFile;
            set
            {
                textInputFile = value;
                RaisePropertyChanged("TextInputFile");
            }
        }

        public string TextInputFileDe
        {
            get => textInputFileDe;
            set
            {
                textInputFileDe = value;
                RaisePropertyChanged("TextInputFileDe");
            }
        }

        #endregion

        #region Commands

        public ICommand Compress { get; private set; }
        private void CompressExecute()
        {
            compressViewModel.CompressStart(TextInputFile, "C:\\1compress.txt");
        }

        public ICommand OpenDeCompress { get; private set; }
        private void OpenDeCompressExecute()
        {
            CurrentViewModel = deCompressViewModel;
        }

        public ICommand OpenCompress { get; private set; }
        private void OpenCompressExecute()
        {
            CurrentViewModel = compressViewModel;
        }

        public ICommand DeCompress { get; private set; }
        private void DeCompressExecute()
        {
            deCompressViewModel.DeCompressStart(TextInputFileDe, "C:\\1Decomrpess.txt");
        }
        #endregion
    }
}