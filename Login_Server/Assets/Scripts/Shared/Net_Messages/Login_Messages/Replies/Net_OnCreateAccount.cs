public static class CREATE_ACCOUNT_RESPONSE
{
    public const byte NONE = 0;
    public const byte SUCCESS = 1;
    //TODO add errors
}

[System.Serializable]
public class Net_OnCreateAccount : NetLoginMessage
{
    //0 -> failure
    //1 -> success
    public byte response { get; set; }

    public Net_OnCreateAccount(byte response)
    {
        OP = NetLoginOP.OnCreateAccount;

        this.response = response;
    }

}
