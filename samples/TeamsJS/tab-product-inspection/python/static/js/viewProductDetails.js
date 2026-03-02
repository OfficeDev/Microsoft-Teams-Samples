//Copyright (c) Microsoft Corporation. All rights reserved.
//Licensed under the MIT License.

$(document).ready(function () {
    $("#productStatus").hide();
});

microsoftTeams.app.initialize().then(() => {
    scanBarCode();
});

let productId;

function scanBarCode() {
    const config = {
        timeOutIntervalInSec: 30
    };

    microsoftTeams.media.scanBarCode((error, decodedText) => {
        if (error) {
            const msg = error.message ? `${error.errorCode}: ${error.message}` : `ErrorCode: ${error.errorCode}`;
            alert(msg);
        } else if (decodedText) {
            productId = decodedText;
            window.location.href = `${window.location.origin}/productDetails?productId=${productId}`;
        }
    }, config);
}
