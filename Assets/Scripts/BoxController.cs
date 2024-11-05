using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxController : MonoBehaviour
{
    public GameController GameController;

    public float HighestRankY = 3.5f;
    public float LowestRankY = -3.5f;

    // Use this for initialization
    void Start()
    {
        if (GameController == null) GameController = FindObjectOfType<GameController>();

        string algebraicName = "";
        algebraicName += (char)(this.transform.position.x - LowestRankY + 'A');
        algebraicName += this.transform.position.y - LowestRankY + 1;
        this.transform.parent.name = algebraicName;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnMouseDown()
    {
        //Debug.Log("POSITION: " + this.transform.position.x + ", " + this.transform.position.y);
        if (GameController.SelectedPiece != null && GameController.SelectedPiece.GetComponent<PieceController>().IsMoving() == true)
        {
            // Prevent clicks during movement
            return;
        }

        if (GameController.SelectedPiece != null)
        {
            GameController.SelectedPiece.GetComponent<PieceController>().MovePiece(this.transform.position);
        }
    }

    public bool MoveTrigger(Vector3 position) {
        if (GameController.SelectedPiece != null && GameController.SelectedPiece.GetComponent<PieceController>().IsMoving() != true)
        {
            if (GameController.SelectedPiece.GetComponent<PieceController>().MovePiece(position)) {
                return true;
            }
        }
        return false;
    }

    void CoordToMatrix(Vector3 position) {
        double x = position.x;
        double y = position.y;
        int newX = -1;
        int newY = -1;

        switch (x) {
            case -3.5:
                newX = 0;
                break;
            case -2.5:
                newX = 1;
                break;
            case -1.5:
                newX = 2;
                break;
            case -0.5:
                newX = 3;
                break;
            case 0.5:
                newX = 4;
                break;
            case 1.5:
                newX = 5;
                break;
            case 2.5:
                newX = 6;
                break;
            case 3.5:
                newX = 7;
                break;
        }

        switch (y) {
            case -3.5:
                newY = 0;
                break;
            case -2.5:
                newY = 1;
                break;
            case -1.5:
                newY = 2;
                break;
            case -0.5:
                newY = 3;
                break;
            case 0.5:
                newY = 4;
                break;
            case 1.5:
                newY = 5;
                break;
            case 2.5:
                newY = 6;
                break;
            case 3.5:
                newY = 7;
                break;
        }

        Debug.Log("MATRIX POS= X: " + newX + ", Y: " + newY);
    }

    public Vector3 MatrixToCoord(Vector3 position) {
        int x = (int)position.x;
        int y = (int)position.y;
        float newX = -1.0f;
        float newY = -1.0f;

        switch (x) {
            case 0:
                newX = -3.5f;
                break;
            case 1:
                newX = -2.5f;
                break;
            case 2:
                newX = -1.5f;
                break;
            case 3:
                newX = -0.5f;
                break;
            case 4:
                newX = 0.5f;
                break;
            case 5:
                newX = 1.5f;
                break;
            case 6:
                newX = 2.5f;
                break;
            case 7:
                newX = 3.5f;
                break;
        }

        switch (y) {
            case 0:
                newY = -3.5f;
                break;
            case 1:
                newY = -2.5f;
                break;
            case 2:
                newY = -1.5f;
                break;
            case 3:
                newY = -0.5f;
                break;
            case 4:
                newY = 0.5f;
                break;
            case 5:
                newY = 1.5f;
                break;
            case 6:
                newY = 2.5f;
                break;
            case 7:
                newY = 3.5f;
                break;
        }

        //Debug.Log("POS= X: " + newX + ", Y: " + newY);
        return new Vector3(newX, newY, 10.0f);
    }
}
