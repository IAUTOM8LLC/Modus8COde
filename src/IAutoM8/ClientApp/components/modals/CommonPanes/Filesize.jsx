import React, { Component } from 'react'
import PropTypes from 'prop-types'

class Filesize extends Component {
    static propTypes = {
        size: PropTypes.number.isRequired
    };

    static defaultProps = {
        units: {
            byte: 'B',
            kilobyte: 'KB',
            megabyte: 'MB',
            gigabyte: 'GB',
            terabyte: 'TB'
        }
    }

    constructor(props) {
        super(props)
    }

    shouldComponentUpdate(nextProps) {
        return nextProps.size !== this.props.size
    }

    render() {
        const size = this.props.size

        if (size == null || size < 0) {
            return (
                <span className="react-fine-uploader-filesize file-size" />
            )
        }

        const units = this.props.units
        const { formattedSize, formattedUnits } = formatSizeAndUnits({ size, units })

        return (
            <span className="react-fine-uploader-filesize file-size">
                <span className="react-fine-uploader-filesize-value">
                    {formattedSize}
                </span>
                <span className="react-fine-uploader-filesize-separator"> </span>
                <span className="react-fine-uploader-filesize-unit">
                    {formattedUnits}
                </span>
            </span>
        )
    }
}

const formatSizeAndUnits = ({ size, units }) => {
    let formattedSize,
        formattedUnits

    if (size < 1e+3) {
        formattedSize = size
        formattedUnits = units.byte
    }
    else if (size >= 1e+3 && size < 1e+6) {
        formattedSize = (size / 1e+3).toFixed(2)
        formattedUnits = units.kilobyte
    }
    else if (size >= 1e+6 && size < 1e+9) {
        formattedSize = (size / 1e+6).toFixed(2)
        formattedUnits = units.megabyte
    }
    else if (size >= 1e+9 && size < 1e+12) {
        formattedSize = (size / 1e+9).toFixed(2)
        formattedUnits = units.gigabyte
    }
    else {
        formattedSize = (size / 1e+12).toFixed(2)
        formattedUnits = units.terabyte
    }

    return { formattedSize, formattedUnits }
}

const areUnitsEqual = (units1, units2) => {
    const keys1 = Object.keys(units1)

    if (keys1.length === Object.keys(units2).length) {
        return keys1.every(key1 => units1[key1] === units2[key1])
    }

    return false
}

export default Filesize
