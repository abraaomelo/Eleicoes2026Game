using System;
using TMPro;
using UnityEngine;
using Unity.Services.Leaderboards.Models;

public class LeaderboardRowUI : MonoBehaviour
{
    [SerializeField] private TMP_Text positionText;
    [SerializeField] private TMP_Text nicknameText;
    [SerializeField] private TMP_Text distanceText;
    [SerializeField] private TMP_Text resultText;

    [Serializable]
    private class ScoreMetadata
    {
        public string result;
    }

    public void Fill(LeaderboardEntry entry, bool isPlayer)
    {
        positionText.richText = false;
        nicknameText.richText = false;
        distanceText.richText = false;
        resultText.richText = false;

        positionText.text = (entry.Rank + 1).ToString();

        string nickname = entry.PlayerName;

        if (string.IsNullOrWhiteSpace(nickname))
        {
            nickname = "Jogador";
        }
        else
        {
            // Oculta o sufixo de identificação adicionado pela Unity.
            int suffixIndex = nickname.LastIndexOf('#');

            if (suffixIndex > 0)
                nickname = nickname.Substring(0, suffixIndex);
        }

        nicknameText.text = isPlayer
            ? $"{nickname} (você)"
            : nickname;

        distanceText.text = $"{Math.Floor(entry.Score):0}m";
        resultText.text = ReadResult(entry.Metadata);
    }

    private string ReadResult(string metadata)
    {
        if (string.IsNullOrEmpty(metadata))
            return "—";

        try
        {
            ScoreMetadata data =
                JsonUtility.FromJson<ScoreMetadata>(metadata);

            return string.IsNullOrEmpty(data?.result)
                ? "—"
                : data.result;
        }
        catch
        {
            return "—";
        }
    }
}

