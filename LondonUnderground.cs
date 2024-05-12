using System;
using System.Collections.Generic;

public class LondonUnderground
{
    public void start()
    {
        NetworkGraph graph = new NetworkGraph();

        while (true)
        {
            Console.WriteLine("The following roles are available");
            Console.WriteLine("1.Engineer");
            Console.WriteLine("2.Customer");
            Console.WriteLine("Enter the role:");

            string role = Console.ReadLine();
            try
            {
                if (role.Equals("Engineer", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Enter the details of Stations separated by space:");
                    string stationInput = Console.ReadLine();
                    string[] words = stationInput.Split(' ');
                    Array.ForEach(words, graph.AddStation);

                    StationConnection[] connections = GetStationConnections(graph, "addConnections");

                    Array.ForEach(connections, graph.AddConnection);

                    graph.PrintStations();

                    Console.WriteLine("select yes or no, if you need to add delay:");
                    string isAddDelayEnabled = Console.ReadLine();

                    if (isAddDelayEnabled.Equals("yes", StringComparison.OrdinalIgnoreCase))
                    {
                        StationConnection[] delayedConnections = GetStationConnections(graph, "addDelay");
                        Array.ForEach(delayedConnections, graph.AddDelay);
                        graph.PrintDelayedStations();
                    }

                    Console.WriteLine("select yes or no, if you need to remove delay:");
                    string isRemoveDelayEnabled = Console.ReadLine();

                    if (isRemoveDelayEnabled.Equals("yes", StringComparison.OrdinalIgnoreCase))
                    {
                        StationConnection[] removeDelayedConnection = GetStationConnections(graph, "removeDelay");
                        Array.ForEach(removeDelayedConnection, graph.RemoveDelay);
                        graph.PrintDelayedStations();
                    }

                    Console.WriteLine("select yes or no, if you need to close track:");
                    string isCloseTrackEnabled = Console.ReadLine();

                    if (isCloseTrackEnabled.Equals("yes", StringComparison.OrdinalIgnoreCase))
                    {
                        StationConnection[] closeTrackConnection = GetStationConnections(graph, "closeTrack");
                        Array.ForEach(closeTrackConnection, graph.CloseTrack);
                        graph.PrintClosedTracks();
                    }

                    Console.WriteLine("select yes or no, if you need to open track:");
                    string isOpenTrackEnabled = Console.ReadLine();

                    if (isOpenTrackEnabled.Equals("yes", StringComparison.OrdinalIgnoreCase))
                    {
                        StationConnection[] openTrackConnection = GetStationConnections(graph, "openTrack");
                        Array.ForEach(openTrackConnection, graph.OpenTrack);
                    }
                }

                if (role.Equals("Customer", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Enter start station:");
                    string startStation = Console.ReadLine();
                    Console.WriteLine("Enter start line and direction (e.g., Jubilee-Eastbound):");
                    string startLineDirection = Console.ReadLine();
                    Console.WriteLine("Enter end station:");
                    string endStation = Console.ReadLine();

                    if (!startStation.Equals(endStation, StringComparison.OrdinalIgnoreCase))
                    {
                        RouteResult result = graph.FindShortestRoute(startStation, endStation, startLineDirection);
                        if (result.Route != null)
                        {
                            List<string> route = result.Route;
                            double totalTime = result.TotalJourneyTime;

                            if (route.Count == 0)
                            {
                                Console.WriteLine("No route found.");
                            }
                            else
                            {
                                Console.WriteLine("Route found:");
                                foreach (string station in route)
                                {
                                    Console.WriteLine(station);
                                }
                                Console.WriteLine($"Total Journey Time: {totalTime} minutes");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Start and end stations are the same");
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("Some error occurred! Check your input!");
            }
        }
    }

    private static StationConnection[] GetStationConnections(NetworkGraph graph, string operation)
    {
        Console.WriteLine("Enter the number of connections:");
        int numberOfConnections = int.Parse(Console.ReadLine());

        StationConnection[] connections = new StationConnection[numberOfConnections];

        for (int i = 0; i < numberOfConnections; i++)
        {
            Console.WriteLine($"Enter details for connection {i + 1} (format: fromName toName lineDirection time):");
            string line = Console.ReadLine();
            string[] parts = line.Split(' ');

            string fromName = parts[0];
            string toName = parts[1];
            string lineDirection = parts[2];
            double time = 0.0;

            if (!operation.Equals("closeTrack", StringComparison.OrdinalIgnoreCase) &&
                !operation.Equals("openTrack", StringComparison.OrdinalIgnoreCase))
            {
                time = double.Parse(parts[3]);
            }

            connections[i] = new StationConnection(fromName, toName, lineDirection, time);
        }

        return connections;
    }
}