using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GeeseManager : MonoBehaviour
{
    [HideInInspector] public EventHandler<BombDiffusionEventArgs> KilledAGoose;
    public List<Goose> _geese = new List<Goose>();

    [SerializeField] private GameManager _gameManager;

    [Header("Text")]
    [SerializeField] private TMP_Text _text;
    private string _lines;
    [SerializeField] private float _textSpeed;
    private Coroutine _typingCoroutine;

    private void Start()
    {
        _gameManager = FindAnyObjectByType<GameManager>();
    }

    public float GeeseLeft()
    {
        return _geese.Count;
    }

    public void ResetGeeseList()
    {
        _geese.Clear();
        _geese = FindObjectsOfType<Goose>().ToList();
    }

    public void HitAGoose(Goose goose)
    {
        KilledAGoose?.Invoke(this, new BombDiffusionEventArgs(goose.transform));

        _geese.Remove(goose);

        var bombsLeft = _geese.Count;

        if (bombsLeft > 1)
            _text.text = bombsLeft.ToString() + " geese left";
        else if (bombsLeft == 1)
            _text.text = bombsLeft.ToString() + " geese left";
        else
            _text.text = "";

        LoadText();
    }

    public void LoadText()
    {
        _lines = _text.text;

        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine); // Stop the previous typing coroutine
        }

        _typingCoroutine = StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        _text.text = "";

        var charArray = _lines.ToCharArray();

        foreach (var c in charArray)
        {
            _text.text += c;
            yield return new WaitForSecondsRealtime(_textSpeed);
        }

        // After typing completes, start the untyping coroutine
        StartCoroutine(UntypeLine());
    }

    IEnumerator UntypeLine()
    {
        yield return new WaitForSecondsRealtime(1.5f);

        var charArray = _text.text.ToCharArray();

        for (int i = charArray.Length - 1; i >= 0; i--)
        {
            _text.text = _text.text.Substring(0, i);
            yield return new WaitForSecondsRealtime(_textSpeed);
        }

        // Reset coroutine reference after untyping completes
        _typingCoroutine = null;
    }
}
