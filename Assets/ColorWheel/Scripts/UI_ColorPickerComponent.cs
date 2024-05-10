using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public enum EUIColorComponent
{
    Hue,
    Saturation,
    Value
}
public class UI_ColorPickerComponent : MonoBehaviour
{

    [SerializeField] EUIColorComponent ComponentType;
    [SerializeField] TextMeshProUGUI ComponentLabel;
    [SerializeField] Slider ComponentSlider;
    [SerializeField] UnityEvent<EUIColorComponent, float> OnColorComponentChanged = new();

    Image SliderHandleImage;


    void Start()
    {

    }
    void Update()
    {
        
    }


    public void OnSliderChanged(float newValue)
    {
        OnColorComponentChanged.Invoke(ComponentType, newValue);
    }

    public void SetCurrentColor(Color InColor, float InHue, float InSaturation, float InValue)
    {
        float ValueToSet = ComponentSlider.value;

        if (ComponentType == EUIColorComponent.Hue)
            ValueToSet = InHue;
        else if (ComponentType == EUIColorComponent.Saturation)
            ValueToSet = InSaturation;
        else if (ComponentType == EUIColorComponent.Value)
            ValueToSet = InValue;

        ComponentSlider.SetValueWithoutNotify(ValueToSet);

        if (SliderHandleImage == null)
            SliderHandleImage = ComponentSlider.handleRect.GetComponent<Image>();

        SliderHandleImage.color = InColor;
    }
}



