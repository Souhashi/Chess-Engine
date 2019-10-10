using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointToGrid : MonoBehaviour
{
    // Start is called before the first frame update
    
    public GameObject g;
    public GameObject m;
    Vector3 point;
    Vector3 move;
    GameObject selectiontile;
    GameObject movetile;
    GameObject s;
    GameObject enemypiece;
    Piece p;
    List<Vector3> positions;
    List<GameObject> movetiles;
    bool canCastle = true;
    Camera camera;
    void Start()
    {
        camera = GetComponent<Camera>();
        GenerateMoveTiles();
        
    }

    // Update is called once per frame
    void Update()
    {
        //GameManager.Instance.CheckCameras();
        Ray ray = new Ray();
        if (GameManager.Instance.current_player.ToString() == gameObject.name)
        {

            ray = camera.ScreenPointToRay(Input.mousePosition);
        }
            RaycastHit hit;
            Physics.Raycast(ray, out hit, 100);
        
        if (GameManager.Instance.currentBoardStatus == GameManager.Status.FreeMove)
        {
            

            if (movetile != null && movetile.activeSelf)
            {
                movetile.SetActive(false);
            }
            if (hit.collider != null)
            {
                point = hit.collider.transform.position;
                move = hit.collider.transform.position;

                if (selectiontile != null)
                {

                    selectiontile.transform.position = point;
                    //Debug.Log(point);

                }
                else
                {
                    selectiontile = Instantiate(g, point, Quaternion.identity);
                }

            }
            if (Input.GetMouseButtonDown(0))
            {
                s = GameManager.Instance.GetPieceAtLocation(point);
                if (s != null)
                {
                    p = s.GetComponent<Piece>();
                    if (p != null)
                    {
                        if (p.pColor.ToString() == GameManager.Instance.current_player.ToString())
                        {
                            //Debug.Log(p.piecetype);
                            GameManager.Instance.CanPawnEP();
                            p.SetStatusMaterial(Piece.Status.Selected);
                            GameManager.Instance.SetCurrentPieceGlobally(s.name);
                            selectiontile.transform.position = point;
                            positions = p.GetPiecePositions(point);
                            //Debug.Log(positions.Count);
                            Debug.Log(p.num_of_turns);
                            foreach (Vector3 p in positions)
                            {
                              
                                    MatchTileWithPos(p);
                                
                                                                
                               
                            }
                            GameManager.Instance.UpdateBoardStatus(2);
                        }
                    }
                }
               
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                GameManager.Instance.ResetBoard();
            }

        }
        if (GameManager.Instance.currentBoardStatus == GameManager.Status.PieceSelected)
        {
            
            /* if (movetile != null && !movetile.activeSelf)
             {
                 movetile.SetActive(true);
             }*/
            if (hit.collider != null)
            {
                point = hit.collider.transform.position;
                

               
}
            
            if (Input.GetMouseButtonDown(0))
            {
                //Debug.Log("hi");
                
                if (p != null)
                {
                    //Debug.Log("Hi"+point);
                    if (isPointinPos(point)) {
                        enemypiece = GameManager.Instance.GetPieceAtLocation(point);
                        GameManager.Instance.UpdateBoardStatus(3);
                        //Debug.Log("Move: " + point);
                        
                    }
                   


                }
            }
            if (Input.GetMouseButtonDown(1))
            {
                p.SetStatusMaterial(Piece.Status.Free);
                DeactivateTiles();
                GameManager.Instance.UpdateBoardStatus(1);

            }
        }
        if (GameManager.Instance.currentBoardStatus == GameManager.Status.PieceMoving)
        {
            
            if (p != null)
            {
                if (enemypiece != null)
                {
                    GameManager.Instance.RemovePieceGlobally(enemypiece.name);
                    GameManager.Instance.CapturePieceGlobally(enemypiece.name);

                }
                
                positions = p.GetPiecePositions(point);
                Debug.Log(positions.Count);
                
                DeactivateTiles();
                if (p.piecetype == Piece.Pieces.Rook&& Input.GetKey(KeyCode.Q) && GameManager.Instance.CanCastle(s, point))
                {
                    GameManager.Instance.Castle(p, point);
                    p.hasMovedBefore = true; 
                }
                if (p.piecetype == Piece.Pieces.Peon)
                {
                    GameManager.Instance.isAboutToEP(p, point);
                    GameManager.Instance.UpdatePieceGlobally(point, s.name);
                    //p.SetPiece(point);
                    p.hasMovedBefore = true;
                }
                else
                {
                    GameManager.Instance.UpdatePieceGlobally(point, s.name);
                    
                    p.hasMovedBefore = true;
                }
                GameManager.Instance.ResetEP();
                Debug.Log(p.num_of_turns);
                GameManager.Instance.UpdateNOTGlobal(s.name);

                //GameManager.Instance.ResetEP();

                List<GameObject> t = GameManager.Instance.IsKingInCheck();
                    if (t.Count != 0 && GameManager.Instance.isScanComplete)
                    {
                      
                                GameManager.Instance.SetCheckGlobal(true);
                                Debug.Log("King in check " + "Player:" + GameManager.Instance.current_player);
                                GameManager.Instance.ScanForCheckMate(GameManager.Instance.current_player, t);
                                                  
                    }

                GameManager.Instance.SetCheckGlobal(false);
                GameManager.Instance.isScanComplete = false;
                p.SetStatusMaterial(Piece.Status.Free);
                if (GameManager.Instance.canBePromoted(p))
                {
                    Debug.Log(p.piecetype + " can be promoted!");
                    GameManager.Instance.ActivatePromotionMenu();
                    Time.timeScale = 0f;
                }
                Debug.Log(GameManager.Instance.current_player);
                Time.timeScale = 1f;
                if (!GameManager.Instance.IsPaused)
                {
                    GameManager.Instance.UpdateBoardStatus(1);
                    GameManager.Instance.NextPlayer();
                }
            }

        }
        if (GameManager.Instance.currentBoardStatus == GameManager.Status.CheckMate)
        {

        }
        

    }

    bool isPointinPos(Vector3 pos)
    {
        foreach (Vector3 p in positions)
        {
            if (pos.x == p.x && pos.z == p.z)
            {
                return true;
            }
        }
        return false;
    }

    void GenerateMoveTiles()
    {
        movetiles = new List<GameObject>();
        for (int i = (int)GameManager.Instance.transform.position.x; i < (int)GameManager.Instance.transform.position.x + 8; i++)
        {
            for (int j = (int)GameManager.Instance.transform.position.z; j < (int)GameManager.Instance.transform.position.z + 8; j++)
            {
                GameObject d = Instantiate(m, new Vector3(i, GameManager.Instance.transform.position.y, j), Quaternion.identity);
                d.SetActive(false);
                movetiles.Add(d);
            }
        }
    }

    void MatchTileWithPos(Vector3 pos) 
    {
        foreach (GameObject d in movetiles)
        {
            if (pos.x == d.transform.position.x && pos.z == d.transform.position.z)
            {
                d.SetActive(true);
               
            }
        }
    }

    void DeactivateTiles()
    {
        foreach (GameObject d in movetiles)
        {
            if (d.activeSelf)
            {
                SwitchMaterial sm = d.GetComponent<SwitchMaterial>();
                sm.SetNormalState();
                d.SetActive(false);
            }
        }
    }

}
