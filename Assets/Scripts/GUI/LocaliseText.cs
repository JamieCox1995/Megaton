using System.Xml;
using System.Xml.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class LocaliseText : MonoBehaviour
{
    [SerializeField]
    private TextAsset localisationFile;

    [SerializeField]
    private string textKey = " ";
    private string language = "en-GB";

    [SerializeField]
    private Text m_text;

    private void Start()
    {

    }

    private void OnGUI()
    {

    }

    private void UpdateText()
    {
        m_text = GetComponent<Text>();

        XmlDocument xml;

    }
}
