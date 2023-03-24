using UnityEngine;
using UnityEngine.UI;

public class SensitivitySlider : MonoBehaviour
{
    public float sensitivity = 1f;

    private Slider slider;

    void Start()
    {
        slider = GetComponent<Slider>();
        slider.value = sensitivity;
        slider.onValueChanged.AddListener(OnSensitivityChanged);
    }

    void OnSensitivityChanged(float value)
    {
        sensitivity = value;
    }
}
