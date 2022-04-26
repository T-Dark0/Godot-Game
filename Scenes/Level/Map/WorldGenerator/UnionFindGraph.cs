using System.Collections.Generic;
using System.Text;

namespace WorldGenerator;

/// <summary>
/// A variant of a Union-Find data structure, as described on
/// https://en.wikipedia.org/wiki/Disjoint-set_data_structure
/// </summary>
public class UnionFindGraph<N, E>
{
    private Graph<N, E> _graph;
    private List<PointMeta> _metadata = new List<PointMeta>();

    public int NodeCount { get { return _graph.NodeCount; } }
    public int EdgeCount { get { return _graph.EdgeCount; } }

    public UnionFindGraph(Graph<N, E> graph)
    {
        _graph = graph;
        _metadata = new List<PointMeta>();
    }

    public NodeId AddNode(N data)
    {
        var id = _graph.AddNode(data);
        _metadata.Add(PointMeta.NewRank(1));
        return id;
    }

    public EdgeId AddEdge(NodeId from, NodeId to, E data)
    {
        var id = _graph.AddEdge(from, to, data);
        UnionPoints(from, to);
        return id;
    }

    public bool WouldFormCycle(NodeId node1, NodeId node2)
    {
        return FindRoot(node1) == FindRoot(node2);
    }

    public IEnumerable<(NodeId id, N data)> Nodes()
    {
        return _graph.Nodes();
    }

    public IEnumerable<(EdgeId id, E data)> Edges()
    {
        return _graph.Edges();
    }

    public override string ToString()
    {
        var builder = new StringBuilder(_graph.ToString());
        var metas = _metadata.GetEnumerator();
        builder.Append("\ngroups:");
        if (metas.MoveNext())
        {
            builder.AppendFormat(" {0}", metas.Current);
        }
        while (metas.MoveNext())
        {
            builder.AppendFormat(", {0}", metas.Current);
        }
        return builder.ToString();
    }

    private NodeId FindRoot(NodeId point)
    {
        var meta = GetMeta(point);
        if (meta.Parent is NodeId parent)
        {
            var root = FindRoot(parent);
            SetParent(point, root);
            return root;
        }
        else
        {
            return point;
        }
    }

    private void UnionPoints(NodeId p1, NodeId p2)
    {
        var root1 = FindRoot(p1);
        var root2 = FindRoot(p2);
        if (root1 == root2) return;

        var rank1 = GetMeta(root1).Rank!.Value;
        var rank2 = GetMeta(root2).Rank!.Value;
        switch (rank1.CompareTo(rank2))
        {
            case -1:
                SetParent(root1, root2);
                break;
            case 1:
                SetParent(root2, root1);
                break;
            case 0:
                SetRank(root1, rank1 + 1);
                SetParent(root2, root1);
                break;
        }
    }

    private PointMeta GetMeta(NodeId point)
    {
        return _metadata[point.Id];
    }
    private void SetParent(NodeId point, NodeId parent)
    {
        _metadata[point.Id] = PointMeta.NewParent(parent);
    }
    private void SetRank(NodeId point, uint rank)
    {
        _metadata[point.Id] = PointMeta.NewRank(rank);
    }

    private struct PointMeta
    {
        private readonly int _meta;

        public NodeId? Parent
        {
            get
            {
                if (_meta >= 0) return new NodeId(_meta);
                else return null;
            }
        }
        public uint? Rank
        {
            get
            {
                if (_meta < 0) return (uint)-_meta;
                else return null;

            }
        }

        public override string ToString()
        {
            return _meta >= 0 ? $"P{new NodeId(_meta)}" : $"R{-_meta}";
        }

        private PointMeta(NodeId id)
        {
            _meta = id.Id;
        }
        private PointMeta(uint rank)
        {
            _meta = -(int)rank;
        }
        public static PointMeta NewParent(NodeId id)
        {
            return new PointMeta(id);
        }
        public static PointMeta NewRank(uint rank)
        {
            return new PointMeta(rank);
        }
    }
}