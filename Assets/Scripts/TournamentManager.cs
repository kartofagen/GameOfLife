using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class TournamentManager : MonoBehaviour
{
    [Header("Tournament Settings")]
    [SerializeField] private bool tournamentMode = false;
    [SerializeField] private int totalRounds = 10;
    [SerializeField] private int simulationsPerRound = 100;
    [SerializeField] private int maxZonesPerPlayer = 5;
    [SerializeField] private int maxCellsPerPlayer = 50;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI tournamentStatusText;
    [SerializeField] private TextMeshProUGUI playerTurnText;
    [SerializeField] private TextMeshProUGUI zonesRemainingText;
    [SerializeField] private TextMeshProUGUI cellsRemainingText;
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private TextMeshProUGUI resultsText;
    [SerializeField] private Button nextRoundButton;

    [Header("Mode Toggles")]
    [SerializeField] private Toggle cellsModeToggle;
    [SerializeField] private Toggle zonesModeToggle;

    private LifeController lifeController;
    private GridManager gridManager;
    private RuleZoneManager zoneManager;
    private StructuresUI structuresUI;

    private int currentRound = 0;
    private int player1Wins = 0;
    private int player2Wins = 0;
    private TournamentState currentState = TournamentState.Player1Zones;
    private int player1ZonesPlaced = 0;
    private int player2CellsPlaced = 0;

    public bool TournamentMode => tournamentMode;
    public TournamentState CurrentState => currentState;

    private void Start()
    {
        lifeController = GetComponent<LifeController>();
        gridManager = GetComponent<GridManager>();
        zoneManager = GetComponent<RuleZoneManager>();
        structuresUI = GetComponent<StructuresUI>();

        if (tournamentMode)
        {
            StartTournament();
        }
        else
        {
            tournamentStatusText.gameObject.SetActive(false);
            playerTurnText.gameObject.SetActive(false);
        }
    }

    public void StartTournament()
    {
        currentRound = 0;
        player1Wins = 0;
        player2Wins = 0;
        tournamentMode = true;
        SetTogglesInteractable(false);
        
        tournamentStatusText.gameObject.SetActive(true);
        playerTurnText.gameObject.SetActive(true);
        resultsPanel.SetActive(false);

        StartNextRound();
    }
    
    public void SetTogglesInteractable(bool interactable)
    {
        if (cellsModeToggle != null) cellsModeToggle.interactable = interactable;
        if (zonesModeToggle != null) zonesModeToggle.interactable = interactable;
    }

    private void StartNextRound()
    {
        ++currentRound;
        if (currentRound > totalRounds)
        {
            EndTournament();
            return;
        }

        player1ZonesPlaced = 0;
        player2CellsPlaced = 0;
        zoneManager.ClearAllZones();
        gridManager.RandomizeGrid(0f);

        currentState = TournamentState.Player1Zones;
        UpdateUI();
        UpdateStructuresUI();
    }

    private void Update()
    {
        if (!tournamentMode) return;

        UpdateUI();
    }

    private void UpdateUI()
    {
        tournamentStatusText.text = $"Round: {currentRound}/{totalRounds}";
        
        switch (currentState)
        {
            case TournamentState.Player1Zones:
                playerTurnText.text = "Player 1: Place Zones";
                zonesRemainingText.text = $"Zones Remaining: {maxZonesPerPlayer - player1ZonesPlaced}";
                cellsRemainingText.text = "";
                zonesModeToggle.isOn = true;
                cellsModeToggle.isOn = false;
                break;
            case TournamentState.Player2Cells:
                playerTurnText.text = "Player 2: Place Cells";
                zonesRemainingText.text = "";
                cellsRemainingText.text = $"Cells Remaining: {maxCellsPerPlayer - player2CellsPlaced}";
                zonesModeToggle.isOn = false;
                cellsModeToggle.isOn = true;
                break;
            case TournamentState.Simulating:
                playerTurnText.text = "Simulating...";
                zonesRemainingText.text = "";
                cellsRemainingText.text = "";
                zonesModeToggle.isOn = false;
                cellsModeToggle.isOn = true;
                break;
        }
    }

    private void UpdateStructuresUI()
    {
        // Force the correct mode based on tournament state
        if (currentState == TournamentState.Player1Zones)
        {
            // Force zones mode for player 1
            if (structuresUI != null)
            {
                var toggles = structuresUI.GetComponentsInChildren<Toggle>();
                foreach (var toggle in toggles)
                {
                    if (toggle.name.Contains("Zones")) toggle.isOn = true;
                    if (toggle.name.Contains("Cells")) toggle.isOn = false;
                }
            }
        }
        else if (currentState == TournamentState.Player2Cells)
        {
            // Force cells mode for player 2
            if (structuresUI != null)
            {
                var toggles = structuresUI.GetComponentsInChildren<Toggle>();
                foreach (var toggle in toggles)
                {
                    if (toggle.name.Contains("Zones")) toggle.isOn = false;
                    if (toggle.name.Contains("Cells")) toggle.isOn = true;
                }
            }
        }
    }

    public bool CanPlaceZone()
    {
        return tournamentMode && 
               currentState == TournamentState.Player1Zones && 
               player1ZonesPlaced < maxZonesPerPlayer;
    }

    public bool CanPlaceCell()
    {
        return tournamentMode && 
               currentState == TournamentState.Player2Cells && 
               player2CellsPlaced < maxCellsPerPlayer;
    }

    public void OnZonePlaced()
    {
        if (CanPlaceZone())
        {
            player1ZonesPlaced++;
            
            // Auto-advance if player has used all zones
            if (player1ZonesPlaced >= maxZonesPerPlayer)
            {
                AdvanceToPlayer2();
            }
        }
    }

    public void OnCellPlaced()
    {
        if (CanPlaceCell())
        {
            player2CellsPlaced++;
            
            // Auto-advance if player has used all cells
            if (player2CellsPlaced >= maxCellsPerPlayer)
            {
                StartSimulation();
            }
        }
    }

    public void AdvanceToPlayer2()
    {
        if (currentState == TournamentState.Player1Zones)
        {
            currentState = TournamentState.Player2Cells;
            UpdateStructuresUI();
        }
    }

    public void StartSimulation()
    {
        if (currentState == TournamentState.Player2Cells)
        {
            currentState = TournamentState.Simulating;
            StartCoroutine(RunSimulation());
        }
    }

    private IEnumerator RunSimulation()
    {
        lifeController.ToggleSimulation();
        
        // Run simulations
        for (int i = 0; i < simulationsPerRound; i++)
        {
            yield return new WaitForSeconds(lifeController.UpdateInterval);
        }
        
        lifeController.ToggleSimulation();
        
        // Check results
        bool cellsAlive = CheckLivingCells();
        if (cellsAlive)
        {
            player1Wins++;
        }
        else
        {
            player2Wins++;
        }
        
        // Show round result and wait for next round
        ShowRoundResult(cellsAlive);
    }

    private bool CheckLivingCells()
    {
        for (int x = 0; x < gridManager.Width; x++)
        {
            for (int y = 0; y < gridManager.Height; y++)
            {
                if (gridManager.GetCellState(x, y))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void ShowRoundResult(bool player1Won)
    {
        string result = player1Won ? "Player 1 Wins!" : "Player 2 Wins!";
        resultsText.text = $"Round {currentRound} Result: {result}\n" +
                          $"Score: Player 1 - {player1Wins} | Player 2 - {player2Wins}";
        
        resultsPanel.SetActive(true);
        nextRoundButton.onClick.RemoveAllListeners();
        nextRoundButton.onClick.AddListener(() => {
            resultsPanel.SetActive(false);
            StartNextRound();
        });
    }

    public void EndTournament()
    {
        string finalResult;
        if (player1Wins > player2Wins)
        {
            finalResult = "Player 1 Wins the Tournament!";
        }
        else if (player2Wins > player1Wins)
        {
            finalResult = "Player 2 Wins the Tournament!";
        }
        else
        {
            finalResult = "Tournament Ended in a Tie!";
        }

        resultsText.text = $"{finalResult}\nFinal Score: {player1Wins} - {player2Wins}";
        resultsPanel.SetActive(true);

        tournamentMode = false;
        SetTogglesInteractable(true);
    }
}

public enum TournamentState
{
    Player1Zones,
    Player2Cells,
    Simulating
}