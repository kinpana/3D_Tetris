using TMPro;
using UnityEngine;

public class HudController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TextMeshProUGUI scoreValue;
    [SerializeField] private TextMeshProUGUI levelValue;
    [SerializeField] private TextMeshProUGUI clearsValue;
    [SerializeField] private TextMeshProUGUI nextValue;
    
    public void SetNext(string id)
    {
        if (nextValue != null) nextValue.text = id;
    }


    public void SetScore(int score)
    {
        if (scoreValue != null) scoreValue.text = score.ToString();
    }

    public void SetLevel(int level)
    {
        if (levelValue != null) levelValue.text = level.ToString();
    }

    public void SetClears(int clears)
    {
        if (clearsValue != null) clearsValue.text = clears.ToString();
    }
}
