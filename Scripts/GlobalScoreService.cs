using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;

[Serializable]
public class GlobalScoreData
{
    public long leftMeters;
    public long leftProposals;
    public long rightMeters;
    public long rightProposals;

    public bool counted;
    public bool duplicate;
    public bool ignored;
}

public static class GlobalScoreService
{
    private static Task connectionTask;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetConnection()
    {
        connectionTask = null;
    }

    private static Task EnsureConnectedAsync()
    {
        if (connectionTask == null ||
            connectionTask.IsFaulted ||
            connectionTask.IsCanceled)
        {
            connectionTask = ConnectAsync();
        }

        return connectionTask;
    }

    private static async Task ConnectAsync()
    {
        var options = new InitializationOptions();
        options.SetEnvironmentName("production");

        await UnityServices.InitializeAsync(options);

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance
                .SignInAnonymouslyAsync();
        }
    }

    public static async Task<GlobalScoreData> RegisterAsync(
        string runId,
        GameController.RunResult result,
        int meters,
        int proposals)
    {
        // Proteção no cliente: não chama a API nesses casos.
        if (result != GameController.RunResult.Left &&
            result != GameController.RunResult.Right)
        {
            return null;
        }

        var parameters = new Dictionary<string, object>
        {
            { "runId", runId },
            { "side", result.ToString() },
            { "meters", meters },
            { "proposals", proposals }
        };

        await EnsureConnectedAsync();

        return await CloudCodeService.Instance
            .CallEndpointAsync<GlobalScoreData>(
                "RegisterGlobalRun",
                parameters
            );
    }

    public static async Task<GlobalScoreData> GetAsync()
    {
        await EnsureConnectedAsync();

        return await CloudCodeService.Instance
            .CallEndpointAsync<GlobalScoreData>(
                "GetGlobalScore",
                new Dictionary<string, object>()
            );
    }
}

