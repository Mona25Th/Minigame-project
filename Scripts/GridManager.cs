using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Data.SqlTypes;



public class GridManager : Minigame
{
    public int width = 10;
    public int height = 10;

    public GameObject tilePrefab;
    public GameObject flowerPrefab;
    public static GridManager Instance;




    // Drag your 4 ScriptableObjects here
    public TileType[] tileTypes;
    private Tile[,] tiles;
    //public Tile selectedTile;
    public GameObject spriteInTurn;
    public GameObject tierraFull;
    public GameObject tierraSemillasFull;
    public GameObject tierraAguaFull;
    public GameObject tierraSnowFull;
    public SpriteRenderer gameOverSprite;
    public Button tryAgainButton;
    public int intentos = 12;
    private bool isGameOver;
    public TextMeshProUGUI intentosText;
    public PaletteController palette;

    private bool step1Completed;
    private bool step2Completed;
    private bool step3Completed;
    private bool step2CoroutineStarted = false;
    private bool step3CoroutineStarted = false;
    private bool step4CoroutineStarted = false;

    [SerializeField] private float SpreadWait = 1f;



    protected override void Awake()
    {
        base.Awake();
        Instance = this;
    }

    void Start()
    {
        tiles = new Tile[width, height];
        GenerateGrid();
        spriteInTurn.GetComponent<Image>().sprite = tileTypes[0].sprite;
        UpdateIntentosUI();

    }

