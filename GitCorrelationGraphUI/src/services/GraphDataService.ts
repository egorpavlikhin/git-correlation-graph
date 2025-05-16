import { CorrelationGraph, FilterSettings, GraphData, GraphLink, GraphNode } from '../models/GraphTypes';

/**
 * Service for processing graph data
 */
export class GraphDataService {
  /**
   * Convert the correlation graph to a format suitable for visualization
   */
  public static convertToGraphData(graph: CorrelationGraph, filters: FilterSettings): GraphData {
    const nodes: GraphNode[] = [];
    const links: GraphLink[] = [];
    const nodeMap = new Map<string, number>(); // Map to track connections per node

    // First pass: create nodes and count connections
    graph.Nodes.forEach(node => {
      // Count connections for this node
      const connectionCount = node.Edges.length;
      nodeMap.set(node.FilePath, connectionCount);
    });

    // Second pass: apply filters and create nodes
    graph.Nodes.forEach(node => {
      const connectionCount = nodeMap.get(node.FilePath) || 0;

      // Apply filters
      if (node.CommitCount >= filters.minCommitCount &&
          connectionCount <= filters.maxConnections) {
        nodes.push({
          id: node.FilePath,
          name: node.FilePath,
          commitCount: node.CommitCount,
          val: Math.log(node.CommitCount + 1) * 2 // Scale node size logarithmically
        });
      }
    });

    // Create a set of node IDs that passed the filter
    const nodeIds = new Set(nodes.map(n => n.id));

    // Third pass: create links between nodes that passed the filter
    graph.Nodes.forEach(node => {
      if (nodeIds.has(node.FilePath)) {
        node.Edges.forEach(edge => {
          if (nodeIds.has(edge.TargetFilePath)) {
            // Calculate correlation
            const sourceNode = graph.Nodes.find(n => n.FilePath === edge.SourceFilePath);
            const targetNode = graph.Nodes.find(n => n.FilePath === edge.TargetFilePath);

            if (sourceNode && targetNode) {
              // Get the maximum commit count between the two files
              const maxCommitCount = Math.max(sourceNode.CommitCount, targetNode.CommitCount);

              // Calculate correlation as percentage of the largest number of commits
              // This ensures that file A with 8 commits cannot be correlated more than 50%
              // with file B that had 4 commits, even if file B is always committed with file A
              const correlation = maxCommitCount > 0 ? edge.CoCommitCount / maxCommitCount : 0;

              // Apply correlation filter
              if (correlation >= filters.minCorrelation) {
                links.push({
                  source: edge.SourceFilePath,
                  target: edge.TargetFilePath,
                  coCommitCount: edge.CoCommitCount,
                  correlation: correlation,
                  value: correlation // Link strength based on correlation
                });
              }
            }
          }
        });
      }
    });

    return { nodes, links };
  }

  /**
   * Load graph data from a JSON file
   */
  public static async loadGraphFromFile(file: File): Promise<CorrelationGraph> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();

      reader.onload = (event) => {
        try {
          const json = event.target?.result as string;
          const data = JSON.parse(json) as CorrelationGraph;
          resolve(data);
        } catch (error) {
          reject(error);
        }
      };

      reader.onerror = () => {
        reject(new Error('Error reading file'));
      };

      reader.readAsText(file);
    });
  }
}
