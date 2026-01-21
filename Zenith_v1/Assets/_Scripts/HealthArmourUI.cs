using UnityEngine;
using UnityEngine.UI;

public class HealthArmourUI : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] Slider healthSlider;
    [SerializeField] Slider armourSlider;

    void Start()
    {
        if (MainController.Instance == null)
            return;

        // Initialize max values
        healthSlider.maxValue = MainController.Instance.maxHealth;
        armourSlider.maxValue = MainController.Instance.maxArmour;
    }

    void Update()
    {
        if (MainController.Instance == null)
            return;

        UpdateHealth();
        UpdateArmour();
    }

    void UpdateHealth()
    {
        healthSlider.value =
            MainController.Instance.currentHealth;
    }

    void UpdateArmour()
    {
        armourSlider.value =
            MainController.Instance.currentArmour;

        // Optional: hide armour bar when empty
        armourSlider.gameObject.SetActive(
            MainController.Instance.currentArmour > 0
        );
    }
}