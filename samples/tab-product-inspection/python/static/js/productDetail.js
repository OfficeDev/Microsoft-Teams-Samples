//Copyright (c) Microsoft Corporation. All rights reserved.
//Licensed under the MIT License.

document.addEventListener("DOMContentLoaded", function () {
  microsoftTeams.app.initialize();

  const urlParams = new URLSearchParams(window.location.search);
  const productId = urlParams.get("productId");

  if (!productId) {
    document.getElementById("noProductFound").innerText = "Missing product ID in URL.";
    document.getElementById("productDetails").style.display = "none";
    return;
  }

  fetch(`/api/productDetails?productId=${productId}`)
    .then(res => res.json())
    .then(productDetails => {
      if (!productDetails || !productDetails.productId) {
        document.getElementById("noProductFound").innerText = "No product found for this ID.";
        document.getElementById("productDetails").style.display = "none";
      } else if (!productDetails.image) {
        document.getElementById("noProductFound").innerText = "No product image found.";
        document.getElementById("productDetails").style.display = "none";
      } else {
        document.getElementById("productName").innerText = productDetails.productName;
        document.getElementById("productStatus").innerText = productDetails.status;
        document.getElementById("productImg").src = productDetails.image;
      }
    })
    .catch(err => {
      console.error("Error loading product details:", err);
      document.getElementById("noProductFound").innerText = "Error fetching product data.";
    });

  document.getElementById("btnBack").addEventListener("click", function () {
    microsoftTeams.dialog.url.submit();
  });
});
