var mongoose = require('mongoose');
 
module.exports = mongoose.model('Gitconfig',{
    guid:String,
    groupName:String,
    webhookUrl: String,
    repoName: String
});