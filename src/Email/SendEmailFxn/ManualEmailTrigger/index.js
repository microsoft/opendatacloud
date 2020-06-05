
const {evaluateDocument} = require('../src/evaluate-document');

function ManuallySendEmail(context) {
    try {
        const { inputDoc } = context.bindings;
        evaluateDocument(context, inputDoc[0]);
        context.done();
    }
    catch(err) {
        context.done(err);
    }
}

module.exports = ManuallySendEmail;
