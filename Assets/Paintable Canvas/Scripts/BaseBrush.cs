using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName ="Brush", menuName ="Painting/Brush")]
public class BaseBrush : ScriptableObject
{
    public string DisplayName;
    public Texture2D BrushTexture;
    public bool bIsTintable = true;
    public bool bIsStamp = false;
    public bool bIsBucket = false;
    public bool bIsEraser = false;

 
    public Color Apply(Color InCurrentColor, Color InBrushColor, Color InTintColor, float InWeight)
    {
        Color DesiredColor = bIsTintable ? InTintColor : InBrushColor;

        float Intensity = InWeight * (bIsTintable ? InBrushColor.r : 1f);

        return Color.Lerp(InCurrentColor, DesiredColor, Intensity);
    }
     

}
