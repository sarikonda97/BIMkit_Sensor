# Overview
A rule editor web application used for creating design rules from natural language.

Old documentation: [here](https://ualberta-cmput401.github.io/natural-language-rules/).

## Development
### Set Up
1. Clone repo
2. Download and install node.js and npm: https://nodejs.org/en/download/
3. Open react-nlp-rule-editor folder in vs code or another ide.
4. In the terminal use command: npm install 
5. Now the application can be started with command: npm start. (make sure to open bimkit solution first so that it can connect to the database)

### How To Use
Write a rule in the text box and press submit. This will update the current rule that is open. To create a new rule open the sidebar on the left and click the + symbol. To export the rule click the export button in the sidebar.

The natural language text is translated into a rule by tokenizing important words and then grouping the tokens into design rule components such as property, ecs, relation etc.

The tokenizing works by iterating through the text and matching regular expressions to each word. If the regex matches then that word is hgihlighted with the appropriate color.
These regular expressions can be found in regexTokens.js. If you choose to add another word to the regex it is important to also add it to the mapping property. The mapping property is a dictionary with a key == a word in the regex expression and a value == what is written into the json document. 

The grouping works by using the order that the tokens come in and the type of token to create design rule components. 
A few things to keep in mind when writing natural language rules:
1. Properties and objectchecks are very similar to make a distinction between the two it is important to use a OBJECTCHECK token (i.e. "must", "should", "require", "need") before object checks. This means a property would be written as: All Chairs with length of 9 inches. A objectcheck is written as: All Chairs **must** have length of 9 inches
2. Make sure that a relation token (ie. "above", "below", etc.) always has an ECS before and after it.
3. If a token is not being highlighted with the appropriate colour try a different spelling or updating the regex expression in regexTokens.js

