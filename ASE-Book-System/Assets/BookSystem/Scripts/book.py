import requests, os, socket, sys, signal
from flask import Flask, request, jsonify
from ping3 import ping


app = Flask(__name__)

def safe_filename(name):
    """
    reformat a filename to remove any character that isn't alphanumeric or a space, underscore or hyphen.

    @param:
        - name: str - the filename to be formatted.
    @returns: str - formatted filename.
    """
    return "".join(
        c for c in name if c.isalnum() or c in (" ", "_", "-")
    ).strip()

@app.route("/health", methods=["GET"])
def health():
    """
    Ping the Gutenberg books database through the flask server.

    @returns:
        - ok if successful,
        - invalid otherwise.
    """
    try:
        delay = ping("https://gutendex.com/books", timeout = 1000)
        return "ok"
    except socket.error() as e:
        print("ping Error:", e)
        return "invalid"

def shutdown_handler(signum, frame):
    """
    Cleanly close the Flask server.
    """
    print("Shutting down Flask server...")
    sys.exit(0)

@app.route("/search", methods=["POST"])
def search():
    """
    Input channel for a Gutenberg book request.

    - Queries the Gutenberg catalogue for a match
    - if there's a match, downloads the book as a .txt file to save in the folder Resources/BookFiles
    [Should be relative to Assets folder, please double check if the files aren't being saved properly].
    Path is determined by going two folder levels up from this script's directory then searching for Resources/BookFiles.

    @params:
        - (No explicit parameters)
        - REQUIRES a Json format {"name": string} submitted through a web request to the Flask server.

    @returns:
        - A Json object formatted:
        {
            "name": book_name,
            "id": book_id,
            "success": success [True],
            "path": save_path
        } if successful, otherwise:
        {
            "name": "book_not_found",
            "id": -1,
            "success": success [False],
            "path": "invalid_path"
        }
    """
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
    """
    Queries the Gutenberg database to find the first book title & id to match the input title.

    @params:
        - name: str - a partial or full book title.

    @returns:
        - tuple<int, string> - a pair of values representing the book ID on the Gutenberg DB, and the full book title there.
        (ID = -1 if matches found have invalid IDs, 
        the entire tuple is nulled if the Gutenberg query doesn't succeed or if there are no results.)
    """
    url = "https://gutendex.com/books"
    params = {"search": name}
    headers = {"User-Agent": "Mozilla/5.0"}

    response = requests.get(url, params=params, headers=headers)

    if r:
        r = response.json()

        if r["results"]:
            id = r["results"][0]["id"]

            try:
                id = int(id)
            except ValueError:
                id = -1

            return id, r["results"][0]["title"]
        return None
    
    return None

def download_gutenberg_txt(book_id, save_path):
    """
    Downloads a specific file from the Gutenberg DB and saves it to the specified path.

    @params:
        - book_id: int - the Gutenberg ID attributed to the book to download.
        - save_path: os.path - the absolute save path where the resulting text should be saved, INCLUDING filename.
            -> should be in the project's Assets/Resources/BookFiles folder!

    @returns:
        - os.path: the same exact save_path if successful, a None object otherwise.
    """
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
    # listeners for signals to shut down the Flask server.
    signal.signal(signal.SIGTERM, shutdown_handler)
    signal.signal(signal.SIGINT, shutdown_handler)

    # start the app on port 5000 (hard-coded).
    app.run(port=5000)