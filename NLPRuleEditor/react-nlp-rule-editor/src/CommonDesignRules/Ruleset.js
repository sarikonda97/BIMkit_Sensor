
class RuleSet{
    constructor(ID, name){
        this.ID = ID
        this.Name = name;
        this.Rules = [];
    }
    set name(x){
        this.Name = x;
    }
    set rules(x){
        this.Rules = x;
    }
    set xmlstring(x){
        this.xmlstring = x;
    }
    //adds rule to ruleset
    AddRule(rule){
        this.Rules.push(rule);
    }
    update(tempRule, rule) {
        // Once we have received a rule object from translation phase, replace it in the list
        let index = this.Rules.indexOf(tempRule);
        this.Rules[index] = rule;
    }
    removeRule(rule) {
    // Remove the rule from the list
    let index = this.Rules.findIndex(rule);
    this.Rules.pop(index);
    }
    //checks if the current rule set is valid
    isValid(){
        var is_valid = true;
        //iterates every rule insided the set and checks if it is valid
        this.Rules.forEach(element => {
            if(!element.valid){
                is_valid = false;
                return false;
            }
        });
          
        return is_valid;
    }
    toJSON(){
        return {
            Rules: this.Rules,
            Name: this.Name,
            Description: ""};
    }
}

