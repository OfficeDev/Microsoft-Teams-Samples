function openWindow(url, source, width, height) {
  var callback = function() {
    window.location.reload(true);
  };

    microsoftTeams.authentication.authenticate({
      url: url,
      width: width,
      height: height,
      successCallback: callback,
      failureCallback: callback,
    });
 
}

function signInWithGithub(event,source) {
  event.preventDefault();
  openWindow("/auth/github", source, 450, 650);
}