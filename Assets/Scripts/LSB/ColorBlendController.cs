using UnityEngine;

public class ColorBlendController : MonoBehaviour
{
    [Header("Color Setting")]
    public Color targetColor = Color.red;

    [Range(0f, 1f)]
    public float blendStrength = 0.5f;

    private Renderer[] _renderers;
    private MaterialPropertyBlock _propBlock;

    void Start()
    {
        _propBlock = new MaterialPropertyBlock();
        _renderers = GetComponentsInChildren<Renderer>();
    }

    void Update()
    {
        if (_renderers == null) return;

        foreach (Renderer r in _renderers)
        {
            r.GetPropertyBlock(_propBlock);

            Color finalColor = Color.Lerp(Color.white, targetColor, blendStrength);

            _propBlock.SetColor("_BaseColor", finalColor);


            r.SetPropertyBlock(_propBlock);
        }
    }
}