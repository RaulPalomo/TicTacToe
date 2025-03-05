using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Node
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Team { get; set; }
    public int[,] MatrixNode { get; set; }
    public int Valuated { get; set; }
    public int Evaluated { get; set; }
    public Stack<Node> NodeChildren { get; set; }
    public Node(int[,] matrix, int team, int x, int y)
    {
        MatrixNode = matrix;
        Team = team;
        X = x;
        Y = y;
        NodeChildren = new Stack<Node>();
    }
}