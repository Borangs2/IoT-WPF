namespace ControlSystem.MVVM.ViewModels;

public class MainWindowViewModel : ObservableObject
{
    private object _currentView;
    public RelayCommand KitchenViewCommand { get; set; }
    public RelayCommand BedroomViewCommand { get; set; }
    public RelayCommand LivingRoomViewCommand { get; set; }
    public KitchenViewModel KitchenViewModel { get; set; }
    public BedroomViewModel BedroomViewModel { get; set; }
    public LivingRoomViewModel LivingRoomViewModel { get; set; }


    public MainWindowViewModel()
    {
        KitchenViewModel = new KitchenViewModel();
        BedroomViewModel = new BedroomViewModel();
        LivingRoomViewModel = new LivingRoomViewModel();

        KitchenViewCommand = new RelayCommand(x => { CurrentView = KitchenViewModel; });
        BedroomViewCommand = new RelayCommand(x => { CurrentView = BedroomViewModel; });
        LivingRoomViewCommand = new RelayCommand(x => { CurrentView = LivingRoomViewModel; });

        CurrentView = BedroomViewModel;
    }

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