import requests, os, socket, sys, signal
from flask import Flask, request, jsonify
from ping3 import ping



app = Flask(__name__)

def safe_filename(name):
    return "".join(
        c for c in name if c.isalnum() or c in (" ", "_", "-")
    ).strip()

@app.route("/health", methods=["GET"])
def health():
    try:
        delay = ping("https://gutendex.com/books", timeout = 1000)
        return "ok"
    except socket.error() as e:
        print("ping Error:", e)
        return "invalid"

def shutdown_handler(signum, frame):
    print("Shutting down Flask server...")
    sys.exit(0)

@app.route("/search", methods=["POST"])
def search():
    data = request.get_json()
    name = data.get("name")
    
    success = False
    try:
        book_id, book_name = find_book(name)
    except TypeError as e:
        print(f"[ERROR] {e}")
        return jsonify({
            "name": "book_not_found",
            "id": -1,
            "success": success,
            "path": "invalid_path"
        })
    resource_key = None

    if book_id:
        safe_name = safe_filename(book_name)

        BASE_DIR = os.path.dirname(os.path.abspath(__file__))
        RESOURCES_DIR = os.path.abspath(
            os.path.join(BASE_DIR, "..", "..", "Resources/BookFiles")
        )

        print(RESOURCES_DIR)

        os.makedirs(RESOURCES_DIR, exist_ok=True)

        resource_key = f"{safe_name}_{book_id}"
        save_path = os.path.join(
            RESOURCES_DIR,
            resource_key + ".txt"
        )

        download_gutenberg_txt(book_id, save_path)

        success = True

    return jsonify({
        "name": book_name,
        "id": book_id,
        "success": success,
        "path": save_path
    })



def find_book(name):
    url = "https://gutendex.com/books"
    params = {"search": name}
    headers = {"User-Agent": "Mozilla/5.0"}

    response = requests.get(url, params=params, headers=headers)

    r = response.json()

    if r["results"]:
        id = r["results"][0]["id"]

        try:
            id = int(id)
        except ValueError:
            id = -1

        return id, r["results"][0]["title"]
    
    return None

def download_gutenberg_txt(book_id, save_path):
    url = f"https://www.gutenberg.org/files/{book_id}/{book_id}-0.txt"
    headers = {"User-Agent": "Mozilla/5.0"}

    r = requests.get(url, headers=headers)

    if(r.status_code != 200):
        print("File do not exist!\n")
        return None
    
    path_checker = os.path.dirname(save_path)
    if os.path.exists(path_checker):
        with open(save_path, "wb") as f:
            f.write(r.content)

    print(f"Successfully downloaded at:",save_path)
    return save_path

if __name__ == "__main__":
    signal.signal(signal.SIGTERM, shutdown_handler)
    signal.signal(signal.SIGINT, shutdown_handler)

    app.run(port=5000)