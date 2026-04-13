
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[Serializable]
public class RadioButtonElement
{
    public Button ButtonElement;
    public Text ButtonElementText;
    public bool _value;
    private Action<RadioButtonElement> _callback;
    public UnityEvent<int> OnToggleButtonValue;
    private int _indexInList;

    public void Subscribe(Action<RadioButtonElement> callback, int index)
    {
        _indexInList = index;
        _callback = callback;
        ButtonElement.onClick.AddListener(_onClicked);
        SetButtonValue(_value); // set button state to initial value (will also trigger the UnityEvent in case that initial value is true)
    }
    private void _onClicked()
    {
        _callback?.Invoke(this);
    }

    public void SetButtonValue(bool val)
    {
        _value = val;
        if (_value) ButtonElementText.text = "o";   // looks like a checkmark dot
        else ButtonElementText.text = "";

        if (_value) // only trigger unity event if value = true
            OnToggleButtonValue?.Invoke(_indexInList);
    }

}