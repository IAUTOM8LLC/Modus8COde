import React from 'react'
import hoistNonReactStatic from 'hoist-non-react-statics';

const initialState = {
    open: false,
    resolve: () => { },
    reject: () => { },
    params: {}
}

export default function editModalHoc(WrappedComponent) {
    class EditModalWrapper extends React.Component {
        state = initialState

        show = (params) => {
            return new Promise((resolve, reject) => {
                this.setState({
                    open: true,
                    resolve,
                    reject,
                    params
                });
            });
        }

        handleClose = () => {
            this.state.reject();
            this.setState(initialState);
        }

        handleSubmit = (values) => {
            this.state.resolve(values);
            this.setState(initialState);
        }

        render() {
            const { open, params } = this.state;

            return <WrappedComponent
                open={open}
                onClose={this.handleClose}
                onSubmit={this.handleSubmit}
                {...params}
                {...this.props}
            />;
        }
    }

    hoistNonReactStatic(EditModalWrapper, WrappedComponent);

    return EditModalWrapper;
}
