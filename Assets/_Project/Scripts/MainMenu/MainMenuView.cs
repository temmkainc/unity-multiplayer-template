using Zenject;
using UnityEngine;

public class MainMenuView : MonoBehaviour
{
    private MainMenuController _mainMenuController;

    [Inject]
    public void Construct(MainMenuController controller)
    {
        _mainMenuController = controller;
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 200, 40), "Host"))
        {
            _mainMenuController?.OnHostButtonClicked();
        }

        if (GUI.Button(new Rect(10, 60, 200, 40), "Join"))
        {
            _mainMenuController?.OnJoinButtonClicked();
        }
    }
}
