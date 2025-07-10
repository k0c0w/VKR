const CracoAlias = require("craco-alias");

module.exports = {
    plugins: [
        {
            plugin: CracoAlias,
            options: {
                source: "tsconfig",
                // baseUrl SHOULD be specified
                // plugin does not take it from tsconfig
                baseUrl: "./src",
                tsConfigPath: "./tsconfig.extend.json"
            }
        }
    ],
    webpack: {
        configure: (webpackConfig) => {
            return {
                ...webpackConfig,
                resolve: {
                  ...webpackConfig.resolve,
                  fallback: {
                    ...webpackConfig.resolve.fallback,
                    fs: false,
                    path: require.resolve("path-browserify")
                  }
                }
            };
        }
    },
};