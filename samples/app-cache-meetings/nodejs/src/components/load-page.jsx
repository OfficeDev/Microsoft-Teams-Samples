import React, { Component } from "react";
import { Flex, FlexItem } from "@fluentui/react-northstar";
import * as microsoftTeams from "@microsoft/teams-js";
import "../style/style.css";

class LoadPage extends Component {
    constructor(props) {
        super(props)
        this.state = {
            test: "",
            result: "Successfully Loaded"
        }
    }
    componentDidMount() {
        microsoftTeams.app.initialize();
    }
    render() {
        return (
            <Flex>
                <FlexItem push>
                    <div className="tag-container">
                        <h3>App Cache Page Load From Teams </h3>
                        <li className="break"> Status : <b>{this.state.result}</b></li>
                    </div>
                </FlexItem>
            </Flex>
        )
    }
}

export default LoadPage