import './App.css';
import {
  BrowserRouter as Router,
  Route,
  Switch
} from 'react-router-dom';
import Configuration from './components/configuration';
import RecruitingDetails from './components/recruiting-details/recruiting-details';
import AddQuestions from './components/recruiting-details/questions/add-questions';
import AddNotes from './components/recruiting-details/notes/add-notes';
import ShareAssets from './components/recruiting-details/share-assets/share-assets';

function App() {
  return (
    <Router>
      <Switch>
        <Route exact path='/configure' component={Configuration}></Route>
        <Route exact path='/details' component={RecruitingDetails}></Route>
        <Route exact path='/questions' component={AddQuestions}></Route>
        <Route exact path='/addNote' component={AddNotes}></Route>
        <Route exact path='/shareAssets' component={ShareAssets}></Route>
      </Switch>
    </Router>
  );
}

export default App;
