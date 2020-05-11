import React from 'react';
import './App.css';
import { store } from './actions/store';
import { Provider } from 'react-redux';
import DUserForm from './components/ProductForm';
import DUsers from './components/Products';

function App() {
  return (
    <Provider store={store}>
      <DUserForm></DUserForm>
      <DUsers></DUsers>
    </Provider>
  );
}

export default App;
