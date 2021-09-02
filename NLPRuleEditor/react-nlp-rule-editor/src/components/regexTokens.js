import {fetchData} from '../tools/Fetch';
import {LogicalOperator, OccuranceRule, PropertyNegation, RelationNegation, Unit, OperatorNum, OperatorString} from '../CommonDesignRules/DesignRuleEnums'


const tokenTypes = {OCCURRENCE : 'occurrence', TYPE: 'type', NEGATION: 'negation', OPERATION: 'operation', PROPERTY_BOOLEAN: 'propertybool', 
PROPERTY: 'property', VALUE: 'value', UNIT: 'unit', LOGICAL_OPERATOR:'operator', OBJECTCHECK:'check', RELATION:'relation', UNKNOWN: 'unknown'}

export function lemmatizedToken(token){
    let mapping = getMappingOfType(token.type)
    if(token.value.toLowerCase() in mapping){
        token.value = mapping[token.value.toLowerCase()];
    }
    return token;
}
function getMappingOfType(type){
    console.log(type == tokenTypes.TYPE);
    switch(type){
        case tokenTypes.OCCURRENCE:
            return OCCURRENCE_MATCH.mapping;
        case tokenTypes.TYPE:
            return TYPE_MATCH.mapping;
        case tokenTypes.NEGATION:
            return NEGATION_MATCH.mapping;
        case tokenTypes.OPERATION:
            return OPERATION_MATCH.mapping;

        case tokenTypes.PROPERTY_BOOLEAN:
            return PROPERTY_BOOLEAN_MATCH.mapping;
        case tokenTypes.PROPERTY:
            return PROPERTY_MATCH.mapping;

        case tokenTypes.VALUE:
            return VALUE_MATCH.mapping;

        case tokenTypes.UNIT:
            return UNIT_MATCH.mapping;

        case tokenTypes.LOGICAL_OPERATOR:
            return LOGICAL_OPERATOR_MATCH.mapping;

        case tokenTypes.OBJECTCHECK:
            return OBJECTCHECK_MATCH.mapping;

        case tokenTypes.RELATION:
            return RELATION_MATCH.mapping;
    }
    return {};
}
export var OCCURRENCE_MATCH = { 
    regex: RegExp('\\b(ANY|ALL|NONE)\\b', 'gi'),
    type: tokenTypes.OCCURRENCE,
    mapping: {any:OccuranceRule.ANY, all:OccuranceRule.ALL, none:OccuranceRule.NONE}
}

const defaultTypeValues = ["chair","sofa","oven","microwave","windows","sink"];
export var TYPE_MATCH = {
    regex: RegExp('\\b('+arrayOfStringsToSingleString(defaultTypeValues,'|')+')(s)?\\b', 'gi'),
    type: tokenTypes.TYPE,
    mapping: {...mapStringsToStrings(getArrayOfStringsWithSuffix(defaultTypeValues, 's'), defaultTypeValues), ...mapStringsToStrings( defaultTypeValues.map(v => v.toLowerCase()),defaultTypeValues)}
}

export var NEGATION_MATCH = { 
    regex: RegExp('\\b(Not|NO)\\b', 'gi'),
    type: tokenTypes.NEGATION,
    mapping: {not:PropertyNegation.MUST_NOT_HAVE, no:PropertyNegation.MUST_NOT_HAVE}
}
export var OPERATION_MATCH = { 
    regex: RegExp('\\b(less than( or equal)?|more than|GREATER THAN|GREATER THAN or equal|more than or equal|equal|<|<=|=|>=|>|!=|contains)\\b', 'gi'),
    type: tokenTypes.OPERATION,
    mapping: {'less than':OperatorNum.LESS_THAN, 'less than or equal':OperatorNum.LESS_THAN_OR_EQUAL, 'more than':OperatorNum.GREATER_THAN, 
    'more than or equal':OperatorNum.GREATER_THAN_OR_EQUAL, 'greater than or equal':OperatorNum.GREATER_THAN_OR_EQUAL, 'greater than':OperatorNum.GREATER_THAN,
    '<':OperatorNum.LESS_THAN,'<=':OperatorNum.LESS_THAN_OR_EQUAL,'=':OperatorNum.EQUAL,'>=': OperatorNum.GREATER_THAN_OR_EQUAL,'>':OperatorNum.GREATER_THAN,
    '!=':OperatorNum.NOT_EQUAL,'equal':OperatorNum.EQUAL}
}

