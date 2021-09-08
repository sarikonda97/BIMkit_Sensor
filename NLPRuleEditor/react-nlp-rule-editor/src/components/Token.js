import {lemmatizedToken} from './regexTokens';

export class Token {
    constructor(index, value, type ='') {
      this.index = index;
      this.value = value;
      this.type = type;
    }
}


export class Tokens{
    updateTokensOfType(type, newTokens){
        this[type] = newTokens;
    }
    lemmatizeTokens(){
        for(var key in this){
            let tokens = this[key];
            for(var i = 0; i < tokens.length; i++){
                tokens[i] = lemmatizedToken(tokens[i]);
            }
        }
    }
    
    get allTokens(){
        let allTokens = [];
        for(var key in this){
            allTokens = allTokens.concat(this[key]);
        }
        return allTokens;
    }
    get allTokensSortedByIndex(){
        let allTokens = this.allTokens;
        allTokens.sort(function(token1, token2){
            let index1 = token1.index;
            let index2 = token2.index;
            if(index1 < index2){
                return -1;
            }
            if(index1 > index2){
                return 1;
            }
            return 0;
        });
        return allTokens;
    }
}
/*
class TokenGrouper{
    constructor(sortedTokens){
        this.tokens = sortedTokens;
    }
    groupTokens = function(){
        this.groupECS();
        //this.groupLogicalExpressions();
    }

    groupECS = function(){
        this.groupedTokens = this.tokens;
        this.startIndexOfECS = null;
        this.endIndexOfECS = null;

        for(let i = 0; i < this.tokens.length; i++){
            switch(this.tokens[i].type){
                case 'occurrence':
                    this.handleOccurrenceToken(this.tokens[i]);
                    break;
                case 'type':
                    this.handleTypeToken(this.tokens[i]);
                    break;
                case 'property':
                    this.handlePropertyToken(this.tokens[i]);
                    break;
                case 'propertybool':
                    this.handlePropertyToken(this.tokens[i]);
                    break;
                case 'operation':
                    this.handleOperationToken(this.tokens[i]);
                    break;
                case 'value':
                    this.handleValueToken(this.tokens[i]);
                    break;
                case 'unit':
                    this.handleUnitToken(this.tokens[i]);
                    break;
                case 'relation':
                    this.handleRelationToken(this.tokens[i]);
                    break;
                case 'check':
                    this.handleCheckToken(this.tokens[i]);
                    break;
            }
        }
    }

    handleOccurrenceToken = function(occuranceToken) {
        this.currentOccurrence = occuranceToken.value;
    }
    handleTypeToken = function(occuranceToken) {
        this.currentOccurrence = occuranceToken.value;
    }
    handlePropertyToken = function(occuranceToken) {
        this.currentOccurrence = occuranceToken.value;
    }
    handleOperationToken = function(occuranceToken) {
        this.currentOccurrence = occuranceToken.value;
    }
    handleOperationToken = function(occuranceToken) {
        this.currentOccurrence = occuranceToken.value;
    }
}


export class Translator{
    constructor() {
        this.occuranceTokens = [];
        this.typeTokens = [];
        this.negationTokens = [];
        this.propertyTokens = [];
        this.propertyBoolTokens = [];
        this.operationTokens = [];
        this.valueTokens = [];
        this.unitTokens = [];
        this.logicalOperatorTokens = [];
        this.relationTokens = [];
        this.relationNumTokens = [];
        this.checkTokens = [];
        this.unknownTokens = [];
    }
}*/
