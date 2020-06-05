// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


const DatasetNominationTemplateId = '286353d1-2d54-4d25-8930-00465136cb96';
const DatasetIssueTemplateId = 'ab4313a8-b3fc-447a-bf61-413e6a6c983f';
const NominationApprovedTemplateId = '0926ee72-a168-4ae5-afb3-12e46d09e263';
const NominationRejectedTemplateId = '8b891dda-6d5f-4bcb-ab11-b5f0ebd61dcd';
const GeneralIssueTemplateId = 'e63bd247-16f6-45d9-bc49-0ad1372963c9';

const idMap = new Map([
    [DatasetNominationTemplateId, 'nomination'],
    [DatasetIssueTemplateId, 'issue'],
    [NominationApprovedTemplateId, 'nominationApproved'],
    [NominationRejectedTemplateId, 'nominationRejected'],
    [GeneralIssueTemplateId, 'generalIssue'],
]);

let loadedTemplates = null;

// List retrieved by query from emailTemplates binding
// SELECT c.id, c.html FROM c WHERE (c.dataType = 'email-templates' AND c.datasetId = 'fd56f7c8-89a5-4997-82bc-95e955468e14')
function loadEmailTemplates(context) {
    const loadTemplates = () => {
        const {emailTemplates} = context.bindings;
        return emailTemplates.reduce((v, {id, html}) => {
            const key = idMap.get(id);
            if(key) {
                v = Object.assign(v, {
                    [key]: html
                });
            }
            return v;
        }, {});
    };
    loadedTemplates = loadedTemplates || loadTemplates();
    return loadedTemplates;
}

module.exports = {
    loadEmailTemplates
};
