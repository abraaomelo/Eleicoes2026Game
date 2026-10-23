using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;

public class LeaderboardScoreUploader : MonoBehaviour
{
    [Serializable]
    public class ScoreMetadata
    {
        public string result;
        public int proposals;
    }

    [SerializeField]
    private string leaderboardId = "distance_ranking_nkkeleicoes";

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

    private async void Start()
    {
        // Abrir um Game Over diretamente pelo editor
        // não deve enviar uma partida inexistente.
        if (!LastRunData.HasResult)
            return;

        // Copia os dados antes de aguardar a conexão.
        // Eles continuam corretos se o jogador mudar de cena.
        int distance = Mathf.FloorToInt(
            LastRunData.DistanceMeters
        );

        var metadata = new ScoreMetadata
        {
            result = GetResultLabel(LastRunData.Result),
            proposals = LastRunData.ProposalPoints
        };

        try
        {
            await EnsureConnectedAsync();

            var entry = await LeaderboardsService.Instance
                .AddPlayerScoreAsync(
                    leaderboardId,
                    distance,
                    new AddPlayerScoreOptions
                    {
                        Metadata = metadata
                    }
                );

            Debug.Log(
                $"Placar: partida enviada! " +
                $"Distância: {distance}m | " +
                $"Resultado: {metadata.result} | " +
                $"Recorde online: {entry.Score}m"
            );
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"Placar: não foi possível enviar a partida.\n" +
                exception
            );
        }
    }

    private static string GetResultLabel(
        GameController.RunResult result)
    {
        switch (result)
        {
            case GameController.RunResult.Left:
                return "Esquerda";

            case GameController.RunResult.Right:
                return "Direita";

            case GameController.RunResult.Tie:
                return "Centro";

            case GameController.RunResult.NoProposals:
                return "Isentão";

            default:
                return "Sem resultado";
        }
    }
}

