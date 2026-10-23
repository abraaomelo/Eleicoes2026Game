using System;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;

public class LeaderboardConnectionTest : MonoBehaviour
{
    [SerializeField]
    private string leaderboardId = "distance_ranking_nkkeleicoes";

    private async void Start()
    {
        try
        {
            Debug.Log("Placar: conectando aos serviços...");

            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance
                    .SignInAnonymouslyAsync();
            }

            Debug.Log("Placar: autenticação concluída.");

            await LeaderboardsService.Instance.GetScoresAsync(
                leaderboardId
            );

            Debug.Log(
                $"Placar: conexão com '{leaderboardId}' funcionando!"
            );
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"Placar: falha na conexão.\n{exception}"
            );
        }
    }
}

