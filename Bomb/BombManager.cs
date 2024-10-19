using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class BombManager : MonoBehaviour
{
    [SerializeField] private GameManager _gameManager;

    [Header("Text")]
    [SerializeField] private TMP_Text _text;
    private string _lines;
    [SerializeField] private float _textSpeed;

    [HideInInspector] public EventHandler<BombDiffusionEventArgs> DiffusedABomb;

    public List<BombDiffuser> _bombs = new List<BombDiffuser>();

    private Coroutine _typingCoroutine;

    private void Start()
    {
        _gameManager = FindAnyObjectByType<GameManager>();
        _bombs = FindObjectsOfType<BombDiffuser>().ToList();

        _text.text = _bombs.Count.ToString() + " bombs left";

        LoadText();
    }
    private void LateUpdate()
    {
        ResetBombList();
    }
    public void ResetBombList()
    {
        _bombs.Clear();
        _bombs = FindObjectsOfType<BombDiffuser>().ToList();
    }
    public int BombsLeft()
    {
        return _bombs.Count;
    }

    public void DiffusedPoint(BombDiffuser bomb)
    {
        DiffusedABomb?.Invoke(this, new BombDiffusionEventArgs(bomb.transform));

        _bombs.Remove(bomb);

        var bombsLeft = _bombs.Count;

        if (bombsLeft > 1)
            _text.text = bombsLeft.ToString() + " signals left";
        else if (bombsLeft == 1)
            _text.text = bombsLeft.ToString() + " signal left";
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
