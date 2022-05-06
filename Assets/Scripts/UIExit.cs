using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIExit : MonoBehaviour
{
    public GameObject exit;
    private GameObject exitWindow;

    public void ExitWindow()
    {
        exitWindow = Instantiate(exit);
        GameObject Canvas = GetComponentInParent<Canvas>().gameObject;
        exitWindow.transform.SetParent(Canvas.transform, false);
        exitWindow.GetComponentsInChildren<Button>()[0].onClick.AddListener(Exit);
        exitWindow.GetComponentsInChildren<Button>()[1].onClick.AddListener(Stay);
    }

    private void Stay()
    {
        Destroy(exitWindow);
    }

    private void Exit()
    {
        Application.Quit();
    }
}
