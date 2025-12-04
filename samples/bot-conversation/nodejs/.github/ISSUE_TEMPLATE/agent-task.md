name: Copilot Agent Task
description: Define a scoped task for GitHub Copilot Agent to complete.
title: "[Agent Task] "
labels: agent-task
body:
  - type: textarea
    id: task_description
    attributes:
      label: Task description
      description: Provide clear and specific instructions for the agent.
    validations:
      required: true
 