export const ErrorLevel = {Recommended : 'Recommended', Warning : 'Warning', Error : 'Error'}
//Used by Rule, LogicalExpression
export const LogicalOperator = {AND : 'AND', OR : 'OR', XOR : 'XOR'}
//Used by ECS
export const OccuranceRule = {ALL : 'ALL', ANY : 'ANY', NONE : 'NONE'}
//Used by Property
export const PropertyNegation = {MUST_HAVE : 'EQUAL', MUST_NOT_HAVE : 'NOT_EQUAL'}
//Used by Relation
export const RelationNegation = {MUST_HAVE : 'MUST_HAVE', MUST_NOT_HAVE : 'MUST_NOT_HAVE'}
//Used by Property, Relation
export const Unit = {MM:'MM', CM:'CM', M:'M', INCH:'INCH', FT:'FT', DEG:'DEG', RAD:'RAD'}
export const OperatorNum = {GREATER_THAN : 'GREATER_THAN', GREATER_THAN_OR_EQUAL : 'GREATER_THAN_OR_EQUAL', EQUAL : 'EQUAL', LESS_THAN : 'LESS_THAN', LESS_THAN_OR_EQUAL : 'LESS_THAN_OR_EQUAL', NOT_EQUAL : 'NOT_EQUAL'}
export const OperatorString = {EQUAL:'EQUAL', NOT_EQUAL:'NOT_EQUAL', CONTAINS:'CONTAINS'}

