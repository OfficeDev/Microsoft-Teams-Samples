const path = require('path');
const webpack = require('webpack');

module.exports = {
  entry: './samples/app-auth/nodejs/views/entra-script.js',
  output: {
    filename: 'entra-bundle.js',
    path: path.resolve(__dirname, 'samples/app-auth/nodejs/public'),
    publicPath: '/samples/app-auth/nodejs/public/',
  },
  mode: 'development',
  module: {
    rules: [
      {
        test: /\.js$/,
        exclude: /node_modules/,
        use: {
          loader: 'babel-loader',
          options: {
            presets: ['@babel/preset-env'],
          },
        },
      },
    ],
  },
  devtool: 'source-map',
  devServer: {
    static: {
      directory: path.join(__dirname, 'samples/app-auth/nodejs/public'),
    },
    hot: true,
    port: 3001,
    devMiddleware: {
      publicPath: '/samples/app-auth/nodejs/public/',
    },
    headers: {
      'Access-Control-Allow-Origin': '*',
    },
  },
  plugins: [
    new webpack.HotModuleReplacementPlugin(),
  ],
};
