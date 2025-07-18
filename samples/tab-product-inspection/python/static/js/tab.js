//Copyright (c) Microsoft Corporation. All rights reserved.
//Licensed under the MIT License.

$(document).ready(() => {
  microsoftTeams.app.initialize();

  const origin = window.location.origin;

$("#desktop").attr("src", `${window.location.origin}/Images/01DU890.png`);
$("#laptop").attr("src", `${window.location.origin}/Images/01SD001.png`);
$("#mobile").attr("src", `${window.location.origin}/Images/01PM998.png`);


  $("#btnInspectProduct").on("click", () => {
    const taskInfo = {
      title: "Scan Product",
      url: `${origin}/scanProduct`,
      height: 510,
      width: 430,
    };
    microsoftTeams.dialog.url.open(taskInfo, (err, result) => {
      console.log("Submit handler - err:", err);
    });
  });

  $("#btnViewProduct").on("click", () => {
    const taskInfo = {
      title: "View product",
      url: `${origin}/viewProductDetail`,
      height: 510,
      width: 430,
    };
    microsoftTeams.dialog.url.open(taskInfo, (err, result) => {
      console.log("Submit handler - err:", err);
    });
  });
});
