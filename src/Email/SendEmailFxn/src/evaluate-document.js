// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

const {generateEmail} = require('./generate-email');

const logPrefix = '[EVAL-DOC]';

function evaluateDocument(context, doc) {
    const {dataType} = doc;
    switch(dataType) {
        case "dataset-nomination":
            handleNominationDocument(context, doc);
            break;
        case "dataset-issue":
            handleDatasetIssueDocument(context, doc);
            break;
        case "general-issue":
            handleGeneralIssueDocument(context, doc);
            break;
        default:
            context.log(`${logPrefix} Document data type, "${dataType}", ignored`);
            break;
    }
}

function handleNominationDocument(context, doc) {
    const {
        id,
        nominationStatus: status,
        sentNominationNotice,
        sentFollowUpNotice,
        contactName,
        contactInfo
    } = doc;
    context.log(`${logPrefix} Processing nomination ${id}`);
    let sentEmail = false;

    if(!status || status === 'Pending Approval' || status === 'PendingApproval') {
        if(!sentNominationNotice) {
            context.log(`${logPrefix} Sending new dataset nomination email.`);
            generateEmail(context, 'nomination', 'New Dataset Nomination', doc);
            addOutputDocument(context, Object.assign(doc, {
                sentNominationNotice: new Date().toUTCString()
            }));
            sentEmail = true;
        }
    } else if(status === 'Complete') {
        if(!sentFollowUpNotice) {
            context.log(`${logPrefix} Sending dataset complete follow-up email.`);
            generateEmail(context, 'nominationApproved', 'Dataset Nomination Status', doc);
            addOutputDocument(context, Object.assign(doc, {
                sentFollowUpNotice: new Date().toUTCString()
            }));
            sentEmail = true;
        }
    } else if(status === 'Rejected') {
        if(!sentFollowUpNotice) {
            context.log(`${logPrefix} Sending dataset rejected follow-up email.`);
            generateEmail(context, 'nominationRejected', 'Dataset Nomination Status', doc);
            addOutputDocument(context, Object.assign(doc, {
                sentFollowUpNotice: new Date().toUTCString()
            }));
            sentEmail = true;
        }
    }

    if(!sentEmail) {
        context.log(`${logPrefix} No action taken.`);
    }
}

function handleDatasetIssueDocument(context, doc) {
    const {id, sentIssueNotice} = doc;
    context.log(`${logPrefix} Processing dataset issue ${id}`);
    if(!sentIssueNotice) {
        context.log(`${logPrefix} Sending new dataset issue email.`);
        generateEmail(context, 'issue', 'New Dataset Issue', doc);
        addOutputDocument(context, Object.assign(doc, {
            sentIssueNotice: new Date().toUTCString()
        }));
    } else {
        context.log(`${logPrefix} No action taken.`);
    }
}

function handleGeneralIssueDocument(context, doc) {
    const {id, sentIssueNotice} = doc;
    context.log(`${logPrefix} Processing general issue ${id}`);
    if(!sentIssueNotice) {
        context.log(`${logPrefix} Sending new general issue email.`);
        generateEmail(context, 'generalIssue', 'New Feedback', doc);
        addOutputDocument(context, Object.assign(doc, {
            sentIssueNotice: new Date().toUTCString()
        }));
    } else {
        context.log(`${logPrefix} No action taken.`);
    }
}

function addOutputDocument(context, doc) {
    const {outputDoc} = context.bindings;
    context.bindings.outputDoc = [
        ...(outputDoc || []),
        doc
    ];
}

module.exports = {
    evaluateDocument
};
