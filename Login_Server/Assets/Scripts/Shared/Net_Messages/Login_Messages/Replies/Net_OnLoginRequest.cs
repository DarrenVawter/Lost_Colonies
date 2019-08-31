public static class LOGIN_RESPONSE
{
    public const byte NONE = 0;
    public const byte SUCCESS = 1;
    //TODO add errors
}

[System.Serializable]
public class Net_OnLoginRequest : NetLoginMessage
{
    public byte response { get; set; }

    public string username { get; set; }
    public string discriminator { get; set; }
    public string token { get; set; }
    public string sectorIP { get; set; }

    public Net_OnLoginRequest(byte response, string username, string discriminator, string token, string sectorIP)
    {
        OP = NetLoginOP.OnLoginRequest;

        this.response = response;
        this.username = username;
        this.discriminator = discriminator;
        this.token = token;
        this.sectorIP = sectorIP;
    }

}
