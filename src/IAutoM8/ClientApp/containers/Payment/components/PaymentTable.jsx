import React, { useState, useEffect, useRef } from "react";
import { Table, Header, Checkbox } from "semantic-ui-react";
import { v4 as uuidv4 } from "uuid";
import { filter, sortBy } from "lodash";
import DateTimePicker from 'react-widgets/lib/DateTimePicker';
import momentLocalizer from 'react-widgets-moment';
import moment from 'moment';

import { Inline, ModusButton, TimeAgo } from "@components";

import { toggleDirection } from "@utils/sort";
import { formatMoney } from "@utils/formatMoney";

momentLocalizer();

export default function PaymentTable({
    paymentRequestList,
    onChange,
    downloadCSV,
    acceptRequest,
    denyRequest,
    batchProcess,
    activeTab
}) {
    const inputRef = useRef([]);

    const [data, setData] = useState([]);
    const [orderColumn, setOrderColumn] = useState("vendorName");
    const [orderDirection, setOrderDirection] = useState("ascending");
    const [paymentRequests, setPaymentRequests] = useState([]);
    const [startDate, setStartDate] = useState(null);
    const [endDate, setEndDate] = useState(null);

    const allChecked = paymentRequests.every(request => request.isChecked);
    const noneChecked = paymentRequests.every(request => !request.isChecked);

    useEffect(() => {
        if (orderColumn !== null && orderDirection !== null) {
            let orderedPaymentRequests = sortBy(
                paymentRequestList,
                orderColumn
            );

            if (orderDirection === "descending") {
                orderedPaymentRequests = orderedPaymentRequests.reverse();
                setPaymentRequests(orderedPaymentRequests);
            } else {
                setPaymentRequests(orderedPaymentRequests);
            }

        } else {
            setPaymentRequests(paymentRequestList);

        }
    }, [paymentRequestList]);



    useEffect(() => {
        // We will fetch data and receive an array
        // let data = fetchData();
        // To simplify, let's suppose that the array is:
        const data = paymentRequestList;

        // We define the size of array after receiving the data
        inputRef.current = new Array(data.length);

        // We set state
        setData(data);
    }, []);

    useEffect(() => {
        // If data is filled -> focus
        if (data.length !== 0) {
            // Focusing the last <input></input>
            inputRef.current[data.length - 1].focus();
        }
    }, [data]);

    function handleAcceptClick(event, index, id) {
        //const id = paymentRequestList[index].id;
        const desc = inputRef.current[index].value;
        acceptRequest(id, desc);
    }

    const handleDeclineClick = (event, index, id) => {
        //const id = paymentRequestList[index].id;
        const desc = inputRef.current[index].value;
        denyRequest(id, desc);
    };

    const onSort = (event, sortKey) => {
        if (orderColumn !== sortKey) {
            setOrderColumn(sortKey);
            setPaymentRequests(sortBy(paymentRequests, sortKey));
            setOrderDirection("ascending");

            return;
        }

        setPaymentRequests(paymentRequests.reverse());
        setOrderDirection(toggleDirection(orderDirection));
    };

    const onSetEndDate = (value) => {
        if (!startDate || startDate === null) {
            return;
        }
        setEndDate(value)
    };

    const onApplyFilters = () => {
        if (!startDate || startDate === null) {
            return;
        }

        if (!endDate || endDate === null) {
            return;
        }

        let filteredPaymentRequest = paymentRequests;
        filteredPaymentRequest = filter(filteredPaymentRequest, (f) => {
            return moment(f.requestTime).isSameOrAfter(startDate, 'day')
                && moment(f.requestTime).isSameOrAfter(startDate, 'month')
                && moment(f.requestTime).isSameOrAfter(startDate, 'year')
                && moment(f.requestTime).isSameOrBefore(endDate, 'day')
                && moment(f.requestTime).isSameOrBefore(endDate, 'month')
                && moment(f.requestTime).isSameOrBefore(endDate, 'year');
        });

        setPaymentRequests(filteredPaymentRequest);
    };

    const handleMasterChange = (value) => {
        if (value.checked) {
            let filteredPaymentRequest = paymentRequests.map(request => ({
                ...request, isChecked: true
            }));
            setPaymentRequests(filteredPaymentRequest);
        } else if (!value.indeterminate) {
            let filteredPaymentRequest = paymentRequests.map(request => ({
                ...request, isChecked: false
            }));
            setPaymentRequests(filteredPaymentRequest);
        }
    }

    const onClearFilters = () => {
        setStartDate(null);
        setEndDate(null);

        if (orderColumn !== null && orderDirection !== null) {
            let orderedPaymentRequests = sortBy(
                paymentRequestList,
                orderColumn
            );


            if (orderDirection === "descending") {
                orderedPaymentRequests = orderedPaymentRequests.reverse();
                setPaymentRequests(orderedPaymentRequests);
            } else {
                setPaymentRequests(orderedPaymentRequests);
            }
        } else {
            setPaymentRequests(paymentRequestList);
        }
    };


    const showDateFilters = paymentRequests
        && paymentRequests.length > 0
        && (paymentRequests[0].status === 2 || paymentRequests[0].status === 3);

    return (
        //
        <div>
            <div className="modus-items-filter">
                <Inline>
                    <label>
                        <strong>FILTER BY: </strong>
                    </label>

                    <label>Start Date</label>
                    <DateTimePicker
                        className="modus-date-time"
                        name="startDate"
                        time={false}
                        format="MMM D, YYYY"
                        onChange={value => setStartDate(value)}
                        value={startDate}
                    />

                    <label>End Date</label>
                    <DateTimePicker
                        className="modus-date-time"
                        name="startDate"
                        min={startDate}
                        time={false}
                        format="MMM D, YYYY"
                        onChange={value => onSetEndDate(value)}
                        value={endDate}
                        disabled={!startDate}
                    />
                    <ModusButton
                        className="button-flex-order3"
                        grey
                        content="Apply Filter"
                        onClick={onApplyFilters}
                    />
                    <ModusButton
                        className="button-flex-order3"
                        grey
                        content="Clear Filter"
                        onClick={onClearFilters}
                    />

                    {paymentRequests[0] && paymentRequests[0].status == 0 && (
                        <div style={{ float: "right" }}>
                            <button
                                className="ui button iauto purple"
                                onClick={() => { downloadCSV(paymentRequests); onClearFilters(); }}
                                role="button"
                            >
                                Download CSV
                    </button>
                        </div>
                    )}
                    {paymentRequests[0] && paymentRequests[0].status == 1 && (
                        <ModusButton
                            className="ui button iauto purple"
                            grey
                            content="Process Selected"
                            onClick={() => { batchProcess(1, paymentRequests), onClearFilters(); }}
                        />
                    )}
                    {paymentRequests[0] && paymentRequests[0].status == 1 && (
                        <ModusButton
                            className="ui button iauto purple"
                            grey
                            content="Decline Selected"
                            onClick={() => { batchProcess(0, paymentRequests); onClearFilters(); }}
                        />
                    )}

                </Inline>
            </div>
            <Table sortable>
                <Table.Header>
                    <Table.Row>
                        {paymentRequests[0] && (paymentRequests[0].status == 0
                            || paymentRequests[0].status == 1) && (
                                <Table.HeaderCell textAlign="center">
                                    <Checkbox
                                        checked={allChecked}
                                        indeterminate={!allChecked && !noneChecked}
                                        onClick={(e, d) => handleMasterChange(d)}
                                    />
                                </Table.HeaderCell>
                            )}
                        <Table.HeaderCell
                            sorted={
                                orderColumn === "vendorName"
                                    ? orderDirection
                                    : null
                            }
                            onClick={(e) => onSort(e, "vendorName")}
                        >
                            Vendor Name
                        </Table.HeaderCell>

                        <Table.HeaderCell
                            textAlign="right"
                            sorted={
                                orderColumn === "requestedAmount"
                                    ? orderDirection
                                    : null
                            }
                            onClick={(e) => onSort(e, "requestedAmount")}
                        >
                            Requested Amount
                        </Table.HeaderCell>

                        <Table.HeaderCell
                            textAlign="right"
                            sorted={
                                orderColumn === "requestedAmount"
                                    ? orderDirection
                                    : null
                            }
                            onClick={(e) => onSort(e, "requestedAmount")}
                        >
                            Requested Amount With Tax
                        </Table.HeaderCell>

                        <Table.HeaderCell
                            textAlign="center"
                            sorted={
                                orderColumn === "requestTime"
                                    ? orderDirection
                                    : null
                            }
                            onClick={(e) => onSort(e, "requestTime")}
                        >
                            Requested Date
                        </Table.HeaderCell>

                        <Table.HeaderCell
                            sorted={
                                orderColumn === "status"
                                    ? orderDirection
                                    : null
                            }
                            onClick={(e) => onSort(e, "status")}
                        >
                            Status
                        </Table.HeaderCell>

                        {((paymentRequests[0] &&
                            paymentRequests[0].status == 1) ||
                            (paymentRequests[0] &&
                                paymentRequests[0].status == 2) ||
                            (paymentRequests[0] &&
                                paymentRequests[0].status == 3)) && (
                                <Table.HeaderCell
                                    sorted={
                                        orderColumn === "description"
                                            ? orderDirection
                                            : null
                                    }
                                    onClick={(e) => onSort(e, "description")}
                                >
                                    Description
                                </Table.HeaderCell>
                            )}

                        {paymentRequests[0] &&
                            paymentRequests[0].status == 1 && (
                                <Table.HeaderCell>Action</Table.HeaderCell>
                            )}

                        <Table.HeaderCell />
                    </Table.Row>
                </Table.Header>
                <Table.Body>
                    {paymentRequests &&
                        paymentRequests.map((data, index) => {
                            return (
                                < Table.Row key={uuidv4()} >
                                    {
                                        paymentRequests[0] &&
                                        (paymentRequests[0].status == 0
                                            || paymentRequests[0].status == 1) && (
                                            <Table.Cell textAlign="center">
                                                <Checkbox
                                                    checked={data.isChecked}
                                                    onChange={() =>
                                                        onChange(data.id, paymentRequests)
                                                    }
                                                />
                                                {data.isChecked}
                                            </Table.Cell>
                                        )
                                    }
                                    < Table.Cell >
                                        <div style={{ color: "black" }}>
                                            {data.vendorName}
                                        </div>
                                    </Table.Cell>
                                    <Table.Cell textAlign="right">
                                        <div style={{ color: "black" }}>
                                            $ {formatMoney(data.requestedAmount, 2, ".", " ")}
                                        </div>
                                    </Table.Cell>
                                    <Table.Cell textAlign="right">
                                        <div style={{ color: "black" }}>
                                            $ {formatMoney(data.requestedAmountWithTax, 2, ".", " ")}
                                        </div>
                                    </Table.Cell>
                                    <Table.Cell textAlign="center">
                                        <div style={{ color: "black" }}>
                                            <TimeAgo date={data.requestTime} />
                                        </div>
                                    </Table.Cell>
                                    <Table.Cell>
                                        <div style={{ color: "black" }}>
                                            {data.status == 0 && (
                                                <span>Requested</span>
                                            )}
                                            {data.status == 1 && (
                                                <span>InProcess</span>
                                            )}
                                            {data.status == 2 && (
                                                <span>Processed</span>
                                            )}
                                            {data.status == 3 && (
                                                <span>Declined</span>
                                            )}
                                        </div>
                                    </Table.Cell>
                                    {paymentRequests[0] &&
                                        paymentRequests[0].status == 1 && (
                                            <Table.Cell>
                                                <div className="ui form">
                                                    {paymentRequests[0] &&
                                                        paymentRequests[0]
                                                            .status == 1 && (
                                                            <div className="field">
                                                                <textarea
                                                                    ref={(el) =>
                                                                        (inputRef.current[
                                                                            index
                                                                        ] = el)
                                                                    }
                                                                    rows="2"
                                                                    placeholder="Add description..."
                                                                />
                                                            </div>
                                                        )}
                                                </div>
                                            </Table.Cell>
                                        )}
                                    {paymentRequests[0] &&
                                        (paymentRequests[0].status == 2 ||
                                            paymentRequests[0].status == 3) && (
                                            <Table.Cell>
                                                <div style={{ color: "black" }}>
                                                    {data.description}
                                                </div>
                                            </Table.Cell>
                                        )}

                                    {paymentRequests[0] &&
                                        paymentRequests[0].status == 1 && (
                                            <Table.Cell collapsing className="center aligned">
                                                <Inline>
                                                    <ModusButton
                                                        circular
                                                        icon="checkmark"
                                                        popup="Accept"
                                                        onClick={(e) =>
                                                            handleAcceptClick(
                                                                e,
                                                                index,
                                                                data.id
                                                            )
                                                        }
                                                    />

                                                    <ModusButton
                                                        circular
                                                        icon="close"
                                                        popup="Decline"
                                                        onClick={(e) =>
                                                            handleDeclineClick(
                                                                e,
                                                                index, data.id
                                                            )
                                                        }
                                                    />
                                                </Inline>
                                            </Table.Cell>
                                        )}
                                </Table.Row>
                            );
                        }
                        )}

                    {paymentRequests.length === 0 && (
                        <Table.Row>
                            <Table.HeaderCell
                                colSpan={6}
                                style={{ height: 250 }}
                            >
                                <Header as="h2" icon textAlign="center">
                                    <Header.Content>
                                        {activeTab == 0 &&
                                            <span>No Vendor Payment Request Found</span>
                                        }
                                        {activeTab == 1 &&
                                            <span>No In Process Request Found</span>
                                        }
                                        {activeTab == 2 &&
                                            <span>No Payment History Found</span>
                                        }
                                    </Header.Content>
                                </Header>
                            </Table.HeaderCell>
                        </Table.Row>
                    )}
                </Table.Body>
            </Table>
        </div >
    );
}
