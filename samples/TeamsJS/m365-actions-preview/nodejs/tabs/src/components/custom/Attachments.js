import * as React from "react";

import {
    Card,
    CardHeader,
    CardPreview,
    Menu,
    MenuButton,
    MenuItem,
    MenuList,
    MenuPopover,
    MenuTrigger,
    Text,
    shorthands,
    tokens
} from "@fluentui/react-components";
import { getImageIcon, getSingleAttachmentElement } from '../helper/helper';

import MicrosoftGraph from '../helper/graph';
import { createMicrosoftGraphClientWithCredential } from "@microsoft/teamsfx";

const useStyles = {
    main: {
        ...shorthands.gap("36px"),
        display: "flex",
        flexDirection: "column",
        flexWrap: "wrap",
    },

    card: {
        maxWidth: "100%",
        height: "fit-content",
        columnGap: "unset"
    },

    section: {
        width: "fit-content",
    },

    title: {
        ...shorthands.margin(0, 0, "12px"),
    },

    horizontalCardImage: {
        width: "64px",
        height: "64px",
    },

    headerImage: {
        ...shorthands.borderRadius("4px"),
        maxWidth: "42px",
        maxHeight: "42px",
    },

    caption: {
        color: tokens.colorNeutralForeground3,
    },

    text: {
        ...shorthands.margin(0),
    },
};
export class ToDoAttachment extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            actionItem: undefined,
            noItemFoundError: "No Attachment found"
        };
    }

    async componentDidMount() {
        this.fetchData();
    }

    async componentDidUpdate(prevProps) {
        if (prevProps.attachmentId !== this.props.attachmentId) {
            this.fetchData();
        }
    }

    async fetchData() {
        try {
            let itemId = "";
            // Get Microsoft graph client
            const graphClient = createMicrosoftGraphClientWithCredential(
                this.props.teamsUserCredential,
                this.props.scope
            );

            itemId = this.props.attachmentId ? this.props.attachmentId : this.props.itemId;
            const msGraph = new MicrosoftGraph(graphClient, this.props.userId, itemId);

            const actionItem = await msGraph.readActionItem();
            this.setState({ actionItem: actionItem });

            if (this.props.getAttachmentId) {
                this.props.getAttachmentId(itemId);
            }

        } catch (error) {
            console.log("Attachment-fetch", error);
        }
    }
    render() {

        const styles = useStyles;
        return (
            <>
                {!this.state.actionItem && <div>{this.state.noItemFoundError}</div>}
                {this.state.actionItem && !this.props.singleAttachment &&
                    <div style={styles.main} >
                        <Card style={styles.card} orientation="horizontal" appearance="subtle">
                            <CardPreview style={styles.horizontalCardImage}>
                                <span style={{ display: "flex", alignItems: "center" }}>
                                    <img height={48} width={48} src={getImageIcon(this.state.actionItem.file.mimeType)} alt={this.state.actionItem.name} />
                                </span>
                            </CardPreview>
                            <CardHeader
                                header={<Text weight="semibold">{this.state.actionItem.name}</Text>}
                                description={
                                    <a href={this.state.actionItem.webUrl} target="_blank" style={styles.caption}>View</a>
                                }
                                action={this.props.action &&
                                    <Menu>
                                        <MenuTrigger disableButtonEnhancement>
                                            <MenuButton style={{ minWidth: "unset" }} appearance="subtle" menuIcon={null}>
                                                ...
                                            </MenuButton>
                                        </MenuTrigger>
                                        <MenuPopover>
                                            <MenuList>
                                                <MenuItem onClick={() => { this.setState({ actionItem: undefined }) }}>
                                                    Remove
                                                </MenuItem>
                                            </MenuList>
                                        </MenuPopover>
                                    </Menu>
                                }
                            />
                        </Card>
                    </div>
                }
                {this.state.actionItem && this.props.singleAttachment &&
                    getSingleAttachmentElement(this.state.actionItem)
                }
            </>
        );
    }
};