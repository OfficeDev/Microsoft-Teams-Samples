import { Button } from "@fluentui/react-components";
import Profile from "./Profile";

export const Login = (props) => {
    return (
        <div className="auth">
            <Profile userInfo={props.userInfo} />
            <h2>Welcome to To Do List App!</h2>
            <Button appearance="primary" onClick={() => props.loginBtnClick()}>
                Login
            </Button>
        </div>
    )
}