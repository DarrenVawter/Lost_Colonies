[System.Serializable]
class Net_OnActiveWorkerChange : NetSectorMessage
{
    public byte workerIndex { get; }

    public Net_OnActiveWorkerChange(byte workerIndex)
    {
        OP = NetSectorOP.OnActiveWorkerChange;

        this.workerIndex = workerIndex;
    }
}