using System;
using System.Collections.Generic;
using UnityEngine;

public struct Waypoint
{
    public Vector3 position;
    public List<Waypoint> connections;

    public Waypoint(Vector3 position) : this()
    {
        this.position = position;
        connections = new List<Waypoint>();
    }

    public void AddConnectionWith(Waypoint w)
    {
        if (!connections.Contains(w))
            connections.Add(w);
        if (!w.connections.Contains(this))
            w.connections.Add(this);
    }

    public void RemoveConnectionWith(Waypoint w)
    {
        connections.Remove(w);
    }

    public bool Equals(Waypoint w)
    {
        return w.position == position;
    }
}

public class WaypointsManager
{
    public const float MinMagnitude = 11f;
    private Dictionary<Vector3, Waypoint> _waypoints;

    public WaypointsManager()
    {
        _waypoints = new Dictionary<Vector3, Waypoint>();
    }

    public void AddRange(Vector3[] waypoints)
    {
        foreach (var waypoint in waypoints)
        {
            _waypoints.Add(waypoint, new Waypoint(waypoint));
        }
    }

    public void CreateRelations()
    {
        Waypoint temp;
        foreach (var wp in _waypoints.Values)
        {
            foreach (var wp2 in GenerateHypotheticPositionsAround(wp.position))
            {
                if (_waypoints.TryGetValue(wp2, out temp))
                {
                    wp.AddConnectionWith(temp);
                }
            }
        }
    }

    public Vector3[] GenerateHypotheticPositionsAround(Vector3 position)
    {
        List<Vector3> positions = new List<Vector3>();

        // Generates all x positions
        /*
         * x -- x -- x
         * |    |    |
         * x -- 0 -- x
         * |    |    |
         * x -- x -- x
         *
         */
        for (int i = -1; i < 2; ++i)
        {
            for (int j = -1; j < 2; ++j)
            {
                if (i != 0 || j != 0)
                    positions.Add(new Vector3(position.x + i * GenerationManager.StepBetweenWayPoints, position.y,
                        position.z + GenerationManager.StepBetweenWayPoints));
            }
        }

        return positions.ToArray();
    }

    public Vector3[] GetPathBetween(Vector3 position, Vector3 destination)
    {
        Waypoint? pos = ClampToWaypoint(position), dest = ClampToWaypoint(destination);

        if (pos.HasValue && dest.HasValue)
        {
            return DoAStar(pos.Value, dest.Value);
        }

        return new Vector3[]{};
    }

    private Waypoint? ClampToWaypoint(Vector3 position)
    {
        int step = GenerationManager.StepBetweenWayPoints;
        var posx = position.x % step > step / 2
            ? position.x +
              (step - position.x % step)
            : position.x - (position.x % step);
        var posz = position.z % step > step / 2
            ? position.z +
              (step - position.z % step)
            : position.z - (position.z % step);

        Waypoint wp;
        if (_waypoints.TryGetValue(new Vector3(posx, 0, posz), out wp))
        {
            return wp;
        }

        return null;
    }

    private Vector3[] DoAStar(Waypoint position, Waypoint destination)
    {
        var frontier = new Queue<Waypoint>();
        frontier.Enqueue(position); 

        Dictionary<Waypoint, Waypoint> cameFrom = new Dictionary<Waypoint, Waypoint>();
        Dictionary<Waypoint, double> costSoFar = new Dictionary<Waypoint, double>();

        cameFrom[position] = position;
        costSoFar[position] = 0;

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();

            if (current.Equals(destination))
            {
                break;
            }

            foreach (var next in current.connections)
            {
                // 1 for the cost because cost is always 1
                double newCost = costSoFar[current] + 1;
                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    frontier.Enqueue(next);
                    cameFrom[next] = current;
                }
            }
        }

        List<Vector3> path = new List<Vector3>();
        var pathPos = destination;
        while (cameFrom.ContainsKey(pathPos) && pathPos.position != position.position)
        {
            path.Add(pathPos.position);
            pathPos = cameFrom[pathPos];
        }

        path.Reverse();

        return path.ToArray();
    }
}