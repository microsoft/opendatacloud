
const {evaluateDocument} = require('../src/evaluate-document');

const logPrefix = '[SEND-EMAIL]';

function SendEmailTrigger(context) {
    try {
        const { inputDoc } = context.bindings;
        if(!inputDoc || !inputDoc.length) {
            context.log(`${logPrefix} Received no modified documents.`);
        } else {
            inputDoc.forEach(doc => {
                evaluateDocument(context, doc);
            });
        }
        context.done();
    }
    catch(err) {
        context.done(err);
    }
}

module.exports = SendEmailTrigger;
