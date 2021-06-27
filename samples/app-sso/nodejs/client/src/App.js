import React, { Suspense } from 'react';
import { BrowserRouter as Router, Route} from "react-router-dom";
import './App.css';
import Welcome from './components/Welcome';


function App() {

  return (
    <Suspense fallback="loading">
      <Router>
        <Route exact path="/tab" component={Welcome} />
      </Router>
    </Suspense>
  );
}

export default App;
