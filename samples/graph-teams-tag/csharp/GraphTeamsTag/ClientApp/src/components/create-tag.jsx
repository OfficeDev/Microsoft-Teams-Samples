// <copyright file="create-tag.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { useEffect } from "react";
import { Text, Flex, FlexItem, Button } from "@fluentui/react-northstar";
import * as microsoftTeams from "@microsoft/teams-js";
import "../style/style.css";

// This page allow user to create new tag in task module.
const CreateTag = props => {
    return (
        <Flex className="container" vAlign="center">
           <Text content="Team Tag Management" size="larger" weight="semibold"/>
           <FlexItem push>
                <Button primary content="Create new Tag" />
            </FlexItem> 
        </Flex>
    )
}

export default CreateTag;