[System.Serializable]
public class Net_WorkerDataRequest : NetSectorMessage
{
    public string workerName { get; }

    public Net_WorkerDataRequest(string workerName)
    {
        OP = NetSectorOP.WorkerDataRequest;

        this.workerName = workerName;
    }
}
