import api from './api';
import ACTION_TYPES from '../const/actiontypes';

export const fetch_all_product = () => {

    var user = localStorage.getItem('user');
    if (!user) {
        api.login({ username: 'Admin01', password: 'Admin01@gmail.com' });
    }


    api.fetch_all_product()
        .then((res) => {
            dispatch({
                type: ACTION_TYPES.CREATE,
                payload: res.data
            });
        })
        .catch((res) => {
            console.log(res);

        });
}



