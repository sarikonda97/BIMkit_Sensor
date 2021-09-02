import { useState, useCallback, useEffect } from 'react';
import { postData } from '../tools/Tools.js';
import _ from "lodash";
import Button from 'react-bootstrap/Button';
import { InputEditor } from './InputEditor';
import './InputForm.css'
import { Tokens } from './Token';
import { Translation,groupTokens } from './Translation.js';


//Input form is the component that sends user input to the backend through a socket.io endpoint. It receives text coloring from InputEditor.
export function InputForm(props) {
    // functions that let us access and change state
    const [value, setValue] = useState(props.activeRule.description);
    const [responseObjects, setResponse] = useState(props.activeRule.translation);
    const [responseObjectChecks, setResponseChecks] = useState(props.activeRule.translation);
    const [responseRelations, setResponseRelations] = useState(props.activeRule.translation);


    const [retranslation, setRetranslation] = useState(props.activeRule.retranslation);
    const [priorityLevel, setPriorityLevel] = useState(props.activeRule.ErrorLevel);
    const[tokens, setTokens] = useState(new Tokens());



    const updateTokens = (type, newTokens) => {
        let updatedTokens = tokens;
        updatedTokens.updateTokensOfType(type, newTokens);
        setTokens(updatedTokens);
    }

    const handleSubmit2 = () => {
        console.log(tokens);
        let currentTokens = tokens;
        console.log(currentTokens);
        currentTokens.lemmatizeTokens();
        let sortedTokens = currentTokens.allTokensSortedByIndex;
        console.log(currentTokens.allTokens);
        console.log(sortedTokens);
        let groupedTokens = new groupTokens(sortedTokens);
        groupedTokens.groupECS();
        groupedTokens.groupLogicalExpression();

        /*
        console.log(groupedTokens.ECSTokens);
        console.log(groupedTokens.objectChecks);
        console.log(groupedTokens.relationChecks);*/
    }
    


    useEffect(() => {

        setValue(props.activeRule.description);
        setPriorityLevel(props.activeRule.ErrorLevel);
        setResponse(props.activeRule.translation);
        setRetranslation(props.activeRule.retranslation);
        
    }, [props.activeRule])

    // Callback function, something that can be called from child to set "value" from our state to the contents of the Draft.js textbox
    const setValueFromChild = (childData) => {
        if (childData !== value) {
            setValue(childData);
            props.updateActiveRuleDescription(childData);
        }
        
    }

    //Function to select priority level of a rule with the optionselect rendered below.
    const selectChange = e => {
        setPriorityLevel(e.target.value)
        props.updateActiveRuleErrorLevel(e.target.value);
    }



    const handleSubmit = (event) => {
        //get rule object
        let updatedRule = props.activeRule;

        let currentTokens = tokens;
        currentTokens.lemmatizeTokens();
        let sortedTokens = currentTokens.allTokensSortedByIndex;
        

        
        let groupedTokens = new groupTokens(sortedTokens);
        groupedTokens.groupECS();
        groupedTokens.groupLogicalExpression();


        console.log(groupedTokens.ECSTokens);
        console.log(groupedTokens.objectChecks);
        console.log(groupedTokens.relationChecks);



        updatedRule.ExistentialClauses = groupedTokens.ECSTokens;
        updatedRule.LogicalExpression = groupedTokens.logicalExpression;

 
        let objectstring = '';
        for(var key in groupedTokens.ECSTokens){
            objectstring += "\t\t" + key + ": " + groupedTokens.ECSTokens[key]['OccurrenceRule'].toUpperCase() + " " +groupedTokens.ECSTokens[key]['Characteristic']['Type'];
            if(groupedTokens.ECSTokens[key]['Characteristic']['PropertyChecks'].length > 0){
                var i = 0;
                
                groupedTokens.ECSTokens[key]['Characteristic']['PropertyChecks'].forEach(property => {
                    if(i == 0){
                        objectstring += " with ";
                    }
                    else{
                        objectstring += " AND ";
                    }
                    objectstring += property['Name'] + " " + property['Operation'].toLowerCase() + " " + property['Value'] + " " + property['ValueUnit'];
                    i++;
                });
            }
            objectstring += '\n';
        }

        let objectCheckString = '';
        for(var key in groupedTokens.objectChecks){
            let check = groupedTokens.objectChecks[key];
            objectCheckString += "\t\t" + check['ObjName'] + ' ' + check['Negation'] + " " ;
            let property = check['PropertyCheck'];
            objectCheckString += property['Name'] + " " + property['Operation'].toLowerCase()
            if(property.type == "NUM"){
                relationCheckString += " " + property['Value'] + " " + property['ValueUnit'];
            }
            objectCheckString += '\n';
        }

        let relationCheckString = '';
        for(var key in groupedTokens.relationChecks){
            let check = groupedTokens.relationChecks[key];
            relationCheckString += "\t\t" + check['Obj1Name'] + ' ' + check['Negation'] + " " ;
            let property = check['PropertyCheck'];
            relationCheckString += property['Name'] + " "+ check['Obj2Name'] 
            if(property.type == "NUM"){
                relationCheckString += " "+ property['Operation'].toLowerCase() + " " + property['Value'];
            } 
           relationCheckString += '\n';
        }

        setResponse("Objects:\n"+objectstring);
        setResponseChecks("ObjectChecks:\n"+objectCheckString);
        setResponseRelations("RelationChecks:\n"+relationCheckString);


        setRetranslation(value);
        console.log(updatedRule);
        
    }
    
    
    return (
        <form id='editor-input-output' onSubmit={handleSubmit}>
            <div id="input-form">
                <InputEditor className="input-editor" parentCallback={setValueFromChild} activeRule={props.activeRule} customObjects = {props.customObjects} 
                updateTokens={updateTokens}/>
                <div className="wrapper">
                        <br/>
                        <select className="select-css" onChange={selectChange} value={priorityLevel}>
                            <option value={"Recommended"}>Error Level: Recommended</option>
                            <option value={"Warning"}>Error Level: Warning</option>
                            <option value={"Error"}>Error Level: Error</option>
                        </select>
                </div>
                <Button onClick={handleSubmit} variant="outline-primary" size="lg" block>Submit</Button>
                <div className="response-wrapper" style={{display:'block'}}>
                
                    <div id="response-content"> 
                        <div>{responseObjects}</div>
                        <div>{responseObjectChecks}</div>
                        <div>{responseRelations}</div>
                    </div>
                    <div id="retranslation-content">{retranslation}</div>
                </div>
            </div>
        </form>
    )
}