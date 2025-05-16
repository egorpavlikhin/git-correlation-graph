import React from 'react';
import { FilterSettings } from '../models/GraphTypes';

interface FilterControlsProps {
  filters: FilterSettings;
  onFiltersChange: (filters: FilterSettings) => void;
}

const FilterControls: React.FC<FilterControlsProps> = ({ filters, onFiltersChange }) => {
  const handleMinCommitCountChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const value = parseInt(event.target.value);
    onFiltersChange({
      ...filters,
      minCommitCount: isNaN(value) ? 0 : value
    });
  };

  const handleMaxConnectionsChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const value = parseInt(event.target.value);
    onFiltersChange({
      ...filters,
      maxConnections: isNaN(value) ? 100 : value
    });
  };

  const handleMinCorrelationChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const value = parseFloat(event.target.value) / 100; // Convert from percentage to decimal
    onFiltersChange({
      ...filters,
      minCorrelation: isNaN(value) ? 0 : value
    });
  };

  // Convert decimal to percentage for display
  const correlationPercentage = Math.round(filters.minCorrelation * 100);

  return (
    <div className="filter-controls">
      <h3>Filter Options</h3>
      <div className="filter-control">
        <label htmlFor="min-commit-count">
          Minimum Commit Count: {filters.minCommitCount}
        </label>
        <input
          id="min-commit-count"
          type="range"
          min="0"
          max="20"
          value={filters.minCommitCount}
          onChange={handleMinCommitCountChange}
        />
      </div>
      <div className="filter-control">
        <label htmlFor="max-connections">
          Maximum Connections: {filters.maxConnections}
        </label>
        <input
          id="max-connections"
          type="range"
          min="1"
          max="100"
          value={filters.maxConnections}
          onChange={handleMaxConnectionsChange}
        />
      </div>
      <div className="filter-control">
        <label htmlFor="min-correlation">
          Minimum Correlation: {correlationPercentage}%
        </label>
        <input
          id="min-correlation"
          type="range"
          min="0"
          max="100"
          value={correlationPercentage}
          onChange={handleMinCorrelationChange}
        />
      </div>
    </div>
  );
};

export default FilterControls;
