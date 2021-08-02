import React from 'react';

import './styles/Snapshot.less'

export default function Snapshot({ vendorSnapshots }) {
    return (
        <div className="mainDiv">
            <div style={{ textAlign: "center", paddingBottom: "15px" }}>
                <span className="headingSpan">SNAPSHOT</span>
            </div>

            <div>
                <div style={{
                    paddingTop: "20px",
                    paddingRight: "30px",
                    paddingLeft: "20px",
                    width: "9%", display: "inline-block",
                    textAlign: "center"
                }}>
                    <span className="snapshotSpan">{vendorSnapshots.invites || 0}</span>
                    <span className="snapshothead"> INVITES</span></div>
                <div style={{
                    paddingTop: "20px",
                    paddingRight: "30px",
                    paddingLeft: "30px",
                    width: "9%", display: "inline-block",
                    textAlign: "center"
                }}><span className="snapshotSpan">{vendorSnapshots.active || 0}</span>
                    <span className="snapshothead">  ACTIVE</span>
                </div>

                <div style={{
                    paddingTop: "20px",
                    paddingRight: "30px",
                    paddingLeft: "30px",
                    width: "10%", display: "inline-block",
                    textAlign: "center"
                }}>
                    {/* <span className="snapshotSpan">{vendorSnapshots.atRisk || 0}</span>
                    <span className="snapshothead">  AT RISKff</span> */}
                </div>

                <div style={{
                    paddingTop: "20px",
                    paddingRight: "5px",
                    paddingLeft: "5px",
                    width: "9%", display: "inline-block",
                    textAlign: "center"
                }}><span className="snapshotSpan">{vendorSnapshots.overdue || 0}</span>
                    <span className="snapshothead">   OVERDUE</span>
                </div>

                <div style={{
                    paddingTop: "20px",
                    paddingRight: "30px",
                    paddingLeft: "30px",
                    width: "20%", display: "inline-block",
                    textAlign: "center"
                }}><span className="snapshotSpan">${vendorSnapshots.queueRevenue || 0}</span>
                    <span className="snapshothead">  QUEUE </span>
                </div>

                <div style={{
                    paddingTop: "20px",
                    paddingRight: "30px",
                    paddingLeft: "30px",
                    width: "10%", display: "inline-block",
                    textAlign: "center"
                }}><span className="snapshotSpan">{vendorSnapshots.lost || 0}</span>
                    <span className="snapshothead">  LOST</span>
                </div>

                <div style={{
                    paddingTop: "20px",
                    paddingRight: "30px",
                    paddingLeft: "30px",
                    width: "18%", display: "inline-block",
                    textAlign: "center"
                }}><span className="snapshotSpan">{vendorSnapshots.totalCompleted || 0}</span>
                    <span className="snapshothead"> COMPLETED</span>
                </div>

                <div style={{
                    paddingTop: "20px",
                    paddingRight: "20px",
                    paddingLeft: "30px",
                    width: "15%", display: "inline-block",
                    textAlign: "center"
                }}><span className="snapshotSpan">${vendorSnapshots.totalRevenue || 0}</span>
                    <span className="snapshothead">  REVENUE</span>
                </div>
            </div>
        </div>
    );
}
