using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ASentinelSearchState : IEnemyState
{
    private SentinelAgent _sentinelAgent;
    private NavMeshAgent _agent;
    private List<Vector3> _path = new List<Vector3>();
    private int _nodeIndex = 0;
    private int _graphNodeCapacity = 8;
    private float _minSearchDistance = 7f;
    private float _maxSearchDistance = 12f;
    private float _searchTimer = 0f;
    private float _maxSearchTime = 5f;

    public void EnterState(Enemy enemy)
    {
        this._sentinelAgent = (SentinelAgent)enemy;
        _agent = _sentinelAgent.GetNavMeshAgent();
        _agent.updateRotation = true;

        _searchTimer = 0f;
        CreateSearchPath(); //start search process
    }

    public void UpdateState()
    {
        _searchTimer += Time.deltaTime;

        //agent has spotted target
        if (_sentinelAgent.AcquireTarget())
        {
            _sentinelAgent.SetStateTransitionNum();

            if (_sentinelAgent.CompareTag("TargetII") || _sentinelAgent.CompareTag("TargetIII"))
            {
                _sentinelAgent.SetCombatStateTransitionNum();
                _sentinelAgent.stateMachine.ChangeState(new SplitSentinelCombatState());
            }
            if (_sentinelAgent.CompareTag("Target"))
            {
                _sentinelAgent.SetCombatStateTransitionNum();
                _sentinelAgent.stateMachine.ChangeState(new USentinelCombatState());
            }

            return;
        }

        //agent has been searching area for long enough so try new search - also stops agent from getting stuck
        if (_searchTimer >= _maxSearchTime)
        {
            _searchTimer = 0f;
            CreateSearchPath();
            return;
        }

        //path is empty as no path found or completed path
        if (_path.Count == 0 || _nodeIndex >= _path.Count)
        {
            CreateSearchPath();
            return;
        }

        //agent has reached the node so move on to the next one by incrementing index in list
        if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
        {
            _nodeIndex++;
            if (_nodeIndex < _path.Count)
            {
                _agent.SetDestination(_path[_nodeIndex]);
            }
        }
    }

    private void CreateSearchPath()
    {
        List<Vector3> foundNodes = DiscoverNodes();

        //found only one place so can't traverse a path, clear, return to try again
        if (foundNodes.Count < 2)
        {
            _path.Clear();
            return;
        }

        Vector3 currentPosition = _sentinelAgent.transform.position;
        _path = AStarSearch(currentPosition, foundNodes);

        //no paths found
        if (_path.Count == 0)
        {
            return;
        }

        _nodeIndex = 0; //reset the index
        _sentinelAgent.GetNavMeshAgent().SetDestination(_path[_nodeIndex]); //start the path
        _searchTimer = 0f; //reset timer
    }

    //Search a random positions around the SentinelAgent to discover valid nodes to use pathfinding on
    private List<Vector3> DiscoverNodes()
    {
        List<Vector3> foundNodes = new List<Vector3>();

        for (int i = 0; i < _graphNodeCapacity; i++)
        {
            //Random direction and distance within a circle
            Vector2 searchDir = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(_minSearchDistance, _maxSearchDistance);
            
            Vector3 searchPosition = _sentinelAgent.transform.position + new Vector3(searchDir.x, 0, searchDir.y);

            NavMeshHit hit;

            //If a valid NavMesh point within 2 units of the search position can be found
            if (NavMesh.SamplePosition(searchPosition, out hit, 2.0f, NavMesh.AllAreas))
            {
                Vector3 nodePosition = hit.position;

                //If there is clear line of sight from the agent to the discovered node
                if (HasLineOfSight(_sentinelAgent.transform.position, nodePosition))
                {
                    foundNodes.Add(nodePosition);
                }
            }
        }

        return foundNodes;
    }

    private bool HasLineOfSight(Vector3 startPos, Vector3 endPos)
    {
        Vector3 direction = endPos - startPos;

        return !Physics.Raycast(startPos, direction.normalized, direction.magnitude, LayerMask.GetMask("Default"));
    }

    private List<Vector3> AStarSearch(Vector3 startingPosition, List<Vector3> nodeList)
    {
        Vector3 startNode = startingPosition;
        Vector3 destinationNode = nodeList[UnityEngine.Random.Range(0, nodeList.Count)];

        //Return if no path
        if (nodeList.Count < 2)
        {
            return new List<Vector3>();
        }
            
        //Dictionary to keep estimated distance from the start node to end node
        //f(n) = g + h (heuristic = actual distance + heuristic distance)
        Dictionary<Vector3, float> estDistance = new Dictionary<Vector3, float>();
        //Dictionary to keep predecessor Nodes, in order for shortest path to be re-traced
        Dictionary<Vector3, Vector3> predecessorNodes = new Dictionary<Vector3, Vector3>();
        //List to keep every Node visited so algorithm doesn't keep repeating forever
        HashSet<Vector3> closedSet = new HashSet<Vector3>();
        //Order the nodes in the openSet by the lowest value
        List<Vector3> openSet = new List<Vector3>();

        //add start node to openSet for consideration
        openSet.Add(startNode);

        //Initialise scores for all nodes to show that the score has not yet been calculated
        foreach (var node in nodeList)
        {
            estDistance[node] = float.MaxValue;
        }

        //startNode heursitic set to (f = g + h)
        estDistance[startNode] = Heuristic(startNode, destinationNode);

        //While openSet isn't empty (still need to consider nodes)
        while (openSet.Count > 0)
        {
            //Sort open set to get node with lowest estimated total cost (f)
            openSet.Sort((a, b) => estDistance[a].CompareTo(estDistance[b]));
            Vector3 currentNode = openSet[0];
            openSet.RemoveAt(0);

            //End node/destination reached so retrace path and output shortest route
            if (currentNode == destinationNode)
            {
                //contruct path from the destination node to the start not in LIFO order
                return RecreatePath(predecessorNodes, startingPosition, currentNode);
            }
            //Node is added to closedSet
            closedSet.Add(currentNode);

            //Create a list to store neighbour nodes of the current node 
            List<Vector3> neighbors = new List<Vector3>();

            //Assigning neighbouring nodes to the list if it has line of sight on the current node (it's not blocked)
            foreach (var node in nodeList)
            {
                if (node != currentNode && HasLineOfSight(currentNode, node))
                    neighbors.Add(node);
            }

            //Iterate through neighbouring nodes
            foreach (var neighborNode in neighbors)
            {
                //If neighbour is not in the closed set
                if (!closedSet.Contains(neighborNode))
                {
                    //Calculate the estimated cost for a neighbour node
                    float costToNeighbor = Vector3.Distance(currentNode, neighborNode) + Heuristic(neighborNode, destinationNode);

                    //If neighbouring node's new distance from start node is shorter from current node, update distances
                    if (!openSet.Contains(neighborNode) || costToNeighbor < estDistance[neighborNode])
                    {
                        //Update estDistance with new estimated distance
                        estDistance[neighborNode] = costToNeighbor;

                        //Record current path of previous nodes
                        predecessorNodes[neighborNode] = currentNode;

                        //Add neighbour node to open set for consideration
                        openSet.Add(neighborNode);
                    }
                }

                
            }
        }

        //No path found so return an empty list
        return new List<Vector3>();
    }

    private float Heuristic(Vector3 a, Vector3 b) => Vector3.Distance(a, b);

    //Reconstruct the A* Search path of nodes in the correct order (LIFO)
    private List<Vector3> RecreatePath(Dictionary<Vector3, Vector3> predecessorNodes, Vector3 startNode, Vector3 endNode)
    {
        //List to store the path of nodes
        List<Vector3> nodePath = new List<Vector3>();
        //Get the end Node and add it it to the list
        nodePath.Add(endNode);
        //While the end node does not equal the start node, there are still more nodes to add so the end of the list has not been reached
        while (endNode != startNode)
        {
            //retrieve predecessor node
            endNode = predecessorNodes[endNode];
            //Add to the front of the list/path
            nodePath.Insert(0, endNode); 
        }

        return nodePath;
    }
}
