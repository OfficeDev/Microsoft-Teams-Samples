# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import uuid
from datetime import datetime

class Incident:
    STATUS = {
        'created': 'Created',
        'approved': 'Approved',
        'rejected': 'Rejected'
    }

    def __init__(self, id=None, title=None, category=None, sub_category=None,
                 created_by=None, assigned_to=None, created_at=None, status=None):
        self.id = id or str(uuid.uuid4())
        self.title = title
        self.category = category
        self.sub_category = sub_category
        self.created_by = created_by
        self.assigned_to = assigned_to
        self.created_at = created_at or datetime.now().strftime('%Y-%m-%d %H:%M:%S')
        self.status = status or self.STATUS['created']

    def fill(self, new_fields: dict):
        for field, value in new_fields.items():
            if value is not None:
                setter = getattr(self, f"set_{field}", None)
                if callable(setter):
                    setter(value)
                elif hasattr(self, field):
                    setattr(self, field, value)

    def to_dict(self):
        return {
            "id": self.id,
            "title": self.title,
            "category": self.category,
            "sub_category": self.sub_category,
            "created_by": self.created_by,
            "assigned_to": self.assigned_to,
            "created_at": self.created_at,
            "status": self.status,
        }

    @classmethod
    def from_dict(cls, data):
        return cls(
            id=data.get("id"),
            title=data.get("title"),
            category=data.get("category"),
            sub_category=data.get("sub_category"),
            created_by=data.get("created_by"),
            assigned_to=data.get("assigned_to"),
            created_at=data.get("created_at"),
            status=data.get("status"),
        )

    def set_status(self, status):
        if status in self.STATUS.values():
            self.status = status
        else:
            raise ValueError(f"Invalid status: {status}")
