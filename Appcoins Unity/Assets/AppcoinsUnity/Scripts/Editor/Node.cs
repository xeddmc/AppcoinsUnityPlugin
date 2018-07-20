using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

public class TestTree
{
    private static string appcoinsMainTemplate = UnityEngine.Application.dataPath + "/AppcoinsUnity/Plugins/Android/mainTemplate.gradle";
    private static string currentMainTemplate = UnityEngine.Application.dataPath + "/Plugins/Android/mainTemplate.gradle";

    [MenuItem("TestTree/Print Tree")]
    public static void PrintTree()
    {
        Tree<string> tCurrent = Tree<string>.CreateTreeFromFile(currentMainTemplate, FileParser.BUILD_GRADLE);
        Tree<string> tAppcoins = Tree<string>.CreateTreeFromFile(appcoinsMainTemplate, FileParser.BUILD_GRADLE);

        tCurrent.TraverseDFS(tCurrent.GetRoot(), delegate(Node<string> node)
        {
            UnityEngine.Debug.Log(node.ToString() + " / " + node.ToString().Length);
        }, 
        delegate(Node<string> node){}, delegate(Node<string> node){});

        UnityEngine.Debug.Log("");
        UnityEngine.Debug.Log("\nOtherFile\n");
        UnityEngine.Debug.Log("");

        tAppcoins.TraverseDFS(tAppcoins.GetRoot(), delegate(Node<string> node)
        {
            UnityEngine.Debug.Log(node.ToString() + " / " + node.ToString().Length);
        }, 
        delegate(Node<string> node){}, delegate(Node<string> node){});

        UnityEngine.Debug.Log("");
        UnityEngine.Debug.Log("\nMergeTree\n");
        UnityEngine.Debug.Log("");

        tCurrent.MergeTrees(tAppcoins);

        tCurrent.TraverseDFS(tCurrent.GetRoot(), delegate(Node<string> node)
        {
            UnityEngine.Debug.Log(node.ToString() + " / " + node.ToString().Length);
        }, 
        delegate(Node<string> node){}, delegate(Node<string> node){});
    }
}

// Node of a Tree or Graph
public class Node<T>
{
    // Item to be stored by the node
    private T _item;

    // Reference to current node's father
    private Node<T> _parent;

    // List with all childs (outer edges)
    private List<Node<T>> _childs;

    // Current index child
    public int _indexChild;

    public bool _visited;

    // Default constructor, item is default of 'T'
    public Node()
    {
        _item = default(T);
        _parent = null;
        _childs = new List<Node<T>>();
        _indexChild = 0;
        _visited = false;
    }

    // Constructor with a specific 'T' item
    public Node(T item) : this()
    {
        _item = item;
    }

    // Constructor with a specific 'T' item a spcefic parent
    public Node(T item, Node<T> parent) : this(item)
    {
        _parent = parent;
    }

    // Get item of the node
    public T GetItem()
    {
        return _item;
    }

    // Set new item fot the node
    public void SetItem(T item)
    {
        _item = item;
    }

    // Get refence to parent
    public Node<T> GetParent()
    {
        return _parent;
    }

    // Set refence to parent
    public void SetParent(Node<T> parent)
    {
        _parent = parent;
    }

    // Get child by index
    public Node<T> GetChild(int index)
    {
        return _childs[index];
    }

    public int GetChildsCount()
    {
        return _childs.Count;
    }

    // Change child by index at childs list
    public void SetChild(int index, Node<T> newChild)
    {
        _childs[index] = newChild;
    }

    // Apply delegate to all node's childs
    public void ForEach(Action<Node<T>> action)
    {
        _childs.ForEach(action);
    }

    // Add one child to node at the end of his 'childsList'
    public void AddChild(Node<T> child)
    {
        _childs.Add(child);
    }

    // Remove a node's child at a specific index
    public void RemoveChildAt(int index)
    {
        _childs.RemoveAt(index);
    }

    // Remove the first node's child with a specific item
    public bool RemoveChild(Node<T> childToRemove)
    {
        return _childs.Remove(childToRemove);
    }

    // Find a child by a predicate
    public Node<T> FindChild(Predicate<Node<T>> childToSearch)
    {
        return _childs.Find(childToSearch);
    }

    public string ToString()
    {
        return _item.ToString();
    }
}

public enum FileParser
{
    BUILD_GRADLE
}

public class Tree<T>
{
    // Height of the tree
    private int _height;

    // Root node ('null' item)
    private Node<T> _root = null;

    // Constrctor with a specific root
    public Tree(Node<T> root)
    {
        _height = 0;
        _root = root;
    }

    // Contructor that creates a root node with a specific item
    public Tree(T item) : this(new Node<T>(item)) {}

    public Node<T> GetRoot()
    {
        return _root;
    }

    // Create tree from a file (T = string)
    public static Tree<string> CreateTreeFromFile(string pathToFile, FileParser fileParser)
    {
        StreamReader fileReader = new StreamReader(pathToFile);
        string allFile = fileReader.ReadToEnd();
        fileReader.Close();

        if(fileParser == FileParser.BUILD_GRADLE)
        {
            return CreateTreeFromBuildGradleFile(allFile);
        }

        return new Tree<string>("root");
    }

