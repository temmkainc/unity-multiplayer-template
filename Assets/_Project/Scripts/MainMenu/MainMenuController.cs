using Networking;
using Zenject;

public class MainMenuController
{
    [Inject] private INetworkRunnerService _networkService;

    public void OnHostButtonClicked()
    {
        if (_networkService != null)
        {
            _networkService.Host("MyRoom");
        }
    }

    public void OnJoinButtonClicked()
    {
        if (_networkService != null)
        {
            _networkService.Join("MyRoom");
        }
    }
}
