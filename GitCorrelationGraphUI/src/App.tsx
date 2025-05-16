import React, { useState } from 'react';
import './App.css';
import FileInput from './components/FileInput';
import FilterControls from './components/FilterControls';
import GraphVisualization from './components/GraphVisualization';
import { CorrelationGraph, FilterSettings, GraphData } from './models/GraphTypes';
import { GraphDataService } from './services/GraphDataService';

const App: React.FC = () => {
  const [graph, setGraph] = useState<CorrelationGraph | null>(null);
  const [graphData, setGraphData] = useState<GraphData>({ nodes: [], links: [] });
  const [filters, setFilters] = useState<FilterSettings>({
    minCommitCount: 1,
    maxConnections: 50,
    minCorrelation: 0.1 // 10% minimum correlation
  });
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const handleFileLoaded = async (file: File) => {
    setLoading(true);
    setError(null);

    try {
      const loadedGraph = await GraphDataService.loadGraphFromFile(file);
      setGraph(loadedGraph);

      // Apply initial filters
      const processedData = GraphDataService.convertToGraphData(loadedGraph, filters);
      setGraphData(processedData);
    } catch (err) {
      setError('Failed to load graph data. Please ensure the file is a valid JSON file.');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleFiltersChange = (newFilters: FilterSettings) => {
    setFilters(newFilters);

    if (graph) {
      const processedData = GraphDataService.convertToGraphData(graph, newFilters);
      setGraphData(processedData);
    }
  };

  return (
    <div className="app">
      <header className="app-header">
        <h1>Git Correlation Graph Visualizer</h1>
      </header>

      <main className="app-content">
        {!graph && (
          <div className="file-input-section">
            <FileInput onFileLoaded={handleFileLoaded} />
          </div>
        )}

        {loading && <div className="loading">Loading graph data...</div>}

        {error && <div className="error">{error}</div>}

        {graph && (
          <div className="graph-section">
            <div className="controls-panel">
              <FilterControls filters={filters} onFiltersChange={handleFiltersChange} />
              <div className="stats">
                <h3>Graph Statistics</h3>
                <p>Total Files: {graph.Nodes.length}</p>
                <p>Displayed Files: {graphData.nodes.length}</p>
                <p>Connections: {graphData.links.length}</p>
                <p>Commits Processed: {graph.ProcessingState.TotalCommitsProcessed}</p>
              </div>
              <button
                className="load-new-button"
                onClick={() => setGraph(null)}
              >
                Load New Graph
              </button>
            </div>

            <GraphVisualization graphData={graphData} />
          </div>
        )}
      </main>

      <footer className="app-footer">
        <p>Git Correlation Graph Visualizer</p>
      </footer>
    </div>
  );
};

export default App;
