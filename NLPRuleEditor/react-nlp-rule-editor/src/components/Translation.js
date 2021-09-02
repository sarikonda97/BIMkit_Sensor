import {ExistentialClause, Characteristic} from '../CommonDesignRules/ECS'
import { PropertyCheckNumeric, PropertyCheckBool } from '../CommonDesignRules/Property'
import { SingleRelation } from '../CommonDesignRules/ObjectCheck'
import { DoubleRelation } from '../CommonDesignRules/RelationCheck'
import { LogicalExpression } from '../CommonDesignRules/LogicalExpression'

import { Token } from './Token';


export class groupTokens{
    constructor(tokens){
        this.tokens = tokens;
        this.groupedTokens = null;
        this.startIndex = null;
        this.endIndex = null;

        this.ECSTokens = {};

        this.occurrence = null;
        this.type = null;
        this.properties = [];

        this.isAECSProperty = true;

        this.negation = false;

        this.property = null;
        this.propertyType = '';
        this.operation = null;
        this.value = null;
        this.unit = null;

        this.objectChecks = [];
        this.relationChecks = [];
        this.logicalExpression = new LogicalExpression([],[],[], "AND");
        this.currentLogicalExpression = this.logicalExpression;
        this.lastOperator = null;


        this.isARelation = false;
        this.ecs1 = null;
        this.relation = null;
        this.relationType = '';
        this.ecs2 = null;
    }

    groupLogicalExpression = function(){
        this.groupedTokens = this.tokens.slice();
        this.isAECSProperty = false;
        for(let i = 0; i < this.tokens.length; i++){
            this.endIndex = i;
            switch(this.tokens[i].type){
                case 'ecs':
                    if(this.ecs1 == null || this.isARelation == false){
                        this.ecs1 = this.tokens[i].value;
                        this.ecs2 = null;
                        this.negation = false;
                        break;
                    }
                    this.ecs2 = this.tokens[i].value;
                    //build relation
                    this.buildRelation();
                    break;
                case 'negation':
                    this.negation = true;
                    break;
                case 'property':
                    this.handlePropertyToken(this.tokens[i]);
                    if(this.startIndex == null){
                        this.startIndex = i;
                    }
                    break;
                case 'propertybool':
                    this.handlePropertyToken(this.tokens[i]);
                    if(this.startIndex == null){
                        this.startIndex = this.endIndex;
                    }
                    break;
                case 'operation':
                    this.handleOperationToken(this.tokens[i]);
                    if(this.startIndex == null){
                        this.startIndex = i;
                    }
                    break;
                case 'value':
                    this.handleValueToken(this.tokens[i]);
                    if(this.startIndex == null){
                        this.startIndex = i;
                    }
                    
                    break;
                case 'unit':
                    this.handleUnitToken(this.tokens[i]);
                    if(this.startIndex == null){
                        this.startIndex = i;
                    }
                    break;
                case 'relation':
                    this.buildProperty();
                    this.relation = this.tokens[i].value;
                    this.relationType = this.tokens[i].type;
                    this.isARelation = true;
                    break;
                case 'relationnum':
                    this.buildProperty();
                    this.relation = this.tokens[i].value;
                    this.relationType = this.tokens[i].type;
                    this.isARelation = true;
                    break;
                case 'operator':
                    this.handleLogicalOperatorToken(this.tokens[i]);
            }
        }
        // console.log(this.objectChecks);
        // console.log(this.relationChecks);
        this.buildProperty();
        this.currentLogicalExpression.ObjectChecks.push(...this.objectChecks)
        this.currentLogicalExpression.RelationChecks.push(...this.relationChecks);
        console.log(this.logicalExpression);
    }

    handleLogicalOperatorToken = function(token){
        this.buildProperty();
        if(this.lastOperator == null){
            this.lastOperator = token.value;
            this.currentLogicalExpression.LogicalOperator = token.value;
        }
        if(this.lastOperator == token.value){
            //add logical operator
            this.currentLogicalExpression.ObjectChecks.push(...this.objectChecks)
            this.currentLogicalExpression.RelationChecks.push(...this.relationChecks)
            this.objectChecks = [];
            this.relationChecks =[];
        }
        else{
            let newLogicalExpression = new LogicalExpression([],[],[], token.value);
            this.currentLogicalExpression.LogicalExpressions.push(newLogicalExpression);
            this.currentLogicalExpression = this.logicalExpression.LogicalExpressions[0];
            this.currentLogicalExpression.ObjectChecks.push(...this.objectChecks)
            this.currentLogicalExpression.RelationChecks.push(...this.relationChecks)
            this.objectChecks = [];
            this.relationChecks =[];
            
        }
        this.lastOperator = token.value;
    }

