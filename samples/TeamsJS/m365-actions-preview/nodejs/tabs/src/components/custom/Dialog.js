import * as React from "react";

import {
    Button,
    Dialog,
    DialogActions,
    DialogBody,
    DialogContent,
    DialogSurface,
    DialogTitle,
    DialogTrigger,
    Field,
    Spinner,
    Textarea
} from "@fluentui/react-components";

import { Dismiss24Regular } from "@fluentui/react-icons";
import { ToDoAttachment } from './Attachments';

const useStyles = {
    content: {
        display: "flex",
        flexDirection: "column",
        rowGap: "10px",
    },
};

export class TodoDialog extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            description: '',
            open: false,
            loader: false
        }
    }
    onTrigger = async (e) => {
        this.setState({ loader: true })
        e.preventDefault();
        await this.props.handleFormSubmit();
        this.setState({ loader: false, open: false })
    }
    handleInputChange(value) {
        this.props.handleDialogInputChange(value);
    }
    render() {
        const styles = useStyles;
        return (
            <Dialog onOpenChange={() => this.setState({ open: true })} open={this.state.open}>
                <DialogTrigger disableButtonEnhancement>
                    <Button appearance="primary">+ Add task </Button>
                </DialogTrigger>
                <DialogSurface >
                    {this.state.loader && <Spinner tabIndex={5} className="spinner" />}
                    <form onSubmit={this.onTrigger}>
                        <DialogBody>
                            <DialogTitle
                                action={
                                    <Button
                                        appearance="subtle"
                                        aria-label="close"
                                        icon={<Dismiss24Regular
                                            onClick={() => this.setState({ open: false })} />
                                        }
                                    />
                                }
                            > + Add Task
                            </DialogTitle>
                            <DialogContent className={styles.content}>
                                <Field size="large" label="Attachment: ">
                                    <ToDoAttachment
                                        teamsUserCredential={this.props.teamsUserCredential}
                                        scope={this.props.scope}
                                        getAttachmentId={this.props.getAttachmentId}
                                        action={true}
                                        itemId={this.props.itemId}
                                        userId={this.props.userInfo.objectId}
                                    />
                                </Field>
                                <Field size="large" label="Notes" style={{ marginTop: "20px" }}>
                                    <Textarea style={{ height: "100px" }} placeholder="Add your notes here" value={this.props.description} onChange={(event) => { this.handleInputChange(event.target.value) }} />
                                </Field>
                            </DialogContent>
                            <DialogActions>
                                <Button type="submit" appearance="primary">Submit</Button>
                            </DialogActions>
                        </DialogBody>
                    </form>
                </DialogSurface>
            </Dialog >
        );
    }
};