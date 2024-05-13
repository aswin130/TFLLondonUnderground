using System.Collections.Generic;

public class RouteResult
{
    private List<string> route;
    private double totalJourneyTime;

    public RouteResult()
    {
        // Default constructor
    }

    public RouteResult(List<string> route, double totalJourneyTime)
    {
        this.route = route;
        this.totalJourneyTime = totalJourneyTime;
    }

    public List<string> Route { get => route; set => route = value; }
    public List<string> Route1 { get => route; set => route = value; }

    public double TotalJourneyTime { get => totalJourneyTime; set => totalJourneyTime = value; }
    
    public List<string> GetRoute()
    {
        return route;
    }

    public double GetTotalJourneyTime()
    {
        return totalJourneyTime;
    }
}