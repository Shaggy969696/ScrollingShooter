using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    public float scrollSpeed = 0.5f;
    private Renderer rend;
    private static readonly int MainTex = Shader.PropertyToID("_BaseMap"); // URP default

    private void Start()
    {
        rend = GetComponent<Renderer>();
    }

    private void Update()
    {
        float offset = Time.time * scrollSpeed;
        rend.material.SetTextureOffset(MainTex, new Vector2(0, -offset));
    }
}
