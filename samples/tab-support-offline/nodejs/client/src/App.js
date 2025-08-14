import './style/App.css';
import { AppTabs } from './models/AppTabsModel'
import { Tab } from './components/tab/Tab';
import { TabContent } from './components/tab/TabContent';
import { useState } from 'react';

const App = () => {
  // State to manage the selected flight
  const [selectedFlight, setSelectedFlight] = useState('A001');

  // Function to handle dropdown change
  const handleFlightChange = (event) => {
    setSelectedFlight(event.target.value);
  };

  return (
    <div className="app-container">
      <div>
        <label htmlFor="flight">Flight</label>
        <select id="flight" className='flight-select' value={selectedFlight} onChange={handleFlightChange}>
          <option value="A001">A001</option>
          <option value="A002">A002</option>
          <option value="A003">A003</option>
        </select>
      </div>
      <Tab selectedFlight={selectedFlight}>
        {AppTabs.map((tab, index) => (
          <TabContent tab={tab} key={index}>
          </TabContent>
        ))}
      </Tab>
    </div>
  );
};

export default App;
