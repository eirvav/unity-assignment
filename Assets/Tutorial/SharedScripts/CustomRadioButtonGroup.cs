
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CustomRadioButtonGroup : MonoBehaviour
{
    public List<RadioButtonElement> RadioButtonElements = new();
    void Awake()
    {
        for (int i = 0; i < RadioButtonElements.Count; i++)
        {
            RadioButtonElements[i].Subscribe(OnButtonClicked, i);
        }
    }

    private void OnButtonClicked(RadioButtonElement element)
    {
        for (int i = 0; i < RadioButtonElements.Count; i++)
        {
            var listElem = RadioButtonElements[i];
            bool value;
            if (element != listElem)
                value = false;
            else
                value = true;

            listElem.SetButtonValue(value);
        }
    }
}