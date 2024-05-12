public class StationConnection
{
    private readonly string fromName;
    private readonly string toName;
    private readonly string lineDirection;
    private double time;

    public string FromName => fromName;

    public string ToName => toName;

    public string LineDirection => lineDirection;

    public double Time { get => time; set => time = value; }

    public StationConnection(string fromName, string toName, string lineDirection, double time)
    {
        this.fromName = fromName;
        this.toName = toName;
        this.lineDirection = lineDirection;
        this.time = time;
    }

    public string GetFromName()
    {
        return fromName;
    }

    public string GetToName()
    {
        return toName;
    }

    public string GetLineDirection()
    {
        return lineDirection;
    }

    public double GetTime()
    {
        return time;
    }

    public void SetTime(double time)
    {
        this.time = time;
    }

    public override string ToString()
    {
        return $"StationConnection{{fromName='{fromName}', toName='{toName}', lineDirection='{lineDirection}', time={time}}}";
    }
}