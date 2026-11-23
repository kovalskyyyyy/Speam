using System.Collections;
using TMPro;
using UnityEngine;

public class ThoughtPresenter : MonoBehaviour
{
    public static ThoughtPresenter Instance { get; private set; }

    [SerializeField] TMP_Text label;
    [SerializeField] float showTime = 3f;

    Coroutine routine;

    void Awake()
    {
        Instance = this;
        label.gameObject.SetActive(false);
    }

    public void Show(string text)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ShowRoutine(text));
    }

    IEnumerator ShowRoutine(string text)
    {
        label.text = text;
        label.gameObject.SetActive(true);
        yield return new WaitForSeconds(showTime);
        label.gameObject.SetActive(false);
        routine = null;
    }
}