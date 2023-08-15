# air ducts

## data model

air duct system will follow rails.

an hvac collection contains a list of lists:
each list is a path of hvac ducts

hvac duct contains a "crawlpoint" child

hvac collection builds a (possibly branching) graph from the list of lists
each node in the graph is a crawlpoint
each node in the graph knows its connected neighbors.

gizmos draw lines indicating network

## player movement

player starts at a node in the graph.

when player position is close to the node, the target node is selected by maximum dot product between player move vector and nearest neighbors.

player moves by projecting move vector onto target node.

Q: how to prevent player from moving backward off of node?