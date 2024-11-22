function getCookies() {
  document.getElementById("cookies").innerHTML = document.cookie;
  if (!cookieStore) {
    document.getElementById("cookieStore").innerHTML =
      "Cookie Store API not supported";
    return;
  }

  cookieStore.getAll().then((cookies) => {
    if (cookies.length === 0) {
      document.getElementById("cookieStore").innerHTML = "No cookies";
      return;
    }
    document.getElementById("cookieStore").innerHTML = JSON.stringify(
      cookies,
      null,
      2
    );
  });
}

function popOutApp() {
  window.open("https://{{DOMAIN-NAME}}/partitioned-cookies.html");
}

function setCookiesFromApi() {
  var xhr = new XMLHttpRequest();
  xhr.onreadystatechange = function () {
    if (xhr.readyState === 4) {
      getCookies();
    }
  };
  xhr.open("GET", "/cookies/samesite/set");
  xhr.send();
}

function setPartitionedCookiesFromApi() {
  var xhr = new XMLHttpRequest();
  xhr.onreadystatechange = function () {
    if (xhr.readyState === 4) {
      getCookies();
    }
  };
  xhr.open("GET", "/cookies/partitioned/set");
  xhr.send();
}

function setCookiesFromClient() {
  const key = document.getElementById("key")?.value;
  const cookieValue = document.getElementById("cookieValue")?.value;
  const secure = document.getElementById("secure")?.checked;
  const sameSite = document.getElementById("sameSite")?.value || null;
  const partitioned = document.getElementById("partitioned")?.checked;

  let cookie = `${encodeURIComponent(key)}=${encodeURIComponent(
    cookieValue
  )};max-age=${30 * 24 * 60 * 60}`;
  cookie += secure ? ";Secure" : "";
  cookie += sameSite ? `;SameSite=${sameSite}` : "";
  cookie += partitioned ? ";Partitioned" : "";

  document.cookie = cookie;

  getCookies();
}

function clearCookies() {
  var cookies = document.cookie.split(";");
  for (var i = 0; i < cookies.length; i++) {
    var cookie = cookies[i];
    var eqPos = cookie.indexOf("=");
    var name = (eqPos > -1 ? cookie.substring(0, eqPos) : cookie).trim();
    document.cookie =
      name +
      "=;SameSite=None;Secure;Partitioned;expires=Thu, 01 Jan 1970 00:00:00 GMT";
    document.cookie =
      name + "=;SameSite=None;Secure;expires=Thu, 01 Jan 1970 00:00:00 GMT";
    document.cookie =
      name + "=;SameSite=Non;expires=Thu, 01 Jan 1970 00:00:00 GMT";
    document.cookie = name + "=;Secure;expires=Thu, 01 Jan 1970 00:00:00 GMT";
    document.cookie = name + "=;expires=Thu, 01 Jan 1970 00:00:00 GMT";
  }
  getCookies();
}
