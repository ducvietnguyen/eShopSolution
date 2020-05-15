import { createStore, applyMiddleware, compose } from 'redux';
import ReduxThunk from 'redux-thunk';
import ProductReducer from '../reducers/index';


export const store = createStore(ProductReducer, compose(applyMiddleware(ReduxThunk)))
