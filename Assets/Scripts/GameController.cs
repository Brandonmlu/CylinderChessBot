using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject Board;
    public GameObject WhitePieces;
    public GameObject BlackPieces;
    public GameObject SelectedPiece;
    public bool WhiteTurn = true;
    public List<Transform> validMoves = new List<Transform>();
    BoxController boxController;
    PieceController pieceController;
    TestBot testBot;
    public GameObject savedWhitePieces;
    public GameObject savedBlackPieces;

    // Use this for initialization
    void Start()
    {
        if (pieceController == null) pieceController = FindObjectOfType<PieceController>();
        if (boxController == null) boxController = FindObjectOfType<BoxController>();
        if (testBot == null) testBot = FindObjectOfType<TestBot>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SelectPiece(GameObject piece)
    {
        if (piece.tag == "White" && WhiteTurn == true || piece.tag == "Black" && WhiteTurn == false)
        {
            DeselectPiece();
            SelectedPiece = piece;

            // Highlight
            SelectedPiece.GetComponent<SpriteRenderer>().color = Color.yellow;

            // Put above other pieces
            Vector3 newPosition = SelectedPiece.transform.position;
            newPosition.z = -1;
            SelectedPiece.transform.SetPositionAndRotation(newPosition, SelectedPiece.transform.rotation);
        }
    }

    public void DeselectPiece()
    {
        if (SelectedPiece != null)
        {
            // Remove highlight
            SelectedPiece.GetComponent<SpriteRenderer>().color = Color.white;

            // Put back on the same level as other pieces
            Vector3 newPosition = SelectedPiece.transform.position;
            newPosition.z = 0;
            SelectedPiece.transform.SetPositionAndRotation(newPosition, SelectedPiece.transform.rotation);

            SelectedPiece = null;
        }
    }

    public void EndTurn()
    {
        bool kingIsInCheck = false;
        bool hasValidMoves = false;

        WhiteTurn = !WhiteTurn;

        if (WhiteTurn)
        {
            foreach (Transform piece in WhitePieces.transform)
            {
                //Debug.Log("PIECE: " + piece);
                if (hasValidMoves == false && HasValidMoves(piece.gameObject))
                {
                    hasValidMoves = true;
                }

                if (piece.name.Contains("Pawn"))
                {
                    piece.GetComponent<PieceController>().DoubleStep = false;
                }
                else if (piece.name.Contains("King"))
                {
                    kingIsInCheck = piece.GetComponent<PieceController>().IsInCheck(piece.position);
                }
            }
        }
        else
        {
            foreach (Transform piece in BlackPieces.transform)
            {
                if (hasValidMoves == false && HasValidMoves(piece.gameObject))
                {
                    hasValidMoves = true;
                }

                if (piece.name.Contains("Pawn"))
                {
                    piece.GetComponent<PieceController>().DoubleStep = false;
                }
                else if (piece.name.Contains("King"))
                {
                    kingIsInCheck = piece.GetComponent<PieceController>().IsInCheck(piece.position);
                }
            }
        }

        if (hasValidMoves == false)
        {
            int type = -1;
            if (kingIsInCheck == false)
            {
                Stalemate();
                type = 0;
            }
            else
            {
                Checkmate();
                type = 1;
            }
            testBot.getEndState(WhiteTurn, type);
        }
    }

    public bool HasValidMoves(GameObject piece)
    {
        PieceController pieceController = piece.GetComponent<PieceController>();
        GameObject encounteredEnemy;

        foreach (Transform square in Board.transform)
        {
            if (pieceController.ValidateMovement(piece.transform.position, new Vector3(square.position.x, square.position.y, piece.transform.position.z), out encounteredEnemy))
            {
                //Debug.Log(piece + " on " + square);
                return true;
            }
        }
        return false;
    }

    public Transform[] GetValidMoves(GameObject piece)
    {
        PieceController pieceController = piece.GetComponent<PieceController>();
        GameObject encounteredEnemy;

        foreach (Transform square in Board.transform)
        {
            if (pieceController.ValidateMovement(piece.transform.position, new Vector3(square.position.x, square.position.y, piece.transform.position.z), out encounteredEnemy))
            {
                validMoves.Add(square);
            }
        }
        return validMoves.ToArray();
    }

    public int[,] GetBoardMatrix() {
        int[,] boardMatrix = new int[8, 8];
        Vector3 matrixPos;
        Vector3 position;
        GameObject piece;

        for (int y = 7; y >= 0; y--) {
            for (int x = 0; x < 8; x++) {
                matrixPos = new Vector3(x, y, 0);
                position = boxController.MatrixToCoord(matrixPos);
                piece = pieceController.GetPieceOnPosition(position.x, position.y);

                if (piece == null) {
                    boardMatrix[x,y] = 0;
                    continue;
                }

                //Debug.Log(PieceToInt(piece) + " is on (" + position.x + ", " + position.y + ")");
                boardMatrix[x,y] = PieceToInt(piece);
            }
        }

        return boardMatrix;
    }

    public int PieceToInt(GameObject piece) {
        int val = 0;

        if (piece == null) {
            return 0;
        }

        if (piece.name.Contains("Pawn")) {
            val = 1;
        }
        else if (piece.name.Contains("Bishop")) {
            val = 2;
        }
        else if (piece.name.Contains("Knight")) {
            val = 3;
        }
        else if (piece.name.Contains("Rook")) {
            val = 4;
        }
        else if (piece.name.Contains("Queen")) {
            val = 5;
        }
        else if (piece.name.Contains("King")) {
            val = 6;
        }

        if (piece.name.Contains("Black")) {
            val = -val;
        }

        return val;
    }

    public void ResetScene()
    {
        print("Scene restarted");
        WhiteTurn = true;
        List<Transform> validMoves = new List<Transform>();

        GameObject[] whitePieces = new GameObject[16];
        GameObject[] blackPieces = new GameObject[16];
        
        foreach (Transform child in WhitePieces.transform) {
            GameObject.Destroy(child.gameObject);
        }

        foreach (Transform child in BlackPieces.transform) {
            GameObject.Destroy(child.gameObject);
        }

        GameObject SavedWhitePieces = GameObject.Instantiate(savedWhitePieces);
        SavedWhitePieces.SetActive(true);

        GameObject SavedBlackPieces = GameObject.Instantiate(savedBlackPieces);
        SavedBlackPieces.SetActive(true);
        
        for (int i = 0; i < 16; i++)
        {
            whitePieces[i] = SavedWhitePieces.transform.GetChild(i).gameObject;
            whitePieces[i].GetComponent<PieceController>().WhitePieces = WhitePieces;
        }

        for (int i = 0; i < 16; i++)
        {
            blackPieces[i] = SavedBlackPieces.transform.GetChild(i).gameObject;
            blackPieces[i].GetComponent<PieceController>().BlackPieces = BlackPieces;
        }

        foreach (GameObject child in whitePieces) {
            child.transform.SetParent(WhitePieces.transform);
        }

        foreach (GameObject child in blackPieces) {
            child.transform.SetParent(BlackPieces.transform);
        }

        Destroy(SavedWhitePieces);
        Destroy(SavedBlackPieces);

        pieceController.ResetPieces();
    }

    void Stalemate()
    {
        Debug.Log("Stalemate!");
    }

    void Checkmate()
    {
        Debug.Log("Checkmate!");
    }
}
