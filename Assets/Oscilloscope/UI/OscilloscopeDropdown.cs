using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OscilloscopeDropdown : Dropdown
{
    public OscilloscopeText CaptionText;

    private int itemCount;


    protected override void Start()
    {
        base.Start();

        this.onValueChanged.AddListener(ValueChanged);
    }

    protected override GameObject CreateDropdownList(GameObject template)
    {
        itemCount = 0;

        return base.CreateDropdownList(template);
    }

    protected override DropdownItem CreateItem(DropdownItem itemTemplate)
    {
        var result = base.CreateItem(itemTemplate);

        var text = result.GetComponentInChildren<OscilloscopeText>();

        if (text != null)
        {
            text.Text = options[itemCount].text;
        }

        itemCount++;

        return result;
    }

    private void ValueChanged(int index)
    {
        if (this.CaptionText != null) this.CaptionText.Text = options[index].text;
    }
}
