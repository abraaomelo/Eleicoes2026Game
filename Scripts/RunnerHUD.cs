using TMPro;
using UnityEngine;

public class RunnerHUD : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private GameController gameController;
    [SerializeField] private TMP_Text proposalsText;
    [SerializeField] private TMP_Text distanceText;

    private int lastProposalPoints = -1;
    private int lastDistance = -1;

    private void OnEnable()
    {
        lastProposalPoints = -1;
        lastDistance = -1;
    }

    private void LateUpdate()
    {
        if (gameController == null)
            return;

        int proposalPoints = gameController.TotalProposalPoints;
        int distance = Mathf.FloorToInt(
            gameController.DistanceMeters
        );

        if (proposalsText != null &&
            proposalPoints != lastProposalPoints)
        {
            proposalsText.text = $"{proposalPoints}";
            lastProposalPoints = proposalPoints;
        }

        if (distanceText != null &&
            distance != lastDistance)
        {
            distanceText.text = $"{distance}m";
            lastDistance = distance;
        }
    }
}


