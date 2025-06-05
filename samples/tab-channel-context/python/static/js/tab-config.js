microsoftTeams.app.initialize().then(() => {

  microsoftTeams.pages.config.registerOnSaveHandler((saveEvent) => {
    const baseUrl = `${window.location.protocol}//${window.location.hostname}:${window.location.port}`;

    microsoftTeams.pages.config.setConfig({
      suggestedDisplayName: "Channel Tab",
      entityId: "Test",
      contentUrl: `${baseUrl}/tab`,
      websiteUrl: `${baseUrl}/tab`
    });

    saveEvent.notifySuccess();
  });

  microsoftTeams.pages.config.setValidityState(true);
});
