using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bingo
{
    /// <summary>
    /// Represents a node in a RelationshipGraph
    /// </summary>
    class GraphNode
    {
        public string Name { get; private set; }

        public string Label { get; set; }

        public List<GraphEdge> incidentEdges { get; private set; }

        // constructor
        public GraphNode(string v)
        {
            Name = v;
            Label = "Unvisited";
            incidentEdges = new List<GraphEdge>();
        }

        // Add an edge (but don't add duplicate edges)
        public void AddIncidentEdge(GraphEdge e)
        {
            foreach (GraphEdge edge in incidentEdges)
            {
                if (edge.ToString() == e.ToString())
                    return;
            }
            incidentEdges.Add(e);
        }

        // return a list of all outgoing edges
        public List<GraphEdge> GetEdges()
        {
            return incidentEdges;
        }

        // return a list of outgoing edges of specified Label
        public List<GraphEdge> GetEdges(string label)
        {
            List<GraphEdge> list = new List<GraphEdge>();
            foreach (GraphEdge e in incidentEdges)
                if (e.Label == label)
                    list.Add(e);
            return list;
        }

        //return a GraphEdge for the relationship between two people
        public GraphEdge GetRelationship(string person)
        {
            foreach (GraphEdge edge in incidentEdges)
            {
                if (edge.To() == person)
                    return edge;
            }
            return null;
        }

        // return text form of node, including outgoing edges
        public override string ToString()
        {
            string result = Name + "\n";
            result += "  label: " + Label + "\n";
            foreach (GraphEdge e in incidentEdges)
            {
                result = result + "  " + e.ToString() + "\n";
            }
            return result;
        }
    }
}
