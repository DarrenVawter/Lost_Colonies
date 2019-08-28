public static class NetLoginOP
{
    public const byte None = 0;

    public const byte CreateAccount = 1;
    public const byte OnCreateAccount = 2;

    public const byte LoginRequest = 3;
    public const byte OnLoginRequest = 4;

    public const byte OnKeyUpdate = 5;
}

[System.Serializable]
public abstract class NetLoginMessage : NetMsg
{

    public NetLoginMessage()
    {
        RT = NetRT.LoginServer; //all login messages should be sent to login server
        OP = NetLoginOP.None;   //default
    }

}
