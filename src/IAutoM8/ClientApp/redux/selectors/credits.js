export const getTransferRequestId = (state, props) => {
    return Number(props.match.params.transferRequestId) || 0;
}
