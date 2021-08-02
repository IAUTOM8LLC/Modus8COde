// IE 11, Edge
import 'core-js/fn/object/assign'
import 'core-js/fn/number/is-nan'
import 'core-js/es6/string'
import 'core-js/es6/promise'
import 'core-js/fn/promise/finally'
import 'core-js/es6/array'
import 'core-js/es7/array'

if (typeof window.URLSearchParams === 'undefined') {
    require('url-search-params-polyfill');
}

// React 16 requires this 2
import 'core-js/es6/map'
import 'core-js/es6/set'