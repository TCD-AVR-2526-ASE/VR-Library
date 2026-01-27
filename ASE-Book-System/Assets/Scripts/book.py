import requests
from flask import Flask, request, jsonify
import os

app = Flask(__name__)

def safe_filename(name):
    return "".join(
        c for c in name if c.isalnum() or c in (" ", "_", "-")
    ).strip()


@app.route("/search", methods=["POST"])
def search():
    data = request.get_json()
    name = data.get("name")

    book_id, book_name = find_book(name)
    success = False
    resource_key = None

    if book_id:
        safe_name = safe_filename(book_name)

        BASE_DIR = os.path.dirname(os.path.abspath(__file__))
        RESOURCES_DIR = os.path.abspath(
            os.path.join(BASE_DIR, "..", "Resources")
        )

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
        "path": resource_key 
    })



def find_book(name):
    url = "https://gutendex.com/books"
    params = {"search": name}
    headers = {"User-Agent": "Mozilla/5.0"}

    response = requests.get(url, params=params, headers=headers)

    r = response.json()

    if r["results"]:
        return r["results"][0]["id"], r["results"][0]["title"]
    
    return None

def download_gutenberg_txt(book_id, save_path):
    url = f"https://www.gutenberg.org/files/{book_id}/{book_id}-0.txt"
    headers = {"User-Agent": "Mozilla/5.0"}

    r = requests.get(url, headers=headers)

    if(r.status_code != 200):
        print("File do not exist!\n")
        return None
    
    with open(save_path, "wb") as f:
        f.write(r.content)

    print(f"Successfully downloaded!\n")
    return save_path

app.run(port=5000)