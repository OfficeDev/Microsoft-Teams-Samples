import React, { Suspense } from 'react';
import { BrowserRouter as Router, Route, Switch } from 'react-router-dom';  // Import Switch to handle multiple routes
import './App.css';
import Welcome from './components/welcome';

/**
 * The main application component that renders the routes and handles the rendering
 * of different components based on the URL path.
 */
function App() {
  return (
    <Suspense fallback={<div>Loading...</div>}> 
      <div className="App">
        <Router>
          <Switch> 
            <Route exact path="/" component={Welcome} />
          </Switch>
        </Router>
      </div>
    </Suspense>
  );
}

export default App;
