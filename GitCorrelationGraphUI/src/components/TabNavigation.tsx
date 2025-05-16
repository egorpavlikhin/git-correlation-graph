import React from 'react';

export type TabType = 'graph' | 'clusters';

interface TabNavigationProps {
  activeTab: TabType;
  onTabChange: (tab: TabType) => void;
}

const TabNavigation: React.FC<TabNavigationProps> = ({ activeTab, onTabChange }) => {
  return (
    <div className="tab-navigation">
      <button 
        className={`tab-button ${activeTab === 'graph' ? 'active' : ''}`}
        onClick={() => onTabChange('graph')}
      >
        Graph View
      </button>
      <button 
        className={`tab-button ${activeTab === 'clusters' ? 'active' : ''}`}
        onClick={() => onTabChange('clusters')}
      >
        Clusters View
      </button>
    </div>
  );
};

export default TabNavigation;
