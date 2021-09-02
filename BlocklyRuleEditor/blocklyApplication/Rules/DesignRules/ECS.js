
class ExistentialClause
{
    constructor(occuranceRule, characteristic){
        this.OccurrenceRule = occuranceRule;
        this.Characteristic = characteristic;
    }
    set occurenceRule(x) {
        this.OccurrenceRule = x;
    }
    set characteristic(x) {
        this.Characteristic = x;
    }

    
}



class Characteristic
{
    constructor(type, propertyChecks){
        this.Type = type;
        this.PropertyChecks = propertyChecks;
    }
    set type(x) {
        this.Type = x;
    }
    set propertyChecks(x) {
        this.PropertyChecks = x;
    }
}