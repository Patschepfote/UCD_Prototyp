using StarterAssets;
using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Collections.Generic;


[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public AudioClip tervetuloa;
    public AudioClip minaOlenEriika;
    public AudioClip hauskaTutustua;

    public Camera playerCamera;
    public float maxLookDistance = 5f;

    public GameObject interactionUI; 
    public GameObject dialogUI1;    
    public GameObject dialogUI2;  

    public FirstPersonController playerController; 

    private AudioSource audioSource;
    private bool hasSpoken = false;
    private int dialogStep = 0;

    public List<GameObject> speechEntries;
    public GameObject speechPanel;

    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!hasSpoken && Input.GetKeyDown(KeyCode.E) && IsPlayerLookingAtMe())
        {
            hasSpoken = true;
            interactionUI.SetActive(false);
            PlayAudio(tervetuloa, ShowDialog1);
            ShowSpeechEntry(0);
        }
    }

    bool IsPlayerLookingAtMe()
    {
        Ray ray = new Ray(playerCamera.transform.position + playerCamera.transform.forward * 0.1f, playerCamera.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, maxLookDistance))
        {
            return hit.transform == transform;
        }
        return false;
    }

    void PlayAudio(AudioClip clip, System.Action onComplete = null)
    {
        if (clip == null) return;

        audioSource.clip = clip;
        audioSource.Play();
        StartCoroutine(WaitForAudio(clip.length, onComplete));
    }

    System.Collections.IEnumerator WaitForAudio(float duration, System.Action callback)
    {
        yield return new WaitForSeconds(duration);
        callback?.Invoke();
    }

    void ShowDialog1()
    {
        LockPlayer(true);
        dialogUI1.SetActive(true);
        dialogStep = 1;

        keywords.Clear();
        keywords.Add("moi", OnDialogChoiceClicked);
        keywords.Add("hei", OnDialogChoiceClicked);
        keywords.Add("terve", OnDialogChoiceClicked);

        StartRecognition();
    }

    void ShowDialog2()
    {
        dialogUI1.SetActive(false);
        dialogUI2.SetActive(true);
        dialogStep = 2;


        keywords.Clear();
        keywords.Add("minä olen", OnDialogChoiceClicked);
        keywords.Add("mun nimi on", OnDialogChoiceClicked);
        keywords.Add("nimeni on", OnDialogChoiceClicked);

        StartRecognition();
    }

    void StartRecognition()
    {
        if (keywordRecognizer != null && keywordRecognizer.IsRunning)
            keywordRecognizer.Stop();

        if (keywords.Count > 0)
        {
            keywordRecognizer = new KeywordRecognizer(new List<string>(keywords.Keys).ToArray());
            keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
            keywordRecognizer.Start();
        }
    }

    void StopRecognition()
    {
        if (keywordRecognizer != null && keywordRecognizer.IsRunning)
        {
            keywordRecognizer.OnPhraseRecognized -= OnPhraseRecognized;
            keywordRecognizer.Stop();
            keywordRecognizer.Dispose();
            keywordRecognizer = null;
        }
    }
    void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        Debug.Log("Erkannt: " + args.text);
        if (keywords.ContainsKey(args.text.ToLower()))
        {
            StopRecognition(); 
            keywords[args.text.ToLower()].Invoke();
        }
    }

    void EndDialog()
    {
        dialogUI2.SetActive(false);
        LockPlayer(false);
        StopRecognition();
    }

    public void OnDialogChoiceClicked()
    {
        StopRecognition();

        if (dialogStep == 1)
        {
            PlayAudio(minaOlenEriika, ShowDialog2);
            ShowSpeechEntry(1);
        }
        else if (dialogStep == 2)
        {
            PlayAudio(hauskaTutustua, EndDialog);
            ShowSpeechEntry(2);
        }
    }

    void LockPlayer(bool isLocked)
    {
        Cursor.visible = isLocked;
        Cursor.lockState = isLocked ? CursorLockMode.None : CursorLockMode.Locked;

        if (playerController != null)
            playerController.enabled = !isLocked;
    }

    void ShowSpeechEntry(int index)
    {
        if (speechPanel != null && !speechPanel.activeSelf)
            speechPanel.SetActive(true);

        for (int i = 0; i < speechEntries.Count; i++)
        {
            if (speechEntries[i] != null)
                speechEntries[i].SetActive(i == index);
        }
    }
}