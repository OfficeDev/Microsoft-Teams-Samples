import React, { Component } from 'react';
import { Text } from '@fluentui/react-components';

class TokenIndicator extends Component {
    render() {
        return this.props.show ?
            (
                <div className="token-indicator-container" >
                    <Text className="token-indicator-title" variant="mediumPlus" color="green">
                        {this.props.title}
                    </Text>
                    <Text className="token-indicator-value" variant="xLargePlus" >
                        {this.props.value || "N/A"}
                    </Text>
                </div>
            ) : null
    }
}

export default TokenIndicator;