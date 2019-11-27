using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PieceMovement : MonoBehaviour
{
    private static PieceMovement _instance;
    public static PieceMovement Instance { get { return _instance; } }
    int currentScreenheight, currentScreenwidth;
    public RectTransform board, pieces, cappieces, status;
    public float viewportwidth, viewportheight;
    public List<Sprite> white_sprites, black_sprites;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        white_sprites = new List<Sprite>();
        black_sprites = new List<Sprite>();
        Debug.Log(Screen.width + " " + Screen.height);
        currentScreenheight = Screen.height;
        currentScreenwidth = Screen.width;
        SetPanels();
        Invoke("SetPieces", 1);
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    void SetPanels()
    {
        float vieporthratio = currentScreenwidth * viewportwidth;
        float viewportvratio = currentScreenheight * viewportheight;
        float panel_target_width = currentScreenwidth - vieporthratio;
        float panel_target_height = currentScreenheight - panel_target_width;
        float cell_target_width = (panel_target_width - 10) * (float)0.125;

        board.offsetMin = new Vector2(vieporthratio, panel_target_height);
        pieces.offsetMin = new Vector2(vieporthratio, panel_target_height);
        cappieces.offsetMin = new Vector2(vieporthratio, 0);
        cappieces.offsetMax = new Vector2(0, -panel_target_width);
        status.offsetMax = new Vector2(-(currentScreenwidth - vieporthratio), - viewportvratio);

        
        transform.GetComponent<GridLayoutGroup>().cellSize = new Vector2(Mathf.Floor(cell_target_width), Mathf.Floor(cell_target_width));
        cappieces.GetComponent<GridLayoutGroup>().cellSize = new Vector2(Mathf.Floor(cell_target_width), Mathf.Floor(cell_target_width));

        for (int i = 0; i < pieces.childCount; i++)
        {
            pieces.transform.GetChild(i).GetComponent<RectTransform>().sizeDelta = new Vector2(Mathf.Floor(cell_target_width), Mathf.Floor(cell_target_width));
        }
    }

    void SetPieces()
    {
        int count = transform.childCount;
        for (int i = 0; i < 16; i++)
        {
            Transform child = transform.GetChild(i);
            Debug.Log(child.name);
            RectTransform rectT = child.GetComponent<RectTransform>();

            RectTransform imgt = pieces.transform.GetChild(i).GetComponent<RectTransform>();

            imgt.anchoredPosition = rectT.anchoredPosition;
        }
        int counter = 16;
        for (int i = 48; i < 64; i++)
        {
            Transform child = transform.GetChild(i);
            Debug.Log(child.name);
            RectTransform rectT = child.GetComponent<RectTransform>();

            RectTransform imgt = pieces.transform.GetChild(counter).GetComponent<RectTransform>();

            imgt.anchoredPosition = rectT.anchoredPosition;
            counter++;
        }
    }

    public void MoveImage(string name, string position)
    {
        RectTransform posRectT = GetChild(board, position);
        RectTransform imgRectT =GetChild(pieces, name);

        imgRectT.anchoredPosition = posRectT.anchoredPosition;
    }

    public void DeleteImage(string position)
    {
        RectTransform posRectT = GetChild(board, position);
        
        int count = pieces.transform.childCount;
        for (int i = 0; i < count; i++)
        {
            RectTransform rt = pieces.transform.GetChild(i).GetComponent<RectTransform>();
            if (rt.anchoredPosition == posRectT.anchoredPosition)
            {
                rt.GetComponent<Image>().enabled = false;
            }
        }
    }

    public void DeleteImageWithName(string name)
    {
        RectTransform imgRectT = GetChild(pieces, name);

       
                imgRectT.GetComponent<Image>().enabled = false;
           
        

    }

    public void MoveHierarchies(string name)
    {
        RectTransform imgRectT = GetChild(pieces, name);
        imgRectT.parent = null;
        imgRectT.parent = cappieces.transform;
        imgRectT.GetComponent<Image>().enabled = true;
    }

    public void SetImageSprite(string name, int spriteindex, string player)
    {
        RectTransform image = GetChild(pieces, name);
        if (player == "White")
        {
            image.GetComponent<Image>().sprite = white_sprites[spriteindex-1];
        }
        else
        {
            image.GetComponent<Image>().sprite = black_sprites[spriteindex-1];
        }
        
    }

    RectTransform GetChild(Transform t, string name)
    {
        int count= t.transform.childCount;
        for (int i = 0; i < count; i++)
        {
            Transform child = t.transform.GetChild(i);
            if (child.name == name)
            {
                RectTransform rectT = child.GetComponent<RectTransform>();
                return rectT;
            }
        }
        return null;
    }
}
