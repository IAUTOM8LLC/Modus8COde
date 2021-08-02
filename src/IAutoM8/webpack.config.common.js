const path = require('path');
const webpack = require('webpack');
const ExtractTextPlugin = require('extract-text-webpack-plugin');
const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin;

const analyze = !!process.env.ANALYZE_ENV;
const isProd = process.env.ASPNETCORE_ENVIRONMENT === 'staging' || process.env.ASPNETCORE_ENVIRONMENT === 'production';

const root = path.join(__dirname, './ClientApp');

const cssLoader = {
    loader: 'css-loader',
    options: {
        minimize: isProd
    }
};

function resolve(dir) {
    return path.join(root, dir);
}

const config = {
    cache: true,
    stats: {
        children: false
    },
    context: path.resolve(root),
    resolve: {
        extensions: ['.js', '.jsx'],
        alias: {
            // custom aliases to simplify imports
            '@components': resolve('components'),
            '@containers': resolve('containers'),
            '@store': resolve('redux/modules'),
            '@selectors': resolve('redux/selectors'),
            '@infrastructure': resolve('infrastructure'),
            '@styles': resolve('styles'),
            '@utils': resolve('utils'),
            '@constants': resolve('constants')
        }
    },
    plugins: [
        new ExtractTextPlugin({
            filename: '[name].[contenthash:8].css',
            disable: !isProd
        }),

        new webpack.DefinePlugin({
            'process.env.NODE_ENV': isProd ? '"production"' : '"development"',
            __DEV__: !isProd
        }),

        // Move everything from node_modules folder into separate vendor chunks (js and css).
        new webpack.optimize.CommonsChunkPlugin({
            name: 'vendor',
            minChunks: function (module) {
                return module.context && module.context.indexOf('node_modules') !== -1;
            }
        }),

        // Move the webpack bootstrap logic into its own js file.
        new webpack.optimize.CommonsChunkPlugin({
            name: 'manifest',
            minChunks: Infinity
        }),

        // hack to make moment.js to not load all locales
        new webpack.IgnorePlugin(/^\.\/locale$/, /moment$/)
    ],
    module: {
        loaders: [
            {
                enforce: 'pre',
                test: /\.(js|jsx)$/,
                exclude: /node_modules/,
                loader: 'eslint-loader'
            },
            {
                test: /\.(js|jsx)$/,
                include: path.resolve(root),
                exclude: /node_modules/,
                loader: 'babel-loader?cacheDirectory'
            },

            {
                test: /\.css$/,
                use: ExtractTextPlugin.extract({
                    fallback: 'style-loader',
                    use: cssLoader
                })
            },

            // for .less files:
            {
                test: /\.less$/,
                use: ExtractTextPlugin.extract({
                    fallback: 'style-loader',
                    use: [
                        cssLoader,
                        'less-loader'
                    ]
                }),
                exclude: [/[\/\\]node_modules[\/\\]semantic-ui-less[\/\\]/]
            },

            // for semantic-ui-less files:
            {
                test: /\.less$/,
                use: ExtractTextPlugin.extract({
                    fallback: 'style-loader',
                    use: [
                        cssLoader,
                        {
                            loader: 'semantic-ui-less-module-loader',
                            // you can also add specific options:
                            options: {
                                siteFolder: resolve('semantic/site'),
                                themeConfigPath: resolve('semantic/theme.config')
                            }
                        }
                    ]
                }),
                include: [/[\/\\]node_modules[\/\\]semantic-ui-less[\/\\]/]
            },

            // loader for static assets
            {
                test: /\.(png|jpg|jpeg|gif|svg)$/,
                use: {
                    loader: 'file-loader',
                    options: {
                        limit: 10240,
                        name: '[name]-[hash:7].[ext]'
                    }
                },
                include: [
                    root,
                    /[\/\\]node_modules[\/\\]semantic-ui-less[\/\\]/,
                    /[\/\\]node_modules[\/\\]react-widgets[\/\\]/,
                ]
            },
            {
                test: /\.(woff|woff2|ttf|eot)$/,
                use: {
                    loader: 'url-loader',
                    options: {
                        limit: 10240,
                        name: '[name]-[hash:7].[ext]'
                    }
                },
                include: [
                    root,
                    /[\/\\]node_modules[\/\\]semantic-ui-less[\/\\]/,
                    /[\/\\]node_modules[\/\\]react-widgets[\/\\]/,
                ]
            }
        ]
    }
};

if (analyze) {
    config.plugins.push(new BundleAnalyzerPlugin({
        analyzerMode: 'static',
        openAnalyzer: false
    }));
}

module.exports = config;
