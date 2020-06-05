// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

const mjml2html = require('mjml');
const {promisify} = require('util');
const {
    readFile: readFileCallback,
    writeFile: writeFileCallback,
    mkdir: mkdirCallback
} = require('fs');
const rimrafCallback = require('rimraf');
const {join} = require('path');

const readFile = promisify(readFileCallback);
const writeFile = promisify(writeFileCallback);
const mkdir = promisify(mkdirCallback);
const rimraf = promisify(rimrafCallback);

const files = [
    "dataset-issue.html",
    "dataset-nomination.html",
    "general-issue.html",
    "nomination-approved.html",
    "nomination-rejected.html",
];

const mjmlOptions = {
    keepComments: false,
    minify: true
};

main()
    .then(() => console.log("Finished."))
    .catch(err => console.error(err));

async function main() {

    const buildPath = join(__dirname, '..', 'build');
    await rimraf(buildPath);
    await mkdir(buildPath);

    for(const file of files) {
        console.log(`- ${file}`);
        const fileName = join(__dirname, '..', 'src', file);
        const content = await readFile(fileName, 'utf8');
        const output = mjml2html(content, mjmlOptions);
        if(output.errors && output.errors.length) {
            for(const error of output.errors) {
                console.error(error);
            }
            throw new Error('Error in generating output.');
        }

        const outputName = join(buildPath, file);
        await writeFile(outputName, output.html, 'utf8');
    }
}
