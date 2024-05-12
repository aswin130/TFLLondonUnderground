using System;
using System.Collections.Generic;
using System.Linq;

public class NetworkGraph
{
    private readonly Dictionary<string, Station> stations;
    private readonly Dictionary<string, string> closedTracks;
    private readonly Dictionary<string, double> delays;
    private readonly Dictionary<string, double> originalTime;

    public NetworkGraph()
    {
        stations = new Dictionary<string, Station>();
        closedTracks = new Dictionary<string, string>();
        delays = new Dictionary<string, double>();
        originalTime = new Dictionary<string, double>();
    }

    public void AddStation(string name)
    {
        stations.TryAdd(name, new Station(name));
    }

    public void AddConnection(StationConnection stationConnection)
    {
        if (!stations.ContainsKey(stationConnection.FromName) || !stations.ContainsKey(stationConnection.ToName))
            throw new ArgumentException("Station not found");

        Station fromStation = stations[stationConnection.FromName];
        Station toStation = stations[stationConnection.ToName];
        string key = $"{stationConnection.FromName}-{stationConnection.ToName}-{stationConnection.LineDirection}";

        if (!closedTracks.ContainsKey(key))
        {
            fromStation.AddConnection(toStation, stationConnection.LineDirection, stationConnection.Time);
            toStation.AddConnection(fromStation, stationConnection.LineDirection, stationConnection.Time);
        }
    }

    public void AddDelay(StationConnection stationConnection)
    {
        string key = $"{stationConnection.FromName}-{stationConnection.ToName}-{stationConnection.LineDirection}";

        if (!stations.ContainsKey(stationConnection.FromName) || !stations.ContainsKey(stationConnection.ToName))
            throw new ArgumentException("Station not found");

        double originalTime = stations[stationConnection.FromName]
            .GetConnections(stationConnection.LineDirection)
            .GetValueOrDefault(stations[stationConnection.ToName], 0.0) - delays.GetValueOrDefault(key, 0.0);

        delays[key] = stationConnection.Time;
        stationConnection.Time = originalTime + stationConnection.Time;
        AddConnection(stationConnection);
    }

    public void RemoveDelay(StationConnection stationConnection)
    {
        string key = $"{stationConnection.FromName}-{stationConnection.ToName}-{stationConnection.LineDirection}";

        if (!stations.ContainsKey(stationConnection.FromName) || !stations.ContainsKey(stationConnection.ToName))
            throw new ArgumentException("Station not found");

        double originalTime = stations[stationConnection.FromName]
            .GetConnections(stationConnection.LineDirection)
            .GetValueOrDefault(stations[stationConnection.ToName], 0.0) - delays.GetValueOrDefault(key, 0.0);

        delays.Remove(key);
        stationConnection.Time = originalTime;
        AddConnection(stationConnection);
    }

    public void CloseTrack(StationConnection stationConnection)
    {
        string key = $"{stationConnection.FromName}-{stationConnection.ToName}-{stationConnection.LineDirection}";

        if (!stations.ContainsKey(stationConnection.FromName) || !stations.ContainsKey(stationConnection.ToName))
            throw new ArgumentException("Station not found");

        closedTracks[key] = "closed";

        double time = stations[stationConnection.FromName]
            .GetConnections(stationConnection.LineDirection)
            .GetValueOrDefault(stations[stationConnection.ToName]);

        if (time != 0)
            originalTime[key] = time;

        //stations[stationConnection.FromName].RemoveConnection(stationConnection.ToName, stationConnection.LineDirection);
        stations[stationConnection.FromName].Connections[stationConnection.LineDirection].Remove(stations[stationConnection.ToName]);
        //stations[stationConnection.ToName].RemoveConnection(stationConnection.FromName, stationConnection.LineDirection);
        stations[stationConnection.ToName].Connections[stationConnection.LineDirection].Remove(stations[stationConnection.FromName]);
    }

    public void PrintStations()
    {
        foreach (var station in stations)
            Console.WriteLine(station);
    }

    public void PrintDelayedStations()
    {
        foreach (var delay in delays)
            Console.WriteLine(delay);
    }

    public void OpenTrack(StationConnection stationConnection)
    {
        string key = $"{stationConnection.FromName}-{stationConnection.ToName}-{stationConnection.LineDirection}";

        if (!closedTracks.ContainsKey(key))
            throw new ArgumentException("Track is not closed");

        closedTracks.Remove(key);

        if (!originalTime.ContainsKey(key))
            throw new ArgumentException("Original time not found");

        double time = originalTime[key];

        stationConnection.Time = time;
        AddConnection(stationConnection);
    }

    public void PrintClosedTracks()
    {
        foreach (var track in closedTracks)
            Console.WriteLine(track);
    }

    public RouteResult FindShortestRoute(string startName, string endName, string startLineDirection)
    {
        if (!stations.ContainsKey(startName) || !stations.ContainsKey(endName))
            throw new ArgumentException("Start or end station not found in London");

        Station start = stations[startName];
        Station end = stations[endName];

        var settled = new HashSet<Station>();
        var unsettled = new HashSet<Station>();
        var distance = new Dictionary<Station, double>();
        var predecessors = new Dictionary<Station, Station>();
        var lineDirections = new Dictionary<Station, string>();

        foreach (var station in stations.Values)
        {
            distance[station] = double.MaxValue;
            predecessors[station] = null;
        }

        distance[start] = 0.0;
        unsettled.Add(start);
        lineDirections[start] = startLineDirection;

        while (unsettled.Any())
        {
            Station current = GetLowestDistanceStation(unsettled, distance);
            unsettled.Remove(current);

            foreach (var connection in current.GetConnections(lineDirections[current]))
            {
                Station neighbor = connection.Key;
                double time = connection.Value;

                if (!settled.Contains(neighbor))
                {
                    double currentDistance = distance[current];
                    double newDistance = currentDistance + time;

                    if (lineDirections[current] != null && !lineDirections[current].Equals(lineDirections[current]))
                        newDistance += 2; // Adding interchange time

                    if (newDistance < distance[neighbor])
                    {
                        distance[neighbor] = newDistance;
                        predecessors[neighbor] = current;
                        lineDirections[neighbor] = lineDirections[current];
                        unsettled.Add(neighbor);
                    }
                }
            }

            settled.Add(current);
        }

        return new RouteResult(GetPath(end, predecessors), distance.GetValueOrDefault(end, -1.0));
    }

    private Station GetLowestDistanceStation(HashSet<Station> unsettled, Dictionary<Station, double> distance)
    {
        Station lowestDistanceStation = null;
        double lowestDistance = double.MaxValue;

        foreach (var station in unsettled)
        {
            double stationDistance = distance[station];

            if (stationDistance < lowestDistance)
            {
                lowestDistance = stationDistance;
                lowestDistanceStation = station;
            }
        }

        return lowestDistanceStation;
    }

    private List<string> GetPath(Station end, Dictionary<Station, Station> predecessors)
    {
        var path = new LinkedList<string>();
        Station step = end;

        if (predecessors[step] == null)
            return path.ToList();

        path.AddLast(step.Name);

        while (predecessors[step] != null)
        {
            step = predecessors[step];
            path.AddLast(step.Name);
        }

        path.Reverse();
        return path.ToList();
    }
}