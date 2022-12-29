import React from 'react';
import * as microsoftTeams from '@microsoft/teams-js';
const navItems = [
  {
    id: '1',
    title: 'Home',
    icon: "there is an <svg></svg> tag here but I shortened it for easier reading",
    enabled: true,
    viewData: null as any,
    selected: false,
  },
  {
    id: '2',
    title: 'News',
    icon: "there is an <svg></svg> tag here but I shortened it for easier reading",
    enabled: true,
    viewData: null as any,
    selected: false,
  },
  {
    id: '3',
    title: 'Contact',
    icon: "there is an <svg></svg> tag here but I shortened it for easier reading",
    enabled: true,
    viewData: null as any,
    selected: false,
  },
  {
    id: '4',
    title: 'About',
    icon: "there is an <svg></svg> tag here but I shortened it for easier reading",
    enabled: true,
    viewData: null as any,
    selected: false,
  }
];
export interface INavProps {
}
interface INavState {
  menuId: string;
}
class TermsOfUse extends React.Component<INavProps, INavState> {
  constructor(props: any) {
    super(props);
    this.state = {
      menuId: "0"
    }
  }
  public async componentDidMount() {
    microsoftTeams.app.initialize();
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
      content = <h1>Dashboard-Q</h1>
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
export default TermsOfUse;