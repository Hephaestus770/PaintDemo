using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BrushPicker : MonoBehaviour
{
    [SerializeField] List<BaseBrush> Brushes = new();
    [SerializeField] GameObject BrushUIPreFab;
    [SerializeField] Transform BrushUIRoot;
    [SerializeField] UnityEvent<BaseBrush> OnBrushChanged = new();

    List<BrushElement> BrushUIElements = new();

    void Start()
    {
        foreach (var Brush in Brushes)
        {
            var NewBrushUIGO = GameObject.Instantiate(BrushUIPreFab, BrushUIRoot);
            var NewBrushUILogic = NewBrushUIGO.GetComponent<BrushElement>();

            BrushUIElements.Add(NewBrushUILogic);

            NewBrushUILogic.BindToBrush(Brush);
            NewBrushUILogic.OnBrushSelected.AddListener(OnBrushSelectedInternal);
        }

        
        if (Brushes.Count > 0)
            OnBrushSelectedInternal(Brushes[0]);
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnBrushSelectedInternal(BaseBrush InBrush)
    {
        foreach (var BrushUI in BrushUIElements)
        {
            BrushUI.SetIsSelected(InBrush);
        }

        OnBrushChanged.Invoke(InBrush);
    }
}
