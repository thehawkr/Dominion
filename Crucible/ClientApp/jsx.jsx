import React from 'react';
import { BrowserRouter as Router, Route, Switch } from 'react-router-dom';
import FileUpload from './FileUpload';

const App = () => {
    return (
        <Router>
            <Switch>
                <Route path="/upload" component={FileUpload} />
            </Switch>
        </Router>
    );
};

export default App;
