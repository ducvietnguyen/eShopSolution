import axios from 'axios'

const apiUrl = 'https://localhost:5001/api'

class api {
    constructor() {
        const token = JSON.parse(localStorage.getItem('user') || '{}')['token'];

        const api_instance = axios.create({
            baseURL: apiUrl,
            headers: { 'Authorization': `Bearer ${token}` }
        });

        this.axiosInstance = api_instance;
    }



    login = (loginRequest) => {
        const url = '/user/login';
        const dataForm = JSON.stringify(loginRequest);
        return this.axiosInstance.post(url, dataForm)
            .then((resp) => {
                localStorage.setItem('user', resp);
            })
            .catch((resp) => {
                if (resp.response !== undefined && resp.response.status == '401') {
                    localStorage.removeItem('user')
                    location.replace('/login')
                } else {
                    return Promise.reject(resp)
                }
            })
    }




fetch_all_product() {
    return this.axiosInstance.get(url)
        .then((res) => {
            return res;
        })
        .catch((res) => {
            if (resp.response !== undefined && resp.response.status == '401') {
                localStorage.removeItem('user');
                location.replace('/login');
            } else {
                return Promise.reject(res);
            }
        })

}

}

export default api;