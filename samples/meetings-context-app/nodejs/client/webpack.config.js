devServer: {
  setupMiddlewares: (middlewares, devServer) => {
    if (!devServer) {
      throw new Error('webpack-dev-server is not defined');
    }

    // Replace old 'onBeforeSetupMiddleware'
    devServer.app.use((req, res, next) => {
      console.log('Custom before middleware');
      next();
    });

    // Replace old 'onAfterSetupMiddleware'
    devServer.app.use((req, res, next) => {
      console.log('Custom after middleware');
      next();
    });

    return middlewares;
  }
}
