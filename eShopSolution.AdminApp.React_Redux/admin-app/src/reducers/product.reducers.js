import { ACTION_TYPES } from "../const/actiontypes";

const initialState = {
    list: []
}

const ProductReducer = (state = initialState, action) => {
    switch (action.type) {
        case ACTION_TYPES.FETCH_ALL:
            {

                return {
                    ...state,
                    list: [...action.payload]
                };
            }

        default:
            break;
    }
}

export default ProductReducer;