// <copyright file="shareview.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { Component } from 'react'
class ShareView extends Component {
    constructor(props) {
        super(props);
        this.state = { seconds: 0 };
    }
    componentDidMount() {
        this.timer();
    }
    onStart = () => {
        this.setState({ seconds: this.state.seconds + 1 });
    }
    timer = () => {
        this.f = setInterval(this.onStart, 1000);
    }
    render() {
        return (
            <div className="timerCount">
                <h1>{this.state.seconds}</h1>
            </div>
        )
    }
}
export default ShareView