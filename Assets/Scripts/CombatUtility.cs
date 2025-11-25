using System.Collections.Generic;
using UnityEngine;

public static class CombatUtility
{
    /// <summary>
/// Finds all HealthComponents within a radius at the specified position, optionally filtered by team
/// </summary>
/// <param name="position">Center position to search from</param>
/// <param name="radius">Search radius</param>
/// <param name="team">Team ID to filter against (-1 means no team filter)</param>
/// <param name="excludeSameTeam">If true, excludes entities of the same team as the provided team parameter</param>
/// <returns>List of HealthComponents that match the criteria</returns>
public static List<HealthComponent> FindHealthComponentsInRadius(Vector3 position, float radius, int team = -1, bool excludeSameTeam = true)
{
    List<HealthComponent> results = new List<HealthComponent>();
    float sqrRadius = radius * radius; // Square the radius for more efficient distance checks
    
    // Find all HealthComponents in the scene
    HealthComponent[] allHealthComponents = GameObject.FindObjectsByType<HealthComponent>(FindObjectsSortMode.None);
    
    foreach (var health in allHealthComponents)
    {
        // Skip if null or already in results
        if (health == null || results.Contains(health))
            continue;
            
        // Check if within radius using squared distance for better performance
        float sqrDistance = (health.GetPosition() - position).sqrMagnitude;
        if (sqrDistance > sqrRadius)
            continue;
            
        // Apply team filtering if needed
        if (team != -1)
        {
            if (excludeSameTeam && health.GetTeam() == team)
                continue;
                
            if (!excludeSameTeam && health.GetTeam() != team)
                continue;
        }
        
        // Add to results
        results.Add(health);
    }
    
    return results;
}
    
    /// <summary>
    /// Finds the nearest HealthComponent to the specified position, optionally filtered by team
    /// </summary>
    /// <param name="position">Position to search from</param>
    /// <param name="maxDistance">Maximum search distance</param>
    /// <param name="team">Team ID to filter against (-1 means no team filter)</param>
    /// <param name="excludeSameTeam">If true, excludes entities of the same team as the provided team parameter</param>
    /// <returns>Nearest HealthComponent that matches the criteria, or null if none found</returns>
    public static HealthComponent FindNearestHealthComponent(Vector3 position, float maxDistance, int team = -1, bool excludeSameTeam = true)
    {
        HealthComponent nearest = null;
        float nearestDist = maxDistance;
        
        // Get all health components in radius
        List<HealthComponent> healthComponents = FindHealthComponentsInRadius(position, maxDistance, team, excludeSameTeam);
        
        // Find the nearest one
        foreach (var health in healthComponents)
        {
            float dist = Vector3.Distance(position, health.GetPosition());
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = health;
            }
        }
        
        return nearest;
    }
}