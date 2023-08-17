using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class TutorialUILayoutUtils
{
    private static GameObject ButtonPrefab;
    private static string ControlInputFormat;
    private static string GameConceptFormat;
    private static Font Font;
    private static int FontSize;
    private static Color Color;

    public static void SetButtonPrefab(GameObject go)
    {
        if (go.GetComponent<Button>() == null) throw new MissingComponentException("The button prefab does not have a \"Button\" component attached.");
        if (go.transform.Find("Text").GetComponent<Text>() == null) throw new MissingComponentException("The button prefab does not have a \"Text\" component child attached.");

        ButtonPrefab = go;
    }

    public static void SetControlInputFormat(string format)
    {
        if (string.IsNullOrEmpty(format)) Debug.LogWarning("Control input format not supplied: input names will be unformatted.");

        ControlInputFormat = format;
    }

    public static void SetGameConceptFormat(string format)
    {
        if (string.IsNullOrEmpty(format)) Debug.LogWarning("Game concept format not supplied: game concepts will be unformatted.");

        GameConceptFormat = format;
    }

    public static void SetFont(Font font)
    {
        if (font == null) throw new System.ArgumentNullException("font");

        Font = font;
    }

    public static void SetFontSize(int fontSize)
    {
        if (fontSize <= 0) throw new System.ArgumentOutOfRangeException("fontSize");

        FontSize = fontSize;
    }

    public static void SetColor(Color color)
    {
        Color = color;
    }

    public static void Clear(this CanvasGroup tutorialUI)
    {
        CanvasGroup textPanel = tutorialUI.GetTextPanel();

        List<GameObject> toDelete = new List<GameObject>();

        foreach (Transform child in textPanel.transform)
        {
            toDelete.Add(child.gameObject);
        }

        foreach (GameObject go in toDelete)
        {
            Object.Destroy(go);
        }
    }

    public static void Hide(this CanvasGroup tutorialUI)
    {
        tutorialUI.alpha = 0;
        tutorialUI.interactable = false;
        tutorialUI.blocksRaycasts = false;
    }

    public static void Show(this CanvasGroup tutorialUI)
    {
        tutorialUI.alpha = 1;
        tutorialUI.interactable = true;
        tutorialUI.blocksRaycasts = true;
    }

    public static CanvasGroup SetTextPanelRect(this CanvasGroup tutorialUI, Rect rect)
    {
        CanvasGroup textPanel = tutorialUI.GetTextPanel();

        RectTransform rectTransform = textPanel.GetComponent<RectTransform>();

        rectTransform.anchoredPosition = rect.position;
        rectTransform.sizeDelta = rect.size;

        return tutorialUI;
    }

    public static CanvasGroup AddParagraph(this CanvasGroup tutorialUI, string text)
    {
        CanvasGroup textPanel = tutorialUI.GetTextPanel();

        textPanel.EnsureLayoutComponentsAreAttached();

        GameObject child = new GameObject("Text");

        child.transform.SetParent(textPanel.transform);

        Text textComponent = child.AddComponent<Text>();

        //textComponent.text = text;
        FormatInputText formatInputText = child.AddComponent<FormatInputText>();
        formatInputText.TextToFormat = text;
        formatInputText.Format = ControlInputFormat; //"<color=cyan>{0}</color>";

        textComponent.font = Font; //Resources.Load<Font>("Fonts/Smart");
        textComponent.fontSize = FontSize;
        textComponent.color = Color; //new Color(0.88f, 0.89f, 0.6f, 1f);

        textComponent.rectTransform.localScale = Vector3.one;

        return tutorialUI;
    }

    public static CanvasGroup AddParagraph(this CanvasGroup tutorialUI, string format, params object[] args)
    {
        return tutorialUI.AddParagraph(string.Format(format, args));
    }

    public static CanvasGroup AddRichTextParagraph(this CanvasGroup tutorialUI, string text)
    {
        CanvasGroup textPanel = tutorialUI.GetTextPanel();

        textPanel.EnsureLayoutComponentsAreAttached();

        GameObject child = new GameObject("Text");

        child.transform.SetParent(textPanel.transform);

        Text textComponent = child.AddComponent<Text>();

        textComponent.supportRichText = true;

        //textComponent.text = text;
        FormatInputText formatInputText = child.AddComponent<FormatInputText>();
        formatInputText.TextToFormat = text;
        formatInputText.Format = ControlInputFormat; //"<color=cyan>{0}</color>";

        textComponent.font = Font; //Resources.Load<Font>("Fonts/Smart");
        textComponent.fontSize = FontSize;
        textComponent.color = Color; //new Color(0.88f, 0.89f, 0.6f, 1f);

        textComponent.rectTransform.localScale = Vector3.one;

        return tutorialUI;
    }

    public static CanvasGroup AddRichTextParagraph(this CanvasGroup tutorialUI, string format, params object[] args)
    {
        return tutorialUI.AddRichTextParagraph(string.Format(format, args));
    }

    public static CanvasGroup HighlightRect(this CanvasGroup tutorialUI, Rect rect)
    {
        return tutorialUI.HighlightRect(rect, Color.white);
    }

    public static CanvasGroup HighlightRect(this CanvasGroup tutorialUI, Rect rect, Color color)
    {
        Image highlight = tutorialUI.GetHighlight();

        highlight.enabled = true;

        highlight.rectTransform.anchoredPosition = rect.position;
        highlight.rectTransform.sizeDelta = rect.size;

        highlight.color = color;

        return tutorialUI;
    }

    public static CanvasGroup SetHighlightAnchor(this CanvasGroup tutorialUI, AnchorPresets anchor)
    {
        Image highlight = tutorialUI.GetHighlight();

        highlight.rectTransform.SetAnchor(anchor);

        return tutorialUI;
    }

    public static CanvasGroup HideHighlight(this CanvasGroup tutorialUI)
    {
        Image highlight = tutorialUI.GetHighlight();

        //highlight.enabled = false;

        highlight.color = Color.clear;

        return tutorialUI;
    }

    public static IEnumerator AddButton(this CanvasGroup tutorialUI, string buttonText)
    {
        CanvasGroup textPanel = tutorialUI.GetTextPanel();

        textPanel.EnsureLayoutComponentsAreAttached();

        GameObject buttonObj = GameObject.Instantiate(ButtonPrefab);

        buttonObj.transform.SetParent(textPanel.transform);

        Button buttonComponent = buttonObj.GetComponent<Button>();
        buttonComponent.GetComponent<RectTransform>().localScale = Vector3.one;
        Text textComponent = buttonObj.transform.Find("Text").GetComponent<Text>();

        textComponent.text = buttonText;
        textComponent.font = Font; //Resources.Load<Font>("Fonts/Smart");
        textComponent.rectTransform.localScale = Vector3.one;

        bool shouldBreak = false;

        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(buttonObj);
        buttonComponent.onClick.AddListener(() => { shouldBreak = true; });

        while (!shouldBreak)
        {
            yield return null;
        }
    }

    public static Coroutine<int> AddButtons(this CanvasGroup tutorialUI, params string[] buttonTexts)
    {
        Coroutine<int> coroutine = new Coroutine<int>(tutorialUI.AddButtonsInternal(buttonTexts));

        return coroutine;
    }

    public static void SetAnchor(this RectTransform rectTransform, AnchorPresets alignment, float offsetX = 0f, float offsetY = 0f)
    {
        rectTransform.anchoredPosition = new Vector2(offsetX, offsetY);

        switch (alignment)
        {
            case (AnchorPresets.TopLeft):
            {
                rectTransform.anchorMin = new Vector2(0, 1);
                rectTransform.anchorMax = new Vector2(0, 1);
                break;
            }
            case (AnchorPresets.TopCenter):
            {
                rectTransform.anchorMin = new Vector2(0.5f, 1);
                rectTransform.anchorMax = new Vector2(0.5f, 1);
                break;
            }
            case (AnchorPresets.TopRight):
            {
                rectTransform.anchorMin = new Vector2(1, 1);
                rectTransform.anchorMax = new Vector2(1, 1);
                break;
            }

            case (AnchorPresets.MiddleLeft):
            {
                rectTransform.anchorMin = new Vector2(0, 0.5f);
                rectTransform.anchorMax = new Vector2(0, 0.5f);
                break;
            }
            case (AnchorPresets.MiddleCenter):
            {
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                break;
            }
            case (AnchorPresets.MiddleRight):
            {
                rectTransform.anchorMin = new Vector2(1, 0.5f);
                rectTransform.anchorMax = new Vector2(1, 0.5f);
                break;
            }

            case (AnchorPresets.BottomLeft):
            {
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(0, 0);
                break;
            }
            case (AnchorPresets.BottomCenter):
            {
                rectTransform.anchorMin = new Vector2(0.5f, 0);
                rectTransform.anchorMax = new Vector2(0.5f, 0);
                break;
            }
            case (AnchorPresets.BottomRight):
            {
                rectTransform.anchorMin = new Vector2(1, 0);
                rectTransform.anchorMax = new Vector2(1, 0);
                break;
            }

            case (AnchorPresets.HorizontalStretchTop):
            {
                rectTransform.anchorMin = new Vector2(0, 1);
                rectTransform.anchorMax = new Vector2(1, 1);
                break;
            }
            case (AnchorPresets.HorizontalStretchMiddle):
            {
                rectTransform.anchorMin = new Vector2(0, 0.5f);
                rectTransform.anchorMax = new Vector2(1, 0.5f);
                break;
            }
            case (AnchorPresets.HorizontalStretchBottom):
            {
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(1, 0);
                break;
            }

            case (AnchorPresets.VerticalStretchLeft):
            {
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(0, 1);
                break;
            }
            case (AnchorPresets.VerticalStretchCenter):
            {
                rectTransform.anchorMin = new Vector2(0.5f, 0);
                rectTransform.anchorMax = new Vector2(0.5f, 1);
                break;
            }
            case (AnchorPresets.VerticalStretchRight):
            {
                rectTransform.anchorMin = new Vector2(1, 0);
                rectTransform.anchorMax = new Vector2(1, 1);
                break;
            }

            case (AnchorPresets.StretchAll):
            {
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(1, 1);
                break;
            }
        }
    }

    public static void SetPivot(this RectTransform rectTransform, PivotPresets pivot)
    {
        switch (pivot)
        {
            case (PivotPresets.TopLeft):
            {
                rectTransform.pivot = new Vector2(0, 1);
                break;
            }
            case (PivotPresets.TopCenter):
            {
                rectTransform.pivot = new Vector2(0.5f, 1);
                break;
            }
            case (PivotPresets.TopRight):
            {
                rectTransform.pivot = new Vector2(1, 1);
                break;
            }

            case (PivotPresets.MiddleLeft):
            {
                rectTransform.pivot = new Vector2(0, 0.5f);
                break;
            }
            case (PivotPresets.MiddleCenter):
            {
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                break;
            }
            case (PivotPresets.MiddleRight):
            {
                rectTransform.pivot = new Vector2(1, 0.5f);
                break;
            }

            case (PivotPresets.BottomLeft):
            {
                rectTransform.pivot = new Vector2(0, 0);
                break;
            }
            case (PivotPresets.BottomCenter):
            {
                rectTransform.pivot = new Vector2(0.5f, 0);
                break;
            }
            case (PivotPresets.BottomRight):
            {
                rectTransform.pivot = new Vector2(1, 0);
                break;
            }
        }
    }

    private static IEnumerator AddButtonsInternal(this CanvasGroup tutorialUI, params string[] buttonTexts)
    {
        CanvasGroup textPanel = tutorialUI.GetTextPanel();

        textPanel.EnsureLayoutComponentsAreAttached();

        int buttonValue = -1;

        for (int i = 0; i < buttonTexts.Length; i++)
        {
            GameObject buttonObj = GameObject.Instantiate(ButtonPrefab);

            buttonObj.transform.SetParent(textPanel.transform);

            Button buttonComponent = buttonObj.GetComponent<Button>();
            buttonComponent.GetComponent<RectTransform>().localScale = Vector3.one;
            Text textComponent = buttonObj.transform.Find("Text").GetComponent<Text>();

            textComponent.text = buttonTexts[i];
            textComponent.font = Font; //Resources.Load<Font>("Fonts/Smart");
            textComponent.rectTransform.localScale = Vector3.one;

            if (i == 0) UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(buttonObj);

            int buttonIndex = i;
            buttonComponent.onClick.AddListener(() => { buttonValue = buttonIndex; });
        }

        while (buttonValue == -1)
        {
            yield return null;
        }

        yield return buttonValue;
    }

    private static Image GetHighlight(this CanvasGroup tutorialUI)
    {
        Transform child = tutorialUI.transform.Find("Highlight");

        if (child != null)
        {
            Image imageComponent = child.GetComponent<Image>();

            if (imageComponent != null) return imageComponent;
        }

        throw new MissingComponentException("The CanvasGroup does not contain a child named \"Highlight\" with an Image component.");
    }

    private static CanvasGroup GetTextPanel(this CanvasGroup tutorialUI)
    {
        Transform child = tutorialUI.transform.Find("Text Panel");

        if (child != null)
        {
            CanvasGroup canvasGroup = child.GetComponent<CanvasGroup>();

            if (canvasGroup != null) return canvasGroup;
        }

        throw new MissingComponentException("The CanvasGroup does not contain a child named \"Text Panel\" with a CanvasGroup component.");
    }

    private static void EnsureLayoutComponentsAreAttached(this CanvasGroup tutorialUI)
    {
        VerticalLayoutGroup layoutGroup = tutorialUI.GetComponent<VerticalLayoutGroup>();

        if (layoutGroup == null)
        {
            layoutGroup = tutorialUI.gameObject.AddComponent<VerticalLayoutGroup>();

            layoutGroup.padding = new RectOffset(5, 5, 5, 10);
            layoutGroup.spacing = 10f;

            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = true;
        }

        ContentSizeFitter sizeFitter = tutorialUI.GetComponent<ContentSizeFitter>();

        if (sizeFitter == null)
        {
            sizeFitter = tutorialUI.gameObject.AddComponent<ContentSizeFitter>();

            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
    }
}

public enum AnchorPresets
{
    TopLeft,
    TopCenter,
    TopRight,

    MiddleLeft,
    MiddleCenter,
    MiddleRight,

    BottomLeft,
    BottomCenter,
    BottomRight,

    VerticalStretchLeft,
    VerticalStretchCenter,
    VerticalStretchRight,

    HorizontalStretchTop,
    HorizontalStretchMiddle,
    HorizontalStretchBottom,

    StretchAll,
}

public enum PivotPresets
{
    TopLeft,
    TopCenter,
    TopRight,

    MiddleLeft,
    MiddleCenter,
    MiddleRight,

    BottomLeft,
    BottomCenter,
    BottomRight,
}
