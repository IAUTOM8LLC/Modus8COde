import React from "react";

import { ItemsFilter, SimpleSegment, Inline } from "@components";

export default function PerformanceHeader() {
    const filterOptions = [
        { key: "team", value: "team", text: "Team" },
        { key: "skill", value: "skill", text: "Skill" },
        { key: "formula", value: "formula", text: "Formula" },
        { key: "review", value: "review", text: "Review" },
        { key: "rating", value: "rating", text: "Rating" },
        { key: "price", value: "price", text: "Price" },
        { key: "dwell", value: "dwell", text: "Dwell" },
        { key: "ct", value: "ct", text: "CT" },
        { key: "tat", value: "tat", text: "TAT" },
    ];

    return (
        <SimpleSegment
            clearing
            className="iauto-projects__header"
            style={{ marginBottom: 30 }}
        >
            <Inline floated="right">
                <ItemsFilter by={filterOptions} />
            </Inline>
        </SimpleSegment>
    );
}
