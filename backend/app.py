"""
SafeCity Flask backend — designed for Render.com free tier.
Start command: gunicorn app:app --timeout 120
"""
import os, json, datetime
from flask import Flask, jsonify, request

app = Flask(__name__)

# ── In-memory store (replace with DB for production) ─────────────────────────
_incidents: list[dict] = []

# ── Health ────────────────────────────────────────────────────────────────────
@app.route("/health")
def health():
    return jsonify({"status": "ok", "timestamp": datetime.datetime.utcnow().isoformat()})

# ── Incidents ─────────────────────────────────────────────────────────────────
@app.route("/incidents", methods=["GET"])
def get_incidents():
    lat  = request.args.get("lat", type=float)
    lon  = request.args.get("lon", type=float)
    radius = request.args.get("radius_km", default=10.0, type=float)
    data = _incidents
    if lat is not None and lon is not None:
        data = [i for i in _incidents if _haversine(lat, lon, i["latitude"], i["longitude"]) <= radius]
    return jsonify({"incidents": data, "count": len(data)})

@app.route("/incidents", methods=["POST"])
def create_incident():
    body = request.get_json(silent=True) or {}
    body["id"] = len(_incidents) + 1
    body["created_at"] = datetime.datetime.utcnow().isoformat()
    _incidents.append(body)
    return jsonify(body), 201

# ── Assistant (Gemini proxy) ──────────────────────────────────────────────────
@app.route("/assistant", methods=["POST"])
def assistant():
    body   = request.get_json(silent=True) or {}
    prompt = body.get("prompt", "")
    api_key = os.environ.get("GEMINI_API_KEY", "")
    if not api_key:
        return jsonify({"reply": "Assistant not configured on the server."}), 200
    import urllib.request, json as _json
    payload = _json.dumps({
        "contents": [{"parts": [{"text": f"You are SafeCity safety assistant. {prompt}"}]}]
    }).encode()
    url = f"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={api_key}"
    try:
        req = urllib.request.Request(url, data=payload,
                                     headers={"Content-Type": "application/json"}, method="POST")
        with urllib.request.urlopen(req, timeout=15) as r:
            data = _json.loads(r.read())
        text = data["candidates"][0]["content"]["parts"][0]["text"]
        return jsonify({"reply": text})
    except Exception as e:
        return jsonify({"reply": f"Error: {e}"}), 500

# ── Helpers ───────────────────────────────────────────────────────────────────
def _haversine(lat1, lon1, lat2, lon2):
    import math
    R = 6371
    d_lat = math.radians(lat2 - lat1)
    d_lon = math.radians(lon2 - lon1)
    a = (math.sin(d_lat/2)**2
         + math.cos(math.radians(lat1)) * math.cos(math.radians(lat2))
         * math.sin(d_lon/2)**2)
    return R * 2 * math.atan2(math.sqrt(a), math.sqrt(1 - a))

if __name__ == "__main__":
    port = int(os.environ.get("PORT", 5000))
    app.run(host="0.0.0.0", port=port)
