import React, { useMemo, useState } from 'react';
import { CorrelationGraph } from '../models/GraphTypes';
import { ClusterService } from '../services/ClusterService';

interface ClusterViewProps {
  graph: CorrelationGraph;
}

const ClusterView: React.FC<ClusterViewProps> = ({ graph }) => {
  // Use the cluster service to identify clusters
  const clusters = useMemo(() => {
    return ClusterService.identifyClusters(graph);
  }, [graph]);

  // State to track which clusters have their connections expanded
  const [expandedConnections, setExpandedConnections] = useState<Record<number, boolean>>({});

  // Toggle the expanded state for a specific cluster
  const toggleConnectionsExpanded = (clusterIndex: number) => {
    setExpandedConnections(prev => ({
      ...prev,
      [clusterIndex]: !prev[clusterIndex]
    }));
  };

  if (clusters.length === 0) {
    return (
      <div className="cluster-view">
        <div className="no-clusters">
          No highly correlated clusters found based on the criteria:
          <ul>
            <li>Files with fewer than 15 connections</li>
            <li>Files with at least 2 commits</li>
            <li>Files with correlation of 40% or more</li>
          </ul>
        </div>
      </div>
    );
  }

  return (
    <div className="cluster-view">
      <div className="cluster-header">
        <h2>Highly Correlated File Clusters</h2>
        <p className="cluster-criteria">
          Showing clusters where files have:
          <ul>
            <li>Fewer than 15 connections</li>
            <li>At least 2 commits</li>
            <li>Correlation of 40% or more</li>
          </ul>
        </p>
      </div>

      <div className="clusters-list">
        {clusters.map((cluster, index) => (
          <div key={index} className="cluster-item">
            <h3>Cluster {index + 1} ({cluster.files.length} files)</h3>
            <div className="cluster-files">
              {cluster.files.map((file, fileIndex) => (
                <div key={fileIndex} className="cluster-file">
                  <div className="file-name">{file.filePath}</div>
                  <div className="file-stats">
                    <span>{file.commitCount} commits</span>
                    <span>{file.connectionCount} connections</span>
                  </div>
                </div>
              ))}
            </div>
            <div className="cluster-connections">
              <button
                className="toggle-connections-button"
                onClick={() => toggleConnectionsExpanded(index)}
              >
                <h4>
                  Connections within cluster ({cluster.connections.length})
                  <span className="toggle-icon">
                    {expandedConnections[index] ? '▼' : '►'}
                  </span>
                </h4>
              </button>

              {expandedConnections[index] && (
                <ul className="connection-list">
                  {cluster.connections.map((conn, connIndex) => (
                    <li key={connIndex} className="connection-item">
                      <div className="connection-files">
                        <span>{conn.source}</span>
                        <span>↔</span>
                        <span>{conn.target}</span>
                      </div>
                      <div className="connection-strength">
                        {(conn.correlation * 100).toFixed(1)}% correlation
                        ({conn.coCommitCount} co-commits)
                      </div>
                    </li>
                  ))}
                </ul>
              )}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default ClusterView;
