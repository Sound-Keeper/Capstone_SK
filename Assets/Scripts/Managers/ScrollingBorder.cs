using UnityEngine;

public class ScrollingBorder : MonoBehaviour
{
    public float scrollSpeedX = 0.1f;
    public float scrollSpeedY = 0.0f;

    private Material borderMaterial;

    void Start()
    {
        // Grab the material assigned to the renderer
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            borderMaterial = renderer.material;
        }
    }

    void Update()
    {
        if (borderMaterial != null)
        {
            // Shift the texture coordinates over time
            Vector2 currentOffset = borderMaterial.GetTextureOffset("_BaseMap");
            currentOffset.x += scrollSpeedX * Time.deltaTime;
            currentOffset.y += scrollSpeedY * Time.deltaTime;

            borderMaterial.SetTextureOffset("_BaseMap", currentOffset);
        }
    }
}