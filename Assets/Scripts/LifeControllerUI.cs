using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LifeControllerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider updateIntervalSlider;
    [SerializeField] private TextMeshProUGUI updateIntervalValueText;
    [SerializeField] private Button simulationButton;
    [SerializeField] private TextMeshProUGUI simulationButtonText;
    [SerializeField] private Toggle randomFillToggle;
    [SerializeField] private Slider randomFillSlider;
    [SerializeField] private TextMeshProUGUI randomFillValueText;
    [SerializeField] private TextMeshProUGUI aliveCellsText; 
    [SerializeField] private Button tournamentModeButton;
    [SerializeField] private TextMeshProUGUI tournamentButtonText;
    [SerializeField] private Button clearZonesButton;

    private LifeController _lifeController;
    private GridManager _gridManager;
    private TournamentManager _tournamentManager;

    private void Start()
    {
        _lifeController = GetComponent<LifeController>();
        _gridManager = GetComponent<GridManager>();
        _tournamentManager = GetComponent<TournamentManager>();
        
        updateIntervalSlider.onValueChanged.AddListener(OnUpdateIntervalChanged);
        simulationButton.onClick.AddListener(OnSimulationButtonClicked);
        randomFillToggle.onValueChanged.AddListener(OnRandomFillToggleChanged);
        randomFillSlider.onValueChanged.AddListener(OnRandomFillSliderChanged);
        clearZonesButton.onClick.AddListener(OnClearZonesButtonClicked);
        tournamentModeButton.onClick.AddListener(OnTournamentModeButtonClicked);
        
        UpdateTournamentButtonText();
        UpdateSimulationButtonText();
        UpdateSliders();
        UpdateClearZonesButton();
    }

    private void Update()
    {
        UpdateAliveCellsCount();
    }
    
    private void OnClearZonesButtonClicked()
    {
        RuleZoneManager zoneManager = GetComponent<RuleZoneManager>();
        if (zoneManager != null && !_tournamentManager.IsTournament)
        {
            zoneManager.ClearAllZones();
        }
    }
    
    private void UpdateClearZonesButton()
    {
        if (clearZonesButton != null)
        {
            clearZonesButton.interactable = !_tournamentManager.IsTournament;
        }
    }
    
    private void OnTournamentModeButtonClicked()
    {
        if (_tournamentManager.IsTournament)
        {
            _tournamentManager.AbortTournament();
        }
        else
        {
            _tournamentManager.StartTournament();
        }

        UpdateTournamentButtonText();
    }

    private void UpdateTournamentButtonText()
    {
        if (tournamentButtonText != null)
        {
            tournamentButtonText.text = _tournamentManager.IsTournament ? "Free Mode" : "Tournament";
        }
        
        UpdateClearZonesButton();
    }

    private void OnUpdateIntervalChanged(float value)
    {
        _lifeController.SetUpdateInterval(value);
        updateIntervalValueText.text = value.ToString("F2") + "s";
    }

    private void OnRandomFillSliderChanged(float value)
    {
        _lifeController.SetRandomFillShare(value);
        randomFillValueText.text = (value * 100).ToString("F0") + "%";
        
        if (randomFillToggle.isOn)
        {
            _lifeController.RandomizeGrid();
        }
    }

    private void OnSimulationButtonClicked()
    {
        _lifeController.ToggleSimulation();
        UpdateSimulationButtonText();
    }

    private void OnRandomFillToggleChanged(bool isOn)
    {
        if (isOn)
        {
            _lifeController.RandomizeGrid();
        }
    }

    private void UpdateAliveCellsCount()
    {
        int aliveCount = 0;
        for (int x = 0; x < _gridManager.Width; ++x)
        {
            for (int y = 0; y < _gridManager.Height; ++y)
            {
                if (_gridManager.GetCellState(x, y))
                {
                    ++aliveCount;
                }
            }
        }
        aliveCellsText.text = $"Alive Cockroaches: {aliveCount}";
    }

    private void UpdateSimulationButtonText()
    {
        simulationButtonText.text = _lifeController.IsSimulating ? "Pause" : "Start";
    }
    
    private void UpdateSliders()
    {
        updateIntervalSlider.value = _lifeController.UpdateInterval;
        randomFillSlider.value = _lifeController.RandomFillShare;
        updateIntervalValueText.text = updateIntervalSlider.value.ToString("F2") + "s";
        randomFillValueText.text = (randomFillSlider.value * 100).ToString("F0") + "%";
    }
}