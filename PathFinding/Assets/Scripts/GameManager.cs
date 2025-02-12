using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int Size;
    public BoxCollider2D Panel;
    public GameObject token;
    public GameObject startToken;
    public GameObject endToken;
    private Node[,] NodeMatrix;
    private int startPosx, startPosy;
    private int endPosx, endPosy;
    public TextMeshProUGUI textMesh;

    void Awake()
    {
        Instance = this;
        //GameMatrix = new int[Size, Size];
        Calculs.CalculateDistances(Panel, Size);
    }
    private void Start()
    {   
        /*for(int i = 0; i<Size; i++)
        {
            for (int j = 0; j< Size; j++)
            {
                GameMatrix[i, j] = 0;
            }
        }*/
        
        startPosx = Random.Range(0, Size);
        startPosy = Random.Range(0, Size);
        do
        {
            endPosx = Random.Range(0, Size);
            endPosy = Random.Range(0, Size);
        } while(endPosx== startPosx || endPosy== startPosy);

        //GameMatrix[startPosx, startPosy] = 2;
        //GameMatrix[startPosx, startPosy] = 1;
        NodeMatrix = new Node[Size, Size];
        CreateNodes();

        Vector2 endPosition = Calculs.CalculatePoint(endPosx, endPosy);
        Debug.Log("End position: " + endPosition);
        Vector2 startPosition = Calculs.CalculatePoint(startPosx, startPosy);
        Debug.Log("Start position: " + startPosition);
        Instantiate(startToken, startPosition, Quaternion.identity);
        Instantiate(endToken, endPosition, Quaternion.identity);
        PathfindingAlgorithm();
    }
    public void CreateNodes()
    {
        for(int i=0; i<Size; i++)
        {
            for(int j=0; j<Size; j++)
            {
                NodeMatrix[i, j] = new Node(i, j, Calculs.CalculatePoint(i,j));
                NodeMatrix[i,j].Heuristic = Calculs.CalculateHeuristic(NodeMatrix[i,j],endPosx,endPosy);
            }
        }
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                SetWays(NodeMatrix[i, j], i, j);
            }
        }
        DebugMatrix();
    }
    public void DebugMatrix()
    {
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                if (NodeMatrix[i,j] != NodeMatrix[startPosx, startPosy] && NodeMatrix[i, j] != NodeMatrix[endPosx, endPosy]) Instantiate(token, NodeMatrix[i, j].RealPosition, Quaternion.identity);
                Debug.Log("Element (" + j + ", " + i + ")");
                Debug.Log("Position " + NodeMatrix[i, j].RealPosition);
                Debug.Log("Heuristic " + NodeMatrix[i, j].Heuristic);
                Debug.Log("Ways: ");
                foreach (var way in NodeMatrix[i, j].WayList)
                {
                    Debug.Log(" (" + way.NodeDestiny.PositionX + ", " + way.NodeDestiny.PositionY + ")");
                    
                }
            }
        }
    }
    public void SetWays(Node node, int x, int y)
    {
        node.WayList = new List<Way>();
        if (x>0)
        {
            node.WayList.Add(new Way(NodeMatrix[x - 1, y], Calculs.LinearDistance));
            if (y > 0)
            {
                node.WayList.Add(new Way(NodeMatrix[x - 1, y - 1], Calculs.DiagonalDistance));
            }
        }
        if(x<Size-1)
        {
            node.WayList.Add(new Way(NodeMatrix[x + 1, y], Calculs.LinearDistance));
            if (y > 0)
            {
                node.WayList.Add(new Way(NodeMatrix[x + 1, y - 1], Calculs.DiagonalDistance));
            }
        }
        if(y>0)
        {
            node.WayList.Add(new Way(NodeMatrix[x, y - 1], Calculs.LinearDistance));
        }
        if (y<Size-1)
        {
            node.WayList.Add(new Way(NodeMatrix[x, y + 1], Calculs.LinearDistance));
            if (x>0)
            {
                node.WayList.Add(new Way(NodeMatrix[x - 1, y + 1], Calculs.DiagonalDistance));
            }
            if (x<Size-1)
            {
                node.WayList.Add(new Way(NodeMatrix[x + 1, y + 1], Calculs.DiagonalDistance));
            }
        }

    }
    public Node GetNode(int x, int y)
    {
        return NodeMatrix[x, y];
    }
    public Node GetStartNode()
    {
        return NodeMatrix[startPosx, startPosy];
    }
    public Node GetEndNode()
    {
        return NodeMatrix[endPosx, endPosy];
    }
    public void PathfindingAlgorithm()
    {
        List<Node> openList = new List<Node>();
        List<Node> closedList = new List<Node>();

        Node startNode = GetStartNode();
        Node endNode = GetEndNode();

        openList.Add(startNode);

        while (openList.Count > 0)
        {
            Node currentNode = openList.OrderBy(node => node.Heuristic).First();

            // Si hemos llegado al nodo final, terminamos el ciclo
            if (currentNode == endNode)
            {
                HighlightPath(currentNode);
                break;
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            // Procesamos los vecinos del nodo actual
            foreach (var way in currentNode.WayList)
            {
                Node neighbor = way.NodeDestiny;

                // Si el vecino está en la lista cerrada, lo ignoramos
                if (closedList.Contains(neighbor))
                    continue;

                // Calculamos el valor de g para el vecino
                float tentativeG = currentNode.Heuristic + way.Cost;

                if (!openList.Contains(neighbor))
                {
                    openList.Add(neighbor);
                }
                else if (tentativeG >= neighbor.Heuristic)
                {
                    continue;
                }

                neighbor.Heuristic = tentativeG;
                neighbor.NodeParent = currentNode;
            }

            UpdateTextMesh(openList, closedList);
        }
    }

    private void HighlightPath(Node currentNode)
    {
        while (currentNode != null)
        {
            GameObject nodeObj = Instantiate(token, currentNode.RealPosition, Quaternion.identity);
            nodeObj.GetComponent<SpriteRenderer>().color = Color.yellow;

            currentNode = currentNode.NodeParent;
        }
    }

    private void UpdateTextMesh(List<Node> openList, List<Node> closedList)
    {
        string openListText = "LOOK IN EDITOR!\n Open List:\n";
        string closedListText = "Closed List:\n";

        foreach (Node node in openList)
        {
            float g = node.Heuristic; 
            float h = Calculs.CalculateHeuristic(node, endPosx, endPosy); 
            float f = g + h; 

            openListText += $"({node.PositionX}, {node.PositionY}) g: {g:F2}, h: {h:F2}, f: {f:F2}\n";
        }

        foreach (Node node in closedList)
        {
            float g = node.Heuristic;
            float h = Calculs.CalculateHeuristic(node, endPosx, endPosy);
            float f = g + h;

            closedListText += $"({node.PositionX}, {node.PositionY}) g: {g:F2}, h: {h:F2}, f: {f:F2}\n";
        }

        textMesh.text = openListText + "\n\n" + closedListText;
    }


}
