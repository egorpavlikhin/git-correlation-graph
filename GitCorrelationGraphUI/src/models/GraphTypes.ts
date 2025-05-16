// Types for the JSON data structure
export interface ProcessingState {
  LastProcessedCommitHash: string;
  LastProcessedCommitDate: string;
  TotalCommitsProcessed: number;
}

export interface FileEdge {
  SourceFilePath: string;
  TargetFilePath: string;
  CoCommitCount: number;
}

export interface FileNode {
  FilePath: string;
  CommitCount: number;
  Edges: FileEdge[];
}

export interface CorrelationGraph {
  Nodes: FileNode[];
  ProcessingState: ProcessingState;
}

// Types for the visualization library
export interface GraphNode {
  id: string;
  name: string;
  commitCount: number;
  val: number; // Size of the node in visualization
}

export interface GraphLink {
  source: string;
  target: string;
  coCommitCount: number;
  correlation: number;
  value: number; // Strength of the link in visualization
}

export interface GraphData {
  nodes: GraphNode[];
  links: GraphLink[];
}

// Filter settings
export interface FilterSettings {
  minCommitCount: number;
  maxConnections: number;
  minCorrelation: number; // Minimum correlation percentage (0-1)
}
