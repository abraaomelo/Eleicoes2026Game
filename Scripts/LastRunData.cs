using System;
using UnityEngine;

public static class LastRunData
{
    public static bool HasResult { get; private set; }

    public static string RunId { get; private set; }

    public static int ProposalPoints { get; private set; }
    public static int CollectedProposals { get; private set; }

    public static float DistanceMeters { get; private set; }

    public static GameController.RunResult Result { get; private set; }

    public static void Save(
        int proposalPoints,
        float distanceMeters,
        GameController.RunResult result,
        int collectedProposals)
    {
        RunId = Guid.NewGuid().ToString("N");

        ProposalPoints = proposalPoints;
        CollectedProposals = collectedProposals;

        DistanceMeters = distanceMeters;
        Result = result;

        HasResult = true;
    }

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void Clear()
    {
        HasResult = false;
        RunId = null;

        ProposalPoints = 0;
        CollectedProposals = 0;

        DistanceMeters = 0f;
        Result = GameController.RunResult.None;
    }
}

