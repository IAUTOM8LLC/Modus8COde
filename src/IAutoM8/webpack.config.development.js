const path = require('path');
const webpack = require('webpack');

var commonConfig = require('./webpack.config.common');

module.exports = Object.assign(commonConfig, {
    entry: {
        bundle: [
            'react-hot-loader/patch',
            './main.js'
        ]
    },
    output: {
        path: path.join(__dirname, 'wwwroot', 'dist'),
        publicPath: '/dist/',
        chunkFilename: '[name].js',
        filename: '[name].js'
    },
    plugins: commonConfig.plugins.concat([

        // generate sourcemaps for our code only
        new webpack.SourceMapDevToolPlugin({
            test: /\.(js|jsx)($|\?)/i,
            filename: '[file].map',
            exclude: ['vendor.js', 'manifest.js']

        }),

        // prints more readable module names in the browser console on HMR updates
        new webpack.NamedModulesPlugin()
    ])
});
