function getCookies() {
    document.getElementById("cookies").innerHTML = document.cookie;
  }
  
  function setCookies() {
    var xhr = new XMLHttpRequest();
    xhr.onreadystatechange = function() {
      if (xhr.readyState === 4) {
        getCookies();
      }
    };
    xhr.open("GET", "/set");
    xhr.send();
  }

  function clearCookies() {
    var cookies = document.cookie.split(";");
    for (var i = 0; i < cookies.length; i++) {
      var cookie = cookies[i];
      var eqPos = cookie.indexOf("=");
      var name = eqPos > -1 ? cookie.substr(0, eqPos) : cookie;
      document.cookie = name + "=;SameSite=None;Secure;expires=Thu, 01 Jan 1970 00:00:00 GMT";
      document.cookie = name + "=;SameSite=Non;expires=Thu, 01 Jan 1970 00:00:00 GMT";
      document.cookie = name + "=;Secure;expires=Thu, 01 Jan 1970 00:00:00 GMT";
      document.cookie = name + "=;expires=Thu, 01 Jan 1970 00:00:00 GMT";
    }
    getCookies();
  }