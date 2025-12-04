# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from server.models.incident import Incident
from server.services import storage_service

INC_KEY = 'incidents_key'
incidents = []

async def save_inc(action, members):
    global incidents
    assigned_to = next((m for m in members if m.get('aadObjectId') == action['data']['inc_assigned_to']), None)
    title = action['data'].get('inc_title')
    category = action['data'].get('category')
    sub_category = action['data'].get('sub_category')
    created_by = action['data'].get('inc_created_by')

    if storage_service.store_check(INC_KEY):
        incidents = storage_service.store_fetch(INC_KEY)
        # incidents is likely list of dicts from storage, convert to Incident instances
        incidents = [Incident().from_dict(inc) for inc in incidents]

    new_inc = Incident(
        id=None,
        title=title,
        category=category,
        sub_category=sub_category,
        created_by=created_by,
        assigned_to=assigned_to,
        created_at=None,
        status=None
    )

    incidents.append(new_inc)
    # Save as list of dicts
    storage_service.store_save(INC_KEY, [inc.to_dict() for inc in incidents])
    return new_inc


async def update_inc(action, members):
    global incidents

    data = action.get("data", {})

    title = data.get("title")
    category = data.get("category")  # Optional: You may want to include category input in card if editable
    sub_category = data.get("subCategory")
    created_by_name = data.get("createdByName")
    assigned_to_id = data.get("assignedToId")

    # Find assigned member by aadObjectId
    assigned_to = next((m for m in members if m.get("aadObjectId") == assigned_to_id), None)

    # Load incidents from storage
    if storage_service.store_check(INC_KEY):
        stored = storage_service.store_fetch(INC_KEY)
        incidents = [Incident().from_dict(inc) for inc in stored]
    else:
        incidents = []

    old_inc_data = data.get("incident", {})

    inc = Incident().from_dict(old_inc_data)

    inc.fill({
        "title": title or inc.title,
        "category": category or inc.category,
        "sub_category": sub_category or inc.sub_category,
        "created_by": {"name": created_by_name} if created_by_name else inc.created_by,
        "assigned_to": assigned_to or inc.assigned_to,
    })

    # Find existing incident index to update
    index = next((i for i, item in enumerate(incidents) if item.id == inc.id), -1)
    if index >= 0:
        incidents[index] = inc
    else:
        incidents.append(inc)

    storage_service.store_save(INC_KEY, [inc.to_dict() for inc in incidents])

    return inc.to_dict()



async def update_status_inc(action):
    global incidents
    old_inc_data = action['data']['incident']
    status = action['data']['status']

    if storage_service.store_check(INC_KEY):
        incidents = storage_service.store_fetch(INC_KEY)
        incidents = [Incident().from_dict(inc) for inc in incidents]

    inc = Incident().from_dict(old_inc_data)
    inc.set_status(status)

    index = next((i for i, item in enumerate(incidents) if item.id == inc.id), -1)
    if index >= 0:
        incidents[index] = inc
        storage_service.store_save(INC_KEY, [inc.to_dict() for inc in incidents])
    return inc


async def get_all_inc():
    global incidents
    if storage_service.store_check(INC_KEY):
        incidents = storage_service.store_fetch(INC_KEY)
        # Return list of Incident objects (converted from dict)
        return [Incident().from_dict(inc) for inc in incidents]
    return incidents
