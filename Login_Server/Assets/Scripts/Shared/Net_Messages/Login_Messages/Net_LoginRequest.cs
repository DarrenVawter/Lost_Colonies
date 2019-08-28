[System.Serializable]
public class Net_LoginRequest : NetLoginMessage
{
    public string user { get; set; }
    public string password { get; set; }

    public Net_LoginRequest(string user, string password)
    {
        OP = NetLoginOP.LoginRequest;

        this.user = user;
        this.password = password;
    }

}
