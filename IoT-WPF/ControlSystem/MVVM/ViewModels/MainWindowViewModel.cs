namespace ControlSystem.MVVM.ViewModels;

public class MainWindowViewModel : ObservableObject
{

    public MainWindowViewModel()
    {
        BedroomViewModel = new BedroomViewModel();

        CurrentView = BedroomViewModel;

        BedroomViewCommand = new RelayCommand(x => { CurrentView = BedroomViewModel; });
    }

    private object _currentView;

    public RelayCommand BedroomViewCommand { get; set; }
    public BedroomViewModel BedroomViewModel { get; set; }


    public object CurrentView
    {
        get { return _currentView; }
        set
        {
            _currentView = value;
            OnPropertyChanged();
        }
    }

}