const path = require('path');
const nodeExternals = require('webpack-node-externals');

module.exports = {
  mode: 'production', //add this line here
  entry: './index.js',
  output: {
    path: path.resolve(__dirname, 'dist'),
    filename: 'bundle.js'
  },
  target: 'node', // use require() & use NodeJs CommonJS style
  externals: [nodeExternals()], // in order to ignore all modules in node_modules folder
  externalsPresets: {
      node: true // in order to ignore built-in modules like path, fs, etc. 
  },
};