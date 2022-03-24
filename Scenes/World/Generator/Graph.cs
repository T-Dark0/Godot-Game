using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Graph<N, E>
{
    private List<Node> _nodes;
    private List<Edge> _edges;

    public Graph()
    {
        _nodes = new List<Node>();
        _edges = new List<Edge>();
    }

    public NodeId AddNode(N data)
    {
        _nodes.Add(new Node(data));
        return new NodeId(_nodes.Count - 1);
    }

    public EdgeId AddEdge(NodeId from, NodeId to, E data)
    {
        //See the comment on Edge for an explanation of the concept at the basis of this
        var fromNode = GetNode(from);
        var toNode = GetNode(to);

        var edgeId = new EdgeId(_edges.Count);

        // to construct an edge, we need two pairs of two things: The index of a node, 
        // as well as the current head of the corresponding linked list, once per list.

        // outEdge is the head of the linked lists of edges outgoing from fromNode
        // to which we're going to add ourselves, at the start
        var outEdge = fromNode.OutNext;
        fromNode.OutNext = edgeId;

        // inEdge, on the other hand, is the head of the linked lists of edges ingoing into toNode
        var inEdge = toNode.InNext;
        toNode.InNext = edgeId;

        //add an edge: it links from and to, carrying linked list information for inEdge and outEdge
        var newEdge = new Edge(data, from, to, inEdge, outEdge);
        _edges.Add(newEdge);

        return edgeId;
    }

    public bool ContainsEdge(NodeId from, NodeId to)
    {
        var fromNode = GetNode(from);
        return
            EdgesOfOutgoing(from).Any(pair => GetEdge(pair.id).OutNode == to)
            || EdgesOfIngoing(from).Any(pair => GetEdge(pair.id).InNode == to);
    }

    public IEnumerable<(EdgeId id, E data)> EdgesOfOutgoing(NodeId node)
    {
        var nodeNode = GetNode(node);
        var currentEdgeIdOpt = nodeNode.OutNext;
        while (currentEdgeIdOpt is EdgeId currentEdgeId)
        {
            var currentEdge = GetEdge(currentEdgeId);
            yield return (currentEdgeId, currentEdge.Data);
            currentEdgeIdOpt = currentEdge.OutNext;
        }
    }

    public IEnumerable<(EdgeId id, E data)> EdgesOfIngoing(NodeId node)
    {
        var nodeNode = GetNode(node);
        var currentEdgeIdOpt = nodeNode.InNext;
        while (currentEdgeIdOpt is EdgeId currentEdgeId)
        {
            var currentEdge = GetEdge(currentEdgeId);
            yield return (currentEdgeId, currentEdge.Data);
            currentEdgeIdOpt = currentEdge.InNext;
        }
    }

    public IEnumerable<(EdgeId id, E data)> EdgesOf(NodeId node)
    {
        return EdgesOfOutgoing(node).Concat(EdgesOfIngoing(node));
    }

    public IEnumerable<(NodeId id, N data)> Neighbors(NodeId node)
    {
        var outgoing = EdgesOfOutgoing(node)
            .Select(pair =>
            {
                var edge = GetEdge(pair.id);
                var nodeId = edge.OutNode;
                return (nodeId, GetNode(nodeId).Data);
            });
        var ingoing = EdgesOfIngoing(node)
            .Select(pair =>
            {
                var edge = GetEdge(pair.id);
                var nodeId = edge.InNode;
                return (nodeId, GetNode(nodeId).Data);
            });
        return outgoing.Concat(ingoing);
    }

    public IEnumerable<(NodeId id, N data)> Nodes()
    {
        return _nodes.Select((node, id) => (new NodeId(id), node.Data));
    }

    public IEnumerable<(EdgeId id, E data)> Edges()
    {
        return _edges.Select((edge, id) => (new EdgeId(id), edge.Data));
    }

    public override string ToString()
    {
        var builder = new StringBuilder("nodes: ");
        foreach (var node in _nodes)
        {
            builder.AppendFormat("{0}, ", node);
        }
        builder.Append("\nedges: ");
        foreach (var edge in _edges)
        {
            var from = GetNode(edge.InNode);
            var to = GetNode(edge.OutNode);
            builder.AppendFormat("{0}->{1}, ", from.Data, to.Data);
        }
        return builder.ToString();
    }

    private Node GetNode(NodeId id)
    {
        return _nodes[id.Id];
    }

    private Edge GetEdge(EdgeId id)
    {
        return _edges[id.Id];
    }

    private class Node
    {
        public N Data;
        //first incoming edge: if this is node A, that would be an edge like XA
        public EdgeId? InNext;
        //first outgoing edge. To continue the example, AX
        public EdgeId? OutNext;

        public Node(N data)
        {
            Data = data;
            InNext = null;
            OutNext = null;
        }

        public override string ToString()
        {
            return $"({Data}, {InNext?.Id.ToString() ?? "null"}, {OutNext?.Id.ToString() ?? "null"})";
        }
    }

    // As you read above, a Node only stores at most two edges: one incoming and one outgoing.
    // Any other edges are stored as an intertwined pair of intrusive indices-instead-of-pointers singly linked lists
    // For a given Edge, InNext and OutNext are the indices of the next edge in the corresponding list (if one exists)
    //
    // InNode and OutNode don't manage anything that complex, they're just the indices of the nodes connected by a given
    // Edge
    private class Edge
    {
        public E Data;
        public NodeId InNode;
        public NodeId OutNode;
        public EdgeId? InNext;
        public EdgeId? OutNext;

        public Edge(E data, NodeId inNode, NodeId outNode, EdgeId? inNext, EdgeId? outNext)
        {
            Data = data;
            InNode = inNode;
            OutNode = outNode;
            InNext = inNext;
            OutNext = outNext;
        }
        public override string ToString()
        {
            return $"({Data}, {InNode.Id}, {OutNode.Id}, {InNext?.Id}, {OutNext?.Id})";
        }
    }
}

public struct NodeId : IComparable<NodeId>
{
    public int Id;

    public NodeId(int id) => Id = id;

    public static bool operator ==(NodeId lhs, NodeId rhs) => lhs.Id == rhs.Id;
    public static bool operator !=(NodeId lhs, NodeId rhs) => lhs.Id != rhs.Id;
    public static bool operator <(NodeId lhs, NodeId rhs) => lhs.Id < rhs.Id;
    public static bool operator >(NodeId lhs, NodeId rhs) => lhs.Id > rhs.Id;
    public override bool Equals(object obj) => obj is NodeId rhs && Id == rhs.Id;
    public override int GetHashCode() => Id.GetHashCode();
    public int CompareTo(NodeId other) => this.Id.CompareTo(other.Id);
}

public struct EdgeId : IComparable<EdgeId>
{
    public int Id;

    public EdgeId(int id) => Id = id;

    public static bool operator ==(EdgeId lhs, EdgeId rhs) => lhs.Id == rhs.Id;
    public static bool operator !=(EdgeId lhs, EdgeId rhs) => lhs.Id != rhs.Id;
    public static bool operator <(EdgeId lhs, EdgeId rhs) => lhs.Id < rhs.Id;
    public static bool operator >(EdgeId lhs, EdgeId rhs) => lhs.Id > rhs.Id;
    public override bool Equals(object obj) => obj is NodeId rhs && Id == rhs.Id;
    public override int GetHashCode() => Id.GetHashCode();
    public int CompareTo(EdgeId other) => this.Id.CompareTo(other.Id);
}