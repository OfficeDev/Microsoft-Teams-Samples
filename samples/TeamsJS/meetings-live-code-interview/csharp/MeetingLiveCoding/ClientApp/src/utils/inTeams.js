/*!
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */

export function inTeams() {
    const url = new URL(window.location);
    const params = url.searchParams;
    return !!params.get("inTeams");
}