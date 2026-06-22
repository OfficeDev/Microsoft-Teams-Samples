# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

class ActivityLog:
    # Constructor to initialize the ActivityLog class with a storage object.
    def __init__(self, storage):
        self._storage = storage  # The storage object is used for storing and retrieving activities.

    # Asynchronous method to append an activity to the log.
    async def append(self, activity_id, activity):
        # Check if activity_id is provided; raise an error if not.
        if activity_id is None:
            raise TypeError('activity_id is required for ActivityLog.append')
        
        # Check if activity is provided; raise an error if not.
        if activity is None:
            raise TypeError('activity is required for ActivityLog.append')

        # Create an object to store the activity with the activity_id as the key.
        obj = {activity_id: {"activity": activity}}

        # Use the storage's write method to save the activity.
        await self._storage.write(obj)

    # Asynchronous method to find and retrieve an activity from the log by its ID.
    async def find(self, activity_id):
        # Check if activity_id is provided; raise an error if not.
        if activity_id is None:
            raise TypeError('activity_id is required for ActivityLog.find')

        # Use the storage's read method to fetch the activity by its ID.
        # The read method is expected to return a dictionary where keys are IDs and values are the stored objects.
        items = await self._storage.read([activity_id])

        # Retrieve the activity from the fetched data if it exists, otherwise return None.
        return items.get(activity_id, {}).get("activity") if items else None

