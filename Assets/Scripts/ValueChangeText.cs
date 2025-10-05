using UnityEngine;
using TMPro;

public class ValueChangeText : MonoBehaviour
{
    public AnimationCurve displacement;
    private float mag;
    private Vector2 dir;
    private Vector3 col;
    
    public TMP_Text deltaText;
    public TMP_Text deltaShadowText;

    private float timer = 0f;

    private bool doUpdate = false;
    public void Initialize(Vector3 position, Vector3 direction, float magnitude, Vector3 color, string text, int fontSize)
    {
        mag = magnitude;
        transform.position = position;
        dir = direction;
        col = color;
        deltaText.text = text;
        deltaShadowText.text = text;
        deltaText.fontSize = fontSize;
        deltaShadowText.fontSize = fontSize;

        doUpdate = true;
    }

    void Update()
    {
        if (!doUpdate) return;

        if (timer > (displacement.keys[displacement.keys.Length - 1].time))
        {
            Destroy(gameObject);
            return;
        }

        transform.position += new Vector3(dir.x, dir.y, 0f) * displacement.Evaluate(timer) * mag;

        timer += Time.deltaTime;
    }
}
