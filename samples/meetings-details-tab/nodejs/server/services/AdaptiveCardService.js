const {CardFactory } = require('botbuilder');
const ACData = require("adaptivecards-templating");
const fs = require('fs');
const createAdaptiveCard = (fileName, taskInfo, percentOption1, percentOption2) => {
    const jsonContentStr = fs.readFileSync(`resources/${fileName}`, 'utf8');
    const cardJson = JSON.parse(jsonContentStr);
    const template = new ACData.Template(cardJson);
    const cardPayload = template.expand({
      $root: {
            title: taskInfo.title,
            option1: taskInfo.option1,
            option2: taskInfo.option2,
            Id: taskInfo.Id,
            percentoption1: percentOption1,
            percentoption2: percentOption2       
        }
    }); 
    return CardFactory.adaptiveCard(cardPayload);
  }
  module.exports ={
    createAdaptiveCard
  }