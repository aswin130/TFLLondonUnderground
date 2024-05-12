using System;
using System.Collections.Generic;
using System.Text;

public class Station
{
    private string name;
    public Dictionary<string, Dictionary<Station, double>> Connections { get => connections; set => connections = value; }
    public string Name { get => name; set => name = value; }

    private Dictionary<string, Dictionary<Station, double>> connections;

    public Station(string name)
    {
        Name = name;
        Connections = new Dictionary<string, Dictionary<Station, double>>();
    }

    public void AddConnection(Station station, string lineDirection, double time)
    {
        if (!Connections.ContainsKey(lineDirection))
        {
            Connections[lineDirection] = new Dictionary<Station, double>();
        }
        Connections[lineDirection][station] = time;
    }

    public Dictionary<Station, double> GetConnections(string lineDirection)
    {
        return Connections.GetValueOrDefault(lineDirection, new Dictionary<Station, double>());
    }

    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append($"Station{{name='{Name}', connections=");
        foreach (var (lineDirection, stations) in Connections)
        {
            builder.Append($"\n  {lineDirection}: ");
            foreach (var (station, time) in stations)
            {
                builder.Append($"{station.Name} ({time} mins), ");
            }
        }
        builder.Append("}}");
        return builder.ToString();
    }
}