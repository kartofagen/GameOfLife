using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class CompetitionManager : MonoBehaviour
{
    [Header("Competition Settings")]
    [SerializeField] private bool competitionMode = false;
    [SerializeField] private int maxTurns = 10;
    [SerializeField] private int iterationsPerTurn = 100;
    [SerializeField] private int totalZonesForGame = 5;
    [SerializeField] private int totalCellsForGame = 20;

    [Header("UI References")]
    [SerializeField] private GameObject competitionPanel;
    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private TextMeshProUGUI phaseText;
    [SerializeField] private TextMeshProUGUI zonesLeftText;
    [SerializeField] private TextMeshProUGUI cellsLeftText;
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private TextMeshProUGUI resultsText;
    [SerializeField] private Button endTurnButton;

    private LifeController lifeController;
    private GridManager gridManager;
    private RuleZoneManager zoneManager;
    private StructuresUI structuresUI;

    private int currentTurn = 0;
    private CompetitionPhase currentPhase = CompetitionPhase.Player1Zones;
    private int zonesPlacedTotal = 0;
    private int cellsPlacedTotal = 0;
    private int player1Wins = 0;
    private int player2Wins = 0;
    private bool isSimulating = false;

    public bool CompetitionMode => competitionMode;
    public CompetitionPhase CurrentPhase => currentPhase;

    public enum CompetitionPhase
    {
        Player1Zones,
        Player2Cells,
        Simulation,
        TurnResults
    }

    private void Start()
    {
        lifeController = GetComponent<LifeController>();
        gridManager = GetComponent<GridManager>();
        zoneManager = GetComponent<RuleZoneManager>();
        structuresUI = GetComponent<StructuresUI>();

        if (endTurnButton != null)
        {
            endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);
        }

        if (competitionMode)
        {
            StartCompetition();
        }
        else
        {
            competitionPanel.SetActive(false);
            if (endTurnButton != null) endTurnButton.gameObject.SetActive(false);
        }
    }

    private void StartCompetition()
    {
        currentTurn = 0;
        player1Wins = 0;
        player2Wins = 0;
        zonesPlacedTotal = 0;
        cellsPlacedTotal = 0;
        competitionPanel.SetActive(true);
        resultsPanel.SetActive(false);
        if (endTurnButton != null) endTurnButton.gameObject.SetActive(true);
        StartNextTurn();
    }

    private void StartNextTurn()
    {
        currentTurn++;
        if (currentTurn > maxTurns)
        {
            EndCompetition();
            return;
        }

        ClearGrid();
        
        StartPhase(CompetitionPhase.Player1Zones);
    }

    private void StartPhase(CompetitionPhase phase)
    {
        currentPhase = phase;
        UpdateUI();

        switch (phase)
        {
            case CompetitionPhase.Player1Zones:
                SetStructuresUIMode(StructuresUI.PlacementMode.Zones);
                if (endTurnButton != null) endTurnButton.gameObject.SetActive(true);
                break;
            case CompetitionPhase.Player2Cells:
                SetStructuresUIMode(StructuresUI.PlacementMode.Cells);
                if (endTurnButton != null) endTurnButton.gameObject.SetActive(true);
                break;
            case CompetitionPhase.Simulation:
                SetStructuresUIMode(StructuresUI.PlacementMode.Cells); // Neutral mode
                if (endTurnButton != null) endTurnButton.gameObject.SetActive(false);
                StartSimulation();
                break;
            case CompetitionPhase.TurnResults:
                SetStructuresUIMode(StructuresUI.PlacementMode.Cells); // Neutral mode
                if (endTurnButton != null) endTurnButton.gameObject.SetActive(false);
                break;
        }
    }

    private void SetStructuresUIMode(StructuresUI.PlacementMode mode)
    {
        if (structuresUI != null)
        {
            // Use reflection to call the private method since we can't modify StructuresUI extensively
            System.Reflection.MethodInfo method = typeof(StructuresUI).GetMethod("SetMode", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method != null)
            {
                method.Invoke(structuresUI, new object[] { mode });
            }
        }
    }

    private void UpdateUI()
    {
        turnText.text = $"Turn: {currentTurn}/{maxTurns}";
        zonesLeftText.text = $"Zones Left: {totalZonesForGame - zonesPlacedTotal}";
        cellsLeftText.text = $"Cells Left: {totalCellsForGame - cellsPlacedTotal}";

        switch (currentPhase)
        {
            case CompetitionPhase.Player1Zones:
                phaseText.text = "Player 1: Place Zones";
                if (endTurnButton != null) endTurnButton.GetComponentInChildren<TextMeshProUGUI>().text = "End Zone Placement";
                break;
            case CompetitionPhase.Player2Cells:
                phaseText.text = "Player 2: Place Cells (Right-click to remove)";
                if (endTurnButton != null) endTurnButton.GetComponentInChildren<TextMeshProUGUI>().text = "End Cell Placement";
                break;
            case CompetitionPhase.Simulation:
                phaseText.text = "Simulating...";
                break;
            case CompetitionPhase.TurnResults:
                phaseText.text = "Turn Complete";
                break;
        }
    }

    public bool CanPlaceZone()
    {
        return competitionMode && 
               currentPhase == CompetitionPhase.Player1Zones && 
               zonesPlacedTotal < totalZonesForGame;
    }

    public bool CanPlaceCell()
    {
        return competitionMode && 
               currentPhase == CompetitionPhase.Player2Cells && 
               cellsPlacedTotal < totalCellsForGame;
    }

    public void OnZonePlaced()
    {
        if (!CanPlaceZone()) return;
        
        zonesPlacedTotal++;
        UpdateUI();
    }

    public void OnCellPlaced()
    {
        if (!CanPlaceCell()) return;
        
        cellsPlacedTotal++;
        UpdateUI();
    }

    public void OnZoneRemoved()
    {
        zonesPlacedTotal = Mathf.Max(0, zonesPlacedTotal - 1);
        UpdateUI();
    }

    public void OnCellRemoved()
    {
        cellsPlacedTotal = Mathf.Max(0, cellsPlacedTotal - 1);
        UpdateUI();
    }

    private void OnEndTurnButtonClicked()
    {
        switch (currentPhase)
        {
            case CompetitionPhase.Player1Zones:
                StartPhase(CompetitionPhase.Player2Cells);
                break;
            case CompetitionPhase.Player2Cells:
                StartPhase(CompetitionPhase.Simulation);
                break;
        }
    }

    private void StartSimulation()
    {
        StartCoroutine(RunSimulation());
    }

    private IEnumerator RunSimulation()
    {
        isSimulating = true;
        lifeController.ToggleSimulation();
        
        for (int i = 0; i < iterationsPerTurn; i++)
        {
            yield return new WaitForSeconds(lifeController.UpdateInterval);
        }
        
        lifeController.ToggleSimulation();
        isSimulating = false;
        EvaluateTurn();
    }

    private void EvaluateTurn()
    {
        bool hasLiveCells = CheckLiveCells();
        
        if (hasLiveCells)
        {
            player1Wins++;
            resultsText.text = $"Turn {currentTurn}: Player 1 Wins!\nLive cells survived!";
        }
        else
        {
            player2Wins++;
            resultsText.text = $"Turn {currentTurn}: Player 2 Wins!\nAll cells died!";
        }

        StartPhase(CompetitionPhase.TurnResults);
        ShowTurnResults();
    }

    private void ShowTurnResults()
    {
        resultsPanel.SetActive(true);
        StartCoroutine(HideResultsAfterDelay(3f));
    }

    private IEnumerator HideResultsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        resultsPanel.SetActive(false);
        StartNextTurn();
    }

    private bool CheckLiveCells()
    {
        for (int x = 0; x < gridManager.Width; x++)
        {
            for (int y = 0; y < gridManager.Height; y++)
            {
                if (gridManager.GetCellState(x, y) && !gridManager.IsWall(x, y))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void ClearGrid()
    {
        for (int x = 0; x < gridManager.Width; x++)
        {
            for (int y = 0; y < gridManager.Height; y++)
            {
                if (!gridManager.IsWall(x, y))
                {
                    gridManager.SetCellState(x, y, false);
                }
            }
        }
    }

    private void EndCompetition()
    {
        competitionPanel.SetActive(false);
        if (endTurnButton != null) endTurnButton.gameObject.SetActive(false);
        
        string finalResult;
        if (player1Wins > player2Wins)
        {
            finalResult = $"Player 1 Wins! {player1Wins}-{player2Wins}";
        }
        else if (player2Wins > player1Wins)
        {
            finalResult = $"Player 2 Wins! {player2Wins}-{player1Wins}";
        }
        else
        {
            finalResult = $"Draw! {player1Wins}-{player2Wins}";
        }

        resultsText.text = $"Match Result: {finalResult}";
        resultsPanel.SetActive(true);
        
        // Return to free mode after showing results
        StartCoroutine(ReturnToFreeModeAfterDelay(5f));
    }

    private IEnumerator ReturnToFreeModeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        competitionMode = false;
        competitionPanel.SetActive(false);
        resultsPanel.SetActive(false);
    }

    public void ToggleCompetitionMode()
    {
        competitionMode = !competitionMode;
        if (competitionMode)
        {
            StartCompetition();
        }
        else
        {
            competitionPanel.SetActive(false);
            resultsPanel.SetActive(false);
            if (endTurnButton != null) endTurnButton.gameObject.SetActive(false);
        }
    }

    public bool IsSimulating => isSimulating;
}