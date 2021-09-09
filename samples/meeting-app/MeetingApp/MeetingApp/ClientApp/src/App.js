import './App.css';
import {
  BrowserRouter as Router,
  Route,
  Switch
} from 'react-router-dom';
import Configuration from './components/configuration';
import RecruitingDetails from './components/recruiting-details/recruiting-details';

function App() {
  return (
    <Router>
      <Switch>
        <Route exact path='/configure' component={Configuration}></Route>
        <Route exact path='/details' component={RecruitingDetails}></Route>
      </Switch>
    </Router>
  );
}

export default App;
