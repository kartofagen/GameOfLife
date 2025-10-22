using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class TournamentManager : MonoBehaviour
{
    [Header("Tournament Settings")]
    [SerializeField] private bool isTournament = false;
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
    
    [Header("Outer UI References")]
    [SerializeField] private Toggle randomizeToggle;
    [SerializeField] private Button simulationButton;

    [Header("Mode Toggles")]
    [SerializeField] private Toggle cellsModeToggle;
    [SerializeField] private Toggle zonesModeToggle;

    private LifeController _lifeController;
    private GridManager _gridManager;
    private RuleZoneManager _zoneManager;
    private StructuresUI _structuresUI;

    private int _currentRound = 0;
    private TournamentState _currentState = TournamentState.Player1Zones;
    private int _player1ZonesPlaced = 0;
    private int _player2CellsPlaced = 0;
    private int _simulationProgress = 0;

    public bool IsTournament => isTournament;
    public TournamentState CurrentState => _currentState;

    private void Start()
    {
        _lifeController = GetComponent<LifeController>();
        _gridManager = GetComponent<GridManager>();
        _zoneManager = GetComponent<RuleZoneManager>();
        _structuresUI = GetComponent<StructuresUI>();

        endTurnButton.onClick.RemoveAllListeners();
        endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);

        if (isTournament)
        {
            StartTournament();
        }
        else
        {
            tournamentPanel.SetActive(false);
            resultsPanel.SetActive(false);
        }
    }

    public void StartTournament()
    {
        _player1ZonesPlaced = 0;
        _player2CellsPlaced = 0;
        _zoneManager.ClearAllZones();
        _gridManager.RandomizeGrid(0f);
        _currentRound = 0;
        
        isTournament = true;
        SetTogglesInteractable(false);
        SetOuterUIInteractable(false);
        
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
        cellsModeToggle.interactable = interactable;
        zonesModeToggle.interactable = interactable;
    }
    
    private void SetOuterUIInteractable(bool interactable)
    {
        randomizeToggle.interactable = interactable;
        simulationButton.interactable = interactable;
    }

    private void StartNextRound()
    {
        ++_currentRound;
        if (_currentRound > totalRounds)
        {
            EndTournament();
            return;
        }
        
        endTurnButton.onClick.RemoveAllListeners();
        endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);

        _currentState = TournamentState.Player1Zones;
        UpdateUI();
        UpdateStructuresUI();
    }

    private void Update()
    {
        if (!isTournament) return;

        UpdateUI();
    }

    private void UpdateUI()
    {
        tournamentStatusText.text = $"Round: {_currentRound}/{totalRounds}";
        
        switch (_currentState)
        {
            case TournamentState.Player1Zones:
                playerTurnText.text = "P1: Place Poison";
                remainingText.text = $"Poisons: {maxZonesPerPlayer - _player1ZonesPlaced}";
                break;
            case TournamentState.Player2Cells:
                playerTurnText.text = "P2: Place Cockroaches";
                remainingText.text = $"Cockroaches: {maxCellsPerPlayer - _player2CellsPlaced}";
                break;
            case TournamentState.Simulating:
                playerTurnText.text = $"Survival: {_simulationProgress}/{simulationsPerRound}";
                remainingText.text = "";
                break;
        }
    }

    private void UpdateStructuresUI()
    {
        if (_currentState == TournamentState.Player1Zones)
        {
            zonesModeToggle.isOn = true;
            cellsModeToggle.isOn = false;
        }
        else if (_currentState == TournamentState.Player2Cells)
        {
            zonesModeToggle.isOn = false;
            cellsModeToggle.isOn = true;
        }
        else
        {
            zonesModeToggle.isOn = false;
            cellsModeToggle.isOn = false;
        }
    }

    public bool CanPlaceZone()
    {
        return isTournament && 
               _currentState == TournamentState.Player1Zones && 
               _player1ZonesPlaced < maxZonesPerPlayer;
    }

    public bool CanPlaceCell()
    {
        return isTournament && 
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
    
    public void OnCellRemoved()
    {
        if (isTournament && _currentState == TournamentState.Player2Cells && _player2CellsPlaced > 0)
        {
            --_player2CellsPlaced;
            UpdateUI();
        }
    }

    public void OnZoneRemoved()
    {
        if (isTournament && _currentState == TournamentState.Player1Zones && _player1ZonesPlaced > 0)
        {
            --_player1ZonesPlaced;
            UpdateUI();
        }
    }

    private void AdvanceToPlayer2()
    {
        _currentState = TournamentState.Player2Cells;
        UpdateStructuresUI();
    }

    private void StartSimulation()
    {
        endTurnButton.interactable = false;
        _currentState = TournamentState.Simulating;
        UpdateStructuresUI();
        StartCoroutine(RunSimulation());
    }
    
    private IEnumerator RunSimulation()
    {
        _lifeController.ToggleSimulation();
    
        for (int i = 0; i < simulationsPerRound; ++i)
        {
            yield return new WaitForSeconds(_lifeController.UpdateInterval);
            _simulationProgress = i + 1;
        }
    
        _lifeController.ToggleSimulation();
    
        int aliveCount = CountAliveCells();
        ShowRoundResult(aliveCount);
    }
    
    private int CountAliveCells()
    {
        int count = 0;
        for (int x = 0; x < _gridManager.Width; ++x)
        {
            for (int y = 0; y < _gridManager.Height; ++y)
            {
                if (_gridManager.GetCellState(x, y))
                {
                    ++count;
                }
            }
        }
        return count;
    }

    private void ShowRoundResult(int aliveCount)
    {
        resultsText.text = $"Round {_currentRound} Complete!\nAlive Cockroaches: {aliveCount}";
    
        resultsPanel.SetActive(true);
        endTurnButton.interactable = true;
        endTurnButton.onClick.RemoveAllListeners();
        endTurnButton.onClick.AddListener(() => {
            resultsPanel.SetActive(false);
            StartNextRound();
        });
    }
    
    public void AbortTournament()
    {
        if (isTournament)
        {
            isTournament = false;
            SetTogglesInteractable(true);
            SetOuterUIInteractable(true);
        
            tournamentPanel.SetActive(false);
            resultsPanel.SetActive(false);
        
            _currentState = TournamentState.Player1Zones;
            _player1ZonesPlaced = 0;
            _player2CellsPlaced = 0;
        
            _zoneManager.ClearAllZones();
            _gridManager.RandomizeGrid(0f);
        
            UpdateUI();
        }
    }

    public void EndTournament()
    {
        bool cellsAlive = CountAliveCells() > 0;
        string finalResult;
    
        if (cellsAlive)
        {
            finalResult = "COCKROACHES GOD WINS!";
        }
        else
        {
            finalResult = "EXTERMINATOR WINS!";
        }

        resultsText.text = $"{finalResult}\nWhy: {(cellsAlive ? "Cockroaches Survived" : "All Cockroaches Eliminated")}";
        resultsPanel.SetActive(true);

        StartCoroutine(SwitchToFreeModeAfterDelay(3f));
    }

    private IEnumerator SwitchToFreeModeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
    
        tournamentPanel.SetActive(false);
        resultsPanel.SetActive(false);
    
        isTournament = false;
        SetTogglesInteractable(true);
        SetOuterUIInteractable(true);
    
        _currentState = TournamentState.Player1Zones;
        _player1ZonesPlaced = 0;
        _player2CellsPlaced = 0;
    }
}

public enum TournamentState
{
    Player1Zones,
    Player2Cells,
    Simulating
}