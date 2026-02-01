using System.Collections;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public TileType type;
    private SpriteRenderer sr;
    public GridManager gridManager;
    public PaletteController palette;
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void Init(TileType newType, float wait = 0f)
    {
        type = newType;
        if (wait == 0f) sr.sprite = type.sprite;
        else StartCoroutine(DelayInit(wait));
    }
    private IEnumerator DelayInit(float wait)
    {
        yield return new WaitForSeconds(wait);
        sr.sprite = type.sprite;
    }

    void OnMouseDown()
    {

        StartCoroutine(GridManager.Instance.ApplyColor(this, PaletteController.Instance.selectedColor));
    }
}
