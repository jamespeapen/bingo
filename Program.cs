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

        //show descendants
        private static void ShowDescendants(string name)
        {
            // check if there are descendants
            if (rg.GetChildNodes(name).Count < 1)
            {
                Console.WriteLine(name + " has no descendants");
                return;
            }

            List<GraphNode> current_generation = new List<GraphNode>();                 //list of nodes in current generation being printed
            List<GraphNode> next_generation = new List<GraphNode>();                    //list of nodes of children of current generation being printed
            int generation_number = 1;                                                  //count of generations

            // print children and get grandchildren
            Console.WriteLine("*Children: ");
            current_generation = rg.GetChildNodes(name);
            foreach (GraphNode child in current_generation)
            {
                Console.Write(child.Name + " ");
                child.Label = "visited";
                foreach (GraphNode grandchild in rg.GetChildNodes(child.Name))
                {
                    // check for cycle
                    if (node_visited(grandchild))
                    {
                        Console.WriteLine("Cycle detected!");
                        return;
                    }
                    
                    // add to grandchild list
                    next_generation.Add(grandchild);
                    grandchild.Label = "visited";
                }
            }
            Console.WriteLine();

            //return if there are no more children
            if (next_generation.Count < 1)
                return;

            generation_number = 2;

            // move nodes from next_geneation into current_generation
            current_generation.Clear();
            copy_list(current_generation, next_generation);
            next_generation.Clear();

            //print grandchildren and get greatgrandchildren
            Console.WriteLine();
            Console.WriteLine("*Grandchilren: ");
            foreach (GraphNode grandchild in current_generation)
            {
                Console.Write(grandchild.Name + " ");
                foreach (GraphNode greatgrandchild in rg.GetChildNodes(grandchild.Name))
                {
                    if (node_visited(greatgrandchild))
                    {
                        Console.WriteLine("Cycle detected!");
                        return;
                    }
                    next_generation.Add(greatgrandchild);
                    greatgrandchild.Label = "visited";
                }
            }
            Console.WriteLine();

            if (next_generation.Count < 1)
                return;

            current_generation.Clear();

            //while there are kids in each next generation, print them and get their kids
            while (next_generation.Count > 1)
            {   
                Console.WriteLine();
                Console.Write("*Great ");
                generation_number++;
                copy_list(current_generation, next_generation);
                next_generation.Clear();

                // print the required number of 'greats'
                for (int i = 2; i < generation_number; i++)
                {
                    Console.Write("great ");
                }

                Console.WriteLine("grandchildren: ");

                foreach (GraphNode greatgrandchild in current_generation)
                {
                    Console.Write(greatgrandchild.Name + " ");
                    foreach (GraphNode nextgreatkid in rg.GetChildNodes(greatgrandchild.Name))
                    {
                        if (node_visited(nextgreatkid))
                        {
                            Console.WriteLine("Cycle detected!");
                            return;
                        }
                        next_generation.Add(nextgreatkid);
                    }
                }
                current_generation.Clear();
                Console.WriteLine();
            }

            reset_label();
            return;
        }

        // copy function to copy next generation list into current generation
        private static void copy_list(List<GraphNode> current, List<GraphNode> next)
        {
            foreach (GraphNode person in next)
                current.Add(person);
        }

        // check if a node has been visited
        private static bool node_visited(GraphNode node)
        {
            return node.Label == "visited";
        }

        // reset visit labels to unvisited after a descendant search
        private static void reset_label()
        {
            foreach (GraphNode person in rg.nodes)
                person.Label = "Unvisited";
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
                    return;                                               // do nothing

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
                    Console.Write("\nLegal commands: read [filename], dump, show [personname],\n  friends [personname], descendants [personname] exit\n");
            }
        }

        static void Main(string[] args)
        {
            CommandLoop();
        }
    }
}