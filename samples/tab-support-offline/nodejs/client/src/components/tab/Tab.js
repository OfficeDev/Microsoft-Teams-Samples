import React, { useState, createContext, Children } from 'react';
import TabBar from './TabBar';
import { TabContent } from './TabContent';
// Create a context for the active tab
export const TabContext = createContext({});

const Tab = ({ selectedFlight, children }) => {
    const [activeTab, setActiveTab] = useState(null);

    const handleTabChange = (tab) => {
        setActiveTab(tab);
    };

    const arrayOfChildren = Children.toArray(children);
    const tabs = arrayOfChildren.map((child, index) => {
        return child.props.tab;
    }
    );

    const shouldShowTabContent = (tab, index) => {
        if (activeTab) {
            return activeTab.key === tab.key
        }else {
            return index === 0;
        }
    }

    return (
        <div>
        <TabContext.Provider value={activeTab}>
        <TabBar tabs={tabs} onTabChange={handleTabChange} />
            {
             tabs.map((tab, index) => {
                return (
                    shouldShowTabContent(tab, index) ? <TabContent tab={tab} selectedFlight={selectedFlight}/> : null
                );   
            })}
        </TabContext.Provider>
        </div>
    );
};

export { Tab };