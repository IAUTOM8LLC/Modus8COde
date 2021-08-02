    import React, { Component } from 'react'
    import { Accordion, Icon, Segment } from 'semantic-ui-react'

    import PerformanceTable from './PerformanceTable'

    export default class CompanyUserAccordion extends Component {
    state = { activeIndex: 0 }
    handleClick = (e, titleProps) => {
        const { index } = titleProps
        const { activeIndex } = this.state
        const newIndex = activeIndex === index ? -1 : index
        this.setState({ activeIndex: newIndex })
    }
    render() {
        const { activeIndex } = this.state
        return (
        <Segment >
            <Accordion >
            <Accordion.Title
                active={activeIndex === 1}
                index={1}
                onClick={this.handleClick}
        >
                <Icon name="dropdown" />
                {this.props.userName}
                {/* {this.props.email} */}
                
            </Accordion.Title>
            
            <Accordion.Content active={activeIndex === 1}>
                {/* <p>
                A dog is a type of domesticated animal. Known for its loyalty and
                faithfulness, it can be found as a welcome guest in many
                households across the world.
                </p> */}
                {this.props.performanceData.length && 
                <PerformanceTable performanceData = {this.props.performanceData} />}
            </Accordion.Content>
            
            </Accordion>
        </Segment>


    //         <div className="ui styled fluid accordion">
    // <div className="active title">
        
    // <div className="title">
    //     <i className="dropdown icon"></i>
    //     {this.props.userName}
    // </div>
    // <div className="content">
    // {this.props.performanceData.length && 
    //             <PerformanceTable performanceData = {this.props.performanceData} />}
    // </div>
    
    // </div>
    // </div>
        )
    }
    }