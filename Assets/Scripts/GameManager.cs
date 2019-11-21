﻿using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }
    // Start is called before the first frame update
    public enum Status { FreeMove, PieceSelected, PieceMoving, CheckMate };
    public enum Player { Black, White };
    public Player current_player;
    public Status currentBoardStatus;
    public GameObject currentPiece;
    public Grid grid;
    GameObject[] allpieces;
    public List<GameObject> Allpieces;
    public List<GameObject> black_pieces;
    public List<GameObject> white_pieces;
    public List<GameObject> bcaptured_pieces;
    public List<GameObject> wcaptured_pieces;
    List<Vector3> positions_black;
    List<Vector3> positions_white;
    public Vector3 g;
    public bool isBlackPlayerinCheck;
    public bool isWhitePlayerinCheck;
    public bool isScanComplete;
    public GameObject PromotionGUI;
    public bool IsPaused;
    public Camera wp_camera, bp_camera;
    public GameObject wp, bp, player;
    public TextMeshProUGUI gui;
    int counter = 0;
    string objectname;
    


    void Start()
    {
        InstantiatePieces();
        GeneratePieces();
        InitialiseCapturePositions();
        currentBoardStatus = Status.FreeMove;
        current_player = Player.White;

        g = transform.position;
        isScanComplete = false;
        IsPaused = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void InstantiatePieces()
    {
        if (PhotonNetwork.IsMasterClient)
        {

            GameObject p = Instantiate(player, wp.transform.position, wp.transform.rotation);
            p.name = "White";
        }
        else
        {
            GameObject p = Instantiate(player, bp.transform.position, bp.transform.rotation);
            p.name = "Black";
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else {
            _instance = this;
        }
    }

    public void UpdatePieceGlobally(Vector3 np, string name)
    {
        PhotonView photonView = PhotonView.Get(this);
        Debug.Log("PRC RUN");
        photonView.RPC("SetPiece", RpcTarget.All, np, name);
    }

    [PunRPC]
    void SetPiece(Vector3 newpos, string name)
    {
        GameObject piece = GameObject.Find(name);
        Vector3 newp = new Vector3(newpos.x, piece.transform.position.y, newpos.z);
        piece.transform.position = newp;
    }

    [PunRPC]
    void UpdateCurrentPlayer(string currentplayer)
    {
        if (currentplayer == "Black")
        {
            current_player = Player.Black;
            gui.text = "Black is Playing";
        }
        if (currentplayer == "White")
        {
            current_player = Player.White;
            gui.text = "White is Playing";

        }
    }

    [PunRPC]
    void RemovePieceFromList(string name)
    {
        GameObject piece = GameObject.Find(name);
        Allpieces.Remove(piece);
    }
    [PunRPC]
    void SetPauseState(bool state)
    {
        IsPaused = state;
    }

    [PunRPC]
    void UpdateEnPassantStatus(string name, bool status, int state)
    {
        GameObject peon = GameObject.Find(name);
        Piece _peonpiece = peon.GetComponent<Piece>();
        switch (state)
        {
            case 1:
                _peonpiece.EnPassantLeft = status;
                break;
            case 2:
                _peonpiece.EnPassantRight = status;
                break;
            case 3:
                _peonpiece.CanEnPassantLeft = status;
                break;
            case 4:
                _peonpiece.CanEnPassantRight = status;
                break;
            case 5:
                _peonpiece.CanEnPassantRight = status;
                _peonpiece.CanEnPassantLeft = status;
                break;
        }

    }

    public void UpdateEnPassantStatusGlobal(string name, bool status, int state)
    {
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("UpdateEnPassantStatus", RpcTarget.All, name, status, state);
    }

    void SetPauseStateGlobal(bool state)
    {
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("SetPauseState", RpcTarget.All, state);
    }

    
    void UpdatePlayerGlobally(string cp)
    {
        PhotonView photonView = PhotonView.Get(this);
        Debug.Log("PLAYER PRC RUN");
        photonView.RPC("UpdateCurrentPlayer", RpcTarget.All, cp);
    }

    public void RemovePieceGlobally(string piece)
    {
        PhotonView photonView = PhotonView.Get(this);
        
        photonView.RPC("RemovePieceFromList", RpcTarget.All, piece);
    }

    [PunRPC]
    void SetGuiText(string text)
    {
        gui.text = text;
    }

    public void SetGUITextGlobal(string text)
    {
        PhotonView photonView = PhotonView.Get(this);

        photonView.RPC("SetGuiText", RpcTarget.All, text);
    }
        
    public void ResetBoard()
    {
        foreach (GameObject g in Allpieces)
        {
            Piece piece = g.GetComponent<Piece>();
            piece.SetPiece(piece.startposition);

        }
        foreach (GameObject g in bcaptured_pieces)
        {
            Piece piece = g.GetComponent<Piece>();
            piece.SetPiece(piece.startposition);
            Allpieces.Add(g);
        }
        bcaptured_pieces.Clear();
        foreach (GameObject g in wcaptured_pieces)
        {
            Piece piece = g.GetComponent<Piece>();
            piece.SetPiece(piece.startposition);
            Allpieces.Add(g);
        }
        wcaptured_pieces.Clear();
    }

    public void Capture(GameObject piece)
    {
        switch (current_player)
        {
            case Player.Black:

                wcaptured_pieces.Add(piece);
                piece.transform.position = new Vector3(positions_white[wcaptured_pieces.Count - 1].x, piece.transform.position.y, positions_white[wcaptured_pieces.Count - 1].z);

                break;
            case Player.White:
                
                
                bcaptured_pieces.Add(piece);
                piece.transform.position = new Vector3(positions_black[bcaptured_pieces.Count - 1].x, piece.transform.position.y, positions_black[bcaptured_pieces.Count - 1].z);
                break;
        }
    }

    [PunRPC]
    public void CapturePiece(string name)
    {
        switch (current_player)
        {
            case Player.Black:
                GameObject piece = GameObject.Find(name);
                wcaptured_pieces.Add(piece);
                piece.transform.position = new Vector3(positions_white[wcaptured_pieces.Count - 1].x, piece.transform.position.y, positions_white[wcaptured_pieces.Count - 1].z);
                break;
            case Player.White:
                GameObject wpiece = GameObject.Find(name);
                bcaptured_pieces.Add(wpiece);
                wpiece.transform.position = new Vector3(positions_black[bcaptured_pieces.Count - 1].x, wpiece.transform.position.y, positions_black[bcaptured_pieces.Count - 1].z);
                break;
        }
    }

    public void CapturePieceGlobally(string name)
    {
        PhotonView photonView = PhotonView.Get(this);

        photonView.RPC("CapturePiece", RpcTarget.All, name);
    }
    [PunRPC]
    void UpdateNumOfTurns(string name)
    {
        GameObject piece = GameObject.Find(name);
        Piece p = piece.GetComponent<Piece>();
        p.num_of_moves++;
    }

    public void UpdateNOTGlobal(string name)
    {
        PhotonView photonView = PhotonView.Get(this);

        photonView.RPC("UpdateNumOfTurns", RpcTarget.All, name);
    }

    public void InitialiseCapturePositions()
    {
        positions_black = new List<Vector3>();
        positions_white = new List<Vector3>();
        for (int i = (int)transform.position.x - 1; i >= (int)transform.position.x - 2; i--)
        {
            for (int j = (int)transform.position.z; j <= (int)transform.position.z + 7; j++)
            {
                Vector3 bpos = new Vector3(i, 0, j);
                positions_black.Add(bpos);
            }
        }

        for (int i = (int)transform.position.x + 8; i <= (int)transform.position.x + 9; i++)
        {
            for (int j = (int)transform.position.z; j <= (int)transform.position.z + 7; j++)
            {
                Vector3 wpos = new Vector3(i, 0, j);
                positions_white.Add(wpos);
            }
        }
    }

    public void isAboutToEP(Piece p, Vector3 point)
    {
        switch (p.pColor)
        {
            case Piece.Color.Black:
                Vector3 upleft = new Vector3(p.transform.position.x - 1, p.transform.position.y, p.transform.position.z + 1);
                Vector3 upright = new Vector3(p.transform.position.x + 1 , p.transform.position.y, p.transform.position.z + 1);
                if (p.EnPassantLeft && upleft.x == point.x && upleft.z == point.z)
                {
                    Vector3 enemylocation = new Vector3(p.transform.position.x - 1, p.transform.position.y, p.transform.position.z);
                    GameObject enemy = GetPieceAtLocation(enemylocation);
                    RemovePieceGlobally(enemy.name);
                    CapturePieceGlobally(enemy.name);
                }
                if (p.EnPassantRight && upright.x == point.x && upright.z == point.z)
                {
                    Vector3 enemylocation = new Vector3(p.transform.position.x +1, p.transform.position.y, p.transform.position.z);
                    GameObject enemy = GetPieceAtLocation(enemylocation);
                    RemovePieceGlobally(enemy.name);
                    CapturePieceGlobally(enemy.name);
                }
                break;
            case Piece.Color.White:
                Vector3 wupleft = new Vector3(p.transform.position.x - 1, p.transform.position.y, p.transform.position.z - 1);
                Vector3 wupright = new Vector3(p.transform.position.x + 1, p.transform.position.y, p.transform.position.z - 1);
                if (p.EnPassantLeft && wupleft.x == point.x && wupleft.z == point.z)
                {
                    Vector3 enemylocation = new Vector3(p.transform.position.x - 1, p.transform.position.y, p.transform.position.z);
                    GameObject enemy = GetPieceAtLocation(enemylocation);
                    RemovePieceGlobally(enemy.name);
                    CapturePieceGlobally(enemy.name);
                }
                if (p.EnPassantRight && wupright.x == point.x && wupright.z == point.z)
                {
                    Vector3 enemylocation = new Vector3(p.transform.position.x + 1, p.transform.position.y, p.transform.position.z);
                    GameObject enemy = GetPieceAtLocation(enemylocation);
                    RemovePieceGlobally(enemy.name);
                    CapturePieceGlobally(enemy.name);
                }
                break;
        }
       
    }

    public void UpdateEPCounter()
    {
        foreach (GameObject piece in Allpieces)
        {
            Piece p = piece.GetComponent<Piece>();
            if (p.piecetype == Piece.Pieces.Peon && (p.EnPassantLeft || p.EnPassantRight))
            {
                p.num_of_turns--;
            }
        }
    }

    public void ResetEP()
    {
        switch (current_player)
        {
            case Player.Black:
                for (int i = (int)transform.position.x; i < (int)transform.position.x + 7; i++)
                {
                    Vector3 fourthrank = new Vector3(i, 0, 15);
                    GameObject piece = GetPieceAtLocation(fourthrank);
                    if (piece != null)
                    {
                        Piece p = piece.GetComponent<Piece>();
                        if (p != null)
                        {
                            if (p.EnPassantLeft)
                            {
                                p.CanEnPassantLeft = false;
                                UpdateEnPassantStatusGlobal(piece.name, false, 3);
                            }
                            if (p.EnPassantRight)
                            {
                                p.CanEnPassantRight = false;
                                UpdateEnPassantStatusGlobal(piece.name, false, 4);
                            }
                            if (p.EnPassantLeft && p.EnPassantRight)
                            {
                                p.CanEnPassantRight = false;
                                p.CanEnPassantLeft = false;
                                UpdateEnPassantStatusGlobal(piece.name, false, 5);
                            }
                        }
                    }
                }
                break;
            case Player.White:
                for (int i = (int)transform.position.x; i < (int)transform.position.x + 7; i++)
                {
                    Vector3 fifthrank = new Vector3(i, 0, 14);
                    GameObject piece = GetPieceAtLocation(fifthrank);
                    if (piece != null)
                    {
                        Piece p = piece.GetComponent<Piece>();
                        if (p != null)
                        {
                            if (p.pColor == Piece.Color.White && p.piecetype == Piece.Pieces.Peon)
                            {
                                if (p.EnPassantLeft)
                                {
                                    p.CanEnPassantLeft = false;
                                    UpdateEnPassantStatusGlobal(piece.name, false, 3);
                                }
                                if (p.EnPassantRight)
                                {
                                    p.CanEnPassantRight = false;
                                    UpdateEnPassantStatusGlobal(piece.name, false, 4);
                                }
                                if (p.EnPassantLeft && p.EnPassantRight)
                                {
                                    p.CanEnPassantRight = false;
                                    p.CanEnPassantLeft = false;
                                    UpdateEnPassantStatusGlobal(piece.name, false, 5);
                                }
                            }
                        }
                    }
                }
                break;
        }
    }

    public void CanPawnEP()
    {
        switch (current_player)
        {
            case Player.Black:
                for (int i = (int)transform.position.x; i < (int)transform.position.x + 7; i++)
                {
                    Vector3 fourthrank = new Vector3(i, 0, 15);
                    GameObject piece = GetPieceAtLocation(fourthrank);
                    if (piece != null)
                    {
                        Piece p = piece.GetComponent<Piece>();
                        if (p != null)
                        {
                            if (p.pColor == Piece.Color.Black && p.piecetype == Piece.Pieces.Peon)
                            {
                                Vector3 left = new Vector3(i - 1, 0, 15);
                                Vector3 right = new Vector3(i + 1, 0, 15);
                                Vector3 upleft = new Vector3(i - 1, 0, 16);
                                Vector3 upright = new Vector3(i + 1, 0, 16);
                                GameObject pieceleft = GetPieceAtLocation(left);
                                if (pieceleft != null)
                                {
                                    Piece pleft = pieceleft.GetComponent<Piece>();
                                    if (pleft != null)
                                    {
                                        if (pleft.pColor == Piece.Color.White && pleft.piecetype == Piece.Pieces.Peon && pleft.num_of_moves == 1 && !HasPiece(upleft))
                                        {

                                            p.EnPassantLeft = true;
                                            p.num_of_turns = 1;
                                            UpdateEnPassantStatusGlobal(piece.name, true, 1);

                                        }

                                    }
                                }

                                GameObject pieceright = GetPieceAtLocation(right);
                                if (pieceright != null)
                                {
                                    Piece pright = pieceright.GetComponent<Piece>();
                                    if (pright != null)
                                    {
                                        if (pright.pColor == Piece.Color.White && pright.piecetype == Piece.Pieces.Peon && pright.num_of_moves == 1 && !HasPiece(upright))
                                        {

                                            p.EnPassantRight = true;
                                            p.num_of_turns = 1;
                                            UpdateEnPassantStatusGlobal(piece.name, true, 2);

                                        }

                                    }
                                }

                            }
                        }
                    }
                }

                break;
            case Player.White:
                for (int i = (int)transform.position.x; i < (int)transform.position.x + 7; i++)
                {
                    Vector3 fifthrank = new Vector3(i, 0, 14);
                    GameObject piece = GetPieceAtLocation(fifthrank);
                    if (piece != null)
                    {
                        Piece p = piece.GetComponent<Piece>();
                        if (p != null)
                        {
                            if (p.pColor == Piece.Color.White && p.piecetype == Piece.Pieces.Peon)
                            {
                                Vector3 left = new Vector3(i - 1, 0, 14);
                                Vector3 right = new Vector3(i + 1, 0, 14);
                                Vector3 upleft = new Vector3(i - 1, 0, 13);
                                Vector3 upright = new Vector3(i + 1, 0, 13);
                                GameObject pieceleft = GetPieceAtLocation(left);
                                if (pieceleft != null)
                                {
                                    Piece pleft = pieceleft.GetComponent<Piece>();
                                    if (pleft != null)
                                    {
                                        if (pleft.pColor == Piece.Color.Black && pleft.piecetype == Piece.Pieces.Peon && pleft.num_of_moves == 1 && !HasPiece(upleft))
                                        {

                                            p.EnPassantLeft = true;
                                            p.num_of_turns = 1;
                                            UpdateEnPassantStatusGlobal(piece.name, true, 1);
                                        }

                                    }
                                }

                                GameObject pieceright = GetPieceAtLocation(right);
                                if (pieceright != null)
                                {
                                    Piece pright = pieceright.GetComponent<Piece>();
                                    if (pright != null)
                                    {
                                        if (pright.pColor == Piece.Color.Black && pright.piecetype == Piece.Pieces.Peon && pright.num_of_moves == 1 && !HasPiece(upright))
                                        {

                                            p.EnPassantRight = true;
                                            p.num_of_turns = 1;
                                            UpdateEnPassantStatusGlobal(piece.name, true, 2);
                                        }

                                    }
                                }

                            }
                        }
                    }
                }
                break;
        }
    }

    void GeneratePieces() {
        Allpieces = new List<GameObject>();
        allpieces = GameObject.FindGameObjectsWithTag("Piece");
        foreach (GameObject g in allpieces) {
            Allpieces.Add(g);
        }
        
    }

    public void ActivatePromotionMenu()
    {
        PromotionGUI.SetActive(true);
        SetPauseStateGlobal(true);
        
    }

    public void DeactivatePromotionMenu()
    {
        PromotionGUI.SetActive(false);
        SetPauseStateGlobal(false);
    }

    [PunRPC]
    void CreateNewPiece(int piece, string player, Vector3 pos)
    {
        
            if (player == "Black")
            {
                
                GameObject bp = GameObject.Instantiate(black_pieces[piece], pos, black_pieces[piece].transform.rotation);
                bp.name = black_pieces[piece].name + counter++;
                objectname = bp.name;


                Debug.Log("Fired");
            }
            if (player == "White")
            {
                
                GameObject wp = GameObject.Instantiate(white_pieces[piece], pos, white_pieces[piece].transform.rotation);
                wp.name = white_pieces[piece].name + counter++;
                objectname = wp.name;

        }
    }
    [PunRPC]
    void AddPiece(string name)
    {
        GameObject piece = GameObject.Find(name);
        Allpieces.Add(piece);
    }

    [PunRPC]
    void SetBoardStatus(int status)
    {
        switch(status)
        {
            case 1:
                currentBoardStatus = Status.FreeMove;
                break;
            case 2:
                currentBoardStatus = Status.PieceSelected;
                break;
            case 3:
                currentBoardStatus = Status.PieceMoving;
                break;


        }
    }
    public void UpdateBoardStatus(int status)
    {
        PhotonView photonView = PhotonView.Get(this);

        photonView.RPC("SetBoardStatus", RpcTarget.All, status);
    }

    [PunRPC]
    void SetCurrentPiece(string name)
    {
        GameObject piece = GameObject.Find(name);
        currentPiece = piece;
    }
    [PunRPC]
    void DestroyPiece()
    {
        Destroy(currentPiece);
    }

    void DestroySceneObject()
    {
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("DestroyPiece", RpcTarget.All);
    }

    public void SetCurrentPieceGlobally(string name)
    {
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("SetCurrentPiece", RpcTarget.All, name);

    }

    void AddPieceGlobally(string name)
    {
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("AddPiece", RpcTarget.All, name);
    }

    void UpdatePiece(int piece, string player, Vector3 pos)
    {
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("CreateNewPiece", RpcTarget.All, piece, player, pos);
    }

    public void SwapPiece(int piece)
    {
        Debug.Log("Piece swapped");
        switch (current_player)
        {
            case Player.White:
                Vector3 wposition = new Vector3(currentPiece.transform.position.x, white_pieces[piece].transform.position.y, currentPiece.transform.position.z);
                UpdatePiece(piece, "White", wposition);
                RemovePieceGlobally(currentPiece.name);
                AddPieceGlobally(objectname);
                DestroySceneObject();
                break;
            case Player.Black:
                Vector3 position = new Vector3(currentPiece.transform.position.x, black_pieces[piece].transform.position.y, currentPiece.transform.position.z);
                UpdatePiece(piece, "Black", position);
                RemovePieceGlobally(currentPiece.name);
                AddPieceGlobally(objectname);
                DestroySceneObject();
                break;
        }
    }

    GameObject GetBlackKing()
    {
        foreach (GameObject g in Allpieces)
        {
            Piece p = g.GetComponent<Piece>();
            if (p.pColor == Piece.Color.Black && p.piecetype == Piece.Pieces.King)
            {
                return g;
            }
        }
        return null;
    }
    GameObject GetWhiteKing()
    {
        foreach (GameObject g in Allpieces)
        {
            Piece p = g.GetComponent<Piece>();
            if (p.pColor == Piece.Color.White && p.piecetype == Piece.Pieces.King)
            {
                return g;
            }
        }
        return null;
    }

    public List<Piece> GetAllPiecesOfType(Piece.Pieces type)
    {
        List<Piece> pieces = new List<Piece>();
        foreach (GameObject g in Allpieces)
        {
            Piece p = g.GetComponent<Piece>();
            if (p.piecetype == type)
            {
                pieces.Add(p);
            }
        }
        return pieces;
    }

    

    public GameObject GetPieceAtLocation(Vector3 pos) {
        
        foreach (GameObject g in Allpieces)
        {
           
            if (g.transform.position.x == pos.x && g.transform.position.z == pos.z) {  return g; }
           
        }
        return null;
    }

    public bool HasPiece(Vector3 pos)
    {
        foreach (GameObject g in Allpieces)
        {

            if (g.transform.position.x == pos.x && g.transform.position.z == pos.z) { return true; }

        }
        return false;
    }

    bool isAttacked(List<Vector3> one, List<Vector3> two)
    {
        foreach (Vector3 o in one)
        {
            foreach (Vector3 t in two)
            {
                if (o.x == t.x && o.z == t.z) return true;
            }
        }
        return false;
    }

    public bool canBePromoted(Piece p)
    {
        if (p.pColor == Piece.Color.White && p.piecetype == Piece.Pieces.Peon && p.transform.position.z == p.startposition.z - 6)
        {
            return true;
        }
        if (p.pColor == Piece.Color.Black && p.piecetype == Piece.Pieces.Peon && p.transform.position.z == p.startposition.z + 6)
        {
            return true;
        }
        return false;
    }

    public bool CanCastle(GameObject rook, Vector3 point)
    {
        Debug.Log("Castle");
        Vector3 castlemoveOOO = new Vector3(rook.transform.position.x - 3, rook.transform.position.y, rook.transform.position.z);
        Vector3 castlemoveOO = new Vector3(rook.transform.position.x + 2, rook.transform.position.y, rook.transform.position.z);
        switch (current_player)
        {
            case Player.Black:
                Piece rookp = rook.GetComponent<Piece>();
                GameObject blackking = GetBlackKing();
                Piece blackp = blackking.GetComponent<Piece>();
                List<Vector3> line = GetLineOfAttackNA(rook.transform.position, blackking.transform.position);
                List<Vector3> attacks = new List<Vector3>();
                List<GameObject> o = GetPiecesinLine(line);
                foreach (GameObject obj in Allpieces)
                {
                    Piece p = obj.GetComponent<Piece>();
                    if (p.pColor == Piece.Color.White && p.piecetype != Piece.Pieces.Peon)
                    {
                        attacks.AddRange(p.GetPiecePositions(obj.transform.position));
                    }
                    else if (p.pColor == Piece.Color.White && p.piecetype == Piece.Pieces.Peon)
                    {
                        attacks.AddRange(p.GetPositions(obj.transform.position));
                    }

                }
                
                return (!rookp.hasMovedBefore && !blackp.hasMovedBefore && o.Count == 0 && !isAttacked(line, attacks)
                   && !isBlackPlayerinCheck);





            case Player.White:
                Piece rookpw = rook.GetComponent<Piece>();
                GameObject whiteking = GetWhiteKing();
                Piece whitep = whiteking.GetComponent<Piece>();
                List<Vector3> wline = GetLineOfAttackNA(rook.transform.position, whiteking.transform.position);
                List<Vector3> wattacks = new List<Vector3>();
                List<GameObject> wo = GetPiecesinLine(wline);
                foreach (GameObject obj in Allpieces)
                {
                    Piece p = obj.GetComponent<Piece>();
                    if (p.pColor == Piece.Color.Black && p.piecetype != Piece.Pieces.Peon)
                    {
                        wattacks.AddRange(p.GetPiecePositions(obj.transform.position));
                    }
                    else if (p.pColor == Piece.Color.Black && p.piecetype == Piece.Pieces.Peon)
                    {
                        wattacks.AddRange(p.GetPositions(obj.transform.position));
                    }

                }
                Debug.Log(rookpw.hasMovedBefore);
                Debug.Log(whitep.hasMovedBefore);
                Debug.Log(wo.Count);
                Debug.Log(isAttacked(wline, wattacks));
                Debug.Log(isWhitePlayerinCheck);
                return (!rookpw.hasMovedBefore && !whitep.hasMovedBefore && wo.Count == 0 && !isAttacked(wline, wattacks)
                    && !isWhitePlayerinCheck);
        }
                
        return false;

    }

    public void Castle(Piece rook, Vector3 point)
    {
        Piece p = rook.GetComponent<Piece>();
        Vector3 castlemoveOO = new Vector3(rook.transform.position.x + 2, rook.transform.position.y, rook.transform.position.z);
        
        Vector3 castlemoveOOO = new Vector3(rook.transform.position.x - 3, rook.transform.position.y, rook.transform.position.z);
        
        switch (current_player) {
         case Player.White:
        Piece p1 = GetWhiteKing().GetComponent<Piece>();
        Vector3 castlemovekOO = new Vector3(p1.transform.position.x - 2, p1.transform.position.y, p1.transform.position.z);
        Vector3 castlemovekOOO = new Vector3(p1.transform.position.x + 2, p1.transform.position.y, p1.transform.position.z);
                //Debug.Log("Castle");

        if (rook.transform.position.x == 1 && castlemoveOO.x == point.x && castlemoveOO.z == point.z)
        {
            Debug.Log("CastleK");
            UpdatePieceGlobally(castlemoveOO, p.name);
            UpdatePieceGlobally(castlemovekOO, p1.name);
        }
        else if (rook.transform.position.x == 8 && castlemoveOOO.x == point.x && castlemoveOOO.z == point.z)
        {
            Debug.Log("CastleQ");
            UpdatePieceGlobally(castlemoveOOO, p.name);
            UpdatePieceGlobally(castlemovekOOO, p1.name);
        }
                break;
            case Player.Black:
                Piece bp1 = GetBlackKing().GetComponent<Piece>();
                Vector3 bcastlemovekOO = new Vector3(bp1.transform.position.x - 2, bp1.transform.position.y, bp1.transform.position.z);
                Vector3 bcastlemovekOOO = new Vector3(bp1.transform.position.x + 2, bp1.transform.position.y, bp1.transform.position.z);
                //Debug.Log("Castle");

                if (rook.transform.position.x == 1 && castlemoveOO.x == point.x && castlemoveOO.z == point.z)
                {
                    Debug.Log("CastleK");
                    UpdatePieceGlobally(castlemoveOO, p.name);
                    UpdatePieceGlobally(bcastlemovekOO, bp1.name);
                }
                else if (rook.transform.position.x == 8 && castlemoveOOO.x == point.x && castlemoveOOO.z == point.z)
                {
                    Debug.Log("CastleQ");
                    UpdatePieceGlobally(castlemoveOOO, p.name);
                    UpdatePieceGlobally(bcastlemovekOOO, bp1.name);
                }
                break;
    }
    }

    public bool EnemyPieceAtLocation(Vector3 pos, Piece.Color color)
    {
        switch (color)
        {
            case Piece.Color.White:
                foreach (GameObject g in Allpieces)
                {
                    Piece p = g.GetComponent<Piece>();
                    if ((g.transform.position.x == pos.x && g.transform.position.z == pos.z) && p.pColor == Piece.Color.Black) { return true; }

                }
                return false;
            case Piece.Color.Black:
                foreach (GameObject g in Allpieces)
                {
                    Piece p = g.GetComponent<Piece>();
                    if ((g.transform.position.x == pos.x && g.transform.position.z == pos.z) && p.pColor == Piece.Color.White) { return true; }

                }
                return false;
            default:
                return false;


        }
    }

    public bool EnemyPieceAtLocationOpPlayer(Vector3 pos, Player player)
    {
        switch (player)
        {
            case Player.White:
                foreach (GameObject g in Allpieces)
                {
                    Piece p = g.GetComponent<Piece>();
                    if ((g.transform.position.x == pos.x && g.transform.position.z == pos.z) && p.pColor == Piece.Color.White) { return true; }

                }
                return false;
            case Player.Black:
                foreach (GameObject g in Allpieces)
                {
                    Piece p = g.GetComponent<Piece>();
                    if ((g.transform.position.x == pos.x && g.transform.position.z == pos.z) && p.pColor == Piece.Color.Black) { Debug.Log(p.piecetype); return true; }

                }
                return false;
            default:
                return false;


        }
    }
    [PunRPC]
    void SetCheck(bool c)
    {
        if (current_player == Player.White)
        {
            isBlackPlayerinCheck = c;

        }
        else if (current_player == Player.Black)
        {
            isWhitePlayerinCheck = c;
        }
    }

    public void SetCheckGlobal(bool c)
    {
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("SetCheck", RpcTarget.All, c);
    }

    public void NextPlayer()
    {
        if (current_player == Player.White)
        {
            current_player = Player.Black;
            UpdatePlayerGlobally("Black");
        }
        else if (current_player == Player.Black)
        {
            current_player = Player.White;
            UpdatePlayerGlobally("White");
        }
        
    }

    List<Vector3> GetLineOfAttack(Vector3 attacker, Vector3 king)
    {
        List<Vector3> loa = new List<Vector3>();
        if (attacker.x < king.x && attacker.z == king.z)
        {
            loa.Add(attacker);
            for (int i = 1; i < (int)king.x-attacker.x; i++)
            {
                loa.Add(new Vector3(attacker.x + i, king.y, attacker.z));
            }
            
        }
        if (attacker.x > king.x && attacker.z == king.z)
        {
            loa.Add(attacker);
            for (int i = 1; i < (int)attacker.x - king.x; i++)
            {
                loa.Add(new Vector3(king.x + i, king.y, king.z));
            }
        }
        if (attacker.z < king.z && attacker.x == king.x)
        {
            loa.Add(attacker);
            for (int i = 1; i < (int)king.z - attacker.z; i++)
            {
                loa.Add(new Vector3(attacker.x, attacker.y, attacker.z + i));
            }
        }
        if (attacker.z > king.z && attacker.x == king.x)
        {
            loa.Add(attacker);
            for (int i = 1; i < (int)attacker.z - king.z; i++)
            {
                loa.Add(new Vector3(king.x, king.y, king.z + i));
            }
        }
        if (attacker.x < king.x && attacker.z < king.z)
        {
            loa.Add(attacker);
            for (int i = 1; i < (int)king.x - attacker.x; i++)
            {
               
                
                    loa.Add(new Vector3(attacker.x + i, king.y, attacker.z+i));
                
            }
        }
        if (attacker.x > king.x && attacker.z > king.z)
        {
            loa.Add(attacker);
            for (int i = 1; i < (int)attacker.x - king.x; i++)
            {
                
                
                    loa.Add(new Vector3(king.x + i, king.y, king.z+i));
                
            }
        }
        if (attacker.x < king.x && attacker.z > king.z)
        {
            loa.Add(attacker);
            for (int i = 1; i < (int)(king.x - attacker.x); i++)
            {
                
                
                    loa.Add(new Vector3(attacker.x + i, king.y, king.z + i));
                
            }
        }
        if (attacker.x > king.x && attacker.z < king.z)
        {
            loa.Add(attacker);
            for (int i = 1; i < (int)attacker.x - king.x; i++)
            {
              
                
                    loa.Add(new Vector3(king.x + i, king.y, attacker.z + i));
                
            }
        }
            return loa;
    }

    List<Vector3> GetLineOfAttackNA(Vector3 attacker, Vector3 king)
    {
        List<Vector3> loa = new List<Vector3>();
        if (attacker.x < king.x && attacker.z == king.z)
        {
            
            for (int i = 1; i < (int)king.x - attacker.x; i++)
            {
                loa.Add(new Vector3(attacker.x + i, king.y, attacker.z));
            }

        }
        if (attacker.x > king.x && attacker.z == king.z)
        {
            
            for (int i = 1; i < (int)attacker.x - king.x; i++)
            {
                loa.Add(new Vector3(king.x + i, king.y, king.z));
            }
        }
        if (attacker.z < king.z && attacker.x == king.x)
        {
            
            for (int i = 1; i < (int)king.z - attacker.z; i++)
            {
                loa.Add(new Vector3(attacker.x, attacker.y, attacker.z + i));
            }
        }
        if (attacker.z > king.z && attacker.x == king.x)
        {
            
            for (int i = 1; i < (int)attacker.z - king.z; i++)
            {
                loa.Add(new Vector3(king.x, king.y, king.z + i));
            }
        }
        if (attacker.x < king.x && attacker.z < king.z)
        {
           
            for (int i = 1; i < (int)king.x - attacker.x; i++)
            {


                loa.Add(new Vector3(attacker.x + i, king.y, attacker.z + i));

            }
        }
        if (attacker.x > king.x && attacker.z > king.z)
        {
            
            for (int i = 1; i < (int)attacker.x - king.x; i++)
            {


                loa.Add(new Vector3(king.x + i, king.y, king.z + i));

            }
        }
        if (attacker.x < king.x && attacker.z > king.z)
        {
            
            for (int i = 1; i < (int)(king.x - attacker.x); i++)
            {


                loa.Add(new Vector3(attacker.x + i, king.y, king.z + i));

            }
        }
        if (attacker.x > king.x && attacker.z < king.z)
        {
            
            for (int i = 1; i < (int)attacker.x - king.x; i++)
            {


                loa.Add(new Vector3(king.x + i, king.y, attacker.z + i));

            }
        }
        return loa;
    }

    public void ScanForCheckMate(Player currentplayer, List<GameObject> threats)
    {
        List<GameObject> kingthreats = new List<GameObject>();
        List<Vector3> piecethreats = new List<Vector3>();
        List<Vector3> kingmoves = new List<Vector3>();
        List<Vector3> piecemoves = new List<Vector3>();
        List<Vector3> t = new List<Vector3>();
        List<Vector3> interposition = new List<Vector3>();

        switch (currentplayer)
        {
            case Player.Black:
                GameObject g1 = GetWhiteKing();
                Piece kingwhite = g1.GetComponent<Piece>();
                kingthreats = GetPinnedPieces();
                
                kingmoves = kingwhite.GetPiecePositions(g1.transform.position);

                //Debug.Log(threats.Count);
                foreach (GameObject p in threats)
                {


                    
                    Piece piece = p.GetComponent<Piece>();
                    if (piece.piecetype != Piece.Pieces.Knight)
                    {
                        t.AddRange(GetLineOfAttack(p.transform.position, g1.transform.position));
                    }
                    else if (piece.piecetype == Piece.Pieces.Knight)
                    {
                        t.Add(piece.transform.position);
                    }
                    piecethreats.AddRange(piece.GenerateCheckMateMoves(p.transform.position));


                }
                foreach (GameObject p in Allpieces)
                {
                    Piece pie = p.GetComponent<Piece>();
                    if (pie.pColor == Piece.Color.Black && !threats.Contains(p) && pie.piecetype != Piece.Pieces.Peon)
                    {
                        //Debug.Log(pie.piecetype);
                        piecethreats.AddRange(pie.GetPositions(p.transform.position));
                        
                    }
                    if (pie.pColor == Piece.Color.Black && !threats.Contains(p) && pie.piecetype == Piece.Pieces.Peon)
                    {
                        piecethreats.AddRange(pie.GenerateCheckMateMoves(p.transform.position));
                    }
                }
                //Debug.Log(piecethreats.Count);
                foreach (GameObject f in Allpieces)
                {
                    Piece p = f.GetComponent<Piece>();
                    if (p.pColor == Piece.Color.White && p.piecetype != Piece.Pieces.King && !kingthreats.Contains(f))
                    {
                        List<Vector3> k = p.GetPiecePositions(f.transform.position);
                        foreach (Vector3 r in k)
                        {
                            if (!piecemoves.Contains(r)) piecemoves.Add(r);
                        }

                    }
                }
               // Debug.Log(piecemoves.Count);
                piecemoves = piecemoves.Select(x => x).Distinct().ToList<Vector3>();

                for (int i = piecethreats.Count - 1; i >= 0; i--)
                {
                    for (int j = kingmoves.Count - 1; j >= 0; j--)
                    {
                        //Debug.Log(piecethreats[i] + ": " + kingmoves[j]);
                        if (piecethreats[i].x == kingmoves[j].x && piecethreats[i].z == kingmoves[j].z)
                        {
                            Debug.Log(kingmoves[j]);
                            kingmoves.RemoveAt(j);
                        }
                    }
                }
                //Debug.Log(piecemoves.Count);
                var list = new HashSet<Vector3>(piecemoves).ToList();


                for (int i = piecemoves.Count - 1; i >= 0; i--)
                {
                    for (int j = t.Count - 1; j >= 0; j--)
                    {
                        //Debug.Log(t[j] + ": " + list[i]);
                        if (piecemoves[i].x == t[j].x && piecemoves[i].z == t[j].z)
                        {

                            interposition.Add(t[j]);
                            t.RemoveAt(j);
                        }
                    }

                }

                Debug.Log("Kings moves left: " + kingmoves.Count + "Interference: " + interposition.Count);
                if ((kingmoves.Count == 0 && interposition.Count == 0) || (kingmoves.Count == 0 && interposition.Count != 0 && threats.Count > 1)) { Debug.Log("Checkmate"); }
                break;
                
            case Player.White:
                GameObject g = GetBlackKing();
                Piece king = g.GetComponent<Piece>();
                kingthreats.AddRange(GetPinnedPieces());

                kingmoves = king.GetPiecePositions(g.transform.position);
                
                //Debug.Log(threats.Count);
                foreach (GameObject p in threats)
                {

                    Piece piece = p.GetComponent<Piece>();
                    if (piece.piecetype != Piece.Pieces.Knight)
                    {
                        t.AddRange(GetLineOfAttack(p.transform.position, g.transform.position));
                    }
                    else if (piece.piecetype == Piece.Pieces.Knight)
                    {
                        t.Add(piece.transform.position);
                    }
                    piecethreats.AddRange(piece.GenerateCheckMateMoves(p.transform.position));



                }
                foreach (GameObject p in Allpieces)
                {
                    Piece pie = p.GetComponent<Piece>();
                    if (pie.pColor == Piece.Color.White && !threats.Contains(p) )
                    {
                        //Debug.Log(pie.piecetype);
                        piecethreats.AddRange(pie.GetPositions(p.transform.position));

                    }
                    
                }
                //Debug.Log(kingthreats.Count);
                foreach (GameObject f in Allpieces)
                {
                    Piece p = f.GetComponent<Piece>();
                    if (p.pColor == Piece.Color.Black && p.piecetype != Piece.Pieces.King && !kingthreats.Contains(f))
                    {
                        List<Vector3> k = p.GetPiecePositions(f.transform.position);
                        foreach (Vector3 r in k)
                        {
                            if(!piecemoves.Contains(r)) piecemoves.Add(r);
                        }
                        
                    }
                }
                //Debug.Log(piecemoves.Count);
                piecemoves = piecemoves.Select(x => x).Distinct().ToList<Vector3>();
                
                for (int i = piecethreats.Count - 1; i >= 0; i--)
                {
                    for (int j = kingmoves.Count - 1; j >= 0; j--)
                    {
                        //Debug.Log(piecethreats[i] + ": " + kingmoves[j]);
                        if (piecethreats[i].x == kingmoves[j].x && piecethreats[i].z == kingmoves[j].z)
                        {
                            Debug.Log(kingmoves[j]);
                            kingmoves.RemoveAt(j);
                        }
                    }
                }
                //Debug.Log(piecemoves.Count);
               
                
               
                for (int i = piecemoves.Count-1; i >=0; i--)
                {
                    for (int j = t.Count-1; j >=0; j--)
                    {
                       
                        if (piecemoves[i].x == t[j].x && piecemoves[i].z == t[j].z)
                        {

                            interposition.Add(t[j]);
                            t.RemoveAt(j);
                            
                        }
                    }
                    
                }

                Debug.Log("Kings moves left: " + kingmoves.Count + "Interference: " + interposition.Count);
                if ((kingmoves.Count == 0 && interposition.Count == 0) || (kingmoves.Count == 0 && interposition.Count != 0 && threats.Count > 1)) { Debug.Log("Checkmate"); }
                break;
        }
    }

    
    public List<GameObject> IsKingInCheck()
    {
        List<Vector3> positions = new List<Vector3>();
        List<GameObject> threats = new List<GameObject>();
        switch (current_player)
        {
            case Player.White:
                foreach (GameObject g in Allpieces)
                {
                    Piece P = g.GetComponent<Piece>();
                    if (P.pColor == Piece.Color.White)
                    {
                        List<Vector3> p = P.GetPiecePositions(g.transform.position);
                        foreach (Vector3 pos in p)
                        {
                            GameObject t = GetPieceAtLocation(pos);
                            if (t != null)
                            {
                                Piece piece = t.GetComponent<Piece>();
                                if (piece.pColor == Piece.Color.Black && piece.piecetype == Piece.Pieces.King)
                                {
                                    threats.Add(g);
                                }
                            }
                        }

                    }
                    
                }
                isScanComplete = true;
                return threats;
            case Player.Black:
                foreach (GameObject g in Allpieces)
                {
                    Piece P = g.GetComponent<Piece>();
                    if (P.pColor == Piece.Color.Black)
                    {
                        List<Vector3> p = P.GetPiecePositions(g.transform.position);
                        foreach (Vector3 pos in p)
                        {
                            GameObject t = GetPieceAtLocation(pos);
                            if (t != null)
                            {
                                Piece piece = t.GetComponent<Piece>();
                                if (piece.pColor == Piece.Color.White && piece.piecetype == Piece.Pieces.King)
                                {
                                    threats.Add(g);
                                }
                            }
                        }

                    }

                }
                isScanComplete = true;
                return threats;
            default:
                return null;
           
        }
    }

    public List<GameObject> GetPinnedPieces()
    {
        List<GameObject> threats = new List<GameObject>();
        List<GameObject> pinned_pieces = new List<GameObject>();
        
        GameObject king;
        switch (current_player)
        {
            case Player.White:
                king = GetBlackKing();
                foreach (GameObject g in Allpieces)
                {
                    Piece P = g.GetComponent<Piece>();
                    if (P.pColor == Piece.Color.White && (P.piecetype == Piece.Pieces.Rook || P.piecetype == Piece.Pieces.Bishop || P.piecetype == Piece.Pieces.Queen))
                    {
                        List<Vector3> p = P.GenerateCheckMateMoves(g.transform.position);
                        foreach (Vector3 pos in p)
                        {
                            GameObject t = GetPieceAtLocation(pos);
                            if (t != null)
                            {
                                Piece piece = t.GetComponent<Piece>();
                                if (piece.pColor == Piece.Color.Black && piece.piecetype == Piece.Pieces.King)
                                {
                                    threats.Add(g);
                                }
                            }
                        }

                    }

                }
               // Debug.Log("Xray threats: " + threats.Count);
                foreach (GameObject threat in threats)
                {
                    List<Vector3> positions = new List<Vector3>();
                    positions.AddRange(GetLineOfAttackNA(threat.transform.position, king.transform.position));
                    List<GameObject> p = GetPiecesinLine(positions);
                    //Debug.Log("Pieces in line: " + p.Count);
                    if (p.Count == 1)
                    {
                        Piece piece = p[0].GetComponent<Piece>();
                        if (piece.pColor == Piece.Color.Black)
                        {
                            //Debug.Log("Flag");
                            
                            pinned_pieces.AddRange(p);
                        }
                    }
                    
                }
                return pinned_pieces;
            case Player.Black:
                king = GetWhiteKing();
                foreach (GameObject g in Allpieces)
                {
                    Piece P = g.GetComponent<Piece>();
                    if (P.pColor == Piece.Color.Black && (P.piecetype == Piece.Pieces.Rook || P.piecetype == Piece.Pieces.Bishop || P.piecetype == Piece.Pieces.Queen))
                    {
                        List<Vector3> p = P.GenerateCheckMateMoves(g.transform.position);
                        foreach (Vector3 pos in p)
                        {
                            GameObject t = GetPieceAtLocation(pos);
                            if (t != null)
                            {
                                Piece piece = t.GetComponent<Piece>();
                                if (piece.pColor == Piece.Color.White && piece.piecetype == Piece.Pieces.King)
                                {
                                    threats.Add(g);
                                }
                            }
                        }

                    }

                }
                foreach (GameObject threat in threats)
                {
                    List<Vector3> positions = new List<Vector3>();
                    positions.AddRange(GetLineOfAttackNA(threat.transform.position, king.transform.position));
                    List<GameObject> p = GetPiecesinLine(positions);
                    if (p.Count == 1)
                    {
                        Piece piece = p[0].GetComponent<Piece>();
                        if (piece.pColor == Piece.Color.White)
                        {
                            pinned_pieces.AddRange(p);
                        }
                    }

                }
                return pinned_pieces;
            default:
                return null;

        }

    }

    List<GameObject> GetPiecesinLine(List<Vector3> positions)
    {
        List<GameObject> go = new List<GameObject>();
        foreach (Vector3 loa in positions)
        {
            GameObject t = GetPieceAtLocation(loa);
            if (t != null)
            {
                Piece p = t.GetComponent<Piece>();
                if (p != null)
                {
                    if (p.piecetype != Piece.Pieces.King)  go.Add(t);
                }
            }
        }

        return go;
    }

    
}
