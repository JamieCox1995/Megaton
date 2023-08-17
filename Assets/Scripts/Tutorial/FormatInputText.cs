using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FormatInputText : MonoBehaviour
{
    public string TextToFormat;
    public string Format;

    private Text _textComponent;

    void Start()
    {
        _textComponent = gameObject.GetComponent<Text>();

        if (_textComponent == null) _textComponent = gameObject.AddComponent<Text>();
    }
    
	void Update()
    {
        _textComponent.text = this.TextToFormat.FormatInputAxes(this.Format);
    }
}
