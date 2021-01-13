const HtmlWebPackPlugin = require('html-webpack-plugin');
const path = require('path');
const CopyPlugin = require('copy-webpack-plugin');

module.exports = {
   context: __dirname,
   entry: [
      './src/index.js'
   ],
   output: {
      path: path.resolve(__dirname, 'dist'),
      filename: 'index.js',
      publicPath: '/',
   },
   devServer: {
      historyApiFallback: true,
      proxy: {
         '/api': 'http://localhost:3978'
      },
      hot: true,
      port: 3003
   },
   module: {
      rules: [
         {
            test: /\.js$/,
            use: 'babel-loader',
         },
         {
            test: /\.css$/,
            use: ['style-loader', 'css-loader'],
         }, {
            test: /\.(png|j?g|svg|gif|ico)?$/,
            use: 'file-loader'
         }
      ]
   },
   plugins: [
      new HtmlWebPackPlugin({
         template: path.resolve(__dirname, 'src/index.html'),
         filename: 'index.html'
      }),
      new CopyPlugin({
         patterns: [
            { from: 'public' },
         ],
      }),
   ]
};