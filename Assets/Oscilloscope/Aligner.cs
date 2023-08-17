using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aligner : MonoBehaviour {

    public OscilloscopeText Text;

    public bool AutomaticMode;

#if UNITY_EDITOR
    private void Reset()
    {
        this.Text = GetComponent<OscilloscopeText>();
    }
#endif
	
	// Update is called once per frame
	void Update()
    {
        TextAnchor textAnchor = this.Text.Typesetter.TextAnchor;
        if (this.AutomaticMode)
        {
            int i = Mathf.FloorToInt(Time.time % 10);

            textAnchor = KeyCodeToTextAnchor(KeyCode.Alpha1 + i);
        }
        else
        {
            foreach (var keyCode in KeyCodes(KeyCode.Alpha1, 9))
            {
                if (Input.GetKeyDown(keyCode))
                {
                    textAnchor = KeyCodeToTextAnchor(keyCode);

                    break;
                }
            }
        }

        this.Text.Typesetter.TextAnchor = textAnchor;
    }

    private IEnumerable<KeyCode> KeyCodes(KeyCode start, int count)
    {
        for (int i = 0; i < count; i++)
        {
            yield return start + i;
        }
    }

    private TextAnchor KeyCodeToTextAnchor(KeyCode keyCode)
    {
        return (TextAnchor)(keyCode - KeyCode.Alpha1);
    }
}
