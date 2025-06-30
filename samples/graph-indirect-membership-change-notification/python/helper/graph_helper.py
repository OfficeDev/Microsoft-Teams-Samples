import os
import requests
from helper.auth import AuthHelper


class GraphHelper:
    @staticmethod
    def get_headers(token):
        return {
            "Accept": "application/json",
            "Content-Type": "application/json",
            "Authorization": f"Bearer {token}"
        }

    @staticmethod
    def delete_subscription(subscription_id):
        token = AuthHelper.get_access_token()
        url = f"https://graph.microsoft.com/v1.0/subscriptions/{subscription_id}"
        headers = GraphHelper.get_headers(token)
        response = requests.delete(url, headers=headers)
        response.raise_for_status()

    @staticmethod
    def create_subscription(teams_id, page_id):
        token = AuthHelper.get_access_token()
        notification_url = os.getenv("notificationUrl")
        resource = ""
        change_type = ""

        if page_id == "1":
            resource = f"/teams/{teams_id}/channels/getAllMembers"
            change_type = "created,deleted,updated"

        headers = GraphHelper.get_headers(token)

        try:
            resp = requests.get("https://graph.microsoft.com/beta/subscriptions", headers=headers)
            existing_subscriptions = resp.json().get("value", [])
        except requests.RequestException:
            return None

        existing_subscription = next(
            (sub for sub in existing_subscriptions if sub["resource"] == resource), None
        )

        if existing_subscription and existing_subscription["notificationUrl"] != notification_url:
            print(f"CreateNewSubscription-ExistingSubscriptionFound: {resource}")
            GraphHelper.delete_subscription(existing_subscription["id"])
            existing_subscription = None

        if not existing_subscription:
            subscription_payload = {
                "changeType": change_type,
                "notificationUrl": notification_url,
                "lifecycleNotificationUrl": notification_url,
                "resource": resource,
                "includeResourceData": True,
                "encryptionCertificate": os.getenv("Base64EncodedCertificate"),
                "encryptionCertificateId": "meeting-notification",
                "expirationDateTime": GraphHelper.get_expiration_time(hours=10),
                "clientState": "clientState"
            }

            try:
                resp = requests.post(
                    "https://graph.microsoft.com/v1.0/subscriptions",
                    headers=headers,
                    json=subscription_payload
                )
                existing_subscription = resp.json()
            except requests.RequestException as e:
                print(f"Error--{e}")
                return None

        return existing_subscription

    @staticmethod
    def create_shared_with_teams_subscription(teams_id, channel_id):
        token = AuthHelper.get_access_token()
        resource = f"/teams/{teams_id}/channels/{channel_id}/sharedWithTeams"
        change_type = "created,deleted,updated"
        notification_url = os.getenv("notificationUrl")
        headers = GraphHelper.get_headers(token)

        try:
            resp = requests.get("https://graph.microsoft.com/beta/subscriptions", headers=headers)
            existing_subscriptions = resp.json().get("value", [])
        except requests.RequestException:
            return None

        existing_subscription = next(
            (sub for sub in existing_subscriptions if sub["resource"] == resource), None
        )

        if existing_subscription and existing_subscription["notificationUrl"] != notification_url:
            GraphHelper.delete_subscription(existing_subscription["id"])
            existing_subscription = None

        if not existing_subscription:
            subscription_payload = {
                "changeType": change_type,
                "notificationUrl": notification_url,
                "lifecycleNotificationUrl": notification_url,
                "resource": resource,
                "includeResourceData": True,
                "encryptionCertificate": os.getenv("Base64EncodedCertificate"),
                "encryptionCertificateId": "meeting-notification",
                "expirationDateTime": GraphHelper.get_expiration_time(days=50) + 'Z',
                "clientState": "clientState"
            }

            try:
                resp = requests.post(
                    "https://graph.microsoft.com/beta/subscriptions",
                    headers=headers,
                    json=subscription_payload
                )
                existing_subscription = resp.json()
            except requests.RequestException as e:
                print(f"Error--{e}")
                return None

        return existing_subscription

    @staticmethod
    def check_user_channel_access(team_id, channel_id, user_id, tenant_id):
        token = AuthHelper.get_access_token()
        url = (
            f"https://graph.microsoft.com/v1.0/teams/{team_id}/channels/{channel_id}/"
            f"microsoft.graph.doesUserHaveAccess(userId='{user_id}', tenantId='{tenant_id}')"
        )

        try:
            response = requests.get(url, headers=GraphHelper.get_headers(token))
            return response.json().get("value", False)
        except requests.RequestException as e:
            print(f"Error checking user channel access: {e}")
            return False

    @staticmethod
    def get_expiration_time(hours=0, days=0):
        from datetime import datetime, timedelta
        return (datetime.utcnow() + timedelta(hours=hours, days=days)).isoformat() + "Z"
