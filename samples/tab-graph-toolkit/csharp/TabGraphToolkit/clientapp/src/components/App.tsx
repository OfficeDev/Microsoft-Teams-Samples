import './App.css';
import {
    BrowserRouter as Router,
    Route,
    Switch
} from 'react-router-dom';
import Home from './Home';
import './App.css';

function App() {

    return (
        <div className="App">
            <Router>
                <Switch>
                    <Route exact path='/tab' component={Home}></Route>
                </Switch>
            </Router>
        </div>
    );
}

export default App;