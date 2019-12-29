// Decompiled with JetBrains decompiler
// Type: SECTR_Graph
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

public static class SECTR_Graph
{
  private static List<SECTR_Sector> initialSectors = new List<SECTR_Sector>(4);
  private static List<SECTR_Sector> goalSectors = new List<SECTR_Sector>(4);
  private static SECTR_PriorityQueue<SECTR_Graph.Node> openSet = new SECTR_PriorityQueue<SECTR_Graph.Node>(64);
  private static Dictionary<SECTR_Portal, SECTR_Graph.Node> closedSet = new Dictionary<SECTR_Portal, SECTR_Graph.Node>(64);

  public static void DepthWalk(
    ref List<SECTR_Graph.Node> nodes,
    SECTR_Sector root,
    SECTR_Portal.PortalFlags stopFlags,
    int maxDepth)
  {
    nodes.Clear();
    if ((UnityEngine.Object) root == (UnityEngine.Object) null)
      return;
    if (maxDepth == 0)
    {
      nodes.Add(new SECTR_Graph.Node() { Sector = root });
    }
    else
    {
      int count1 = SECTR_Sector.All.Count;
      for (int index = 0; index < count1; ++index)
        SECTR_Sector.All[index].Visited = false;
      Stack<SECTR_Graph.Node> nodeStack = new Stack<SECTR_Graph.Node>(count1);
      nodeStack.Push(new SECTR_Graph.Node()
      {
        Sector = root,
        Depth = 1
      });
      root.Visited = true;
      int num = 0;
      while (nodeStack.Count > 0)
      {
        SECTR_Graph.Node node = nodeStack.Pop();
        nodes.Add(node);
        ++num;
        if (maxDepth < 0 || node.Depth <= maxDepth)
        {
          int count2 = node.Sector.Portals.Count;
          for (int index = 0; index < count2; ++index)
          {
            SECTR_Portal portal = node.Sector.Portals[index];
            if ((bool) (UnityEngine.Object) portal && (portal.Flags & stopFlags) == (SECTR_Portal.PortalFlags) 0)
            {
              SECTR_Sector sectrSector = (UnityEngine.Object) portal.FrontSector == (UnityEngine.Object) node.Sector ? portal.BackSector : portal.FrontSector;
              if ((bool) (UnityEngine.Object) sectrSector && !sectrSector.Visited)
              {
                nodeStack.Push(new SECTR_Graph.Node()
                {
                  Parent = node,
                  Sector = sectrSector,
                  Portal = portal,
                  Depth = node.Depth + 1
                });
                sectrSector.Visited = true;
              }
            }
          }
        }
      }
    }
  }

  public static void BreadthWalk(
    ref List<SECTR_Graph.Node> nodes,
    SECTR_Sector root,
    SECTR_Portal.PortalFlags stopFlags,
    int maxDepth)
  {
    nodes.Clear();
    if ((UnityEngine.Object) root == (UnityEngine.Object) null)
      return;
    if (maxDepth == 0)
    {
      nodes.Add(new SECTR_Graph.Node() { Sector = root });
    }
    else
    {
      int count1 = SECTR_Sector.All.Count;
      for (int index = 0; index < count1; ++index)
        SECTR_Sector.All[index].Visited = false;
      Queue<SECTR_Graph.Node> nodeQueue = new Queue<SECTR_Graph.Node>(count1);
      nodeQueue.Enqueue(new SECTR_Graph.Node()
      {
        Sector = root,
        Depth = 0
      });
      root.Visited = true;
      int num = 0;
      while (nodeQueue.Count > 0)
      {
        SECTR_Graph.Node node = nodeQueue.Dequeue();
        nodes.Add(node);
        ++num;
        if (maxDepth < 0 || node.Depth < maxDepth)
        {
          int count2 = node.Sector.Portals.Count;
          for (int index = 0; index < count2; ++index)
          {
            SECTR_Portal portal = node.Sector.Portals[index];
            if ((bool) (UnityEngine.Object) portal && (portal.Flags & stopFlags) == (SECTR_Portal.PortalFlags) 0)
            {
              SECTR_Sector sectrSector = (UnityEngine.Object) portal.FrontSector == (UnityEngine.Object) node.Sector ? portal.BackSector : portal.FrontSector;
              if ((bool) (UnityEngine.Object) sectrSector && !sectrSector.Visited)
              {
                nodeQueue.Enqueue(new SECTR_Graph.Node()
                {
                  Parent = node,
                  Sector = sectrSector,
                  Portal = portal,
                  Depth = node.Depth + 1
                });
                node.Sector.Visited = true;
              }
            }
          }
        }
      }
    }
  }

