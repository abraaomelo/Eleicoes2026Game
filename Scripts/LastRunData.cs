using UnityEngine;

public static class LastRunData
{
    public static bool HasResult { get; private set; }

    public static int ProposalPoints { get; private set; }
    public static float DistanceMeters { get; private set; }

    public static GameController.RunResult Result { get; private set; }

    public static void Save(
        int proposalPoints,
        float distanceMeters,
        GameController.RunResult result)
    {
        ProposalPoints = proposalPoints;
        DistanceMeters = distanceMeters;
        Result = result;
        HasResult = true;
    }

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void Clear()
    {
        ProposalPoints = 0;
        DistanceMeters = 0f;
        Result = GameController.RunResult.None;
        HasResult = false;
    }
}


