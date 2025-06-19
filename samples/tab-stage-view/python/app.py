import os
import sys
import traceback
from pathlib import Path
from aiohttp import web
from dotenv import load_dotenv
from api.routes import api_routes  

ENV_PATH = Path(__file__).resolve().parent.parent / ".env"
load_dotenv(dotenv_path=ENV_PATH)

PORT = int(os.getenv("PORT", 3978))
BASE_DIR = Path(__file__).parent
VIEWS_DIR = BASE_DIR / "views"

app = web.Application()

async def content_handler(request):
    return web.FileResponse(VIEWS_DIR / "content-tab.html")

async def tab_handler(request):
    return web.FileResponse(VIEWS_DIR / "sampleTab.html")

async def config_api(request):
    return web.json_response({
        "BaseUrl": os.getenv("BaseUrl"),
        "teamsAppId": os.getenv("TeamsAppId")
    })

async def handle_404(request):
    return web.json_response({"error": "Route not found"}, status=404)

app.router.add_get("/content", content_handler)
app.router.add_get("/tab", tab_handler)
app.router.add_get("/api/config", config_api)
app.add_routes(api_routes)
app.router.add_route("*", "/{tail:.*}", handle_404)

if __name__ == "__main__":
    try:
        web.run_app(app, port=PORT)
    except Exception as e:
        print("Error starting server:", e, file=sys.stderr)
        traceback.print_exc()