const defaultBoolPropertyValues = ['Door'];
export var PROPERTY_BOOLEAN_MATCH = {
    regex: RegExp('\\b(Door)\\b', 'gi'),
    type: tokenTypes.PROPERTY_BOOLEAN,
    mapping: {door:'HasDoor'}
}


export var VALUE_MATCH = {
    regex: RegExp('\\b[0-9]+(\.[0-9]+)?','gi'),
    type: tokenTypes.VALUE,
    mapping: {}
}
export var UNIT_MATCH = {
    regex: RegExp('(?<=([0-9]|\\b))(MM|CM|M|INCH(es)?|FT|DEG|RAD|millimeter|centimeter|meter|feet|degree|radian)\\b', 'gi'),
    type: tokenTypes.UNIT,
    mapping: {mm:Unit.MM, cm: Unit.CM, m:Unit.M, inch:Unit.INCH, inches:Unit.INCH, ft:Unit.FT, deg:Unit.DEG, rad:Unit.RAD, millimeter:Unit.MM, 
        centimeter: Unit.CM, meter:Unit.M, feet:Unit.FT, degree:Unit.DEG, radian:Unit.RAD}
}

export var LOGICAL_OPERATOR_MATCH = {
    regex: RegExp('\\b(AND|OR|XOR)\\b', 'gi'),
    type: tokenTypes.LOGICAL_OPERATOR,
    mapping: {and: LogicalOperator.AND, or: LogicalOperator.OR, xor: LogicalOperator.XOR}

}
export var OBJECTCHECK_MATCH = {
    regex: RegExp('\\b(MUST|SHOULD|REQUIRE(S)?|need(s)?)\\b', 'gi'),
    type: tokenTypes.OBJECTCHECK,
    mapping: {}
}

const defaultPropertyValues = ['Property',"FunctionOfObj","Width","Height","Length"]
export var PROPERTY_MATCH = {
    regex: RegExp('\\b('+arrayOfStringsToSingleString(defaultPropertyValues,'|')+')\\b', 'gi'),
    type: tokenTypes.PROPERTY,
    mapping: mapStringsToStrings( defaultPropertyValues.map(v => v.toLowerCase()),defaultPropertyValues)
}

export var RELATION_MATCH = {
    regex: RegExp('\\b(Next|Above|DISTANCE|facing|inside|behind)\\b', 'gi'),
    type: tokenTypes.RELATION,
    mapping: {next: 'IsNextTo', above: "IsAbove", facing: "Facing", inside:"IsInside",behind:"IsBehind",distance: "Distance"}
}

export var UNKNOWN_MATCH = RegExp('Unknown', 'gi')


function getArrayOfStringsWithPrefix(arrayOfStrings, prefix){
    let stringsWithPrefix = [];
    for(var i = 0; i < arrayOfStrings.length; i++){
        stringsWithPrefix.push(prefix + arrayOfStrings[i]);
    }
    return stringsWithPrefix;
}
function getArrayOfStringsWithSuffix(arrayOfStrings, suffix){
    let stringsWithSuffix = [];
    for(var i = 0; i < arrayOfStrings.length; i++){
        stringsWithSuffix.push(arrayOfStrings[i] + suffix);
    }
    return stringsWithSuffix;
}
function mapStringsToStrings(arrayOfStrings1, arrayOfStrings2){
    let map = {}
    let length = Math.max(arrayOfStrings1.length, arrayOfStrings2.length);

    for(var i = 0; i < length; i++){
        map[arrayOfStrings1[i].toLowerCase()] = arrayOfStrings2[i];
    }
    return map;
}

