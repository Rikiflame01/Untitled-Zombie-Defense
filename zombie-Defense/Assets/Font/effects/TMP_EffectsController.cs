using UnityEngine;
using TMPro;
using System.Collections;

public class TMP_EffectsController : MonoBehaviour
{
    public TextMeshProUGUI tmpText; // Reference to TMP Text Component

    [Header("Effects Toggle")]
    public bool enableRainbow = false;
    public bool enableShake = false;
    public bool enableWave = false;
    public bool enableTypingEffect = false;

    [Header("Effect Parameters")]
    public float rainbowSpeed = 2f;
    public float shakeIntensity = 2f;
    public float shakeSpeed = 10f;
    public float waveSpeed = 2f;
    public float waveHeight = 5f;
    public float typingSpeed = 0.05f;

    private TMP_TextInfo textInfo;
    private float timeElapsed;

    void Start()
    {
        if (tmpText == null)
            tmpText = GetComponent<TextMeshProUGUI>();

        textInfo = tmpText.textInfo;

        if (enableTypingEffect)
            StartCoroutine(TypingEffect());
    }

    void Update()
    {
        if (tmpText == null || textInfo == null) return;

        tmpText.ForceMeshUpdate();
        timeElapsed += Time.deltaTime;

        Color32[] newVertexColors;
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            if (!charInfo.isVisible)
                continue;

            int vertexIndex = charInfo.vertexIndex;
            int materialIndex = charInfo.materialReferenceIndex;
            newVertexColors = textInfo.meshInfo[materialIndex].colors32;
            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            // Apply Rainbow Effect
            if (enableRainbow)
            {
                float hue = Mathf.Repeat((timeElapsed * rainbowSpeed + i * 0.1f), 1f);
                Color rainbowColor = Color.HSVToRGB(hue, 1, 1);
                Color32 color32 = rainbowColor;

                newVertexColors[vertexIndex + 0] = color32;
                newVertexColors[vertexIndex + 1] = color32;
                newVertexColors[vertexIndex + 2] = color32;
                newVertexColors[vertexIndex + 3] = color32;
            }

            // Apply Shake Effect
            if (enableShake)
            {
                Vector3 jitterOffset = new Vector3(
                    Mathf.Sin(timeElapsed * shakeSpeed + i) * shakeIntensity,
                    Mathf.Cos(timeElapsed * shakeSpeed + i) * shakeIntensity,
                    0);

                vertices[vertexIndex + 0] += jitterOffset;
                vertices[vertexIndex + 1] += jitterOffset;
                vertices[vertexIndex + 2] += jitterOffset;
                vertices[vertexIndex + 3] += jitterOffset;
            }

            // Apply Wave Effect
            if (enableWave)
            {
                float waveOffset = Mathf.Sin((timeElapsed + i * 0.1f) * waveSpeed) * waveHeight;
                Vector3 waveMotion = new Vector3(0, waveOffset, 0);

                vertices[vertexIndex + 0] += waveMotion;
                vertices[vertexIndex + 1] += waveMotion;
                vertices[vertexIndex + 2] += waveMotion;
                vertices[vertexIndex + 3] += waveMotion;
            }
        }

        // Apply the updated vertex colors and positions
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            textInfo.meshInfo[i].mesh.colors32 = textInfo.meshInfo[i].colors32;
            tmpText.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }

    IEnumerator TypingEffect()
    {
        tmpText.ForceMeshUpdate();
        string fullText = tmpText.text;
        tmpText.text = "";

        for (int i = 0; i < fullText.Length; i++)
        {
            tmpText.text += fullText[i];
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}
