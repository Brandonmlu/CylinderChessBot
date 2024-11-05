using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AustinHarris.JsonRpc;

public class MyVector3 {
    public float x;
    public float y;
    public float z;
    public MyVector3(Vector3 v) {
        this.x = v.x;
        this.y = v.y;
        this.z = v.z;
    }
    public Vector3 AsVector3() {
        return new Vector3(x, y, z);
    }
}

public class Observation {
    public int[,] board_state;
    public int selected_piece;
    public int in_check;
    public int curr_player;
    public Observation(int[,] board_state, int selected_piece, int in_check, int curr_player) {
        this.board_state = board_state;
        this.selected_piece = selected_piece;
        this.in_check = in_check;
        this.curr_player = curr_player;
    }
}

public class RlResult {
    public float reward;
    public bool finished;
    public Observation obs;
    public RlResult(float reward, bool finished, Observation obs) {
        this.reward = reward;
        this.finished = finished;
        this.obs = obs;
    }
}

public class TestBot : MonoBehaviour
{
    class Rpc : JsonRpcService {
        TestBot agent;

        public Rpc(TestBot agent) {
            this.agent = agent;
        }

        [JsonRpcMethod]
        void Say (string message) {
            Debug.Log($"you sent {message}");
        }

        [JsonRpcMethod]
        void SelectPiece (int index) {
            agent.Select(index);
        }

        [JsonRpcMethod]
        void MovePiece (Vector3 position) {
            agent.Move(position);
        }

        [JsonRpcMethod]
        int[,] GetBoardMatrix () {
            return agent.GetBoard();
        }

        [JsonRpcMethod]
        int InCheck () {
            return agent.InCheck();
        }

        [JsonRpcMethod]
        int getCurrPlayer () {
            return agent.getCurrPlayer();
        }

        [JsonRpcMethod]
        bool isPlayerPiece (int index, int player) {
            return agent.isPlayerPiece(index, player);
        }

        [JsonRpcMethod]
        int getSelectedPiece (int index) {
            return agent.getSelectedPiece(index);
        }

        [JsonRpcMethod]
        int getGameType () {
            return agent.getGameType();
        }

        [JsonRpcMethod]
        bool isValidPieceMove (int index, int piece) {
            return agent.isValidPieceMove(index, piece);
        }

        [JsonRpcMethod]
        RlResult Step(int piece_action, int move_action) {
            return agent.Step(piece_action, move_action);
        }

        [JsonRpcMethod]
        Observation Reset() {
            return agent.Reset();
        }
    }

    Rpc rpc;
    Simulation simulation;
    float reward;
    bool finished;
    int step;
    public GameObject WhitePieces;
    GameObject piece;
    public GameController GameController;
    public PieceController pieceController;
    public BoxController boxController;
    public Transform[] possibleMoves;
    public int gameType = 2; // 0: No bot, 1: Black Bot, 2: Both bots

    // Start is called before the first frame update
    void Start()
    {
        simulation = GetComponent<Simulation>();
        rpc = new Rpc(this);
        
        if (GameController == null) GameController = FindObjectOfType<GameController>();
        if (pieceController == null) pieceController = FindObjectOfType<PieceController>();
        if (boxController == null) boxController = FindObjectOfType<BoxController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /*
        Takes the action int and selects a valid piece and valid move
    */
    public RlResult Step(int piece_action, int move_action) {
        // Select a valid piece and select a valid move
        // Return observations
        reward = 0f;
        
        Debug.Log(getCurrPlayer() + "'s TURN..." + " PIECE ACTION: " + piece_action + " MOVE ACTION: " + move_action);
        Select(piece_action);
        
        if (isValidPieceMove(move_action, piece_action)) {
            Vector3 matrixPos = indexToMatrix(move_action);
            Vector3 pos = boxController.MatrixToCoord(matrixPos);
            
            if (Move(pos)) {
                reward += 0.2f;
            }
            else {
                reward -= 0.5f;
            }
        }
        else {
            reward -= 0.5f;
        }

        simulation.Simulate();
        step += 1;
        
        if (step >= 200000) {
            Debug.Log("Timed out; ending episode");
            finished = true;
        }
        print("Reward: " + reward);
        return new RlResult(reward, finished, GetObservation());
    }

    public Observation Reset() {
        GameController.ResetScene();

        finished = false;
        step = 0;

        return GetObservation();
    }

    public int getGameType() {
        return gameType;
    }

    /*
        Get the observations:
        - Board state
        - In Check
        - Win state
    */
    public Observation GetObservation() {
        int[,] board_state = GameController.GetBoardMatrix();
        int selected_piece = GameController.PieceToInt(GameController.SelectedPiece);
        int in_check = InCheck();
        int curr_player = getCurrPlayer();

        return new Observation(board_state, selected_piece, in_check, curr_player);
    }

    public int getSelectedPiece(int index) {
        Vector3 matrixPos = indexToMatrix(index);
        Vector3 pos = boxController.MatrixToCoord(matrixPos);

        GameObject piece = pieceController.GetPieceOnPosition(pos.x, pos.y);

        return GameController.PieceToInt(piece);
    }

    public void Select(int index) {
        Vector3 matrixPos = indexToMatrix(index);
        Vector3 pos = boxController.MatrixToCoord(matrixPos);

        GameObject piece = pieceController.GetPieceOnPosition(pos.x, pos.y);

        if (piece != null) {
            piece.GetComponent<PieceController>().BotOnMouseDown(); // Select
            reward += 0.1f;
        
            if (GameController.HasValidMoves(piece)) {
                possibleMoves = GameController.GetValidMoves(piece);
                reward += 0.2f;
            }
            else {
                reward -= 0.5f;
            }
        }
    }

    public bool Move(Vector3 position) {
        if (boxController.MoveTrigger(position)) {
            return true;
        }
        
        return false;
    }

    public int[,] GetBoard() {
        return GameController.GetBoardMatrix();
    }

    public int InCheck() {
        return pieceController.GetInCheck();
    }

    public int getCurrPlayer() {
        if (GameController.WhiteTurn) {
            return 1;
        }

        return 0;
    }

    public void getEndState(bool whitePlayer, int type) {
        if (whitePlayer && type == 1) {
            reward += 1f;
        }
        else if (!whitePlayer && type == 1) {
            reward -= 1f;
        }
        finished = true;
    }

    public bool isPlayerPiece(int index, int currPlayer) {
        Vector3 matrixPos = indexToMatrix(index);
        Vector3 pos = boxController.MatrixToCoord(matrixPos);

        GameObject piece = pieceController.GetPieceOnPosition(pos.x, pos.y);

        if (piece == null) {
            return false;
        }

        if ((currPlayer == 1 && piece.tag == "White") || (currPlayer == 0 && piece.tag == "Black")) {
            return true;
        }

        return false;
    }

    public bool isValidPieceMove(int index, int pieceIndex) {
        GameObject enemy;

        Vector3 matrixPiecePos = indexToMatrix(pieceIndex);
        Vector3 piecePos = boxController.MatrixToCoord(matrixPiecePos);

        Vector3 matrixSpacePos = indexToMatrix(index);
        Vector3 spacePos = boxController.MatrixToCoord(matrixSpacePos);

        GameObject piece = pieceController.GetPieceOnPosition(piecePos.x, piecePos.y);

        if (piece == null) {
            return false;
        }

        return piece.GetComponent<PieceController>().ValidateMovement(piecePos, spacePos, out enemy);
    }

    public Vector3 indexToMatrix(int index) {
        int x = index % 8;
        int y = index / 8;

        return new Vector3(x, y, 0);
    }
}
