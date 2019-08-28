using TMPro;
using UnityEngine;

public class LoginScene : MonoBehaviour
{
    public static LoginScene Instance { private set; get; }
       
    private void Start()
    {
        Instance = this;
        GameObject.Find("LoginPassword").GetComponent<TMP_InputField>().inputType = TMP_InputField.InputType.Password;
        GameObject.Find("CreatePassword").GetComponent<TMP_InputField>().inputType = TMP_InputField.InputType.Password;

    }

    public void OnClickCreateAccount()
    {
        DisableInputs();
        Client.Instance.SendCreateAccount();
    }

    public void OnClickLoginRequest()
    {
        DisableInputs();
        Client.Instance.SendLoginRequest();
    }

    public void ChangeWelcomeText(string msg)
    {
        GameObject.Find("WelcomeMessageText").GetComponent<TextMeshProUGUI>().text = msg;
    }

    public void ChangeAuthenticationText(string msg)
    {
        GameObject.Find("AuthenticationMessageText").GetComponent<TextMeshProUGUI>().text = msg;
    }
       
    public void EnableInputs()
    {
        GameObject.Find("UI").GetComponent<CanvasGroup>().interactable = true;
    }

    public void DisableInputs()
    {
        GameObject.Find("UI").GetComponent<CanvasGroup>().interactable = false;
    }

}
