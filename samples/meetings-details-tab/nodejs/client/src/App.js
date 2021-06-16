import React, { Suspense } from 'react';
import { BrowserRouter as Router, Route} from "react-router-dom";
import Welcome from './components/welcome';
import Config from './components/Config';
import Detail from './components/Detail';
import Result from './components/Result';

function App() {
  return (
    <Suspense fallback="loading">
      <div className="App">
      <Router>
        <Route exact path="/configuretab" component={Config} />
        <Route exact path="/" component={Welcome} />
        <Route exact path="/detail" component={Detail} />
        <Route exact path="/result" component={Result} />
      </Router>
      </div>
    </Suspense>
  );
}

export default App;
