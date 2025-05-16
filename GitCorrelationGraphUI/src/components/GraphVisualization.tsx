import React, { useEffect, useMemo, useRef, useState } from 'react';
import { ForceGraph2D } from 'react-force-graph';
import { GraphData } from '../models/GraphTypes';

interface GraphVisualizationProps {
  graphData: GraphData;
}

const GraphVisualization: React.FC<GraphVisualizationProps> = ({ graphData }) => {
  const graphRef = useRef<any>(null);
  const [dimensions, setDimensions] = useState({ width: 800, height: 600 });
  const [isLargeGraph, setIsLargeGraph] = useState(false);

  // Function to handle zoom to fit
  const handleZoomToFit = () => {
    if (graphRef.current) {
      graphRef.current.zoomToFit(400, 40);
    }
  };

  // Optimize graph data for datasets
  const optimizedGraphData = useMemo(() => {
    // Consider graphs with more than 300 connections as "large"
    const isLarge = graphData.links.length > 800;
    // Consider graphs with more than 1000 connections as "very large"
    const isVeryLarge = graphData.links.length > 1500;
    setIsLargeGraph(isLarge);

    if (!isLarge) return graphData;

    // For large graphs, filter to keep only stronger connections
    // This helps with visualization when there are too many links
    const minCorrelationThreshold = isVeryLarge ? 0.2 : 0.1; // Higher threshold for very large graphs

    // Sort links by correlation strength (descending)
    const sortedLinks = [...graphData.links].sort((a, b) => b.correlation - a.correlation);

    // For large graphs, limit the number of links
    const maxLinks = isVeryLarge ? 1500 : 800;

    // Take top links or those above threshold, whichever is smaller
    const filteredLinks = sortedLinks
      .filter(link => link.correlation >= minCorrelationThreshold)
      .slice(0, maxLinks);

    // Get unique node IDs from filtered links
    const nodeIds = new Set<string>();
    filteredLinks.forEach(link => {
      nodeIds.add(link.source as string);
      nodeIds.add(link.target as string);
    });

    // Filter nodes to only those in the filtered links
    const filteredNodes = graphData.nodes.filter(node => nodeIds.has(node.id));

    return {
      nodes: filteredNodes,
      links: filteredLinks
    };
  }, [graphData]);

  // Update dimensions when window resizes
  useEffect(() => {
    const handleResize = () => {
      setDimensions({
        width: window.innerWidth * 0.8,
        height: window.innerHeight * 0.7
      });
    };

    handleResize(); // Set initial dimensions
    window.addEventListener('resize', handleResize);

    return () => {
      window.removeEventListener('resize', handleResize);
    };
  }, []);

  // Configure force simulation when graph is initialized
  useEffect(() => {
    if (graphRef.current && optimizedGraphData.nodes.length > 0) {
      // Reset the simulation completely to avoid issues with previous configurations
      graphRef.current.d3ReheatSimulation();

      // Configure the force simulation based on graph size
      // Much more conservative parameters to prevent the graph from falling apart
      const baseDistance = 20; // Shorter base distance (was 30-50)
      const strengthMultiplier = 1.5; // Lower strength multiplier (was 3-5)
      const chargeStrength = -30; // Less repulsion
      const chargeDistanceMax = 100; // Shorter max distance for charge effect
      const centerStrength = 0.3; // Much stronger center force to keep nodes together

      // Configure link forces
      graphRef.current
        .d3Force('link')
        .distance((link: any) => {
          // Make strongly correlated links pull nodes closer together
          // Links with higher correlation values will have shorter distances
          // Use a more linear relationship to prevent extreme distances
          return baseDistance * (1 - link.correlation);
        })
        .strength((link: any) => {
          // Make strongly correlated links have stronger forces
          // Use a more linear relationship to prevent extreme forces
          return (0.5 + link.correlation * 0.5) * strengthMultiplier;
        });

      // Adjust charge force to better separate clusters
      graphRef.current
        .d3Force('charge')
        .strength(chargeStrength)
        .distanceMax(chargeDistanceMax)
        .distanceMin(5); // Ensure nodes don't get too close to each other

      // Add center force to keep nodes from drifting too far
      graphRef.current
        .d3Force('center')
        .strength(centerStrength); // Much stronger center force

      // Add a radial force to keep nodes within a reasonable area
      // This is crucial for preventing the graph from falling apart
      try {
        const d3 = (window as any).d3;
        if (d3 && d3.forceRadial) {
          const radialForce = d3.forceRadial(
            Math.min(dimensions.width, dimensions.height) * 0.3, // Radius - 30% of the smaller dimension
            dimensions.width / 2,  // Center X
            dimensions.height / 2  // Center Y
          ).strength(0.2);  // Moderate strength

          graphRef.current.d3Force('radial', radialForce);
        }
      } catch (e) {
        // If radial force fails, strengthen the center force even more
        graphRef.current.d3Force('center').strength(0.5);
      }
    }
  }, [optimizedGraphData, isLargeGraph, dimensions]);

  // Zoom to fit when graph data changes
  useEffect(() => {
    if (graphRef.current && optimizedGraphData.nodes.length > 0) {
      // Initial zoom to fit
      graphRef.current.zoomToFit(10, 40);

      // Wait for graph to stabilize, then zoom to fit again
      setTimeout(() => {
        graphRef.current.zoomToFit(400, 40);
      }, 500);

      // Final adjustment after full stabilization
      setTimeout(() => {
        graphRef.current.zoomToFit(400, 40);
      }, 2000);
    }
  }, [optimizedGraphData]);

  if (graphData.nodes.length === 0) {
    return <div className="no-data">No graph data to display</div>;
  }

  // Display warning for large graphs
  const largeGraphWarning = isLargeGraph && (
    <div className="large-graph-warning">
      <p>Large graph detected ({graphData.links.length} connections).
      Showing only the {optimizedGraphData.links.length} strongest connections for better visualization.</p>
    </div>
  );

  return (
    <div className="graph-container">
      {largeGraphWarning}
      <button
        className="zoom-fit-button"
        onClick={handleZoomToFit}
        title="Zoom to fit all nodes"
      >
        Zoom to Fit
      </button>
      <ForceGraph2D
        ref={graphRef}
        graphData={optimizedGraphData}
        width={dimensions.width}
        height={dimensions.height}
        nodeLabel={(node: any) => `${node.name} (${node.commitCount} commits)`}
        linkLabel={(link: any) => `Correlation: ${(link.correlation * 100).toFixed(1)}% (${link.coCommitCount} co-commits)`}
        nodeAutoColorBy="commitCount"
        // Color links based on correlation strength
        linkColor={(link: any) => {
          // Use color intensity to represent correlation strength
          // Higher correlation = more vibrant color
          const intensity = Math.pow(link.correlation, 0.5) * 255; // Non-linear scaling for better visibility
          return `rgba(100, 100, ${intensity + 100}, 0.8)`;
        }}
        // Width based on correlation strength - thinner for large graphs
        linkWidth={(link: any) => Math.pow(link.correlation, 0.7) * (isLargeGraph ? 3 : 5)}
        // Add particles for visual interest - fewer for large graphs
        linkDirectionalParticles={(link: any) => isLargeGraph ?
          (link.correlation > 0.4 ? 2 : 0) : // Only show particles for strong links in large graphs
          Math.ceil(link.correlation * 4)
        }
        linkDirectionalParticleSpeed={(link: any) => link.correlation * 0.01}
        linkDirectionalParticleWidth={(link: any) => Math.pow(link.correlation, 0.7) * (isLargeGraph ? 2 : 4)}
        // Adjust simulation parameters for better stability
        d3AlphaDecay={0.01} // Slower decay to allow more time to find stable positions
        d3VelocityDecay={0.3} // Lower value to allow more movement
        cooldownTicks={300} // More ticks to ensure proper stabilization
        warmupTicks={200} // More warmup for better initial layout
        d3AlphaMin={0.001} // Lower alpha min for more precise positioning
        // Enable gravity to pull nodes toward the center
        d3Force={(d3, nodes) => {
          // Add additional forces if needed
          // This callback allows us to add custom forces
        }}
        linkCanvasObjectMode={() => 'after'}
        linkCanvasObject={(link: any, ctx, globalScale) => {
          // For large graphs, only show percentages on stronger correlations
          const correlationThreshold = isLargeGraph ? 0.3 : 0.1;

          if (link.correlation > correlationThreshold) {
            const start = link.source;
            const end = link.target;

            if (typeof start !== 'object' || typeof end !== 'object') return;

            // Calculate the midpoint position
            const midX = start.x + (end.x - start.x) / 2;
            const midY = start.y + (end.y - start.y) / 2;

            // Draw the correlation percentage text
            const label = `${(link.correlation * 100).toFixed(0)}%`;
            const fontSize = (isLargeGraph ? 8 : 10) / globalScale; // Smaller font for large graphs
            ctx.font = `${fontSize}px Sans-Serif`;

            // Create a background for the text
            const textWidth = ctx.measureText(label).width;
            const bckgDimensions = [textWidth, fontSize].map(n => n + fontSize * 0.2);

            ctx.fillStyle = 'rgba(0, 0, 0, 0.6)';
            ctx.fillRect(
              midX - bckgDimensions[0] / 2,
              midY - bckgDimensions[1] / 2,
              bckgDimensions[0],
              bckgDimensions[1]
            );

            ctx.textAlign = 'center';
            ctx.textBaseline = 'middle';
            ctx.fillStyle = 'rgba(255, 255, 255, 0.9)';
            ctx.fillText(label, midX, midY);
          }
        }}
        nodeCanvasObject={(node: any, ctx, globalScale) => {
          // For large graphs, use shorter labels
          const label = isLargeGraph
            ? node.name.split('/').pop()?.split('.')[0] || '' // Just the filename without extension
            : node.name.split('/').pop() || '';

          // Smaller font for large graphs
          const fontSize = (isLargeGraph ? 10 : 12) / globalScale;
          ctx.font = `${fontSize}px Sans-Serif`;
          const textWidth = ctx.measureText(label).width;
          const bckgDimensions = [textWidth, fontSize].map(n => n + fontSize * 0.2);

          // Darker background for better readability
          ctx.fillStyle = 'rgba(50, 50, 50, 0.8)';
          ctx.fillRect(
            node.x - bckgDimensions[0] / 2,
            node.y - bckgDimensions[1] / 2,
            bckgDimensions[0],
            bckgDimensions[1]
          );

          ctx.textAlign = 'center';
          ctx.textBaseline = 'middle';
          // Use white text for better contrast
          ctx.fillStyle = 'rgba(255, 255, 255, 0.9)';
          ctx.fillText(label, node.x, node.y);

          node.__bckgDimensions = bckgDimensions; // to re-use in nodePointerAreaPaint
        }}
        nodePointerAreaPaint={(node: any, color, ctx) => {
          ctx.fillStyle = color;
          const bckgDimensions = node.__bckgDimensions;
          bckgDimensions &&
            ctx.fillRect(
              node.x - bckgDimensions[0] / 2,
              node.y - bckgDimensions[1] / 2,
              bckgDimensions[0],
              bckgDimensions[1]
            );
        }}
      />
    </div>
  );
};

export default GraphVisualization;
