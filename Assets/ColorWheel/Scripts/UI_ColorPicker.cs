using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UI_ColorPicker : MonoBehaviour
{

    [SerializeField] UI_ColorWheel ColorWheel;
    [SerializeField] Color DefaultColor = Color.white;
    [SerializeField] List<UI_ColorPickerComponent> ColorPickerComponents;
    [SerializeField] UnityEvent<Color> OnColorChanged = new();

    public Color CurrentColor { get; private set; } = Color.white;

    float CurrentHue;
    float CurrentSaturation;
    float CurrentValue;

    void Start()
    {
        Color.RGBToHSV(DefaultColor, out CurrentHue, out CurrentSaturation, out CurrentValue);

        ColorUpdated();
    }

    void Update()
    {
        
    }

    public void SetNewColor(Color InNewColor)
    {
        CurrentColor = InNewColor;

        ColorUpdated();
    }

    public void OnColorWheelClicked(float InNewHue, float InNewSaturation)
    {
        CurrentHue = InNewHue;
        CurrentSaturation = InNewSaturation;

        ColorUpdated();
    }

    public void OnColorComponentChanged(EUIColorComponent InComponent, float InNewValue)
    {
        if (InComponent == EUIColorComponent.Hue)
            CurrentHue = InNewValue;
        else if (InComponent == EUIColorComponent.Saturation)
            CurrentSaturation = InNewValue;
        else if (InComponent == EUIColorComponent.Value)
            CurrentValue = InNewValue;

        ColorUpdated();
    }

    void ColorUpdated()
    {
        CurrentColor = Color.HSVToRGB(CurrentHue, CurrentSaturation, CurrentValue);

        foreach (var ComponentUI in ColorPickerComponents)
        {
            ComponentUI.SetCurrentColor(CurrentColor, CurrentHue, CurrentSaturation, CurrentValue);
        }

        ColorWheel.SetCurrentColor(CurrentColor, CurrentHue, CurrentSaturation, CurrentValue);

        OnColorChanged.Invoke(CurrentColor);
    }
}
