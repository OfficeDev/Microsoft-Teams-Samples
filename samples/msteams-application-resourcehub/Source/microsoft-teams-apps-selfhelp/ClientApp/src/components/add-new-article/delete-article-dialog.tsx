// <copyright file="delete-channel-dialog.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Text, Flex, FlexItem, Button } from "@fluentui/react-northstar";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";

class DeleteArticleDialog extends React.Component<WithTranslation> {
    localize: TFunction;
    constructor(props: any) {
        super(props);
        this.localize = this.props.t;
    }

    confirmDelete = (value) => {
        const confirmMessage = {
            confirm: value,
        }

        microsoftTeams.tasks.submitTask(JSON.stringify(confirmMessage));
        return true;
    }

    /**
    * Renders the component
    */
    public render(): JSX.Element {
        return (
            <div className="delete-container-div-preview">
                <Flex column gap="gap.small">
                    <Flex hAlign="center" className="space-delete-message" column>
                        <Text weight="bold"
                            content={this.localize("deleteArticleText")}
                            size="medium"
                        />
                    </Flex>
                    <Flex column gap="gap.small" hAlign="center">
                        <FlexItem push>
                            <Flex gap="gap.medium">
                            <Button secondary content={this.localize("cancelButton")} onClick={() => this.confirmDelete(false)} />
                            <Button primary content={this.localize("confirmButton")} onClick={() => this.confirmDelete(true)} />
                        </Flex>
                        </FlexItem>
                    </Flex>
                </Flex>
            </div >
        )
    }
}

export default withTranslation()(DeleteArticleDialog)