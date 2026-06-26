import './FilteredResult.css';

import { Button, Text } from "@fluentui/react-components";

import { ActionObjectType } from "@microsoft/teams-js";
import React from "react";
import { ToDoAttachment } from './Attachments';

class FilteteredResult extends React.Component {
    render() {
        return (
            <>
                {this.props.actionObjectsType && this.props.actionObjectsType === ActionObjectType.M365Content && (
                    <div className='filteredResult'>
                        <div className='filteredHeader'>
                            <Text as="h4" style={{ margin: "5px 0" }} size={500} weight='bold'>Filtered Result:</Text>
                            <Button className="filteredHeaderBtn" appearance='primary' onClick={this.props.clearFilter}>
                                Clear Filter
                            </Button>
                        </div>
                        <div className='filteredBody'>
                            <div className='filteredAttachment'>
                                {this.props.count > 0 &&
                                    <Text className='filteredText'>{`Found ${this.props.count} tasks`}</Text>
                                }
                                <ToDoAttachment
                                    teamsUserCredential={this.props.teamsUserCredential}
                                    scope={this.props.scope}
                                    getAttachmentId={this.props.getAttachmentId}
                                    itemId={this.props.itemId}
                                    userId={this.props.objectId}
                                />
                            </div>
                        </div>
                    </div>)
                }
            </>
        );

    }
}

export default FilteteredResult;