    groupECS = function(){
        this.groupedTokens = this.tokens;
        for(let i = 0; i < this.tokens.length; i++){
            this.endIndex = i;
            //this.tokens[i].value = this.tokens[i].value.toLowerCase();
            if(!this.isAECSProperty){
                //this.startIndex = this.endIndex;
            }
            switch(this.tokens[i].type){
                case 'occurrence':
                    this.isAECSProperty = true;
                    this.handleOccuranceToken(this.tokens[i]);
                    if(i != this.endIndex){
                        i = this.endIndex;
                    }
                    if(this.startIndex == null){
                        this.startIndex = this.endIndex;
                    }
                    break;
                case 'type':
                    this.isAECSProperty = true;
                    this.handleTypeToken(this.tokens[i]);
                    if(i != this.endIndex){
                        i = this.endIndex;
                    }
                    if(this.startIndex == null){
                        this.startIndex = this.endIndex;
                    }
                    this.negation = false;
                    break;
                case 'negation':
                    this.negation = true;
                    break;
                case 'property':
                    if(this.isAECSProperty){
                        this.handlePropertyToken(this.tokens[i]);
                        if(this.startIndex == null){
                            this.startIndex = this.endIndex;
                        }
                    }
                    break;
                case 'propertybool':
                    if(this.isAECSProperty){
                        this.handlePropertyToken(this.tokens[i]);
                        if(this.startIndex == null){
                            this.startIndex = this.endIndex;
                        }
                    }
                    break;
                case 'operation':
                    if(this.isAECSProperty){
                        this.handleOperationToken(this.tokens[i]);
                        if(this.startIndex == null){
                            this.startIndex = this.endIndex;
                        }
                    }
                    break;
                case 'value':
                    if(this.isAECSProperty){
                        this.handleValueToken(this.tokens[i]);
                        if(this.startIndex == null){
                            this.startIndex = this.endIndex;
                        }
                    }
                    break;
                case 'unit':
                    if(this.isAECSProperty){
                        this.handleUnitToken(this.tokens[i]);
                        if(this.startIndex == null){
                            this.startIndex = this.endIndex;
                        }
                    }
                    break;
                case 'relation':
                    if(this.isAECSProperty){
                        this.buildProperty();
                        this.buildNewECS();
                        this.resetProperty();
                        this.resetECS();
                        if(i != this.endIndex){
                            i = this.endIndex;
                        }
                    }
                    
                    if(this.tokens[i].value.toLowerCase() == "distance"){
                        this.isAECSProperty = false;
                    }
                    else{
                        this.isAECSProperty = true;
                    }
                    break;
                case 'check':
                    if(this.isAECSProperty){
                        this.buildProperty();
                        this.resetProperty();
                        this.buildNewECS();
                        this.resetECS();
                        if(i != this.endIndex){
                            i = this.endIndex;
                        }
                    }
                    this.isAECSProperty = false;
                    break;
            }
        }

        this.endIndex += 1;
        this.buildProperty();
        this.buildNewECS();
        console.log(this.ECSTokens);
        // console.log(this.groupedTokens);
        this.tokens = this.groupedTokens;
    }
    handleOccuranceToken = function(token){
        if(this.occurrence != null || this.type != null){
            //create property
            this.buildProperty();
            this.buildNewECS();
        }
        this.occurrence = token.value;
    }
    handleTypeToken = function(token){
        if(this.type != null){
            //create property
            this.buildProperty();
            this.buildNewECS();
        }
        this.type = token.value;
    }
    handlePropertyToken = function(token){
        if(this.property != null){
            //create property
            this.buildProperty();
        }
        this.propertyType = token.type;
        this.property = token.value;
    }
    handleOperationToken = function(token){
        if(this.operation != null){
            //create property
            this.buildProperty();
        }
        this.operation = token.value;
    }
    handleValueToken = function(token){
        if(this.value != null){
            //create property
            this.buildProperty();
        }
        this.value = token.value;
    }
    handleUnitToken = function(token){
        if(this.unit != null){
            //create property
            this.buildProperty();
        }
        this.unit = token.value;
    }


    resetECS = function(){
        this.occurrence = null;
        this.type = null;
        this.properties = [];
    }
    resetProperty = function(){
        this.property = null;
        this.propertyType = '';
        this.operation = null;
        this.value = null;
        this.unit = null;
    }

