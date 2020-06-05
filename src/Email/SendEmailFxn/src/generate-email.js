// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

const {format} = require('date-fns');
const {encode} = require('ent');
const {loadEmailTemplates} = require('./email-templates');
const emailParser = require("email-addresses");

function generateEmail(context, templateName, subject, doc) {

    const emailTemplates = loadEmailTemplates(context);
    const template = emailTemplates[templateName];
    if(!template) {
        context.log.error(`No email template found for ${templateName}`);
        return;
    }

    const useContactEmail =
        templateName === 'nominationApproved' ||
        templateName === 'nominationRejected';
    const {contactName, contactInfo} = doc;
    const toEmail = useContactEmail ?
        toAddressObject(`${contactInfo}; ${process.env['ToEmail']}`, contactName) :
        toAddressObject(process.env['ToEmail']);
    const fromEmail = toAddressObject(process.env['FromEmail'])[0];
    if(toEmail.length === 0) {
        context.log.error(`No 'to' address found for email.`);
        return;
    }

    const html = replaceValuesInEmailTemplate(doc, template);
    const text = createEmailText(doc);

    const msg = {
        personalizations: [
            {
                "to": toEmail
            }
        ],
        from: fromEmail,
        subject,
        content: [
            {
                type: 'text/plain',
                value: text,
            },
            {
                type: 'text/html',
                value: html,
            }
        ]
    };

    const {outputEmail} = context.bindings;
    context.bindings.outputEmail = [
        ...(outputEmail || []),
        msg
    ];
}

function isEmpty(value) {
    return value === null || value === undefined;
}

function formatContent(value, encodeContent = true) {
    return isEmpty(value) ? '' : (encodeContent ? encode(value.toString()) : value);
}

function replaceValuesInEmailTemplate(doc, template) {
    const props = propFormatter(doc);
    const propRegex = /##([a-z0-9_]+)##/gi;
    return template.replace(propRegex, (m, name) => formatContent(props[name], !isHtmlContent(name)));
}

function isHtmlContent(name) {
    switch(name) {
        case "description":
        case "sourceUri":
            return true;
        default:
            return false;
    }
}

function propFormatter(doc) {
    return Object.assign({}, doc, {
        description: formatContent(doc.description).replace(/(\n|&#10;)/g, '<br />\n'),
        tags: (doc.tags || []).join(', '),
        sourceUri: isEmpty(doc.sourceUri) ? '' : `<a href="${doc.sourceUri}">${doc.sourceUri}</a>`,
        published: isEmpty(doc.published) ? '' : format(new Date(doc.published), 'MMM D, YYYY'),
        created: isEmpty(doc.created) ? null : format(new Date(doc.created), 'MMM D, YYYY'),
        modified: isEmpty(doc.modified) ? null : format(new Date(doc.modified), 'MMM D, YYYY'),
        userName: doc.userName || doc.createdByUserName,
        userEmail: doc.userEmail || doc.createdByUserEmail,
        _ts: format(new Date(doc._ts * 1000), 'ddd, MMM D, YYYY h:mm:ss a') + ' (UTC)',
    });
}

function toAddressObject(value, defaultName) {
    return value
        .split(/\s*;\s*/)
        .map((addrText) => {

            const result = emailParser.parseOneAddress(addrText);
            if(!result) {
                return null;
            }

            const {name, address: email} = result;
            if(!email) {
                return null;
            }

            return {
                name: name || defaultName || undefined,
                email
            };
        })
        .filter(a => a)
        .reduce((lst, itm) => {
            if(!lst.find(a => a.email === itm.email)) {
                lst = [...lst, itm];
            }
            return lst;
        }, []);
}

function createEmailText(doc) {
    const {id, name, description} = doc;
    return [
        ['Id', id],
        ['Name', name],
        ['Description', description],
    ].map(([hdr, value]) => {
        return `${hdr}:\t${value}`;
    }).join('\n');
}

module.exports = {
    generateEmail
};
