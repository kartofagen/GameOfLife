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

    private LifeController lifeController;
    private GridManager gridManager;
    private TournamentManager tournamentManager;

    private void Start()
    {
        lifeController = GetComponent<LifeController>();
        gridManager = GetComponent<GridManager>();
        tournamentManager = GetComponent<TournamentManager>();
        
        updateIntervalSlider.value = lifeController.UpdateInterval;
        randomFillSlider.value = lifeController.RandomFillShare;
        
        updateIntervalSlider.onValueChanged.AddListener(OnUpdateIntervalChanged);
        simulationButton.onClick.AddListener(OnSimulationButtonClicked);
        randomFillToggle.onValueChanged.AddListener(OnRandomFillToggleChanged);
        randomFillSlider.onValueChanged.AddListener(OnRandomFillSliderChanged);
        
        if (tournamentModeButton != null)
        {
            tournamentModeButton.onClick.AddListener(OnTournamentModeButtonClicked);
            UpdateTournamentButtonText();
        }
        
        UpdateUI();
    }

    private void Update()
    {
        UpdateAliveCellsCount();
    }
    
    private void OnTournamentModeButtonClicked()
    {
        if (tournamentManager.TournamentMode)
        {
            tournamentManager.EndTournament();
        }
        else
        {
            tournamentManager.StartTournament();
        }

        UpdateTournamentButtonText();
    }

    private void UpdateTournamentButtonText()
    {
        if (tournamentButtonText != null)
        {
            tournamentButtonText.text = tournamentManager.TournamentMode ? 
                "Free Mode" : "Tournament";
        }
    }

    private void OnUpdateIntervalChanged(float value)
    {
        lifeController.SetUpdateInterval(value);
        updateIntervalValueText.text = value.ToString("F2") + "s";
    }

    private void OnRandomFillSliderChanged(float value)
    {
        lifeController.SetRandomFillShare(value);
        randomFillValueText.text = (value * 100).ToString("F0") + "%";
        
        if (randomFillToggle.isOn)
        {
            lifeController.RandomizeGrid();
        }
    }

    private void OnSimulationButtonClicked()
    {
        lifeController.ToggleSimulation();
        UpdateSimulationButtonText();
    }

    private void OnRandomFillToggleChanged(bool isOn)
    {
        if (isOn)
        {
            lifeController.RandomizeGrid();
        }
    }

    private void UpdateAliveCellsCount()
    {
        int aliveCount = 0;
        for (int x = 0; x < gridManager.Width; x++)
        {
            for (int y = 0; y < gridManager.Height; y++)
            {
                if (gridManager.GetCellState(x, y))
                {
                    aliveCount++;
                }
            }
        }
        aliveCellsText.text = $"Alive Cells: {aliveCount}";
    }

    public void UpdateSimulationButtonText()
    {
        simulationButtonText.text = lifeController.IsSimulating ? "Pause" : "Start";
    }

    private void UpdateUI()
    {
        UpdateSimulationButtonText();
        updateIntervalValueText.text = updateIntervalSlider.value.ToString("F2") + "s";
        randomFillValueText.text = randomFillSlider.value.ToString("F0") + "%";
    }
}