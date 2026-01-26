import requests
from flask import Flask, request, jsonify

app = Flask(__name__)

@app.route("/search", methods=["POST"])
def search():
    data = request.get_json()
    name = data.get('name')

    book_id, book_name = find_book(name)
    success = False

    if book_id:
        download_gutenberg_txt(book_id, f"../Resources/{book_name}_{book_id}.txt")
        success = True

    return jsonify({
        "name": book_name,
        "id": book_id,
        "success": success,
        "path": f"../Resources/{book_name}_{book_id}.txt"
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

    print(f"Successfully downloaded at:",save_path)
    return save_path

app.run(port=5000)