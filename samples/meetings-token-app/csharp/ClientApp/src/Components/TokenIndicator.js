import React, { Component } from 'react';
import { Text } from '@fluentui/react-northstar';

class TokenIndicator extends Component {
    render() {
        return this.props.show ?
            (
                <div className="token-indicator-container" >
                    <Text className="token-indicator-title" size="medium" success>
                        {this.props.title}
                    </Text>
                    <Text className="token-indicator-value" size="large" >
                        {this.props.value || "N/A"}
                    </Text>
                </div>
            ) : null


    }
}

export default TokenIndicator;