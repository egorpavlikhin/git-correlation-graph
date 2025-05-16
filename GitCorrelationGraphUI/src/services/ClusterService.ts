import { CorrelationGraph, FileNode } from '../models/GraphTypes';

// Interface for a file in a cluster
export interface ClusterFile {
  filePath: string;
  commitCount: number;
  connectionCount: number;
}

// Interface for a connection between files
export interface ClusterConnection {
  source: string;
  target: string;
  correlation: number;
  coCommitCount: number;
}

// Interface for a cluster of files
export interface FileCluster {
  files: ClusterFile[];
  connections: ClusterConnection[];
}

/**
 * Service for identifying clusters of highly correlated files
 */
export class ClusterService {
  /**
   * Identify clusters of highly correlated files based on specified criteria
   * - Files with fewer than 15 connections
   * - Files with at least 2 commits
   * - Files with correlation of 40% or more
   */
  public static identifyClusters(graph: CorrelationGraph): FileCluster[] {
    // Step 1: Filter nodes based on criteria
    const eligibleNodes = graph.Nodes.filter(node => 
      node.CommitCount >= 2 && // At least 2 commits
      node.Edges.length <= 15  // Fewer than 15 connections
    );

    // Step 2: Create a map of eligible nodes for quick lookup
    const eligibleNodeMap = new Map<string, FileNode>();
    eligibleNodes.forEach(node => {
      eligibleNodeMap.set(node.FilePath, node);
    });

    // Step 3: Create a graph representation for cluster identification
    const adjacencyList = new Map<string, Set<string>>();
    const connections = new Map<string, ClusterConnection>();

    // Populate the adjacency list with strong connections (40% or more)
    eligibleNodes.forEach(node => {
      if (!adjacencyList.has(node.FilePath)) {
        adjacencyList.set(node.FilePath, new Set<string>());
      }

      node.Edges.forEach(edge => {
        const targetNode = eligibleNodeMap.get(edge.TargetFilePath);
        
        if (targetNode) {
          // Calculate correlation
          const maxCommitCount = Math.max(node.CommitCount, targetNode.CommitCount);
          const correlation = maxCommitCount > 0 ? edge.CoCommitCount / maxCommitCount : 0;
          
          // Only consider strong correlations (40% or more)
          if (correlation >= 0.4) {
            // Add to adjacency list
            adjacencyList.get(node.FilePath)?.add(edge.TargetFilePath);
            
            if (!adjacencyList.has(edge.TargetFilePath)) {
              adjacencyList.set(edge.TargetFilePath, new Set<string>());
            }
            adjacencyList.get(edge.TargetFilePath)?.add(node.FilePath);
            
            // Store connection details
            const connectionKey = [node.FilePath, edge.TargetFilePath].sort().join('|');
            if (!connections.has(connectionKey)) {
              connections.set(connectionKey, {
                source: node.FilePath,
                target: edge.TargetFilePath,
                correlation: correlation,
                coCommitCount: edge.CoCommitCount
              });
            }
          }
        }
      });
    });

    // Step 4: Identify clusters using depth-first search
    const visited = new Set<string>();
    const clusters: FileCluster[] = [];

    eligibleNodes.forEach(node => {
      if (!visited.has(node.FilePath)) {
        const clusterFiles = new Set<string>();
        const clusterConnections = new Set<string>();
        
        // DFS to find connected components
        const dfs = (filePath: string) => {
          visited.add(filePath);
          clusterFiles.add(filePath);
          
          adjacencyList.get(filePath)?.forEach(neighbor => {
            // Add the connection
            const connectionKey = [filePath, neighbor].sort().join('|');
            clusterConnections.add(connectionKey);
            
            if (!visited.has(neighbor)) {
              dfs(neighbor);
            }
          });
        };
        
        dfs(node.FilePath);
        
        // Only consider as clusters if there are at least 2 files
        if (clusterFiles.size >= 2) {
          const files: ClusterFile[] = Array.from(clusterFiles).map(filePath => {
            const node = eligibleNodeMap.get(filePath)!;
            return {
              filePath: node.FilePath,
              commitCount: node.CommitCount,
              connectionCount: node.Edges.length
            };
          });
          
          const clusterConnectionsList: ClusterConnection[] = Array.from(clusterConnections)
            .map(key => connections.get(key)!)
            .sort((a, b) => b.correlation - a.correlation);
          
          clusters.push({
            files: files,
            connections: clusterConnectionsList
          });
        }
      }
    });

    // Sort clusters by size (number of files) in descending order
    return clusters.sort((a, b) => b.files.length - a.files.length);
  }
}
