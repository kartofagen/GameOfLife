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
    [SerializeField] private int maxZonesPerTurn = 3;
    [SerializeField] private int maxCellsPerTurn = 10;

    [Header("UI References")]
    [SerializeField] private GameObject competitionPanel;
    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private TextMeshProUGUI phaseText;
    [SerializeField] private TextMeshProUGUI zonesLeftText;
    [SerializeField] private TextMeshProUGUI cellsLeftText;
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private TextMeshProUGUI resultsText;

    private LifeController lifeController;
    private GridManager gridManager;
    private RuleZoneManager zoneManager;
    private StructuresUI structuresUI;

    private int currentTurn = 0;
    private CompetitionPhase currentPhase = CompetitionPhase.Player1Zones;
    private int zonesPlacedThisTurn = 0;
    private int cellsPlacedThisTurn = 0;
    private int player1Wins = 0;
    private int player2Wins = 0;

    public bool CompetitionMode => competitionMode;

    private enum CompetitionPhase
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

        if (competitionMode)
        {
            StartCompetition();
        }
        else
        {
            competitionPanel.SetActive(false);
        }
    }

    private void StartCompetition()
    {
        currentTurn = 0;
        player1Wins = 0;
        player2Wins = 0;
        competitionPanel.SetActive(true);
        resultsPanel.SetActive(false);
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

        zonesPlacedThisTurn = 0;
        cellsPlacedThisTurn = 0;
        
        // Clear previous turn
        ClearGrid();
        zoneManager.ClearAllZones();
        
        StartPhase(CompetitionPhase.Player1Zones);
    }

    private void StartPhase(CompetitionPhase phase)
    {
        currentPhase = phase;
        UpdateUI();

        switch (phase)
        {
            case CompetitionPhase.Player1Zones:
                structuresUI.SetMode(StructuresUI.PlacementMode.Zones);
                break;
            case CompetitionPhase.Player2Cells:
                structuresUI.SetMode(StructuresUI.PlacementMode.Cells);
                break;
            case CompetitionPhase.Simulation:
                StartSimulation();
                break;
        }
    }

    private void UpdateUI()
    {
        turnText.text = $"Turn: {currentTurn}/{maxTurns}";
        zonesLeftText.text = $"Zones Left: {maxZonesPerTurn - zonesPlacedThisTurn}";
        cellsLeftText.text = $"Cells Left: {maxCellsPerTurn - cellsPlacedThisTurn}";

        switch (currentPhase)
        {
            case CompetitionPhase.Player1Zones:
                phaseText.text = "Player 1: Place Zones";
                break;
            case CompetitionPhase.Player2Cells:
                phaseText.text = "Player 2: Place Cells";
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
               zonesPlacedThisTurn < maxZonesPerTurn;
    }

    public bool CanPlaceCell()
    {
        return competitionMode && 
               currentPhase == CompetitionPhase.Player2Cells && 
               cellsPlacedThisTurn < maxCellsPerTurn;
    }

    public void OnZonePlaced()
    {
        if (!CanPlaceZone()) return;
        
        zonesPlacedThisTurn++;
        UpdateUI();

        if (zonesPlacedThisTurn >= maxZonesPerTurn)
        {
            StartPhase(CompetitionPhase.Player2Cells);
        }
    }

    public void OnCellPlaced()
    {
        if (!CanPlaceCell()) return;
        
        cellsPlacedThisTurn++;
        UpdateUI();

        if (cellsPlacedThisTurn >= maxCellsPerTurn)
        {
            StartPhase(CompetitionPhase.Simulation);
        }
    }

    private void StartSimulation()
    {
        StartCoroutine(RunSimulation());
    }

    private IEnumerator RunSimulation()
    {
        lifeController.ToggleSimulation();
        
        for (int i = 0; i < iterationsPerTurn; i++)
        {
            yield return new WaitForSeconds(lifeController.UpdateInterval);
        }
        
        lifeController.ToggleSimulation();
        EvaluateTurn();
    }

    private void EvaluateTurn()
    {
        bool hasLiveCells = CheckLiveCells();
        
        if (hasLiveCells)
        {
            player1Wins++;
            resultsText.text = $"Turn {currentTurn}: Player 1 Wins!";
        }
        else
        {
            player2Wins++;
            resultsText.text = $"Turn {currentTurn}: Player 2 Wins!";
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
        
        // Return to free mode
        competitionMode = false;
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
        }
    }
}