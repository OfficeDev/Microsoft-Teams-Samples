const moment = require('moment');
const { v4 } = require('uuid');
function TaskDTO(task_title, task_description) {
    this.taskId = v4();
    this.taskTitle = task_title;
    this.taskDescription = task_description;
    this.createDate = moment().format('DD MMM YYYY HH:MM:SS')
    this.isDevelopmentRequired = false;
    this.isApprovalRequired = false;
}

module.exports = {
    TaskDTO: TaskDTO
}