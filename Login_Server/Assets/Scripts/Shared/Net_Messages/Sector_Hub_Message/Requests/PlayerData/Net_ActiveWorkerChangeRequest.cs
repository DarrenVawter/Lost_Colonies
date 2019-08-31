[System.Serializable]
public class Net_ActiveWorkerChangeRequest : NetSectorMessage
{
    public string token { get; }
    public byte workerIndex { get; }

    public Net_ActiveWorkerChangeRequest(string token, byte workerIndex)
    {
        OP = NetSectorOP.ActiveWorkerChangeRequest;

        this.token = token;
        this.workerIndex = workerIndex;
    }
}
