using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace Bingo
{
    class Program
    {
        private static RelationshipGraph rg;

        // Read RelationshipGraph whose filename is passed in as a parameter.
        // Build a RelationshipGraph in RelationshipGraph rg
        private static void ReadRelationshipGraph(string filename)
        {
            rg = new RelationshipGraph();                           // create a new RelationshipGraph object

            string name = "";                                       // name of person currently being read
            int numPeople = 0;
            string[] values;
            Console.Write("Reading file " + filename + "\n");
            try
            {
                string input = System.IO.File.ReadAllText(filename);// read file
                input = input.Replace("\r", ";");                   // get rid of nasty carriage returns 
                input = input.Replace("\n", ";");                   // get rid of nasty new lines
                string[] inputItems = Regex.Split(input, @";\s*");  // parse out the relationships (separated by ;)
                foreach (string item in inputItems)
                {
                    if (item.Length > 2)                            // don't bother with empty relationships
                    {
                        values = Regex.Split(item, @"\s*:\s*");     // parse out relationship:name
                        if (values[0] == "name")                    // name:[personname] indicates start of new person
                        {
                            name = values[1];                       // remember name for future relationships
                            rg.AddNode(name);                       // create the node
                            numPeople++;
                        }
                        else
                        {
                            rg.AddEdge(name, values[1], values[0]); // add relationship (name1, name2, relationship)

                            // handle symmetric relationships -- add the other way
                            if (values[0] == "hasSpouse" || values[0] == "hasFriend")
                                rg.AddEdge(values[1], name, values[0]);

                            // for parent relationships add child as well
                            else if (values[0] == "hasParent")
                                rg.AddEdge(values[1], name, "hasChild");
                            else if (values[0] == "hasChild")
                                rg.AddEdge(values[1], name, "hasParent");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.Write("Unable to read file {0}: {1}\n", filename, e.ToString());
            }
            Console.WriteLine(numPeople + " people read");
        }

        // Show the relationships a person is involved in
        private static void ShowPerson(string name)
        {
            GraphNode n = rg.GetNode(name);
            if (n != null)
                Console.Write(n.ToString());
            else
                Console.WriteLine("{0} not found", name);
        }

        // Show a person's friends
        private static void ShowFriends(string name)
        {
            GraphNode n = rg.GetNode(name);
            if (n != null)
            {
                Console.Write("{0}'s friends: ", name);
                List<GraphEdge> friendEdges = n.GetEdges("hasFriend");
                foreach (GraphEdge e in friendEdges)
                {
                    Console.Write("{0} ", e.To());
                }
                Console.WriteLine();
            }
            else
                Console.WriteLine("{0} not found", name);
        }

        //Display all orphans in the dataset by checking each nodes edges for parent relationship
        private static void ShowOrphans()
        {
            foreach (GraphNode n in rg.nodes)
            {
                if (n.GetEdges("hasParent").Count == 0)
                {
                    Console.Write(n.Name + " ");
                }
            }
        }

        //Get all descendants of a node
        private static Dictionary<int, ArrayList> GetDescendants(string name)
        {
            Queue<GraphNode> descendants = new Queue<GraphNode>();              // queue of successive child nodes for BFS
            List<GraphNode> children = rg.GetChildNodes(name);                  // list of children for each node during search
            Dictionary<int, ArrayList> dict = new Dictionary<int, ArrayList>(); // dictionary to store nodes by generation
            int generation = 0;                                                 // count of generation                     
            int child_count = children.Count;                                   // count of children in current generation

            // first generation
            dict.Add(generation, new ArrayList());

            foreach (GraphNode child in children)
            {
                descendants.Enqueue(child);
                dict[generation].Add(child);
            }

            //while the queue is populated, dequeue a node and add its children to the queue and the dictionary
            int dequeue_count = 0;                                              // count of dequeues to know when to add a new generation
            int next_gen_count = 0;                                             // count of children in the next generation
            while (descendants.Count != 0)
            {
                dict.Add(generation + 1, new ArrayList());
                dequeue_count = 0;

                while (dequeue_count < child_count)                             // while the children of a generation are being added to the queue and dict
                {
                    GraphNode descendant = descendants.Dequeue();
                    Console.WriteLine(descendant.Label);
                    if (descendant.Label != "Unvisited")
                    {
                        Console.WriteLine("Cycle detected!");
                        dict.Clear();
                        return dict;
                    }

                    children = rg.GetChildNodes(descendant.Name);
                    foreach (GraphNode child in children)
                    {
                        descendants.Enqueue(child);
                        dict[generation + 1].Add(child);
                        next_gen_count += 1;
                    }
                    dequeue_count += 1;
                    descendant.Label = "visited";
                }

                // one generation is done
                generation += 1;
                child_count = next_gen_count;   // store the next generation child count in present generations child count
                next_gen_count = 0;             // reset to count for the next generation
            }
            return dict;
        }

        //show descendants from GetDescendants
        private static void ShowDescendants(string name)
        {
            if (rg.GetChildNodes(name).Count < 1)
            {
                Console.WriteLine(name + " has no descendants");
                return;
            }

            Dictionary<int, ArrayList> descendants = GetDescendants(name);
            if (descendants.Count < 1)
                return;

            Console.Write("Children: ");
            foreach (GraphNode child in descendants[0])
                Console.Write(child.Name + " ");
            Console.WriteLine();

            Console.Write("Grandchildren: ");
            foreach (GraphNode grandchild in descendants[1])
                Console.Write(grandchild.Name + " ");

            Console.WriteLine();

            for (int i = 2; i < descendants.Count - 1; i++)
            {
                Console.Write("Great ");
                for (int j = 2; j < i; j++)
                    Console.Write("great ");

                Console.Write("grandchildren: ");
                foreach (GraphNode descendant in descendants[i])
                {
                    Console.Write(descendant.Name + " ");
                }
                Console.WriteLine();
            }
        }

        // accept, parse, and execute user commands
        private static void CommandLoop()
        {
            string command = "";
            string[] commandWords;
            Console.Write("Welcome to Harry's Dutch Bingo Parlor!\n");

            while (command != "exit")
            {
                Console.Write("\nEnter a command: ");
                command = Console.ReadLine();
                commandWords = Regex.Split(command, @"\s+");        // split input into array of words
                command = commandWords[0];

                if (command == "exit")
                    ;                                               // do nothing

                // read a relationship graph from a file
                else if (command == "read" && commandWords.Length > 1)
                    ReadRelationshipGraph(commandWords[1]);

                // show information for one person
                else if (command == "show" && commandWords.Length > 1)
                    ShowPerson(commandWords[1]);

                else if (command == "friends" && commandWords.Length > 1)
                    ShowFriends(commandWords[1]);

                //show list of orphans
                else if (command == "orphans")
                    ShowOrphans();

                //show list of descendants
                else if (command == "descendants" && commandWords.Length > 1)
                    ShowDescendants(commandWords[1]);

                // dump command prints out the graph
                else if (command == "dump")
                    rg.Dump();

                // illegal command
                else
                    Console.Write("\nLegal commands: read [filename], dump, show [personname],\n  friends [personname], exit\n");
            }
        }

        static void Main(string[] args)
        {
            CommandLoop();
        }
    }
}