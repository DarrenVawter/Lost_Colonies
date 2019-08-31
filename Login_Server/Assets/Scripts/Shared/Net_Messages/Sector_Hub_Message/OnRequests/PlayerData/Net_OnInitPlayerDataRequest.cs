using System.Collections.Generic;

[System.Serializable]
public class Net_OnInitPlayerDataRequest : NetSectorMessage
{
    public string CreatedOn { get; set; }

    public List<Message_This_Worker> Workers { get; set; }
    public int ActiveWorkerIndex { get; set; }

    public Net_OnInitPlayerDataRequest(string CreatedOn, List<Message_This_Worker> Workers, int ActiveWorkerIndex)
    {
        OP = NetSectorOP.OnInitPlayerDataRequest;

        this.CreatedOn = CreatedOn;
        this.Workers = Workers;
        this.ActiveWorkerIndex = ActiveWorkerIndex;
    }
}
