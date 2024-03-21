'use strict'

const fs = require('fs');

if (process.argv.length < 5) {
  console.error('Wrong format. Usage: node ./extract.js <env> <group> <key>');
  process.exit(1);
}

const env = process.argv[2];
const group = process.argv[3];
const key = process.argv[4];

if (!env || !group || !key) {
  console.error('Invalid env, group or key entered.')
  process.exit(1);
}

const rawData = fs.readFileSync(`.fx/states/state.${env}.json`);
const config = JSON.parse(rawData);

const candidate = config[group]?.[key];
if (!candidate) {
  console.error(`Cannot find the config by config[${group}][${key}].`);
  process.exit(1);
}

console.log(candidate);