    // Parse build.gradle file to a tree
    private static Tree<string> CreateTreeFromBuildGradleFile(string allFile)
    {
        string newString = "";
        bool newLine = true;

        Node<string> currentNode = new Node<string>("root");
        Tree<string> t = new Tree<string>(currentNode);

        for(int i = 0; i < allFile.Length; i++)
        {
            Node<string> auxNode;

            // Ignore whitespaces till first letter (or number?)
            while(newLine && i < allFile.Length)
            {
                if(Char.IsDigit(allFile[i]) || Char.IsLetter(allFile[i]) || Char.IsPunctuation(allFile[i]))
                {
                    newLine = false;
                    break;
                }

                i++;
            }

            // Create new node (inner node)
            if(allFile[i].Equals('{'))
            {
                currentNode.AddChild(new Node<string>(newString, currentNode));
                currentNode = currentNode.GetChild(currentNode.GetChildsCount() - 1);
                // UnityEngine.Debug.Log(newString);
                newString = "";
                t._height++;
                newLine = true;
            }

            // Create new child node (leaf)
            else if(allFile[i].Equals('\n'))
            {
                if(newString.Length > 0)
                {
                    currentNode.AddChild(new Node<string>(newString, currentNode));
                }

                // UnityEngine.Debug.Log(newString);
                newString = "";
                newLine = true;
            }

            // Close node and make father the current node
            else if(allFile[i].Equals('}'))
            {
                // Create new child node before closing father node
                if(newString.Length > 0 && 
                    (Char.IsLetter(newString[0]) || Char.IsDigit(newString[0]) || Char.IsPunctuation(newString[0])))
                {
                    currentNode.AddChild(new Node<string>(newString, currentNode));
                    // UnityEngine.Debug.Log(newString);
                    newString = "";
                    t._height++;                 
                }

                currentNode = currentNode.GetParent();
            }

            // Remove spaces between punctiation marks
            else if(allFile[i].Equals(' '))
            {
                if(i == allFile.Length)
                {
                    break;
                }

                else if(i > 0 && Char.IsLetter(allFile[i - 1]) && Char.IsLetter(allFile[i + 1]))
                {
                    newString += ' ';
                }

                else
                {
                    continue;
                }
            }

            else
            {
             newString += allFile[i];
            }
        }

        return t;
    }

    public TreeBFS(Node<T> source, Action<Node<T>> OnVisitingNode, Action<Node<T>> OnVisitingAllChilds, Action<Node<T>> OnVisitingUnvisitedChilds)
    {
        TraverseDFS(source, delegate(Node<T> node)
        {
            node-_visited = false;
        }, delegate(Node<T> node){}, delegate(Node<T> node){});

        Queue q = new Queue();
        q.Enqueue(source);

        while(q.Count > 0)
        {
            Node<T> currentNode = (Node<T>) q.Dequeue();
            OnVisitingNode(currentNode);

            if(currentNode._visited == false)
            {
                currentNode._visited = true;
            }

            for(int i = 0; i < currentNode.GetChildsCount(); i++)
            {
                Node<T> childNode = currentNode.GetChild(i);
                OnVisitingAllChilds(childNode);

                if(childNode._visited == false)
                {
                    q.Enqueue(childNode);
                    OnVisitingUnvisitedChilds(childNode);
                }
            }
        }
    }

    // (BFS) Find a the tree path from a list of nodes (returns the last node that exists at tree and list)
    // If a path's node exists in the tree that node is removed from the 'path' list
    private Node<T> FindPath(List<Node<T>> path)
    {
        bool childFounded = false;

        Node<T> treeNodeToSearch = _root;
        Node<T> currentPathNode = path[0];

        do
        {
            // Check if 'currentPathNode' is a child of 'treeNodeToSearch'
            Node<T> aux = treeNodeToSearch.FindChild(node => node.Equals(currentPathNode.GetItem()));

            if(!treeNodeToSearch.GetItem().Equals(default(T)))
            {
                treeNodeToSearch = aux;
                childFounded = true;
                path.RemoveAt(0);
                currentPathNode = path[0];
            }
        } while(childFounded && path.Count > 0);

        return treeNodeToSearch;
    }

    // Insert new path to tree
    private void InsertPath(Node<T> parentTreeNode, List<Node<T>> path)
    {
       if(path.Count <= 0)
       {
           return;
       }

       while(path.Count > 0)
       {
           parentTreeNode.AddChild(path[0]);
           parentTreeNode = path[0];
           path.RemoveAt(0);
       }
    }

    // Merge full path to tree (repeated nodes are not inserted)
    public void FindAndInsertPath(List<Node<T>> path)
    {
        Node<T> parent = FindPath(path);
        InsertPath(parent, path);
    }

    public void MergeTrees(Tree<T> tree)
    {
        List<Node<T>> path = new List<Node<T>>();

        TraverseDFS(tree.GetRoot(), delegate(Node<T> node)
        {
            path.Add(node);
        }, delegate(Node<T> node)
        {
            FindAndInsertPath(path);
        }, delegate(Node<T> node)
        {
            path.RemoveAt(path.Count - 1);
        });
    }

    // Preorder traverse (For each node run a predicate)
    public void TraverseDFS(Node<T> currentNode, Action<Node<T>> CurrentNodeAction, Action<Node<T>> NextNodeAction, Action<Node<T>> RetrieveToNodeAction)
    {
        currentNode._indexChild = 0;
        CurrentNodeAction(currentNode);

        if(currentNode.GetChildsCount() == 0)
        {
            RetrieveToNodeAction(currentNode);
            return;
        }

        else
        {
            while(currentNode._indexChild < currentNode.GetChildsCount())
            {
                NextNodeAction(currentNode);

                TraverseDFS(currentNode.GetChild(currentNode._indexChild), CurrentNodeAction, NextNodeAction, RetrieveToNodeAction);
                currentNode._indexChild++;
            }

            return;
        }
    }
}