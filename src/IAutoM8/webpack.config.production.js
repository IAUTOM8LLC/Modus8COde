const path = require('path');
const webpack = require('webpack');
const WebpackChunkHash = require('webpack-chunk-hash');

var commonConfig = require('./webpack.config.common');

module.exports = Object.assign(commonConfig, {
    devtool: false,
    entry: {
        bundle: './main.js'
    },
    output: {
        path: path.join(__dirname, 'wwwroot', 'dist'),
        publicPath: '/dist/',
        chunkFilename: '[name].[chunkhash:8].js',
        filename: '[name].[chunkhash:8].js'
    },
    plugins: commonConfig.plugins.concat([

        new WebpackChunkHash(),

        new webpack.HashedModuleIdsPlugin(),

        new webpack.optimize.ModuleConcatenationPlugin(),

        new webpack.optimize.UglifyJsPlugin({
            beautify: false,
            comments: false,
            sourceMap: true,
            compress: {
                warnings: false
            }
        })
    ])
});
