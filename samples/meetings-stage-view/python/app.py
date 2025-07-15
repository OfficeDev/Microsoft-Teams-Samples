import os
from aiohttp import web
from aiohttp.web import FileResponse

todo_data = {}
doing_data = {}
done_data = {}

@web.middleware
async def cors_middleware(request, handler):
    if request.method == "OPTIONS":
        response = web.Response(status=200)
    else:
        response = await handler(request)

    response.headers['Access-Control-Allow-Origin'] = '*'
    response.headers['Access-Control-Allow-Methods'] = 'GET, POST, OPTIONS'
    response.headers['Access-Control-Allow-Headers'] = 'Content-Type'
    return response

async def get_meeting_data(request):
    status = request.rel_url.query.get('status')
    meeting_id = request.rel_url.query.get('meetingId')

    if status == "todo":
        data = todo_data.get(meeting_id)
    elif status == "doing":
        data = doing_data.get(meeting_id)
    elif status == "done":
        data = done_data.get(meeting_id)
    else:
        return web.Response(status=400, text="Invalid status")

    return web.json_response({'data': data})

async def save_meeting_data(request):
    try:
        body = await request.json()
        meeting_id = body.get("meetingId")
        status = body.get("status")

        if status == "todo":
            todo_data.setdefault(meeting_id, []).append(body)
        elif status == "doing":
            doing_data.setdefault(meeting_id, []).append(body)
        elif status == "done":
            done_data.setdefault(meeting_id, []).append(body)
        else:
            return web.Response(status=400, text="Invalid status")

        return web.Response(status=200)
    except Exception as e:
        return web.Response(status=500, text=str(e))

async def serve_static(request):
    """Serve static files from ClientApp build directory"""
    path = request.match_info.get('path', '')
    
    build_dir = os.path.join(os.path.dirname(__file__), 'ClientApp', 'build')
    
    if path:
        static_path = os.path.join(build_dir, path)
        if os.path.exists(static_path) and os.path.isfile(static_path):
            return FileResponse(static_path)
    
    index_path = os.path.join(build_dir, 'index.html')
    if os.path.exists(index_path):
        return FileResponse(index_path)
    else:
        return web.Response(status=404, text="React app not built. Please run 'npm run build' in ClientApp directory.")


app = web.Application(middlewares=[cors_middleware])
app.router.add_get("/getMeetingData", get_meeting_data)
app.router.add_post("/saveMeetingData", save_meeting_data)

app.router.add_get("/", serve_static)
app.router.add_get("/{path:.*}", serve_static)

if __name__ == '__main__':
    port = int(os.getenv("PORT", 3978)) 
    print(f"Starting server on port {port}")
    print(f"Server will serve React app and API endpoints")
    print(f"Access the app at: http://localhost:{port}")
    web.run_app(app, port=port, host='0.0.0.0')
