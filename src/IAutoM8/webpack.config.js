var isProd = process.env.ASPNETCORE_ENVIRONMENT === "production";

var config = null;

if (isProd) {
    console.log('Using Production build');
    config = require('./webpack.config.production');
}
else {
    console.log('Using Development build');
    config = require('./webpack.config.development');
}

module.exports = config;
