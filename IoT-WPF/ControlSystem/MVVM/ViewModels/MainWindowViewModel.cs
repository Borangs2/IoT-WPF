namespace ControlSystem.MVVM.ViewModels;

public class MainWindowViewModel : ObservableObject
{

    public MainWindowViewModel()
    {
        BedroomViewModel = new BedroomViewModel();
        KitchenViewModel = new BedroomViewModel();
        LivingRoomViewModel = new BedroomViewModel();

        CurrentView = BedroomViewModel;

        BedroomViewCommand = new RelayCommand(x => { CurrentView = BedroomViewModel; });
        KitchenViewCommand = new RelayCommand(x => { CurrentView = KitchenViewModel; });
        LivingRoomViewCommand = new RelayCommand(x => { CurrentView = LivingRoomViewModel; });
    }

    private object _currentView;

    public RelayCommand BedroomViewCommand { get; set; }
    public BedroomViewModel BedroomViewModel { get; set; }
    public RelayCommand KitchenViewCommand { get; set; }
    public BedroomViewModel KitchenViewModel { get; set; }
    public RelayCommand LivingRoomViewCommand { get; set; }
    public BedroomViewModel LivingRoomViewModel { get; set; }


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