function arrayOfStringsToSingleString(arrayOfStrings, separator){
    var singleString = '';
    for(var i =0; i < arrayOfStrings.length; i++){
        singleString += arrayOfStrings[i];
        if(i != arrayOfStrings.length-1){
            singleString += separator;
        }
    }
    return singleString;
}
function seperateStringAtCamelCase(string, seperator){
    for(var i = 1; i < string.length; i++){
        if(string[i].toLowerCase() != string[i]){
            string = string.slice(0, i) +' '+ string.slice(i)
            i++;
        }
    }
    return string;
}

function getKeysOfTypes(data, types){
    let returnArray = [];
    var keys = Object.keys(data);
    var values = Object.values(data);
    for(var i =0; i < keys.length; i++){
        //iterate through wanted data types
        for(var j = 0; j < types.length; j++){
            //if the methods type is acceptable add to return array
            if(values[i].toLowerCase().includes(types[j].toLowerCase())){
                returnArray.push(keys[i]);
            }
        }
    }
    return returnArray;
}

async function initalizeAllRegexMatches(){
    try{
        let typeMethods = await fetchData("https://localhost:44370/api/method/type");
        let typeValues = Object.values(typeMethods);

        let typeValuesSperated = typeValues.map(v => seperateStringAtCamelCase(v,' '));
        TYPE_MATCH.regex = RegExp('\\b('+arrayOfStringsToSingleString(typeValuesSperated,'|')+')(s)?\\b', 'gi');
        TYPE_MATCH.mapping = {...mapStringsToStrings(getArrayOfStringsWithSuffix(typeValuesSperated, 's'), typeValues), ...mapStringsToStrings( typeValuesSperated.map(v => v.toLowerCase()),typeValues)}

        console.log(TYPE_MATCH)
    }
    catch{
        console.log("Failed to fetch type methods.");
    }

    try{
        let properties = await fetchData("https://localhost:44370/api/method/property");

        let propertyValues = getKeysOfTypes(properties, ['Double']);
        let propertyValuesSperated = propertyValues.map(v => seperateStringAtCamelCase(v,' '));
        PROPERTY_MATCH.regex = RegExp('\\b('+arrayOfStringsToSingleString(propertyValuesSperated,'|')+')\\b', 'gi');
        PROPERTY_MATCH.mapping =  mapStringsToStrings( propertyValuesSperated.map(v => v.toLowerCase()),propertyValues);
        console.log(PROPERTY_MATCH)

        let boolPropertyValues = getKeysOfTypes(properties,['Boolean'])
        let boolPropertyValuesSperated = boolPropertyValues.map(v => seperateStringAtCamelCase(v,' '));
        PROPERTY_BOOLEAN_MATCH.regex = RegExp('\\b('+arrayOfStringsToSingleString(boolPropertyValuesSperated,'|')+')\\b', 'gi');
        PROPERTY_BOOLEAN_MATCH.mapping =  mapStringsToStrings( boolPropertyValuesSperated.map(v => v.toLowerCase()),boolPropertyValues);
        console.log(PROPERTY_BOOLEAN_MATCH)
    }
    catch{
        console.log("Failed to fetch property methods.")
    }

    try{
        let relations = await fetchData("https://localhost:44370/api/method/relation");

        let relationValues = getKeysOfTypes(relations,['Boolean','Double', 'String'])
        let relationValuesSperated = relationValues.map(v => seperateStringAtCamelCase(v,' '));
        RELATION_MATCH.regex = RegExp('\\b('+arrayOfStringsToSingleString(relationValuesSperated,'|')+')\\b', 'gi');
        RELATION_MATCH.mapping =  mapStringsToStrings( relationValuesSperated.map(v => v.toLowerCase()),relationValues);
        console.log(RELATION_MATCH)
    }
    catch{
        console.log("Failed to fetch relation methods.")
    }
}

initalizeAllRegexMatches();