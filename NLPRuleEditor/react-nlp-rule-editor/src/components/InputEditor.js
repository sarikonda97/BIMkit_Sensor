import React from 'react';
import ReactDOM from 'react-dom';

import { Editor, EditorState, CompositeDecorator, ContentState } from 'draft-js';
import { ParseJson } from '../tools/jsonParser';
import _ from "lodash";
import OverlayTrigger from 'react-bootstrap/OverlayTrigger';
import { popoverSuggestion } from './Popovers.js';
import 'draft-js/dist/Draft.css';


import { Token } from './Token';
import { OCCURRENCE_MATCH, TYPE_MATCH, NEGATION_MATCH, PROPERTY_MATCH, OPERATION_MATCH, PROPERTY_BOOLEAN_MATCH, VALUE_MATCH, UNIT_MATCH, LOGICAL_OPERATOR_MATCH, OBJECTCHECK_MATCH, RELATION_MATCH, UNKNOWN_MATCH} from './regexTokens';

// Component for text input field that handles text coloring with the Draft.js library.
// Resides within InputForm
export class InputEditor extends React.Component {

    constructor(props) {
        super(props);

        const occurrenceStrategy = (contentBlock, callback, contentState) => {
            let occurrenceTokens = findCategory(OCCURRENCE_MATCH.regex, contentBlock, callback);
            occurrenceTokens.forEach(token =>{
                token.type = OCCURRENCE_MATCH.type
            });
            props.updateTokens(OCCURRENCE_MATCH.type, occurrenceTokens);
        }

        const typeStrategy = (contentBlock, callback, contentState) => {
            let typeTokens = findCategory(TYPE_MATCH.regex, contentBlock, callback);
            typeTokens.forEach(token =>{
                token.type = TYPE_MATCH.type
            });
            props.updateTokens(TYPE_MATCH.type, typeTokens);
        }

        function negationStrategy(contentBlock, callback, contentState) {
            let negationTokens = findCategory(NEGATION_MATCH.regex, contentBlock, callback);
            negationTokens.forEach(token =>{
                token.type = NEGATION_MATCH.type
            });
            props.updateTokens(NEGATION_MATCH.type, negationTokens);
        }

        function propertyStrategy(contentBlock, callback, contentState) {
            let propertyTokens = findCategory(PROPERTY_MATCH.regex, contentBlock, callback);

            propertyTokens.forEach(token =>{
                token.type = 'property'
            });
            props.updateTokens('property', propertyTokens);
        }

        function propertyBooleanStrategy(contentBlock, callback, contentState) {
            let propertyTokens = findCategory(PROPERTY_BOOLEAN_MATCH.regex, contentBlock, callback);

            propertyTokens.forEach(token =>{
                token.type = PROPERTY_BOOLEAN_MATCH.type
            });
            props.updateTokens(PROPERTY_BOOLEAN_MATCH.type, propertyTokens);
        }

        function operationStrategy(contentBlock, callback, contentState) {
            let operationTokens = findCategory(OPERATION_MATCH.regex, contentBlock, callback);

            operationTokens.forEach(token =>{
                token.type = OPERATION_MATCH.type
            });
            props.updateTokens(OPERATION_MATCH.type, operationTokens);
        }
        
        function valueStrategy(contentBlock, callback, contentState) {
            
            let valueTokens = findCategory(VALUE_MATCH.regex, contentBlock, callback);

            valueTokens.forEach(token =>{
                token.type = VALUE_MATCH.type
            });
            props.updateTokens(VALUE_MATCH.type, valueTokens);
        }
        
        function logicalOperatorStrategy(contentBlock, callback, contentState) {
            let logicalOperatorTokens =findCategory(LOGICAL_OPERATOR_MATCH.regex, contentBlock, callback);
            logicalOperatorTokens.forEach(token =>{
                token.type = LOGICAL_OPERATOR_MATCH.type
            });
            props.updateTokens(LOGICAL_OPERATOR_MATCH.type, logicalOperatorTokens);
        }
        
        function unitStrategy(contentBlock, callback, contentState) {
            
            let unitTokens = findCategory(UNIT_MATCH.regex, contentBlock, callback);
            unitTokens.forEach(token =>{
                token.type = UNIT_MATCH.type
            });
            props.updateTokens(UNIT_MATCH.type, unitTokens);
        }
        
        function relationStrategy(contentBlock, callback, contentState) {
            
            let relationTokens = findCategory(RELATION_MATCH.regex, contentBlock, callback);
            relationTokens.forEach(token =>{
                token.type = RELATION_MATCH.type
            });
            props.updateTokens(RELATION_MATCH.type, relationTokens);
        }

        function objectCheckStrategy(contentBlock, callback, contentState) {
            
            let checkTokens = findCategory(OBJECTCHECK_MATCH.regex, contentBlock, callback);
            checkTokens.forEach(token =>{
                token.type = OBJECTCHECK_MATCH.type
            });
            props.updateTokens(OBJECTCHECK_MATCH.type, checkTokens);
        }

        

        function unknownStrategy(contentBlock, callback, contentState) {
            findCategory(UNKNOWN_MATCH, contentBlock, callback);
        }
        

        this.compositeDecorator = new CompositeDecorator([
            {
                strategy: occurrenceStrategy,
                component: occurrenceSpan,
            },
            {
                strategy: typeStrategy,
                component: typeSpan,
            },
            {
                strategy: negationStrategy,
                component: negationSpan,
            },
            {
                strategy: propertyStrategy,
                component: propertySpan,
            },
            {
                strategy: propertyBooleanStrategy,
                component: propertySpan,
            },
            {
                strategy: operationStrategy,
                component: operationSpan,
            },
            
            {
                strategy: unitStrategy,
                component: unitSpan,
            },
            {
                strategy: valueStrategy,
                component:valueSpan,
            },
            
            {
                strategy: logicalOperatorStrategy,
                component: logicalOperatorSpan,
            },
            {
                strategy: relationStrategy,
                component: relationSpan,
            },
            {
                strategy: objectCheckStrategy,
                component: null,
            },
            {
                strategy: unknownStrategy,
                component: unknownSpan,
            },
        ]);

       

        this.state = {
            editorState: EditorState.createEmpty(this.compositeDecorator),
            categories: { types: [], properties: [], relations: [], units: [], unknowns: [] },
        };
        
        this.editor = React.createRef();

        this.focus = () => this.editor.current.focus();

        //called when editor is changed
        this.onChange = (editorState) => {
            //console.log(11111);
            //console.log(editorState.getCurrentContent().getPlainText() !== this.state.editorState.getCurrentContent().getPlainText());
            // Sets current state, and also sets the value state of our parent component via the callback function passed in props.
            if (editorState.getCurrentContent().getPlainText() !== this.state.editorState.getCurrentContent().getPlainText()) {
                //console.log("parent call back");
                this.props.parentCallback(editorState.getCurrentContent().getPlainText());
            }
            this.setState({ editorState: editorState });
        }
    }
    

    

