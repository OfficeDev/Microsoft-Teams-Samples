import React, { useEffect, useState } from 'react';

function TabBar({ tabs, onTabChange }) {
  const [activeTab, setActiveTab] = useState(null);

  const isSelected = (tab, index) => {
    if (activeTab) {
    console.log("... isSelected: activeTab.key: " + activeTab.key);
      return activeTab.key === tab.key;
    }
    return index === 0;
  }

  return (
    <div className="tabs">
      {
        tabs.map((tab, index) => {
          return (
            <button
            key={index}
            className={isSelected(tab, index) ? 'active-tab' : 'tab'}
            onClick={() => handleTap(tab)}>
            {tab.title}
          </button>
          );
        })
        }
    </div>
  );

  function handleTap(tab) {
    setActiveTab(tab);
    onTabChange(tab);
  }
}

export default TabBar;