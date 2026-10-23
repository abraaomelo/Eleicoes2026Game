using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class GlobalScoreSubmission
{
    private class PendingRun
    {
        public string id;
        public GameController.RunResult result;
        public int meters;
        public int proposals;
    }

    private static readonly Dictionary<string, PendingRun> pending =
        new Dictionary<string, PendingRun>();

    private static Task submissionTask;

    public static int PendingCount => pending.Count;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        pending.Clear();
        submissionTask = null;
    }

    public static void QueueLastRun()
    {
        if (!LastRunData.HasResult)
            return;

        GameController.RunResult result = LastRunData.Result;

        // Centro e isentão não geram chamada de registro.
        if (result != GameController.RunResult.Left &&
            result != GameController.RunResult.Right)
        {
            return;
        }

        string id = LastRunData.RunId;

        if (string.IsNullOrEmpty(id))
            return;

        pending[id] = new PendingRun
        {
            id = id,
            result = result,
            meters = Mathf.FloorToInt(LastRunData.DistanceMeters),
            proposals = LastRunData.CollectedProposals
        };

        SubmitInBackground();
    }

    private static async void SubmitInBackground()
    {
        try
        {
            await FlushAsync();
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                "Placar: partida pendente. " +
                "Tentaremos novamente ao abrir o placar.\n" +
                exception.Message
            );
        }
    }

    public static Task FlushAsync()
    {
        if (submissionTask == null || submissionTask.IsCompleted)
            submissionTask = ProcessPendingAsync();

        return submissionTask;
    }

    private static async Task ProcessPendingAsync()
    {
        while (pending.Count > 0)
        {
            PendingRun run = null;

            foreach (PendingRun candidate in pending.Values)
            {
                run = candidate;
                break;
            }

            GlobalScoreData response =
                await GlobalScoreService.RegisterAsync(
                    run.id,
                    run.result,
                    run.meters,
                    run.proposals
                );

            if (response == null ||
                (!response.counted && !response.duplicate))
            {
                throw new Exception(
                    "O servidor não confirmou o registro da partida."
                );
            }

            pending.Remove(run.id);

            Debug.Log(
                $"Placar: partida confirmada. " +
                $"Contabilizada={response.counted}, " +
                $"Duplicada={response.duplicate}"
            );
        }
    }
}