    componentDidUpdate(oldProps) {
        //console.log(12344);
        if (oldProps.activeRule !== this.props.activeRule) {
            // We want to set the text in the rule box to be the description of the rule
            const selectionState = this.state.editorState.getSelection();
            const newContentState = ContentState.createFromText(this.props.activeRule.Description);
            const newEditorState = EditorState.create({ currentContent: newContentState, selection: selectionState, decorator: this.compositeDecorator })
            this.setState({ editorState: newEditorState })
            // Then try and refresh our highlighting information.
            this.props.parentCallback(newEditorState.getCurrentContent().getPlainText())
        }
    }



    render() {
        return (
            <div id="editor-container" style={styles.root}>
                <div id="editor" style={styles.editor} onClick={this.focus}>
                    <Editor
                        editorState={this.state.editorState}
                        onChange={this.onChange}
                        placeholder="Enter your rule here, then press Submit."
                        ref={this.editor}
                        spellCheck={true}
                    />
                </div>
            </div>
        );
    }
}








function findCategory(regex, contentBlock, callback) {
    let tokens = [];
    const text = contentBlock.getText();
    let matchArr, start;
    while ((matchArr = regex.exec(text)) !== null) {
        start = matchArr.index;
        tokens.push(new Token(start, matchArr[0]))
        callback(start, start + matchArr[0].length);
    }
    return tokens;
}


