//Copyright (c) Microsoft Corporation. All rights reserved.
//Licensed under the MIT License.

let productId, productName, image;

$(document).ready(() => {
  $("#productStatus, #captureImage, #noProductFound, #status, #wait").hide();
  microsoftTeams.app.initialize().then(scanBarCode);
});

function scanBarCode() {
  const config = { timeOutIntervalInSec: 30 };

  microsoftTeams.media.scanBarCode((error, decodedText) => {
    if (error) {
      alert("ErrorCode: " + error.errorCode + (error.message || ""));
      return;
    }

    productId = decodedText;

    const knownProducts = {
      "01SD001": "Laptop",
      "01DU890": "Desktop",
      "01PM998": "Mobile",
    };

    productName = knownProducts[productId];

    if (productName) {
      $("#captureImage").show();
    } else {
      $("#noProductFound").show();
    }
  }, config);
}

let capturedFile = null;  

function selectMedia() {
    microsoftTeams.app.initialize().then(() => {
        const imageProps = {
            sources: [microsoftTeams.media.Source.Camera, microsoftTeams.media.Source.Gallery],
            startMode: microsoftTeams.media.CameraStartMode.Photo,
            ink: false,
            cameraSwitcher: false,
            textSticker: false,
            enableFilter: true,
        };

        const mediaInput = {
            mediaType: microsoftTeams.media.MediaType.Image,
            maxMediaCount: 1,
            imageProps: imageProps,
        };

        microsoftTeams.media.selectMedia(mediaInput, function (error, attachments) {
            if (error) {
                alert("Error: " + (error.message || error.errorCode));
            } else if (attachments && attachments.length > 0) {
                const base64Data = attachments[0].preview;
                const mimeType = "image/png"; 

                const byteString = atob(base64Data);
                const arrayBuffer = new ArrayBuffer(byteString.length);
                const intArray = new Uint8Array(arrayBuffer);
                for (let i = 0; i < byteString.length; i++) {
                    intArray[i] = byteString.charCodeAt(i);
                }

                capturedFile = new Blob([arrayBuffer], { type: mimeType });

                const objectURL = URL.createObjectURL(capturedFile);
                $("#productImg").attr("src", objectURL);
                $("#productStatus").show();
            }
        });
    });
}

function saveProduct(isApproved) {
    const status = isApproved ? "Approved" : "Rejected";
    const formData = new FormData();
    formData.append("productId", productId);
    formData.append("status", status);
    formData.append("image", capturedFile, `${productId}_captured_image.png`);

    $("#productStatus").hide();
    $("#captureImage").hide();
    $("#wait").show();

    fetch("/save", {
        method: "POST",
        body: formData,
    })
    .then((response) => {
        $("#wait").hide();
        if (!response.ok) throw new Error("Server error");
        $("#status").show();
    })
    .catch((error) => {
        console.error("Upload error:", error);
        alert("An error occurred while saving the product. Please try again.");
        $("#wait").hide();
    });
}