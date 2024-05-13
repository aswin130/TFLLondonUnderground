
using QuickGraph;

using QuickGraph.Algorithms.ShortestPath;


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

    var start = stations[startName];
    var end = stations[endName];

    var graph = new AdjacencyGraph<Station, Edge<Station>>();

    foreach (var station in stations.Values)
    {
        graph.AddVertex(station);
    }

    foreach (var station in stations.Values)
    {
        foreach (var connection in station.GetConnections(startLineDirection))
        {
            var from = station;
            var to = connection.Key;
            var weight = connection.Value;

            graph.AddEdge(new Edge<Station>(from, to));
            graph.AddEdge(new Edge<Station>(to, from));
        }
    }

    var algorithm = new DijkstraShortestPathAlgorithm<Station, Edge<Station>>(graph, edge => 1);

    algorithm.Compute(start);

    var path = algorithm.TryGetPath(end, out var shortestPath) ? shortestPath : new List<Edge<Station>>();
    var totalDistance = algorithm.Distances[end];

    var route = new List<string>();
    foreach (var edge in path)
    {
        route.Add(edge.Source.Name);
    }
    route.Add(end.Name);

    return new RouteResult(route, totalDistance);
    }
}