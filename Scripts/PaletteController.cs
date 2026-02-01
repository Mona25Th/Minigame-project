using UnityEngine;
using UnityEngine.UI;

public class PaletteController : MonoBehaviour
{
    public static PaletteController Instance;
    public int selectedColor;
    [SerializeField] private Transform PaletteOutline;

    void Awake()
    {
        Instance = this;
    }

    public void OnColorSelected(int colorIndex)
    {
        selectedColor = colorIndex;
        PaletteOutline.position = transform.GetChild(selectedColor).position;
        Debug.Log("color index: " + selectedColor);
    }
}
