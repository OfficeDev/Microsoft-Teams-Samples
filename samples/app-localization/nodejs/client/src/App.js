import React, { Suspense } from 'react';
import { BrowserRouter as Router, Route} from "react-router-dom";
import './App.css';
import Welcome from './components/welcome';


function App() {

  return (
    <Suspense fallback="loading">
      <div className="App">
      <Router>
        <Route exact path="/" component={Welcome} />
      </Router>
      </div>
    </Suspense>
  );
}

export default App;