const occurrenceSpan = (props) => {
    return (
        <span
            style={styles.occurrence}
            data-offset-key={props.offsetKey}
        >
            {props.children}
        </span>
    );
};

const typeSpan = (props) => {
    return (
        <span
            style={styles.type}
            data-offset-key={props.offsetKey}
        >
            {props.children}
        </span>
    );
};

const negationSpan = (props) => {
    return (
        <span
            style={styles.negation}
            data-offset-key={props.offsetKey}
        >
            {props.children}
        </span>
    );
};

const propertySpan = (props) => {
    return (
        <span
            style={styles.property}
            data-offset-key={props.offsetKey}
        >
            {props.children}
        </span>
    );
};

const operationSpan = (props) => {
    return (
        <span
            style={styles.operation}
            data-offset-key={props.offsetKey}
        >
            {props.children}
        </span>
    );
};

const valueSpan = (props) => {
    return (
        <span
            style={styles.value}
            data-offset-key={props.offsetKey}
        >
            {props.children}
        </span>
    );
};

const unitSpan = (props) => {
    return (
        <span
            style={styles.unit}
            data-offset-key={props.offsetKey}
        >
            {props.children}
        </span>
    );
};

const logicalOperatorSpan = (props) => {
    return (
        <span
            style={styles.logicalOperator}
            data-offset-key={props.offsetKey}
        >
            {props.children}
        </span>
    );
};

const relationSpan = (props) => {
    return (
        <span
            style={styles.relation}
            data-offset-key={props.offsetKey}
        >
            {props.children}
        </span>
    );
};

const unknownSpan = (props) => {
    return (
        <OverlayTrigger trigger="focus" placement="top" overlay={popoverSuggestion}>
            <span
                style={styles.unknown}
                data-offset-key={props.offsetKey}
            >
                {props.children}
            </span>
        </OverlayTrigger>
    );
};



const styles = {
    root: {
        fontFamily: '\'Helvetica\', sans-serif',
        paddingBottom: 10,
        width: "100%"
    },
    editor: {
        border: '1px solid #ddd',
        cursor: 'text',
        fontSize: 16,
        minHeight: 275,
        padding: 10,
    },
    button: {
        marginTop: 10,
        textAlign: 'center',
    },
    occurrence: {
        color: 'rgb(255, 122, 45)',
        //orange
    },
    type: {
        color: 'rgba(255, 193, 7, 1.0)',
        //yellow
    },
    negation: {
        color: 'rgba(67,205,49,1.0)',
        //apple
    },
    
    property: {
        color: 'rgba(40, 167, 69, 1.0)',
        //green
    },
    operation: {
        color: 'rgb(198, 207, 167)',
        //mimosa
    },
    value: {
        color: 'rgb(207, 91, 163)',
        //darker cyan
    },
    unit: {
        color: '#0dcaf0',
        //Sweet Pink
    },
    logicalOperator: {
        color: 'rgb(70, 154, 255)',
        //purple
    },
    relation: {
        color: 'rgba(0, 123, 255, 1.0)',
        //blue
    },
    
    unknown: {
        color: 'rgba(220, 53, 69, 1.0)',
        //red
    },
    suggestions: {
        float: 'right',
        width: '1%',
        right: '-100px'

    }
};