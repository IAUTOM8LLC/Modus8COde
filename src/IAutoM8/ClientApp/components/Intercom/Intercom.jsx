/* eslint-disable */
import React, { Component } from 'react'

/* This is adjusted version of lib https://github.com/nhagen/react-intercom 
   Main differences:
   - react 16 ready
   - componentWillReceiveProps is called differently in react 16 and may occur before mount
*/
export default class Intercom extends Component {

    constructor(props) {
        super(props);
        const { appID, ...otherProps } = props;

        if (!appID) {
            console.warn('Seams like you\'re using Intercom but forgot to provide appId');
            return;
        }

        if (!window.Intercom) {
            (function (w, d, id, s, x) {
                function i() {
                    i.c(arguments);
                }
                i.q = [];
                i.c = function (args) {
                    i.q.push(args);
                };
                w.Intercom = i;
                s = d.createElement('script');
                s.async = 1;
                s.src = `https://widget.intercom.io/widget/${id}`;
                x = d.getElementsByTagName('script')[0];
                x.parentNode.insertBefore(s, x);
            })(window, document, appID);
        }

        window.intercomSettings = { ...otherProps, app_id: appID };
    }

    componentDidUpdate() {
        this.execute('boot', this.props);
    }

    componentWillReceiveProps(nextProps) {
        const { appID, ...otherProps } = nextProps;

        window.intercomSettings = { ...otherProps, app_id: appID };

        this.execute('update', otherProps);
    }

    shouldComponentUpdate() {
        return false;
    }

    componentWillUnmount() {
        this.execute('shutdown');
        delete window.Intercom;
    }

    execute(command, props) {
        if (window.Intercom) {
            window.Intercom(command, props);
        }
    }

    render() {
        return null;
    }
}
