import React from 'react';
import * as microsoftTeams from '@microsoft/teams-js';
import { navItems } from "./Constants";

export interface INavProps {
}
interface INavState {
  menuId: string;
}
class Tab extends React.Component<INavProps, INavState> {
  constructor(props: any) {
    super(props);
    this.state = {
      menuId: "0"
    }
  }
  
  public async componentDidMount() {
    microsoftTeams.menus.initialize();
    this.navBarMenu();
  }

  public navBarMenu = async () => {
    microsoftTeams.menus.setNavBarMenu(navItems, (id: string) => {
      this.setState({ menuId: id })
      return true;
    });
   }
   
  public render() {
    let content;
    if(this.state.menuId === "0"){
      content = <h1>Dashboard</h1>
    }else if(this.state.menuId === "1"){
      content = <h1>Home</h1>
    }else if(this.state.menuId === "2"){
      content = <h1>News</h1>
    }else if(this.state.menuId === "3"){
      content = <h1>Contact</h1>
    }else  if(this.state.menuId === "4"){
      content = <h1>About</h1>
    }
    return (
      <>
        <p>{content}</p>
      </>
    );
  }
  }
export default Tab;