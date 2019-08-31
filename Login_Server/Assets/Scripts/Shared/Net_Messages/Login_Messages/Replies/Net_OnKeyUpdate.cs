[System.Serializable]
public class Net_KeyUpdate : NetLoginMessage
{
    public string pubKeyString { get; set; }

    public Net_KeyUpdate(string pubKeyString)
    {
        OP = NetLoginOP.OnKeyUpdate;

        this.pubKeyString = pubKeyString;
    }

}
