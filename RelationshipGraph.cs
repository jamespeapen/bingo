using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bingo
{
    /// <summary>
    /// Represents a directed labeled graph with a string name at each node
    /// and a string Label for each edge.
    /// </summary>
    class RelationshipGraph
    {
        /*
         *  This data structure contains a list of nodes (each of which has
         *  an adjacency list) and a dictionary (hash table) for efficiently 
         *  finding nodes by name
         */
        public List<GraphNode> nodes { get; private set; }
        private Dictionary<String, GraphNode> nodeDict;

        // constructor builds empty relationship graph
        public RelationshipGraph()
        {
            nodes = new List<GraphNode>();
            nodeDict = new Dictionary<String,GraphNode>();
        }

        // AddNode creates and adds a new node if there isn't already one by that name
        public void AddNode(string name)
        {
            if (!nodeDict.ContainsKey(name))
            {
                GraphNode n = new GraphNode(name);
                nodes.Add(n);
                nodeDict.Add(name, n);
            }
        }

        // AddEdge adds the edge, creating endpoint nodes if necessary.
        // Edge is added to adjacency list of from edges.
        public void AddEdge(string name1, string name2, string relationship) 
        {
            AddNode(name1);                     // create the node if it doesn't already exist
            GraphNode n1 = nodeDict[name1];     // now fetch a reference to the node
            AddNode(name2);
            GraphNode n2 = nodeDict[name2];
            GraphEdge e = new GraphEdge(n1, n2, relationship);
            n1.AddIncidentEdge(e);
        }

        // Get a node by name using dictionary
        public GraphNode GetNode(string name)
        {
            if (nodeDict.ContainsKey(name))
                return nodeDict[name];
            else
                return null;
        }

        // Get parent node
        public List<GraphNode> GetParentNodes(string name)
        {
            GraphNode child = GetNode(name);
            List<GraphEdge> parentEdges = child.GetEdges("hasParent");

            List<GraphNode> parentNodes = new List<GraphNode>();

            foreach (GraphEdge parentEdge in parentEdges)
            {
                parentNodes.Add(GetNode(parentEdge.To()));
            } 
            return parentNodes;
        }
        // Get Child Nodes
        public List<GraphNode> GetChildNodes(string name)
        {
            GraphNode n = GetNode(name);
            List<GraphEdge> childEdges = n.GetEdges("hasChild");

            List<GraphNode> childNodes = new List<GraphNode>();

            foreach (GraphEdge childEdge in childEdges)
            {
                childNodes.Add(GetNode(childEdge.To()));
            }

            return childNodes;
        }

        // Return a text representation of graph
        public void Dump()
        {
            foreach (GraphNode n in nodes)
            {
                Console.Write(n.ToString());
            }
        }
    }
}
