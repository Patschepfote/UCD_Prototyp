using UnityEngine;

public class LanguageToggle : MonoBehaviour
{
    private bool showingFinnish = true;

    public void ToggleLanguage()
    {
        showingFinnish = !showingFinnish;
        UpdateLanguageVisibility();
    }

    void UpdateLanguageVisibility()
    {
        foreach (Transform parent in transform)
        {
            Transform finnish = parent.Find("finnisch");
            Transform german = parent.Find("deutsch");

            if (finnish != null) finnish.gameObject.SetActive(showingFinnish);
            if (german != null) german.gameObject.SetActive(!showingFinnish);
        }
    }

    void Start()
    {
        UpdateLanguageVisibility();
    }
}
