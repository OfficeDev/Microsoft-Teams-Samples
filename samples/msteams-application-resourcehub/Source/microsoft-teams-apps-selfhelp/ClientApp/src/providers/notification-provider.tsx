// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { createContext, useContext, useReducer } from "react";
import IArticleCheckBox from "../models/articleCheckBox";
const SET_SELECTED_CHECKBOX = "SET_SELECTED_CHECKBOX";
interface SetSelectedCheckBoxAction
{
    type: typeof SET_SELECTED_CHECKBOX;
    payload: IArticleCheckBox[] 
}
type IArticleAction =
SetSelectedCheckBoxAction;
type IArticleState ={
  notificationCheckBoxList : IArticleCheckBox[] 
}
type IContextModel ={
    state:IArticleState;
    setCheckBoxNotificationList:(checkboxList:IArticleCheckBox[]) =>void;
    setClearCheckBoxList:()=>void
}
const iState: IArticleState = {
  notificationCheckBoxList:[],
}
const Context = createContext({} as IContextModel);
const reducer = (state: IArticleState, action: IArticleAction): IArticleState => {
  alert(state.notificationCheckBoxList.length);
  console.log(state.notificationCheckBoxList);
  switch (action.type) {
  case SET_SELECTED_CHECKBOX:
    return {
      notificationCheckBoxList: action.payload,
    };
    default:
      return state;
  }
 
}
const NotificationProvider: React.FC = ({ children }) => {
  const [state, dispatch] = useReducer(
    reducer,
    { ...iState },
  );
  const setCheckBoxNotificationList = (checkboxList:IArticleCheckBox[]) => {
    dispatch({ type: SET_SELECTED_CHECKBOX, payload: checkboxList });
  };
  const setClearCheckBoxList = () => {
    dispatch({ type: SET_SELECTED_CHECKBOX, payload:[] });
  };
  return (
    <Context.Provider
      value={{
        state,
        setCheckBoxNotificationList,
        setClearCheckBoxList
      }}
    >
      {children}
    </Context.Provider>
  );
};
const useNotificationContextProvider = (): IContextModel => useContext(Context);
export { NotificationProvider, useNotificationContextProvider };