  public static void FindShortestPath(
    ref List<SECTR_Graph.Node> path,
    Vector3 start,
    Vector3 goal,
    SECTR_Portal.PortalFlags stopFlags)
  {
    path.Clear();
    SECTR_Graph.openSet.Clear();
    SECTR_Graph.closedSet.Clear();
    SECTR_Sector.GetContaining(ref SECTR_Graph.initialSectors, start);
    SECTR_Sector.GetContaining(ref SECTR_Graph.goalSectors, goal);
    int count1 = SECTR_Graph.initialSectors.Count;
    for (int index1 = 0; index1 < count1; ++index1)
    {
      SECTR_Sector initialSector = SECTR_Graph.initialSectors[index1];
      if (SECTR_Graph.goalSectors.Contains(initialSector))
      {
        path.Add(new SECTR_Graph.Node()
        {
          Sector = initialSector
        });
        return;
      }
      int count2 = initialSector.Portals.Count;
      for (int index2 = 0; index2 < count2; ++index2)
      {
        SECTR_Portal portal = initialSector.Portals[index2];
        if ((portal.Flags & stopFlags) == (SECTR_Portal.PortalFlags) 0)
        {
          SECTR_Graph.Node node = new SECTR_Graph.Node();
          node.Portal = portal;
          node.Sector = initialSector;
          node.ForwardTraversal = (UnityEngine.Object) initialSector == (UnityEngine.Object) portal.FrontSector;
          node.Cost = Vector3.Magnitude(start - portal.transform.position);
          float num = Vector3.Magnitude(goal - portal.transform.position);
          node.CostPlusEstimate = node.Cost + num;
          SECTR_Graph.openSet.Enqueue(node);
        }
      }
    }
    while (SECTR_Graph.openSet.Count > 0)
    {
      SECTR_Graph.Node currentNode = SECTR_Graph.openSet.Dequeue();
      SECTR_Sector sectrSector = currentNode.ForwardTraversal ? currentNode.Portal.BackSector : currentNode.Portal.FrontSector;
      if ((bool) (UnityEngine.Object) sectrSector)
      {
        if (SECTR_Graph.goalSectors.Contains(sectrSector))
        {
          SECTR_Graph.Node.ReconstructPath(path, currentNode);
          break;
        }
        int count2 = sectrSector.Portals.Count;
        for (int index1 = 0; index1 < count2; ++index1)
        {
          SECTR_Portal portal = sectrSector.Portals[index1];
          if ((UnityEngine.Object) portal != (UnityEngine.Object) currentNode.Portal && (portal.Flags & stopFlags) == (SECTR_Portal.PortalFlags) 0)
          {
            SECTR_Graph.Node node1 = new SECTR_Graph.Node()
            {
              Parent = currentNode,
              Portal = portal,
              Sector = sectrSector,
              ForwardTraversal = (UnityEngine.Object) sectrSector == (UnityEngine.Object) portal.FrontSector
            };
            node1.Cost = currentNode.Cost + Vector3.Magnitude(node1.Portal.transform.position - currentNode.Portal.transform.position);
            float num = Vector3.Magnitude(goal - node1.Portal.transform.position);
            node1.CostPlusEstimate = node1.Cost + num;
            SECTR_Graph.Node node2 = (SECTR_Graph.Node) null;
            SECTR_Graph.closedSet.TryGetValue(node1.Portal, out node2);
            if (node2 == null || (double) node2.CostPlusEstimate >= (double) node1.CostPlusEstimate)
            {
              SECTR_Graph.Node node3 = (SECTR_Graph.Node) null;
              for (int index2 = 0; index2 < SECTR_Graph.openSet.Count; ++index2)
              {
                if ((UnityEngine.Object) SECTR_Graph.openSet[index2].Portal == (UnityEngine.Object) node1.Portal)
                {
                  node3 = SECTR_Graph.openSet[index2];
                  break;
                }
              }
              if (node3 == null || (double) node3.CostPlusEstimate >= (double) node1.CostPlusEstimate)
                SECTR_Graph.openSet.Enqueue(node1);
            }
          }
        }
        if (!SECTR_Graph.closedSet.ContainsKey(currentNode.Portal))
          SECTR_Graph.closedSet.Add(currentNode.Portal, currentNode);
      }
    }
  }

  public static string GetGraphAsDot(string graphName)
  {
    string str = "graph " + graphName + " {\n" + "\tlayout=neato\n";
    foreach (SECTR_Portal sectrPortal in SECTR_Portal.All)
    {
      str += "\t";
      str += (string) (object) sectrPortal.GetInstanceID();
      str += " [";
      str = str + "label=" + sectrPortal.name;
      str += ",shape=hexagon";
      str += "];\n";
    }
    foreach (SECTR_Sector sectrSector in SECTR_Sector.All)
    {
      str += "\t";
      str += (string) (object) sectrSector.GetInstanceID();
      str += " [";
      str = str + "label=" + sectrSector.name;
      str += ",shape=box";
      str += "];\n";
    }
    foreach (SECTR_Portal sectrPortal in SECTR_Portal.All)
    {
      if ((bool) (UnityEngine.Object) sectrPortal.FrontSector)
      {
        str += "\t";
        str = str + (object) sectrPortal.GetInstanceID() + " -- " + (object) sectrPortal.FrontSector.GetInstanceID();
        str += ";\n";
      }
      if ((bool) (UnityEngine.Object) sectrPortal.BackSector)
      {
        str += "\t";
        str = str + (object) sectrPortal.GetInstanceID() + " -- " + (object) sectrPortal.BackSector.GetInstanceID();
        str += ";\n";
      }
    }
    return str + "\n}";
  }

  public class Node : IComparable<SECTR_Graph.Node>
  {
    public SECTR_Portal Portal;
    public SECTR_Sector Sector;
    public float CostPlusEstimate;
    public float Cost;
    public int Depth;
    public bool ForwardTraversal;
    public SECTR_Graph.Node Parent;

    public int CompareTo(SECTR_Graph.Node other)
    {
      if ((double) this.CostPlusEstimate > (double) other.CostPlusEstimate)
        return 1;
      return (double) this.CostPlusEstimate < (double) other.CostPlusEstimate ? -1 : 0;
    }

    public static void ReconstructPath(List<SECTR_Graph.Node> path, SECTR_Graph.Node currentNode)
    {
      if (currentNode == null)
        return;
      path.Insert(0, currentNode);
      SECTR_Graph.Node.ReconstructPath(path, currentNode.Parent);
    }
  }
}
