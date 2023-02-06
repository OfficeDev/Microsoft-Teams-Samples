// <copyright file="helper.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import { TFunction } from "i18next";

export const isNullorWhiteSpace = (input: string): boolean => {
    return !input || !input.trim();
}

/**
* get initial of user names to show in avatar.
*/
export const getInitials = (userPostName : string) => {
    let fullName = userPostName;
    let names = fullName.split(' '),
        initials = names[0].substring(0, 1).toUpperCase();

    if (names.length > 1) {
        initials += names[names.length - 1].substring(0, 1).toUpperCase();
    }
    return initials;
}

export const isGuid = (stringToTest) => {
    if (stringToTest[0] === "{") {
        stringToTest = stringToTest.substring(1, stringToTest.length - 1);
    }
    var regexGuid = /^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$/gi;
    return regexGuid.test(stringToTest);
}