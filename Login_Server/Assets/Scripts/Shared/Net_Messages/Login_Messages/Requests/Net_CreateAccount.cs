[System.Serializable]
public class Net_CreateAccount : NetLoginMessage
{
    public string username { get; set; }
    public string password { get; set; }
    public string email { get; set; }

    public Net_CreateAccount(string username, string password, string email)
    {
        OP = NetLoginOP.CreateAccount;

        this.username = username;
        this.password = password;
        this.email = email;
    }

}
