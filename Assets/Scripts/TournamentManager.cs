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
    [SerializeField] private GameObject tournamentPanel;
    [SerializeField] private TextMeshProUGUI playerTurnText;
    [SerializeField] private TextMeshProUGUI remainingText;
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private TextMeshProUGUI resultsText;
    [SerializeField] private Button endTurnButton;

    [Header("Mode Toggles")]
    [SerializeField] private Toggle cellsModeToggle;
    [SerializeField] private Toggle zonesModeToggle;

    private LifeController _lifeController;
    private GridManager _gridManager;
    private RuleZoneManager _zoneManager;
    private StructuresUI _structuresUI;

    private int _currentRound = 0;
    private int _player1Wins = 0;
    private int _player2Wins = 0;
    private TournamentState _currentState = TournamentState.Player1Zones;
    private int _player1ZonesPlaced = 0;
    private int _player2CellsPlaced = 0;

    public bool TournamentMode => tournamentMode;
    public TournamentState CurrentState => _currentState;

    private void Start()
    {
        _lifeController = GetComponent<LifeController>();
        _gridManager = GetComponent<GridManager>();
        _zoneManager = GetComponent<RuleZoneManager>();
        _structuresUI = GetComponent<StructuresUI>();

        endTurnButton.onClick.RemoveAllListeners();
        endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);

        if (tournamentMode)
        {
            StartTournament();
        }
        else
        {
            tournamentPanel.SetActive(false);
        }
    }

    public void StartTournament()
    {
        _player1ZonesPlaced = 0;
        _player2CellsPlaced = 0;
        _zoneManager.ClearAllZones();
        _gridManager.RandomizeGrid(0f);
        _currentRound = 0;
        
        _player1Wins = 0;
        _player2Wins = 0;
        tournamentMode = true;
        SetTogglesInteractable(false);
        
        tournamentPanel.SetActive(true);
        resultsPanel.SetActive(false);

        StartNextRound();
    }
    
    private void OnEndTurnButtonClicked()
    {
        switch (_currentState)
        {
            case TournamentState.Player1Zones:
                AdvanceToPlayer2();
                break;
            case TournamentState.Player2Cells:
                StartSimulation();
                break;
        }
    }

    private void SetTogglesInteractable(bool interactable)
    {
        if (cellsModeToggle) cellsModeToggle.interactable = interactable;
        if (zonesModeToggle) zonesModeToggle.interactable = interactable;
    }

    private void StartNextRound()
    {
        ++_currentRound;
        if (_currentRound > totalRounds)
        {
            EndTournament();
            return;
        }
        
        zonesModeToggle.isOn = true;
        cellsModeToggle.isOn = false;
        endTurnButton.onClick.RemoveAllListeners();
        endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);

        _currentState = TournamentState.Player1Zones;
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
        tournamentStatusText.text = $"Round: {_currentRound}/{totalRounds}";
        
        switch (_currentState)
        {
            case TournamentState.Player1Zones:
                remainingText.text = $"Zones Remaining: {maxZonesPerPlayer - _player1ZonesPlaced}";
                break;
            case TournamentState.Player2Cells:
                playerTurnText.text = "Player 2: Place Cells";
                remainingText.text = $"Cells Remaining: {maxCellsPerPlayer - _player2CellsPlaced}";
                break;
            case TournamentState.Simulating:
                playerTurnText.text = "Simulating...";
                remainingText.text = "";
                break;
        }
    }

    private void UpdateStructuresUI()
    {
        if (_currentState == TournamentState.Player1Zones)
        {
            if (_structuresUI)
            {
                var toggles = _structuresUI.GetComponentsInChildren<Toggle>();
                foreach (var toggle in toggles)
                {
                    if (toggle.name.Contains("Zones")) toggle.isOn = true;
                    if (toggle.name.Contains("Cells")) toggle.isOn = false;
                }
            }
        }
        else if (_currentState == TournamentState.Player2Cells)
        {
            if (_structuresUI)
            {
                var toggles = _structuresUI.GetComponentsInChildren<Toggle>();
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
               _currentState == TournamentState.Player1Zones && 
               _player1ZonesPlaced < maxZonesPerPlayer;
    }

    public bool CanPlaceCell()
    {
        return tournamentMode && 
               _currentState == TournamentState.Player2Cells && 
               _player2CellsPlaced < maxCellsPerPlayer;
    }

    public void OnZonePlaced()
    {
        if (CanPlaceZone())
        {
            ++_player1ZonesPlaced;
            
            if (_player1ZonesPlaced >= maxZonesPerPlayer)
            {
                AdvanceToPlayer2();
            }
        }
    }

    public void OnCellPlaced()
    {
        if (CanPlaceCell())
        {
            ++_player2CellsPlaced;
            
            if (_player2CellsPlaced >= maxCellsPerPlayer)
            {
                StartSimulation();
            }
        }
    }

    private void AdvanceToPlayer2()
    {
        zonesModeToggle.isOn = false;
        cellsModeToggle.isOn = true;
        _currentState = TournamentState.Player2Cells;
        UpdateStructuresUI();
    }

    private void StartSimulation()
    {
        zonesModeToggle.isOn = false;
        cellsModeToggle.isOn = true;
        endTurnButton.interactable = false;
        _currentState = TournamentState.Simulating;
        StartCoroutine(RunSimulation());
    }

    private IEnumerator RunSimulation()
    {
        _lifeController.ToggleSimulation();
        
        for (int i = 0; i < simulationsPerRound; ++i)
        {
            yield return new WaitForSeconds(_lifeController.UpdateInterval);
        }
        
        _lifeController.ToggleSimulation();
        
        bool cellsAlive = CheckLivingCells();
        if (cellsAlive)
        {
            ++_player1Wins;
        }
        else
        {
            ++_player2Wins;
        }
        
        ShowRoundResult(cellsAlive);
    }

    private bool CheckLivingCells()
    {
        for (int x = 0; x < _gridManager.Width; ++x)
        {
            for (int y = 0; y < _gridManager.Height; ++y)
            {
                if (_gridManager.GetCellState(x, y))
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
        resultsText.text = $"Round {_currentRound} Result: {result}\n" +
                           $"Score: Player 1 - {_player1Wins} | Player 2 - {_player2Wins}";
        
        resultsPanel.SetActive(true);
        endTurnButton.interactable = true;
        endTurnButton.onClick.RemoveAllListeners();
        endTurnButton.onClick.AddListener(() => {
            resultsPanel.SetActive(false);
            StartNextRound();
        });
    }

    public void EndTournament()
    {
        string finalResult;
        if (_player1Wins > _player2Wins)
        {
            finalResult = "Player 1 Wins the Tournament!";
        }
        else if (_player2Wins > _player1Wins)
        {
            finalResult = "Player 2 Wins the Tournament!";
        }
        else
        {
            finalResult = "Tournament Ended in a Tie!";
        }

        resultsText.text = $"{finalResult}\nFinal Score: {_player1Wins} - {_player2Wins}";
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