    buildNewECS = function(){
        if(this.type == null){
            return;
        }
        
        let newECS = new ExistentialClause(this.occurrence, new Characteristic(this.type, this.properties))
        //logic about duplicate etc.
        if(this.isDuplicateECSType(newECS)){
            //if occurrence not same, or property not same or this is the second in a relation and the first one is of the same type. then create a new 
            let originalECS =  this.getOriginalECS(newECS);
            if(originalECS != null){
                this.type = originalECS.Characteristic.Type;
            }
            else{
                //create new ecs
                let index = 1;
                while(this.type + index in this.ECSTokens){
                    index ++;
                }
                this.type = this.type + index;
                this.ECSTokens[this.type] = newECS;
            }

        }else{
            if(newECS.OccurrenceRule == null){
                //default
                newECS.OccurrenceRule = "ANY"
            }
            this.ECSTokens[this.type] = newECS;
        }
        
        this.groupedTokens.splice(this.startIndex, this.endIndex-this.startIndex, new Token(this.groupedTokens[this.startIndex].index, this.type,'ecs'));
        this.endIndex = this.startIndex + 1;
        this.resetECS();
        this.startIndex = null;
        return;
    }

    isDuplicateECSType(newECS){
        if(newECS.Characteristic.Type in this.ECSTokens){
            return true;
        }
        return false;        
    }

    getOriginalECS(newECS){
        let type = newECS.Characteristic.Type;
        let index = '';
        while(type + index in this.ECSTokens){
            if(this.newECSIsEqualToOldECS(newECS,this.ECSTokens[type + index])){
                return this.ECSTokens[type + index];
            }

            if(index == ''){
                index = 0;
            }
            index ++;
        }
        return null;
    }

    newECSIsEqualToOldECS = function(newECS, oldECS){
        if(newECS.OccurrenceRule != null && newECS.OccurrenceRule != oldECS.OccurrenceRule){
            return false;
        }
        let same = true;
        //check if new ecs has any properties the old one does not have.
        newECS.Characteristic.PropertyChecks.forEach(property => {
            let hasProperty = false;
            oldECS.Characteristic.PropertyChecks.forEach(oldproperty => {
                if(this.isEqual(property, oldproperty)){
                    hasProperty = true;
                }
            });
            if(!hasProperty){
                same = false;
            }
        });

        return same;
    }

    isEqual(obj1, obj2){
        return JSON.stringify(obj1) === JSON.stringify(obj2);
    }

    buildProperty = function(){
        //for now assume it is a numeric property
        if(this.propertyType == "property"){
            if(this.operation == null){
                this.operation = "EQUAL";
            }
            if(this.value == null){
                return null;
            }
            if(this.unit == null){
                this.unit = "M";
            }
            if(this.property == null){
                return null;
            }
            if(this.isAECSProperty){
                this.properties.push(new PropertyCheckNumeric(this.operation, this.value, this.unit, this.unit, this.property));
            }
            else{

                this.objectChecks.push(new SingleRelation (this.ecs1 , this.negation?"MUST_NOT_HAVE":"MUST_HAVE", new PropertyCheckNumeric(this.operation, this.value, this.unit, this.unit, this.property)));
                this.negation = false;
            }

        }
        else if(this.propertyType == "propertybool"){
            if(this.operation == null){
                this.operation = "EQUAL";
            }
            if(this.property == null){
                return null;
            }
            if(this.isAECSProperty){
                this.properties.push(new PropertyCheckBool(this.operation, this.property));

            }
            else{
                this.objectChecks.push(new SingleRelation (this.ecs1 ,this.negation?"MUST_NOT_HAVE":"MUST_HAVE", new PropertyCheckBool(this.operation, this.property)));
                this.negation = false;
            }
        }
        
        
        this.resetProperty();
        return;
    }

    buildRelation = function(){
        //for now assume it is a numeric property
        if(this.value != null){
            if(this.operation == null){
                this.operation = "EQUAL";
            }
            if(this.unit == null){
                this.unit = "M";
            }
            this.relationChecks.push(new DoubleRelation(this.ecs1, this.ecs2, new PropertyCheckNumeric(this.operation, this.value, this.unit, this.unit, "Center Distance")));
        }
        else{
            this.relationChecks.push(new DoubleRelation(this.ecs1, this.ecs2, new PropertyCheckBool(this.negation?"MUST_NOT_HAVE":"MUST_HAVE", this.relation)));

        }

        this.negation = false;
        this.resetProperty();
        this.ecs2 = null;
        this.isARelation = false;
        this.relation = null;
        return;
    }
}