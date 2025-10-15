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

    private LifeController lifeController;
    private GridManager gridManager;

    private void Start()
    {
        lifeController = GetComponent<LifeController>();
        gridManager = GetComponent<GridManager>();
        
        updateIntervalSlider.minValue = 0.01f;
        updateIntervalSlider.maxValue = 2.0f;
        updateIntervalSlider.value = lifeController.UpdateInterval;
        
        randomFillSlider.minValue = 0f;
        randomFillSlider.maxValue = 1f;
        randomFillSlider.value = lifeController.RandomFillShare;
        
        updateIntervalSlider.onValueChanged.AddListener(OnUpdateIntervalChanged);
        simulationButton.onClick.AddListener(OnSimulationButtonClicked);
        randomFillToggle.onValueChanged.AddListener(OnRandomFillToggleChanged);
        randomFillSlider.onValueChanged.AddListener(OnRandomFillSliderChanged);
        
        
        UpdateUI();
    }

    private void Update()
    {
        UpdateAliveCellsCount();
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
        // Можно добавить логику для немедленного применения или просто сохранить настройку
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

    private void UpdateSimulationButtonText()
    {
        simulationButtonText.text = lifeController.IsSimulating ? "Pause" : "Start";
    }

    private void UpdateUI()
    {
        UpdateSimulationButtonText();
        updateIntervalValueText.text = updateIntervalSlider.value.ToString("F2") + "s";
    }
}