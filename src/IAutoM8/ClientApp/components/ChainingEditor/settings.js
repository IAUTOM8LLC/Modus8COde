const AnchorType = ["Perimeter", {
    shape: "Rectangle", // Shape of actual figure on canvas, so that js plumb can place anchors better
    anchorCount: 200 // amount of anchorst that are placed on perimeter. The more anchors - smoother connections
}];

const ConnectorType = ["StateMachine", {
    curviness: 30,
    proximityLimit: 160,
    margin: 6
}];

export const DefaultPadding = {
    left: 20,
    top: 50
};
export const CustomConnectionType = "iauto-connection";
export const SelectedConditionConnectionType = "iauto-connection/selected-condition";
export const ColapsedFormula = "colapsed-connection";

export const connectionType = {
    anchor: AnchorType,
    connector: ConnectorType
};

export const instance = {
    Endpoint: ['Dot', { radius: 2 }],
    Connector: ConnectorType,
    HoverPaintStyle: { stroke: '#1e8151' },
    ConnectionOverlays: [
        ['Arrow', {
            location: 1,
            id: 'arrow',
            length: 10,
            foldback: .3
        }]
    ],
    Container: 'canvas',
    ConnectionsDetachable: false
};

export const source = {
    filter: '.ep',
    anchor: AnchorType,
    connectorStyle: {
        stroke: '#5c96bc',
        strokeWidth: 2,
        outlineStroke: 'transparent',
        outlineWidth: 2
    },
    isSource: true,
    connectionType: CustomConnectionType
};

export const target = {
    dropOptions: { hoverClass: 'dragHover' },
    anchor: AnchorType,
    allowLoopback: false,
    connectorStyle: {
        stroke: '#5c96bc',
        strokeWidth: 2,
        outlineStroke: 'transparent',
        outlineWidth: 2
    },
    connectionType: CustomConnectionType

};

export const conditional = {
    endpoint: ["Dot", { radius: 6 }],
    anchor: ["Continuous", { faces: ["top", "left", "bottom", "right"] }],
    paintStyle: { strokeWidth: 2, stroke: "#ffa500", fill: '#ffffff' },
    connectorStyle: { strokeWidth: 2, stroke: "#ffa500" },

    allowLoopback: false,
    isSource: true,
    maxConnections: 2,
    connector: ConnectorType
};

export const selectedCondition = {
    ...conditional,
    paintStyle: { strokeWidth: 2, stroke: "#1e8151" }
};
