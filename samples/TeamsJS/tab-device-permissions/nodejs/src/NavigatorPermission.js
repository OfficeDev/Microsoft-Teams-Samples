// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. 
function CheckAndAlertForCameraPermission() {
    navigator.permissions.query({ name: "camera" }).then(function (result) {
      if (result.state === 'denied') {
          alert("failed");
      }
      else {
          console.log("result is" + result);
          alert("success");
      }
  });
} 
export default CheckAndAlertForCameraPermission;