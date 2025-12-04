from flask import Flask, render_template
import random
from faker import Faker

app = Flask(__name__)
fake = Faker()

def setup_routes(app):
    @app.route('/welcome')
    def welcome():
        resources = [
            {
                "image_url": "/static/images/platform-ui.jpg",
                "title": "Design with the Teams UI Kit",
                "desc": "The Microsoft Teams UI Kit includes UI components, templates, best practices, and other comprehensive resources to help design your Teams app.",
                "links": [
                    {"label": "Get the UI Kit", "href": "https://www.figma.com/community/file/916836509871353159"},
                ],
            },
            {
                "image_url": "/static/images/teams-templates.jpg",
                "title": "Use Teams app templates",
                "desc": "Browse our collection of production-ready, open-source apps that you can customize or deploy right away to Teams.",
                "links": [
                    {"label": "See the templates", "href": "https://docs.microsoft.com/en-us/microsoftteams/platform/samples/app-templates"},
                ],
            },
            {
                "image_url": "/static/images/teams-apps.jpg",
                "title": "Learn Teams development",
                "desc": "Are you new to Teams development? To familiarize yourself, quickly build a “Hello, World!” app or read all about how Teams apps work.",
                "links": [
                    {"label": "See more", "href": "https://docs.microsoft.com/en-us/microsoftteams/platform/overview"},
                ],
            },
            {
                "image_url": "/static/images/visual-studio.jpg",
                "title": "Build with the Teams Toolkit",
                "desc": "The Microsoft Teams Toolkit extension is the fastest way to build, test, and deploy your app to Teams.",
                "links": [
                    {"label": "Get the Visual Studio Code extension", "href": "https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.vsteamstemplate&ssr=false"},
                    {"label": "Get the Visual Studio extension", "href": "https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.vsteamstemplate&ssr=false"},
                ],
            },
        ]
        return render_template('welcome.html', resources=resources)

    @app.route('/dashboard')
    def dashboard():
        widgets = [
            {
                "title": "Card 1",
                "desc": "Last updated Monday, April 4 at 11:15 AM (PT)",
                "size": "75%",
                "body": [
                    {"id": "t1", "title": "Tab 1"},
                    {"id": "t2", "title": "Tab 2"},
                    {"id": "t3", "title": "Tab 3"},
                ],
                "link": {"href": "#"},
            },
            {"title": "Card 2", "size": "20%", "link": {"href": "#"}},
            {"title": "Card 3", "size": "55%", "link": {"href": "#"}},
            {"title": "Card 4", "size": "20%", "link": {"href": "#"}},
            {"title": "Card 5", "size": "20%", "link": {"href": "#"}},
            {"title": "Card 6", "size": "100%", "link": {"href": "#"}},
        ]
        return render_template('dashboard.html', widgets=widgets)

    @app.route('/list')
    def list_page():
        items = [
            {
                "MemberName": {"label": "Babak Shammas (no delete)"},
                "Location": {"label": "Seattle, WA"},
                "Role": {"label": "Senior analyst"},
                "Menu": {"icon": True},
            },
            {
                "MemberName": {"label": "Aadi Kapoor"},
                "Location": {"label": "Seattle, WA"},
                "Role": {"label": "Security associate"},
                "Menu": {"icon": True},
            },
            {
                "MemberName": {"label": "Aaron Buxton"},
                "Location": {"label": "Seattle, WA"},
                "Role": {
                    "label": (
                        "Security engineer: Lorem ipsum dolor sit amet, "
                        "consectetur adipiscing elit. Cras in ultricies mi."
                    )
                },
                "Menu": {"icon": True},
            },
            {
                "MemberName": {"label": "Alvin Tao (no actions)"},
                "Location": {"label": "Seattle, WA"},
                "Role": {"label": "Marketing analyst"},
                "Menu": {"icon": False},
            },
            {
                "MemberName": {"label": "Beth Davies"},
                "Location": {"label": "Seattle, WA"},
                "Role": {"label": "Senior engineer"},
                "Menu": {"icon": True},
            },
        ]
        return render_template('list.html', items=items)

    @app.route('/board')
    def task_boards():
        def random_users():
            return [f"User {i}" for i in range(1, 25) if random.random() > 0.67]

        # Generate lanes with correct method
        lanes = {
            f"l{i}": {"title": fake.job()} for i in range(1, 6)
        }

        items = {}
        item_id = 1
        for lane_id in lanes.keys():
            num_items = random.randint(2, 6)
            for _ in range(num_items):
                items[f"t{item_id}"] = {
                    "lane": lane_id,
                    "title": fake.catch_phrase(),
                    "subtitle": fake.bs() if random.random() > 0.33 else None,
                    "body": fake.sentence() if random.random() > 0.33 else None,
                    "preview": fake.image_url() if random.random() > 0.33 else None,
                    "badges": {
                        "attachments": random.randint(1, 10) if random.random() > 0.5 else None
                    },
                }
                item_id += 1

        return render_template("task_boards.html", lanes=lanes, items=items)

    @app.route('/about')
    def about():
        return render_template('about.html')

    @app.route('/privacy')
    def privacy():
        return render_template('privacy.html')

    @app.route('/terms_of_use')
    def terms_of_use():
        return render_template('terms_of_use.html')