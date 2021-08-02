import React from 'react'

export default class BzIframe extends React.Component {

    componentDidMount() {
        this.iframe.addEventListener('load', () => {
            this.props.onLoad();
        });
    }

    render() {
        return <iframe ref={ref => (this.iframe = ref)} {...this.props} style={{
            visibility: "hidden",
            width: "0px",
            height: "0px"
        }} />;
    }
}