    void GenerateGrid()
    {



        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float posX = x - (width - 1) / 2f;
                float posY = y - (height - 1) / 2f;

                Vector3 position = new Vector3(posX, posY, 0);

                GameObject tileGO = Instantiate(tilePrefab, position, Quaternion.identity);
                tileGO.transform.parent = this.transform;

                Tile tile = tileGO.GetComponent<Tile>();
                tiles[x, y] = tile;

                TileType chosen = tileTypes[Random.Range(0, tileTypes.Length)];
                tile.Init(chosen);
            }
        }


    }

    private void UpdateIntentosUI()
    {
        intentosText.text = intentos.ToString();
        Debug.Log("intentos:" + intentosText.text);
    }

    public IEnumerator ApplyColor(Tile selectedTile, int colorIndex)
    {
        Debug.Log("Tile selected: " + selectedTile.type.tileName);
        TileType newType = tileTypes[colorIndex];
        /*if (selectedTile == null)
        {
            Debug.Log("No tile selected yet!");
            return;
        }*/

        TileType original = selectedTile.type;
        Vector2Int pos = FindTilePosition(selectedTile);

        if (original == newType) yield break;

        yield return FloodFill(pos.x, pos.y, original, newType);
        intentos--;
        UpdateIntentosUI();
        if (intentos <= 0)
        {
            if (!ganadoPartida())
            {
                gameOver();
            }
        }
        Debug.Log("new Type = " + newType);

        if (!isGameOver)
        {
            checkStep1Completed();
            StartCoroutine(ShowStep2AfterDelay());
            checkStep2Completed();
            StartCoroutine(ShowStep3AfterDelay());
            checkStep3Completed();
            StartCoroutine(ShowStep4AfterDelay());
        }

    }

    void gameOver()
    {

        gameOverSprite.gameObject.SetActive(true);
        tryAgainButton.gameObject.SetActive(true);
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        tierraFull.SetActive(false);
        isGameOver = true;
        Debug.Log("Game Over!");

    }

    public void refresh()
    {

        isGameOver = false;
        this.gameObject.SetActive(true);
        tiles = new Tile[width, height];
        GenerateGrid();
        intentos = 12;
        UpdateIntentosUI();
        gameOverSprite.gameObject.SetActive(false);
        tryAgainButton.gameObject.SetActive(false);
        // ganadoPartida();

        //AudioManager.PlayMusic("Flores_Live");

    }
    /*
    public void SelectTile(Tile tile)
    {
        selectedTile = tile;
        Debug.Log("Tile selected: " + tile.type.tileName);
    }
    */
    bool ganadoPartida()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (tiles[x, y].type.sprite != spriteInTurn.GetComponent<Image>().sprite)
                {
                    return false; // Exit early: not complete
                }
            }
        }
        return true;

    }

    public Vector2Int FindTilePosition(Tile tile)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (tiles[x, y] == tile)
                    return new Vector2Int(x, y);
            }
        }

        return new Vector2Int(-1, -1); // Not found
    }

    // Recursive, depth-first search
    /*
    void FloodFill(int x, int y, TileType target, TileType replacement, float wait = 0f, int i = 1)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return;

        Tile tile = tiles[x, y];
        if (tile.type != target) return;

        tile.Init(replacement, wait);
        //yield return new WaitForSeconds(SpreadWait/i);

        wait += SpreadWait / i; i++;
        FloodFill(x + 1, y, target, replacement, wait, i);
        FloodFill(x - 1, y, target, replacement, wait, i);
        FloodFill(x, y + 1, target, replacement, wait, i);
        FloodFill(x, y - 1, target, replacement, wait, i);

        if (i == 1) filled = true;
    }
    */

    // Iterative, breadth-first search
    IEnumerator FloodFill(int x, int y, TileType target, TileType replacement)
    {
        Queue<Vector2Int> TilesToCheck = new Queue<Vector2Int>();
        TilesToCheck.Enqueue(new Vector2Int(x, y));
        int i = 1; float wait = 0f;
        while (TilesToCheck.Count > 0)
        {
            int c = TilesToCheck.Count;
            for (int j = 0; j < c; j++)
            {
                var pos = TilesToCheck.Dequeue();
                Debug.Log(1 + ": " + pos);
                if (pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height) continue;
                
                Tile tile = tiles[pos.x, pos.y];
                Debug.Log(2 + ": " + tile.type);
                if (tile.type != target) continue;
                Debug.Log(3);

                tile.Init(replacement, wait);
                TilesToCheck.Enqueue(pos + Vector2Int.right);
                TilesToCheck.Enqueue(pos + Vector2Int.left);
                TilesToCheck.Enqueue(pos + Vector2Int.up);
                TilesToCheck.Enqueue(pos + Vector2Int.down);
            }
            wait += SpreadWait / i; i++;
        }
        yield return new WaitForSeconds(wait-SpreadWait/(i-1));
    }

    void checkStep1Completed()
    {
        // Check all tiles
        bool ganado = ganadoPartida();
        if (ganado)
        {

            // If we reached here, ALL tiles are tierra
            Debug.Log("Has completado la fase Tierra");

            step1Completed = true;
        }
    }

    void checkStep2Completed()
    {
        if (step1Completed)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // If any tile is NOT tierra (element 0), phase NOT complete
                    if (tiles[x, y].type != tileTypes[1])
                    {
                        return; // Exit early: not complete
                    }
                }
            }

            // If we reached here, ALL tiles are tierra
            Debug.Log("Has completado la fase semilla");

            step2Completed = true;
        }
    }


    void showStep2()
    {
        if (step1Completed && !step2CoroutineStarted)
        {
            // Update the UI sprite to "semillas" (element 1)
            spriteInTurn.GetComponent<Image>().sprite = tileTypes[1].sprite;

            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }



            refresh();

            tierraFull.SetActive(true);
            step2CoroutineStarted = true;

        }
    }

    IEnumerator ShowStep2AfterDelay()
    {

        yield return new WaitForSeconds(2f);
        showStep2();

    }

    void showStep3()
    {
        if (step2Completed && !step3CoroutineStarted)
        {
            // Update the UI sprite to "semillas" (element 1)
            spriteInTurn.GetComponent<Image>().sprite = tileTypes[2].sprite;

            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }


            refresh();

            tierraFull.SetActive(true);
            tierraFull.GetComponent<Animator>().enabled = false;
            tierraFull.GetComponent<SpriteRenderer>().sprite = tierraSemillasFull.GetComponent<SpriteRenderer>().sprite;
            step3CoroutineStarted = true;


        }
    }

    IEnumerator ShowStep3AfterDelay()
    {
        yield return new WaitForSeconds(2f);
        showStep3();

    }


    IEnumerator ShowStep4AfterDelay()
    {
        yield return new WaitForSeconds(2f);
        showStep4();

    }

    void checkStep3Completed()
    {
        if (step1Completed && step2Completed)
        {
            // Check all tiles
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // If any tile is NOT tierra (element 0), phase NOT complete
                    if (tiles[x, y].type != tileTypes[2])
                    {
                        return; // Exit early: not complete
                    }
                }
            }

            // If we reached here, ALL tiles are tierra
            Debug.Log("Has completado la fase Agua");

            step3Completed = true;
        }
    }
    void showStep4()
    {

        if (step3Completed && !step4CoroutineStarted)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Tile tile = tiles[x, y];
                    tile.Init(tileTypes[3]);


                    if (tile.transform.Find("Flower") == null)
                    {
                        GameObject flower =
                            Instantiate(flowerPrefab, tile.transform.position, Quaternion.identity, tile.transform);
                        flower.name = "Flower"; // so Find() works
                    }
                }
            }

            tierraFull.SetActive(true);
            tierraFull.GetComponent<Animator>().enabled = false;
            tierraFull.GetComponent<SpriteRenderer>().sprite = tierraSnowFull.GetComponent<SpriteRenderer>().sprite;
            step4CoroutineStarted = true;

            StartCoroutine(DelayFinish());
        }
    }

    IEnumerator DelayFinish()
    {
        yield return new WaitForSeconds(1f);
        Finish();
    }
}
