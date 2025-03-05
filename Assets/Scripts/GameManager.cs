using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditorInternal;
using UnityEngine;
public enum States
{
    CanMove,
    CantMove
}
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public BoxCollider2D collider;
    public GameObject token1, token2;
    public int Size = 3;
    public int[,] Matrix;
    public int lastMoveX, lastMoveY;
    [SerializeField] private States state = States.CanMove;
    public Camera camera;
    void Start()
    {
        Instance = this;
        Matrix = new int[Size, Size];
        Calculs.CalculateDistances(collider, Size);
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                Matrix[i, j] = 0; // 0: desocupat, 1: fitxa jugador 1, -1: fitxa IA;
            }
        }
    }
    private void Update()
    {
        if (state == States.CanMove)
        {
            Vector3 m = Input.mousePosition;
            m.z = 10f;
            Vector3 mousepos = camera.ScreenToWorldPoint(m);
            if (Input.GetMouseButtonDown(0))
            {
                if (Calculs.CheckIfValidClick((Vector2)mousepos, Matrix))
                {
                    state = States.CantMove;
                    if (Calculs.EvaluateWin(Matrix) == 2)
                        StartCoroutine(WaitingABit());
                }
            }
        }
    }
    private IEnumerator WaitingABit()
    {
        yield return new WaitForSeconds(1f);
        RandomAI();
        MinMaxAlg();
    }
    public void MinMaxAlg()
    {
        int bestScore = int.MinValue;
        int moveX = -1, moveY = -1;
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                if (Matrix[i, j] == 0)
                {
                    Matrix[i, j] = -1;
                    int score = MinMax(Matrix, 0, false);
                    Matrix[i, j] = 0;
                    if (score > bestScore)
                    {
                        bestScore = score;
                        moveX = i;
                        moveY = j;
                    }
                }
            }
        }
        if (moveX != -1 && moveY != -1)
        {
            DoMove(moveX, moveY, -1, ref Matrix);
        }
    }
    public int MinMax(int[,] board, int depth, bool isMax)
    {
        int result = Calculs.EvaluateWin(board);
        if (result == -1) return 10 - depth;
        else if (result == 1) return depth - 10;
        else if (result == 0) return 0;

        if (isMax)
        {
            int bestScore = int.MinValue;
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (board[i, j] == 0)
                    {
                        board[i, j] = -1;
                        int score = MinMax(board, depth + 1, false);
                        board[i, j] = 0;
                        bestScore = Mathf.Max(score, bestScore);
                    }
                }
            }
            return bestScore;
        }
        else
        {
            int bestScore = int.MaxValue;
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (board[i, j] == 0)
                    {
                        board[i, j] = 1;
                        int score = MinMax(board, depth + 1, true);
                        board[i, j] = 0;
                        bestScore = Mathf.Min(score, bestScore);
                    }
                }
            }
            return bestScore;
        }
    }
    public void RandomAI()
    {
        Node initialNode = new Node(Matrix, -1, lastMoveX, lastMoveY);
        MatrixGenerator(Matrix.Length, Matrix.Length, initialNode);
        state = States.CanMove;
    }
    public void MatrixGenerator(int x, int y, Node node)
    {
        for (int i = 0; i < node.MatrixNode.GetLength(0); i++)
        {
            for (int j = 0; j < node.MatrixNode.GetLength(1); j++)
            {
                if (node.MatrixNode[i, j] == 0)
                {
                    int[,] clonedMatrix = MatrixCloner(node.MatrixNode);
                    clonedMatrix[i, j] = -1;
                    Node child = new Node(clonedMatrix, -1, i, j);
                    node.NodeChildren.Push(child);
                    int result = Calculs.EvaluateWin(clonedMatrix);
                    if (result == 2)
                    {
                        MatrixGenerator(node.MatrixNode.GetLength(0), node.MatrixNode.GetLength(1), child);
                    }
                    else
                    {
                        child.Valuated = result;
                    }
                }
            }
        }
    }
    public int[,] MatrixCloner(int[,] matrix)
    {
        int[,] clone = new int[matrix.GetLength(0), matrix.GetLength(1)];
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                clone[i, j] = matrix[i, j];
            }
        }
        return clone;
    }
    public void DoMove(int x, int y, int team, ref int[,] matrix)
    {
        matrix[x, y] = team;
        if (team == 1)
            Instantiate(token1, Calculs.CalculatePoint(x, y), Quaternion.identity);
        else
            Instantiate(token2, Calculs.CalculatePoint(x, y), Quaternion.identity);
        int result = Calculs.EvaluateWin(Matrix);
        switch (result)
        {
            case 0:
                Debug.Log("Draw");
                break;
            case 1:
                Debug.Log("You Win");
                break;
            case -1:
                Debug.Log("You Lose");
                break;
            case 2:
                if (state == States.CantMove)
                    state = States.CanMove;
                break;
        }
    }
}