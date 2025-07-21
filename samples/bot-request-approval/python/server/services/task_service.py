# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from server.models.task import Task
from server.services import storage_service

inc_key = "incidents_key"
tasks = []


async def save_task(action, members):
    # Match the assigned person from AAD Object ID
    assigned_to = next(
        (m for m in members if m.id == action["data"]["inc_assigned_to"]), None
    )

    title = action["data"].get("inc_title", "")
    description = action["data"].get("inc_description", "")
    created_by = action["data"].get(
        "inc_created_by", {}
    )  # Assuming it's already a dict

    global tasks
    if storage_service.store_check(inc_key):
        tasks = storage_service.store_fetch(inc_key)

    new_inc = Task(
        id=None,
        title=title,
        description=description,
        created_by=created_by,  # Should be a dict like { name, aadObjectId }
        assigned_to=(
            {"name": assigned_to.name, "aadObjectId": assigned_to.id}
            if assigned_to
            else None
        ),
        created_at=None,
        status=None,
    )

    tasks.append(new_inc)
    storage_service.store_save(inc_key, tasks)

    return new_inc

async def update_inc(action, members):
    assigned_to = next(
        (m for m in members if m.id == action["data"]["inc_assigned_to"]),
        None,
    )

    old_task = action["data"]["task"]
    title = action["data"]["inc_title"]
    description = action["data"]["inc_description"]
    created_by = action["data"]["inc_created_by"]

    global tasks
    if storage_service.store_check(inc_key):
        tasks = storage_service.store_fetch(inc_key)

    # Convert old_task dict to match Task class attributes
    converted_old_task = {
        "id": old_task.get("id"),
        "title": old_task.get("title"),
        "description": old_task.get("description"),
        "created_by": old_task.get("createdBy"),
        "assigned_to": old_task.get("assignedTo"),
        "created_at": old_task.get("createdAt"),
        "status": old_task.get("status"),
    }

    # Now fill it correctly
    inc = Task()
    inc.fill(converted_old_task)

    # Apply updates from UI/input
    inc.fill(
        {
            "title": title,
            "description": description,
            "created_by": created_by,
            "assigned_to": assigned_to,
        }
    )

    index = next(
        (
            i
            for i, t in enumerate(tasks)
            if getattr(t, "id", None) == getattr(inc, "id", None)
        ),
        None,
    )
    if index is not None:
        tasks[index] = inc
        storage_service.store_update(inc_key, tasks)
    return inc

async def update_status_inc(action):
    old_task = action["data"]["task"]
    status = action["data"]["status"]

    global tasks
    if storage_service.store_check(inc_key):
        tasks = storage_service.store_fetch(inc_key)

    inc = Task()
    inc.fill(old_task)
    inc.set_status(status)

    index = next(
        (
            i
            for i, t in enumerate(tasks)
            if getattr(t, "id", None) == getattr(inc, "id", None)
        ),
        None,
    )
    if index is not None:
        tasks[index] = inc
        storage_service.store_update(inc_key, tasks)
    return inc

async def get_all_inc():
    global tasks
    if storage_service.store_check(inc_key):
        tasks = storage_service.store_fetch(inc_key)
    return tasks
