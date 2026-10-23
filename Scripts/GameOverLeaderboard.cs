using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;

public class GameOverLeaderboard : MonoBehaviour
{
    [Serializable]
    private class ScoreMetadata
    {
        public string result;
        public int proposals;
    }

    [Header("Ranking")]
    [SerializeField]
    private string leaderboardId = "distance_ranking_nkkeleicoes";

    [Header("Painéis")]
    [SerializeField] private GameObject resultContent;
    [SerializeField] private GameObject leaderboardPanel;

    [Header("Formulário")]
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private Button submitButton;
    [SerializeField] private TMP_Text submitStatusText;

    [Header("Lista")]
    [SerializeField] private LeaderboardRowUI rowPrefab;
    [SerializeField] private RectTransform rowsContainer;
    [SerializeField] private RectTransform playerContainer;
    [SerializeField] private TMP_Text statusText;

    private const string NicknameKey = "LeaderboardNickname";

    private static Task connectionTask;

    private readonly List<GameObject> generatedRows =
        new List<GameObject>();

    private bool busy;
    private bool submitted;
    private bool hasRun;

    private int runDistance;
    private int runProposals;
    private GameController.RunResult runResult;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetConnection()
    {
        connectionTask = null;
    }

    private void Start()
    {
        // Guarda a partida exibida nesta tela.
        hasRun = LastRunData.HasResult;
        runDistance = Mathf.FloorToInt(LastRunData.DistanceMeters);
        runProposals = LastRunData.ProposalPoints;
        runResult = LastRunData.Result;

        nicknameInput.text = PlayerPrefs.GetString(
            NicknameKey,
            ""
        );

        resultContent.SetActive(true);
        leaderboardPanel.SetActive(false);
        playerContainer.gameObject.SetActive(false);

        submitStatusText.text = hasRun
            ? ""
            : "Termine uma corrida para enviar seu resultado.";

        submitButton.interactable = hasRun;
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

    public async void SubmitAndShow()
    {
        if (busy || !hasRun)
            return;

        string nickname = nicknameInput.text.Trim();

        if (nickname.Length < 2 || nickname.Length > 20)
        {
            submitStatusText.text =
                "Use um apelido entre 2 e 20 caracteres.";
            return;
        }

        foreach (char character in nickname)
        {
            if (!char.IsLetterOrDigit(character) &&
                character != '_' &&
                character != '-')
            {
                submitStatusText.text =
                    "Use letras, números, _ ou -, sem espaços.";
                return;
            }
        }

        busy = true;
        submitButton.interactable = false;
        nicknameInput.interactable = false;
        submitStatusText.text = "Conectando...";

        try
        {
            await EnsureConnectedAsync();

            if (this == null)
                return;

            await AuthenticationService.Instance
                .UpdatePlayerNameAsync(nickname);

            if (this == null)
                return;

            PlayerPrefs.SetString(NicknameKey, nickname);
            PlayerPrefs.Save();

            if (!submitted)
            {
                submitStatusText.text = "Enviando resultado...";

                var metadata = new ScoreMetadata
                {
                    result = GetResultLabel(runResult),
                    proposals = runProposals
                };

                await LeaderboardsService.Instance.AddPlayerScoreAsync(
                    leaderboardId,
                    runDistance,
                    new AddPlayerScoreOptions
                    {
                        Metadata = metadata
                    }
                );

                if (this == null)
                    return;

                submitted = true;
            }

            resultContent.SetActive(false);
            leaderboardPanel.SetActive(true);
            statusText.text = "Carregando placar...";

            await LoadLeaderboardAsync();
        }
        catch (Exception exception)
        {
            if (this == null)
                return;

            string message =
                "Não foi possível conectar. Volte e tente novamente.";

            submitStatusText.text = message;
            statusText.text = message;

            Debug.LogError($"Placar: {exception}");
        }
        finally
        {
            if (this != null)
            {
                busy = false;
                submitButton.interactable = hasRun;
                nicknameInput.interactable = true;
            }
        }
    }

    private async Task LoadLeaderboardAsync()
    {
        ClearRows();

        var top = await LeaderboardsService.Instance.GetScoresAsync(
            leaderboardId,
            new GetScoresOptions
            {
                Limit = 10,
                IncludeMetadata = true
            }
        );

        if (this == null)
            return;

        string playerId = AuthenticationService.Instance.PlayerId;
        bool playerInTop = false;

        foreach (LeaderboardEntry entry in top.Results)
        {
            bool isPlayer = entry.PlayerId == playerId;

            if (isPlayer)
                playerInTop = true;

            CreateRow(entry, rowsContainer, isPlayer);
        }

        // Só busca e exibe a linha adicional se o jogador
        // não estiver entre os registros do Top 10.
        if (!playerInTop)
        {
            var playerEntry = await LeaderboardsService.Instance
                .GetPlayerScoreAsync(
                    leaderboardId,
                    new GetPlayerScoreOptions
                    {
                        IncludeMetadata = true
                    }
                );

            if (this == null)
                return;

            // Rank começa em zero: 10 corresponde ao 11º lugar.
            if (playerEntry.Rank >= 10)
            {
                playerContainer.gameObject.SetActive(true);
                CreateRow(playerEntry, playerContainer, true);
            }
        }

        statusText.text = top.Results.Count == 0
            ? "O placar ainda está vazio."
            : "";
    }

    private void CreateRow(
        LeaderboardEntry entry,
        RectTransform container,
        bool isPlayer)
    {
        LeaderboardRowUI row = Instantiate(
            rowPrefab,
            container,
            false
        );

        row.gameObject.SetActive(true);
        row.Fill(entry, isPlayer);
        generatedRows.Add(row.gameObject);

        // A linha adicional ocupa seu container.
        // As linhas do Top 10 são organizadas pelo Layout Group.
        if (container == playerContainer)
        {
            RectTransform rect =
                row.GetComponent<RectTransform>();

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }

    private void ClearRows()
    {
        foreach (GameObject row in generatedRows)
        {
            if (row == null)
                continue;

            row.SetActive(false);
            Destroy(row);
        }

        generatedRows.Clear();
        playerContainer.gameObject.SetActive(false);
    }

    public void BackToResult()
    {
        leaderboardPanel.SetActive(false);
        resultContent.SetActive(true);
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
                return "—";
        }
    }
}

