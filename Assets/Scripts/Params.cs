using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;


public class Params
{
    // public Dictionary<string, float> Defaults = new Dictionary<string, float>
    // {
    //     {"potential", 0.4f},
    //     {"goal", 1.0f},
    //     {"collision", -0.3f}
    // };

    public static float Potential => Get("potential", 0.4f);
    public static float Goal => Get("goal", 1.0f);
    public static float Collision => Get("collision", -0.3f);
    public static float SightRadius => Get("radius", 5f);
    public static int SightAgents => Mathf.RoundToInt(Get("sight_agents", 10f));
    public static float ComfortSpeed => Get("comfort_speed", 1.4f);
    public static float ComfortSpeedWeight => Get("comfort_speed_weight", 1f);
    public static float ComfortDistance => Get("comfort_distance", 1.5f);
    public static float ComfortDistanceWeight => Get("comfort_distance_weight", 1f);
    public static float PredatorPreyWeight => Get("predator_prey_weight", 1f);

    public static float Get(string name, float defaultValue)
    {
        return Academy.Instance.EnvironmentParameters.GetWithDefault(name, defaultValue);
    }
}