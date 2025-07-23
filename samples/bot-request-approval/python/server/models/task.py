# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import uuid
from datetime import datetime


class Task:
    class Status:
        CREATED = "Created"
        APPROVED = "Approved"
        REJECTED = "Rejected"

    def __init__(
        self,
        id=None,
        title=None,
        description=None,
        created_by=None,
        assigned_to=None,
        created_at=None,
        status=None,
    ):
        self.id = id or str(uuid.uuid4())
        self.title = title
        self.description = description
        self.created_by = created_by
        self.assigned_to = assigned_to
        self.created_at = created_at or datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        self.status = status or self.Status.CREATED

    # Equality check
    def equals(self, other_task):
        return (
            self.get_id() == other_task.get_id()
            and self.get_title() == other_task.get_title()
            and self.get_created_by() == other_task.get_created_by()
            and self.get_assigned_to() == other_task.get_assigned_to()
            and self.get_created_at() == other_task.get_created_at()
            and self.get_status() == other_task.get_status()
        )

    # Fill method to update fields from dictionary

    def fill(self, new_fields: dict):
        key_map = {
            "createdBy": "created_by",
            "assignedTo": "assigned_to",
            "createdAt": "created_at",
        }

        for field, value in new_fields.items():
            attr = key_map.get(field, field)
            if hasattr(self, attr):
                setattr(self, attr, value)

    # Convert to dict for serialization
    def to_dict(self):
        return {
            "id": self.id,
            "title": self.title,
            "description": self.description,
            "createdBy": (
                {
                    "aadObjectId": self.created_by.get("aadObjectId"),
                    "name": self.created_by.get("name"),
                }
                if isinstance(self.created_by, dict)
                else self.created_by
            ),
            "assignedTo": (
                {
                    "aadObjectId": self.assigned_to.get("aadObjectId"),
                    "name": self.assigned_to.get("name"),
                }
                if isinstance(self.assigned_to, dict)
                else self.assigned_to
            ),
            "createdAt": self.created_at,
            "status": self.status,
        }

    # Create a Task from a dictionary
    @classmethod
    def from_dict(cls, data):
        return cls(
            id=data.get("id"),
            title=data.get("title"),
            description=data.get("description"),
            created_by=data.get("createdBy"),
            assigned_to=data.get("assignedTo"),
            created_at=data.get("createdAt"),
            status=data.get("status"),
        )

    def set_status(self, new_status):
        self.status = new_status
